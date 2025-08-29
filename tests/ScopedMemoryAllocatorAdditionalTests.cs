using System;
using System.Threading.Tasks;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class ScopedMemoryAllocatorAdditionalTests
    {
        [Fact]
        public void ScopedMemoryAllocator_DisposeMultipleTimes_HandledGracefully()
        {
            // Arrange
            var allocator = new ScopedMemoryAllocator();
            using (allocator)
            {
                using var buffer = allocator.Allocate<int>(10);
                buffer[0] = 42;
            } // First disposal
            
            // Act & Assert
            // Second disposal should not throw
            allocator.Dispose();
            
            // Test passes if no exception thrown
        }

        [Fact]
        public void ScopedMemoryAllocator_UseAfterDispose_ThrowsObjectDisposedException()
        {
            // Arrange
            var allocator = new ScopedMemoryAllocator();
            allocator.Dispose(); // Dispose first
            
            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => allocator.Allocate<int>(10));
            Assert.Throws<ObjectDisposedException>(() => allocator.TotalAllocatedBytes);
        }

        [Fact]
        public void ScopedMemoryAllocator_LargeNumberOfAllocations_HandledCorrectly()
        {
            // Arrange
            using var allocator = new ScopedMemoryAllocator();
            const int allocationCount = 10000;
            
            // Act
            var buffers = new UnmanagedBuffer<int>[allocationCount];
            long totalExpectedBytes = 0;
            
            for (int i = 0; i < allocationCount; i++)
            {
                buffers[i] = allocator.Allocate<int>(10);
                totalExpectedBytes += 10 * sizeof(int);
            }
            
            // Assert
            Assert.Equal(totalExpectedBytes, allocator.TotalAllocatedBytes);
        }

        [Fact]
        public void ScopedMemoryAllocator_VeryLargeAllocation_HandledCorrectly()
        {
            // Arrange
            using var allocator = new ScopedMemoryAllocator();
            const int veryLargeSize = 50_000_000; // 50M elements
            
            // Act
            using var buffer = allocator.Allocate<byte>(veryLargeSize);
            
            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(veryLargeSize, buffer.Length);
        }

        [Fact]
        public void ScopedMemoryAllocator_StructTypeAllocation_Works()
        {
            // Arrange
            using var allocator = new ScopedMemoryAllocator();
            
            // Act
            using var buffer = allocator.Allocate<Point3D>(1000);
            
            // Fill with data
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = new Point3D { X = i, Y = i * 2, Z = i * 3 };
            }
            
            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(1000, buffer.Length);
            Assert.Equal(0, buffer[0].X);
            Assert.Equal(100, buffer[50].Y);
            Assert.Equal(150, buffer[50].Z);
        }

        [Fact]
        public void ScopedMemoryAllocator_ConcurrentAllocations_ThreadSafe()
        {
            // Arrange
            using var allocator = new ScopedMemoryAllocator();
            const int threadCount = 10;
            const int allocationsPerThread = 1000;
            var tasks = new Task[threadCount];
            
            // Act - Run allocations in parallel
            for (int t = 0; t < threadCount; t++)
            {
                int threadId = t;
                tasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < allocationsPerThread; i++)
                    {
                        using var buffer = allocator.Allocate<int>(10 + threadId);
                        // Do some work with the buffer
                        for (int j = 0; j < Math.Min(5, buffer.Length); j++)
                        {
                            buffer[j] = threadId * 1000 + i * 100 + j;
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
        public void ScopedMemoryAllocator_NestedAllocators_LargeHierarchy()
        {
            // Arrange & Act
            using var level1 = new ScopedMemoryAllocator();
            using var buffer1 = level1.Allocate<int>(100);
            
            for (int i = 0; i < 10; i++)
            {
                using var level2 = new ScopedMemoryAllocator();
                using var buffer2 = level2.Allocate<double>(50);
                
                for (int j = 0; j < 5; j++)
                {
                    using var level3 = new ScopedMemoryAllocator();
                    using var buffer3 = level3.Allocate<byte>(25);
                    
                    // Use buffers
                    buffer1[0] = i;
                    buffer2[0] = j;
                    buffer3[0] = (byte)(i + j);
                }
            }
            
            // Assert - All allocators should work independently
            Assert.True(buffer1.IsValid);
            Assert.Equal(400, level1.TotalAllocatedBytes); // 100 * 4 bytes
        }

        [Fact]
        public void ScopedMemoryAllocator_AllocationPatterns_MixedSizes()
        {
            // Arrange
            using var allocator = new ScopedMemoryAllocator();
            
            // Act - Allocate buffers with exponentially increasing sizes
            for (int i = 0; i < 20; i++)
            {
                int size = (int)Math.Pow(2, i);
                if (size > 1_000_000) size = 1_000_000; // Cap at 1M
                
                using var buffer = allocator.Allocate<byte>(size);
                // Use buffer
                if (buffer.Length > 0)
                {
                    buffer[0] = (byte)(i % 256);
                    buffer[buffer.Length - 1] = (byte)((i * 2) % 256);
                }
            }
            
            // Assert - All allocations should succeed
            Assert.True(allocator.TotalAllocatedBytes > 0);
        }

        public struct Point3D
        {
            public float X;
            public float Y;
            public float Z;
        }
    }
}