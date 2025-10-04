using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Runtime.InteropServices;

namespace ZiggyAlloc
{
    /// <summary>
    /// A thread-local memory pool for unmanaged buffers that eliminates lock contention by providing each thread with its own pools.
    /// </summary>
    /// <remarks>
    /// This pool offers significant performance improvements over shared pools in highly concurrent scenarios by eliminating
    /// synchronization overhead. Each thread maintains its own size-class pools, reducing contention and improving scalability.
    /// 
    /// Key benefits:
    /// - Zero lock contention for size-class allocations
    /// - Better cache locality per thread
    /// - Improved scalability for multi-threaded applications
    /// - Optional cross-thread buffer sharing for memory efficiency
    /// - Automatic cleanup of thread-local pools when threads terminate
    /// 
    /// Best used for:
    /// - Highly concurrent applications with many threads
    /// - Performance-critical code with frequent allocations per thread
    /// - Applications where threads have predictable allocation patterns
    /// - Scenarios requiring maximum allocation throughput
    /// </remarks>
    public sealed class ThreadLocalMemoryPool : IUnmanagedMemoryAllocator, IDisposable
    {
        private readonly IUnmanagedMemoryAllocator _baseAllocator;

        // Thread-local storage for per-thread pool data
        private readonly ThreadLocal<ThreadLocalPoolData> _threadLocalPools;

        // Optional cross-thread sharing for unused buffers
        private readonly bool _enableCrossThreadSharing;
        private readonly ConcurrentQueue<SharedBuffer>? _sharedBuffers;
        private readonly Timer? _cleanupTimer;


        private long _totalAllocatedBytes;
        private long _totalPoolsCreated;
        private int _activePoolCount;
        private bool _disposed = false;

        /// <summary>
        /// Gets a value indicating that this allocator supports individual memory deallocation.
        /// </summary>
        public bool SupportsIndividualDeallocation => true;

        /// <summary>
        /// Gets the total number of bytes currently allocated by this allocator across all threads.
        /// </summary>
        public long TotalAllocatedBytes => Interlocked.Read(ref _totalAllocatedBytes);

        /// <summary>
        /// Gets the total number of thread-local pools created.
        /// </summary>
        public long TotalPoolsCreated => Interlocked.Read(ref _totalPoolsCreated);

        /// <summary>
        /// Gets the number of active thread-local pools.
        /// </summary>
        public int ActivePoolCount => _activePoolCount;

        /// <summary>
        /// Initializes a new instance of the ThreadLocalMemoryPool class.
        /// </summary>
        /// <param name="baseAllocator">The underlying allocator to use for actual memory allocation when pools are empty</param>
        /// <param name="enableCrossThreadSharing">Whether to enable sharing of unused buffers between threads</param>
        public ThreadLocalMemoryPool(IUnmanagedMemoryAllocator baseAllocator, bool enableCrossThreadSharing = true)
        {
            _baseAllocator = baseAllocator ?? throw new ArgumentNullException(nameof(baseAllocator));
            _enableCrossThreadSharing = enableCrossThreadSharing;
            _sharedBuffers = enableCrossThreadSharing ? new ConcurrentQueue<SharedBuffer>() : null;

            // Initialize thread-local storage
            _threadLocalPools = new ThreadLocal<ThreadLocalPoolData>(CreateThreadLocalPool, trackAllValues: true);

            // Set up periodic cleanup of abandoned thread pools (every 30 seconds)
            if (enableCrossThreadSharing && _sharedBuffers != null)
            {
                _cleanupTimer = new Timer(CleanupAbandonedPools, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
            }
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
                throw new ObjectDisposedException(nameof(ThreadLocalMemoryPool));

            if (elementCount < 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount), "Element count cannot be negative");

            if (elementCount == 0)
            {
                return new UnmanagedBuffer<T>(null, 0, this);
            }

            int sizeInBytes = elementCount * sizeof(T);
            var threadPool = GetThreadLocalPool();

            // Try thread-local size-class pools first
            int sizeClassIndex = FindSizeClass(sizeInBytes);
            if (sizeClassIndex >= 0 && threadPool.TryAllocateFromSizeClass(sizeClassIndex, out var pointer))
            {
                if (zeroMemory)
                {
                    new Span<byte>((void*)pointer, sizeInBytes).Clear();
                }
                // Track the allocation in thread-local pool for proper cleanup
                threadPool.TrackAllocation(pointer, sizeInBytes);
                return new UnmanagedBuffer<T>((T*)pointer, elementCount, this);
            }

            // Try fallback pool for uncommon sizes
            if (threadPool.TryAllocateFromFallback(sizeInBytes, out pointer))
            {
                if (zeroMemory)
                {
                    new Span<byte>((void*)pointer, sizeInBytes).Clear();
                }
                // Track the allocation in thread-local pool for proper cleanup
                threadPool.TrackAllocation(pointer, sizeInBytes);
                return new UnmanagedBuffer<T>((T*)pointer, elementCount, this);
            }

            // Try cross-thread sharing if enabled
            if (_enableCrossThreadSharing && TryGetSharedBuffer(sizeInBytes, out pointer))
            {
                if (zeroMemory)
                {
                    new Span<byte>((void*)pointer, sizeInBytes).Clear();
                }
                // Track the allocation in thread-local pool for proper cleanup
                threadPool.TrackExternalAllocation(pointer, sizeInBytes);
                return new UnmanagedBuffer<T>((T*)pointer, elementCount, this);
            }

            // Allocate new buffer from base allocator
            var buffer = _baseAllocator.Allocate<T>(elementCount, zeroMemory);
            Interlocked.Add(ref _totalAllocatedBytes, buffer.SizeInBytes);

            // Track the allocation in thread-local pool for proper cleanup
            threadPool.TrackAllocation(buffer.RawPointer, buffer.SizeInBytes);

            return new UnmanagedBuffer<T>((T*)buffer.RawPointer, buffer.Length, this);
        }

        /// <summary>
        /// Frees previously allocated unmanaged memory.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to free</param>
        public void Free(IntPtr pointer)
        {
            if (_disposed || pointer == IntPtr.Zero)
                return;

            var threadPool = GetThreadLocalPool();

            // Try to return to thread-local pools first
            if (threadPool.TryReturnToPool(pointer, out var size))
            {
                // Memory was returned to pool, no need to decrement total allocated bytes
                // as it was already accounted for when allocated
                return;
            }

            // If not found in thread-local pools, offer for cross-thread sharing
            if (_enableCrossThreadSharing && _sharedBuffers != null)
            {
                _sharedBuffers.Enqueue(new SharedBuffer(pointer, size));
            }
            else
            {
                // Free directly if cross-thread sharing is disabled
                _baseAllocator.Free(pointer);
                Interlocked.Add(ref _totalAllocatedBytes, -size);
            }
        }

        /// <summary>
        /// Clears all pooled buffers across all threads, freeing their memory.
        /// </summary>
        public void Clear()
        {
            if (_disposed)
                return;

            // Clear all thread-local pools and account for freed bytes
            long totalFreedBytes = 0;
            if (_threadLocalPools.Values != null)
            {
                foreach (var threadPool in _threadLocalPools.Values)
                {
                    totalFreedBytes += threadPool.Clear();
                }
            }

            // Clear shared buffers and account for freed bytes
            if (_enableCrossThreadSharing && _sharedBuffers != null)
            {
                while (_sharedBuffers.TryDequeue(out var sharedBuffer))
                {
                    _baseAllocator.Free(sharedBuffer.Pointer);
                    totalFreedBytes += sharedBuffer.Size;
                }
            }

            // Reset total allocated bytes (matches UnmanagedMemoryPool behavior)
            Interlocked.Exchange(ref _totalAllocatedBytes, 0);
        }

        /// <summary>
        /// Attempts to get a buffer from cross-thread sharing.
        /// </summary>
        private bool TryGetSharedBuffer(int sizeInBytes, out IntPtr pointer)
        {
            if (_sharedBuffers?.TryDequeue(out var sharedBuffer) == true)
            {
                if (sharedBuffer.Size == sizeInBytes)
                {
                    pointer = sharedBuffer.Pointer;
                    return true;
                }
                else
                {
                    // Size doesn't match, return it and keep looking
                    _sharedBuffers.Enqueue(sharedBuffer);
                }
            }

            pointer = IntPtr.Zero;
            return false;
        }

        /// <summary>
        /// Creates a new thread-local pool data instance.
        /// </summary>
        private ThreadLocalPoolData CreateThreadLocalPool()
        {
            Interlocked.Increment(ref _totalPoolsCreated);
            Interlocked.Increment(ref _activePoolCount);
            return new ThreadLocalPoolData(_baseAllocator, DecrementActivePoolCount);
        }

        /// <summary>
        /// Decrements the active pool count (called when a thread pool is disposed).
        /// </summary>
        private void DecrementActivePoolCount()
        {
            Interlocked.Decrement(ref _activePoolCount);
        }

        /// <summary>
        /// Gets the value for the current thread, creating it if necessary.
        /// </summary>
        private ThreadLocalPoolData GetThreadLocalPool()
        {
            return _threadLocalPools.Value ?? new ThreadLocalPoolData(_baseAllocator);
        }

        /// <summary>
        /// Cleans up abandoned thread pools (called periodically).
        /// </summary>
        private const int MaxSharedBuffers = 1000;

        private void CleanupAbandonedPools(object? state)
        {
            if (_disposed)
                return;

            if (_enableCrossThreadSharing && _sharedBuffers != null)
            {
                while (_sharedBuffers.Count > MaxSharedBuffers)
                {
                    if (_sharedBuffers.TryDequeue(out var sharedBuffer))
                    {
                        _baseAllocator.Free(sharedBuffer.Pointer);
                        Interlocked.Add(ref _totalAllocatedBytes, -sharedBuffer.Size);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Finds the appropriate size class for the given size.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindSizeClass(int sizeInBytes)
        {
            return AllocatorConstants.FindSizeClass(sizeInBytes);
        }

        /// <summary>
        /// Disposes the pool.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _cleanupTimer?.Dispose();
                Clear();
                _threadLocalPools.Dispose();
            }
        }

        /// <summary>
        /// Represents a buffer shared between threads.
        /// </summary>
        private readonly struct SharedBuffer
        {
            public readonly IntPtr Pointer;
            public readonly int Size;

            public SharedBuffer(IntPtr pointer, int size)
            {
                Pointer = pointer;
                Size = size;
            }
        }

        /// <summary>
        /// Thread-local pool data containing per-thread pools and tracking information.
        /// </summary>
        private sealed class ThreadLocalPoolData
        {
            // Size-class pools (no locks needed since they're thread-local)
            private readonly IntPtr[][] _sizeClassPools = new IntPtr[AllocatorConstants.MaxSizeClasses][];
            private readonly int[] _sizeClassSizes = new int[AllocatorConstants.MaxSizeClasses];
            private readonly int[] _poolCounts = new int[AllocatorConstants.MaxSizeClasses];

            // Fallback pool for uncommon sizes
            private readonly ConcurrentDictionary<int, ConcurrentStack<IntPtr>> _fallbackPools = new();

            // Track allocations made by this thread for proper cleanup
            private readonly ConcurrentDictionary<IntPtr, int> _threadAllocations = new();

            // Base allocator for freeing buffers
            private readonly IUnmanagedMemoryAllocator _baseAllocator;

            // Callback to decrement active pool count when this instance is disposed
            private readonly Action? _disposeCallback;

            public ThreadLocalPoolData(IUnmanagedMemoryAllocator baseAllocator, Action? disposeCallback = null)
            {
                _baseAllocator = baseAllocator ?? throw new ArgumentNullException(nameof(baseAllocator));
                _disposeCallback = disposeCallback;

                // Initialize size classes
                for (int i = 0; i < AllocatorConstants.MaxSizeClasses && i < AllocatorConstants.SizeClasses.Length; i++)
                {
                    _sizeClassSizes[i] = AllocatorConstants.SizeClasses[i];
                    _sizeClassPools[i] = new IntPtr[AllocatorConstants.MaxSlotsPerClass];
                }
            }

            ~ThreadLocalPoolData()
            {
                _disposeCallback?.Invoke();
            }

            /// <summary>
            /// Attempts to allocate from a size class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryAllocateFromSizeClass(int sizeClassIndex, out IntPtr pointer)
            {
                if (sizeClassIndex < 0 || sizeClassIndex >= AllocatorConstants.MaxSizeClasses)
                {
                    pointer = IntPtr.Zero;
                    return false;
                }

                var pool = _sizeClassPools[sizeClassIndex];
                int count = _poolCounts[sizeClassIndex];

                if (count > 0)
                {
                    count--;
                    _poolCounts[sizeClassIndex] = count;
                    pointer = pool[count];
                    return true;
                }

                pointer = IntPtr.Zero;
                return false;
            }

            /// <summary>
            /// Attempts to allocate from fallback pools.
            /// </summary>
            public bool TryAllocateFromFallback(int sizeInBytes, out IntPtr pointer)
            {
                var fallbackPool = _fallbackPools.GetOrAdd(sizeInBytes, _ => new ConcurrentStack<IntPtr>());
                return fallbackPool.TryPop(out pointer);
            }

            /// <summary>
            /// Attempts to return a pointer to the appropriate pool.
            /// </summary>
            public bool TryReturnToPool(IntPtr pointer, out int size)
            {
                if (_threadAllocations.TryGetValue(pointer, out size))
                {
                    _threadAllocations.TryRemove(pointer, out _);

                    // Try size-class pools first
                    int sizeClassIndex = FindSizeClass(size);
                    if (sizeClassIndex >= 0 && TryReturnToSizeClass(sizeClassIndex, pointer))
                    {
                        return true;
                    }

                    // Try fallback pools
                    var fallbackPool = _fallbackPools.GetOrAdd(size, _ => new ConcurrentStack<IntPtr>());
                    fallbackPool.Push(pointer);
                    return true;
                }

                size = 0;
                return false;
            }

            /// <summary>
            /// Tracks an allocation made by this thread.
            /// </summary>
            public void TrackExternalAllocation(IntPtr pointer, int size)
            {
                _threadAllocations[pointer] = size;
            }

            /// <summary>
            /// Tracks an allocation made by this thread.
            /// </summary>
            public void TrackAllocation(IntPtr pointer, int size)
            {
                _threadAllocations[pointer] = size;
            }

            /// <summary>
            /// Attempts to return a pointer to a specific size class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool TryReturnToSizeClass(int sizeClassIndex, IntPtr pointer)
            {
                if (sizeClassIndex < 0 || sizeClassIndex >= AllocatorConstants.MaxSizeClasses)
                {
                    return false;
                }

                var pool = _sizeClassPools[sizeClassIndex];
                int count = _poolCounts[sizeClassIndex];

                if (count < AllocatorConstants.MaxSlotsPerClass)
                {
                    pool[count] = pointer;
                    _poolCounts[sizeClassIndex] = count + 1;
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Clears all pools in this thread-local data.
            /// </summary>
            /// <returns>The total number of bytes that were freed</returns>
            public long Clear()
            {
                long totalFreedBytes = 0;

                // Free all buffers in size-class pools before clearing
                for (int i = 0; i < AllocatorConstants.MaxSizeClasses; i++)
                {
                    var pool = _sizeClassPools[i];
                    int count = _poolCounts[i];

                    // Free each buffer in the pool (guard against null pointers)
                    for (int j = 0; j < count; j++)
                    {
                        if (pool[j] != IntPtr.Zero)
                        {
                            _baseAllocator.Free(pool[j]);
                            totalFreedBytes += _sizeClassSizes[i];
                        }
                    }

                    _poolCounts[i] = 0;
                }

                // Free all buffers in fallback pools before clearing
                foreach (var kvp in _fallbackPools)
                {
                    var fallbackPool = kvp.Value;
                    int sizeInBytes = kvp.Key;

                    while (fallbackPool.TryPop(out var pointer))
                    {
                        if (pointer != IntPtr.Zero)
                        {
                            _baseAllocator.Free(pointer);
                            totalFreedBytes += sizeInBytes;
                        }
                    }
                }
                _fallbackPools.Clear();

                // Clear tracked allocations (these are just tracking, not actual allocations to free)
                _threadAllocations.Clear();

                return totalFreedBytes;
            }

            /// <summary>
            /// Gets a snapshot of all allocations for calculating totals before clearing.
            /// </summary>
            public Dictionary<IntPtr, int> GetAllocationsSnapshot()
            {
                return new Dictionary<IntPtr, int>(_threadAllocations);
            }

            /// <summary>
            /// Gets the total bytes that were newly allocated by this thread (not reused from pools).
            /// </summary>
            public long GetTotalNewlyAllocatedBytes()
            {
                long total = 0;
                foreach (var size in _threadAllocations.Values)
                {
                    total += size;
                }
                return total;
            }

            /// <summary>
            /// Gets the total bytes that are currently pooled (available for reuse).
            /// This includes size-class pools and fallback pools.
            /// </summary>
            public long GetTotalPooledBytes()
            {
                long total = 0;

                // Add memory from size-class pools
                for (int i = 0; i < AllocatorConstants.MaxSizeClasses; i++)
                {
                    int count = _poolCounts[i];
                    if (count > 0 && i < _sizeClassSizes.Length)
                    {
                        total += count * (long)_sizeClassSizes[i];
                    }
                }

                // Add memory from fallback pools (estimate based on allocations)
                // This is an approximation since we don't track pooled vs new allocations separately
                long newlyAllocated = GetTotalNewlyAllocatedBytes();
                long totalTracked = 0;
                foreach (var size in _threadAllocations.Values)
                {
                    totalTracked += size;
                }

                // The pooled bytes are the difference between tracked and newly allocated
                // This is a simplified calculation - in practice we'd need more sophisticated tracking
                return Math.Max(0, totalTracked - newlyAllocated);
            }
        }
    }
}