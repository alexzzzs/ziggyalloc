using System;
using System.Text;
using ZiggyAlloc;

namespace ZiggyAlloc.Examples
{
    public static class AdvancedUsage
    {
        public static void RunAdvancedExamples()
        {
            Console.WriteLine("=== ZiggyAlloc Advanced Usage Examples ===\n");

            // Example 1: Different allocator types
            DemonstrateAllocatorTypes();
            
            // Example 2: Memory leak detection
            DemonstrateLeakDetection();
            
            // Example 3: High-performance buffer operations
            DemonstrateHighPerformance();
            
            // Example 4: Interop scenarios
            DemonstrateInterop();
        }

        static void DemonstrateAllocatorTypes()
        {
            Console.WriteLine("1. Allocator Types Demo");
            Console.WriteLine("----------------------");

            // Manual allocator - explicit control
            Console.WriteLine("Manual Allocator:");
            var manual = new ManualAllocator();
            var ptr = manual.Alloc<int>(10);
            for (int i = 0; i < 10; i++) ptr[i] = i * i;
            Console.WriteLine($"Allocated array, first few values: {ptr[0]}, {ptr[1]}, {ptr[2]}");
            manual.Free(ptr.Raw);
            Console.WriteLine("Manually freed\n");

            // Scoped allocator - automatic cleanup
            Console.WriteLine("Scoped Allocator:");
            using (var scoped = new ScopedAllocator())
            {
                var slice1 = scoped.Slice<double>(5, zeroed: true);
                var slice2 = scoped.Slice<byte>(100);
                
                slice1[0] = 3.14159;
                slice2[0] = 0xFF;
                
                Console.WriteLine($"Allocated multiple arrays: double[0]={slice1[0]}, byte[0]={slice2[0]:X2}");
                Console.WriteLine("Will auto-free on scope exit");
            }
            Console.WriteLine("Scoped allocator disposed, all memory freed\n");
        }

        static void DemonstrateLeakDetection()
        {
            Console.WriteLine("2. Memory Leak Detection Demo");
            Console.WriteLine("-----------------------------");

            try
            {
                using var debugAlloc = new DebugAllocator("LeakTest", Z.DefaultAllocator, LeakReportingMode.Throw);
                
                var ptr1 = debugAlloc.Alloc<int>();
                ptr1.Value = 42;
                debugAlloc.Free(ptr1.Raw); // Properly freed
                
                var ptr2 = debugAlloc.Alloc<double>(5);
                ptr2[0] = 1.23;
                // Intentionally not freeing ptr2 to demonstrate leak detection
                
                Console.WriteLine("Created allocations, one will leak...");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Leak detected: {ex.Message.Split('\n')[0]}");
            }
            Console.WriteLine();
        }

        static void DemonstrateHighPerformance()
        {
            Console.WriteLine("3. High-Performance Buffer Operations");
            Console.WriteLine("------------------------------------");

            using var allocator = new ScopedAllocator();
            var ctx = new Ctx(allocator, Z.ctx.@out, Z.ctx.@in);

            // Large buffer allocation
            const int bufferSize = 1024 * 1024; // 1MB
            var buffer = ctx.AllocSlice<byte>(bufferSize, zeroed: true);
            
            // Fast memory operations using Span<T>
            var span = buffer.AsSpan();
            
            // Fill with pattern
            for (int i = 0; i < span.Length; i += 4)
            {
                if (i + 3 < span.Length)
                {
                    span[i] = 0xDE;
                    span[i + 1] = 0xAD;
                    span[i + 2] = 0xBE;
                    span[i + 3] = 0xEF;
                }
            }

            Console.WriteLine($"Filled {bufferSize:N0} byte buffer with pattern");
            Console.WriteLine($"First 8 bytes: {span[0]:X2} {span[1]:X2} {span[2]:X2} {span[3]:X2} {span[4]:X2} {span[5]:X2} {span[6]:X2} {span[7]:X2}");
            Console.WriteLine();
        }

        static void DemonstrateInterop()
        {
            Console.WriteLine("4. Interop Scenarios");
            Console.WriteLine("-------------------");

            using var defer = DeferScope.Start();
            var ctx = new Ctx(Z.DefaultAllocator, Z.ctx.@out, Z.ctx.@in);

            // Simulate preparing data for native API call
            var structArray = ctx.AllocSlice<Point3D>(defer, 1000, zeroed: true);
            
            // Fill with sample data
            for (int i = 0; i < structArray.Length; i++)
            {
                structArray[i] = new Point3D
                {
                    X = i * 0.1f,
                    Y = i * 0.2f,
                    Z = i * 0.3f
                };
            }

            // Get raw pointer for native interop
            var rawPtr = structArray.Ptr.Raw;
            Console.WriteLine($"Prepared {structArray.Length} Point3D structs for native API");
            Console.WriteLine($"Raw pointer: 0x{rawPtr:X}");
            Console.WriteLine($"Sample point: ({structArray[100].X:F1}, {structArray[100].Y:F1}, {structArray[100].Z:F1})");
            
            // In real scenario, you'd pass rawPtr to native function
            // SimulateNativeApiCall(rawPtr, structArray.Length);
            
            Console.WriteLine("Memory will be automatically freed by defer scope");
        }

        // Example struct for interop
        struct Point3D
        {
            public float X, Y, Z;
        }
    }
}