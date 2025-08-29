using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class DebugMemoryAllocatorTests
    {
        [Fact]
        public void DebugMemoryAllocator_BasicAllocation_Works()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);

            // Act
            using var buffer = debugAllocator.Allocate<int>(100);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(100, buffer.Length);
            Assert.Equal(1, debugAllocator.GetTrackedAllocationCount());
        }

        [Fact]
        public void DebugMemoryAllocator_ProperDisposal_RemovesTracking()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);

            // Act
            var buffer = debugAllocator.Allocate<int>(10);
            Assert.Equal(1, debugAllocator.GetTrackedAllocationCount());

            buffer.Dispose();
            
            // Assert
            Assert.Equal(0, debugAllocator.GetTrackedAllocationCount());
        }

        [Fact]
        public void DebugMemoryAllocator_LeakDetection_WithThrowMode()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            bool leakDetected = false;

            try
            {
                using var debugAllocator = new DebugMemoryAllocator("TestComponent", backend, MemoryLeakReportingMode.Throw);
                var buffer = debugAllocator.Allocate<int>(10);
                buffer[0] = 42;
                // Intentionally not disposing buffer to test leak detection
            }
            catch (InvalidOperationException ex)
            {
                leakDetected = ex.Message.Contains("MEMORY LEAK DETECTED");
            }

            // Assert
            Assert.True(leakDetected);
        }

        [Fact]
        public void DebugMemoryAllocator_LeakDetection_WithLogMode()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            
            // Capture the leak detection output
            var originalError = Console.Error;
            using var stringWriter = new StringWriter();
            Console.SetError(stringWriter);
            
            try
            {
                using var debugAllocator = new DebugMemoryAllocator("TestComponent", backend, MemoryLeakReportingMode.Log);
                var buffer = debugAllocator.Allocate<int>(10);
                buffer[0] = 42;
                // Intentionally not disposing buffer to test leak detection
                
                // The debug allocator will be disposed when exiting the using block
                // This should log the leak detection message
            }
            finally
            {
                // Restore original error output
                Console.SetError(originalError);
            }
            
            // Check that leak detection message was logged
            var output = stringWriter.ToString();
            Assert.Contains("MEMORY LEAK DETECTED", output);
            Assert.Contains("TestComponent", output);
        }

        [Fact]
        public void DebugMemoryAllocator_ZeroMemoryFlag_Works()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);

            // Act
            using var buffer = debugAllocator.Allocate<int>(10, zeroMemory: true);

            // Assert
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(0, buffer[i]);
            }
        }

        [Fact]
        public void DebugMemoryAllocator_MultipleAllocations_TrackedCorrectly()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);

            // Act
            using var buffer1 = debugAllocator.Allocate<int>(10);
            using var buffer2 = debugAllocator.Allocate<double>(20);
            using var buffer3 = debugAllocator.Allocate<byte>(30);

            // Assert
            Assert.Equal(3, debugAllocator.GetTrackedAllocationCount());
            
            // Dispose one buffer
            buffer2.Dispose();
            Assert.Equal(2, debugAllocator.GetTrackedAllocationCount());
            
            // Dispose remaining buffers
            buffer1.Dispose();
            buffer3.Dispose();
            Assert.Equal(0, debugAllocator.GetTrackedAllocationCount());
        }

        [Fact]
        public async Task DebugMemoryAllocator_ThreadSafety_Works()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);
            const int threadCount = 5;
            const int allocationsPerThread = 20;
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
            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(0, debugAllocator.GetTrackedAllocationCount());
        }

        [Fact]
        public void DebugMemoryAllocator_EmptyAllocation_HandledCorrectly()
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
        public void DebugMemoryAllocator_NegativeSize_ThrowsException()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => debugAllocator.Allocate<int>(-1));
        }

        [Fact]
        public void DebugMemoryAllocator_GetTrackedAllocationCount_Works()
        {
            // Arrange
            var backend = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("TestComponent", backend);

            // Act
            using var buffer1 = debugAllocator.Allocate<int>(10);
            using var buffer2 = debugAllocator.Allocate<double>(20);
            
            var trackedAllocationCount = debugAllocator.GetTrackedAllocationCount();

            // Assert
            Assert.Equal(2, trackedAllocationCount);
        }
    }
}