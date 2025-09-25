using System;
using System.Threading.Tasks;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class UnmanagedBufferExtendedTests
    {
        [Fact]
        public void UnmanagedBuffer_DisposeMultipleTimes_HandledGracefully()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            var buffer = allocator.Allocate<int>(100);
            buffer[0] = 42;
            
            // Act & Assert
            buffer.Dispose(); // First disposal
            buffer.Dispose(); // Second disposal should not throw
            
            // Test passes if no exception thrown
        }

        [Fact]
        public void UnmanagedBuffer_AccessAfterDispose_ThrowsObjectDisposedException()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            var buffer = allocator.Allocate<int>(10);
            buffer[0] = 42;
            buffer.Dispose(); // Dispose first
            
            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => { var _ = buffer[0]; });
            Assert.Throws<ObjectDisposedException>(() => buffer[0] = 42);
            Assert.Throws<ObjectDisposedException>(() => { var _ = buffer.First; });
            Assert.Throws<ObjectDisposedException>(() => { var _ = buffer.Last; });
        }

        [Fact]
        public void UnmanagedBuffer_VeryLargeAllocation_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            const int veryLargeSize = 10_000_000; // 10M elements
            
            // Act
            using var buffer = allocator.Allocate<byte>(veryLargeSize);
            
            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(veryLargeSize, buffer.Length);
            Assert.Equal(veryLargeSize, buffer.SizeInBytes);
        }

        [Fact]
        public void UnmanagedBuffer_StructType_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            
            // Act
            using var buffer = allocator.Allocate<Point3D>(1000);
            
            // Fill with data
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = new Point3D { X = i, Y = i * 2, Z = i * 3 };
            }
            
            // Assert
            Assert.True(buffer.IsValid);
            Assert.Equal(1000, buffer.Length);
            Assert.Equal(0, buffer[0].X);
            Assert.Equal(100, buffer[50].Y);
            Assert.Equal(150, buffer[50].Z);
        }

        [Fact]
        public void UnmanagedBuffer_DifferentPrimitiveTypes_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();

            // Act & Assert - sbyte
            using var sbyteBuffer = allocator.Allocate<sbyte>(10);
            sbyteBuffer.Fill(-128);
            Assert.Equal(-128, sbyteBuffer[0]);

            // Act & Assert - ushort
            using var ushortBuffer = allocator.Allocate<ushort>(10);
            ushortBuffer.Fill(65535);
            Assert.Equal((ushort)65535, ushortBuffer[0]);

            // Act & Assert - uint
            using var uintBuffer = allocator.Allocate<uint>(10);
            uintBuffer.Fill(4294967295U);
            Assert.Equal(4294967295U, uintBuffer[0]);

            // Act & Assert - ulong
            using var ulongBuffer = allocator.Allocate<ulong>(10);
            ulongBuffer.Fill(18446744073709551615UL);
            Assert.Equal(18446744073709551615UL, ulongBuffer[0]);

            // Act & Assert - char
            using var charBuffer = allocator.Allocate<char>(10);
            charBuffer.Fill('Z');
            Assert.Equal('Z', charBuffer[0]);
        }

        [Fact]
        public async Task UnmanagedBuffer_ConcurrentAccess_ThreadSafe()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(1000);
            const int threadCount = 8;
            const int operationsPerThread = 100;
            var tasks = new Task[threadCount];

            // Initialize buffer
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = i;
            }

            // Act - Run concurrent read operations
            for (int t = 0; t < threadCount; t++)
            {
                int threadId = t;
                tasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < operationsPerThread; i++)
                    {
                        // Read from buffer
                        int sum = 0;
                        for (int j = 0; j < Math.Min(100, buffer.Length); j++)
                        {
                            sum += buffer[j + threadId * 10];
                        }
                        // Do some work
                        System.Threading.Thread.Sleep(1);
                    }
                });
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Assert - If we reach here without exceptions, the test passes
            Assert.True(true); // No exceptions thrown
        }

        [Fact]
        public void UnmanagedBuffer_AsSpan_WithLargeBuffer_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            const int largeSize = 1_000_000; // 1M elements
            using var buffer = allocator.Allocate<int>(largeSize);
            
            // Fill with data
            for (int i = 0; i < Math.Min(1000, buffer.Length); i++)
            {
                buffer[i] = i;
            }
            
            // Act
            var span = buffer.AsSpan();
            
            // Assert
            Assert.Equal(largeSize, span.Length);
            Assert.Equal(0, span[0]);
            Assert.Equal(10, span[10]);
        }

        [Fact]
        public void UnmanagedBuffer_CopyFrom_LargeSpan_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            using var destinationBuffer = allocator.Allocate<int>(10000);
            var sourceArray = new int[10000];
            
            // Fill source array
            for (int i = 0; i < sourceArray.Length; i++)
            {
                sourceArray[i] = i * 2;
            }
            
            var sourceSpan = new ReadOnlySpan<int>(sourceArray);

            // Act
            destinationBuffer.CopyFrom(sourceSpan);

            // Assert
            for (int i = 0; i < Math.Min(100, destinationBuffer.Length); i++)
            {
                Assert.Equal(sourceArray[i], destinationBuffer[i]);
            }
        }

        [Fact]
        public void UnmanagedBuffer_Fill_WithLargeBuffer_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            const int largeSize = 1_000_000; // 1M elements
            using var buffer = allocator.Allocate<int>(largeSize);
            
            // Act
            buffer.Fill(42);

            // Assert
            // Check a sample of elements to verify fill worked
            for (int i = 0; i < Math.Min(1000, buffer.Length); i += 100)
            {
                Assert.Equal(42, buffer[i]);
            }
        }

        [Fact]
        public void UnmanagedBuffer_Clear_WithLargeBuffer_Works()
        {
            // Arrange
            var allocator = new SystemMemoryAllocator();
            const int largeSize = 1_000_000; // 1M elements
            using var buffer = allocator.Allocate<byte>(largeSize);
            
            // Fill with non-zero data
            buffer.Fill(0xFF);
            Assert.Equal(0xFF, buffer[1000]);
            
            // Act
            buffer.Clear();

            // Assert
            // Check a sample of elements to verify clear worked
            for (int i = 0; i < Math.Min(1000, buffer.Length); i += 100)
            {
                Assert.Equal((byte)0, buffer[i]);
            }
        }

        public struct Point3D
        {
            public float X;
            public float Y;
            public float Z;
        }
    }
}