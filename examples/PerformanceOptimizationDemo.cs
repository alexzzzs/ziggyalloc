using System;
using ZiggyAlloc;

namespace ZiggyAlloc.Examples
{
    public class PerformanceOptimizationDemo
    {
        public static void Run()
        {
            Console.WriteLine("ZiggyAlloc Performance Optimization Demo");
            Console.WriteLine("=======================================");
            
            // Test UnmanagedMemoryPool
            TestUnmanagedMemoryPool();
            
            // Test HybridAllocator
            TestHybridAllocator();
            
            Console.WriteLine("\nAll tests completed successfully!");
        }
        
        static void TestUnmanagedMemoryPool()
        {
            Console.WriteLine("\n--- Testing UnmanagedMemoryPool ---");
            
            var systemAllocator = new SystemMemoryAllocator();
            var poolAllocator = new UnmanagedMemoryPool(systemAllocator);
            
            // Allocate and dispose multiple buffers of the same size
            Console.WriteLine("Allocating and disposing 5 buffers of 100 ints each:");
            for (int i = 0; i < 5; i++)
            {
                using var buffer = poolAllocator.Allocate<int>(100);
                buffer[0] = i;
                buffer[99] = i * 2;
                Console.WriteLine($"  Buffer {i}: First={buffer[0]}, Last={buffer[99]}");
            }
            
            // Pre-warm the pool
            Console.WriteLine("Pre-warming the pool with 10 buffers of 50 bytes each:");
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
            Console.WriteLine("Allocating from warmed pool:");
            using var pooledBuffer = poolAllocator.Allocate<byte>(50);
            pooledBuffer[0] = 42;
            Console.WriteLine($"  Pooled buffer first element: {pooledBuffer[0]}");
            
            poolAllocator.Dispose();
            Console.WriteLine("UnmanagedMemoryPool test completed.");
        }
        
        static void TestHybridAllocator()
        {
            Console.WriteLine("\n--- Testing HybridAllocator ---");
            
            var systemAllocator = new SystemMemoryAllocator();
            var hybridAllocator = new HybridAllocator(systemAllocator);
            
            // Test small allocations (should use managed allocation)
            Console.WriteLine("Testing small byte array allocation (100 elements):");
            using var smallBuffer = hybridAllocator.Allocate<byte>(100);
            smallBuffer[0] = 1;
            smallBuffer[99] = 2;
            Console.WriteLine($"  Small buffer: First={smallBuffer[0]}, Last={smallBuffer[99]}");
            
            // Test large allocations (should use unmanaged allocation)
            Console.WriteLine("Testing large double array allocation (10,000 elements):");
            using var largeBuffer = hybridAllocator.Allocate<double>(10000);
            largeBuffer[0] = 1.5;
            largeBuffer[9999] = 2.5;
            Console.WriteLine($"  Large buffer: First={largeBuffer[0]}, Last={largeBuffer[9999]}");
            
            Console.WriteLine("HybridAllocator test completed.");
        }
    }
}