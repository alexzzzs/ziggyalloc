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
            using var hybridAllocator = new HybridAllocator(new SystemMemoryAllocator());

            // Act - Allocate a small array that should use managed memory
            using var smallBuffer = hybridAllocator.Allocate<int>(10); // 10 ints, below threshold of 512

            // Assert
            Assert.True(smallBuffer.IsValid);
            Assert.Equal(10, smallBuffer.Length);
            Assert.Equal(40, smallBuffer.SizeInBytes); // 10 * 4 bytes per int
        }

        [Fact]
        public void HybridAllocator_AllocatesLargeArraysUsingUnmanagedMemory()
        {
            // Arrange
            using var hybridAllocator = new HybridAllocator(new SystemMemoryAllocator());

            // Act - Allocate a large array that should use unmanaged memory
            using var largeBuffer = hybridAllocator.Allocate<int>(1000); // 1000 ints, above threshold of 512

            // Assert
            Assert.True(largeBuffer.IsValid);
            Assert.Equal(1000, largeBuffer.Length);
            Assert.Equal(4000, largeBuffer.SizeInBytes); // 1000 * 4 bytes per int
        }

        [Fact]
        public void HybridAllocator_ZeroMemoryFlagWorks()
        {
            // Arrange
            using var hybridAllocator = new HybridAllocator(new SystemMemoryAllocator());

            // Act - Allocate with zero memory flag
            using var buffer = hybridAllocator.Allocate<int>(100, true);

            // Assert - All elements should be zero
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(0, buffer[i]);
            }
        }

        [Fact]
        public void HybridAllocator_BufferOperationsWork()
        {
            // Arrange
            using var hybridAllocator = new HybridAllocator(new SystemMemoryAllocator());
            using var buffer = hybridAllocator.Allocate<int>(10);

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
        public void HybridAllocator_Dispose_WorksCorrectly()
        {
            // Arrange
            var hybridAllocator = new HybridAllocator(new SystemMemoryAllocator());

            // Act & Assert - Should not throw when disposing
            hybridAllocator.Dispose();
            
            // After disposal, allocating should throw ObjectDisposedException
            Assert.Throws<ObjectDisposedException>(() => hybridAllocator.Allocate<int>(10));
        }

        [Fact]
        public void HybridAllocator_TotalAllocatedBytes_TracksCorrectly()
        {
            // Arrange
            using var hybridAllocator = new HybridAllocator(new SystemMemoryAllocator());
            long initialBytes = hybridAllocator.TotalAllocatedBytes;

            // Act
            using var buffer1 = hybridAllocator.Allocate<int>(100); // 400 bytes
            using var buffer2 = hybridAllocator.Allocate<byte>(200); // 200 bytes
            
            // Assert
            Assert.Equal(initialBytes + 600, hybridAllocator.TotalAllocatedBytes);
        }
    }
}