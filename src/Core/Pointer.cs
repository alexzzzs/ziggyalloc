using System;
using System.Runtime.CompilerServices;

namespace ZiggyAlloc
{
    /// <summary>
    /// A type-safe wrapper around an unmanaged pointer.
    /// Note: No implicit conversion to Span&lt;T&gt; is provided, as it would require
    /// assuming a length of 1, which is unsafe for pointers to arrays.
    /// Use the explicit .AsSpan(count) method for clarity and safety.
    /// </summary>
    /// <typeparam name="T">The unmanaged type this pointer points to</typeparam>
    public readonly ref struct Pointer<T> where T : unmanaged
    {
        /// <summary>The raw pointer value</summary>
        public readonly IntPtr Ptr;

        /// <summary>Creates a new Pointer wrapper around the given raw pointer</summary>
        public Pointer(IntPtr ptr) => Ptr = ptr;

        /// <summary>Gets a reference to the value at this pointer location</summary>
        public ref T Value
        {
            get
            {
                unsafe { return ref Unsafe.AsRef<T>((void*)Ptr); }
            }
        }

        /// <summary>Gets a reference to the value at the specified index from this pointer</summary>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe { return ref ((T*)Ptr)[index]; }
            }
        }

        /// <summary>Creates a Span view over the memory starting at this pointer</summary>
        public Span<T> AsSpan(int count)
        {
            unsafe { return new Span<T>((void*)Ptr, count); }
        }

        /// <summary>Gets the raw pointer value</summary>
        public IntPtr Raw => Ptr;
        
        /// <summary>Returns true if this pointer is null</summary>
        public bool IsNull => Ptr == IntPtr.Zero;

        /// <summary>Implicitly converts to IntPtr</summary>
        public static implicit operator IntPtr(Pointer<T> p) => p.Ptr;
    }
}