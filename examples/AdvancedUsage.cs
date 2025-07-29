using System;
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

            // System allocator - direct control
            Console.WriteLine("System Memory Allocator:");
            var allocator = new SystemMemoryAllocator();
            using var buffer = allocator.Allocate<int>(10, zeroMemory: true);
            
            for (int i = 0; i < buffer.Length; i++) 
                buffer[i] = i * i;
            
            Console.WriteLine($"Allocated array, first few values: {buffer[0]}, {buffer[1]}, {buffer[2]}");
            Console.WriteLine("Memory freed automatically with 'using'\n");

            // Scoped allocator - automatic cleanup
            Console.WriteLine("Scoped Allocator:");
            using (var scoped = new ScopedMemoryAllocator())
            {
                using var buffer1 = scoped.Allocate<double>(5, zeroMemory: true);
                using var buffer2 = scoped.Allocate<byte>(100, zeroMemory: false);
                
                buffer1[0] = 3.14159;
                buffer2[0] = 0xFF;
                
                Console.WriteLine($"Allocated multiple arrays: double[0]={buffer1[0]}, byte[0]={buffer2[0]:X2}");
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
                using var debugAlloc = new DebugMemoryAllocator("LeakTest", Z.DefaultAllocator, MemoryLeakReportingMode.Throw);
                
                using var buffer1 = debugAlloc.Allocate<int>(1);
                buffer1[0] = 42;
                // This buffer is properly disposed
                
                var buffer2 = debugAlloc.Allocate<double>(5);
                buffer2[0] = 1.23;
                // Intentionally not disposing buffer2 to demonstrate leak detection
                
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

            var allocator = new SystemMemoryAllocator();

            // Large buffer allocation
            const int bufferSize = 1024 * 1024; // 1MB
            using var buffer = allocator.Allocate<byte>(bufferSize, zeroMemory: true);
            
            // Fast memory operations using Span<T>
            Span<byte> span = buffer;
            
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
            Console.WriteLine($"Total allocated: {allocator.TotalAllocatedBytes / (1024 * 1024)}MB");
            Console.WriteLine();
        }

        static void DemonstrateInterop()
        {
            Console.WriteLine("4. Interop Scenarios");
            Console.WriteLine("-------------------");

            var allocator = new SystemMemoryAllocator();

            // Simulate preparing data for native API call
            using var structArray = allocator.Allocate<Point3D>(1000, zeroMemory: true);
            
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
            var rawPtr = structArray.RawPointer;
            Console.WriteLine($"Prepared {structArray.Length} Point3D structs for native API");
            Console.WriteLine($"Raw pointer: 0x{rawPtr:X}");
            Console.WriteLine($"Sample point: ({structArray[100].X:F1}, {structArray[100].Y:F1}, {structArray[100].Z:F1})");
            Console.WriteLine($"Buffer size: {structArray.SizeInBytes} bytes");
            
            // In real scenario, you'd pass rawPtr to native function
            // SimulateNativeApiCall(rawPtr, structArray.Length);
            
            Console.WriteLine("Memory will be automatically freed when buffer is disposed");
        }

        // Example struct for interop
        struct Point3D
        {
            public float X, Y, Z;
        }
    }
}