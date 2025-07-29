using System;

namespace ZiggyAlloc
{
    /// <summary>
    /// Defines the contract for unmanaged memory allocators.
    /// </summary>
    /// <remarks>
    /// Allocators are responsible for managing unmanaged memory allocation and deallocation.
    /// They provide the foundation for high-performance memory management scenarios where
    /// garbage collection overhead needs to be avoided.
    /// 
    /// Key use cases:
    /// - Interop with native libraries requiring contiguous memory
    /// - Large buffer allocations to reduce GC pressure
    /// - Performance-critical scenarios in games, scientific computing, etc.
    /// - Custom memory layout patterns (struct-of-arrays)
    /// </remarks>
    public interface IUnmanagedMemoryAllocator
    {
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
        /// <remarks>
        /// The returned buffer owns the allocated memory and will free it when disposed.
        /// For zero-length allocations, a valid but empty buffer is returned.
        /// </remarks>
        UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false) where T : unmanaged;

        /// <summary>
        /// Frees previously allocated unmanaged memory.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to free</param>
        /// <remarks>
        /// Passing IntPtr.Zero is safe and will be ignored.
        /// Freeing the same pointer twice may result in undefined behavior.
        /// This method is primarily used internally by UnmanagedBuffer disposal.
        /// </remarks>
        void Free(IntPtr pointer);

        /// <summary>
        /// Gets a value indicating whether this allocator supports individual memory deallocation.
        /// </summary>
        /// <remarks>
        /// Some allocators (like arena allocators) may not support freeing individual allocations.
        /// When false, calling Free() may throw NotSupportedException.
        /// </remarks>
        bool SupportsIndividualDeallocation { get; }

        /// <summary>
        /// Gets the total number of bytes currently allocated by this allocator.
        /// </summary>
        /// <remarks>
        /// Returns -1 if the allocator doesn't track memory usage.
        /// This is useful for monitoring memory consumption and detecting leaks.
        /// </remarks>
        long TotalAllocatedBytes { get; }
    }
}