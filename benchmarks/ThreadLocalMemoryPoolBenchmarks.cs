using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ZiggyAlloc;

namespace ZiggyAlloc.Benchmarks
{
    /// <summary>
    /// Comprehensive benchmarks for ThreadLocalMemoryPool comparing performance with other allocators.
    /// </summary>
    [SimpleJob(RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    [GcServer(true)]
    public class ThreadLocalMemoryPoolBenchmarks
    {
        private const int ParallelTaskCount = 16; // More threads to stress test thread-local benefits
        private const int AllocationsPerTask = 200;
        private const int SmallBufferSize = 64;
        private const int MediumBufferSize = 1024;
        private const int LargeBufferSize = 8192;
        private const int ReuseBufferSize = 256; // Size that fits in size classes for reuse testing

        private SystemMemoryAllocator _systemAllocator = null!;
        private UnmanagedMemoryPool _memoryPool = null!;
        private ThreadLocalMemoryPool _threadLocalPool = null!;
        private ThreadLocalMemoryPool _threadLocalPoolWithSharing = null!;
        private SlabAllocator _slabAllocator = null!;
        private HybridAllocator _hybridAllocator = null!;

        [GlobalSetup]
        public void Setup()
        {
            _systemAllocator = new SystemMemoryAllocator();
            _memoryPool = new UnmanagedMemoryPool(_systemAllocator);
            _threadLocalPool = new ThreadLocalMemoryPool(_systemAllocator, enableCrossThreadSharing: false);
            _threadLocalPoolWithSharing = new ThreadLocalMemoryPool(_systemAllocator, enableCrossThreadSharing: true);
            _slabAllocator = new SlabAllocator(_systemAllocator);
            _hybridAllocator = new HybridAllocator(_systemAllocator);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _memoryPool.Dispose();
            _threadLocalPool.Dispose();
            _threadLocalPoolWithSharing.Dispose();
            _slabAllocator.Dispose();
            _hybridAllocator.Dispose();
        }

        #region Single-Threaded Benchmarks

        [Benchmark(Baseline = true)]
        public void SystemAllocator_SingleThread()
        {
            for (int i = 0; i < AllocationsPerTask; i++)
            {
                using var buffer = _systemAllocator.Allocate<byte>(MediumBufferSize);
                buffer[0] = (byte)i;
            }
        }

        [Benchmark]
        public void MemoryPool_SingleThread()
        {
            for (int i = 0; i < AllocationsPerTask; i++)
            {
                using var buffer = _memoryPool.Allocate<byte>(MediumBufferSize);
                buffer[0] = (byte)i;
            }
        }

        [Benchmark]
        public void ThreadLocalPool_SingleThread()
        {
            for (int i = 0; i < AllocationsPerTask; i++)
            {
                using var buffer = _threadLocalPool.Allocate<byte>(MediumBufferSize);
                buffer[0] = (byte)i;
            }
        }

        [Benchmark]
        public void ThreadLocalPoolWithSharing_SingleThread()
        {
            for (int i = 0; i < AllocationsPerTask; i++)
            {
                using var buffer = _threadLocalPoolWithSharing.Allocate<byte>(MediumBufferSize);
                buffer[0] = (byte)i;
            }
        }

        #endregion

        #region Multi-Threaded Benchmarks - Small Allocations

        [Benchmark]
        public void SystemAllocator_ParallelSmallAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask; j++)
                {
                    using var buffer = _systemAllocator.Allocate<byte>(SmallBufferSize);
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
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        [Benchmark]
        public void ThreadLocalPool_ParallelSmallAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask; j++)
                {
                    using var buffer = _threadLocalPool.Allocate<byte>(SmallBufferSize);
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        [Benchmark]
        public void ThreadLocalPoolWithSharing_ParallelSmallAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask; j++)
                {
                    using var buffer = _threadLocalPoolWithSharing.Allocate<byte>(SmallBufferSize);
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
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        #endregion

        #region Multi-Threaded Benchmarks - Medium Allocations

        [Benchmark]
        public void SystemAllocator_ParallelMediumAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask; j++)
                {
                    using var buffer = _systemAllocator.Allocate<byte>(MediumBufferSize);
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
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        [Benchmark]
        public void ThreadLocalPool_ParallelMediumAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask; j++)
                {
                    using var buffer = _threadLocalPool.Allocate<byte>(MediumBufferSize);
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        [Benchmark]
        public void ThreadLocalPoolWithSharing_ParallelMediumAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask; j++)
                {
                    using var buffer = _threadLocalPoolWithSharing.Allocate<byte>(MediumBufferSize);
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        #endregion

        #region Multi-Threaded Benchmarks - Large Allocations

        [Benchmark]
        public void SystemAllocator_ParallelLargeAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask / 10; j++) // Fewer large allocations
                {
                    using var buffer = _systemAllocator.Allocate<byte>(LargeBufferSize);
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
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        [Benchmark]
        public void ThreadLocalPool_ParallelLargeAllocations()
        {
            Parallel.For(0, ParallelTaskCount, i =>
            {
                for (int j = 0; j < AllocationsPerTask / 10; j++) // Fewer large allocations
                {
                    using var buffer = _threadLocalPool.Allocate<byte>(LargeBufferSize);
                    buffer[0] = (byte)(i + j);
                }
            });
        }

        #endregion

        #region Reuse Pattern Benchmarks

        [Benchmark]
        public void SystemAllocator_ReusePattern()
        {
            // Allocate and dispose many buffers of same size to test reuse
            for (int iteration = 0; iteration < 5; iteration++)
            {
                var buffers = new UnmanagedBuffer<byte>[AllocationsPerTask / 10];
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i] = _systemAllocator.Allocate<byte>(ReuseBufferSize);
                }
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i].Dispose();
                }
            }
        }

        [Benchmark]
        public void MemoryPool_ReusePattern()
        {
            // Allocate and dispose many buffers of same size to test reuse
            for (int iteration = 0; iteration < 5; iteration++)
            {
                var buffers = new UnmanagedBuffer<byte>[AllocationsPerTask / 10];
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i] = _memoryPool.Allocate<byte>(ReuseBufferSize);
                }
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i].Dispose();
                }
            }
        }

        [Benchmark]
        public void ThreadLocalPool_ReusePattern()
        {
            // Allocate and dispose many buffers of same size to test reuse
            for (int iteration = 0; iteration < 5; iteration++)
            {
                var buffers = new UnmanagedBuffer<byte>[AllocationsPerTask / 10];
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i] = _threadLocalPool.Allocate<byte>(ReuseBufferSize);
                }
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i].Dispose();
                }
            }
        }

        [Benchmark]
        public void ThreadLocalPoolWithSharing_ReusePattern()
        {
            // Allocate and dispose many buffers of same size to test reuse
            for (int iteration = 0; iteration < 5; iteration++)
            {
                var buffers = new UnmanagedBuffer<byte>[AllocationsPerTask / 10];
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i] = _threadLocalPoolWithSharing.Allocate<byte>(ReuseBufferSize);
                }
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i].Dispose();
                }
            }
        }

        #endregion

        #region High Contention Benchmarks

        [Benchmark]
        public void HighContention_SystemAllocator()
        {
            var tasks = new Task[ParallelTaskCount];
            for (int t = 0; t < ParallelTaskCount; t++)
            {
                int threadId = t;
                tasks[t] = Task.Run(() =>
                {
                    // Very frequent allocations to create contention
                    for (int i = 0; i < AllocationsPerTask * 2; i++)
                    {
                        using var buffer = _systemAllocator.Allocate<byte>(SmallBufferSize);
                        buffer[0] = (byte)(threadId + i);
                    }
                });
            }
            Task.WaitAll(tasks);
        }

        [Benchmark]
        public void HighContention_MemoryPool()
        {
            var tasks = new Task[ParallelTaskCount];
            for (int t = 0; t < ParallelTaskCount; t++)
            {
                int threadId = t;
                tasks[t] = Task.Run(() =>
                {
                    // Very frequent allocations to create contention
                    for (int i = 0; i < AllocationsPerTask * 2; i++)
                    {
                        using var buffer = _memoryPool.Allocate<byte>(SmallBufferSize);
                        buffer[0] = (byte)(threadId + i);
                    }
                });
            }
            Task.WaitAll(tasks);
        }

        [Benchmark]
        public void HighContention_ThreadLocalPool()
        {
            var tasks = new Task[ParallelTaskCount];
            for (int t = 0; t < ParallelTaskCount; t++)
            {
                int threadId = t;
                tasks[t] = Task.Run(() =>
                {
                    // Very frequent allocations - should be fast with thread-local pools
                    for (int i = 0; i < AllocationsPerTask * 2; i++)
                    {
                        using var buffer = _threadLocalPool.Allocate<byte>(SmallBufferSize);
                        buffer[0] = (byte)(threadId + i);
                    }
                });
            }
            Task.WaitAll(tasks);
        }

        [Benchmark]
        public void HighContention_ThreadLocalPoolWithSharing()
        {
            var tasks = new Task[ParallelTaskCount];
            for (int t = 0; t < ParallelTaskCount; t++)
            {
                int threadId = t;
                tasks[t] = Task.Run(() =>
                {
                    // Very frequent allocations with cross-thread sharing enabled
                    for (int i = 0; i < AllocationsPerTask * 2; i++)
                    {
                        using var buffer = _threadLocalPoolWithSharing.Allocate<byte>(SmallBufferSize);
                        buffer[0] = (byte)(threadId + i);
                    }
                });
            }
            Task.WaitAll(tasks);
        }

        #endregion

        #region Memory Efficiency Benchmarks

        [Benchmark]
        public void MemoryEfficiency_SystemAllocator()
        {
            var buffers = new System.Collections.Generic.List<UnmanagedBuffer<byte>>();
            try
            {
                // Allocate many buffers and keep them alive
                for (int i = 0; i < 100; i++)
                {
                    var buffer = _systemAllocator.Allocate<byte>(ReuseBufferSize);
                    buffer[0] = (byte)i;
                    buffers.Add(buffer);
                }
            }
            finally
            {
                foreach (var buffer in buffers)
                {
                    buffer.Dispose();
                }
            }
        }

        [Benchmark]
        public void MemoryEfficiency_MemoryPool()
        {
            var buffers = new System.Collections.Generic.List<UnmanagedBuffer<byte>>();
            try
            {
                // Allocate many buffers and keep them alive
                for (int i = 0; i < 100; i++)
                {
                    var buffer = _memoryPool.Allocate<byte>(ReuseBufferSize);
                    buffer[0] = (byte)i;
                    buffers.Add(buffer);
                }
            }
            finally
            {
                foreach (var buffer in buffers)
                {
                    buffer.Dispose();
                }
            }
        }

        [Benchmark]
        public void MemoryEfficiency_ThreadLocalPool()
        {
            var buffers = new System.Collections.Generic.List<UnmanagedBuffer<byte>>();
            try
            {
                // Allocate many buffers and keep them alive
                for (int i = 0; i < 100; i++)
                {
                    var buffer = _threadLocalPool.Allocate<byte>(ReuseBufferSize);
                    buffer[0] = (byte)i;
                    buffers.Add(buffer);
                }
            }
            finally
            {
                foreach (var buffer in buffers)
                {
                    buffer.Dispose();
                }
            }
        }

        #endregion
    }
}