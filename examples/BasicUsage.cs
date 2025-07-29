using System;
using ZiggyAlloc;

namespace ZiggyAlloc.Examples
{
    public static class BasicUsage
    {
        public static void Main()
        {
            Console.WriteLine("=== ZiggyAlloc: High-Performance Unmanaged Memory ===\n");
            
            RunBasicExample();
            
            Console.WriteLine("\n" + new string('=', 60) + "\n");
            
            RealWorldUsage.RunExamples();
        }

        static void RunBasicExample()
        {
            Console.WriteLine("Basic Usage - Simple Memory Management");
            Console.WriteLine("-------------------------------------");

            var allocator = new SystemMemoryAllocator();

            // 1. Basic allocation with automatic cleanup
            using (var buffer = allocator.Allocate<int>(10, zeroMemory: true))
            {
                Console.WriteLine($"✓ Allocated buffer for {buffer.Length} integers");
                Console.WriteLine($"  Size: {buffer.SizeInBytes} bytes");
                Console.WriteLine($"  Raw pointer: 0x{buffer.RawPointer:X}");

                // Fill with data
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = i * i;
                }

                // Use as Span<T> for high-performance operations
                Span<int> span = buffer;
                Console.WriteLine($"  First few values: {span[0]}, {span[1]}, {span[2]}, {span[3]}");
            } // Memory automatically freed here

            // 2. Large allocation demonstrating value over managed arrays
            Console.WriteLine("\n✓ Large allocation (no GC pressure):");
            using (var largeBuffer = allocator.Allocate<double>(1_000_000, zeroMemory: false))
            {
                Console.WriteLine($"  Allocated {largeBuffer.Length:N0} doubles ({largeBuffer.SizeInBytes / (1024 * 1024)}MB)");
                Console.WriteLine($"  Total allocator usage: {allocator.TotalAllocatedBytes / (1024 * 1024)}MB");
                
                // Fast operations using Span<T>
                Span<double> span = largeBuffer;
                span.Fill(Math.PI);
                
                Console.WriteLine($"  Filled with π, first value: {span[0]:F6}");
            }

            Console.WriteLine($"\n✓ All memory freed, allocator usage: {allocator.TotalAllocatedBytes} bytes");
        }
    }
}