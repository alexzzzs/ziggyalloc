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

        // Thresholds based on benchmark results
        private const int BYTE_THRESHOLD = 1024;
        private const int INT_THRESHOLD = 512;
        private const int DOUBLE_THRESHOLD = 128;
        private const int STRUCT_THRESHOLD = 64;

        /// <summary>
        /// Gets a value indicating that this allocator supports individual memory deallocation.
        /// </summary>
        public bool SupportsIndividualDeallocation => _unmanagedAllocator.SupportsIndividualDeallocation;

        /// <summary>
        /// Gets the total number of bytes currently allocated by this allocator.
        /// </summary>
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
                return new UnmanagedBuffer<T>(null, 0, this);
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
                
                // Zero-initialize if requested (arrays are zero-initialized by default in .NET)
                if (zeroMemory && !IsZeroInitialized<T>())
                {
                    Array.Clear(managedArray, 0, managedArray.Length);
                }
                
                // Pin the array to get a stable pointer
                var handle = GCHandle.Alloc(managedArray, GCHandleType.Pinned);
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
                int threshold = Math.Max(32, 1024 / elementSize);
                return elementCount > threshold;
            }
        }

        /// <summary>
        /// Checks if a type is zero-initialized by default.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsZeroInitialized<T>() where T : unmanaged
        {
            // Arrays in .NET are zero-initialized by default
            return true;
        }

        /// <summary>
        /// Frees previously allocated unmanaged memory.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to free</param>
        public void Free(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
                return;

            if (_disposed)
                throw new ObjectDisposedException(nameof(HybridAllocator));

            // Delegate to the unmanaged allocator for unmanaged memory
            _unmanagedAllocator.Free(pointer);
            // Note: Managed arrays are automatically cleaned up by the GC when the UnmanagedBuffer is disposed
        }

        /// <summary>
        /// Disposes the HybridAllocator and releases any unmanaged resources.
        /// </summary>
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