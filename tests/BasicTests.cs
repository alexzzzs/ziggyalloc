using System;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class BasicTests
    {
        [Fact]
        public void SystemMemoryAllocator_CanBeCreated()
        {
            // Arrange & Act
            var allocator = new SystemMemoryAllocator();

            // Assert
            Assert.NotNull(allocator);
            Assert.True(allocator.SupportsIndividualDeallocation);
        }

        [Fact]
        public void ScopedMemoryAllocator_CanBeCreated()
        {
            // Arrange & Act
            using var allocator = new ScopedMemoryAllocator();

            // Assert
            Assert.NotNull(allocator);
            Assert.False(allocator.SupportsIndividualDeallocation);
        }

        [Fact]
        public void Z_DefaultAllocator_IsAvailable()
        {
            // Arrange & Act & Assert
            // Just verify the static default allocator exists
            Assert.NotNull(Z.DefaultAllocator);
        }
    }
}