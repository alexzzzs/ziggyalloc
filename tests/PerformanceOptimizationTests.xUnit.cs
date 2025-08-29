using System;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class PerformanceOptimizationXUnitTests
    {
        [Fact]
        public void UnmanagedMemoryPool_AllocatesAndReusesBuffers()
        {
            var systemAllocator = new SystemMemoryAllocator();
            var poolAllocator = new UnmanagedMemoryPool(systemAllocator);
            
            // Allocate and dispose multiple buffers of the same size
            for (int i = 0; i < 5; i++)
            {
                using var buffer = poolAllocator.Allocate<int>(100);
                buffer[0] = i;
                buffer[99] = i * 2;
                Assert.Equal(i, buffer[0]);
                Assert.Equal(i * 2, buffer[99]);
            }
            
            // Pre-warm the pool
            var warmupBuffers = new UnmanagedBuffer<byte>[10];
            for (int i = 0; i < 10; i++)
            {
                warmupBuffers[i] = poolAllocator.Allocate<byte>(50);
            }
            
            for (int i = 0; i < 10; i++)
            {
                warmupBuffers[i].Dispose();
            }
            
            // Allocate from warmed pool
            using var pooledBuffer = poolAllocator.Allocate<byte>(50);
            pooledBuffer[0] = 42;
            Assert.Equal((byte)42, pooledBuffer[0]);
            
            poolAllocator.Dispose();
        }
        
        [Fact]
        public void HybridAllocator_ChoosesAppropriateAllocationStrategy()
        {
            var systemAllocator = new SystemMemoryAllocator();
            var hybridAllocator = new HybridAllocator(systemAllocator);
            
            // Test small allocations (should use managed allocation)
            using var smallBuffer = hybridAllocator.Allocate<byte>(100);
            smallBuffer[0] = 1;
            smallBuffer[99] = 2;
            Assert.Equal((byte)1, smallBuffer[0]);
            Assert.Equal((byte)2, smallBuffer[99]);
            
            // Test large allocations (should use unmanaged allocation)
            using var largeBuffer = hybridAllocator.Allocate<double>(10000);
            largeBuffer[0] = 1.5;
            largeBuffer[9999] = 2.5;
            Assert.Equal(1.5, largeBuffer[0]);
            Assert.Equal(2.5, largeBuffer[9999]);
        }
        
        [Fact]
        public void UnmanagedMemoryPool_TracksAllocatedBytes()
        {
            var systemAllocator = new SystemMemoryAllocator();
            var poolAllocator = new UnmanagedMemoryPool(systemAllocator);
            
            var initialBytes = poolAllocator.TotalAllocatedBytes;
            
            // Allocate some buffers
            using var buffer1 = poolAllocator.Allocate<int>(100);
            using var buffer2 = poolAllocator.Allocate<byte>(200);
            
            var allocatedBytes = poolAllocator.TotalAllocatedBytes;
            Assert.True(allocatedBytes > initialBytes);
            
            poolAllocator.Dispose();
        }
        
        [Fact]
        public void HybridAllocator_TracksAllocatedBytes()
        {
            var systemAllocator = new SystemMemoryAllocator();
            var hybridAllocator = new HybridAllocator(systemAllocator);
            
            var initialBytes = hybridAllocator.TotalAllocatedBytes;
            
            // Allocate some buffers
            using var buffer1 = hybridAllocator.Allocate<int>(100);
            using var buffer2 = hybridAllocator.Allocate<byte>(200);
            
            var allocatedBytes = hybridAllocator.TotalAllocatedBytes;
            Assert.True(allocatedBytes > initialBytes);
        }
    }
}