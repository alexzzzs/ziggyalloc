using System;
using System.Runtime.InteropServices;

namespace ZiggyAlloc
{
    /// <summary>
    /// A manual memory allocator that provides direct control over memory allocation and deallocation.
    /// </summary>
    /// <remarks>
    /// This allocator uses the platform's native memory allocation functions (NativeMemory on .NET 6+,
    /// or Marshal.AllocHGlobal on older versions). Memory allocated with this allocator must be
    /// explicitly freed using the Free method to avoid memory leaks.
    /// 
    /// This allocator is thread-safe as it delegates to the underlying platform allocator.
    /// </remarks>
    public class ManualMemoryAllocator : IMemoryAllocator
    {
        /// <summary>
        /// Allocates memory for one or more instances of the specified unmanaged type.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="count">The number of instances to allocate space for</param>
        /// <param name="zeroed">Whether to zero-initialize the allocated memory</param>
        /// <returns>A pointer to the allocated memory</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when count is less than 1</exception>
        public virtual Pointer<T> Allocate<T>(int count = 1, bool zeroed = false) where T : unmanaged
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be at least 1");

            int size;
            unsafe { size = sizeof(T) * count; }
            IntPtr ptr;

#if NET6_0_OR_GREATER
            unsafe { ptr = (IntPtr)NativeMemory.Alloc((nuint)size); }
#else
            ptr = Marshal.AllocHGlobal(size);
#endif

            if (ptr == IntPtr.Zero)
                throw new OutOfMemoryException($"Failed to allocate {size} bytes for {count} instance(s) of {typeof(T).Name}");

            if (zeroed)
            {
                unsafe { new Span<byte>((void*)ptr, size).Clear(); }
            }

            return new Pointer<T>(ptr);
        }

        /// <summary>
        /// Frees previously allocated memory.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to free</param>
        /// <remarks>
        /// Passing IntPtr.Zero is safe and will be ignored.
        /// Freeing the same pointer twice may result in undefined behavior.
        /// </remarks>
        public virtual void Free(IntPtr pointer)
        {
            if (pointer != IntPtr.Zero)
            {
#if NET6_0_OR_GREATER
                unsafe { NativeMemory.Free((void*)pointer); }
#else
                Marshal.FreeHGlobal(pointer);
#endif
            }
        }

        /// <summary>
        /// Allocates memory for a slice (array) of the specified unmanaged type.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to allocate memory for</typeparam>
        /// <param name="count">The number of elements in the slice</param>
        /// <param name="zeroed">Whether to zero-initialize the allocated memory</param>
        /// <returns>A slice representing the allocated memory</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when count is less than 0</exception>
        public Slice<T> AllocateSlice<T>(int count, bool zeroed = false) where T : unmanaged
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative");

            if (count == 0)
                return new Slice<T>(new Pointer<T>(IntPtr.Zero), 0);

            return new Slice<T>(Allocate<T>(count, zeroed), count);
        }
    }
}