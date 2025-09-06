using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ZiggyAlloc;

namespace ZiggyAlloc.Examples.Advanced
{
    /// <summary>
    /// Example demonstrating the SlabAllocator for high-frequency small allocations.
    /// </summary>
    public class SlabAllocatorExample
    {
        public static void Run()
        {
            Console.WriteLine("=== SlabAllocator Example ===\n");

            // Create allocators for comparison
            var systemAllocator = new SystemMemoryAllocator();
            var slabAllocator = new SlabAllocator(systemAllocator);

            // Example 1: Basic usage
            Console.WriteLine("1. Basic Slab Allocation:");
            using (var buffer = slabAllocator.Allocate<int>(100))
            {
                // Fill the buffer
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = i * 2;
                }
                Console.WriteLine($"   Allocated {buffer.Length} integers, first few: {buffer[0]}, {buffer[1]}, {buffer[2]}");
            }
            Console.WriteLine();

            // Example 2: Performance comparison
            Console.WriteLine("2. Performance Comparison:");
            PerformanceComparison();
            Console.WriteLine();

            // Example 3: High-frequency allocation scenario
            Console.WriteLine("3. High-Frequency Allocation:");
            HighFrequencyAllocation();
            Console.WriteLine();

            Console.WriteLine($"Total allocated bytes: {slabAllocator.TotalAllocatedBytes:N0}");
            
            // Clean up
            slabAllocator.Dispose();
        }

        static void PerformanceComparison()
        {
            var systemAllocator = new SystemMemoryAllocator();
            var slabAllocator = new SlabAllocator(systemAllocator);

            const int iterations = 10000;
            const int bufferSize = 256; // Small buffer size, ideal for slab allocation

            // Time system allocator
            var stopwatch1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var buffer = systemAllocator.Allocate<int>(bufferSize);
                // Simulate some work
                buffer[0] = i;
            }
            stopwatch1.Stop();

            // Time slab allocator
            var stopwatch2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var buffer = slabAllocator.Allocate<int>(bufferSize);
                // Simulate some work
                buffer[0] = i;
            }
            stopwatch2.Stop();

            Console.WriteLine($"   System allocator: {stopwatch1.ElapsedMilliseconds} ms");
            Console.WriteLine($"   Slab allocator: {stopwatch2.ElapsedMilliseconds} ms");
            Console.WriteLine($"   Performance improvement: {(double)stopwatch1.ElapsedMilliseconds / stopwatch2.ElapsedMilliseconds:F2}x");
            
            slabAllocator.Dispose();
        }

        static void HighFrequencyAllocation()
        {
            var systemAllocator = new SystemMemoryAllocator();
            var slabAllocator = new SlabAllocator(systemAllocator);

            // Simulate a high-frequency allocation scenario
            const int allocationCount = 50000;
            const int bufferSize = 128; // Small, consistent size

            // Using system allocator
            var stopwatch1 = Stopwatch.StartNew();
            for (int i = 0; i < allocationCount; i++)
            {
                using var buffer = systemAllocator.Allocate<byte>(bufferSize);
                // Simulate work
                buffer[0] = (byte)(i % 256);
            }
            stopwatch1.Stop();

            // Using slab allocator
            var stopwatch2 = Stopwatch.StartNew();
            for (int i = 0; i < allocationCount; i++)
            {
                using var buffer = slabAllocator.Allocate<byte>(bufferSize);
                // Simulate work
                buffer[0] = (byte)(i % 256);
            }
            stopwatch2.Stop();

            Console.WriteLine($"   High-frequency allocations ({allocationCount:N0} times {bufferSize} bytes):");
            Console.WriteLine($"   System allocator: {stopwatch1.ElapsedMilliseconds} ms");
            Console.WriteLine($"   Slab allocator: {stopwatch2.ElapsedMilliseconds} ms");
            Console.WriteLine($"   Performance improvement: {(double)stopwatch1.ElapsedMilliseconds / stopwatch2.ElapsedMilliseconds:F2}x");
            
            slabAllocator.Dispose();
        }
    }
}