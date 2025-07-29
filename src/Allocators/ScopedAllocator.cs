using System;
using System.Collections.Generic;

namespace ZiggyAlloc
{
    /// <summary>
    /// A scoped memory allocator that automatically frees all allocated memory when disposed.
    /// </summary>
    /// <remarks>
    /// This allocator is ideal for scenarios where you want automatic cleanup of all allocations
    /// at the end of a scope. Individual allocations cannot be freed - all memory is freed
    /// when the allocator is disposed. This follows the RAII (Resource Acquisition Is Initialization)
    /// pattern and is similar to arena allocators.
    /// 
    /// This allocator is NOT thread-safe. Use separate instances for different threads.
    /// </remarks>
    public sealed class ScopedMemoryAllocator : IMemoryAllocator, IDisposable
    {
        private readonly ManualMemoryAllocator _backingAllocator = new();
        private readonly List<IntPtr> _allocatedPointers = new();
        private bool _disposed = false;

        /// <summary>
        /// Allocates memory for one or more instances of the specified unmanaged type.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="count">The number of instances to allocate space for</param>
        /// <param name="zeroed">Whether to zero-initialize the allocated memory</param>
        /// <returns>A pointer to the allocated memory</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when count is less than 1</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the allocator has been disposed</exception>
        public Pointer<T> Allocate<T>(int count = 1, bool zeroed = false) where T : unmanaged
        {
            ThrowIfDisposed();
            
            var pointer = _backingAllocator.Allocate<T>(count, zeroed);
            _allocatedPointers.Add(pointer.Raw);
            return pointer;
        }

        /// <summary>
        /// Individual memory deallocation is not supported in scoped allocators.
        /// All memory is automatically freed when the allocator is disposed.
        /// </summary>
        /// <param name="pointer">The pointer to free (ignored)</param>
        /// <exception cref="NotSupportedException">Always thrown as individual deallocation is not supported</exception>
        public void Free(IntPtr pointer) =>
            throw new NotSupportedException(
                "Individual memory deallocation is not supported in ScopedMemoryAllocator. " +
                "All memory is automatically freed when the allocator is disposed.");

        /// <summary>
        /// Allocates memory for a slice (array) of the specified unmanaged type.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="count">The number of elements in the slice</param>
        /// <param name="zeroed">Whether to zero-initialize the allocated memory</param>
        /// <returns>A slice representing the allocated memory</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when count is less than 0</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the allocator has been disposed</exception>
        public Slice<T> AllocateSlice<T>(int count, bool zeroed = false) where T : unmanaged
        {
            ThrowIfDisposed();
            
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative");

            if (count == 0)
                return new Slice<T>(new Pointer<T>(IntPtr.Zero), 0);

            return new Slice<T>(Allocate<T>(count, zeroed), count);
        }

        /// <summary>
        /// Frees all allocated memory and disposes the allocator.
        /// </summary>
        /// <remarks>
        /// Memory is freed in reverse order of allocation (LIFO - Last In, First Out).
        /// After disposal, the allocator cannot be used for further allocations.
        /// </remarks>
        public void Dispose()
        {
            if (!_disposed)
            {
                // Free all allocations in reverse order (LIFO)
                for (int i = _allocatedPointers.Count - 1; i >= 0; i--)
                {
                    _backingAllocator.Free(_allocatedPointers[i]);
                }
                
                _allocatedPointers.Clear();
                _disposed = true;
            }
        }

        /// <summary>
        /// Throws ObjectDisposedException if the allocator has been disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ScopedMemoryAllocator));
        }
    }
}