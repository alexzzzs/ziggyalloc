using System;
using ZiggyAlloc;

namespace ZiggyAlloc.Examples.Basic
{
    /// <summary>
    /// Simple allocation example demonstrating basic usage of ZiggyAlloc.
    /// </summary>
    public class SimpleAllocation
    {
        public static void Run()
        {
            Console.WriteLine("=== Simple Allocation Example ===\n");

            // Create a system memory allocator
            var allocator = new SystemMemoryAllocator();

            // Allocate an integer buffer
            using var intBuffer = allocator.Allocate<int>(10);
            
            // Fill the buffer with values
            for (int i = 0; i < intBuffer.Length; i++)
            {
                intBuffer[i] = i * i; // Square of index
            }

            // Display some values
            Console.WriteLine($"Buffer length: {intBuffer.Length}");
            Console.WriteLine($"First 5 values: {intBuffer[0]}, {intBuffer[1]}, {intBuffer[2]}, {intBuffer[3]}, {intBuffer[4]}");

            // Convert to Span for high-performance operations
            Span<int> span = intBuffer;
            int sum = 0;
            foreach (int value in span)
            {
                sum += value;
            }
            
            Console.WriteLine($"Sum of all values: {sum}");
            Console.WriteLine($"Raw pointer: 0x{intBuffer.RawPointer:X}");
            
            // Memory is automatically freed when the buffer is disposed
            Console.WriteLine("\nBuffer will be automatically disposed when exiting scope.");
        }
    }
}