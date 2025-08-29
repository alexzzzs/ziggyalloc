using System;
using System.Buffers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ZiggyAlloc;

namespace ZiggyAlloc.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    [GcServer(true)]
    public class AllocationBenchmarks
    {
        private const int SmallArraySize = 100;
        private const int MediumArraySize = 10000;
        private const int LargeArraySize = 1000000;

        private SystemMemoryAllocator _allocator = null!;
        private ArrayPool<int> _arrayPool = null!;

        [GlobalSetup]
        public void Setup()
        {
            _allocator = new SystemMemoryAllocator();
            _arrayPool = ArrayPool<int>.Shared;
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            // SystemMemoryAllocator doesn't implement IDisposable, so no cleanup needed
        }

        [Benchmark(Baseline = true)]
        public void ManagedArray_Small()
        {
            var array = new int[SmallArraySize];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }
        }

        [Benchmark]
        public void UnmanagedArray_Small()
        {
            using var buffer = _allocator.Allocate<int>(SmallArraySize);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = i;
            }
        }

        [Benchmark]
        public void ArrayPool_Small()
        {
            var array = _arrayPool.Rent(SmallArraySize);
            try
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = i;
                }
            }
            finally
            {
                _arrayPool.Return(array);
            }
        }

        [Benchmark]
        public void ManagedArray_Medium()
        {
            var array = new int[MediumArraySize];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }
        }

        [Benchmark]
        public void UnmanagedArray_Medium()
        {
            using var buffer = _allocator.Allocate<int>(MediumArraySize);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = i;
            }
        }

        [Benchmark]
        public void ArrayPool_Medium()
        {
            var array = _arrayPool.Rent(MediumArraySize);
            try
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = i;
                }
            }
            finally
            {
                _arrayPool.Return(array);
            }
        }

        [Benchmark]
        public void ManagedArray_Large()
        {
            var array = new int[LargeArraySize];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }
        }

        [Benchmark]
        public void UnmanagedArray_Large()
        {
            using var buffer = _allocator.Allocate<int>(LargeArraySize);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = i;
            }
        }

        [Benchmark]
        public void ArrayPool_Large()
        {
            var array = _arrayPool.Rent(LargeArraySize);
            try
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = i;
                }
            }
            finally
            {
                _arrayPool.Return(array);
            }
        }
    }
}