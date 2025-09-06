using System;
using System.Threading.Tasks;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class SlabAllocatorTests
    {
        [Fact]
        public void SlabAllocator_BasicAllocation_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var slabAllocator = new SlabAllocator(baseAllocator);

            // Act
            using var buffer = slabAllocator.Allocate<int>(100);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(100, buffer.Length);
            Assert.Equal(400, buffer.SizeInBytes); // 100 * 4 bytes per int
        }

        [Fact]
        public void SlabAllocator_LargeAllocation_DelegatesToBase()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var slabAllocator = new SlabAllocator(baseAllocator);

            // Act - Allocate something larger than MaxSlabAllocationSize
            using var buffer = slabAllocator.Allocate<int>(2000); // 8000 bytes > 4096

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(2000, buffer.Length);
        }

        [Fact]
        public void SlabAllocator_ZeroMemoryFlag_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var slabAllocator = new SlabAllocator(baseAllocator);

            // Act - Allocate with zero memory flag
            using var buffer = slabAllocator.Allocate<int>(100, true);

            // Assert - All elements should be zero
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(0, buffer[i]);
            }
        }

        [Fact]
        public void SlabAllocator_BufferOperationsWork()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var slabAllocator = new SlabAllocator(baseAllocator);
            using var buffer = slabAllocator.Allocate<int>(10);

            // Act - Fill buffer with values
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = i * 2;
            }

            // Assert - Verify values
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(i * 2, buffer[i]);
            }

            // Test Span conversion
            var span = buffer.AsSpan();
            Assert.Equal(10, span.Length);
        }

        [Fact]
        public void SlabAllocator_Dispose_WorksCorrectly()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            var slabAllocator = new SlabAllocator(baseAllocator);

            // Act & Assert - Should not throw when disposing
            slabAllocator.Dispose();
            
            // After disposal, allocating should throw ObjectDisposedException
            Assert.Throws<ObjectDisposedException>(() => slabAllocator.Allocate<int>(10));
        }

        [Fact]
        public void SlabAllocator_TotalAllocatedBytes_TracksCorrectly()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var slabAllocator = new SlabAllocator(baseAllocator);
            long initialBytes = slabAllocator.TotalAllocatedBytes;

            // Act
            using var buffer1 = slabAllocator.Allocate<int>(100); // 400 bytes
            using var buffer2 = slabAllocator.Allocate<byte>(200); // 200 bytes
            
            // Assert
            Assert.Equal(initialBytes + 600, slabAllocator.TotalAllocatedBytes);
        }

        [Fact]
        public async Task SlabAllocator_ThreadSafety_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var slabAllocator = new SlabAllocator(baseAllocator);
            const int threadCount = 10;
            const int allocationsPerThread = 100;
            var tasks = new Task[threadCount];

            // Act - Run allocations in parallel
            for (int t = 0; t < threadCount; t++)
            {
                int threadId = t;
                tasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < allocationsPerThread; i++)
                    {
                        using var buffer = slabAllocator.Allocate<int>(10 + threadId);
                        // Do some work with the buffer
                        for (int j = 0; j < Math.Min(5, buffer.Length); j++)
                        {
                            buffer[j] = threadId * 1000 + i * 100 + j;
                        }
                    }
                });
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Assert - No exceptions should have been thrown
            // The test passes if we reach this point without exceptions
            Assert.True(true);
        }

        [Fact]
        public void SlabAllocator_EmptyAllocation_HandledCorrectly()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var slabAllocator = new SlabAllocator(baseAllocator);

            // Act
            using var buffer = slabAllocator.Allocate<int>(0);

            // Assert
            Assert.True(buffer.IsEmpty);
            Assert.Equal(0, buffer.Length);
            Assert.False(buffer.IsValid); // Empty buffers should not be valid
        }

        [Fact]
        public void SlabAllocator_NegativeSize_ThrowsException()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var slabAllocator = new SlabAllocator(baseAllocator);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => slabAllocator.Allocate<int>(-1));
        }

        [Fact]
        public void SlabAllocator_LargeAllocation_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var slabAllocator = new SlabAllocator(baseAllocator);

            // Act
            const int largeSize = 100000; // 100K elements
            using var buffer = slabAllocator.Allocate<int>(largeSize);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(largeSize, buffer.Length);
            Assert.Equal(largeSize * sizeof(int), buffer.SizeInBytes);
        }
    }
}