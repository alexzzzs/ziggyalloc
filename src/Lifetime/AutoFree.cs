using System;

namespace ZiggyAlloc
{
    public readonly ref struct AutoFree<T> : IDisposable where T : unmanaged
    {
        private readonly IAllocator _allocator;
        public readonly Pointer<T> Ptr { get; }
        public ref T Value => ref Ptr.Value;

        internal AutoFree(IAllocator allocator, int count, bool zeroed)
        {
            _allocator = allocator;
            Ptr = allocator.Alloc<T>(count, zeroed);
        }

        public void Dispose() => _allocator.Free(Ptr.Raw);
    }
}