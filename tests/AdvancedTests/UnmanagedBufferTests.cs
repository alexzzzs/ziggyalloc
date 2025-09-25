using System;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class UnmanagedBufferAdditionalTests
    {
        [Fact]
        public void UnmanagedBuffer_AsSpanWithRange_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(100);

            // Fill buffer with sequential values
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = i;
            }

            // Act
            var span = buffer.AsSpan(10, 20); // Get elements 10-29

            // Assert
            Assert.Equal(20, span.Length);
            for (int i = 0; i < span.Length; i++)
            {
                Assert.Equal(10 + i, span[i]);
            }
        }

        [Fact]
        public void UnmanagedBuffer_AsSpanWithRange_InvalidParameters_Throws()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(10);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.AsSpan(-1, 5)); // Negative start
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.AsSpan(15, 5)); // Start beyond buffer
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.AsSpan(5, -1)); // Negative length
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.AsSpan(5, 10)); // Length beyond buffer
        }

        [Fact]
        public void UnmanagedBuffer_AsReadOnlySpan_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(10);

            // Fill buffer
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = i * 2;
            }

            // Act
            var readOnlySpan = buffer.AsReadOnlySpan();

            // Assert
            Assert.Equal(buffer.Length, readOnlySpan.Length);
            for (int i = 0; i < readOnlySpan.Length; i++)
            {
                Assert.Equal(i * 2, readOnlySpan[i]);
            }
        }

        [Fact]
        public void UnmanagedBuffer_CopyFrom_Span_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var destinationBuffer = allocator.Allocate<int>(5);
            var sourceArray = new int[] { 1, 4, 9, 16, 25 };
            var sourceSpan = new ReadOnlySpan<int>(sourceArray);

            // Act
            destinationBuffer.CopyFrom(sourceSpan);

            // Assert
            for (int i = 0; i < destinationBuffer.Length; i++)
            {
                Assert.Equal(sourceArray[i], destinationBuffer[i]);
            }
        }

        [Fact]
        public void UnmanagedBuffer_EqualsAndHashcode_Work()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer1 = allocator.Allocate<int>(10);
            using var buffer2 = allocator.Allocate<int>(10);
            using var buffer3 = allocator.Allocate<int>(10);

            // Act & Assert
            // Different buffers should not be equal
            Assert.False(buffer1.Equals(buffer2));
            Assert.False(buffer1.Equals(buffer3));
            Assert.False(buffer2.Equals(buffer3));
            
            // Buffer should equal itself
            Assert.True(buffer1.Equals(buffer1));
            
            // Hash codes should be different for different buffers
            Assert.NotEqual(buffer1.GetHashCode(), buffer2.GetHashCode());
        }

        [Fact]
        public void UnmanagedBuffer_StructType_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<Point>(5);

            // Act
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = new Point { X = i, Y = i * 2 };
            }

            // Assert
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(i, buffer[i].X);
                Assert.Equal(i * 2, buffer[i].Y);
            }
        }

        [Fact]
        public void UnmanagedBuffer_DifferentPrimitiveTypes_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();

            // Act & Assert - byte
            using var byteBuffer = allocator.Allocate<byte>(10);
            byteBuffer.Fill(0xFF);
            Assert.Equal(0xFF, byteBuffer[0]);

            // Act & Assert - short
            using var shortBuffer = allocator.Allocate<short>(10);
            shortBuffer.Fill(1000);
            Assert.Equal(1000, shortBuffer[0]);

            // Act & Assert - long
            using var longBuffer = allocator.Allocate<long>(10);
            longBuffer.Fill(123456789L);
            Assert.Equal(123456789L, longBuffer[0]);

            // Act & Assert - float
            using var floatBuffer = allocator.Allocate<float>(10);
            floatBuffer.Fill(3.14f);
            Assert.Equal(3.14f, floatBuffer[0]);

            // Act & Assert - double
            using var doubleBuffer = allocator.Allocate<double>(10);
            doubleBuffer.Fill(2.718);
            Assert.Equal(2.718, doubleBuffer[0]);
        }

        [Fact]
        public void UnmanagedBuffer_NullPointer_InvalidBufferHandled()
        {
            // Arrange
            unsafe
            {
                int* nullPointer = null;
                var buffer = new UnmanagedBuffer<int>(nullPointer, 10);

                // Act & Assert
                Assert.False(buffer.IsValid);
                Assert.Equal(10, buffer.Length);
                
                // Accessing invalid buffer should throw for indexed access and First/Last
                Assert.Throws<InvalidOperationException>(() => { var _ = buffer[0]; });
                Assert.Throws<InvalidOperationException>(() => { var _ = buffer.First; });
                Assert.Throws<InvalidOperationException>(() => { var _ = buffer.Last; });
                // But AsSpan should return an empty span for invalid buffers
                var span = buffer.AsSpan();
                Assert.Equal(0, span.Length);
            }
        }

        [Fact]
        public void UnmanagedBuffer_WrapExisting_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var originalBuffer = allocator.Allocate<int>(10);
            
            // Fill original buffer
            for (int i = 0; i < originalBuffer.Length; i++)
            {
                originalBuffer[i] = i * 10;
            }

            // Act - Wrap existing memory (doesn't own it)
            unsafe
            {
                var wrappedBuffer = new UnmanagedBuffer<int>((int*)originalBuffer.RawPointer, originalBuffer.Length);
                
                // Assert
                Assert.True(wrappedBuffer.IsValid);
                Assert.Equal(originalBuffer.Length, wrappedBuffer.Length);
                
                // Access through wrapped buffer
                for (int i = 0; i < wrappedBuffer.Length; i++)
                {
                    Assert.Equal(i * 10, wrappedBuffer[i]);
                }
                
                // Modify through wrapped buffer should affect original
                wrappedBuffer[0] = 999;
                Assert.Equal(999, originalBuffer[0]);
            }
        }

        [Fact]
        public void UnmanagedBuffer_Dispose_MultipleTimes_Safe()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            var buffer = allocator.Allocate<int>(10);

            // Act & Assert - Should not throw exceptions
            buffer.Dispose();
            buffer.Dispose(); // Second dispose should be safe
            buffer.Dispose(); // Third dispose should be safe
        }

        [Fact]
        public void UnmanagedBuffer_AfterDispose_AccessThrowsObjectDisposedException()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            var buffer = allocator.Allocate<int>(10);
            buffer.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => { var _ = buffer[0]; });
            Assert.Throws<ObjectDisposedException>(() => { var _ = buffer.First; });
            Assert.Throws<ObjectDisposedException>(() => { var _ = buffer.Last; });
            Assert.Throws<ObjectDisposedException>(() => buffer.AsSpan());
            Assert.Throws<ObjectDisposedException>(() => buffer.AsReadOnlySpan());
            Assert.Throws<ObjectDisposedException>(() => buffer.Fill(42));
            Assert.Throws<ObjectDisposedException>(() => buffer.Clear());
        }

        [Fact]
        public void UnmanagedBuffer_CopyFrom_Buffer_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var sourceBuffer = allocator.Allocate<int>(5);
            using var destinationBuffer = allocator.Allocate<int>(5);

            // Fill source buffer
            for (int i = 0; i < sourceBuffer.Length; i++)
            {
                sourceBuffer[i] = i * 10;
            }

            // Act
            destinationBuffer.CopyFrom(sourceBuffer);

            // Assert
            for (int i = 0; i < destinationBuffer.Length; i++)
            {
                Assert.Equal(i * 10, destinationBuffer[i]);
            }
        }

        public struct Point
        {
            public int X;
            public int Y;
        }
    }
}