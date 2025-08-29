using System;
using ZiggyAlloc;

namespace ZiggyAlloc.Examples
{
    public static class BasicUsage
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== ZiggyAlloc: High-Performance Unmanaged Memory ===\n");
            
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "advanced":
                        AdvancedUsage.RunAdvancedExamples();
                        return;
                    case "performance":
                        PerformanceOptimizationDemo.Run();
                        return;
                    case "realworld":
                        RealWorldUsage.RunExamples();
                        return;
                }
            }
            
            RunBasicExample();
            
            Console.WriteLine("\n" + new string('=', 60) + "\n");
            
            RealWorldUsage.RunExamples();
            
            Console.WriteLine("\n" + new string('=', 60) + "\n");
            
            Console.WriteLine("To run other examples:");
            Console.WriteLine("  dotnet run -- advanced     - Advanced usage examples");
            Console.WriteLine("  dotnet run -- performance  - Performance optimization examples");
            Console.WriteLine("  dotnet run -- realworld    - Real-world usage examples");
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

            // 3. Defer scope example - Zig-style cleanup
            Console.WriteLine("\n✓ Defer scope example (Zig-style):");
            using (var defer = DeferScope.Start())
            {
                // Allocate multiple buffers with deferred cleanup
                var buffer1 = allocator.AllocateDeferred<int>(defer, 100);
                var buffer2 = allocator.AllocateDeferred<float>(defer, 50);
                var buffer3 = allocator.AllocateDeferred<byte>(defer, 1000);

                // Fill with data
                buffer1[0] = 42;
                buffer2[0] = 3.14f;
                buffer3[0] = 255;

                Console.WriteLine($"  Allocated 3 buffers with defer scope");
                Console.WriteLine($"  Values: {buffer1[0]}, {buffer2[0]:F2}, {buffer3[0]}");
                Console.WriteLine($"  Defer count: {defer.Count} cleanup actions");
                
                // Custom cleanup action
                defer.Defer(() => Console.WriteLine("  Custom cleanup executed!"));
                
                Console.WriteLine($"  All cleanup will happen in reverse order...");
            } // Cleanup happens here: custom action, buffer3, buffer2, buffer1

            Console.WriteLine($"\n✓ All memory freed, allocator usage: {allocator.TotalAllocatedBytes} bytes");
        }
    }
}