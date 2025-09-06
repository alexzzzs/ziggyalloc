using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ZiggyAlloc;

namespace ZiggyAlloc.Benchmarks
{
    /// <summary>
    /// Benchmarks for testing allocator performance under multithreading scenarios.
    /// </summary>
    [SimpleJob(RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    [GcServer(true)]
    public class MultithreadingBenchmarks
    {
        private const int ParallelTaskCount = 8;
        private const int AllocationsPerTask = 100;
        private const int SmallBufferSize = 64;
        private const int MediumBufferSize = 1024;
        private const int LargeBufferSize = 8192;

        private SystemMemoryAllocator _systemAllocator = null!;
        private ScopedMemoryAllocator _scopedAllocator = null!;
        private UnmanagedMemoryPool _memoryPool = null!;
        private HybridAllocator _hybridAllocator = null!;
        private SlabAllocator _slabAllocator = null!;

        [GlobalSetup]
        public void Setup()
        {
            _systemAllocator = new SystemMemoryAllocator();
            _scopedAllocator = new ScopedMemoryAllocator();
            _memoryPool = new UnmanagedMemoryPool(_systemAllocator);
            _hybridAllocator = new HybridAllocator(_systemAllocator);
            _slabAllocator = new SlabAllocator(_systemAllocator);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _scopedAllocator.Dispose();
            _memoryPool.Dispose();
            _hybridAllocator.Dispose();
            _slabAllocator.Dispose();
        }

        [Benchmark]
        public void SystemAllocator_ParallelSmallAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask; j++)
                {
                    using var buffer = _systemAllocator.Allocate<byte>(SmallBufferSize);
                    // Do some work
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        [Benchmark]
        public void SystemAllocator_ParallelMediumAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask; j++)
                {
                    using var buffer = _systemAllocator.Allocate<byte>(MediumBufferSize);
                    // Do some work
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        [Benchmark]
        public void SystemAllocator_ParallelLargeAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask / 10; j++) // Fewer large allocations
                {
                    using var buffer = _systemAllocator.Allocate<byte>(LargeBufferSize);
                    // Do some work
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        [Benchmark]
        public void MemoryPool_ParallelSmallAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask; j++)
                {
                    using var buffer = _memoryPool.Allocate<byte>(SmallBufferSize);
                    // Do some work
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        [Benchmark]
        public void MemoryPool_ParallelMediumAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask; j++)
                {
                    using var buffer = _memoryPool.Allocate<byte>(MediumBufferSize);
                    // Do some work
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        [Benchmark]
        public void MemoryPool_ParallelLargeAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask / 10; j++) // Fewer large allocations
                {
                    using var buffer = _memoryPool.Allocate<byte>(LargeBufferSize);
                    // Do some work
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        [Benchmark]
        public void SlabAllocator_ParallelSmallAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask; j++)
                {
                    using var buffer = _slabAllocator.Allocate<byte>(SmallBufferSize);
                    // Do some work
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        [Benchmark]
        public void SlabAllocator_ParallelMediumAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask; j++)
                {
                    using var buffer = _slabAllocator.Allocate<byte>(MediumBufferSize);
                    // Do some work
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        [Benchmark]
        public void HybridAllocator_ParallelSmallAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask; j++)
                {
                    using var buffer = _hybridAllocator.Allocate<byte>(SmallBufferSize);
                    // Do some work
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        [Benchmark]
        public void HybridAllocator_ParallelLargeAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask; j++)
                {
                    using var buffer = _hybridAllocator.Allocate<byte>(LargeBufferSize);
                    // Do some work
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        [Benchmark]
        public void ScopedAllocator_SerialSmallAllocations()
        {
            // Scoped allocator is not thread-safe, so we test serial performance
            for (int i = 0; i < ParallelTaskCount; i++)
            {
                using var scope = new ScopedMemoryAllocator();
                for (int j = 0; j < AllocationsPerTask; j++)
                {
                    using var buffer = scope.Allocate<byte>(SmallBufferSize);
                    // Do some work
                    buffer[0] = (byte)(i + j);
                }
            }
        }

        [Benchmark]
        public void TaskBasedAllocationPattern()
        {
            var tasks = new Task[ParallelTaskCount];
            
            for (int i = 0; i < ParallelTaskCount; i++)
            {
                int taskId = i;
                tasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < AllocationsPerTask; j++)
                    {
                        using var buffer1 = _systemAllocator.Allocate<byte>(SmallBufferSize);
                        using var buffer2 = _memoryPool.Allocate<int>(16);
                        using var buffer3 = _slabAllocator.Allocate<double>(8);
                        
                        // Do some work with all buffers
                        buffer1[0] = (byte)(taskId + j);
                        buffer2[0] = taskId + j;
                        buffer3[0] = taskId + j;
                    }
                });
            }
            
            Task.WaitAll(tasks);
        }

        [Benchmark]
        public void ProducerConsumerPattern()
        {
            var tasks = new Task[ParallelTaskCount];
            
            // Half producers, half consumers
            for (int i = 0; i < ParallelTaskCount / 2; i++)
            {
                int taskId = i;
                tasks[i] = Task.Run(() =>
                {
                    // Producers - allocate and "process"
                    for (int j = 0; j < AllocationsPerTask * 2; j++)
                    {
                        using var buffer = _memoryPool.Allocate<byte>(MediumBufferSize);
                        // Simulate work
                        for (int k = 0; k < Math.Min(10, buffer.Length); k++)
                        {
                            buffer[k] = (byte)(taskId + j + k);
                        }
                    }
                });
            }
            
            for (int i = ParallelTaskCount / 2; i < ParallelTaskCount; i++)
            {
                int taskId = i;
                tasks[i] = Task.Run(() =>
                {
                    // Consumers - allocate and "process"
                    for (int j = 0; j < AllocationsPerTask * 2; j++)
                    {
                        using var buffer = _slabAllocator.Allocate<int>(64);
                        // Simulate work
                        for (int k = 0; k < Math.Min(5, buffer.Length); k++)
                        {
                            buffer[k] = taskId + j + k;
                        }
                    }
                });
            }
            
            Task.WaitAll(tasks);
        }
    }
}