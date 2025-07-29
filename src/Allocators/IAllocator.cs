using System;

namespace ZiggyAlloc
{
    /// <summary>
    /// Defines the contract for memory allocators in ZiggyAlloc.
    /// Allocators are responsible for managing unmanaged memory allocation and deallocation.
    /// </summary>
    /// <remarks>
    /// All allocators must implement this interface to provide consistent memory management
    /// across different allocation strategies (manual, scoped, debug, etc.).
    /// </remarks>
    public interface IMemoryAllocator
    {
        /// <summary>
        /// Allocates memory for one or more instances of the specified unmanaged type.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="count">The number of instances to allocate space for (default: 1)</param>
        /// <param name="zeroed">Whether to zero-initialize the allocated memory (default: false)</param>
        /// <returns>A pointer to the allocated memory</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when count is less than 1</exception>
        Pointer<T> Allocate<T>(int count = 1, bool zeroed = false) where T : unmanaged;

        /// <summary>
        /// Frees previously allocated memory.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to free</param>
        /// <remarks>
        /// Passing IntPtr.Zero is safe and will be ignored.
        /// Freeing the same pointer twice may result in undefined behavior.
        /// </remarks>
        void Free(IntPtr pointer);

        /// <summary>
        /// Allocates memory for a slice (array) of the specified unmanaged type.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="count">The number of elements in the slice</param>
        /// <param name="zeroed">Whether to zero-initialize the allocated memory (default: false)</param>
        /// <returns>A slice representing the allocated memory</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when count is less than 0</exception>
        Slice<T> AllocateSlice<T>(int count, bool zeroed = false) where T : unmanaged;
    }
}