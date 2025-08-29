using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ZiggyAlloc;

namespace ZiggyAlloc.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    [GcServer(true)]
    public class DataTypeBenchmarks
    {
        private const int ElementCount = 10000;

        private SystemMemoryAllocator _allocator = null!;

        [GlobalSetup]
        public void Setup()
        {
            _allocator = new SystemMemoryAllocator();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            // SystemMemoryAllocator doesn't implement IDisposable
        }

        [Benchmark(Baseline = true)]
        public void ManagedArray_Byte()
        {
            var array = new byte[ElementCount];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (byte)(i % 256);
            }
        }

        [Benchmark]
        public void UnmanagedArray_Byte()
        {
            using var buffer = _allocator.Allocate<byte>(ElementCount);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(i % 256);
            }
        }

        [Benchmark]
        public void ManagedArray_Int()
        {
            var array = new int[ElementCount];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }
        }

        [Benchmark]
        public void UnmanagedArray_Int()
        {
            using var buffer = _allocator.Allocate<int>(ElementCount);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = i;
            }
        }

        [Benchmark]
        public void ManagedArray_Double()
        {
            var array = new double[ElementCount];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i * 3.14159;
            }
        }

        [Benchmark]
        public void UnmanagedArray_Double()
        {
            using var buffer = _allocator.Allocate<double>(ElementCount);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = i * 3.14159;
            }
        }

        [Benchmark]
        public void ManagedArray_Struct()
        {
            var array = new Point[ElementCount];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new Point { X = i, Y = i * 2 };
            }
        }

        [Benchmark]
        public void UnmanagedArray_Struct()
        {
            using var buffer = _allocator.Allocate<Point>(ElementCount);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = new Point { X = i, Y = i * 2 };
            }
        }
    }

    public struct Point
    {
        public int X;
        public int Y;
    }
}