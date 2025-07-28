using System;
using System.Text;

namespace ZiggyAlloc
{
    public readonly struct Ctx
    {
        public readonly IAllocator alloc;
        public readonly IWriter @out;
        public readonly IReader @in;

        public Ctx(IAllocator alloc, IWriter @out, IReader @in)
        {
            this.alloc = alloc;
            this.@out = @out;
            this.@in = @in;
        }

        public void Print(string s) => @out.Write(s);
        public void Print(char c) => @out.Write(c);
        public void PrintLine() => @out.WriteLine();
        public void PrintLine<T>(T value) => @out.WriteLine(value);
        public void Printf(string format, params object[] args) => @out.WriteLine(string.Format(format, args));

        public string? ReadLine() => @in.ReadLine();
        public int ReadChar() => @in.Read();

        public Pointer<T> Alloc<T>(int count = 1, bool zeroed = false) where T : unmanaged =>
            alloc.Alloc<T>(count, zeroed);

        public Slice<T> AllocSlice<T>(int count, bool zeroed = false) where T : unmanaged =>
            alloc.Slice<T>(count, zeroed);

        public AutoFree<T> Auto<T>(int count = 1, bool zeroed = false) where T : unmanaged =>
            new(alloc, count, zeroed);

        public Pointer<T> Alloc<T>(DeferScope defer, int count = 1, bool zeroed = false) where T : unmanaged
        {
            var p = alloc.Alloc<T>(count, zeroed);
            var allocatorCopy = alloc;
            var ptrRaw = p.Raw;
            defer.Defer(() => allocatorCopy.Free(ptrRaw));
            return p;
        }

        public Slice<T> AllocSlice<T>(DeferScope defer, int count, bool zeroed = false) where T : unmanaged
        {
            var s = alloc.Slice<T>(count, zeroed);
            var allocatorCopy = alloc;
            var ptrRaw = s.Ptr.Raw;
            defer.Defer(() => allocatorCopy.Free(ptrRaw));
            return s;
        }

        public Slice<byte> FormatToSlice(DeferScope defer, string format, params object[] args)
        {
            var formatted = string.Format(format, args);
            int byteCount = Encoding.UTF8.GetByteCount(formatted);
            var slice = alloc.Slice<byte>(byteCount + 1); // +1 for null terminator
            Encoding.UTF8.GetBytes(formatted, slice.AsSpan());
            slice[byteCount] = 0; // Null-terminated
            var allocatorCopy = alloc;
            var ptrRaw = slice.Ptr.Raw;
            defer.Defer(() => allocatorCopy.Free(ptrRaw));
            return slice;
        }
    }
}