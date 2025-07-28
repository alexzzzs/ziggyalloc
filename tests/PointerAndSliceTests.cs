using System;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class PointerAndSliceTests
    {
        [Fact]
        public void Pointer_BasicOperations_Work()
        {
            var allocator = new ManualAllocator();
            var ptr = allocator.Alloc<int>(5);
            
            // Test indexing
            ptr[0] = 10;
            ptr[1] = 20;
            ptr[2] = 30;
            
            Assert.Equal(10, ptr[0]);
            Assert.Equal(20, ptr[1]);
            Assert.Equal(30, ptr[2]);
            
            // Test AsSpan
            var span = ptr.AsSpan(3);
            Assert.Equal(3, span.Length);
            Assert.Equal(10, span[0]);
            Assert.Equal(20, span[1]);
            Assert.Equal(30, span[2]);
            
            allocator.Free(ptr.Raw);
        }

        [Fact]
        public void Slice_BasicOperations_Work()
        {
            var allocator = new ManualAllocator();
            var slice = allocator.Slice<int>(5);
            
            Assert.Equal(5, slice.Length);
            Assert.False(slice.IsEmpty);
            
            // Test indexing
            slice[0] = 100;
            slice[4] = 500;
            
            Assert.Equal(100, slice[0]);
            Assert.Equal(500, slice[4]);
            
            allocator.Free(slice.Ptr.Raw);
        }

        [Fact]
        public void Slice_IndexOutOfRange_ThrowsException()
        {
            var allocator = new ManualAllocator();
            var slice = allocator.Slice<int>(3);
            
            // Test out of range access
            bool threw1 = false, threw2 = false;
            try { var _ = slice[3]; } catch (IndexOutOfRangeException) { threw1 = true; }
            try { var _ = slice[-1]; } catch (IndexOutOfRangeException) { threw2 = true; }
            
            Assert.True(threw1, "Should throw IndexOutOfRangeException for index 3");
            Assert.True(threw2, "Should throw IndexOutOfRangeException for index -1");
            
            allocator.Free(slice.Ptr.Raw);
        }

        [Fact]
        public void Slice_ImplicitConversions_Work()
        {
            var allocator = new ManualAllocator();
            var slice = allocator.Slice<int>(3, zeroed: true);
            
            slice[0] = 1;
            slice[1] = 2;
            slice[2] = 3;
            
            // Test implicit conversion to Span<T>
            Span<int> span = slice.AsSpan();
            Assert.Equal(3, span.Length);
            Assert.Equal(1, span[0]);
            
            // Test implicit conversion to ReadOnlySpan<T>
            ReadOnlySpan<int> readOnlySpan = slice.AsSpan();
            Assert.Equal(3, readOnlySpan.Length);
            Assert.Equal(2, readOnlySpan[1]);
            
            allocator.Free(slice.Ptr.Raw);
        }
    }
}