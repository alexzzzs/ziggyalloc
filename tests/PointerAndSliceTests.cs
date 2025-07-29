using System;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class UnmanagedBufferTests
    {
        [Fact]
        public void UnmanagedBuffer_BasicOperations_Work()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(5);
            
            Assert.Equal(5, buffer.Length);
            Assert.False(buffer.IsEmpty);
            Assert.True(buffer.IsValid);
            Assert.Equal(5 * sizeof(int), buffer.SizeInBytes);
            
            // Test indexing
            buffer[0] = 10;
            buffer[1] = 20;
            buffer[2] = 30;
            buffer[3] = 40;
            buffer[4] = 50;
            
            Assert.Equal(10, buffer[0]);
            Assert.Equal(20, buffer[1]);
            Assert.Equal(30, buffer[2]);
            Assert.Equal(40, buffer[3]);
            Assert.Equal(50, buffer[4]);
        }

        [Fact]
        public void UnmanagedBuffer_FirstAndLast_Work()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(5);
            
            // Set first and last
            buffer.First = 100;
            buffer.Last = 500;
            
            Assert.Equal(100, buffer[0]);
            Assert.Equal(500, buffer[4]);
            Assert.Equal(100, buffer.First);
            Assert.Equal(500, buffer.Last);
        }

        [Fact]
        public void UnmanagedBuffer_IndexOutOfRange_ThrowsException()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(3);
            
            // Test out of range access
            Assert.Throws<IndexOutOfRangeException>(() => buffer[3]);
            Assert.Throws<IndexOutOfRangeException>(() => buffer[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => buffer[100]);
        }

        [Fact]
        public void UnmanagedBuffer_SpanConversions_Work()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(3, zeroMemory: true);
            
            buffer[0] = 1;
            buffer[1] = 2;
            buffer[2] = 3;
            
            // Test explicit conversion to Span<T>
            Span<int> span = buffer.AsSpan();
            Assert.Equal(3, span.Length);
            Assert.Equal(1, span[0]);
            Assert.Equal(2, span[1]);
            Assert.Equal(3, span[2]);
            
            // Test explicit conversion to ReadOnlySpan<T>
            ReadOnlySpan<int> readOnlySpan = buffer.AsReadOnlySpan();
            Assert.Equal(3, readOnlySpan.Length);
            Assert.Equal(1, readOnlySpan[0]);
            Assert.Equal(2, readOnlySpan[1]);
            Assert.Equal(3, readOnlySpan[2]);
            
            // Test implicit conversions
            Span<int> implicitSpan = buffer;
            ReadOnlySpan<int> implicitReadOnlySpan = buffer;
            
            Assert.Equal(3, implicitSpan.Length);
            Assert.Equal(3, implicitReadOnlySpan.Length);
        }

        [Fact]
        public void UnmanagedBuffer_PartialSpan_Works()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(10);
            
            // Fill buffer
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = i * 10;
            }
            
            // Get partial span
            var partialSpan = buffer.AsSpan(2, 5);
            Assert.Equal(5, partialSpan.Length);
            Assert.Equal(20, partialSpan[0]); // buffer[2]
            Assert.Equal(60, partialSpan[4]); // buffer[6]
        }

        [Fact]
        public void UnmanagedBuffer_UtilityMethods_Work()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(5);
            
            // Test Fill
            buffer.Fill(42);
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(42, buffer[i]);
            }
            
            // Test Clear
            buffer.Clear();
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(0, buffer[i]);
            }
        }

        [Fact]
        public void UnmanagedBuffer_CopyFrom_Works()
        {
            var allocator = new SystemMemoryAllocator();
            using var source = allocator.Allocate<int>(3);
            using var destination = allocator.Allocate<int>(3);
            
            // Fill source
            source[0] = 10;
            source[1] = 20;
            source[2] = 30;
            
            // Copy to destination
            destination.CopyFrom(source.AsReadOnlySpan());
            
            // Verify copy
            Assert.Equal(10, destination[0]);
            Assert.Equal(20, destination[1]);
            Assert.Equal(30, destination[2]);
        }

        [Fact]
        public void UnmanagedBuffer_CopyFromSpan_Works()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(3);
            
            // Create source span
            int[] sourceArray = { 100, 200, 300 };
            ReadOnlySpan<int> sourceSpan = sourceArray;
            
            // Copy from span
            buffer.CopyFrom(sourceSpan);
            
            // Verify copy
            Assert.Equal(100, buffer[0]);
            Assert.Equal(200, buffer[1]);
            Assert.Equal(300, buffer[2]);
        }

        [Fact]
        public void UnmanagedBuffer_EmptyBuffer_HandledCorrectly()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(0);
            
            Assert.True(buffer.IsEmpty);
            Assert.Equal(0, buffer.Length);
            Assert.Equal(0, buffer.SizeInBytes);
            
            // For empty buffers, we expect the buffer to not be valid (null pointer)
            // This is the expected behavior for 0-length allocations
            Assert.False(buffer.IsValid);
            
            // First/Last should throw on empty buffer
            Assert.Throws<InvalidOperationException>(() => buffer.First);
            Assert.Throws<InvalidOperationException>(() => buffer.Last);
            
            // AsSpan should also throw for invalid buffer
            Assert.Throws<InvalidOperationException>(() => buffer.AsSpan());
        }
    }
}