using System;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class DebugMemoryAllocatorIntegrationTests
    {
        [Fact]
        public void DebugMemoryAllocator_DetectsLeaks()
        {
            var backend = new SystemMemoryAllocator();

            // Capture the leak detection output
            var originalError = Console.Error;
            using var stringWriter = new StringWriter();
            Console.SetError(stringWriter);

            try
            {
                using var debugAllocator = new DebugMemoryAllocator("Test", backend, MemoryLeakReportingMode.CIFriendly);
                var buffer = debugAllocator.Allocate<int>(1);
                buffer[0] = 42;
                // Intentionally not disposing to test leak detection
                // The debug allocator will be disposed when exiting the using block
                // This should log the leak detection message but not crash
            }
            finally
            {
                // Restore original error output
                Console.SetError(originalError);
            }

            // Check that leak detection message was logged
            var output = stringWriter.ToString();
            Assert.Contains("MEMORY LEAK DETECTED", output);
            Assert.Contains("Test", output);
            Assert.Contains("CI-Friendly Mode", output);
        }

        [Fact]
        public void DebugMemoryAllocator_NoLeaksWhenProperlyDisposed()
        {
            var backend = new SystemMemoryAllocator();
            var debugAllocator = new DebugMemoryAllocator("Test", backend, MemoryLeakReportingMode.CIFriendly);

            // Allocate and immediately dispose
            var buffer = debugAllocator.Allocate<int>(1);
            buffer[0] = 42;

            // Check tracking before disposal
            Assert.Equal(1, debugAllocator.GetTrackedAllocationCount());

            // Get the pointer before disposal
            var pointer = buffer.RawPointer;

            // Dispose the buffer
            buffer.Dispose();

            // Should be no tracked allocations after disposal
            Assert.Equal(0, debugAllocator.GetTrackedAllocationCount());

            // Debug allocator disposal should not throw
            debugAllocator.Dispose();
        }

        [Fact]
        public void DebugMemoryAllocator_ZeroSizeAllocation_Works()
        {
            // Arrange
            var backingAllocator = new SystemMemoryAllocator();
            using var allocator = new DebugMemoryAllocator("Test", backingAllocator);

            // Act
            using var buffer = allocator.Allocate<int>(0);

            // Assert
            // For zero-length allocations, the buffer is valid but pointer is null
            Assert.False(buffer.IsValid); // Null pointer for zero-length allocations
            Assert.Equal(0, buffer.Length);
            Assert.Equal(0, buffer.SizeInBytes);
            Assert.True(buffer.IsEmpty);
        }

        [Fact]
        public void DebugMemoryAllocator_NegativeSizeAllocation_Throws()
        {
            // Arrange
            var backingAllocator = new SystemMemoryAllocator();
            using var allocator = new DebugMemoryAllocator("Test", backingAllocator);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => allocator.Allocate<int>(-1));
        }
    }
}