using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using ZiggyAlloc;

namespace ZiggyAlloc.Benchmarks
{
    /// <summary>
    /// Benchmarks for the NUMA-aware allocator to measure performance improvements on multi-socket systems.
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    [GcServer(true)]
    [GcConcurrent(true)]
    [GcForce(true)]
    public class NumaAwareAllocatorBenchmarks
    {
        private SystemMemoryAllocator? _systemAllocator;
        private NumaAwareAllocator? _numaAllocator;
        private UnmanagedMemoryPool? _memoryPool;
        private ThreadLocalMemoryPool? _threadLocalPool;

        [GlobalSetup]
        public void Setup()
        {
            _systemAllocator = new SystemMemoryAllocator();
            _numaAllocator = new NumaAwareAllocator(_systemAllocator);
            _memoryPool = new UnmanagedMemoryPool(_systemAllocator);
            _threadLocalPool = new ThreadLocalMemoryPool(_systemAllocator);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _numaAllocator?.Dispose();
            _memoryPool?.Dispose();
            _threadLocalPool?.Dispose();
            _systemAllocator?.Dispose();
        }

        [Benchmark(Description = "NUMA Allocator - Small Allocations")]
        [BenchmarkCategory("Allocation")]
        public void NumaAllocator_SmallAllocations()
        {
            for (int i = 0; i < 1000; i++)
            {
                using var buffer = _numaAllocator!.Allocate<int>(16);
                buffer[0] = i;
            }
        }

        [Benchmark(Description = "NUMA Allocator - Medium Allocations")]
        [BenchmarkCategory("Allocation")]
        public void NumaAllocator_MediumAllocations()
        {
            for (int i = 0; i < 1000; i++)
            {
                using var buffer = _numaAllocator!.Allocate<int>(1024);
                buffer[i % buffer.Length] = i;
            }
        }

        [Benchmark(Description = "NUMA Allocator - Large Allocations")]
        [BenchmarkCategory("Allocation")]
        public void NumaAllocator_LargeAllocations()
        {
            for (int i = 0; i < 100; i++)
            {
                using var buffer = _numaAllocator!.Allocate<byte>(65536); // 64KB
                buffer[i % buffer.Length] = (byte)i;
            }
        }

        [Benchmark(Description = "NUMA Allocator - Mixed Sizes")]
        [BenchmarkCategory("Allocation")]
        public void NumaAllocator_MixedSizes()
        {
            for (int i = 0; i < 500; i++)
            {
                // Mix of different allocation sizes
                using var small = _numaAllocator!.Allocate<byte>(64);
                using var medium = _numaAllocator.Allocate<int>(512);
                using var large = _numaAllocator.Allocate<double>(2048);

                small[0] = (byte)i;
                medium[0] = i;
                large[0] = i;
            }
        }

        [Benchmark(Description = "NUMA Allocator - Parallel Small Allocations")]
        [BenchmarkCategory("Multithreading")]
        public void NumaAllocator_ParallelSmallAllocations()
        {
            Parallel.For(0, 100, i =>
            {
                for (int j = 0; j < 100; j++)
                {
                    using var buffer = _numaAllocator!.Allocate<int>(32);
                    buffer[0] = i * 1000 + j;
                }
            });
        }

        [Benchmark(Description = "NUMA Allocator - Parallel Medium Allocations")]
        [BenchmarkCategory("Multithreading")]
        public void NumaAllocator_ParallelMediumAllocations()
        {
            Parallel.For(0, 50, i =>
            {
                for (int j = 0; j < 50; j++)
                {
                    using var buffer = _numaAllocator!.Allocate<double>(1024);
                    buffer[0] = i * 1000 + j;
                }
            });
        }

        [Benchmark(Description = "NUMA Allocator - High Frequency")]
        [BenchmarkCategory("Performance")]
        public void NumaAllocator_HighFrequency()
        {
            for (int i = 0; i < 10000; i++)
            {
                using var buffer = _numaAllocator!.Allocate<byte>(8);
                buffer[0] = (byte)(i % 256);
            }
        }

        [Benchmark(Description = "NUMA Allocator - Memory Reuse Pattern")]
        [BenchmarkCategory("MemoryPattern")]
        public void NumaAllocator_MemoryReuse()
        {
            var buffers = new UnmanagedBuffer<int>[100];

            try
            {
                // Allocate
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i] = _numaAllocator!.Allocate<int>(100);
                    buffers[i][0] = i;
                }

                // Use
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i][1] = buffers[i][0] * 2;
                }
            }
            finally
            {
                // Cleanup - guaranteed to run even if exception occurs during Use phase
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i]?.Dispose();
                }
            }
        }

        [Benchmark(Description = "NUMA Allocator - Struct Allocations")]
        [BenchmarkCategory("DataTypes")]
        public void NumaAllocator_StructAllocations()
        {
            for (int i = 0; i < 1000; i++)
            {
                using var buffer = _numaAllocator!.Allocate<Point3D>(100);
                buffer[0] = new Point3D { X = i, Y = i * 2, Z = i * 3 };
            }
        }

        [Benchmark(Description = "NUMA Allocator - Zero Memory Pattern")]
        [BenchmarkCategory("MemoryPattern")]
        public void NumaAllocator_ZeroMemoryPattern()
        {
            for (int i = 0; i < 500; i++)
            {
                using var buffer = _numaAllocator!.Allocate<double>(2048, zeroMemory: true);
                // Buffer is already zero-initialized
                buffer[0] = i;
            }
        }

        // Comparison benchmarks with other allocators

        [Benchmark(Description = "System Allocator - Small Allocations")]
        [BenchmarkCategory("Comparison")]
        public void SystemAllocator_SmallAllocations()
        {
            for (int i = 0; i < 1000; i++)
            {
                using var buffer = _systemAllocator!.Allocate<int>(16);
                buffer[0] = i;
            }
        }

        [Benchmark(Description = "Memory Pool - Small Allocations")]
        [BenchmarkCategory("Comparison")]
        public void MemoryPool_SmallAllocations()
        {
            for (int i = 0; i < 1000; i++)
            {
                using var buffer = _memoryPool!.Allocate<int>(16);
                buffer[0] = i;
            }
        }

        [Benchmark(Description = "Thread Local Pool - Small Allocations")]
        [BenchmarkCategory("Comparison")]
        public void ThreadLocalPool_SmallAllocations()
        {
            for (int i = 0; i < 1000; i++)
            {
                using var buffer = _threadLocalPool!.Allocate<int>(16);
                buffer[0] = i;
            }
        }

        [Benchmark(Description = "NUMA vs System Allocator - Parallel")]
        [BenchmarkCategory("Comparison")]
        public void NumaVsSystem_ParallelAllocations()
        {
            const int taskCount = 4;

            // NUMA allocator tasks
            var numaTasks = new Task[taskCount];
            for (int t = 0; t < taskCount; t++)
            {
                numaTasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < 250; i++)
                    {
                        using var buffer = _numaAllocator!.Allocate<int>(256);
                        buffer[0] = i;
                    }
                });
            }

            // System allocator tasks
            var systemTasks = new Task[taskCount];
            for (int t = 0; t < taskCount; t++)
            {
                systemTasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < 250; i++)
                    {
                        using var buffer = _systemAllocator!.Allocate<int>(256);
                        buffer[0] = i;
                    }
                });
            }

            Task.WaitAll(numaTasks);
            Task.WaitAll(systemTasks);
        }

        [Benchmark(Description = "NUMA Allocator - Statistics Collection")]
        [BenchmarkCategory("Overhead")]
        public void NumaAllocator_StatisticsOverhead()
        {
            for (int i = 0; i < 1000; i++)
            {
                using var buffer = _numaAllocator!.Allocate<int>(64);
                buffer[0] = i;

                // Collect statistics (potential overhead)
                var stats = _numaAllocator.GetNodeStatistics();
            }
        }

        [Benchmark(Description = "NUMA Allocator - Cross-Node Access Pattern")]
        [BenchmarkCategory("NUMAPattern")]
        public void NumaAllocator_CrossNodePattern()
        {
            // This benchmark simulates accessing memory that might be on different NUMA nodes
            var buffers = new UnmanagedBuffer<int>[50];
            try
            {
                // Allocate on multiple "nodes" (simulated by different sizes/patterns)
                for (int i = 0; i < buffers.Length; i++)
                {
                    int size = 100 + (i % 3) * 50; // Vary sizes to potentially hit different nodes
                    buffers[i] = _numaAllocator!.Allocate<int>(size);
                    buffers[i][0] = i;
                }

                // Access all buffers (simulating cross-node access)
                for (int i = 0; i < buffers.Length; i++)
                {
                    for (int j = 0; j < buffers.Length; j++)
                    {
                        buffers[i][j % buffers[i].Length] = buffers[j][0];
                    }
                }
            }
            finally
            {
                // Cleanup
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i]?.Dispose();
                }
            }
        }
        // Helper struct for struct allocation benchmarks
        private struct Point3D
        {
            public float X, Y, Z;
        }
    }
}