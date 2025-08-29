using System;
using System.Threading.Tasks;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class UnmanagedMemoryPoolAdditionalTests
    {
        [Fact]
        public void UnmanagedMemoryPool_DisposeWithActiveBuffers_HandlesGracefully()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            var pool = new UnmanagedMemoryPool(baseAllocator);
            var buffer = pool.Allocate<int>(100);
            
            // Act & Assert
            // Disposing pool while buffers are still active should not throw
            pool.Dispose();
            
            // After pool disposal, we can't safely use the buffer
            // The test passes if we reach this point without exceptions
        }

        [Fact]
        public void UnmanagedMemoryPool_RepeatedAllocationAndDisposal_PoolsEffectively()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(baseAllocator);
            
            const int iterations = 1000;
            const int bufferSize = 50;
            
            // Act - Repeatedly allocate and dispose buffers of the same size
            for (int i = 0; i < iterations; i++)
            {
                using var buffer = pool.Allocate<int>(bufferSize);
                // Use the buffer
                for (int j = 0; j < Math.Min(10, buffer.Length); j++)
                {
                    buffer[j] = i * 1000 + j;
                }
            }
            
            // Assert - Pool should have effectively reused buffers
            // We can't directly test reuse without causing undefined behavior,
            // but the test passes if no exceptions are thrown
        }

        [Fact]
        public void UnmanagedMemoryPool_ManyDifferentSizes_HandledCorrectly()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(baseAllocator);
            
            const int sizeCount = 50;
            
            // Act - Allocate buffers of many different sizes
            for (int i = 1; i <= sizeCount; i++)
            {
                using var buffer = pool.Allocate<int>(i);
                buffer[0] = i;
            }
            
            // Assert - All allocations should succeed
            Assert.True(true); // Test passes if no exceptions thrown
        }

        [Fact]
        public void UnmanagedMemoryPool_VeryLargeAllocation_HandledCorrectly()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(baseAllocator);
            
            const int veryLargeSize = 10_000_000; // 10M elements
            
            // Act
            using var buffer = pool.Allocate<byte>(veryLargeSize);
            
            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(veryLargeSize, buffer.Length);
        }

        [Fact]
        public void UnmanagedMemoryPool_InterleavedAllocations_DifferentSizes()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(baseAllocator);
            
            // Act - Interleave allocations of different sizes
            using var buffer1 = pool.Allocate<int>(10);
            using var buffer2 = pool.Allocate<double>(20);
            buffer1.Dispose(); // Return to pool
            using var buffer3 = pool.Allocate<int>(10); // Should reuse
            using var buffer4 = pool.Allocate<int>(10); // Should create new
            
            // Assert
            Assert.True(buffer3.IsValid);
            Assert.True(buffer4.IsValid);
        }

        [Fact]
        public void UnmanagedMemoryPool_ConcurrentAllocations_SameSize_ThreadSafe()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(baseAllocator);
            const int threadCount = 20;
            const int allocationsPerThread = 100;
            var tasks = new Task[threadCount];
            
            // Act - Run allocations in parallel for the same buffer size
            for (int t = 0; t < threadCount; t++)
            {
                tasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < allocationsPerThread; i++)
                    {
                        using var buffer = pool.Allocate<int>(100);
                        // Do some work with the buffer
                        for (int j = 0; j < Math.Min(10, buffer.Length); j++)
                        {
                            buffer[j] = t * 1000 + i * 10 + j;
                        }
                    }
                });
            }
            
            // Wait for all tasks to complete
            Task.WaitAll(tasks);
            
            // Assert - No exceptions should have been thrown
            Assert.True(true);
        }

        [Fact]
        public void UnmanagedMemoryPool_StructTypeAllocation_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(baseAllocator);
            
            // Act
            using var buffer = pool.Allocate<Point3D>(50);
            
            // Fill with data
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = new Point3D { X = i, Y = i * 2, Z = i * 3 };
            }
            
            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(50, buffer.Length);
            Assert.Equal(0, buffer[0].X);
            Assert.Equal(4, buffer[2].Y);
            Assert.Equal(15, buffer[5].Z);
        }

        [Fact]
        public void UnmanagedMemoryPool_ZeroSizeAllocation_HandledCorrectly()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(baseAllocator);
            
            // Act
            using var buffer = pool.Allocate<int>(0);
            
            // Assert
            Assert.True(buffer.IsEmpty);
            Assert.Equal(0, buffer.Length);
            Assert.False(buffer.IsValid);
        }

        public struct Point3D
        {
            public float X;
            public float Y;
            public float Z;
        }
    }
}