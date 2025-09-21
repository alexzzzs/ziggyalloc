using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace ZiggyAlloc
{
    /// <summary>
    /// An allocator that automatically chooses between managed and unmanaged allocation based on size and type.
    /// </summary>
    public sealed class HybridAllocator : IUnmanagedMemoryAllocator, IDisposable
    {
        private readonly IUnmanagedMemoryAllocator _unmanagedAllocator;
        private long _totalAllocatedBytes;
        private bool _disposed = false;

        // Thresholds based on benchmark results - these can be made configurable in the future
        private const int BYTE_THRESHOLD = 1024;
        private const int INT_THRESHOLD = 512;
        private const int DOUBLE_THRESHOLD = 128;
        private const int STRUCT_THRESHOLD = 64;

        // Constants for allocation strategy calculations
        private const int MIN_THRESHOLD = 32; // Minimum threshold for unknown types
        private const int MAX_THRESHOLD = 1024; // Maximum threshold for unknown types

        /// <summary>
        /// Gets a value indicating that this allocator supports individual memory deallocation.
        /// </summary>
        public bool SupportsIndividualDeallocation => _unmanagedAllocator.SupportsIndividualDeallocation;

        /// <summary>
        /// Gets the total number of bytes allocated by this allocator.
        /// </summary>
        /// <remarks>
        /// This value tracks cumulative allocations but does not decrement when memory is freed.
        /// For unmanaged allocations, memory is freed immediately via Free().
        /// For managed allocations, memory is cleaned up by the GC when UnmanagedBuffer is disposed.
        /// This represents the total bytes ever allocated, not currently allocated bytes.
        /// </remarks>
        public long TotalAllocatedBytes => Interlocked.Read(ref _totalAllocatedBytes);

        /// <summary>
        /// Initializes a new instance of the HybridAllocator class.
        /// </summary>
        /// <param name="unmanagedAllocator">The unmanaged allocator to use when unmanaged allocation is chosen</param>
        public HybridAllocator(IUnmanagedMemoryAllocator unmanagedAllocator)
        {
            _unmanagedAllocator = unmanagedAllocator ?? throw new ArgumentNullException(nameof(unmanagedAllocator));
        }

        /// <summary>
        /// Allocates memory using either managed or unmanaged allocation based on size and type.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="elementCount">The number of elements to allocate space for</param>
        /// <param name="zeroMemory">Whether to zero-initialize the allocated memory</param>
        /// <returns>A buffer representing the allocated memory</returns>
        public unsafe UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false) where T : unmanaged
        {
            if (elementCount < 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount), "Element count cannot be negative");

            if (_disposed)
                throw new ObjectDisposedException(nameof(HybridAllocator));

            if (elementCount == 0)
            {
                // Return a valid but empty buffer for zero-length allocations
                // Don't pass the allocator since there's no memory to free
                return new UnmanagedBuffer<T>(null, 0);
            }

            // Calculate total size
            int elementSize = Marshal.SizeOf<T>();
            long totalSize = (long)elementCount * elementSize;
            if (totalSize > int.MaxValue)
                throw new OutOfMemoryException($"Allocation too large: {totalSize} bytes exceeds maximum allocation size");

            // Determine allocation strategy based on type and size
            bool useUnmanaged = ShouldUseUnmanagedAllocation<T>(elementCount, elementSize);

            if (useUnmanaged)
            {
                // Use unmanaged allocation for larger allocations where GC pressure is a concern
                var buffer = _unmanagedAllocator.Allocate<T>(elementCount, zeroMemory);
                
                // Update allocation tracking
                Interlocked.Add(ref _totalAllocatedBytes, totalSize);
                
                return new UnmanagedBuffer<T>((T*)buffer.RawPointer, elementCount, this);
            }
            else
            {
                // Use managed allocation for small allocations where it's faster
                // Create a managed array and pin it to get a pointer
                var managedArray = new T[elementCount];
                var handle = default(GCHandle);

                try
                {
                    // Zero-initialize if requested (arrays are zero-initialized by default in .NET)
                    if (ShouldClearManagedArray<T>(zeroMemory))
                    {
                        Array.Clear(managedArray, 0, managedArray.Length);
                    }

                    // Pin the array to get a stable pointer
                    handle = GCHandle.Alloc(managedArray, GCHandleType.Pinned);
                    var pointer = (T*)handle.AddrOfPinnedObject();

                    // Update allocation tracking
                    Interlocked.Add(ref _totalAllocatedBytes, totalSize);

                    // Create a buffer that wraps the managed memory with cleanup information
                    var managedArrayInfo = new HybridAllocator.ManagedArrayInfo
                    {
                        Array = managedArray,
                        Handle = handle
                    };
                    return new UnmanagedBuffer<T>(pointer, elementCount, managedArrayInfo);
                }
                catch
                {
                    // Clean up the GCHandle if it was allocated before re-throwing
                    if (handle.IsAllocated)
                    {
                        handle.Free();
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Determines whether unmanaged allocation should be used based on type and size.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ShouldUseUnmanagedAllocation<T>(int elementCount, int elementSize) where T : unmanaged
        {
            var type = typeof(T);
            
            if (type == typeof(byte))
            {
                return elementCount > BYTE_THRESHOLD;
            }
            else if (type == typeof(int))
            {
                return elementCount > INT_THRESHOLD;
            }
            else if (type == typeof(double))
            {
                return elementCount >= DOUBLE_THRESHOLD;
            }
            else if (type.IsValueType && !type.IsPrimitive)
            {
                return elementCount > STRUCT_THRESHOLD;
            }
            else
            {
                // For unknown types, use a reasonable threshold based on element size
                // Prevent very small thresholds for large element sizes
                int threshold = Math.Max(MIN_THRESHOLD, Math.Min(MAX_THRESHOLD, MAX_THRESHOLD / elementSize));
                return elementCount > threshold;
            }
        }

        /// <summary>
        /// Determines if additional zero-initialization is needed for managed arrays.
        /// </summary>
        /// <remarks>
        /// Arrays in .NET are zero-initialized by default, so explicit clearing is only
        /// needed when the zeroMemory flag is true and we're not using the default behavior.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ShouldClearManagedArray<T>(bool zeroMemory) where T : unmanaged
        {
            // Arrays in .NET are zero-initialized by default, but we may still need to clear
            // if the zeroMemory flag was explicitly set to true
            return zeroMemory; // Only clear if explicitly requested
        }

        /// <summary>
        /// Frees previously allocated unmanaged memory.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to free</param>
        /// <remarks>
        /// This method only handles unmanaged allocations. Managed allocations are automatically
        /// cleaned up by the garbage collector when the UnmanagedBuffer is disposed.
        /// Note: TotalAllocatedBytes only tracks allocations and does not decrement on deallocation
        /// as managed memory cleanup timing is non-deterministic.
        /// </remarks>
        public void Free(IntPtr pointer)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(HybridAllocator));

            if (pointer == IntPtr.Zero)
                return;

            try
            {
                // Delegate to the unmanaged allocator for unmanaged memory
                _unmanagedAllocator.Free(pointer);
                // Note: Managed arrays are automatically cleaned up by the GC when the UnmanagedBuffer is disposed
            }
            catch (Exception ex)
            {
                // Log exception in debug builds instead of silently ignoring
                #if DEBUG
                System.Diagnostics.Debug.WriteLine($"Exception during memory cleanup in HybridAllocator.Free: {ex}");
                #endif
                throw; // Re-throw to maintain original behavior for compatibility
            }
        }

        /// <summary>
        /// Disposes the HybridAllocator and releases any unmanaged resources.
        /// </summary>
        /// <remarks>
        /// The unmanaged allocator is not disposed here as this allocator does not own it.
        /// The owner of the HybridAllocator is responsible for disposing the underlying allocator.
        /// </remarks>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                // The unmanaged allocator should be disposed by its owner
                // We don't dispose it here as we don't own it
            }
        }

        /// <summary>
        /// Information about a managed array used for proper cleanup.
        /// </summary>
        internal class ManagedArrayInfo
        {
            public Array? Array { get; set; }
            public GCHandle Handle { get; set; }
        }
    }
}