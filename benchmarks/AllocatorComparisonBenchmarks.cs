using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ZiggyAlloc;

namespace ZiggyAlloc.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    [GcServer(true)]
    public class AllocatorComparisonBenchmarks
    {
        private const int AllocationSize = 1000;
        private const int Iterations = 1000;

        private SystemMemoryAllocator _systemAllocator = null!;
        private DebugMemoryAllocator _debugAllocator = null!;
        private ScopedMemoryAllocator _scopedAllocator = null!;

        [GlobalSetup]
        public void Setup()
        {
            _systemAllocator = new SystemMemoryAllocator();
            _debugAllocator = new DebugMemoryAllocator("Benchmark", _systemAllocator);
            _scopedAllocator = new ScopedMemoryAllocator();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _debugAllocator.Dispose();
            // SystemMemoryAllocator and ScopedMemoryAllocator don't need explicit cleanup
        }

        [Benchmark(Baseline = true)]
        public void SystemAllocator_SingleAllocation()
        {
            var buffer = _systemAllocator.Allocate<int>(AllocationSize);
            buffer.Dispose();
        }

        [Benchmark]
        public void DebugAllocator_SingleAllocation()
        {
            var buffer = _debugAllocator.Allocate<int>(AllocationSize);
            buffer.Dispose();
        }

        [Benchmark]
        public void ScopedAllocator_SingleAllocation()
        {
            using var scoped = new ScopedMemoryAllocator();
            var buffer = scoped.Allocate<int>(AllocationSize);
            // No explicit dispose needed - handled by scoped allocator
        }

        [Benchmark]
        public void SystemAllocator_MultipleAllocations()
        {
            for (int i = 0; i < Iterations; i++)
            {
                var buffer = _systemAllocator.Allocate<int>(AllocationSize);
                buffer.Dispose();
            }
        }

        [Benchmark]
        public void DebugAllocator_MultipleAllocations()
        {
            for (int i = 0; i < Iterations; i++)
            {
                var buffer = _debugAllocator.Allocate<int>(AllocationSize);
                buffer.Dispose();
            }
        }

        [Benchmark]
        public void ScopedAllocator_MultipleAllocations()
        {
            using var scoped = new ScopedMemoryAllocator();
            for (int i = 0; i < Iterations; i++)
            {
                var buffer = scoped.Allocate<int>(AllocationSize);
                // No explicit dispose needed - handled by scoped allocator
            }
        }
    }
}