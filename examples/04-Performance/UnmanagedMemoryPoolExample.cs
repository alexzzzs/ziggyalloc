using System;
using System.Threading.Tasks;
using ZiggyAlloc;

namespace ZiggyAlloc.Examples.Performance
{
    /// <summary>
    /// Example demonstrating the UnmanagedMemoryPool for reducing allocation overhead.
    /// </summary>
    public class UnmanagedMemoryPoolExample
    {
        public static void Run()
        {
            Console.WriteLine("=== UnmanagedMemoryPool Example ===\n");

            // Create a system allocator and wrap it in a pool
            var systemAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(systemAllocator);

            // Example 1: Basic pooling
            Console.WriteLine("1. Basic Pooling:");
            using (var buffer1 = pool.Allocate<byte>(1024))
            {
                // Use buffer1
                buffer1.Fill(42);
                Console.WriteLine($"   Allocated {buffer1.Length} bytes, first value: {buffer1[0]}");
            } // Buffer returned to pool when disposed

            // Second allocation of same size reuses from pool
            using (var buffer2 = pool.Allocate<byte>(1024))
            {
                Console.WriteLine($"   Reused buffer from pool, first value: {buffer2[0]} (should be 0)");
            }
            Console.WriteLine();

            // Example 2: Performance comparison
            Console.WriteLine("2. Performance Comparison:");
            PerformanceComparison();
            Console.WriteLine();

            // Example 3: Thread safety
            Console.WriteLine("3. Thread Safety:");
            ThreadSafetyExample();
            Console.WriteLine();

            Console.WriteLine($"Total allocated bytes: {pool.TotalAllocatedBytes:N0}");
        }

        static void PerformanceComparison()
        {
            var systemAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(systemAllocator);

            const int iterations = 1000;
            const int bufferSize = 1024;

            // Without pooling - each allocation calls into the OS
            var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var buffer = systemAllocator.Allocate<byte>(bufferSize);
                // Process buffer...
            }
            stopwatch1.Stop();

            // With pooling - first allocation per size calls OS, subsequent allocations reuse
            var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var buffer = pool.Allocate<byte>(bufferSize);
                // Process buffer...
            }
            stopwatch2.Stop();

            Console.WriteLine($"   System allocator: {stopwatch1.ElapsedMilliseconds} ms");
            Console.WriteLine($"   Pooled allocator: {stopwatch2.ElapsedMilliseconds} ms");
            Console.WriteLine($"   Performance improvement: {(double)stopwatch1.ElapsedMilliseconds / stopwatch2.ElapsedMilliseconds:F2}x");
        }

        static void ThreadSafetyExample()
        {
            var systemAllocator = new SystemMemoryAllocator();
            using var pool = new UnmanagedMemoryPool(systemAllocator);

            // Run allocations in parallel
            var tasks = new Task[10];
            for (int t = 0; t < tasks.Length; t++)
            {
                int taskId = t;
                tasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        using var buffer = pool.Allocate<int>(10 + taskId);
                        // Do some work with the buffer
                        for (int j = 0; j < Math.Min(5, buffer.Length); j++)
                        {
                            buffer[j] = taskId * 1000 + i * 100 + j;
                        }
                    }
                });
            }

            try
            {
                Task.WaitAll(tasks);
                Console.WriteLine("   All parallel allocations completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Error in parallel allocations: {ex.Message}");
            }
        }
    }
}