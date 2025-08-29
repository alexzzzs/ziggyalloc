using System;
using Xunit;

namespace ZiggyAlloc.Tests
{
    public class HybridAllocatorTests
    {
        [Fact]
        public void HybridAllocator_AllocatesSmallArraysUsingManagedMemory()
        {
            // Arrange
            var hybridAllocator = new HybridAllocator(new SystemMemoryAllocator());

            // Act - Allocate a small array that should use managed memory
            var smallBuffer = hybridAllocator.Allocate<int>(10); // 10 ints, below threshold of 512

            // Assert
            Assert.True(smallBuffer.IsValid);
            Assert.Equal(10, smallBuffer.Length);
            Assert.Equal(40, smallBuffer.SizeInBytes); // 10 * 4 bytes per int

            // Cleanup
            smallBuffer.Dispose();
        }

        [Fact]
        public void HybridAllocator_AllocatesLargeArraysUsingUnmanagedMemory()
        {
            // Arrange
            var hybridAllocator = new HybridAllocator(new SystemMemoryAllocator());

            // Act - Allocate a large array that should use unmanaged memory
            var largeBuffer = hybridAllocator.Allocate<int>(1000); // 1000 ints, above threshold of 512

            // Assert
            Assert.True(largeBuffer.IsValid);
            Assert.Equal(1000, largeBuffer.Length);
            Assert.Equal(4000, largeBuffer.SizeInBytes); // 1000 * 4 bytes per int

            // Cleanup
            largeBuffer.Dispose();
        }

        [Fact]
        public void HybridAllocator_ZeroMemoryFlagWorks()
        {
            // Arrange
            var hybridAllocator = new HybridAllocator(new SystemMemoryAllocator());

            // Act - Allocate with zero memory flag
            var buffer = hybridAllocator.Allocate<int>(100, true);

            // Assert - All elements should be zero
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(0, buffer[i]);
            }

            // Cleanup
            buffer.Dispose();
        }

        [Fact]
        public void HybridAllocator_BufferOperationsWork()
        {
            // Arrange
            var hybridAllocator = new HybridAllocator(new SystemMemoryAllocator());
            var buffer = hybridAllocator.Allocate<int>(10);

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

            // Cleanup
            buffer.Dispose();
        }
    }
}