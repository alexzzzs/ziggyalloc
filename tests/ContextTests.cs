using System;
using System.Text;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class InteropTests
    {
        [Fact]
        public void UnmanagedBuffer_RawPointer_AccessibleForInterop()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(10);
            
            // Raw pointer should be accessible
            IntPtr rawPtr = buffer.RawPointer;
            Assert.NotEqual(IntPtr.Zero, rawPtr);
            
            // Should be able to use for interop scenarios
            buffer[0] = 42;
            buffer[9] = 100;
            
            // Verify data is accessible through raw pointer
            unsafe
            {
                int* ptr = (int*)rawPtr;
                Assert.Equal(42, ptr[0]);
                Assert.Equal(100, ptr[9]);
            }
        }

        [Fact]
        public void UnmanagedBuffer_CopyOperations_Work()
        {
            var allocator = new SystemMemoryAllocator();
            using var source = allocator.Allocate<int>(5);
            using var destination = allocator.Allocate<int>(5);
            
            // Fill source buffer
            for (int i = 0; i < source.Length; i++)
            {
                source[i] = i * i;
            }
            
            // Copy from source to destination
            destination.CopyFrom(source.AsReadOnlySpan());
            
            // Verify copy
            for (int i = 0; i < destination.Length; i++)
            {
                Assert.Equal(i * i, destination[i]);
            }
        }

        [Fact]
        public void UnmanagedBuffer_SpanInterop_WorksWithStandardLibrary()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(10);
            
            // Fill with random data
            var random = new Random(42);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = random.Next(1, 100);
            }
            
            // Use with standard library methods that accept Span<T>
            Span<int> span = buffer;
            span.Sort();
            
            // Verify sorting worked
            for (int i = 1; i < buffer.Length; i++)
            {
                Assert.True(buffer[i] >= buffer[i - 1]);
            }
        }

        [Fact]
        public void SystemMemoryAllocator_WrapExisting_Works()
        {
            // Allocate some memory
            var allocator = new SystemMemoryAllocator();
            using var original = allocator.Allocate<int>(5);
            
            // Fill with data
            for (int i = 0; i < original.Length; i++)
            {
                original[i] = i * 10;
            }
            
            // Wrap the existing memory (doesn't own it)
            var wrapped = SystemMemoryAllocator.WrapExisting<int>(original.RawPointer, original.Length);
            
            // Should access the same memory
            Assert.Equal(original.Length, wrapped.Length);
            for (int i = 0; i < wrapped.Length; i++)
            {
                Assert.Equal(i * 10, wrapped[i]);
            }
            
            // Modifying wrapped should affect original
            wrapped[0] = 999;
            Assert.Equal(999, original[0]);
        }

        [Fact]
        public void SystemMemoryAllocator_WrapSpan_Works()
        {
            // Create a managed array
            int[] managedArray = { 1, 2, 3, 4, 5 };
            Span<int> span = managedArray;
            
            // Wrap the span (doesn't own the memory)
            var wrapped = SystemMemoryAllocator.WrapSpan(span);
            
            Assert.Equal(span.Length, wrapped.Length);
            for (int i = 0; i < wrapped.Length; i++)
            {
                Assert.Equal(managedArray[i], wrapped[i]);
            }
            
            // Modifying wrapped should affect original
            wrapped[0] = 999;
            Assert.Equal(999, managedArray[0]);
        }

        [Fact]
        public void UnmanagedBuffer_LargeAllocation_Works()
        {
            var allocator = new SystemMemoryAllocator();
            
            // Allocate a large buffer (1MB)
            const int elementCount = 1024 * 1024 / sizeof(int); // 1MB of ints
            using var buffer = allocator.Allocate<int>(elementCount);
            
            Assert.Equal(elementCount, buffer.Length);
            Assert.Equal(elementCount * sizeof(int), buffer.SizeInBytes);
            
            // Should be able to access first and last elements
            buffer[0] = 42;
            buffer[elementCount - 1] = 100;
            
            Assert.Equal(42, buffer[0]);
            Assert.Equal(100, buffer[elementCount - 1]);
        }

        [Fact]
        public void Z_DefaultAllocator_IsAvailable()
        {
            // Test that the static default allocator works
            using var buffer = Z.DefaultAllocator.Allocate<int>(10);
            
            Assert.Equal(10, buffer.Length);
            Assert.True(buffer.IsValid);
            
            // Should be able to use it normally
            buffer[0] = 42;
            Assert.Equal(42, buffer[0]);
        }
    }
}