using System;

namespace ZiggyAlloc
{
    /// <summary>
    /// Defines the contract for unmanaged memory allocators.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface provide different strategies for unmanaged memory allocation:
    /// <list type="bullet">
    /// <item><description><see cref="SystemMemoryAllocator"/> - Direct system memory allocation</description></item>
    /// <item><description><see cref="ScopedMemoryAllocator"/> - Arena-style allocator with bulk cleanup</description></item>
    /// <item><description><see cref="DebugMemoryAllocator"/> - Allocator with leak detection for development</description></item>
    /// <item><description><see cref="UnmanagedMemoryPool"/> - Pool-based allocator for frequent allocations</description></item>
    /// <item><description><see cref="HybridAllocator"/> - Intelligent allocator choosing strategy based on size/type</description></item>
    /// <item><description><see cref="SlabAllocator"/> - Slab-based allocator for high-frequency small allocations</description></item>
    /// </list>
    /// </remarks>
    public interface IUnmanagedMemoryAllocator
    {
        /// <summary>
        /// Allocates unmanaged memory for the specified number of elements.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for. Must be a value type that contains no references to managed objects.</typeparam>
        /// <param name="elementCount">The number of elements to allocate space for. Must be non-negative.</param>
        /// <param name="zeroMemory">Whether to zero-initialize the allocated memory. When true, all bytes in the allocated memory are set to zero.</param>
        /// <returns>A buffer representing the allocated memory that implements IDisposable for automatic cleanup.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when elementCount is negative.</exception>
        /// <exception cref="OutOfMemoryException">Thrown when the requested allocation size exceeds system limits.</exception>
        /// <exception cref="OverflowException">Thrown when the total allocation size overflows.</exception>
        /// <example>
        /// <code>
        /// var allocator = new SystemMemoryAllocator();
        /// using var buffer = allocator.Allocate&lt;int&gt;(1000);
        /// buffer[0] = 42;
        /// Span&lt;int&gt; span = buffer; // Zero-cost conversion to Span
        /// </code>
        /// </example>
        UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false) where T : unmanaged;

        /// <summary>
        /// Frees previously allocated unmanaged memory.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to free. Can be IntPtr.Zero, in which case the method does nothing.</param>
        /// <remarks>
        /// Not all allocators support individual deallocation. For example, <see cref="ScopedMemoryAllocator"/> 
        /// manages memory in bulk and throws <see cref="NotSupportedException"/> when this method is called.
        /// </remarks>
        /// <exception cref="NotSupportedException">Thrown by allocators that don't support individual deallocation.</exception>
        void Free(IntPtr pointer);

        /// <summary>
        /// Gets a value indicating whether this allocator supports individual memory deallocation.
        /// </summary>
        /// <value>
        /// True if the allocator supports calling <see cref="Free(IntPtr)"/> on individual allocations, false otherwise.
        /// </value>
        /// <remarks>
        /// Allocators like <see cref="SystemMemoryAllocator"/> and <see cref="UnmanagedMemoryPool"/> return true,
        /// while <see cref="ScopedMemoryAllocator"/> returns false as it manages memory in bulk.
        /// </remarks>
        bool SupportsIndividualDeallocation { get; }

        /// <summary>
        /// Gets the total number of bytes currently allocated by this allocator.
        /// </summary>
        /// <value>
        /// The cumulative size in bytes of all allocations made by this allocator instance.
        /// Note that for pooled allocators, this represents the total bytes ever allocated, not currently allocated.
        /// </value>
        long TotalAllocatedBytes { get; }
    }
}