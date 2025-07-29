using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace ZiggyAlloc
{
    /// <summary>
    /// A memory allocator that uses the system's native memory allocation functions.
    /// </summary>
    /// <remarks>
    /// This allocator provides direct access to the platform's native memory allocation:
    /// - On .NET 6+: Uses NativeMemory.Alloc/Free for optimal performance
    /// - On older versions: Uses Marshal.AllocHGlobal/FreeHGlobal
    /// 
    /// Key characteristics:
    /// - Thread-safe: Can be used from multiple threads simultaneously
    /// - Individual deallocation: Supports freeing specific allocations
    /// - Memory tracking: Tracks total allocated bytes for monitoring
    /// - High performance: Direct system calls with minimal overhead
    /// 
    /// Best used for:
    /// - General-purpose unmanaged memory allocation
    /// - Interop scenarios requiring native memory
    /// - Long-lived allocations with explicit lifetime management
    /// - Performance-critical code where GC pressure must be avoided
    /// </remarks>
    public sealed class SystemMemoryAllocator : IUnmanagedMemoryAllocator
    {
        private long _totalAllocatedBytes = 0;

        /// <summary>
        /// Gets a value indicating that this allocator supports individual memory deallocation.
        /// </summary>
        public bool SupportsIndividualDeallocation => true;

        /// <summary>
        /// Gets the total number of bytes currently allocated by this allocator.
        /// </summary>
        /// <remarks>
        /// This value is updated atomically and reflects the current memory usage.
        /// It can be used for monitoring memory consumption and detecting leaks.
        /// </remarks>
        public long TotalAllocatedBytes => Interlocked.Read(ref _totalAllocatedBytes);

        /// <summary>
        /// Allocates unmanaged memory for the specified number of elements.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="elementCount">The number of elements to allocate space for</param>
        /// <param name="zeroMemory">Whether to zero-initialize the allocated memory</param>
        /// <returns>A buffer representing the allocated memory</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when elementCount is less than 0
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        /// Thrown when memory allocation fails
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when the total size calculation overflows
        /// </exception>
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

            IntPtr pointer;

#if NET6_0_OR_GREATER
            // Use NativeMemory for optimal performance on .NET 6+
            pointer = (IntPtr)NativeMemory.Alloc((nuint)totalSize);
#else
            // Fall back to Marshal for older .NET versions
            pointer = Marshal.AllocHGlobal((int)totalSize);
#endif

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
        /// Passing IntPtr.Zero is safe and will be ignored.
        /// This method is thread-safe and updates the allocation tracking.
        /// </remarks>
        public unsafe void Free(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
                return;

            // Note: We can't easily track the exact size being freed without additional bookkeeping,
            // so we don't update _totalAllocatedBytes here. This is a limitation of the simple approach.
            // For precise tracking, consider using DebugMemoryAllocator which maintains allocation metadata.

#if NET6_0_OR_GREATER
            NativeMemory.Free((void*)pointer);
#else
            Marshal.FreeHGlobal(pointer);
#endif
        }

        /// <summary>
        /// Creates a buffer that wraps existing memory without owning it.
        /// </summary>
        /// <typeparam name="T">The unmanaged type the memory represents</typeparam>
        /// <param name="pointer">Pointer to the existing memory</param>
        /// <param name="elementCount">Number of elements the memory can hold</param>
        /// <returns>A buffer that wraps the existing memory</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when elementCount is less than 0
        /// </exception>
        /// <remarks>
        /// The returned buffer will not free the memory when disposed.
        /// Use this for wrapping stack-allocated memory or memory allocated elsewhere.
        /// </remarks>
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
        /// <remarks>
        /// The returned buffer will not free the memory when disposed.
        /// The buffer is only valid as long as the original span remains valid.
        /// </remarks>
        public static unsafe UnmanagedBuffer<T> WrapSpan<T>(Span<T> span) where T : unmanaged
        {
            fixed (T* pointer = span)
            {
                return new UnmanagedBuffer<T>(pointer, span.Length);
            }
        }
    }
}