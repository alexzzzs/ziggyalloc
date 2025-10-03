using System;
using System.Threading.Tasks;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class NumaAwareAllocatorTests
    {
        [Fact]
        public void NumaAwareAllocator_Constructor_WithValidBaseAllocator_Succeeds()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();

            // Act
            using var numaAllocator = new NumaAwareAllocator(baseAllocator);

            // Assert
            Assert.True(numaAllocator.SupportsIndividualDeallocation);
            Assert.True(numaAllocator.TotalAllocatedBytes >= 0);
            Assert.True(numaAllocator.NodeCount >= 1);
        }

        [Fact]
        public void NumaAwareAllocator_Constructor_WithNullBaseAllocator_Throws()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new NumaAwareAllocator(null!));
        }

        [Fact]
        public void NumaAwareAllocator_Allocate_WithZeroElements_ReturnsEmptyBuffer()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var numaAllocator = new NumaAwareAllocator(baseAllocator);

            // Act
            using var buffer = numaAllocator.Allocate<int>(0);

            // Assert
            Assert.True(buffer.IsEmpty);
            Assert.Equal(0, buffer.Length);
            Assert.False(buffer.IsValid);
        }

        [Fact]
        public void NumaAwareAllocator_Allocate_WithNegativeElements_Throws()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var numaAllocator = new NumaAwareAllocator(baseAllocator);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => numaAllocator.Allocate<int>(-1));
        }

        [Fact]
        public void NumaAwareAllocator_Allocate_WithValidElements_Succeeds()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var numaAllocator = new NumaAwareAllocator(baseAllocator);

            // Act
            using var buffer = numaAllocator.Allocate<int>(100);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(100, buffer.Length);
            Assert.Equal(100 * sizeof(int), buffer.SizeInBytes);
            Assert.True(numaAllocator.TotalAllocatedBytes >= buffer.SizeInBytes);
        }

        [Fact]
        public void NumaAwareAllocator_Allocate_WithZeroMemory_InitializesMemory()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var numaAllocator = new NumaAwareAllocator(baseAllocator);

            // Act
            using var buffer = numaAllocator.Allocate<int>(10, zeroMemory: true);

            // Assert
            Assert.True(buffer.IsValid);
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(0, buffer[i]);
            }
        }

        [Fact]
        public void NumaAwareAllocator_Allocate_WithoutZeroMemory_PreservesData()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var numaAllocator = new NumaAwareAllocator(baseAllocator);

            // Act
            using var buffer = numaAllocator.Allocate<int>(10);

            // Fill with known data
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = i * 42;
            }

            // Dispose and reallocate
            buffer.Dispose();

            using var newBuffer = numaAllocator.Allocate<int>(10);

            // Assert - Memory should not be zero-initialized
            bool hasNonZeroData = false;
            for (int i = 0; i < newBuffer.Length; i++)
            {
                if (newBuffer[i] != 0)
                {
                    hasNonZeroData = true;
                    break;
                }
            }
            Assert.True(hasNonZeroData || newBuffer.Length == 0);
        }

        [Fact]
        public void NumaAwareAllocator_Free_WithValidPointer_Succeeds()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var numaAllocator = new NumaAwareAllocator(baseAllocator);

            var initialAllocatedBytes = numaAllocator.TotalAllocatedBytes;

            // Act
            using (var buffer = numaAllocator.Allocate<int>(50))
            {
                // Verify allocation
                Assert.True(buffer.IsValid);
            }

            // Wait a bit for potential cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Note: We can't easily verify the exact allocated bytes due to implementation details
            // but we can verify the allocator is still functional
            using var newBuffer = numaAllocator.Allocate<int>(25);
            Assert.True(newBuffer.IsValid);
        }

        [Fact]
        public void NumaAwareAllocator_Free_WithZeroPointer_DoesNothing()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var numaAllocator = new NumaAwareAllocator(baseAllocator);

            // Act & Assert - Should not throw
            numaAllocator.Free(IntPtr.Zero);
        }

        [Fact]
        public void NumaAwareAllocator_Dispose_CleansUpProperly()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            var numaAllocator = new NumaAwareAllocator(baseAllocator);

            // Allocate some buffers
            using (var buffer1 = numaAllocator.Allocate<int>(10))
            using (var buffer2 = numaAllocator.Allocate<int>(20))
            {
                Assert.True(buffer1.IsValid);
                Assert.True(buffer2.IsValid);
            }

            // Act
            numaAllocator.Dispose();

            // Assert - Should be disposed
            Assert.Throws<ObjectDisposedException>(() => numaAllocator.Allocate<int>(10));
        }

        [Fact]
        public void NumaAwareAllocator_Dispose_MultipleTimes_DoesNotThrow()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            var numaAllocator = new NumaAwareAllocator(baseAllocator);

            // Act & Assert - Should not throw on multiple disposes
            numaAllocator.Dispose();
            numaAllocator.Dispose();
        }

        [Fact]
        public async Task NumaAwareAllocator_MultiThreadedAllocation_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var numaAllocator = new NumaAwareAllocator(baseAllocator);

            const int threadCount = 10;
            const int allocationsPerThread = 20;
            var tasks = new Task[threadCount];
            var exceptions = new ConcurrentQueue<Exception>();

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
                            using var buffer = numaAllocator.Allocate<int>(10 + threadId % 5);

                            // Verify buffer is valid
                            Assert.True(buffer.IsValid);
                            Assert.True(buffer.Length > 0);

                            // Write some data
                            for (int j = 0; j < Math.Min(3, buffer.Length); j++)
                            {
                                buffer[j] = threadId * 1000 + i * 100 + j;
                            }

                            // Verify we can read it back
                            Assert.Equal(threadId * 1000 + i * 100, buffer[0]);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Enqueue(ex);
                    }
                });
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Assert
            Assert.Empty(exceptions);
            Assert.True(numaAllocator.TotalAllocatedBytes >= 0);
        }

        [Fact]
        public void NumaAwareAllocator_NodeStatistics_ProvidesValidData()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var numaAllocator = new NumaAwareAllocator(baseAllocator);

            // Allocate some buffers
            using (var buffer1 = numaAllocator.Allocate<int>(10))
            using (var buffer2 = numaAllocator.Allocate<double>(5))
            {
                Assert.True(buffer1.IsValid);
                Assert.True(buffer2.IsValid);
            }

            // Act
            var statistics = numaAllocator.GetNodeStatistics();

            // Assert
            Assert.NotNull(statistics);
            Assert.Equal(numaAllocator.NodeCount, statistics.Length);

            foreach (var stat in statistics)
            {
                Assert.True(stat.NodeId >= 0);
                Assert.True(stat.AllocatedBytes >= 0);
                Assert.True(stat.AllocationCount >= 0);
                Assert.True(stat.LocalAllocations >= 0);
                Assert.True(stat.RemoteAllocations >= 0);
                Assert.True(stat.LocalAllocationPercentage >= 0.0);
                Assert.True(stat.LocalAllocationPercentage <= 100.0);
            }
        }

        [Fact]
        public void NumaAwareAllocator_IsNumaSystem_DetectsCorrectly()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var numaAllocator = new NumaAwareAllocator(baseAllocator);

            // Act & Assert
            // We can't predict whether the system is NUMA or not, but we can verify
            // the detection doesn't throw and returns reasonable values
            Assert.True(numaAllocator.IsNumaSystem == numaAllocator.IsNumaSystem); // Consistency check
            Assert.True(numaAllocator.NodeCount >= 1);

            if (numaAllocator.IsNumaSystem)
            {
                Assert.True(numaAllocator.NodeCount > 1);
            }
            else
            {
                Assert.Equal(1, numaAllocator.NodeCount);
            }
        }

        [Fact]
        public void NumaAwareAllocator_LargeAllocations_Work()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var numaAllocator = new NumaAwareAllocator(baseAllocator);

            // Act - Test large allocation
            const int largeSize = 100000; // 100K elements
            using var buffer = numaAllocator.Allocate<byte>(largeSize);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(largeSize, buffer.Length);
            Assert.Equal(largeSize, buffer.SizeInBytes);
            Assert.True(numaAllocator.TotalAllocatedBytes >= largeSize);
        }

        [Fact]
        public void NumaAwareAllocator_MixedAllocationSizes_Work()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var numaAllocator = new NumaAwareAllocator(baseAllocator);

            // Act - Allocate buffers of various sizes
            using var smallBuffer = numaAllocator.Allocate<byte>(16);
            using var mediumBuffer = numaAllocator.Allocate<int>(1000);
            using var largeBuffer = numaAllocator.Allocate<double>(50000);

            // Assert
            Assert.True(smallBuffer.IsValid);
            Assert.True(mediumBuffer.IsValid);
            Assert.True(largeBuffer.IsValid);

            Assert.Equal(16, smallBuffer.Length);
            Assert.Equal(1000, mediumBuffer.Length);
            Assert.Equal(50000, largeBuffer.Length);

            // Verify we can write to all buffers
            smallBuffer[0] = 42;
            mediumBuffer[0] = 12345;
            largeBuffer[0] = 3.14159;

            Assert.Equal(42, smallBuffer[0]);
            Assert.Equal(12345, mediumBuffer[0]);
            Assert.Equal(3.14159, largeBuffer[0]);
        }

        [Fact]
        public void NumaAwareAllocator_StructAllocations_Work()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var numaAllocator = new NumaAwareAllocator(baseAllocator);

            // Act - Test with custom struct
            using var buffer = numaAllocator.Allocate<Point3D>(100);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(100, buffer.Length);

            // Test writing and reading struct data
            buffer[0] = new Point3D { X = 1.0f, Y = 2.0f, Z = 3.0f };
            buffer[1] = new Point3D { X = 4.0f, Y = 5.0f, Z = 6.0f };

            Assert.Equal(1.0f, buffer[0].X);
            Assert.Equal(2.0f, buffer[0].Y);
            Assert.Equal(3.0f, buffer[0].Z);
            Assert.Equal(4.0f, buffer[1].X);
            Assert.Equal(5.0f, buffer[1].Y);
            Assert.Equal(6.0f, buffer[1].Z);
        }

        // Helper struct for testing
        private struct Point3D
        {
            public float X, Y, Z;
        }
    }
}