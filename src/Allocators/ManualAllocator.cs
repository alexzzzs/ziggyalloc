using System;
using System.Runtime.InteropServices;

namespace ZiggyAlloc
{
    public class ManualAllocator : IAllocator
    {
        public virtual Pointer<T> Alloc<T>(int count = 1, bool zeroed = false) where T : unmanaged
        {
            int size;
            unsafe { size = sizeof(T) * count; }
            IntPtr ptr;

#if NET6_0_OR_GREATER
            unsafe { ptr = (IntPtr)NativeMemory.Alloc((nuint)size); }
#else
            ptr = Marshal.AllocHGlobal(size);
#endif

            if (ptr == IntPtr.Zero)
                throw new OutOfMemoryException($"Failed to allocate {size} bytes.");

            if (zeroed)
            {
                unsafe { new Span<byte>((void*)ptr, size).Clear(); }
            }

            return new Pointer<T>(ptr);
        }

        public virtual void Free(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
#if NET6_0_OR_GREATER
                unsafe { NativeMemory.Free((void*)ptr); }
#else
                Marshal.FreeHGlobal(ptr);
#endif
            }
        }

        public Slice<T> Slice<T>(int count, bool zeroed = false) where T : unmanaged
            => new(Alloc<T>(count, zeroed), count);
    }
}