using System;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class LifetimeTests
    {
        [Fact]
        public void UnmanagedBuffer_UsingStatement_AutomaticallyDisposesMemory()
        {
            var allocator = new SystemMemoryAllocator();
            
            // Test that using statement properly disposes the buffer
            using (var buffer = allocator.Allocate<int>(10))
            {
                buffer[0] = 42;
                Assert.Equal(42, buffer[0]);
                Assert.True(buffer.IsValid);
            } // Memory should be freed here
            
            // No way to directly test if memory was freed without causing undefined behavior,
            // but the test passes if no exceptions are thrown during disposal
        }

        [Fact]
        public void ScopedAllocator_DisposesAllAllocationsAtOnce()
        {
            using (var scopedAllocator = new ScopedMemoryAllocator())
            {
                // Allocate multiple buffers
                using var buffer1 = scopedAllocator.Allocate<int>(100);
                using var buffer2 = scopedAllocator.Allocate<double>(50);
                using var buffer3 = scopedAllocator.Allocate<byte>(1000);
                
                // Verify they work
                buffer1[0] = 42;
                buffer2[0] = 3.14;
                buffer3[0] = 255;
                
                Assert.Equal(42, buffer1[0]);
                Assert.Equal(3.14, buffer2[0]);
                Assert.Equal(255, buffer3[0]);
                
                // All allocations should be tracked
                Assert.True(scopedAllocator.TotalAllocatedBytes > 0);
            } // All memory should be freed when scoped allocator is disposed
        }

        [Fact]
        public void DebugAllocator_TracksAllocationLifetime()
        {
            var backend = new SystemMemoryAllocator();
            using var debugAllocator = new DebugMemoryAllocator("Test", backend);
            
            // Initially no allocations
            Assert.Equal(0, debugAllocator.GetTrackedAllocationCount());
            
            // Allocate and verify tracking
            using (var buffer = debugAllocator.Allocate<int>(10))
            {
                Assert.Equal(1, debugAllocator.GetTrackedAllocationCount());
                buffer[0] = 42;
            } // Buffer disposed here
            
            // Should be no tracked allocations after disposal
            Assert.Equal(0, debugAllocator.GetTrackedAllocationCount());
        }

        [Fact]
        public void MultipleAllocators_IndependentLifetimes()
        {
            var allocator1 = new SystemMemoryAllocator();
            var allocator2 = new SystemMemoryAllocator();
            
            using var buffer1 = allocator1.Allocate<int>(100);
            using var buffer2 = allocator2.Allocate<double>(50);
            
            // Each allocator tracks its own allocations
            Assert.True(allocator1.TotalAllocatedBytes >= 100 * sizeof(int));
            Assert.True(allocator2.TotalAllocatedBytes >= 50 * sizeof(double));
            
            // Buffers should be independent
            buffer1[0] = 42;
            buffer2[0] = 3.14;
            
            Assert.Equal(42, buffer1[0]);
            Assert.Equal(3.14, buffer2[0]);
        }

        [Fact]
        public void UnmanagedBuffer_FirstAndLastProperties_Work()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(5);
            
            // Set first and last elements
            buffer.First = 100;
            buffer.Last = 200;
            
            Assert.Equal(100, buffer[0]);
            Assert.Equal(200, buffer[4]);
            Assert.Equal(100, buffer.First);
            Assert.Equal(200, buffer.Last);
        }

        [Fact]
        public void UnmanagedBuffer_EmptyBuffer_HandledCorrectly()
        {
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(0);
            
            Assert.True(buffer.IsEmpty);
            Assert.Equal(0, buffer.Length);
            Assert.Equal(0, buffer.SizeInBytes);
            
            // Should throw when accessing First/Last on empty buffer
            Assert.Throws<InvalidOperationException>(() => buffer.First);
            Assert.Throws<InvalidOperationException>(() => buffer.Last);
        }
    }
}