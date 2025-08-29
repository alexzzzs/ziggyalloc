using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class DebugMemoryAllocatorAdditionalTests
    {
        [Fact]
        public void DebugMemoryAllocator_DisposeMultipleTimes_HandledGracefully()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);
            using (debugAllocator)
            {
                using var buffer = debugAllocator.Allocate<int>(10);
                buffer[0] = 42;
                // buffer is automatically disposed here
            } // First disposal
            
            // Act & Assert
            // Second disposal should not throw
            debugAllocator.Dispose();
            
            // Test passes if no exception thrown
        }

        [Fact]
        public void DebugMemoryAllocator_UseAfterDispose_ThrowsObjectDisposedException()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);
            debugAllocator.Dispose(); // Dispose first
            
            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => debugAllocator.Allocate<int>(10));
            Assert.Throws<ObjectDisposedException>(() => debugAllocator.GetTrackedAllocationCount());
            Assert.Throws<ObjectDisposedException>(() => debugAllocator.Free(IntPtr.Zero));
        }

        [Fact]
        public void DebugMemoryAllocator_LargeNumberOfAllocations_TrackedCorrectly()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);
            const int allocationCount = 5000;
            
            // Act
            var buffers = new List<UnmanagedBuffer<int>>();
            
            for (int i = 0; i < allocationCount; i++)
            {
                buffers.Add(debugAllocator.Allocate<int>(10));
            }
            
            // Assert
            Assert.Equal(allocationCount, debugAllocator.GetTrackedAllocationCount());
            
            // Clean up - dispose all buffers
            foreach (var buffer in buffers)
            {
                buffer.Dispose();
            }
            Assert.Equal(0, debugAllocator.GetTrackedAllocationCount());
        }

        [Fact]
        public void DebugMemoryAllocator_VeryLargeAllocation_TrackedCorrectly()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);
            const int veryLargeSize = 10_000_000; // 10M elements
            
            // Act
            using var buffer = debugAllocator.Allocate<byte>(veryLargeSize);
            
            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(veryLargeSize, buffer.Length);
            Assert.Equal(1, debugAllocator.GetTrackedAllocationCount());
        }

        [Fact]
        public void DebugMemoryAllocator_StructTypeAllocation_TrackedCorrectly()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);
            
            // Act
            using var buffer = debugAllocator.Allocate<Point3D>(1000);
            
            // Fill with data
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = new Point3D { X = i, Y = i * 2, Z = i * 3 };
            }
            
            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(1000, buffer.Length);
            Assert.Equal(1, debugAllocator.GetTrackedAllocationCount());
            
            // The buffer will be automatically disposed by the using statement
        }

        [Fact]
        public void DebugMemoryAllocator_EmptyAllocation_NotTracked()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);
            
            // Act
            using var buffer = debugAllocator.Allocate<int>(0);
            
            // Assert
            Assert.True(buffer.IsEmpty);
            Assert.Equal(0, buffer.Length);
            // Empty allocations should not be tracked since they don't allocate actual memory
            Assert.Equal(0, debugAllocator.GetTrackedAllocationCount());
        }

        [Fact]
        public void DebugMemoryAllocator_ConcurrentAllocations_ThreadSafe()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);
            const int threadCount = 10;
            const int allocationsPerThread = 100;
            var tasks = new Task[threadCount];
            
            // Act
            for (int t = 0; t < threadCount; t++)
            {
                int threadId = t;
                tasks[t] = Task.Run(() =>
                {
                    var buffers = new UnmanagedBuffer<int>[allocationsPerThread];
                    
                    // Allocate buffers
                    for (int i = 0; i < allocationsPerThread; i++)
                    {
                        buffers[i] = debugAllocator.Allocate<int>(10 + threadId);
                        // Use the buffer
                        for (int j = 0; j < Math.Min(5, buffers[i].Length); j++)
                        {
                            buffers[i][j] = threadId * 1000 + i * 100 + j;
                        }
                    }
                    
                    // Dispose buffers
                    for (int i = 0; i < allocationsPerThread; i++)
                    {
                        buffers[i].Dispose();
                    }
                });
            }

            // Wait for all tasks to complete
            Task.WaitAll(tasks);

            // Assert
            Assert.Equal(0, debugAllocator.GetTrackedAllocationCount());
        }

        [Fact]
        public void DebugMemoryAllocator_MultipleDisposeOperations_TrackedCorrectly()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);
            
            // Act
            using var buffer1 = debugAllocator.Allocate<int>(10);
            using var buffer2 = debugAllocator.Allocate<double>(20);
            
            Assert.Equal(2, debugAllocator.GetTrackedAllocationCount());
            
            // Buffers will be automatically disposed by the using statements
            // Assert.Equal(0, debugAllocator.GetTrackedAllocationCount());
        }

        [Fact]
        public void DebugMemoryAllocator_GetTrackedAllocationCount_AfterDispose()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);
            
            // Act
            var buffer = debugAllocator.Allocate<int>(10);
            Assert.Equal(1, debugAllocator.GetTrackedAllocationCount());
            
            // Dispose the buffer before disposing the allocator to avoid leak detection
            buffer.Dispose();
            debugAllocator.Dispose(); // This should not report any leaks now
            
            // After disposal, we can't safely call GetTrackedAllocationCount
            // The test passes if we reach this point without exceptions from the disposal
        }

        public struct Point3D
        {
            public float X;
            public float Y;
            public float Z;
        }
    }
}