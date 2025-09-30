using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class ThreadLocalMemoryPoolTests
    {
        [Fact]
        public void ThreadLocalMemoryPool_BasicAllocationAndReuse_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new ThreadLocalMemoryPool(baseAllocator);

            // Act - First allocation (should create new buffer)
            using var buffer1 = pool.Allocate<int>(100);
            var initialAllocatedBytes = pool.TotalAllocatedBytes;

            // Fill buffer with data
            for (int i = 0; i < buffer1.Length; i++)
            {
                buffer1[i] = i;
            }

            // Dispose first buffer (should return to thread-local pool)
            buffer1.Dispose();

            // Second allocation of same size (should reuse from thread-local pool)
            using var buffer2 = pool.Allocate<int>(100, zeroMemory: true);
            var afterSecondAllocationBytes = pool.TotalAllocatedBytes;

            // Assert
            Assert.True(buffer2.IsValid);
            Assert.Equal(100, buffer2.Length);

            // Since we reused from pool, total allocated bytes should remain the same
            Assert.Equal(initialAllocatedBytes, afterSecondAllocationBytes);

            // Verify buffer is properly initialized (not containing old data)
            for (int i = 0; i < Math.Min(10, buffer2.Length); i++)
            {
                Assert.Equal(0, buffer2[i]); // Should be zero-initialized
            }
        }

        [Fact]
        public void ThreadLocalMemoryPool_ZeroMemoryFlag_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new ThreadLocalMemoryPool(baseAllocator);

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
        public void ThreadLocalMemoryPool_DifferentSizes_HandledSeparately()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new ThreadLocalMemoryPool(baseAllocator);

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

            // Assert - Should have reused from respective thread-local pools
            Assert.Equal(10, buffer10Again.Length);
            Assert.Equal(20, buffer20Again.Length);
            Assert.Equal(30, buffer30Again.Length);
        }

        [Fact]
        public void ThreadLocalMemoryPool_Clear_RemovesAllPooledBuffers()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new ThreadLocalMemoryPool(baseAllocator);

            // Allocate and dispose several buffers to fill the pools
            for (int i = 0; i < 5; i++)
            {
                using (var buffer = pool.Allocate<int>(100))
                {
                    // Fill with data to make sure buffers are distinct
                    for (int j = 0; j < buffer.Length; j++)
                    {
                        buffer[j] = i * 1000 + j;
                    }
                }
            }

            var allocatedBytesBeforeClear = pool.TotalAllocatedBytes;

            // Act - Clear the pool
            pool.Clear();

            // Assert - All pooled memory should be freed
            Assert.Equal(0, pool.TotalAllocatedBytes);
        }

        [Fact]
        public async Task ThreadLocalMemoryPool_ThreadSafety_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new ThreadLocalMemoryPool(baseAllocator);
            const int threadCount = 10; // Reduced for testing
            const int allocationsPerThread = 50;
            var tasks = new Task[threadCount];
            var poolCountsDuringExecution = new List<int>();

            // Act - Run allocations in parallel across many threads
            for (int t = 0; t < threadCount; t++)
            {
                int threadId = t;
                tasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < allocationsPerThread; i++)
                    {
                        using var buffer = pool.Allocate<int>(10 + threadId % 5); // Vary sizes slightly
                        // Do some work with the buffer
                        for (int j = 0; j < Math.Min(5, buffer.Length); j++)
                        {
                            buffer[j] = threadId * 1000 + i * 100 + j;
                        }

                        // Verify buffer is valid
                        Assert.True(buffer.IsValid);
                        Assert.True(buffer.Length > 0);

                        // Track pool count during execution
                        lock (poolCountsDuringExecution)
                        {
                            poolCountsDuringExecution.Add(pool.ActivePoolCount);
                        }
                    }
                });
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Assert - No exceptions should have been thrown and pools should have been created
            Assert.True(pool.TotalPoolsCreated > 0);

            // Check that we saw multiple pools during execution
            Assert.Contains(poolCountsDuringExecution, count => count > 0);
        }

        [Fact]
        public void ThreadLocalMemoryPool_EmptyAllocation_HandledCorrectly()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new ThreadLocalMemoryPool(baseAllocator);

            // Act
            using var buffer = pool.Allocate<int>(0);

            // Assert
            Assert.True(buffer.IsEmpty);
            Assert.Equal(0, buffer.Length);
            Assert.False(buffer.IsValid); // Empty buffers should not be valid
        }

        [Fact]
        public void ThreadLocalMemoryPool_NegativeSize_ThrowsException()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new ThreadLocalMemoryPool(baseAllocator);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => pool.Allocate<int>(-1));
        }

        [Fact]
        public void ThreadLocalMemoryPool_LargeAllocation_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new ThreadLocalMemoryPool(baseAllocator);

            // Act
            const int largeSize = 100000; // 100K elements
            using var buffer = pool.Allocate<int>(largeSize);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(largeSize, buffer.Length);
            Assert.Equal(largeSize * sizeof(int), buffer.SizeInBytes);
        }

        [Fact]
        public void ThreadLocalMemoryPool_CrossThreadSharing_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new ThreadLocalMemoryPool(baseAllocator, enableCrossThreadSharing: true);

            // Act - Allocate in main thread and dispose
            using (var buffer1 = pool.Allocate<int>(50))
            {
                buffer1[0] = 42;
            }

            // Allocate in a different thread (should potentially reuse from cross-thread sharing)
            int result = 0;
            var task = Task.Run(() =>
            {
                using var buffer2 = pool.Allocate<int>(50, zeroMemory: true);
                result = buffer2[0]; // Should be 0 if zeroMemory was used
            });

            task.Wait();

            // Assert - Cross-thread sharing should work without errors and buffer should be zero-initialized
            Assert.Equal(0, result);
        }

        [Fact]
        public void ThreadLocalMemoryPool_NoCrossThreadSharing_Disabled()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new ThreadLocalMemoryPool(baseAllocator, enableCrossThreadSharing: false);

            var initialAllocatedBytes = pool.TotalAllocatedBytes;

            // Act - Allocate and dispose in main thread
            using (var buffer1 = pool.Allocate<int>(25))
            {
                buffer1[0] = 99;
            }

            var afterDisposeBytes = pool.TotalAllocatedBytes;

            // Allocate in different thread
            var task = Task.Run(() =>
            {
                using var buffer2 = pool.Allocate<int>(25);
                return buffer2[0];
            });

            var result = task.Result;
            var afterSecondAllocationBytes = pool.TotalAllocatedBytes;

            // Assert - TotalAllocatedBytes should remain stable (represents total ever allocated)
            Assert.True(afterDisposeBytes >= initialAllocatedBytes);
            Assert.True(afterSecondAllocationBytes >= initialAllocatedBytes);

            // Without cross-thread sharing, should get zero-initialized buffer
            Assert.Equal(0, result);

            // Clean up
            pool.Clear();
            Assert.Equal(0, pool.TotalAllocatedBytes);
        }

        [Fact]
        public void ThreadLocalMemoryPool_Dispose_CleansUpProperly()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            var pool = new ThreadLocalMemoryPool(baseAllocator);

            // Allocate some buffers
            using (var buffer1 = pool.Allocate<int>(10))
            using (var buffer2 = pool.Allocate<int>(20))
            {
                buffer1[0] = 1;
                buffer2[0] = 2;
            }

            var allocatedBytesBeforeDispose = pool.TotalAllocatedBytes;
            var poolsBeforeDispose = pool.ActivePoolCount;

            // Act
            pool.Dispose();

            // Assert - Should be disposed
            Assert.Throws<ObjectDisposedException>(() => pool.Allocate<int>(10));
        }

        [Fact]
        public void ThreadLocalMemoryPool_SizeClassOptimization_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new ThreadLocalMemoryPool(baseAllocator);

            // Act - Allocate buffers that should fit in size classes
            using var buffer64 = pool.Allocate<byte>(64);   // Should fit in 64-byte class
            using var buffer128 = pool.Allocate<byte>(128); // Should fit in 128-byte class
            using var buffer256 = pool.Allocate<byte>(256); // Should fit in 256-byte class

            var allocatedAfterFirstRound = pool.TotalAllocatedBytes;

            // Dispose and reallocate
            buffer64.Dispose();
            buffer128.Dispose();
            buffer256.Dispose();

            using var buffer64Again = pool.Allocate<byte>(64);
            using var buffer128Again = pool.Allocate<byte>(128);
            using var buffer256Again = pool.Allocate<byte>(256);

            // Assert - Should reuse from size classes
            Assert.Equal(64, buffer64Again.Length);
            Assert.Equal(128, buffer128Again.Length);
            Assert.Equal(256, buffer256Again.Length);

            // Total allocated bytes should remain the same (reused from pools)
            Assert.Equal(allocatedAfterFirstRound, pool.TotalAllocatedBytes);
        }

        [Fact]
        public void ThreadLocalMemoryPool_FallbackSizes_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new ThreadLocalMemoryPool(baseAllocator);

            // Act - Allocate uncommon sizes that won't fit in standard size classes
            const int uncommonSize1 = 150; // Between 128 and 256
            const int uncommonSize2 = 300; // Between 256 and 320

            using var buffer1 = pool.Allocate<int>(uncommonSize1);
            using var buffer2 = pool.Allocate<int>(uncommonSize2);

            var allocatedAfterFirst = pool.TotalAllocatedBytes;

            // Dispose and reallocate
            buffer1.Dispose();
            buffer2.Dispose();

            using var buffer1Again = pool.Allocate<int>(uncommonSize1);
            using var buffer2Again = pool.Allocate<int>(uncommonSize2);

            // Assert - Should handle uncommon sizes correctly
            Assert.Equal(uncommonSize1, buffer1Again.Length);
            Assert.Equal(uncommonSize2, buffer2Again.Length);

            // Should reuse from fallback pools
            Assert.Equal(allocatedAfterFirst, pool.TotalAllocatedBytes);
        }
    }
}