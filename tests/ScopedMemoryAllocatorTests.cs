using System;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class ScopedMemoryAllocatorTests
    {
        [Fact]
        public void ScopedMemoryAllocator_BasicAllocation_Works()
        {
            // Arrange
            using var allocator = new ScopedMemoryAllocator();

            // Act
            using var buffer = allocator.Allocate<int>(100);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(100, buffer.Length);
            Assert.Equal(400, buffer.SizeInBytes); // 100 * 4 bytes per int
        }

        [Fact]
        public void ScopedMemoryAllocator_MultipleAllocations_TrackedCorrectly()
        {
            // Arrange
            using var allocator = new ScopedMemoryAllocator();

            // Act
            using var buffer1 = allocator.Allocate<int>(10);
            using var buffer2 = allocator.Allocate<double>(20);
            using var buffer3 = allocator.Allocate<byte>(30);

            long allocatedBytesAfterAll = allocator.TotalAllocatedBytes;

            // Assert
            Assert.Equal(10, buffer1.Length);
            Assert.Equal(20, buffer2.Length);
            Assert.Equal(30, buffer3.Length);

            long expectedTotalBytes = 10 * sizeof(int) + 20 * sizeof(double) + 30 * sizeof(byte);
            Assert.Equal(expectedTotalBytes, allocatedBytesAfterAll);
        }

        [Fact]
        public void ScopedMemoryAllocator_Dispose_FreesAllMemory()
        {
            // Arrange
            var allocator = new ScopedMemoryAllocator();
            using (allocator)
            {
                using var buffer1 = allocator.Allocate<int>(100);
                using var buffer2 = allocator.Allocate<double>(50);
                using var buffer3 = allocator.Allocate<byte>(1000);

                // Fill buffers with data
                buffer1[0] = 42;
                buffer2[0] = 3.14;
                buffer3[0] = 255;

                Assert.True(allocator.TotalAllocatedBytes > 0);
            } // Allocator disposed here

            // Act & Assert
            // After disposal, accessing TotalAllocatedBytes should be safe
            // Note: We can't directly test if memory was freed without causing undefined behavior,
            // but the test passes if no exceptions are thrown
        }

        [Fact]
        public void ScopedMemoryAllocator_ZeroMemoryFlag_Works()
        {
            // Arrange
            using var allocator = new ScopedMemoryAllocator();

            // Act
            using var buffer = allocator.Allocate<int>(10, zeroMemory: true);

            // Assert
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(0, buffer[i]);
            }
        }

        [Fact]
        public void ScopedMemoryAllocator_DoesNotSupportIndividualDeallocation()
        {
            // Arrange
            using var allocator = new ScopedMemoryAllocator();

            // Act & Assert
            Assert.False(allocator.SupportsIndividualDeallocation);
            
            // Calling Free should throw NotSupportedException
            Assert.Throws<NotSupportedException>(() => allocator.Free(IntPtr.Zero));
        }

        [Fact]
        public void ScopedMemoryAllocator_NestedScopes_WorkIndependently()
        {
            // Arrange & Act
            using var outerAllocator = new ScopedMemoryAllocator();
            using var outerBuffer = outerAllocator.Allocate<int>(100);
            
            using (var innerAllocator = new ScopedMemoryAllocator())
            {
                using var innerBuffer = innerAllocator.Allocate<double>(50);
                
                // Assert - Both allocators should track their own allocations
                Assert.True(outerAllocator.TotalAllocatedBytes > 0);
                Assert.True(innerAllocator.TotalAllocatedBytes > 0);
                
                // Allocations should be independent
                outerBuffer[0] = 42;
                innerBuffer[0] = 3.14;
                
                Assert.Equal(42, outerBuffer[0]);
                Assert.Equal(3.14, innerBuffer[0]);
            } // innerAllocator disposed here
            
            // Outer allocator should still be valid
            Assert.Equal(42, outerBuffer[0]);
        }

        [Fact]
        public void ScopedMemoryAllocator_EmptyAllocation_HandledCorrectly()
        {
            // Arrange
            using var allocator = new ScopedMemoryAllocator();

            // Act
            using var buffer = allocator.Allocate<int>(0);

            // Assert
            Assert.True(buffer.IsEmpty);
            Assert.Equal(0, buffer.Length);
            Assert.Equal(0, buffer.SizeInBytes);
        }

        [Fact]
        public void ScopedMemoryAllocator_NegativeSize_ThrowsException()
        {
            // Arrange
            using var allocator = new ScopedMemoryAllocator();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => allocator.Allocate<int>(-1));
        }

        [Fact]
        public void ScopedMemoryAllocator_LargeAllocation_Works()
        {
            // Arrange
            using var allocator = new ScopedMemoryAllocator();

            // Act
            const int largeSize = 1000000; // 1M elements
            using var buffer = allocator.Allocate<int>(largeSize);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(largeSize, buffer.Length);
            Assert.Equal(largeSize * sizeof(int), buffer.SizeInBytes);
        }

        [Fact]
        public void ScopedMemoryAllocator_BufferReuseAfterDispose_HandledCorrectly()
        {
            // Arrange
            using (var allocator = new ScopedMemoryAllocator())
            {
                var buffer = allocator.Allocate<int>(10);
                buffer[0] = 42;
                
                // Buffer should be valid and accessible
                Assert.Equal(42, buffer[0]);
            } // Allocator disposed here
            
            // After allocator disposal, we can't safely access the buffer
            // The test passes if we reach this point without exceptions
        }
    }
}