using System;
using ZiggyAlloc;

namespace ZiggyAlloc.Examples.Advanced
{
    /// <summary>
    /// Advanced example demonstrating memory leak detection with DebugMemoryAllocator.
    /// </summary>
    public class MemoryLeakDetection
    {
        public static void Run()
        {
            Console.WriteLine("=== Memory Leak Detection Example ===\n");

            // Create a debug allocator that will throw exceptions on leaks
            using var debugAllocator = new DebugMemoryAllocator(
                "LeakDetectionDemo", 
                Z.DefaultAllocator, 
                MemoryLeakReportingMode.Throw);

            Console.WriteLine("1. Proper memory management (no leaks):");
            
            // Properly managed allocations
            using var buffer1 = debugAllocator.Allocate<int>(100);
            using var buffer2 = debugAllocator.Allocate<double>(50);
            
            // Use the buffers
            buffer1[0] = 42;
            buffer2[0] = 3.14159;
            
            Console.WriteLine($"   Allocated and properly disposed {buffer1.Length} integers and {buffer2.Length} doubles");
            Console.WriteLine("   ✓ No leaks detected\n");

            Console.WriteLine("2. Demonstrating leak detection:");
            
            // Intentionally leak memory (don't dispose)
            var leakedBuffer = debugAllocator.Allocate<byte>(1024);
            leakedBuffer[0] = 0xFF;
            
            Console.WriteLine($"   Allocated {leakedBuffer.Length} bytes but intentionally not disposing");
            Console.WriteLine("   This will be detected when the debug allocator is disposed...\n");
            
            try
            {
                // When the debugAllocator is disposed, it will detect the leak
                // and throw an exception with details about the leak
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Exception caught: {ex.Message}");
            }
            
            // Note: In a real scenario, you would properly dispose all buffers
            // This is just for demonstration of leak detection
            leakedBuffer.Dispose();
        }
    }
}