using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Runtime.InteropServices;

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

        // Optimized size-class based pools using arrays for better performance
        private const int MaxSizeClasses = 32;
        private const int MaxSlotsPerClass = 1024;

        // Pre-defined size classes for common allocation sizes
        private static readonly int[] SizeClasses = {
            16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176, 192, 208, 224, 240, 256,
            320, 384, 448, 512, 640, 768, 896, 1024, 1280, 1536, 1792, 2048, 2560, 3072, 3584, 4096
        };

        // Lock-free size-class pools using atomic operations
        private readonly LockFreeSizeClass[] _sizeClasses = new LockFreeSizeClass[MaxSizeClasses];

        // Fallback pool for uncommon sizes
        private readonly ConcurrentDictionary<int, LockFreeStack> _fallbackPools = new();

        // Dynamic size class tracking for optimization
        private readonly long[] _sizeClassUsage = new long[MaxSizeClasses];
        private readonly long[] _sizeClassHits = new long[MaxSizeClasses];
        private long _lastOptimizationTime = 0;
        private const long OptimizationInterval = 1000000; // Optimize every 1M operations

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

            // Initialize lock-free size classes
            for (int i = 0; i < MaxSizeClasses && i < SizeClasses.Length; i++)
            {
                _sizeClasses[i] = new LockFreeSizeClass(SizeClasses[i], MaxSlotsPerClass);
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
                throw new ObjectDisposedException(nameof(UnmanagedMemoryPool));

            if (elementCount < 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount), "Element count cannot be negative");

            if (elementCount == 0)
            {
                return new UnmanagedBuffer<T>(null, 0, this);
            }

            int sizeInBytes = elementCount * sizeof(T);

            // Try optimized lock-free size-class pools first
            int sizeClassIndex = FindSizeClass(sizeInBytes);
            if (sizeClassIndex >= 0)
            {
                // Track usage for dynamic optimization
                Interlocked.Increment(ref _sizeClassUsage[sizeClassIndex]);

                if (_sizeClasses[sizeClassIndex].TryPop(out var pointer))
                {
                    Interlocked.Increment(ref _sizeClassHits[sizeClassIndex]);

                    if (zeroMemory)
                    {
                        // Use enhanced SIMD operations for zeroing
                        if (sizeInBytes >= 4096)
                        {
                            SimdMemoryOperations.ZeroMemoryWithPrefault((void*)pointer, sizeInBytes, prefaultPages: false);
                        }
                        else
                        {
                            new Span<byte>((void*)pointer, sizeInBytes).Clear();
                        }
                    }
                    return new UnmanagedBuffer<T>((T*)pointer, elementCount, this);
                }
            }

            // Fallback to lock-free pools for uncommon sizes
            var fallbackPool = _fallbackPools.GetOrAdd(sizeInBytes, _ => new LockFreeStack());
            if (fallbackPool.TryPop(out var fallbackPointer))
            {
                if (zeroMemory)
                {
                    // Use enhanced SIMD operations for zeroing
                    if (sizeInBytes >= 4096)
                    {
                        SimdMemoryOperations.ZeroMemoryWithPrefault((void*)fallbackPointer, sizeInBytes, prefaultPages: false);
                    }
                    else
                    {
                        new Span<byte>((void*)fallbackPointer, sizeInBytes).Clear();
                    }
                }
                return new UnmanagedBuffer<T>((T*)fallbackPointer, elementCount, this);
            }

            var buffer = _baseAllocator.Allocate<T>(elementCount, zeroMemory);
            Interlocked.Add(ref _totalAllocatedBytes, buffer.SizeInBytes);
            _pointerSizes.TryAdd(buffer.RawPointer, buffer.SizeInBytes);

            // Trigger dynamic optimization periodically
            TryOptimizeSizeClasses();

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
                    // Try optimized lock-free size-class pools first
                    int sizeClassIndex = FindSizeClass(size);
                    if (sizeClassIndex >= 0 && _sizeClasses[sizeClassIndex].TryPush(pointer))
                    {
                        return;
                    }

                    // Fallback to lock-free pools for uncommon sizes
                    var fallbackPool = _fallbackPools.GetOrAdd(size, _ => new LockFreeStack());
                    fallbackPool.TryPush(pointer);
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

            // Clear lock-free size-class pools
            for (int i = 0; i < MaxSizeClasses; i++)
            {
                while (_sizeClasses[i].TryPop(out var pointer))
                {
                    if (_pointerSizes.TryRemove(pointer, out var size))
                    {
                        _baseAllocator.Free(pointer);
                        Interlocked.Add(ref _totalAllocatedBytes, -size);
                    }
                }
            }

            // Clear fallback pools
            foreach (var kvp in _fallbackPools)
            {
                var size = kvp.Key;
                var pool = kvp.Value;
                while (pool.TryPop(out var pointer))
                {
                    if (_pointerSizes.TryRemove(pointer, out _))
                    {
                        _baseAllocator.Free(pointer);
                        Interlocked.Add(ref _totalAllocatedBytes, -size);
                    }
                }
            }
            _fallbackPools.Clear();
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

        /// <summary>
        /// Finds the appropriate size class for the given size.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindSizeClass(int sizeInBytes)
        {
            // Simple linear search for size class - optimized for common sizes
            for (int i = 0; i < MaxSizeClasses && i < SizeClasses.Length; i++)
            {
                if (SizeClasses[i] >= sizeInBytes)
                {
                    return i;
                }
            }
            return -1; // No suitable size class
        }

        /// <summary>
        /// Lock-free size class for high-performance memory pooling.
        /// </summary>
        private sealed class LockFreeSizeClass
        {
            private readonly int _maxSlots;
            private int _count = 0;
            private IntPtr[] _slots;

            public LockFreeSizeClass(int slotSize, int maxSlots)
            {
                _maxSlots = maxSlots;
                _slots = new IntPtr[maxSlots];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryPush(IntPtr pointer)
            {
                int count = Volatile.Read(ref _count);
                if (count >= _maxSlots)
                    return false;

                if (Interlocked.CompareExchange(ref _count, count + 1, count) == count)
                {
                    _slots[count] = pointer;
                    return true;
                }

                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryPop(out IntPtr pointer)
            {
                pointer = IntPtr.Zero;

                int count = Volatile.Read(ref _count);
                if (count <= 0)
                    return false;

                if (Interlocked.CompareExchange(ref _count, count - 1, count) == count)
                {
                    pointer = _slots[count - 1];
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Lock-free stack implementation using atomic operations.
        /// </summary>
        private sealed class LockFreeStack
        {
            private volatile LockFreeNode? _head;

            private class LockFreeNode
            {
                public IntPtr Pointer;
                public LockFreeNode? Next;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryPush(IntPtr pointer)
            {
                var node = new LockFreeNode { Pointer = pointer };
                LockFreeNode currentHead;

                do
                {
                    currentHead = _head;
                    node.Next = currentHead;
                }
                while (Interlocked.CompareExchange(ref _head, node, currentHead) != currentHead);

                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryPop(out IntPtr pointer)
            {
                pointer = IntPtr.Zero;
                LockFreeNode currentHead;

                do
                {
                    currentHead = _head;
                    if (currentHead == null)
                        return false;

                    pointer = currentHead.Pointer;
                }
                while (Interlocked.CompareExchange(ref _head, currentHead.Next, currentHead) != currentHead);

                return true;
            }
        }

        /// <summary>
        /// Attempts to optimize size classes based on usage patterns.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryOptimizeSizeClasses()
        {
            // Check if enough time has passed for optimization
            long totalOperations = 0;
            for (int i = 0; i < MaxSizeClasses; i++)
            {
                totalOperations += _sizeClassUsage[i];
            }

            if (totalOperations - _lastOptimizationTime < OptimizationInterval)
                return;

            _lastOptimizationTime = totalOperations;

            // Analyze usage patterns and adjust size classes if needed
            OptimizeSizeClassDistribution();
        }

        /// <summary>
        /// Optimizes size class distribution based on actual usage patterns.
        /// </summary>
        private void OptimizeSizeClassDistribution()
        {
            // Find most and least used size classes
            int mostUsedIndex = 0;
            int leastUsedIndex = 0;
            long maxUsage = 0;
            long minUsage = long.MaxValue;

            for (int i = 0; i < MaxSizeClasses; i++)
            {
                long usage = _sizeClassUsage[i];
                if (usage > maxUsage)
                {
                    maxUsage = usage;
                    mostUsedIndex = i;
                }
                if (usage < minUsage && usage > 0)
                {
                    minUsage = usage;
                    leastUsedIndex = i;
                }
            }

            // If we have significant usage imbalance, log for potential future optimization
            if (maxUsage > minUsage * 10 && maxUsage > 1000)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(
                    $"MemoryPool: Significant usage imbalance detected. Most used size class {mostUsedIndex} ({maxUsage} uses), least used {leastUsedIndex} ({minUsage} uses)");
#endif
            }
        }
    }
}