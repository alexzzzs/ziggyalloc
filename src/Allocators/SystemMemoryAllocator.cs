using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace ZiggyAlloc
{
    /// <summary>
    /// A memory allocator that uses the system's native memory allocation functions.
    /// </summary>
    public sealed class SystemMemoryAllocator : IUnmanagedMemoryAllocator, IDisposable
    {
        private const string DEBUG_EXCEPTION_DISPOSAL = "Exception during disposal in SystemMemoryAllocator.Dispose";
        private const string DEBUG_EXCEPTION_MEMORY_CLEANUP = "Exception during memory cleanup in SystemMemoryAllocator.Free";

        private long _totalAllocatedBytes = 0;

        /// <summary>
        /// Gets a value indicating that this allocator supports individual memory deallocation.
        /// </summary>
        public bool SupportsIndividualDeallocation => true;

        /// <summary>
        /// Gets the total number of bytes allocated by this allocator.
        /// </summary>
        /// <remarks>
        /// This value tracks cumulative allocations but does not decrement when memory is freed.
        /// This represents the total bytes ever allocated, not currently allocated bytes.
        /// </remarks>
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

            // Calculate element size once for performance
            int elementSize = sizeof(T);

            // Calculate total size with overflow checking
            long totalSize;
            try
            {
                totalSize = (long)elementCount * elementSize;
            }
            catch (OverflowException)
            {
                throw new OverflowException($"Allocation size overflow: {elementCount} elements of {elementSize} bytes each");
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
        /// <remarks>
        /// Note: TotalAllocatedBytes only tracks allocations and does not decrement on deallocation
        /// as managed memory cleanup timing is non-deterministic.
        /// </remarks>
        public unsafe void Free(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
                return;

            try
            {
                NativeMemory.Free((void*)pointer);
            }
            catch (Exception ex)
            {
                // Log exception in debug builds instead of silently ignoring
                #if DEBUG
                System.Diagnostics.Debug.WriteLine($"{DEBUG_EXCEPTION_MEMORY_CLEANUP}: {ex}");
                #endif
                throw; // Re-throw to maintain original behavior for compatibility
            }
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

            if (pointer == IntPtr.Zero)
                throw new ArgumentNullException(nameof(pointer), "Pointer cannot be null");

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
            if (span.Length < 0)
                throw new ArgumentOutOfRangeException(nameof(span), "Span length cannot be negative");

            fixed (T* pointer = span)
            {
                return new UnmanagedBuffer<T>(pointer, span.Length);
            }
        }

        /// <summary>
        /// Disposes the allocator. SystemMemoryAllocator doesn't hold any resources that need disposal.
        /// </summary>
        public void Dispose()
        {
            // SystemMemoryAllocator doesn't hold any resources that need disposal
        }
    }
}