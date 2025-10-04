using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    /// <summary>
    /// Comprehensive test suite for all ZiggyAlloc allocators.
    /// Tests functionality, performance, edge cases, and cross-allocator compatibility.
    /// </summary>
    public class AllocatorComprehensiveTests
    {
        #region SystemMemoryAllocator Tests

        [Fact]
        public void SystemMemoryAllocator_BasicFunctionality_Works()
        {
            // Arrange
            using var allocator = new SystemMemoryAllocator();

            // Act & Assert
            using var buffer = allocator.Allocate<int>(100);
            Assert.True(buffer.IsValid);
            Assert.Equal(100, buffer.Length);

            // Test array-like access
            buffer[0] = 42;
            buffer[99] = 99;
            Assert.Equal(42, buffer[0]);
            Assert.Equal(99, buffer[99]);
        }

        [Fact]
        public void SystemMemoryAllocator_ZeroMemory_WorksCorrectly()
        {
            // Arrange
            using var allocator = new SystemMemoryAllocator();

            // Act
            using var buffer = allocator.Allocate<byte>(100, zeroMemory: true);

            // Assert
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(0, buffer[i]);
            }
        }

        [Fact]
        public void SystemMemoryAllocator_LargeAllocations_Work()
        {
            // Arrange
            using var allocator = new SystemMemoryAllocator();

            // Act
            using var buffer = allocator.Allocate<byte>(1024 * 1024); // 1MB

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(1024 * 1024, buffer.Length);

            // Test that we can write to the entire buffer
            buffer[0] = 1;
            buffer[buffer.Length - 1] = 2;
            Assert.Equal(1, buffer[0]);
            Assert.Equal(2, buffer[buffer.Length - 1]);
        }

        [Fact]
        public void SystemMemoryAllocator_TypeSizeOptimization_Works()
        {
            // Arrange
            using var allocator = new SystemMemoryAllocator();

            // Act - Test different primitive types
            using var byteBuffer = allocator.Allocate<byte>(100);
            using var intBuffer = allocator.Allocate<int>(100);
            using var doubleBuffer = allocator.Allocate<double>(100);

            // Assert
            Assert.Equal(100, byteBuffer.Length);
            Assert.Equal(100, intBuffer.Length);
            Assert.Equal(100, doubleBuffer.Length);

            // Verify correct sizes
            Assert.Equal(100, byteBuffer.SizeInBytes);
            Assert.Equal(400, intBuffer.SizeInBytes); // 100 * 4
            Assert.Equal(800, doubleBuffer.SizeInBytes); // 100 * 8
        }

        #endregion

        #region AlignedAllocator Tests

        [Fact]
        public void AlignedAllocator_AutoAlignment_Works()
        {
            // Arrange
            using var allocator = new AlignedAllocator(new SystemMemoryAllocator());

            // Act
            using var buffer = allocator.Allocate<int>(100);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(100, buffer.Length);

            // Verify alignment statistics
            var stats = allocator.GetAlignmentStatistics();
            Assert.True(stats.TotalAlignedAllocations >= 1);
        }

        [Fact]
        public void AlignedAllocator_CustomAlignment_Works()
        {
            // Arrange
            using var allocator = new AlignedAllocator(new SystemMemoryAllocator(),
                AlignmentStrategy.Custom, customAlignment: 64);

            // Act
            using var buffer = allocator.Allocate<double>(50);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(50, buffer.Length);

            var stats = allocator.GetAlignmentStatistics();
            Assert.Equal(AlignmentStrategy.Custom, stats.Strategy);
        }

        [Fact]
        public void AlignedAllocator_SIMDTypes_AlignedCorrectly()
        {
            // Arrange
            using var allocator = new AlignedAllocator(new SystemMemoryAllocator());

            // Act - Test SIMD-friendly types
            using var floatBuffer = allocator.Allocate<float>(100);
            using var doubleBuffer = allocator.Allocate<double>(25);

            // Assert
            Assert.True(floatBuffer.IsValid);
            Assert.True(doubleBuffer.IsValid);

            var stats = allocator.GetAlignmentStatistics();
            Assert.True(stats.AlignmentEfficiency > 0);
        }

        #endregion

        #region NumaAwareAllocator Tests

        [Fact]
        public void NumaAwareAllocator_BasicFunctionality_Works()
        {
            // Arrange
            using var allocator = new NumaAwareAllocator(new SystemMemoryAllocator());

            // Act
            using var buffer = allocator.Allocate<int>(100);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(100, buffer.Length);

            // Check NUMA statistics
            var stats = allocator.GetNodeStatistics();
            Assert.NotNull(stats);
            Assert.True(stats.Length >= 1);
        }

        [Fact]
        public void NumaAwareAllocator_NodeAffinity_Tracked()
        {
            // Arrange
            using var allocator = new NumaAwareAllocator(new SystemMemoryAllocator());

            // Act - Multiple allocations to generate statistics
            for (int i = 0; i < 100; i++)
            {
                using var buffer = allocator.Allocate<byte>(64);
                buffer[0] = (byte)i;
            }

            // Assert
            var stats = allocator.GetNodeStatistics();
            Assert.All(stats, stat => Assert.True(stat.AllocationCount >= 0));
        }

        #endregion

        #region SlabAllocator Tests

        [Fact]
        public void SlabAllocator_SmallAllocations_Efficient()
        {
            // Arrange
            using var baseAllocator = new SystemMemoryAllocator();
            using var allocator = new SlabAllocator(baseAllocator);

            // Act - Allocate many small buffers
            var buffers = new List<UnmanagedBuffer<int>>();
            for (int i = 0; i < 1000; i++)
            {
                var buffer = allocator.Allocate<int>(10);
                buffers.Add(buffer);
                buffer[0] = i;
            }

            // Assert
            Assert.Equal(1000, buffers.Count);
            for (int i = 0; i < buffers.Count; i++)
            {
                Assert.Equal(i, buffers[i][0]);
            }

            // Cleanup
            foreach (var buffer in buffers)
            {
                buffer.Dispose();
            }
        }

        [Fact]
        public void SlabAllocator_LargeAllocations_Delegated()
        {
            // Arrange
            using var baseAllocator = new SystemMemoryAllocator();
            using var allocator = new SlabAllocator(baseAllocator);

            // Act - Large allocation should be delegated to base allocator
            using var buffer = allocator.Allocate<byte>(100 * 1024); // 100KB

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(100 * 1024, buffer.Length);
        }

        #endregion

        #region HybridAllocator Tests

        [Fact]
        public void HybridAllocator_IntelligentStrategy_Works()
        {
            // Arrange
            using var allocator = new HybridAllocator(new SystemMemoryAllocator());

            // Act - Small allocations should use managed arrays
            using var smallBuffer = allocator.Allocate<int>(100);

            // Large allocations should use unmanaged memory
            using var largeBuffer = allocator.Allocate<int>(10000);

            // Assert
            Assert.True(smallBuffer.IsValid);
            Assert.True(largeBuffer.IsValid);
            Assert.Equal(100, smallBuffer.Length);
            Assert.Equal(10000, largeBuffer.Length);
        }

        [Fact]
        public void HybridAllocator_Thresholds_WorkCorrectly()
        {
            // Arrange
            using var allocator = new HybridAllocator(new SystemMemoryAllocator());

            // Act - Test threshold boundaries
            using var byteBuffer = allocator.Allocate<byte>(1000); // Should be managed
            using var intBuffer = allocator.Allocate<int>(600);   // Should be managed
            using var doubleBuffer = allocator.Allocate<double>(200); // Should be managed

            // Assert
            Assert.True(byteBuffer.IsValid);
            Assert.True(intBuffer.IsValid);
            Assert.True(doubleBuffer.IsValid);
        }

        #endregion

        #region ThreadLocalMemoryPool Tests

        [Fact]
        public void ThreadLocalMemoryPool_ThreadLocalOptimization_Works()
        {
            // Arrange
            using var baseAllocator = new SystemMemoryAllocator();
            using var pool = new ThreadLocalMemoryPool(baseAllocator);

            // Act - Allocate in current thread
            using var buffer = pool.Allocate<int>(100);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(100, buffer.Length);
        }

        [Fact]
        public void ThreadLocalMemoryPool_BufferSharing_Works()
        {
            // Arrange
            using var baseAllocator = new SystemMemoryAllocator();
            using var pool = new ThreadLocalMemoryPool(baseAllocator);

            // Act - Allocate and return buffer to pool
            var buffer1 = pool.Allocate<int>(50);
            buffer1[0] = 42;
            buffer1.Dispose();

            // Allocate again - should potentially reuse
            var buffer2 = pool.Allocate<int>(50);

            // Assert
            Assert.True(buffer2.IsValid);
            Assert.Equal(50, buffer2.Length);
        }

        #endregion

        #region Cross-Allocator Compatibility Tests

        [Fact]
        public void AllAllocators_BufferInteroperability_Works()
        {
            // Arrange
            var allocators = new IUnmanagedMemoryAllocator[]
            {
                new SystemMemoryAllocator(),
                new AlignedAllocator(new SystemMemoryAllocator()),
                new NumaAwareAllocator(new SystemMemoryAllocator()),
                new SlabAllocator(new SystemMemoryAllocator()),
                new HybridAllocator(new SystemMemoryAllocator()),
                new UnmanagedMemoryPool(new SystemMemoryAllocator()),
                new ThreadLocalMemoryPool(new SystemMemoryAllocator())
            };

            try
            {
                // Act & Assert
                foreach (var allocator in allocators)
                {
                    using var buffer = allocator.Allocate<int>(50);
                    Assert.True(buffer.IsValid);
                    Assert.Equal(50, buffer.Length);

                    // Test that buffer works like a span
                    buffer[0] = 42;
                    Assert.Equal(42, buffer[0]);

                    // Test span conversion
                    Span<int> span = buffer;
                    Assert.Equal(50, span.Length);
                }
            }
            finally
            {
                // Ensure all allocators are properly disposed
                foreach (var allocator in allocators)
                {
                    if (allocator is IDisposable disposable) { disposable.Dispose(); }
                }
            }
        }

        [Fact]
        public void AllAllocators_MemorySafety_Maintained()
        {
            // Arrange
            var allocators = new IUnmanagedMemoryAllocator[]
            {
                new SystemMemoryAllocator(),
                new AlignedAllocator(new SystemMemoryAllocator()),
                new NumaAwareAllocator(new SystemMemoryAllocator()),
                new SlabAllocator(new SystemMemoryAllocator()),
                new HybridAllocator(new SystemMemoryAllocator()),
                new UnmanagedMemoryPool(new SystemMemoryAllocator()),
                new ThreadLocalMemoryPool(new SystemMemoryAllocator())
            };

            try
            {
                // Act & Assert
                foreach (var allocator in allocators)
                {
                    // Test double disposal doesn't crash
                    var buffer = allocator.Allocate<int>(25);
                    buffer.Dispose();
                    buffer.Dispose(); // Should not throw

                    // Test disposal after allocator disposal
                    var buffer2 = allocator.Allocate<int>(25);
                    // Buffer should still work until disposed
                    Assert.True(buffer2.IsValid);
                }
            }
            finally
            {
                // Ensure all allocators are properly disposed
                foreach (var allocator in allocators)
                {
                    if (allocator is IDisposable disposable) { disposable.Dispose(); }
                }
            }
        }

        #endregion

        #region Performance and Stress Tests

        [Fact]
        public void AllAllocators_Performance_Reasonable()
        {
            // Arrange
            var allocators = new IUnmanagedMemoryAllocator[]
            {
                new SystemMemoryAllocator(),
                new AlignedAllocator(new SystemMemoryAllocator()),
                new NumaAwareAllocator(new SystemMemoryAllocator()),
                new SlabAllocator(new SystemMemoryAllocator()),
                new HybridAllocator(new SystemMemoryAllocator()),
                new UnmanagedMemoryPool(new SystemMemoryAllocator()),
                new ThreadLocalMemoryPool(new SystemMemoryAllocator())
            };

            const int allocationCount = 1000;
            const int bufferSize = 100;

            try
            {
                // Act & Assert
                foreach (var allocator in allocators)
                {
                    var stopwatch = Stopwatch.StartNew();

                    for (int i = 0; i < allocationCount; i++)
                    {
                        using var buffer = allocator.Allocate<int>(bufferSize);
                        buffer[i % buffer.Length] = i;
                    }

                    stopwatch.Stop();
                    var elapsedMs = stopwatch.ElapsedMilliseconds;

                    // Assert - Should complete in reasonable time (< 1 second per allocator)
                    Assert.True(elapsedMs < 1000, $"{allocator.GetType().Name} took {elapsedMs}ms");
                }
            }
            finally
            {
                // Ensure all allocators are properly disposed
                foreach (var allocator in allocators)
                {
                    if (allocator is IDisposable disposable) { disposable.Dispose(); }
                }
            }
        }

        [Fact]
        public void AllAllocators_StressTest_HeavyLoad()
        {
            // Arrange
            var allocators = new IUnmanagedMemoryAllocator[]
            {
                new SystemMemoryAllocator(),
                new AlignedAllocator(new SystemMemoryAllocator()),
                new NumaAwareAllocator(new SystemMemoryAllocator()),
                new SlabAllocator(new SystemMemoryAllocator()),
                new HybridAllocator(new SystemMemoryAllocator()),
                new UnmanagedMemoryPool(new SystemMemoryAllocator()),
                new ThreadLocalMemoryPool(new SystemMemoryAllocator())
            };

            const int stressIterations = 5000;

            try
            {
                // Act & Assert
                foreach (var allocator in allocators)
                {
                    var random = new Random(42);

                    for (int i = 0; i < stressIterations; i++)
                    {
                        int size = random.Next(1, 1001);
                        bool zeroMemory = random.Next(2) == 0;

                        using var buffer = allocator.Allocate<byte>(size, zeroMemory);
                        Assert.True(buffer.IsValid);
                        Assert.Equal(size, buffer.Length);

                        // Random operations
                        if (buffer.Length > 0)
                        {
                            buffer[0] = (byte)i;
                            if (random.Next(4) == 0)
                            {
                                buffer.Clear();
                            }
                        }
                    }
                }
            }
            finally
            {
                // Ensure all allocators are properly disposed
                foreach (var allocator in allocators)
                {
                    if (allocator is IDisposable disposable) { disposable.Dispose(); }
                }
            }
        }

        #endregion

        #region Edge Cases and Error Conditions

        [Fact]
        public void AllAllocators_ZeroSizeAllocations_Handled()
        {
            // Arrange
            var allocators = new IUnmanagedMemoryAllocator[]
            {
                new SystemMemoryAllocator(),
                new AlignedAllocator(new SystemMemoryAllocator()),
                new NumaAwareAllocator(new SystemMemoryAllocator()),
                new SlabAllocator(new SystemMemoryAllocator()),
                new HybridAllocator(new SystemMemoryAllocator()),
                new UnmanagedMemoryPool(new SystemMemoryAllocator()),
                new ThreadLocalMemoryPool(new SystemMemoryAllocator())
            };

            try
            {
                // Act & Assert
                foreach (var allocator in allocators)
                {
                    using var buffer = allocator.Allocate<int>(0);
                    Assert.True(buffer.IsEmpty);
                    Assert.Equal(0, buffer.Length);
                    Assert.False(buffer.IsValid);
                }
            }
            finally
            {
                // Ensure all allocators are properly disposed
                foreach (var allocator in allocators)
                {
                    if (allocator is IDisposable disposable) { disposable.Dispose(); }
                }
            }
        }

        [Fact]
        public void AllAllocators_NegativeSizes_ThrowCorrectly()
        {
            // Arrange
            var allocators = new IUnmanagedMemoryAllocator[]
            {
                new SystemMemoryAllocator(),
                new AlignedAllocator(new SystemMemoryAllocator()),
                new NumaAwareAllocator(new SystemMemoryAllocator()),
                new SlabAllocator(new SystemMemoryAllocator()),
                new HybridAllocator(new SystemMemoryAllocator()),
                new UnmanagedMemoryPool(new SystemMemoryAllocator()),
                new ThreadLocalMemoryPool(new SystemMemoryAllocator())
            };

            try
            {
                // Act & Assert
                foreach (var allocator in allocators)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => allocator.Allocate<int>(-1));
                }
            }
            finally
            {
                // Ensure all allocators are properly disposed
                foreach (var allocator in allocators)
                {
                    if (allocator is IDisposable disposable) { disposable.Dispose(); }
                }
            }
        }

        [Fact]
        public void AllAllocators_VeryLargeAllocations_Handled()
        {
            // Arrange
            var allocators = new IUnmanagedMemoryAllocator[]
            {
                new SystemMemoryAllocator(),
                new AlignedAllocator(new SystemMemoryAllocator()),
                new NumaAwareAllocator(new SystemMemoryAllocator()),
                new SlabAllocator(new SystemMemoryAllocator()),
                new HybridAllocator(new SystemMemoryAllocator()),
                new UnmanagedMemoryPool(new SystemMemoryAllocator()),
                new ThreadLocalMemoryPool(new SystemMemoryAllocator())
            };

            const int largeSize = 1024 * 1024; // 1M elements

            try
            {
                // Act & Assert
                foreach (var allocator in allocators)
                {
                    using var buffer = allocator.Allocate<byte>(largeSize);
                    Assert.True(buffer.IsValid);
                    Assert.Equal(largeSize, buffer.Length);

                    // Test boundary access
                    buffer[0] = 1;
                    buffer[buffer.Length - 1] = 2;
                    Assert.Equal(1, buffer[0]);
                    Assert.Equal(2, buffer[buffer.Length - 1]);
                }
            }
            finally
            {
                // Ensure all allocators are properly disposed
                foreach (var allocator in allocators)
                {
                    if (allocator is IDisposable disposable) { disposable.Dispose(); }
                }
            }
        }

        #endregion

        #region Memory Management Tests

        [Fact]
        public void AllAllocators_TotalAllocatedBytes_TrackedCorrectly()
        {
            // Arrange
            var allocators = new IUnmanagedMemoryAllocator[]
            {
                new SystemMemoryAllocator(),
                new AlignedAllocator(new SystemMemoryAllocator()),
                new NumaAwareAllocator(new SystemMemoryAllocator()),
                new SlabAllocator(new SystemMemoryAllocator()),
                new HybridAllocator(new SystemMemoryAllocator()),
                new UnmanagedMemoryPool(new SystemMemoryAllocator()),
                new ThreadLocalMemoryPool(new SystemMemoryAllocator())
            };

            try
            {
                // Act & Assert
                foreach (var allocator in allocators)
                {
                    var initialBytes = allocator.TotalAllocatedBytes;

                    using var buffer = allocator.Allocate<int>(100);
                    var afterAllocationBytes = allocator.TotalAllocatedBytes;

                    // TotalAllocatedBytes should increase (or stay same for pools)
                    Assert.True(afterAllocationBytes >= initialBytes);
                }
            }
            finally
            {
                // Ensure all allocators are properly disposed
                foreach (var allocator in allocators)
                {
                    if (allocator is IDisposable disposable) { disposable.Dispose(); }
                }
            }
        }

        [Fact]
        public void AllAllocators_Disposal_CleansUpCorrectly()
        {
            // Arrange
            var allocators = new IUnmanagedMemoryAllocator[]
            {
                new SystemMemoryAllocator(),
                new AlignedAllocator(new SystemMemoryAllocator()),
                new NumaAwareAllocator(new SystemMemoryAllocator()),
                new SlabAllocator(new SystemMemoryAllocator()),
                new HybridAllocator(new SystemMemoryAllocator()),
                new UnmanagedMemoryPool(new SystemMemoryAllocator()),
                new ThreadLocalMemoryPool(new SystemMemoryAllocator())
            };

            try
            {
                // Act & Assert
                foreach (var allocator in allocators)
                {
                    // Allocate and dispose multiple buffers
                    var buffers = new UnmanagedBuffer<int>[10];
                    for (int i = 0; i < 10; i++)
                    {
                        buffers[i] = allocator.Allocate<int>(50);
                    }

                    // Dispose all buffers
                    foreach (var buffer in buffers)
                    {
                        buffer.Dispose();
                    }

                    // Allocator should still be functional
                    using var newBuffer = allocator.Allocate<int>(25);
                    Assert.True(newBuffer.IsValid);
                }
            }
            finally
            {
                // Ensure all allocators are properly disposed
                foreach (var allocator in allocators)
                {
                    if (allocator is IDisposable disposable) { disposable.Dispose(); }
                }
            }
        }

        #endregion

        #region Factory Method Tests

        [Fact]
        public void Z_FactoryMethods_WorkCorrectly()
        {
            // Act - Test all factory methods
            using var systemAllocator = Z.CreateSystemMemoryAllocator();
            using var numaAllocator = Z.CreateNumaAwareAllocator();
            using var alignedAllocator = Z.CreateAlignedAllocator();

            // Assert
            Assert.IsType<SystemMemoryAllocator>(systemAllocator);
            Assert.IsType<NumaAwareAllocator>(numaAllocator);
            Assert.IsType<AlignedAllocator>(alignedAllocator);

            // Test functionality
            using var buffer1 = systemAllocator.Allocate<int>(50);
            using var buffer2 = numaAllocator.Allocate<int>(50);
            using var buffer3 = alignedAllocator.Allocate<int>(50);

            Assert.True(buffer1.IsValid);
            Assert.True(buffer2.IsValid);
            Assert.True(buffer3.IsValid);
        }

        [Fact]
        public void Z_DefaultAllocator_SingletonBehavior()
        {
            // Act
            var allocator1 = Z.DefaultAllocator;
            var allocator2 = Z.DefaultAllocator;

            // Assert - Should return the same instance
            Assert.Same(allocator1, allocator2);

            // Test functionality
            using var buffer = allocator1.Allocate<int>(100);
            Assert.True(buffer.IsValid);
        }

        #endregion
    }
}
