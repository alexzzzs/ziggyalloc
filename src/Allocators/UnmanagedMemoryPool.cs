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
    public sealed class UnmanagedMemoryPool : IUnmanagedMemoryAllocator
    {
        private readonly ConcurrentDictionary<string, object> _pools; // Type+size -> pool mapping
        private readonly ConcurrentDictionary<IntPtr, (int size, string key)> _bufferInfo;
        private readonly IUnmanagedMemoryAllocator _baseAllocator;
        private long _totalAllocatedBytes;

        /// <summary>
        /// Gets a value indicating that this allocator supports individual memory deallocation.
        /// </summary>
        public bool SupportsIndividualDeallocation => true;

        /// <summary>
        /// Gets the total number of bytes currently allocated by this allocator.
        /// </summary>
        public long TotalAllocatedBytes => Interlocked.Read(ref _totalAllocatedBytes);

        /// <summary>
        /// Initializes a new instance of the UnmanagedMemoryPool class.
        /// </summary>
        /// <param name="baseAllocator">The underlying allocator to use for actual memory allocation when the pool is empty</param>
        public UnmanagedMemoryPool(IUnmanagedMemoryAllocator baseAllocator)
        {
            _pools = new ConcurrentDictionary<string, object>();
            _bufferInfo = new ConcurrentDictionary<IntPtr, (int size, string key)>();
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
            if (elementCount < 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount), "Element count cannot be negative");

            if (elementCount == 0)
            {
                // Return a valid but empty buffer for zero-length allocations
                return new UnmanagedBuffer<T>(null, 0, this);
            }

            // Calculate total size
            int elementSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
            long totalSize = (long)elementCount * elementSize;
            if (totalSize > int.MaxValue)
                throw new OutOfMemoryException($"Allocation too large: {totalSize} bytes exceeds maximum allocation size");

            IntPtr pointer = IntPtr.Zero;
            int sizeInBytes = (int)totalSize;
            string key = $"{typeof(T).FullName}:{sizeInBytes}";

            // Try to get a buffer from the pool
            if (_pools.TryGetValue(key, out object? poolObj) && 
                poolObj is ConcurrentQueue<IntPtr> poolQueue && 
                poolQueue.TryDequeue(out pointer))
            {
                // Successfully retrieved a buffer from the pool
                _bufferInfo.TryRemove(pointer, out _);
                
                // Zero the memory if requested
                if (zeroMemory)
                {
                    var byteSpan = new Span<byte>((void*)pointer, sizeInBytes);
                    byteSpan.Clear();
                }
            }
            else
            {
                // No buffer available in pool, allocate a new one
                var buffer = _baseAllocator.Allocate<T>(elementCount, zeroMemory);
                pointer = buffer.RawPointer;
                
                // Update allocation tracking
                Interlocked.Add(ref _totalAllocatedBytes, sizeInBytes);
            }

            // Track buffer info for disposal
            _bufferInfo.TryAdd(pointer, (sizeInBytes, key));

            return new UnmanagedBuffer<T>((T*)pointer, elementCount, this);
        }

        /// <summary>
        /// Frees previously allocated unmanaged memory by returning it to the pool.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to free</param>
        public void Free(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
                return;

            // Try to get the buffer info
            if (_bufferInfo.TryGetValue(pointer, out var info))
            {
                // Return to pool instead of actually freeing
                var poolQueue = _pools.GetOrAdd(info.key, _ => new ConcurrentQueue<IntPtr>()) as ConcurrentQueue<IntPtr>;
                if (poolQueue != null)
                {
                    poolQueue.Enqueue(pointer);
                }
            }
            else
            {
                // If we don't know about this buffer, delegate to base allocator
                _baseAllocator.Free(pointer);
            }
        }

        /// <summary>
        /// Clears all pooled buffers, freeing their memory.
        /// </summary>
        public void Clear()
        {
            foreach (var kvp in _pools)
            {
                if (kvp.Value is ConcurrentQueue<IntPtr> queue)
                {
                    while (queue.TryDequeue(out IntPtr pointer))
                    {
                        _bufferInfo.TryRemove(pointer, out _);
                        _baseAllocator.Free(pointer);
                    }
                }
            }
            
            // Reset allocation tracking
            Interlocked.Exchange(ref _totalAllocatedBytes, 0);
        }

        /// <summary>
        /// Disposes the pool, clearing all pooled buffers.
        /// </summary>
        public void Dispose()
        {
            Clear();
        }
    }
}