using System;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class AllocatorTests
    {
        [Fact]
        public void ManualAllocator_BasicAllocation_Works()
        {
            var allocator = new ManualMemoryAllocator();
            var ptr = allocator.Allocate<int>();
            
            Assert.False(ptr.IsNull);
            ptr.Value = 42;
            Assert.Equal(42, ptr.Value);
            
            allocator.Free(ptr.Raw);
        }

        [Fact]
        public void ManualAllocator_ZeroedAllocation_IsZeroed()
        {
            var allocator = new ManualMemoryAllocator();
            var slice = allocator.AllocateSlice<int>(10, zeroed: true);
            
            for (int i = 0; i < slice.Length; i++)
            {
                Assert.Equal(0, slice[i]);
            }
            
            allocator.Free(slice.Ptr.Raw);
        }

        [Fact]
        public void ScopedAllocator_AutomaticallyFreesAllAllocations()
        {
            using var allocator = new ScopedMemoryAllocator();
            
            var ptr1 = allocator.Allocate<int>();
            var ptr2 = allocator.Allocate<double>(5);
            var slice = allocator.AllocateSlice<byte>(100);
            
            ptr1.Value = 123;
            ptr2[0] = 3.14;
            slice[0] = 255;
            
            Assert.Equal(123, ptr1.Value);
            Assert.Equal(3.14, ptr2[0]);
            Assert.Equal(255, slice[0]);
            
            // Dispose is called automatically, freeing all allocations
        }

        [Fact]
        public void DebugAllocator_DetectsLeaks()
        {
            var backend = new ManualMemoryAllocator();
            bool leakDetected = false;
            
            try
            {
                using var debugAllocator = new DebugMemoryAllocator("Test", backend, MemoryLeakReportingMode.Throw);
                var ptr = debugAllocator.Allocate<int>();
                ptr.Value = 42;
                // Intentionally not freeing to test leak detection
            }
            catch (InvalidOperationException ex)
            {
                leakDetected = ex.Message.Contains("Memory Leak");
            }
            
            Assert.True(leakDetected, "Debug allocator should detect memory leaks");
        }

        [Fact]
        public void DebugAllocator_NoLeaksWhenProperlyFreed()
        {
            var backend = new ManualAllocator();
            
            using var debugAllocator = new DebugAllocator("Test", backend, LeakReportingMode.Throw);
            var ptr = debugAllocator.Alloc<int>();
            ptr.Value = 42;
            debugAllocator.Free(ptr.Raw);
            
            // Should not throw on dispose
        }
    }
}