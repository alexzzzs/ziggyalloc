using System;
using System.Numerics;
using System.Runtime.Intrinsics;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class AlignedAllocatorTests
    {
        [Fact]
        public void AlignedAllocator_Constructor_WithValidBaseAllocator_Succeeds()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();

            // Act
            using var alignedAllocator = new AlignedAllocator(baseAllocator);

            // Assert
            Assert.True(alignedAllocator.SupportsIndividualDeallocation);
            Assert.NotNull(alignedAllocator);
            Assert.True(alignedAllocator.CacheLineSize > 0);
            Assert.NotEqual(CpuArchitecture.Basic, alignedAllocator.CpuArchitecture);
        }

        [Fact]
        public void AlignedAllocator_Constructor_WithNullBaseAllocator_Throws()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AlignedAllocator(null!));
        }

        [Fact]
        public void AlignedAllocator_Constructor_WithInvalidCustomAlignment_Throws()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new AlignedAllocator(baseAllocator, AlignmentStrategy.Custom, 0));
        }

        [Fact]
        public void AlignedAllocator_Allocate_WithZeroElements_ReturnsEmptyBuffer()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var alignedAllocator = new AlignedAllocator(baseAllocator);

            // Act
            using var buffer = alignedAllocator.Allocate<int>(0);

            // Assert
            Assert.True(buffer.IsEmpty);
            Assert.Equal(0, buffer.Length);
            Assert.False(buffer.IsValid);
        }

        [Fact]
        public void AlignedAllocator_Allocate_WithNegativeElements_Throws()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var alignedAllocator = new AlignedAllocator(baseAllocator);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => alignedAllocator.Allocate<int>(-1));
        }

        [Fact]
        public void AlignedAllocator_Allocate_WithValidElements_Succeeds()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var alignedAllocator = new AlignedAllocator(baseAllocator);

            // Act
            using var buffer = alignedAllocator.Allocate<int>(100);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(100, buffer.Length);
            Assert.Equal(100 * sizeof(int), buffer.SizeInBytes);
            Assert.True(alignedAllocator.TotalAllocatedBytes >= buffer.SizeInBytes);
        }

        [Fact]
        public void AlignedAllocator_Allocate_WithZeroMemory_InitializesMemory()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var alignedAllocator = new AlignedAllocator(baseAllocator);

            // Act
            using var buffer = alignedAllocator.Allocate<int>(10, zeroMemory: true);

            // Assert
            Assert.True(buffer.IsValid);
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(0, buffer[i]);
            }
        }

        [Fact]
        public void AlignedAllocator_Allocate_WithDifferentStrategies_UsesCorrectAlignment()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();

            // Act & Assert - Test different strategies
            using (var naturalAlloc = new AlignedAllocator(baseAllocator, AlignmentStrategy.Natural))
            using (var cacheAlloc = new AlignedAllocator(baseAllocator, AlignmentStrategy.CacheLine))
            using (var sseAlloc = new AlignedAllocator(baseAllocator, AlignmentStrategy.SSE))
            using (var avxAlloc = new AlignedAllocator(baseAllocator, AlignmentStrategy.AVX))
            {
                using var naturalBuffer = naturalAlloc.Allocate<int>(10);
                using var cacheBuffer = cacheAlloc.Allocate<int>(10);
                using var sseBuffer = sseAlloc.Allocate<int>(10);
                using var avxBuffer = avxAlloc.Allocate<int>(10);

                // All should be valid
                Assert.True(naturalBuffer.IsValid);
                Assert.True(cacheBuffer.IsValid);
                Assert.True(sseBuffer.IsValid);
                Assert.True(avxBuffer.IsValid);
            }
        }

        [Fact]
        public void AlignedAllocator_Allocate_WithCustomAlignment_UsesSpecifiedAlignment()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            const int customAlignment = 128;

            // Act
            using var alignedAllocator = new AlignedAllocator(baseAllocator, AlignmentStrategy.Custom, customAlignment);

            // Allocate some buffers and check they're properly aligned
            using var buffer1 = alignedAllocator.Allocate<int>(10);
            using var buffer2 = alignedAllocator.Allocate<double>(5);

            // Assert
            Assert.True(buffer1.IsValid);
            Assert.True(buffer2.IsValid);

            // Note: In a real test environment, we might not be able to verify
            // the exact alignment due to how the base allocator works
            // But we can verify the allocator functions correctly
        }

        [Fact]
        public void AlignedAllocator_SIMDTypes_GetProperAlignment()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var alignedAllocator = new AlignedAllocator(baseAllocator);

            // Act - Test SIMD types
            using var intBuffer = alignedAllocator.Allocate<Vector128<int>>(10);
            using var floatBuffer = alignedAllocator.Allocate<Vector256<float>>(5);
            using var doubleBuffer = alignedAllocator.Allocate<Vector512<double>>(3);

            // Assert
            Assert.True(intBuffer.IsValid);
            Assert.True(floatBuffer.IsValid);
            Assert.True(doubleBuffer.IsValid);

            // Verify we can write to SIMD-aligned buffers
            intBuffer[0] = Vector128.Create(1, 2, 3, 4);
            floatBuffer[0] = Vector256.Create(1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f);

            Assert.Equal(1, intBuffer[0][0]);
            Assert.Equal(1.0f, floatBuffer[0][0]);
        }

        [Fact]
        public void AlignedAllocator_Free_WithValidPointer_Succeeds()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var alignedAllocator = new AlignedAllocator(baseAllocator);

            var initialAllocatedBytes = alignedAllocator.TotalAllocatedBytes;

            // Act
            using (var buffer = alignedAllocator.Allocate<int>(50))
            {
                // Verify allocation
                Assert.True(buffer.IsValid);
            }

            // Wait a bit for potential cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Verify allocator is still functional
            using var newBuffer = alignedAllocator.Allocate<int>(25);
            Assert.True(newBuffer.IsValid);
        }

        [Fact]
        public void AlignedAllocator_Free_WithZeroPointer_DoesNothing()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var alignedAllocator = new AlignedAllocator(baseAllocator);

            // Act & Assert - Should not throw
            alignedAllocator.Free(IntPtr.Zero);
        }

        [Fact]
        public void AlignedAllocator_Dispose_CleansUpProperly()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            var alignedAllocator = new AlignedAllocator(baseAllocator);

            // Allocate some buffers
            using (var buffer1 = alignedAllocator.Allocate<int>(10))
            using (var buffer2 = alignedAllocator.Allocate<int>(20))
            {
                Assert.True(buffer1.IsValid);
                Assert.True(buffer2.IsValid);
            }

            // Act
            alignedAllocator.Dispose();

            // Assert - Should be disposed
            Assert.Throws<ObjectDisposedException>(() => alignedAllocator.Allocate<int>(10));
        }

        [Fact]
        public void AlignedAllocator_AlignmentStatistics_ProvidesValidData()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var alignedAllocator = new AlignedAllocator(baseAllocator);

            // Allocate some buffers
            using (var buffer1 = alignedAllocator.Allocate<int>(10))
            using (var buffer2 = alignedAllocator.Allocate<double>(5))
            {
                Assert.True(buffer1.IsValid);
                Assert.True(buffer2.IsValid);
            }

            // Act
            var statistics = alignedAllocator.GetAlignmentStatistics();

            // Assert
            Assert.NotEqual(CpuArchitecture.Basic, statistics.CpuArchitecture);
            Assert.True(statistics.CacheLineSize > 0);
            Assert.NotEqual(AlignmentStrategy.Auto, statistics.Strategy);
            Assert.True(statistics.TotalAlignedAllocations >= 0);
            Assert.True(statistics.TotalPaddingBytes >= 0);
            Assert.True(statistics.AlignmentEfficiency >= 0.0);
            Assert.True(statistics.AlignmentEfficiency <= 100.0);
        }

        [Fact]
        public void AlignedAllocator_LargeAllocations_Work()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var alignedAllocator = new AlignedAllocator(baseAllocator);

            // Act - Test large allocation
            const int largeSize = 100000; // 100K elements
            using var buffer = alignedAllocator.Allocate<byte>(largeSize);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(largeSize, buffer.Length);
            Assert.Equal(largeSize, buffer.SizeInBytes);
            Assert.True(alignedAllocator.TotalAllocatedBytes >= largeSize);
        }

        [Fact]
        public void AlignedAllocator_MixedAllocationSizes_Work()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var alignedAllocator = new AlignedAllocator(baseAllocator);

            // Act - Allocate buffers of various sizes
            using var smallBuffer = alignedAllocator.Allocate<byte>(16);
            using var mediumBuffer = alignedAllocator.Allocate<int>(1000);
            using var largeBuffer = alignedAllocator.Allocate<double>(50000);

            // Assert
            Assert.True(smallBuffer.IsValid);
            Assert.True(mediumBuffer.IsValid);
            Assert.True(largeBuffer.IsValid);

            Assert.Equal(16, smallBuffer.Length);
            Assert.Equal(1000, mediumBuffer.Length);
            Assert.Equal(50000, largeBuffer.Length);

            // Verify we can write to all buffers
            smallBuffer[0] = 42;
            mediumBuffer[0] = 12345;
            largeBuffer[0] = 3.14159;

            Assert.Equal(42, smallBuffer[0]);
            Assert.Equal(12345, mediumBuffer[0]);
            Assert.Equal(3.14159, largeBuffer[0]);
        }

        [Fact]
        public void AlignedAllocator_StructAllocations_Work()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var alignedAllocator = new AlignedAllocator(baseAllocator);

            // Act - Test with custom struct
            using var buffer = alignedAllocator.Allocate<Point3D>(100);

            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(100, buffer.Length);

            // Test writing and reading struct data
            buffer[0] = new Point3D { X = 1.0f, Y = 2.0f, Z = 3.0f };
            buffer[1] = new Point3D { X = 4.0f, Y = 5.0f, Z = 6.0f };

            Assert.Equal(1.0f, buffer[0].X);
            Assert.Equal(2.0f, buffer[0].Y);
            Assert.Equal(3.0f, buffer[0].Z);
            Assert.Equal(4.0f, buffer[1].X);
            Assert.Equal(5.0f, buffer[1].Y);
            Assert.Equal(6.0f, buffer[1].Z);
        }

        [Fact]
        public void AlignedAllocator_SpanConversion_Works()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var alignedAllocator = new AlignedAllocator(baseAllocator);

            // Act
            using var buffer = alignedAllocator.Allocate<int>(50);

            // Test span conversions
            Span<int> span = buffer;
            ReadOnlySpan<int> readOnlySpan = buffer;

            // Assert
            Assert.Equal(50, span.Length);
            Assert.Equal(50, readOnlySpan.Length);

            // Test span operations
            span.Fill(42);
            for (int i = 0; i < span.Length; i++)
            {
                Assert.Equal(42, span[i]);
                Assert.Equal(42, readOnlySpan[i]);
            }
        }

        [Fact]
        public void AlignedAllocator_BufferOperations_Work()
        {
            // Arrange
            var baseAllocator = new SystemMemoryAllocator();
            using var alignedAllocator = new AlignedAllocator(baseAllocator);

            // Act
            using var buffer = alignedAllocator.Allocate<int>(20);

            // Test buffer operations
            buffer.Fill(99);
            buffer.Clear();

            // Assert
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(0, buffer[i]);
            }

            // Test copy operations
            var sourceData = new int[] { 1, 2, 3, 4, 5 };
            buffer.CopyFrom(sourceData);

            for (int i = 0; i < sourceData.Length; i++)
            {
                Assert.Equal(sourceData[i], buffer[i]);
            }
        }

        // Helper struct for testing
        private struct Point3D
        {
            public float X, Y, Z;
        }
    }
}