using System;
using System.Diagnostics;
using ZiggyAlloc;

namespace ZiggyAlloc.Examples.Performance
{
    /// <summary>
    /// Example demonstrating the performance benefits of UnmanagedMemoryPool
    /// for frequent allocations of similar-sized buffers.
    /// </summary>
    public class UnmanagedMemoryPoolExample
    {
        public static void Run()
        {
            Console.WriteLine("=== UnmanagedMemoryPool Performance Example ===\n");

            var systemAllocator = new SystemMemoryAllocator();
            
            // Compare regular allocations vs pooled allocations
            Console.WriteLine("1. Comparing regular vs pooled allocations:");
            
            // Regular allocations
            var regularStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                using var buffer = systemAllocator.Allocate<int>(100);
                // Simulate some work
                buffer[0] = i;
            }
            regularStopwatch.Stop();
            
            // Pooled allocations
            using var pool = new UnmanagedMemoryPool(systemAllocator);
            var pooledStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                using var buffer = pool.Allocate<int>(100);
                // Simulate some work
                buffer[0] = i;
            }
            pooledStopwatch.Stop();
            
            Console.WriteLine($"   Regular allocations: {regularStopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"   Pooled allocations:  {pooledStopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"   Performance improvement: {(double)regularStopwatch.ElapsedMilliseconds / pooledStopwatch.ElapsedMilliseconds:F2}x\n");

            // Demonstrate pool warming
            Console.WriteLine("2. Pool warming example:");
            Console.WriteLine("   Pre-allocating buffers to avoid allocation overhead...");
            
            // Warm up the pool with buffers of various sizes
            var warmupStopwatch = Stopwatch.StartNew();
            var warmupBuffers = new UnmanagedBuffer<byte>[100];
            for (int i = 0; i < 100; i++)
            {
                warmupBuffers[i] = pool.Allocate<byte>(50);
            }
            
            // Return buffers to pool
            for (int i = 0; i < 100; i++)
            {
                warmupBuffers[i].Dispose();
            }
            warmupStopwatch.Stop();
            
            Console.WriteLine($"   Warmed up pool with 100 buffers in {warmupStopwatch.ElapsedMilliseconds} ms");
            
            // Now allocate from the warmed pool
            var warmedStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                using var buffer = pool.Allocate<byte>(50);
                buffer[0] = (byte)(i % 256);
            }
            warmedStopwatch.Stop();
            
            Console.WriteLine($"   Allocated 100 buffers from warmed pool in {warmedStopwatch.ElapsedMilliseconds} ms\n");

            // Show pool statistics
            Console.WriteLine("3. Pool statistics:");
            Console.WriteLine($"   Total allocated by pool: {pool.TotalAllocatedBytes:N0} bytes");
            Console.WriteLine($"   Supports individual deallocation: {pool.SupportsIndividualDeallocation}");
            
            Console.WriteLine("\nUnmanagedMemoryPool helps reduce allocation overhead by reusing previously allocated buffers.");
        }
    }
}