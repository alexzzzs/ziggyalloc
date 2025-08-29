using System;

namespace ZiggyAlloc
{
    /// <summary>
    /// Defines the contract for unmanaged memory allocators.
    /// </summary>
    public interface IUnmanagedMemoryAllocator
    {
        /// <summary>
        /// Allocates unmanaged memory for the specified number of elements.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="elementCount">The number of elements to allocate space for</param>
        /// <param name="zeroMemory">Whether to zero-initialize the allocated memory</param>
        /// <returns>A buffer representing the allocated memory</returns>
        UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false) where T : unmanaged;

        /// <summary>
        /// Frees previously allocated unmanaged memory.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to free</param>
        void Free(IntPtr pointer);

        /// <summary>
        /// Gets a value indicating whether this allocator supports individual memory deallocation.
        /// </summary>
        bool SupportsIndividualDeallocation { get; }

        /// <summary>
        /// Gets the total number of bytes currently allocated by this allocator.
        /// </summary>
        long TotalAllocatedBytes { get; }
    }
}