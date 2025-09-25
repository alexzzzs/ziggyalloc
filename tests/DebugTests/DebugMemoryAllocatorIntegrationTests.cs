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
            bool leakDetected = false;

            try
            {
                using var debugAllocator = new DebugMemoryAllocator("Test", backend, MemoryLeakReportingMode.Throw);
                var buffer = debugAllocator.Allocate<int>(1);
                buffer[0] = 42;
                // Intentionally not disposing to test leak detection
            }
            catch (InvalidOperationException ex)
            {
                leakDetected = ex.Message.Contains("MEMORY LEAK DETECTED");
            }

            Assert.True(leakDetected, "Debug allocator should detect memory leaks");
        }

        [Fact]
        public void DebugMemoryAllocator_NoLeaksWhenProperlyDisposed()
        {
            var backend = new SystemMemoryAllocator();
            var debugAllocator = new DebugMemoryAllocator("Test", backend, MemoryLeakReportingMode.Throw);

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