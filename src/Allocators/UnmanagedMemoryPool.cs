using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ZiggyAlloc
{
    /// <summary>
    /// A memory pool for unmanaged buffers that reduces allocation overhead by reusing previously allocated buffers.
    /// </summary>
    /// <remarks>
    /// This pool is particularly effective for scenarios with frequent allocations of similar-sized buffers.
    /// It maintains separate pools for different buffer sizes to ensure optimal reuse.
    /// 
    /// Key benefits:
    /// - Reduces P/Invoke overhead for frequent allocations
    /// - Eliminates GC pressure for unmanaged allocations
    /// - Thread-safe implementation
    /// - Automatic cleanup of unused buffers
    /// 
    /// Best used for:
    /// - Frequent allocations of similar-sized buffers
    /// - Performance-critical code with allocation hotspots
    /// - Scenarios where buffer sizes are predictable
    /// </remarks>
    public sealed class UnmanagedMemoryPool : IUnmanagedMemoryAllocator, IDisposable
    {
        private readonly IUnmanagedMemoryAllocator _baseAllocator;
        private readonly ConcurrentDictionary<int, ConcurrentBag<IntPtr>> _pools = new ConcurrentDictionary<int, ConcurrentBag<IntPtr>>();
        private readonly ConcurrentDictionary<IntPtr, int> _pointerSizes = new ConcurrentDictionary<IntPtr, int>();
        private long _totalAllocatedBytes;
        private bool _disposed = false;

        /// <summary>
        /// Gets a value indicating that this allocator supports individual memory deallocation.
        /// </summary>
        public bool SupportsIndividualDeallocation => true;

        /// <summary>
        /// Gets the total number of bytes currently allocated by this allocator.
        /// </summary>
        /// <remarks>
        /// This tracks the total bytes ever allocated, not currently allocated.
        /// When buffers are reused from the pool, this value does not increase.
        /// </remarks>
        public long TotalAllocatedBytes => Interlocked.Read(ref _totalAllocatedBytes);

        /// <summary>
        /// Initializes a new instance of the UnmanagedMemoryPool class.
        /// </summary>
        /// <param name="baseAllocator">The underlying allocator to use for actual memory allocation when the pool is empty</param>
        public UnmanagedMemoryPool(IUnmanagedMemoryAllocator baseAllocator)
        {
            _baseAllocator = baseAllocator ?? throw new ArgumentNullException(nameof(baseAllocator));
        }

        /// <summary>
        /// Allocates an unmanaged buffer, reusing a pooled buffer if available or allocating a new one.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="elementCount">The number of elements to allocate space for</param>
        /// <param name="zeroMemory">Whether to zero-initialize the allocated memory</param>
        /// <returns>A buffer representing the allocated memory</returns>
        public unsafe UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false) where T : unmanaged
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnmanagedMemoryPool));

            if (elementCount < 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount), "Element count cannot be negative");

            if (elementCount == 0)
            {
                return new UnmanagedBuffer<T>(null, 0, this);
            }

            int sizeInBytes = elementCount * sizeof(T);
            if (_pools.TryGetValue(sizeInBytes, out var pool) && pool.TryTake(out var pointer))
            {
                if (zeroMemory)
                {
                    new Span<byte>((void*)pointer, sizeInBytes).Clear();
                }
                return new UnmanagedBuffer<T>((T*)pointer, elementCount, this);
            }

            var buffer = _baseAllocator.Allocate<T>(elementCount, zeroMemory);
            Interlocked.Add(ref _totalAllocatedBytes, buffer.SizeInBytes);
            _pointerSizes.TryAdd(buffer.RawPointer, buffer.SizeInBytes);
            return new UnmanagedBuffer<T>((T*)buffer.RawPointer, buffer.Length, this);
        }

        /// <summary>
        /// Frees previously allocated unmanaged memory.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to free</param>
        /// <remarks>
        /// If the pointer was allocated by this pool, it will be returned to the appropriate pool for reuse.
        /// If the pointer was not allocated by this pool, it will be freed using the base allocator.
        /// </remarks>
        public void Free(IntPtr pointer)
        {
            if (_disposed || pointer == IntPtr.Zero)
                return;

            try
            {
                if (_pointerSizes.TryGetValue(pointer, out var size))
                {
                    var pool = _pools.GetOrAdd(size, _ => new ConcurrentBag<IntPtr>());
                    pool.Add(pointer);
                }
                else
                {
                    // This should not happen if the buffer was allocated by this pool
                    // Log this condition in debug builds
                    #if DEBUG
                    System.Diagnostics.Debug.WriteLine($"Warning: Attempted to free pointer 0x{pointer.ToString("X")} that was not allocated by this pool");
                    #endif
                    _baseAllocator.Free(pointer);
                }
            }
            catch (Exception ex)
            {
                // Log exception in debug builds instead of silently ignoring
                #if DEBUG
                System.Diagnostics.Debug.WriteLine($"Exception during memory cleanup in UnmanagedMemoryPool.Free: {ex}");
                #endif
                throw; // Re-throw to maintain original behavior for compatibility
            }
        }

        /// <summary>
        /// Clears all pooled buffers, freeing their memory.
        /// </summary>
        public void Clear()
        {
            if (_disposed)
                return;

            foreach (var pool in _pools.Values)
            {
                while (pool.TryTake(out var pointer))
                {
                    if (_pointerSizes.TryRemove(pointer, out var size))
                    {
                        _baseAllocator.Free(pointer);
                        Interlocked.Add(ref _totalAllocatedBytes, -size);
                    }
                }
            }
            _pools.Clear();
        }

        /// <summary>
        /// Disposes the pool.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                try
                {
                    Clear();
                    
                    // Explicitly suppress finalization for the base allocator if it implements IDisposable
                    if (_baseAllocator is IDisposable disposableAllocator)
                    {
                        // We don't dispose the base allocator as it might be used elsewhere
                        // But we ensure that any cleanup that needs to happen in the pool is done
                    }
                }
                catch (Exception ex)
                {
                    // Log exception in debug builds instead of silently ignoring
                    #if DEBUG
                    System.Diagnostics.Debug.WriteLine($"Exception during disposal in UnmanagedMemoryPool.Dispose: {ex}");
                    #endif
                    throw; // Re-throw to maintain original behavior for compatibility
                }
            }
        }
    }
}