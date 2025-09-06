using System;
using System.Threading.Tasks;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class AllocatorEdgeCaseTests
    {
        [Fact]
        public void SystemMemoryAllocator_ZeroSizeAllocation_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();

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
        public void SystemMemoryAllocator_NegativeSizeAllocation_Throws()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => allocator.Allocate<int>(-1));
        }

        [Fact]
        public void SystemMemoryAllocator_LargeSizeAllocation_ThrowsOnOverflow()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();

            // Act & Assert
            // This should overflow when calculating total size
            // The actual exception depends on whether it's an arithmetic overflow or size limit
            var ex = Assert.ThrowsAny<Exception>(() => allocator.Allocate<int>(int.MaxValue / sizeof(int) + 1));
            Assert.True(ex is OverflowException || ex is OutOfMemoryException, 
                $"Expected OverflowException or OutOfMemoryException, but got {ex.GetType().Name}: {ex.Message}");
        }

        [Fact]
        public void SystemMemoryAllocator_ZeroMemoryFlag_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();

            // Act
            using var buffer = allocator.Allocate<byte>(100, true);

            // Assert
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(0, buffer[i]);
            }
        }

        [Fact]
        public void ScopedMemoryAllocator_ZeroSizeAllocation_Works()
        {
            // Arrange
            using var allocator = new ScopedMemoryAllocator();

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
        public void ScopedMemoryAllocator_NegativeSizeAllocation_Throws()
        {
            // Arrange
            using var allocator = new ScopedMemoryAllocator();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => allocator.Allocate<int>(-1));
        }

        [Fact]
        public void ScopedMemoryAllocator_IndividualDeallocation_Throws()
        {
            // Arrange
            using var allocator = new ScopedMemoryAllocator();
            using var buffer = allocator.Allocate<int>(10);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => allocator.Free(buffer.RawPointer));
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

        [Fact]
        public void UnmanagedMemoryPool_ZeroSizeAllocation_Works()
        {
            // Arrange
            var backingAllocator = new SystemMemoryAllocator();
            using var allocator = new UnmanagedMemoryPool(backingAllocator);

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
        public void UnmanagedMemoryPool_NegativeSizeAllocation_Throws()
        {
            // Arrange
            var backingAllocator = new SystemMemoryAllocator();
            using var allocator = new UnmanagedMemoryPool(backingAllocator);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => allocator.Allocate<int>(-1));
        }

        [Fact]
        public void HybridAllocator_ZeroSizeAllocation_Works()
        {
            // Arrange
            var backingAllocator = new SystemMemoryAllocator();
            using var allocator = new HybridAllocator(backingAllocator);

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
        public void HybridAllocator_NegativeSizeAllocation_Throws()
        {
            // Arrange
            var backingAllocator = new SystemMemoryAllocator();
            using var allocator = new HybridAllocator(backingAllocator);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => allocator.Allocate<int>(-1));
        }

        [Fact]
        public void HybridAllocator_DisposedAllocator_Throws()
        {
            // Arrange
            var backingAllocator = new SystemMemoryAllocator();
            var allocator = new HybridAllocator(backingAllocator);
            allocator.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => allocator.Allocate<int>(10));
            Assert.Throws<ObjectDisposedException>(() => allocator.Free(IntPtr.Zero));
        }

        [Fact]
        public void SlabAllocator_ZeroSizeAllocation_Works()
        {
            // Arrange
            var backingAllocator = new SystemMemoryAllocator();
            using var allocator = new SlabAllocator(backingAllocator);

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
        public void SlabAllocator_NegativeSizeAllocation_Throws()
        {
            // Arrange
            var backingAllocator = new SystemMemoryAllocator();
            using var allocator = new SlabAllocator(backingAllocator);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => allocator.Allocate<int>(-1));
        }

        [Fact]
        public void SlabAllocator_DisposedAllocator_Throws()
        {
            // Arrange
            var backingAllocator = new SystemMemoryAllocator();
            var allocator = new SlabAllocator(backingAllocator);
            allocator.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => allocator.Allocate<int>(10));
        }

        [Fact]
        public void UnmanagedBuffer_EmptyBuffer_BoundsCheckingWorks()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(0);

            // Act & Assert
            Assert.True(buffer.IsEmpty);
            
            // Accessing any element should throw InvalidOperationException for null pointer
            Assert.Throws<InvalidOperationException>(() => { var _ = buffer[0]; });
            Assert.Throws<InvalidOperationException>(() => { var _ = buffer.First; });
            Assert.Throws<InvalidOperationException>(() => { var _ = buffer.Last; });
        }

        [Fact]
        public void UnmanagedBuffer_IndexAccess_BoundsCheckingWorks()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(10);

            // Act & Assert
            // Valid indices should work
            buffer[0] = 42;
            buffer[9] = 99;
            Assert.Equal(42, buffer[0]);
            Assert.Equal(99, buffer[9]);

            // Invalid indices should throw
            Assert.Throws<IndexOutOfRangeException>(() => { var _ = buffer[-1]; });
            Assert.Throws<IndexOutOfRangeException>(() => { var _ = buffer[10]; });
            Assert.Throws<IndexOutOfRangeException>(() => { var _ = buffer[100]; });
        }

        [Fact]
        public void UnmanagedBuffer_FirstLastAccess_EmptyBuffer_Throws()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(0);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => { var _ = buffer.First; });
            Assert.Throws<InvalidOperationException>(() => { var _ = buffer.Last; });
        }

        [Fact]
        public void UnmanagedBuffer_FirstLastAccess_NonEmptyBuffer_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(5);
            buffer[0] = 10;
            buffer[4] = 50;

            // Act & Assert
            Assert.Equal(10, buffer.First);
            Assert.Equal(50, buffer.Last);
        }

        [Fact]
        public void UnmanagedBuffer_AsSpan_ZeroLength_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(0);

            // Act
            var span = buffer.AsSpan();

            // Assert
            Assert.Equal(0, span.Length);
        }

        [Fact]
        public void UnmanagedBuffer_AsSpanWithRange_ZeroLength_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(0);

            // Act
            var span = buffer.AsSpan(0, 0);

            // Assert
            Assert.Equal(0, span.Length);
        }

        [Fact]
        public void UnmanagedBuffer_AsSpanWithRange_InvalidRange_Throws()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(10);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.AsSpan(0, 11)); // Length too long
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.AsSpan(10, 1)); // Start at end
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.AsSpan(11, 0)); // Start beyond end
        }

        [Fact]
        public void UnmanagedBuffer_Fill_ZeroLength_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(0);

            // Act & Assert - Should throw InvalidOperationException for null pointer
            Assert.Throws<InvalidOperationException>(() => buffer.Fill(42));
        }

        [Fact]
        public void UnmanagedBuffer_Clear_ZeroLength_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(0);

            // Act & Assert - Should throw InvalidOperationException for null pointer
            Assert.Throws<InvalidOperationException>(() => buffer.Clear());
        }
    }
}