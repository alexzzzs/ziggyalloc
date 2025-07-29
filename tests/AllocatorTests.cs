using System;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class AllocatorTests
    {
        [Fact]
        public void SystemMemoryAllocator_BasicAllocation_Works()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(1);
            
            Assert.True(buffer.IsValid);
            buffer[0] = 42;
            Assert.Equal(42, buffer[0]);
        }

        [Fact]
        public void SystemMemoryAllocator_ZeroedAllocation_IsZeroed()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(10, zeroMemory: true);
            
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(0, buffer[i]);
            }
        }

        [Fact]
        public void SystemMemoryAllocator_MultipleElementAllocation_Works()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<double>(5);
            
            Assert.Equal(5, buffer.Length);
            Assert.Equal(5 * sizeof(double), buffer.SizeInBytes);
            
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = i * 3.14;
            }
            
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(i * 3.14, buffer[i]);
            }
        }

        [Fact]
        public void ScopedMemoryAllocator_AutomaticallyFreesAllAllocations()
        {
            using var allocator = new ScopedMemoryAllocator();
            
            using var buffer1 = allocator.Allocate<int>(1);
            using var buffer2 = allocator.Allocate<double>(5);
            using var buffer3 = allocator.Allocate<byte>(100);
            
            buffer1[0] = 123;
            buffer2[0] = 3.14;
            buffer3[0] = 255;
            
            Assert.Equal(123, buffer1[0]);
            Assert.Equal(3.14, buffer2[0]);
            Assert.Equal(255, buffer3[0]);
            
            // Dispose is called automatically, freeing all allocations
        }

        [Fact]
        public void ScopedMemoryAllocator_DoesNotSupportIndividualDeallocation()
        {
            using var allocator = new ScopedMemoryAllocator();
            Assert.False(allocator.SupportsIndividualDeallocation);
            
            Assert.Throws<NotSupportedException>(() => allocator.Free(IntPtr.Zero));
        }

        [Fact]
        public void DebugMemoryAllocator_DetectsLeaks()
        {
            var backend = new SystemMemoryAllocator();
            bool leakDetected = false;
            
            try
            {
                using var debugAllocator = new DebugMemoryAllocator("Test", backend, MemoryLeakReportingMode.Throw);
                var buffer = debugAllocator.Allocate<int>(1);
                buffer[0] = 42;
                // Intentionally not disposing to test leak detection
            }
            catch (InvalidOperationException ex)
            {
                leakDetected = ex.Message.Contains("MEMORY LEAK DETECTED");
            }
            
            Assert.True(leakDetected, "Debug allocator should detect memory leaks");
        }

        [Fact]
        public void DebugMemoryAllocator_NoLeaksWhenProperlyDisposed()
        {
            var backend = new SystemMemoryAllocator();
            var debugAllocator = new DebugMemoryAllocator("Test", backend, MemoryLeakReportingMode.Throw);
            
            // Allocate and immediately dispose
            var buffer = debugAllocator.Allocate<int>(1);
            buffer[0] = 42;
            
            // Check tracking before disposal
            Assert.Equal(1, debugAllocator.GetTrackedAllocationCount());
            
            // Get the pointer before disposal
            var pointer = buffer.RawPointer;
            
            // Dispose the buffer
            buffer.Dispose();
            
            // Should be no tracked allocations after disposal
            Assert.Equal(0, debugAllocator.GetTrackedAllocationCount());
            
            // Debug allocator disposal should not throw
            debugAllocator.Dispose();
        }

        [Fact]
        public void UnmanagedBuffer_BoundsChecking_Works()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(10);
            
            // Valid access should work
            buffer[0] = 42;
            buffer[9] = 100;
            
            // Out of bounds access should throw
            Assert.Throws<IndexOutOfRangeException>(() => buffer[10]);
            Assert.Throws<IndexOutOfRangeException>(() => buffer[-1]);
        }

        [Fact]
        public void UnmanagedBuffer_SpanConversion_Works()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(5);
            
            // Fill buffer
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = i * i;
            }
            
            // Convert to span
            Span<int> span = buffer;
            Assert.Equal(buffer.Length, span.Length);
            
            // Verify data through span
            for (int i = 0; i < span.Length; i++)
            {
                Assert.Equal(i * i, span[i]);
            }
        }

        [Fact]
        public void UnmanagedBuffer_UtilityMethods_Work()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(10);
            
            // Fill with value
            buffer.Fill(42);
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(42, buffer[i]);
            }
            
            // Clear buffer
            buffer.Clear();
            for (int i = 0; i < buffer.Length; i++)
            {
                Assert.Equal(0, buffer[i]);
            }
        }
    }
}