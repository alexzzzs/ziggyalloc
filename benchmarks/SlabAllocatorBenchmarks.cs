using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;

namespace ZiggyAlloc.Benchmarks
{
    /// <summary>
    /// Benchmarks for the SlabAllocator class.
    /// </summary>
    [SimpleJob(RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    [GcServer(true)]
    public class SlabAllocatorBenchmarks
    {
        private const int SmallSize = 128;
        private const int MediumSize = 1024;
        private const int IterationCount = 1000;
        
        private SystemMemoryAllocator _systemAllocator = null!;
        private SlabAllocator _slabAllocator = null!;
        
        [GlobalSetup]
        public void Setup()
        {
            _systemAllocator = new SystemMemoryAllocator();
            _slabAllocator = new SlabAllocator(_systemAllocator);
        }
        
        [GlobalCleanup]
        public void Cleanup()
        {
            _slabAllocator.Dispose();
        }
        
        // Small allocation benchmarks
        [Benchmark(Baseline = true)]
        public void SystemAllocator_Small()
        {
            for (int i = 0; i < IterationCount; i++)
            {
                using var buffer = _systemAllocator.Allocate<byte>(SmallSize);
                buffer[0] = 1;
            }
        }
        
        [Benchmark]
        public void SlabAllocator_Small()
        {
            for (int i = 0; i < IterationCount; i++)
            {
                using var buffer = _slabAllocator.Allocate<byte>(SmallSize);
                buffer[0] = 1;
            }
        }
        
        // Medium allocation benchmarks
        [Benchmark]
        public void SystemAllocator_Medium()
        {
            for (int i = 0; i < IterationCount; i++)
            {
                using var buffer = _systemAllocator.Allocate<byte>(MediumSize);
                buffer[0] = 1;
            }
        }
        
        [Benchmark]
        public void SlabAllocator_Medium()
        {
            for (int i = 0; i < IterationCount; i++)
            {
                using var buffer = _slabAllocator.Allocate<byte>(MediumSize);
                buffer[0] = 1;
            }
        }
        
        // Large allocation benchmarks (will delegate to base allocator)
        [Benchmark]
        public void SystemAllocator_Large()
        {
            for (int i = 0; i < 100; i++) // Fewer iterations for large allocations
            {
                using var buffer = _systemAllocator.Allocate<byte>(8192);
                buffer[0] = 1;
            }
        }
        
        [Benchmark]
        public void SlabAllocator_Large()
        {
            for (int i = 0; i < 100; i++) // Fewer iterations for large allocations
            {
                using var buffer = _slabAllocator.Allocate<byte>(8192);
                buffer[0] = 1;
            }
        }
    }
}