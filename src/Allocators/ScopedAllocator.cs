using System;
using System.Collections.Generic;
using System.Threading;

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
    public sealed class ScopedMemoryAllocator : IUnmanagedMemoryAllocator, IDisposable
    {
        private readonly SystemMemoryAllocator _backingAllocator = new();
        private readonly List<IntPtr> _allocatedPointers = new();
        private readonly object _lock = new();
        private bool _disposed = false;

        /// <summary>
        /// Gets a value indicating whether this allocator supports individual memory deallocation.
        /// </summary>
        /// <remarks>
        /// Scoped allocators do not support individual deallocation - all memory is freed when disposed.
        /// </remarks>
        public bool SupportsIndividualDeallocation
        {
            get
            {
                ThrowIfDisposed();
                return false;
            }
        }

        /// <summary>
        /// Gets the total number of bytes currently allocated by this allocator.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the allocator has been disposed</exception>
        public long TotalAllocatedBytes
        {
            get
            {
                ThrowIfDisposed();
                return _backingAllocator.TotalAllocatedBytes;
            }
        }

        /// <summary>
        /// Allocates unmanaged memory for the specified number of elements.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="elementCount">The number of elements to allocate space for</param>
        /// <param name="zeroMemory">Whether to zero-initialize the allocated memory</param>
        /// <returns>A buffer representing the allocated memory</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when elementCount is less than 0</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the allocator has been disposed</exception>
        public UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false) where T : unmanaged
        {
            ThrowIfDisposed();

            var backingBuffer = _backingAllocator.Allocate<T>(elementCount, zeroMemory);
            lock (_lock)
            {
                _allocatedPointers.Add(backingBuffer.RawPointer);
            }

            // Create a buffer that doesn't own the memory (won't call Free on dispose)
            // The ScopedMemoryAllocator will handle freeing all memory when it's disposed
            unsafe
            {
                return new UnmanagedBuffer<T>((T*)backingBuffer.RawPointer, backingBuffer.Length);
            }
        }

        /// <summary>
        /// Individual memory deallocation is not supported in scoped allocators.
        /// All memory is automatically freed when the allocator is disposed.
        /// </summary>
        /// <param name="pointer">The pointer to free (ignored)</param>
        /// <exception cref="NotSupportedException">Always thrown as individual deallocation is not supported</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the allocator has been disposed</exception>
        public void Free(IntPtr pointer)
        {
            ThrowIfDisposed();
            throw new NotSupportedException(
                "Individual memory deallocation is not supported in ScopedMemoryAllocator. " +
                "All memory is automatically freed when the allocator is disposed.");
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
            lock (_lock)
            {
                if (Volatile.Read(ref _disposed))
                {
                    return;
                }
                Volatile.Write(ref _disposed, true);

                // Free all allocations in reverse order (LIFO)
                for (int i = _allocatedPointers.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        _backingAllocator.Free(_allocatedPointers[i]);
                    }
                    catch
                    {
                        // Ignore exceptions during cleanup
                    }
                }

                _allocatedPointers.Clear();
            }
        }

        /// <summary>
        /// Throws ObjectDisposedException if the allocator has been disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _disposed))
                throw new ObjectDisposedException(nameof(ScopedMemoryAllocator));
        }
    }
}