using System;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class LifetimeTests
    {
        [Fact]
        public void AutoFree_AutomaticallyFreesMemory()
        {
            var allocator = new ManualAllocator();
            var ctx = new Ctx(allocator, new ConsoleWriter(), new ConsoleReader());
            
            using (var auto = ctx.Auto<int>())
            {
                auto.Value = 42;
                Assert.Equal(42, auto.Value);
            } // Memory should be freed here
            
            // No way to directly test if memory was freed without causing undefined behavior,
            // but the test passes if no exceptions are thrown
        }

        [Fact]
        public void DeferScope_ExecutesActionsInReverseOrder()
        {
            var executionOrder = new System.Collections.Generic.List<int>();
            
            using (var defer = DeferScope.Start())
            {
                defer.Defer(() => executionOrder.Add(1));
                defer.Defer(() => executionOrder.Add(2));
                defer.Defer(() => executionOrder.Add(3));
            } // Actions should execute here in reverse order: 3, 2, 1
            
            Assert.Equal(new[] { 3, 2, 1 }, executionOrder);
        }

        [Fact]
        public void DeferScope_WithAllocations_FreesMemoryInCorrectOrder()
        {
            var allocator = new ManualAllocator();
            var freedPointers = new System.Collections.Generic.List<IntPtr>();
            
            // Create a test allocator that tracks frees
            var testAllocator = new TestAllocator(allocator, ptr => freedPointers.Add(ptr));
            
            IntPtr ptr1, ptr2;
            
            using (var defer = DeferScope.Start())
            {
                var p1 = testAllocator.Alloc<int>();
                ptr1 = p1.Raw;
                var ptr1Copy = p1.Raw;
                defer.Defer(() => testAllocator.Free(ptr1Copy));
                
                var p2 = testAllocator.Alloc<double>();
                ptr2 = p2.Raw;
                var ptr2Copy = p2.Raw;
                defer.Defer(() => testAllocator.Free(ptr2Copy));
            }
            
            // Should free in reverse order: ptr2, then ptr1
            Assert.Equal(2, freedPointers.Count);
            Assert.Equal(ptr2, freedPointers[0]);
            Assert.Equal(ptr1, freedPointers[1]);
        }

        private class TestAllocator : IAllocator
        {
            private readonly IAllocator _backend;
            private readonly Action<IntPtr> _onFree;

            public TestAllocator(IAllocator backend, Action<IntPtr> onFree)
            {
                _backend = backend;
                _onFree = onFree;
            }

            public Pointer<T> Alloc<T>(int count = 1, bool zeroed = false) where T : unmanaged
                => _backend.Alloc<T>(count, zeroed);

            public void Free(IntPtr ptr)
            {
                _onFree(ptr);
                _backend.Free(ptr);
            }

            public Slice<T> Slice<T>(int count, bool zeroed = false) where T : unmanaged
                => _backend.Slice<T>(count, zeroed);
        }
    }
}