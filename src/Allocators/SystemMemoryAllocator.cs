using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace ZiggyAlloc
{
    /// <summary>
    /// A memory allocator that uses the system's native memory allocation functions.
    /// </summary>
    public sealed class SystemMemoryAllocator : IUnmanagedMemoryAllocator
    {
        private const int STACK_ALLOC_THRESHOLD = 1024; // 1KB
        private long _totalAllocatedBytes = 0;

        /// <summary>
        /// Gets a value indicating that this allocator supports individual memory deallocation.
        /// </summary>
        public bool SupportsIndividualDeallocation => true;

        /// <summary>
        /// Gets the total number of bytes currently allocated by this allocator.
        /// </summary>
        public long TotalAllocatedBytes => Interlocked.Read(ref _totalAllocatedBytes);

        /// <summary>
        /// Allocates unmanaged memory for the specified number of elements.
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

            // Calculate total size with overflow checking
            long totalSize;
            try
            {
                totalSize = (long)elementCount * sizeof(T);
            }
            catch (OverflowException)
            {
                throw new OverflowException($"Allocation size overflow: {elementCount} elements of {sizeof(T)} bytes each");
            }

            if (totalSize > int.MaxValue)
                throw new OutOfMemoryException($"Allocation too large: {totalSize} bytes exceeds maximum allocation size");

            IntPtr pointer = (IntPtr)NativeMemory.Alloc((nuint)totalSize);

            if (pointer == IntPtr.Zero)
                throw new OutOfMemoryException($"Failed to allocate {totalSize} bytes for {elementCount} elements of type {typeof(T).Name}");

            // Zero the memory if requested
            if (zeroMemory)
            {
                var byteSpan = new Span<byte>((void*)pointer, (int)totalSize);
                byteSpan.Clear();
            }

            // Update allocation tracking
            Interlocked.Add(ref _totalAllocatedBytes, totalSize);

            return new UnmanagedBuffer<T>((T*)pointer, elementCount, this);
        }

        /// <summary>
        /// Frees previously allocated unmanaged memory.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to free</param>
        public unsafe void Free(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
                return;

            NativeMemory.Free((void*)pointer);
        }

        /// <summary>
        /// Creates a buffer that wraps existing memory without owning it.
        /// </summary>
        /// <typeparam name="T">The unmanaged type the memory represents</typeparam>
        /// <param name="pointer">Pointer to the existing memory</param>
        /// <param name="elementCount">Number of elements the memory can hold</param>
        /// <returns>A buffer that wraps the existing memory</returns>
        public static unsafe UnmanagedBuffer<T> WrapExisting<T>(IntPtr pointer, int elementCount) where T : unmanaged
        {
            if (elementCount < 0)
                throw new ArgumentOutOfRangeException(nameof(elementCount), "Element count cannot be negative");

            return new UnmanagedBuffer<T>((T*)pointer, elementCount);
        }

        /// <summary>
        /// Creates a buffer that wraps a span's memory without owning it.
        /// </summary>
        /// <typeparam name="T">The unmanaged type the span contains</typeparam>
        /// <param name="span">The span to wrap</param>
        /// <returns>A buffer that wraps the span's memory</returns>
        public static unsafe UnmanagedBuffer<T> WrapSpan<T>(Span<T> span) where T : unmanaged
        {
            fixed (T* pointer = span)
            {
                return new UnmanagedBuffer<T>(pointer, span.Length);
            }
        }
    }
}