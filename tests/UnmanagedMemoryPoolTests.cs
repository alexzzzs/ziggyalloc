using System;
using System.Threading.Tasks;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class UnmanagedMemoryPoolTests
    {
        [Fact]
        public void UnmanagedMemoryPool_BasicAllocationAndReuse_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(baseAllocator);

            // Act - First allocation (should create new buffer)
            using var buffer1 = pool.Allocate<int>(100);
            var initialAllocatedBytes = pool.TotalAllocatedBytes;

            // Fill buffer with data
            for (int i = 0; i < buffer1.Length; i++)
            {
                buffer1[i] = i;
            }

            // Dispose first buffer (should return to pool)
            buffer1.Dispose();

            // Second allocation of same size (should reuse from pool)
            using var buffer2 = pool.Allocate<int>(100);
            var afterSecondAllocationBytes = pool.TotalAllocatedBytes;

            // Assert
            Assert.True(buffer2.IsValid);
            Assert.Equal(100, buffer2.Length);
            
            // Since we reused from pool, total allocated bytes should remain the same
            // (The pool tracks the total bytes ever allocated, not currently allocated)
            Assert.Equal(initialAllocatedBytes, afterSecondAllocationBytes);
            
            // Verify buffer is properly initialized (not containing old data)
            for (int i = 0; i < Math.Min(10, buffer2.Length); i++)
            {
                Assert.Equal(0, buffer2[i]); // Should be zero-initialized by default
            }
        }

        [Fact]
        public void UnmanagedMemoryPool_ZeroMemoryFlag_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(baseAllocator);

            // Act - Allocate with data
            using var buffer1 = pool.Allocate<int>(10);
            for (int i = 0; i < buffer1.Length; i++)
            {
                buffer1[i] = i + 100; // Fill with non-zero values
            }

            // Dispose to return to pool
            buffer1.Dispose();

            // Allocate same size with zeroMemory flag
            using var buffer2 = pool.Allocate<int>(10, zeroMemory: true);

            // Assert - Buffer should be zero-initialized
            for (int i = 0; i < buffer2.Length; i++)
            {
                Assert.Equal(0, buffer2[i]);
            }
        }

        [Fact]
        public void UnmanagedMemoryPool_DifferentSizes_HandledSeparately()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(baseAllocator);

            // Act - Allocate buffers of different sizes
            using var buffer10 = pool.Allocate<int>(10);
            using var buffer20 = pool.Allocate<int>(20);
            using var buffer30 = pool.Allocate<int>(30);

            var allocatedBytesAfterAll = pool.TotalAllocatedBytes;

            // Dispose all buffers
            buffer10.Dispose();
            buffer20.Dispose();
            buffer30.Dispose();

            // Allocate again with same sizes
            using var buffer10Again = pool.Allocate<int>(10);
            using var buffer20Again = pool.Allocate<int>(20);
            using var buffer30Again = pool.Allocate<int>(30);

            // Assert - Should have reused from respective pools
            Assert.Equal(10, buffer10Again.Length);
            Assert.Equal(20, buffer20Again.Length);
            Assert.Equal(30, buffer30Again.Length);
        }

        [Fact]
        public void UnmanagedMemoryPool_Clear_RemovesAllPooledBuffers()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(baseAllocator);

            // Allocate and dispose several buffers to fill the pool
            for (int i = 0; i < 5; i++)
            {
                using var buffer = pool.Allocate<int>(100);
                // Fill with data to make sure buffers are distinct
                for (int j = 0; j < buffer.Length; j++)
                {
                    buffer[j] = i * 1000 + j;
                }
            }

            var allocatedBytesBeforeClear = pool.TotalAllocatedBytes;

            // Act - Clear the pool
            pool.Clear();

            // Assert - All pooled memory should be freed
            Assert.Equal(0, pool.TotalAllocatedBytes);
        }

        [Fact]
        public async Task UnmanagedMemoryPool_ThreadSafety_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(baseAllocator);
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
                        using var buffer = pool.Allocate<int>(10 + threadId);
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
        public void UnmanagedMemoryPool_EmptyAllocation_HandledCorrectly()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(baseAllocator);

            // Act
            using var buffer = pool.Allocate<int>(0);

            // Assert
            Assert.True(buffer.IsEmpty);
            Assert.Equal(0, buffer.Length);
            Assert.False(buffer.IsValid); // Empty buffers should not be valid
        }

        [Fact]
        public void UnmanagedMemoryPool_NegativeSize_ThrowsException()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(baseAllocator);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => pool.Allocate<int>(-1));
        }

        [Fact]
        public void UnmanagedMemoryPool_LargeAllocation_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(baseAllocator);

            // Act
            const int largeSize = 100000; // 100K elements
            using var buffer = pool.Allocate<int>(largeSize);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(largeSize, buffer.Length);
            Assert.Equal(largeSize * sizeof(int), buffer.SizeInBytes);
        }
    }
}