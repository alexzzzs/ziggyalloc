using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace ZiggyAlloc
{
    public enum LeakReportingMode { Log, Throw, Break }

    public sealed class DebugAllocator : IAllocator, IDisposable
    {
        private readonly record struct AllocationMetadata(int Size, string FilePath, int LineNumber, string MemberName);

        private readonly Dictionary<IntPtr, AllocationMetadata> _liveAllocations = new();
        private readonly IAllocator _backend;
        private readonly string _name;
        private readonly LeakReportingMode _reportingMode;

        public DebugAllocator(string name, IAllocator backend, LeakReportingMode mode = LeakReportingMode.Log)
        {
            _name = name;
            _backend = backend;
            _reportingMode = mode;
        }

        public Pointer<T> Alloc<T>(int count = 1, bool zeroed = false) where T : unmanaged
        {
            return AllocWithCallerInfo<T>(count, zeroed);
        }

        public Pointer<T> AllocWithCallerInfo<T>(int count = 1, bool zeroed = false,
            [CallerFilePath] string f = "",
            [CallerLineNumber] int l = 0,
            [CallerMemberName] string m = "") where T : unmanaged
        {
            var ptr = _backend.Alloc<T>(count, zeroed);
            int size;
            unsafe { size = sizeof(T) * count; }
            lock (_liveAllocations)
                _liveAllocations.Add(ptr.Raw, new AllocationMetadata(size, f, l, m));
            return ptr;
        }

        public void Free(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) return;

            lock (_liveAllocations)
                if (!_liveAllocations.Remove(ptr))
                    Debug.WriteLine($"[DebugAllocator '{_name}'] Warning: Free untracked ptr 0x{ptr:X}.");

            _backend.Free(ptr);
        }

        public Slice<T> Slice<T>(int count, bool zeroed = false) where T : unmanaged =>
            new(AllocWithCallerInfo<T>(count, zeroed), count);

        public void ReportLeaks()
        {
            lock (_liveAllocations)
            {
                if (_liveAllocations.Count == 0) return;

                var sb = new StringBuilder()
                    .AppendLine($"!!! [DebugAllocator '{_name}'] Memory Leak: {_liveAllocations.Count} unfreed allocation(s) !!!");

                foreach (var (p, meta) in _liveAllocations)
                    sb.AppendLine($"  - 0x{p:X} ({meta.Size} bytes) from {meta.FilePath}:{meta.LineNumber} ({meta.MemberName})");

                string report = sb.ToString();

                if (_reportingMode == LeakReportingMode.Throw)
                    throw new InvalidOperationException(report);

                Console.Error.WriteLine(report);

                if (_reportingMode == LeakReportingMode.Break)
                    Debugger.Break();
            }
        }

        public void Dispose() => ReportLeaks();
    }
}