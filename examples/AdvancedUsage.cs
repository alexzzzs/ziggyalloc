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
            
            // Example 5: Defer scope patterns
            DemonstrateDeferPatterns();
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

        static void DemonstrateDeferPatterns()
        {
            Console.WriteLine("5. Defer Scope Patterns (Zig-style)");
            Console.WriteLine("-----------------------------------");

            var allocator = new SystemMemoryAllocator();

            // Pattern 1: Simple defer with multiple resources
            Console.WriteLine("Pattern 1: Multiple resource cleanup");
            using (var defer = DeferScope.Start())
            {
                var buffer1 = allocator.AllocateDeferred<int>(defer, 10);
                var buffer2 = allocator.AllocateDeferred<double>(defer, 20);
                var buffer3 = allocator.AllocateDeferred<byte>(defer, 100);

                // Use the buffers
                buffer1[0] = 42;
                buffer2[0] = 3.14159;
                buffer3[0] = 0xFF;

                Console.WriteLine($"  Allocated {defer.Count} buffers");
                Console.WriteLine($"  Values: {buffer1[0]}, {buffer2[0]:F3}, {buffer3[0]:X2}");
                
                // Add custom cleanup
                defer.Defer(() => Console.WriteLine("  ✓ Custom cleanup completed"));
            } // All cleanup happens in reverse order

            // Pattern 2: Nested defer scopes
            Console.WriteLine("\nPattern 2: Nested defer scopes");
            using (var outerDefer = DeferScope.Start())
            {
                var outerBuffer = allocator.AllocateDeferred<int>(outerDefer, 5);
                outerDefer.Defer(() => Console.WriteLine("  ✓ Outer scope cleanup"));

                using (var innerDefer = DeferScope.Start())
                {
                    var innerBuffer = allocator.AllocateDeferred<float>(innerDefer, 3);
                    innerDefer.Defer(() => Console.WriteLine("  ✓ Inner scope cleanup"));
                    
                    outerBuffer[0] = 100;
                    innerBuffer[0] = 2.5f;
                    
                    Console.WriteLine($"  Outer: {outerBuffer[0]}, Inner: {innerBuffer[0]}");
                } // Inner cleanup happens first
            } // Then outer cleanup

            // Pattern 3: Error handling with defer
            Console.WriteLine("\nPattern 3: Error handling with defer");
            try
            {
                using var defer = DeferScope.Start();
                
                var buffer = allocator.AllocateDeferred<int>(defer, 10);
                defer.Defer(() => Console.WriteLine("  ✓ Cleanup executed despite exception"));
                
                buffer[0] = 42;
                Console.WriteLine($"  Buffer value: {buffer[0]}");
                
                // Simulate an error
                throw new InvalidOperationException("Simulated error");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"  Caught exception: {ex.Message}");
                Console.WriteLine("  (Notice cleanup still happened)");
            }

            // Pattern 4: Complex resource management
            Console.WriteLine("\nPattern 4: Complex resource management");
            using (var defer = DeferScope.Start())
            {
                // Simulate opening a file
                defer.Defer(() => Console.WriteLine("  ✓ File closed"));
                Console.WriteLine("  File opened");

                // Allocate processing buffer
                var processingBuffer = allocator.AllocateDeferred<byte>(defer, 1024);
                Console.WriteLine("  Processing buffer allocated");

                // Simulate network connection
                defer.Defer(() => Console.WriteLine("  ✓ Network connection closed"));
                Console.WriteLine("  Network connection opened");

                // Allocate network buffer
                var networkBuffer = allocator.AllocateDeferred<byte>(defer, 2048);
                Console.WriteLine("  Network buffer allocated");

                // Use the resources
                processingBuffer[0] = 0xAA;
                networkBuffer[0] = 0xBB;
                
                Console.WriteLine($"  Processing: 0x{processingBuffer[0]:X2}, Network: 0x{networkBuffer[0]:X2}");
                Console.WriteLine("  All resources will be cleaned up in reverse order...");
            }

            Console.WriteLine($"\nTotal allocator usage: {allocator.TotalAllocatedBytes} bytes");
            Console.WriteLine();
        }

        // Example struct for interop
        struct Point3D
        {
            public float X, Y, Z;
        }
    }
}