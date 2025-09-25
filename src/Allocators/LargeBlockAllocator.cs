using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace ZiggyAlloc
{
    /// <summary>
    /// A specialized allocator optimized for large memory blocks (>64KB).
    /// Uses direct allocation strategies and pooling to minimize overhead.
    /// </summary>
    public sealed unsafe class LargeBlockAllocator : IUnmanagedMemoryAllocator, IDisposable
    {
        private readonly IUnmanagedMemoryAllocator _baseAllocator;
        private readonly ConcurrentDictionary<int, LargeBlockPool> _largeBlockPools;
        private readonly int _largeBlockThreshold;
        private long _totalAllocatedBytes;
        private bool _disposed = false;

        // Constants for large block optimization
        private const int DEFAULT_LARGE_BLOCK_THRESHOLD = 64 * 1024; // 64KB
        private const int MAX_POOL_SIZE = 8; // Maximum blocks to keep in pool per size class
        private const int ALIGNMENT = 4096; // 4KB alignment for large blocks

        /// <summary>
        /// Gets a value indicating that this allocator supports individual memory deallocation.
        /// </summary>
        public bool SupportsIndividualDeallocation => true;

        /// <summary>
        /// Gets the total number of bytes allocated by this allocator.
        /// </summary>
        public long TotalAllocatedBytes => Interlocked.Read(ref _totalAllocatedBytes);

        /// <summary>
        /// Initializes a new instance of the LargeBlockAllocator class.
        /// </summary>
        /// <param name="baseAllocator">The underlying allocator to use for actual memory allocation</param>
        /// <param name="largeBlockThreshold">The minimum size in bytes to consider as a large block (default: 64KB)</param>
        public LargeBlockAllocator(IUnmanagedMemoryAllocator baseAllocator, int largeBlockThreshold = DEFAULT_LARGE_BLOCK_THRESHOLD)
        {
            _baseAllocator = baseAllocator ?? throw new ArgumentNullException(nameof(baseAllocator));
            _largeBlockThreshold = Math.Max(largeBlockThreshold, 4096); // Minimum 4KB
            _largeBlockPools = new ConcurrentDictionary<int, LargeBlockPool>();
        }

        /// <summary>
        /// Allocates memory optimized for large blocks.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="elementCount">The number of elements to allocate space for</param>
        /// <param name="zeroMemory">Whether to zero-initialize the allocated memory</param>
        /// <returns>A buffer representing the allocated memory</returns>
        public unsafe UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false) where T : unmanaged
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(LargeBlockAllocator));

            if (elementCount < 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount), "Element count cannot be negative");

            if (elementCount == 0)
                return new UnmanagedBuffer<T>(null, 0, this);

            int elementSize = sizeof(T);
            long totalSize = (long)elementCount * elementSize;

            // Only handle large allocations
            if (totalSize < _largeBlockThreshold)
            {
                // Delegate small allocations to base allocator
                var buffer = _baseAllocator.Allocate<T>(elementCount, zeroMemory);
                Interlocked.Add(ref _totalAllocatedBytes, totalSize);
                return new UnmanagedBuffer<T>((T*)buffer.RawPointer, buffer.Length, _baseAllocator);
            }

            // Use optimized large block allocation
            var largeBuffer = AllocateLargeBlock<T>(elementCount, elementSize, totalSize, zeroMemory);

            Interlocked.Add(ref _totalAllocatedBytes, totalSize);
            return new UnmanagedBuffer<T>((T*)largeBuffer.RawPointer, largeBuffer.Length, this);
        }

        /// <summary>
        /// Optimized allocation for large blocks.
        /// </summary>
        private unsafe UnmanagedBuffer<T> AllocateLargeBlock<T>(int elementCount, int elementSize, long totalSize, bool zeroMemory) where T : unmanaged
        {
            // Round up to alignment boundary for better performance
            int alignedSize = (int)((totalSize + ALIGNMENT - 1) & ~(ALIGNMENT - 1));

            // Try to get from pool first
            var pool = _largeBlockPools.GetOrAdd(alignedSize, _ => new LargeBlockPool(_baseAllocator, alignedSize));
            if (pool.TryAllocate(out var pointer))
            {
                if (zeroMemory)
                {
                    // Use optimized clearing for large blocks
                    ClearLargeMemory((void*)pointer, alignedSize);
                }
                return new UnmanagedBuffer<T>((T*)pointer, elementCount, this);
            }

            // Allocate new large block with alignment
            IntPtr alignedPointer = AllocateAligned(alignedSize);

            if (zeroMemory)
            {
                ClearLargeMemory((void*)alignedPointer, alignedSize);
            }

            return new UnmanagedBuffer<T>((T*)alignedPointer, elementCount, this);
        }

        /// <summary>
        /// Allocates memory aligned to the specified boundary.
        /// </summary>
        private unsafe IntPtr AllocateAligned(int size)
        {
            // Allocate extra space for alignment
            int extraSpace = ALIGNMENT + sizeof(IntPtr);
            IntPtr rawPointer = (IntPtr)NativeMemory.Alloc((nuint)(size + extraSpace));

            if (rawPointer == IntPtr.Zero)
                throw new OutOfMemoryException($"Failed to allocate {size + extraSpace} bytes");

            // Calculate aligned address
            nuint rawAddress = (nuint)rawPointer;
            nuint alignedAddress = (rawAddress + (nuint)ALIGNMENT + (nuint)sizeof(IntPtr) - 1) & ~((nuint)ALIGNMENT - 1);

            // Store the original pointer before the aligned address for later freeing
            IntPtr* header = (IntPtr*)(alignedAddress - (nuint)sizeof(IntPtr));
            *header = rawPointer;

            return (IntPtr)alignedAddress;
        }

        /// <summary>
        /// Optimized memory clearing for large blocks using platform-safe operations.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void ClearLargeMemory(void* pointer, int size)
        {
            // Use platform-safe memory clearing
            if (size <= 0) return;

            // For large blocks, use the most efficient method available
            byte* bytePtr = (byte*)pointer;

            // Use SIMD if available and supported, otherwise use standard clearing
            if (SimdMemoryOperations.IsSimdSupported && size >= 16)
            {
                // Use standard SIMD operations (not AVX2-specific)
                var span = new Span<byte>(pointer, size);
                span.Clear();
            }
            else
            {
                // Fallback to standard byte-by-byte clearing for large blocks
                // Use unrolled loop for better performance
                int unrolledLength = size / 8 * 8;
                for (int i = 0; i < unrolledLength; i += 8)
                {
                    bytePtr[i] = 0;
                    bytePtr[i + 1] = 0;
                    bytePtr[i + 2] = 0;
                    bytePtr[i + 3] = 0;
                    bytePtr[i + 4] = 0;
                    bytePtr[i + 5] = 0;
                    bytePtr[i + 6] = 0;
                    bytePtr[i + 7] = 0;
                }

                // Handle remaining bytes
                for (int i = unrolledLength; i < size; i++)
                {
                    bytePtr[i] = 0;
                }
            }
        }

        /// <summary>
        /// Frees previously allocated memory.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to free</param>
        public unsafe void Free(IntPtr pointer)
        {
            if (_disposed || pointer == IntPtr.Zero)
                return;

            try
            {
                // Check if this is an aligned allocation
                IntPtr* header = (IntPtr*)((nuint)pointer - (nuint)sizeof(IntPtr));
                IntPtr originalPointer = *header;

                if (originalPointer != IntPtr.Zero)
                {
                    // This was an aligned allocation, free the original pointer
                    NativeMemory.Free((void*)originalPointer);
                }
                else
                {
                    // This wasn't an aligned allocation, delegate to base allocator
                    _baseAllocator.Free(pointer);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"Exception during memory cleanup in LargeBlockAllocator.Free: {ex}");
#endif
                throw;
            }
        }

        /// <summary>
        /// Disposes the allocator and releases all pooled memory.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                try
                {
                    foreach (var pool in _largeBlockPools.Values)
                    {
                        pool?.Dispose();
                    }
                    _largeBlockPools.Clear();
                }
                catch (Exception ex)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"Exception during disposal in LargeBlockAllocator.Dispose: {ex}");
#endif
                    throw;
                }
            }
        }

        /// <summary>
        /// Pool for large blocks of a specific size.
        /// </summary>
        private unsafe class LargeBlockPool : IDisposable
        {
            private readonly IUnmanagedMemoryAllocator _allocator;
            private readonly int _blockSize;
            private readonly ConcurrentStack<IntPtr> _pool;
            private int _poolCount;
            private bool _disposed = false;

            public LargeBlockPool(IUnmanagedMemoryAllocator allocator, int blockSize)
            {
                _allocator = allocator;
                _blockSize = blockSize;
                _pool = new ConcurrentStack<IntPtr>();
                _poolCount = 0;
            }

            public bool TryAllocate(out IntPtr pointer)
            {
                if (_disposed)
                {
                    pointer = IntPtr.Zero;
                    return false;
                }

                // Try to get from pool
                if (_pool.TryPop(out pointer))
                {
                    Interlocked.Decrement(ref _poolCount);
                    return true;
                }

                pointer = IntPtr.Zero;
                return false;
            }

            public void Return(IntPtr pointer)
            {
                if (_disposed || pointer == IntPtr.Zero)
                    return;

                // Only pool if we haven't exceeded the maximum pool size
                if (Interlocked.Increment(ref _poolCount) <= MAX_POOL_SIZE)
                {
                    _pool.Push(pointer);
                }
                else
                {
                    // Pool is full, just free the memory
                    Interlocked.Decrement(ref _poolCount);
                    try
                    {
                        // Free the aligned allocation
                        IntPtr* header = (IntPtr*)((nuint)pointer - (nuint)sizeof(IntPtr));
                        IntPtr originalPointer = *header;
                        if (originalPointer != IntPtr.Zero)
                        {
                            NativeMemory.Free((void*)originalPointer);
                        }
                    }
                    catch
                    {
                        // If we can't free it properly, at least decrement the counter
                        Interlocked.Decrement(ref _poolCount);
                    }
                }
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    try
                    {
                        // Free all pooled blocks
                        while (_pool.TryPop(out var pointer))
                        {
                            IntPtr* header = (IntPtr*)((nuint)pointer - (nuint)sizeof(IntPtr));
                            IntPtr originalPointer = *header;
                            if (originalPointer != IntPtr.Zero)
                            {
                                NativeMemory.Free((void*)originalPointer);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"Exception during disposal in LargeBlockPool.Dispose: {ex}");
#endif
                        throw;
                    }
                }
            }
        }
    }
}