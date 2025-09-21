using System;

namespace ZiggyAlloc
{
    /// <summary>
    /// Provides automatic memory management using RAII (Resource Acquisition Is Initialization) pattern.
    /// </summary>
    /// <typeparam name="T">The unmanaged type being managed</typeparam>
    /// <remarks>
    /// This ref struct automatically frees allocated memory when it goes out of scope or is explicitly disposed.
    /// It's designed to be used with 'using' statements to ensure deterministic cleanup.
    /// 
    /// Being a ref struct, it cannot be boxed, stored in fields of reference types, or used across await boundaries.
    /// This ensures stack-only allocation and prevents accidental memory leaks from forgotten cleanup.
    /// </remarks>
    public readonly ref struct AutoFreeMemory<T> : IDisposable where T : unmanaged
    {
        private readonly IUnmanagedMemoryAllocator _allocator;

        /// <summary>
        /// Gets the buffer representing the allocated memory.
        /// </summary>
        /// <remarks>
        /// This buffer remains valid until the AutoFreeMemory instance is disposed.
        /// Do not use this buffer after disposal as it will point to freed memory.
        /// </remarks>
        public readonly UnmanagedBuffer<T> Buffer { get; }

        /// <summary>
        /// Gets a reference to the value at the allocated memory location.
        /// </summary>
        /// <remarks>
        /// For single-element allocations, this provides convenient access to the value.
        /// For multi-element allocations, this refers to the first element.
        /// The reference remains valid until the AutoFreeMemory instance is disposed.
        /// </remarks>
        public ref T Value => ref Buffer[0];

        /// <summary>
        /// Initializes a new AutoFreeMemory instance with allocated memory.
        /// </summary>
        /// <param name="allocator">The allocator to use for memory operations</param>
        /// <param name="elementCount">The number of elements to allocate</param>
        /// <param name="zeroMemory">Whether to zero-initialize the allocated memory</param>
        /// <exception cref="ArgumentNullException">Thrown when allocator is null</exception>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when elementCount is less than 0</exception>
        internal AutoFreeMemory(IUnmanagedMemoryAllocator allocator, int elementCount, bool zeroMemory)
        {
            _allocator = allocator ?? throw new ArgumentNullException(nameof(allocator));
            Buffer = allocator.Allocate<T>(elementCount, zeroMemory);
        }

        /// <summary>
        /// Frees the allocated memory.
        /// </summary>
        /// <remarks>
        /// This method is called automatically when the AutoFreeMemory instance goes out of scope
        /// or when used in a 'using' statement. After disposal, the Buffer and Value properties
        /// should not be accessed as they will refer to freed memory.
        /// </remarks>
        public void Dispose()
        {
            if (Buffer.IsValid && Buffer.RawPointer != IntPtr.Zero)
            {
                _allocator.Free(Buffer.RawPointer);
            }
        }
    }
}