using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ZiggyAlloc;

namespace ZiggyAlloc.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    [GcServer(true)]
    public class AllocatorBenchmarks
    {
        private const int AllocationSize = 1000;
        private const int Iterations = 1000;

        private SystemMemoryAllocator _systemAllocator = null!;

        [GlobalSetup]
        public void Setup()
        {
            _systemAllocator = new SystemMemoryAllocator();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            // SystemMemoryAllocator doesn't implement IDisposable, so no cleanup needed
        }

        [Benchmark]
        public void SystemAllocator_AllocateAndFree()
        {
            for (int i = 0; i < Iterations; i++)
            {
                var buffer = _systemAllocator.Allocate<int>(AllocationSize);
                buffer.Dispose(); // Explicitly free
            }
        }

        [Benchmark]
        public void ScopedAllocator_AllocateAndDispose()
        {
            for (int i = 0; i < Iterations; i++)
            {
                using var scopedAllocator = new ScopedMemoryAllocator();
                var buffer = scopedAllocator.Allocate<int>(AllocationSize);
                // No explicit free needed - will be freed when scoped allocator is disposed
            }
        }

        [Benchmark]
        public void ScopedAllocator_MultipleAllocations()
        {
            using var scopedAllocator = new ScopedMemoryAllocator();
            for (int i = 0; i < Iterations; i++)
            {
                var buffer = scopedAllocator.Allocate<int>(AllocationSize);
                // All allocations will be freed when scoped allocator is disposed
            }
        }
    }
}