using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;

namespace ZiggyAlloc.Benchmarks
{
    /// <summary>
    /// Benchmarks for the HybridAllocator class.
    /// </summary>
    [SimpleJob(RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    [GcServer(true)]
    public class HybridAllocatorBenchmarks
    {
        private const int SmallSize = 128;
        private const int MediumSize = 1024;
        private const int LargeSize = 8192;
        
        private SystemMemoryAllocator _systemAllocator;
        private HybridAllocator _hybridAllocator;
        
        [GlobalSetup]
        public void Setup()
        {
            _systemAllocator = new SystemMemoryAllocator();
            _hybridAllocator = new HybridAllocator(_systemAllocator);
        }
        
        [GlobalCleanup]
        public void Cleanup()
        {
            // Cleanup if needed
        }
        
        // Byte array benchmarks
        [Benchmark(Baseline = true)]
        public void SystemAllocator_Byte_Small()
        {
            using var buffer = _systemAllocator.Allocate<byte>(SmallSize);
            buffer[0] = 1;
        }
        
        [Benchmark]
        public void HybridAllocator_Byte_Small()
        {
            using var buffer = _hybridAllocator.Allocate<byte>(SmallSize);
            buffer[0] = 1;
        }
        
        [Benchmark]
        public void SystemAllocator_Byte_Large()
        {
            using var buffer = _systemAllocator.Allocate<byte>(LargeSize);
            buffer[0] = 1;
        }
        
        [Benchmark]
        public void HybridAllocator_Byte_Large()
        {
            using var buffer = _hybridAllocator.Allocate<byte>(LargeSize);
            buffer[0] = 1;
        }
        
        // Int array benchmarks
        [Benchmark]
        public void SystemAllocator_Int_Small()
        {
            using var buffer = _systemAllocator.Allocate<int>(SmallSize);
            buffer[0] = 1;
        }
        
        [Benchmark]
        public void HybridAllocator_Int_Small()
        {
            using var buffer = _hybridAllocator.Allocate<int>(SmallSize);
            buffer[0] = 1;
        }
        
        [Benchmark]
        public void SystemAllocator_Int_Large()
        {
            using var buffer = _systemAllocator.Allocate<int>(LargeSize);
            buffer[0] = 1;
        }
        
        [Benchmark]
        public void HybridAllocator_Int_Large()
        {
            using var buffer = _hybridAllocator.Allocate<int>(LargeSize);
            buffer[0] = 1;
        }
        
        // Double array benchmarks
        [Benchmark]
        public void SystemAllocator_Double_Small()
        {
            using var buffer = _systemAllocator.Allocate<double>(SmallSize);
            buffer[0] = 1.0;
        }
        
        [Benchmark]
        public void HybridAllocator_Double_Small()
        {
            using var buffer = _hybridAllocator.Allocate<double>(SmallSize);
            buffer[0] = 1.0;
        }
        
        [Benchmark]
        public void SystemAllocator_Double_Large()
        {
            using var buffer = _systemAllocator.Allocate<double>(LargeSize);
            buffer[0] = 1.0;
        }
        
        [Benchmark]
        public void HybridAllocator_Double_Large()
        {
            using var buffer = _hybridAllocator.Allocate<double>(LargeSize);
            buffer[0] = 1.0;
        }
    }
}