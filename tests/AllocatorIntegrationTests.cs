using System;
using System.Threading.Tasks;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class AllocatorIntegrationTests
    {
        [Fact]
        public void HybridAllocator_WithSystemMemoryAllocator_Integration()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var hybridAllocator = new HybridAllocator(systemAllocator);

            // Act
            using var smallBuffer = hybridAllocator.Allocate<int>(10); // Should use managed
            using var largeBuffer = hybridAllocator.Allocate<int>(1000); // Should use unmanaged

            // Assert
            Assert.True(smallBuffer.IsValid);
            Assert.True(largeBuffer.IsValid);
            Assert.Equal(10, smallBuffer.Length);
            Assert.Equal(1000, largeBuffer.Length);
        }

        [Fact]
        public void HybridAllocator_WithScopedMemoryAllocator_Integration()
        {
            // Arrange
            using var scopedAllocator = new ScopedMemoryAllocator();
            using var hybridAllocator = new HybridAllocator(scopedAllocator);

            // Act
            using var buffer = hybridAllocator.Allocate<int>(100);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(100, buffer.Length);
        }

        [Fact]
        public void UnmanagedMemoryPool_WithSystemMemoryAllocator_Integration()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(systemAllocator);

            // Act
            using var buffer1 = pool.Allocate<int>(100);
            using var buffer2 = pool.Allocate<int>(100); // Should come from pool
            using var buffer3 = pool.Allocate<int>(200); // New allocation

            // Assert
            Assert.True(buffer1.IsValid);
            Assert.True(buffer2.IsValid);
            Assert.True(buffer3.IsValid);
        }

        [Fact]
        public void UnmanagedMemoryPool_WithDebugMemoryAllocator_Integration()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("PoolTest", systemAllocator);
            using var pool = new UnmanagedMemoryPool(debugAllocator);

            // Act
            using var buffer1 = pool.Allocate<int>(50);
            using var buffer2 = pool.Allocate<int>(50); // Should come from pool

            // Assert
            Assert.True(buffer1.IsValid);
            Assert.True(buffer2.IsValid);
        }

        [Fact]
        public void SlabAllocator_WithSystemMemoryAllocator_Integration()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var slabAllocator = new SlabAllocator(systemAllocator);

            // Act
            using var smallBuffer = slabAllocator.Allocate<int>(10); // Should use slab
            using var largeBuffer = slabAllocator.Allocate<int>(10000); // Should delegate to system

            // Assert
            Assert.True(smallBuffer.IsValid);
            Assert.True(largeBuffer.IsValid);
            Assert.Equal(10, smallBuffer.Length);
            Assert.Equal(10000, largeBuffer.Length);
        }

        [Fact]
        public void SlabAllocator_WithDebugMemoryAllocator_Integration()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("SlabTest", systemAllocator);
            using var slabAllocator = new SlabAllocator(debugAllocator);

            // Act
            using var buffer = slabAllocator.Allocate<byte>(100);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(100, buffer.Length);
        }

        [Fact]
        public void DebugMemoryAllocator_WithSystemMemoryAllocator_TracksAllocations()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("IntegrationTest", systemAllocator);

            // Act
            using var buffer1 = debugAllocator.Allocate<int>(100);
            using var buffer2 = debugAllocator.Allocate<byte>(50);

            // Assert
            Assert.True(buffer1.IsValid);
            Assert.True(buffer2.IsValid);
            Assert.Equal(400, buffer1.SizeInBytes); // 100 * 4 bytes
            Assert.Equal(50, buffer2.SizeInBytes);  // 50 * 1 byte
        }

        [Fact]
        public void ScopedMemoryAllocator_WithNestedAllocations_Works()
        {
            // Arrange
            using var outerAllocator = new ScopedMemoryAllocator();

            // Act
            using var buffer1 = outerAllocator.Allocate<int>(50);
            
            // Create a nested scope
            using var buffer2 = outerAllocator.Allocate<byte>(100);

            // Assert
            Assert.True(buffer1.IsValid);
            Assert.True(buffer2.IsValid);
        }

        [Fact]
        public void MultipleAllocators_SameType_DifferentInstances_WorkIndependently()
        {
            // Arrange
            var allocator1 = new SystemMemoryAllocator();
            var allocator2 = new SystemMemoryAllocator();
            using var pool1 = new UnmanagedMemoryPool(allocator1);
            using var pool2 = new UnmanagedMemoryPool(allocator2);

            // Act
            using var buffer1 = pool1.Allocate<int>(100);
            using var buffer2 = pool2.Allocate<int>(100);

            // Assert
            Assert.True(buffer1.IsValid);
            Assert.True(buffer2.IsValid);
            
            // Fill with different values
            buffer1.Fill(1);
            buffer2.Fill(2);
            
            // Verify they're independent
            Assert.Equal(1, buffer1[0]);
            Assert.Equal(2, buffer2[0]);
        }

        [Fact]
        public void AllocatorChain_SystemToDebugToPool_Works()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("ChainTest", systemAllocator);
            using var pool = new UnmanagedMemoryPool(debugAllocator);

            // Act
            using var buffer = pool.Allocate<int>(50);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(50, buffer.Length);
        }

        [Fact]
        public void AllocatorChain_SystemToHybridToPool_Works()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var poolAllocator = new UnmanagedMemoryPool(systemAllocator);
            using var hybridAllocator = new HybridAllocator(poolAllocator);

            // Act
            using var smallBuffer = hybridAllocator.Allocate<int>(10); // May use managed
            using var largeBuffer = hybridAllocator.Allocate<int>(1000); // Will use unmanaged/pool

            // Assert
            Assert.True(smallBuffer.IsValid);
            Assert.True(largeBuffer.IsValid);
        }

        [Fact]
        public void AllocatorChain_SystemToSlabToDebug_Works()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var slabAllocator = new SlabAllocator(systemAllocator);
            using var debugAllocator = new DebugMemoryAllocator("SlabDebugTest", slabAllocator);

            // Act
            using var buffer = debugAllocator.Allocate<int>(50);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(50, buffer.Length);
        }

        [Fact]
        public void AllocatorIntegration_WithDifferentStructTypes_Works()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var hybridAllocator = new HybridAllocator(systemAllocator);
            using var pool = new UnmanagedMemoryPool(hybridAllocator);

            // Act
            using var intBuffer = pool.Allocate<int>(10);
            using var doubleBuffer = pool.Allocate<double>(5);
            using var structBuffer = pool.Allocate<TestStruct>(3);

            // Assert
            Assert.True(intBuffer.IsValid);
            Assert.True(doubleBuffer.IsValid);
            Assert.True(structBuffer.IsValid);
            
            // Fill with values
            intBuffer.Fill(42);
            doubleBuffer.Fill(3.14);
            for (int i = 0; i < structBuffer.Length; i++)
            {
                structBuffer[i] = new TestStruct { X = i, Y = i * 2 };
            }
            
            Assert.Equal(42, intBuffer[0]);
            Assert.Equal(3.14, doubleBuffer[0]);
            Assert.Equal(2, structBuffer[1].Y);
        }

        [Fact]
        public void AllocatorIntegration_DisposeOrder_DoesNotAffectOtherAllocators()
        {
            // Arrange
            var systemAllocator = new SystemMemoryAllocator();
            using var hybridAllocator = new HybridAllocator(systemAllocator);
            using var pool = new UnmanagedMemoryPool(hybridAllocator);

            // Act - allocate before dispose
            using var buffer = pool.Allocate<int>(10);
            
            // Dispose hybrid allocator
            hybridAllocator.Dispose(); // Dispose hybrid after allocation
            
            // Assert - buffer should still be valid because it was allocated before dispose
            Assert.True(buffer.IsValid);
        }

        public struct TestStruct
        {
            public int X;
            public int Y;
        }
    }
}