using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ZiggyAlloc;

namespace ZiggyAlloc.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    [GcServer(true)]
    public class ExperimentalOptimizationsBenchmarks
    {
        private const int SmallArraySize = 100;
        private const int MediumArraySize = 10000;
        private const int TestIterations = 10000;

        private SystemMemoryAllocator _systemAllocator = null!;
        private UnmanagedMemoryPool _pooledAllocator = null!;
        private HybridAllocator _hybridAllocator = null!;

        [GlobalSetup]
        public void Setup()
        {
            _systemAllocator = new SystemMemoryAllocator();
            _pooledAllocator = new UnmanagedMemoryPool(_systemAllocator);
            _hybridAllocator = new HybridAllocator(_systemAllocator);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _hybridAllocator?.Dispose();
            _pooledAllocator?.Dispose();
            _systemAllocator = null!;
        }

        [Benchmark]
        public void OptimizedSystemAllocator_Small()
        {
            for (int i = 0; i < TestIterations; i++)
            {
                using var buffer = _systemAllocator.Allocate<int>(SmallArraySize);
                buffer[0] = i;
            }
        }

        [Benchmark]
        public void OptimizedSystemAllocator_Medium()
        {
            for (int i = 0; i < TestIterations / 10; i++)
            {
                using var buffer = _systemAllocator.Allocate<int>(MediumArraySize);
                buffer[0] = i;
            }
        }

        [Benchmark]
        public void OptimizedUnmanagedMemoryPool_Small()
        {
            for (int i = 0; i < TestIterations; i++)
            {
                using var buffer = _pooledAllocator.Allocate<int>(SmallArraySize);
                buffer[0] = i;
            }
        }

        [Benchmark]
        public void OptimizedUnmanagedMemoryPool_Medium()
        {
            for (int i = 0; i < TestIterations / 10; i++)
            {
                using var buffer = _pooledAllocator.Allocate<int>(MediumArraySize);
                buffer[0] = i;
            }
        }

        [Benchmark]
        public void OptimizedHybridAllocator_Small()
        {
            for (int i = 0; i < TestIterations; i++)
            {
                using var buffer = _hybridAllocator.Allocate<int>(SmallArraySize);
                buffer[0] = i;
            }
        }

        [Benchmark]
        public void OptimizedHybridAllocator_Medium()
        {
            for (int i = 0; i < TestIterations / 10; i++)
            {
                using var buffer = _hybridAllocator.Allocate<int>(MediumArraySize);
                buffer[0] = i;
            }
        }
    }
}