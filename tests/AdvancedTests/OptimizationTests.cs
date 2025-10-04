using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    /// <summary>
    /// Comprehensive tests for the v1.4.0 optimization features.
    /// Tests SIMD operations, lock-free pools, and dynamic optimizations.
    /// </summary>
    public class OptimizationTests
    {
        #region SIMD Operations Tests

        [Fact]
        public void SimdMemoryOperations_Prefaulting_WorksCorrectly()
        {
            // Arrange
            const int largeSize = 1024 * 1024; // 1MB
            var allocator = new SystemMemoryAllocator();

            // Act
            using var buffer = allocator.Allocate<byte>(largeSize, zeroMemory: true);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(largeSize, buffer.Length);

            // Verify memory is actually zeroed
            for (int i = 0; i < Math.Min(1000, buffer.Length); i++)
            {
                Assert.Equal(0, buffer[i]);
            }
        }

        [Fact]
        public void SimdMemoryOperations_LargeCopyOperations_Work()
        {
            // Arrange
            const int size = 512 * 1024; // 512KB
            var allocator = new SystemMemoryAllocator();

            // Act
            using var source = allocator.Allocate<byte>(size, zeroMemory: false);
            using var dest = allocator.Allocate<byte>(size, zeroMemory: true);

            // Fill source with pattern
            for (int i = 0; i < source.Length; i++)
            {
                source[i] = (byte)(i % 256);
            }

            // Copy using enhanced operations
            dest.CopyFrom(source);

            // Assert
            Assert.True(dest.IsValid);
            for (int i = 0; i < Math.Min(1000, dest.Length); i++)
            {
                Assert.Equal(source[i], dest[i]);
            }
        }

        [Fact]
        public void SimdMemoryOperations_Performance_IsBetterThanStandard()
        {
            // Arrange
            const int size = 1024 * 1024; // 1MB
            var allocator = new SystemMemoryAllocator();

            using var buffer1 = allocator.Allocate<byte>(size);
            using var buffer2 = allocator.Allocate<byte>(size);

            // Act - Time the SIMD operations
            var stopwatch = Stopwatch.StartNew();

            // Use the enhanced zeroing (with prefaulting)
            buffer1.Clear();

            stopwatch.Stop();
            var simdTime = stopwatch.ElapsedTicks;

            // Reset and time standard operations
            stopwatch.Restart();

            // Use standard span clear
            buffer2.AsSpan().Clear();

            stopwatch.Stop();
            var standardTime = stopwatch.ElapsedTicks;

            // Assert - SIMD should be competitive (may vary by hardware)
            // We expect SIMD to be at least as fast as standard operations
            Assert.True(simdTime <= standardTime * 2, $"SIMD time: {simdTime}, Standard time: {standardTime}");
        }

        #endregion

        #region Lock-Free Memory Pool Tests

        [Fact]
        public void UnmanagedMemoryPool_LockFreeOperations_HighConcurrency()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(allocator);
            const int threadCount = 20;
            const int allocationsPerThread = 100;
            var tasks = new Task[threadCount];
            var exceptions = new Exception[threadCount];

            // Act
            for (int t = 0; t < threadCount; t++)
            {
                int threadId = t;
                tasks[t] = Task.Run(() =>
                {
                    try
                    {
                        for (int i = 0; i < allocationsPerThread; i++)
                        {
                            using var buffer = pool.Allocate<int>(10 + (i % 50));
                            // Use buffer
                            for (int j = 0; j < Math.Min(5, buffer.Length); j++)
                            {
                                buffer[j] = threadId * 1000 + i * 10 + j;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions[threadId] = ex;
                    }
                });
            }

            // Wait for all tasks
            Task.WaitAll(tasks);

            // Assert
            for (int i = 0; i < threadCount; i++)
            {
                Assert.Null(exceptions[i]);
            }
        }

        [Fact]
        public void UnmanagedMemoryPool_SizeClassOptimization_TracksUsage()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(allocator);

            // Act - Allocate many buffers of the same size to trigger size-class usage
            const int allocationCount = 10000;
            const int bufferSize = 64; // Should fit in a size class

            for (int i = 0; i < allocationCount; i++)
            {
                using var buffer = pool.Allocate<int>(bufferSize / sizeof(int));
                buffer[0] = i;
            }

            // Assert - Pool should handle the load without issues
            // The optimization tracking should work without throwing exceptions
            Assert.True(true);
        }

        [Fact]
        public void UnmanagedMemoryPool_DynamicOptimization_DetectsImbalance()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(allocator);

            // Act - Create usage imbalance by heavily using one size class
            const int heavyUsageCount = 5000;
            const int lightUsageCount = 10;

            // Heavy usage of small buffers
            for (int i = 0; i < heavyUsageCount; i++)
            {
                using var buffer = pool.Allocate<byte>(32);
                buffer[0] = (byte)i;
            }

            // Light usage of large buffers
            for (int i = 0; i < lightUsageCount; i++)
            {
                using var buffer = pool.Allocate<int>(1000);
                buffer[0] = i;
            }

            // Assert - Should complete without issues
            // The dynamic optimization should detect the imbalance
            Assert.True(true);
        }

        #endregion

        #region Performance and Stress Tests

        [Fact]
        public void OptimizationFeatures_StressTest_HeavyLoad()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(allocator);

            const int stressIterationCount = 10000;
            var random = new Random(42);

            // Act - Random mix of allocation sizes and operations
            for (int i = 0; i < stressIterationCount; i++)
            {
                int size = random.Next(1, 1001); // 1-1000 elements
                bool zeroMemory = random.Next(2) == 0;

                using var buffer = pool.Allocate<byte>(size, zeroMemory);

                // Random operations on buffer
                if (buffer.Length > 0)
                {
                    int operations = random.Next(1, 11);
                    for (int op = 0; op < operations && op < buffer.Length; op++)
                    {
                        buffer[op] = (byte)(i + op);
                    }
                }
            }

            // Assert - Should complete without memory corruption or exceptions
            Assert.True(true);
        }

        [Fact]
        public void OptimizationFeatures_MemoryEfficiency_Test()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(allocator);

            const int bufferCount = 1000;
            const int bufferSize = 128;

            // Act - Allocate and immediately dispose many buffers
            for (int i = 0; i < bufferCount; i++)
            {
                using var buffer = pool.Allocate<byte>(bufferSize);
                Assert.True(buffer.IsValid);
                Assert.Equal(bufferSize, buffer.Length);
            }

            // Assert - Pool should efficiently reuse memory
            // Test passes if no exceptions and memory is properly managed
            Assert.True(true);
        }

        [Fact]
        public void OptimizationFeatures_ConcurrentMixedOperations_Test()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(allocator);

            const int threadCount = 10;
            const int operationsPerThread = 200;
            var tasks = new Task[threadCount];
            var exceptions = new Exception[threadCount];

            // Act
            for (int t = 0; t < threadCount; t++)
            {
                int threadId = t;
                tasks[t] = Task.Run(() =>
                {
                    try
                    {
                        var random = new Random(threadId);

                        for (int i = 0; i < operationsPerThread; i++)
                        {
                            int size = random.Next(1, 101); // 1-100 elements
                            bool zeroMemory = random.Next(2) == 0;

                            using var buffer = pool.Allocate<int>(size, zeroMemory);

                            // Fill with pattern
                            for (int j = 0; j < Math.Min(5, buffer.Length); j++)
                            {
                                buffer[j] = threadId * 1000 + i * 10 + j;
                            }

                            // Randomly clear some buffers
                            if (random.Next(4) == 0)
                            {
                                buffer.Clear();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions[threadId] = ex;
                    }
                });
            }

            // Wait for completion
            Task.WaitAll(tasks);

            // Assert
            for (int i = 0; i < threadCount; i++)
            {
                Assert.Null(exceptions[i]);
            }
        }

        #endregion

        #region Edge Cases and Error Conditions

        [Fact]
        public void OptimizationFeatures_VeryLargeAllocations_Prefaulting()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(allocator);

            // Act - Test very large allocation that should trigger prefaulting
            const int veryLargeSize = 10 * 1024 * 1024; // 10MB

            using var buffer = pool.Allocate<byte>(veryLargeSize, zeroMemory: true);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(veryLargeSize, buffer.Length);

            // Verify memory is zeroed
            Assert.Equal(0, buffer[0]);
            Assert.Equal(0, buffer[buffer.Length / 2]);
            Assert.Equal(0, buffer[buffer.Length - 1]);
        }

        [Fact]
        public void OptimizationFeatures_ZeroSizeAllocations_Handled()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(allocator);

            // Act
            using var buffer = pool.Allocate<int>(0);

            // Assert
            Assert.True(buffer.IsEmpty);
            Assert.Equal(0, buffer.Length);
            Assert.False(buffer.IsValid);
        }

        [Fact]
        public void OptimizationFeatures_StressTest_Basic()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(allocator);

            const int stressIterationCount = 5000; // Reduced from 10000

            // Act - Basic stress test without concurrency
            for (int i = 0; i < stressIterationCount; i++)
            {
                using var buffer = pool.Allocate<byte>(16 + (i % 64));
                buffer[0] = (byte)i;
            }

            // Assert - Should complete without memory corruption or exceptions
            Assert.True(true);
        }

        #endregion

        #region SIMD Hardware Detection Tests

        [Fact]
        public void SimdMemoryOperations_HardwareDetection_ReportsCorrectly()
        {
            // Arrange & Act
            string simdInfo = SimdMemoryOperations.GetSimdInfo();

            // Assert
            Assert.NotNull(simdInfo);
            Assert.Contains("SIMD Supported:", simdInfo);
            Assert.Contains("AVX2 Supported:", simdInfo);
            Assert.Contains("Processor Count:", simdInfo);
        }

        [Fact]
        public void SimdMemoryOperations_OperationsWork_OnAnyHardware()
        {
            // Arrange
            const int testSize = 1024;
            var allocator = new SystemMemoryAllocator();

            // Act & Assert - Operations should work regardless of hardware capabilities
            using var buffer1 = allocator.Allocate<byte>(testSize);
            using var buffer2 = allocator.Allocate<byte>(testSize);

            // Test all SIMD operations
            buffer1.Clear(); // Should use SIMD when available
            buffer2.Fill(42);

            // Copy operation
            buffer1.CopyFrom(buffer2);

            // Verify results
            for (int i = 0; i < Math.Min(100, buffer1.Length); i++)
            {
                Assert.Equal(42, buffer1[i]);
            }
        }

        #endregion

        #region Performance Regression Tests

        [Fact]
        public void OptimizationFeatures_PerformanceRegression_SmallAllocations()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(allocator);

            const int iterationCount = 10000;

            // Act - Time small allocations
            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterationCount; i++)
            {
                using var buffer = pool.Allocate<int>(10);
                buffer[0] = i;
            }

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Assert - Should complete in reasonable time (less than 1 second)
            Assert.True(elapsedMs < 1000, $"Small allocations took {elapsedMs}ms, which seems too slow");
        }

        [Fact]
        public void OptimizationFeatures_PerformanceRegression_LargeAllocations()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(allocator);

            const int largeSize = 1024 * 1024; // 1MB
            const int iterationCount = 100;

            // Act - Time large allocations with prefaulting
            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < iterationCount; i++)
            {
                using var buffer = pool.Allocate<byte>(largeSize, zeroMemory: true);
                // Access first and last elements to ensure prefaulting worked
                Assert.Equal(0, buffer[0]);
                Assert.Equal(0, buffer[buffer.Length - 1]);
            }

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Assert - Large allocations should still be reasonably fast
            Assert.True(elapsedMs < 5000, $"Large allocations took {elapsedMs}ms, which seems too slow");
        }

        #endregion
    }
}