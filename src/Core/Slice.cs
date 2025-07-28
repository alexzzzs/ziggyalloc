using System;
using System.Runtime.CompilerServices;

namespace ZiggyAlloc
{
    public readonly ref struct Slice<T> where T : unmanaged
    {
        public readonly Pointer<T> Ptr;
        public readonly int Length;

        public Slice(Pointer<T> ptr, int length)
        {
            Ptr = ptr;
            Length = length;
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)Length)
                    throw new IndexOutOfRangeException();
                return ref Ptr[index];
            }
        }

        public Span<T> AsSpan() => Ptr.AsSpan(Length);
        public bool IsEmpty => Length == 0;

        public static implicit operator Span<T>(Slice<T> s) => s.AsSpan();
        public static implicit operator ReadOnlySpan<T>(Slice<T> s) => s.AsSpan();
    }
}