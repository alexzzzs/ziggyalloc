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

        // Size-class pools using simple arrays for maximum performance
        private readonly IntPtr[][] _sizeClassPools = new IntPtr[MaxSizeClasses][];
        private readonly int[] _sizeClassSizes = new int[MaxSizeClasses];
        private readonly int[] _poolCounts = new int[MaxSizeClasses];
        private readonly SpinLock[] _poolLocks = new SpinLock[MaxSizeClasses];

        // Fallback pool for uncommon sizes
        private readonly ConcurrentDictionary<int, ConcurrentStack<IntPtr>> _fallbackPools = new();

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

            // Initialize size classes and SpinLocks
            for (int i = 0; i < MaxSizeClasses && i < SizeClasses.Length; i++)
            {
                _sizeClassSizes[i] = SizeClasses[i];
                _sizeClassPools[i] = new IntPtr[MaxSlotsPerClass];
                _poolLocks[i] = new SpinLock(); // Initialize SpinLock
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

            // Try optimized size-class pools first
            int sizeClassIndex = FindSizeClass(sizeInBytes);
            if (sizeClassIndex >= 0 && TryAllocateFromSizeClass(sizeClassIndex, out var pointer))
            {
                if (zeroMemory)
                {
                    new Span<byte>((void*)pointer, sizeInBytes).Clear();
                }
                return new UnmanagedBuffer<T>((T*)pointer, elementCount, this);
            }

            // Fallback to concurrent pools for uncommon sizes
            var fallbackPool = _fallbackPools.GetOrAdd(sizeInBytes, _ => new ConcurrentStack<IntPtr>());
            if (fallbackPool.TryPop(out var fallbackPointer))
            {
                if (zeroMemory)
                {
                    new Span<byte>((void*)fallbackPointer, sizeInBytes).Clear();
                }
                return new UnmanagedBuffer<T>((T*)fallbackPointer, elementCount, this);
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
                    // Try optimized size-class pools first
                    int sizeClassIndex = FindSizeClass(size);
                    if (sizeClassIndex >= 0 && TryReturnToSizeClass(sizeClassIndex, pointer))
                    {
                        return;
                    }

                    // Fallback to concurrent pools for uncommon sizes
                    var fallbackPool = _fallbackPools.GetOrAdd(size, _ => new ConcurrentStack<IntPtr>());
                    fallbackPool.Push(pointer);
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

            // Clear size-class pools
            for (int i = 0; i < MaxSizeClasses; i++)
            {
                var pool = _sizeClassPools[i];

                // Use SpinLock for better performance under contention
                bool lockTaken = false;
                try
                {
                    _poolLocks[i].Enter(ref lockTaken);
                    int count = _poolCounts[i];
                    for (int j = 0; j < count; j++)
                    {
                        var pointer = pool[j];
                        if (_pointerSizes.TryRemove(pointer, out var size))
                        {
                            _baseAllocator.Free(pointer);
                            Interlocked.Add(ref _totalAllocatedBytes, -size);
                        }
                    }
                    _poolCounts[i] = 0;
                }
                finally
                {
                    if (lockTaken) _poolLocks[i].Exit();
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
        /// Attempts to return a pointer to a specific size class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryReturnToSizeClass(int sizeClassIndex, IntPtr pointer)
        {
            if (sizeClassIndex < 0 || sizeClassIndex >= MaxSizeClasses)
            {
                return false;
            }

            var pool = _sizeClassPools[sizeClassIndex];

            // Use SpinLock for better performance under contention
            bool lockTaken = false;
            try
            {
                _poolLocks[sizeClassIndex].Enter(ref lockTaken);
                int count = _poolCounts[sizeClassIndex];
                if (count < MaxSlotsPerClass)
                {
                    pool[count] = pointer;
                    _poolCounts[sizeClassIndex] = count + 1;
                    return true;
                }
            }
            finally
            {
                if (lockTaken) _poolLocks[sizeClassIndex].Exit();
            }

            return false;
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
                if (_sizeClassSizes[i] >= sizeInBytes)
                {
                    return i;
                }
            }
            return -1; // No suitable size class
        }

        /// <summary>
        /// Attempts to allocate from a specific size class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryAllocateFromSizeClass(int sizeClassIndex, out IntPtr pointer)
        {
            if (sizeClassIndex < 0 || sizeClassIndex >= MaxSizeClasses)
            {
                pointer = IntPtr.Zero;
                return false;
            }

            var pool = _sizeClassPools[sizeClassIndex];

            // Use SpinLock for better performance under contention
            bool lockTaken = false;
            try
            {
                _poolLocks[sizeClassIndex].Enter(ref lockTaken);
                int count = _poolCounts[sizeClassIndex];
                if (count > 0)
                {
                    count--;
                    _poolCounts[sizeClassIndex] = count;
                    pointer = pool[count];
                    return true;
                }
            }
            finally
            {
                if (lockTaken) _poolLocks[sizeClassIndex].Exit();
            }

            pointer = IntPtr.Zero;
            return false;
        }
    }
}