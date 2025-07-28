using System;
using System.Collections.Generic;

namespace ZiggyAlloc
{
    public sealed class ScopedAllocator : IAllocator, IDisposable
    {
        private readonly ManualAllocator _backend = new();
        private readonly List<IntPtr> _allocs = new();

        public Pointer<T> Alloc<T>(int count = 1, bool zeroed = false) where T : unmanaged
        {
            var p = _backend.Alloc<T>(count, zeroed);
            _allocs.Add(p.Raw);
            return p;
        }

        public void Free(IntPtr ptr) =>
            throw new NotSupportedException("Cannot free individual allocations from a ScopedAllocator.");

        public Slice<T> Slice<T>(int count, bool zeroed = false) where T : unmanaged =>
            new(Alloc<T>(count, zeroed), count);

        public void Dispose()
        {
            for (int i = _allocs.Count - 1; i >= 0; i--)
                _backend.Free(_allocs[i]);
            _allocs.Clear();
        }
    }
}