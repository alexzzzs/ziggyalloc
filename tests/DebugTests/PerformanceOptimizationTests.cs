using System;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class PerformanceOptimizationTests
    {
        public static void TestUnmanagedMemoryPool()
        {
            Console.WriteLine("\nTesting UnmanagedMemoryPool...");
            
            var systemAllocator = new SystemMemoryAllocator();
            var poolAllocator = new UnmanagedMemoryPool(systemAllocator);
            
            // Allocate and dispose multiple buffers of the same size
            for (int i = 0; i < 5; i++)
            {
                using var buffer = poolAllocator.Allocate<int>(100);
                buffer[0] = i;
                buffer[99] = i * 2;
                Console.WriteLine($"Buffer {i}: First={buffer[0]}, Last={buffer[99]}");
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
            Console.WriteLine($"Pooled buffer first element: {pooledBuffer[0]}");
            
            poolAllocator.Dispose();
            Console.WriteLine("UnmanagedMemoryPool test completed.");
        }
        
        public static void TestHybridAllocator()
        {
            Console.WriteLine("\nTesting HybridAllocator...");
            
            var systemAllocator = new SystemMemoryAllocator();
            var hybridAllocator = new HybridAllocator(systemAllocator);
            
            // Test small allocations (should use managed allocation)
            using var smallBuffer = hybridAllocator.Allocate<byte>(100);
            smallBuffer[0] = 1;
            smallBuffer[99] = 2;
            Console.WriteLine($"Small buffer: First={smallBuffer[0]}, Last={smallBuffer[99]}");
            
            // Test large allocations (should use unmanaged allocation)
            using var largeBuffer = hybridAllocator.Allocate<double>(10000);
            largeBuffer[0] = 1.5;
            largeBuffer[9999] = 2.5;
            Console.WriteLine($"Large buffer: First={largeBuffer[0]}, Last={largeBuffer[9999]}");
            
            Console.WriteLine("HybridAllocator test completed.");
        }
    }
}