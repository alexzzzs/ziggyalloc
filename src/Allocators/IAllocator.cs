using System;

namespace ZiggyAlloc
{
    public interface IAllocator
    {
        Pointer<T> Alloc<T>(int count = 1, bool zeroed = false) where T : unmanaged;
        void Free(IntPtr ptr);
        Slice<T> Slice<T>(int count, bool zeroed = false) where T : unmanaged;
    }
}