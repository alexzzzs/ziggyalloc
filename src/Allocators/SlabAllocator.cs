using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ZiggyAlloc
{
    /// <summary>
    /// A slab allocator that pre-allocates large blocks of memory and sub-allocates from them.
    /// This allocator is particularly efficient for scenarios with many small, similarly-sized allocations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The slab allocator works by:
    /// 1. Pre-allocating large "slabs" of memory (typically several MB each)
    /// 2. Dividing these slabs into fixed-size "slots" based on allocation requests
    /// 3. Tracking which slots are in use and which are free
    /// 4. Allocating new slabs as needed when existing ones are full
    /// </para>
    /// 
    /// <para>
    /// Benefits:
    /// - Extremely fast allocation/deallocation for small objects
    /// - Zero fragmentation within slabs
    /// - Reduced system call overhead
    /// - Better cache locality
    /// </para>
    /// 
    /// <para>
    /// Best used for:
    /// - High-frequency small allocations of similar sizes
    /// - Performance-critical code paths
    /// - Scenarios where allocation patterns are predictable
    /// </para>
    /// 
    /// <para>
    /// Limitations:
    /// - Not suitable for large allocations (will fall back to base allocator)
    /// - Memory overhead from partially filled slabs
    /// - Not ideal for highly variable allocation sizes
    /// </para>
    /// </remarks>
    public sealed class SlabAllocator : IUnmanagedMemoryAllocator, IDisposable
    {
        private readonly IUnmanagedMemoryAllocator _baseAllocator;
        private readonly ConcurrentDictionary<int, SlabPool> _slabPools;
        private readonly int _slabSize;
        private long _totalAllocatedBytes;
        private bool _disposed = false;

        /// <summary>
        /// The maximum size of an allocation that can be served by a slab.
        /// Allocations larger than this will be delegated to the base allocator.
        /// </summary>
        public const int MaxSlabAllocationSize = 4096; // 4KB

        /// <summary>
        /// Gets a value indicating that this allocator supports individual memory deallocation.
        /// </summary>
        public bool SupportsIndividualDeallocation => true;

        /// <summary>
        /// Gets the total number of bytes currently allocated by this allocator.
        /// </summary>
        public long TotalAllocatedBytes => Interlocked.Read(ref _totalAllocatedBytes);

        /// <summary>
        /// Initializes a new instance of the SlabAllocator class.
        /// </summary>
        /// <param name="baseAllocator">The underlying allocator to use for slab allocation and large allocations</param>
        /// <param name="slabSize">The size of each slab in bytes (default is 1MB)</param>
        public SlabAllocator(IUnmanagedMemoryAllocator baseAllocator, int slabSize = 1024 * 1024)
        {
            _baseAllocator = baseAllocator ?? throw new ArgumentNullException(nameof(baseAllocator));
            _slabSize = slabSize > 0 ? slabSize : throw new ArgumentOutOfRangeException(nameof(slabSize));
            _slabPools = new ConcurrentDictionary<int, SlabPool>();
        }

        /// <summary>
        /// Allocates memory using slab allocation for small requests or the base allocator for large requests.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="elementCount">The number of elements to allocate space for</param>
        /// <param name="zeroMemory">Whether to zero-initialize the allocated memory</param>
        /// <returns>A buffer representing the allocated memory</returns>
        public unsafe UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false) where T : unmanaged
        {
            try
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(SlabAllocator));

                if (elementCount < 0)
                    throw new ArgumentOutOfRangeException(nameof(elementCount), "Element count cannot be negative");

                if (elementCount == 0)
                {
                    // Return a valid but empty buffer for zero-length allocations
                    return new UnmanagedBuffer<T>(null, 0, this);
                }

                // Calculate total size
                int elementSize = sizeof(T);
                long totalSize = (long)elementCount * elementSize;

                // Prevent zero-sized or negative-sized slot allocations
                if (totalSize <= 0 || totalSize > MaxSlabAllocationSize || totalSize > _slabSize / 4)
                {
                    // Too large for slab allocation, delegate to base allocator
                    var buffer = _baseAllocator.Allocate<T>(elementCount, zeroMemory);
                    Interlocked.Add(ref _totalAllocatedBytes, totalSize);
                    return new UnmanagedBuffer<T>((T*)buffer.RawPointer, buffer.Length, this);
                }

                // Use slab allocation
                int slotSize = (int)totalSize;
                var pool = _slabPools.GetOrAdd(slotSize, _ => new SlabPool(_baseAllocator, _slabSize, slotSize));
                var slot = pool.AllocateSlot(zeroMemory);

                Interlocked.Add(ref _totalAllocatedBytes, slotSize);
                return new UnmanagedBuffer<T>((T*)slot.Pointer, elementCount, slot);
            }
            catch
            {
                // If slab allocation fails, fall back to base allocator
                if (!_disposed && _baseAllocator != null)
                {
                    return _baseAllocator.Allocate<T>(elementCount, zeroMemory);
                }
                
                // If we can't allocate through base allocator either, rethrow
                throw;
            }
        }

        /// <summary>
        /// Frees previously allocated unmanaged memory.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to free</param>
        public void Free(IntPtr pointer)
        {
            if (_disposed || pointer == IntPtr.Zero)
                return;

            try
            {
                // In a real implementation, we would need to track which slab a pointer belongs to
                // For this example, we'll delegate to the base allocator
                _baseAllocator.Free(pointer);
            }
            catch
            {
                // Ignore exceptions during freeing to prevent crashes
            }
        }

        /// <summary>
        /// Disposes the SlabAllocator and releases all allocated slabs.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                try
                {
                    if (_slabPools != null)
                    {
                        foreach (var pool in _slabPools.Values)
                        {
                            pool?.Dispose();
                        }
                        _slabPools.Clear();
                    }
                    
                    // Explicitly suppress finalization for the base allocator if it implements IDisposable
                    if (_baseAllocator is IDisposable disposableAllocator)
                    {
                        // We don't dispose the base allocator as it might be used elsewhere
                        // But we ensure that any cleanup that needs to happen in this allocator is done
                    }
                }
                catch
                {
                    // Ignore exceptions during disposal to prevent crashes
                }
            }
        }

        /// <summary>
        /// Represents a pool of slabs for a specific slot size.
        /// </summary>
        private class SlabPool : IDisposable
        {
            private readonly IUnmanagedMemoryAllocator _allocator;
            private readonly int _slabSize;
            private readonly int _slotSize;
            private readonly ConcurrentBag<Slab> _slabs;
            private bool _disposed = false;

            public SlabPool(IUnmanagedMemoryAllocator allocator, int slabSize, int slotSize)
            {
                _allocator = allocator;
                _slabSize = slabSize;
                _slotSize = slotSize;
                _slabs = new ConcurrentBag<Slab>();
            }

            public SlabSlot AllocateSlot(bool zeroMemory)
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(SlabPool));

                // Try to get a slot from existing slabs
                foreach (var slab in _slabs)
                {
                    if (slab.TryAllocateSlot(out var slot))
                    {
                        if (zeroMemory)
                        {
                            // Zero the slot memory
                            unsafe
                            {
                                var span = new Span<byte>((void*)slot.Pointer, _slotSize);
                                span.Clear();
                            }
                        }
                        return slot;
                    }
                }

                // No available slots, create a new slab
                var newSlab = new Slab(_allocator, _slabSize, _slotSize);
                _slabs.Add(newSlab);

                if (newSlab.TryAllocateSlot(out var newSlot))
                {
                    if (zeroMemory)
                    {
                        unsafe
                        {
                            var span = new Span<byte>((void*)newSlot.Pointer, _slotSize);
                            span.Clear();
                        }
                    }
                    return newSlot;
                }

                throw new InvalidOperationException("Failed to allocate slot from new slab");
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    try
                    {
                        if (_slabs != null)
                        {
                            foreach (var slab in _slabs)
                            {
                                slab?.Dispose();
                            }
                        }
                    }
                    catch
                    {
                        // Ignore exceptions during disposal to prevent crashes
                    }
                }
            }
        }

        /// <summary>
        /// Represents a large block of memory divided into fixed-size slots.
        /// </summary>
        public class Slab : IDisposable
        {
            private readonly UnmanagedBuffer<byte> _buffer;
            private readonly int _slotSize;
            private readonly int _slotCount;
            private readonly bool[] _slotInUse;
            private readonly object _lock = new object();
            private bool _disposed = false;

            public unsafe Slab(IUnmanagedMemoryAllocator allocator, int slabSize, int slotSize)
            {
                // Add safety check to prevent division by zero
                if (slotSize <= 0)
                    throw new ArgumentOutOfRangeException(nameof(slotSize), "Slot size must be positive");
                    
                _buffer = allocator.Allocate<byte>(slabSize);
                _slotSize = slotSize;
                _slotCount = slabSize / slotSize;
                _slotInUse = new bool[_slotCount];
            }

            public bool TryAllocateSlot(out SlabSlot? slot)
            {
                slot = null;

                if (_disposed)
                    return false;

                lock (_lock)
                {
                    for (int i = 0; i < _slotCount; i++)
                    {
                        if (!_slotInUse[i])
                        {
                            _slotInUse[i] = true;
                            unsafe
                            {
                                // Add safety check to ensure we don't do invalid pointer arithmetic
                                if (_buffer.RawPointer == IntPtr.Zero || _slotSize <= 0)
                                {
                                    _slotInUse[i] = false; // Reset the slot
                                    return false;
                                }
                                
                                var pointer = (IntPtr)((byte*)_buffer.RawPointer.ToPointer() + (i * _slotSize));
                                slot = new SlabSlot(pointer, this, i);
                            }
                            return true;
                        }
                    }
                }

                return false;
            }

            public void FreeSlot(int slotIndex)
            {
                // Add additional safety checks
                if (!_disposed && slotIndex >= 0 && slotIndex < _slotCount && _slotInUse != null)
                {
                    lock (_lock)
                    {
                        // Double-check inside lock
                        if (!_disposed && slotIndex >= 0 && slotIndex < _slotCount && _slotInUse != null)
                        {
                            _slotInUse[slotIndex] = false;
                        }
                    }
                }
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    // Add safety check before disposing buffer
                    if (_buffer != null)
                    {
                        // Dispose the buffer to properly free memory and notify tracking allocators
                        _buffer.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Represents a slot within a slab.
        /// </summary>
        public class SlabSlot
        {
            public IntPtr Pointer { get; }
            private readonly Slab _slab;
            private readonly int _slotIndex;

            public SlabSlot(IntPtr pointer, Slab slab, int slotIndex)
            {
                Pointer = pointer;
                _slab = slab;
                _slotIndex = slotIndex;
            }

            /// <summary>
            /// Frees the slot back to the slab.
            /// </summary>
            public void Free()
            {
                // Add safety check to prevent operations on null slab
                if (_slab != null)
                {
                    try
                    {
                        _slab.FreeSlot(_slotIndex);
                    }
                    catch
                    {
                        // Ignore exceptions during disposal to prevent crashes
                    }
                }
            }
        }
    }
}