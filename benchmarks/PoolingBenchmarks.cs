using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;

namespace ZiggyAlloc.Benchmarks
{
    /// <summary>
    /// Benchmarks for the UnmanagedMemoryPool class.
    /// </summary>
    [SimpleJob(RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    [GcServer(true)]
    public class PoolingBenchmarks
    {
        private const int BufferSize = 1024;
        private const int IterationCount = 100; // Reduced from 1000 to speed up execution
        
        private SystemMemoryAllocator _systemAllocator;
        private UnmanagedMemoryPool _pooledAllocator;
        
        [GlobalSetup]
        public void Setup()
        {
            _systemAllocator = new SystemMemoryAllocator();
            _pooledAllocator = new UnmanagedMemoryPool(_systemAllocator);
        }
        
        [GlobalCleanup]
        public void Cleanup()
        {
            _pooledAllocator.Dispose();
        }
        
        [Benchmark(Baseline = true)]
        public void SystemAllocator()
        {
            for (int i = 0; i < IterationCount; i++)
            {
                using var buffer = _systemAllocator.Allocate<byte>(BufferSize);
                // Simulate some work
                buffer[0] = 1;
            }
        }
        
        [Benchmark]
        public void PooledAllocator()
        {
            for (int i = 0; i < IterationCount; i++)
            {
                using var buffer = _pooledAllocator.Allocate<byte>(BufferSize);
                // Simulate some work
                buffer[0] = 1;
            }
        }
        
        [Benchmark]
        public void PreWarmedPooledAllocator()
        {
            // Pre-warm the pool
            var prewarmBuffers = new UnmanagedBuffer<byte>[10];
            for (int i = 0; i < 10; i++)
            {
                prewarmBuffers[i] = _pooledAllocator.Allocate<byte>(BufferSize);
            }
            
            for (int i = 0; i < 10; i++)
            {
                prewarmBuffers[i].Dispose();
            }
            
            // Now run the benchmark
            for (int i = 0; i < IterationCount; i++)
            {
                using var buffer = _pooledAllocator.Allocate<byte>(BufferSize);
                // Simulate some work
                buffer[0] = 1;
            }
        }
    }
}