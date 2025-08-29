using System;
using ZiggyAlloc;

namespace ZiggyAlloc.Examples.Advanced
{
    /// <summary>
    /// Advanced example demonstrating various defer patterns and error handling.
    /// </summary>
    public class DeferPatterns
    {
        public static void Run()
        {
            Console.WriteLine("=== Advanced Defer Patterns Example ===\n");

            Console.WriteLine("1. Nested defer scopes:");
            
            var allocator = new SystemMemoryAllocator();
            
            // Outer scope
            using (var outerDefer = DeferScope.Start())
            {
                var outerResource = allocator.AllocateDeferred<int>(outerDefer, 10);
                outerDefer.Defer(() => Console.WriteLine("   ✓ Outer scope cleanup"));
                
                outerResource[0] = 100;
                Console.WriteLine($"   Outer resource value: {outerResource[0]}");
                
                // Inner scope
                using (var innerDefer = DeferScope.Start())
                {
                    var innerResource = allocator.AllocateDeferred<float>(innerDefer, 5);
                    innerDefer.Defer(() => Console.WriteLine("   ✓ Inner scope cleanup"));
                    
                    innerResource[0] = 2.5f;
                    Console.WriteLine($"   Inner resource value: {innerResource[0]}");
                    
                    // Add more cleanup actions
                    innerDefer.Defer(() => Console.WriteLine("   ✓ Additional inner cleanup"));
                }
                // Inner scope cleanup happens here
                
                outerDefer.Defer(() => Console.WriteLine("   ✓ Additional outer cleanup"));
            }
            // Outer scope cleanup happens here
            Console.WriteLine();

            Console.WriteLine("2. Error handling with defer:");
            
            try
            {
                using (var defer = DeferScope.Start())
                {
                    var resource1 = allocator.AllocateDeferred<byte>(defer, 100);
                    var resource2 = allocator.AllocateDeferred<int>(defer, 50);
                    
                    // Simulate some work
                    resource1[0] = 0xFF;
                    resource2[0] = 42;
                    
                    defer.Defer(() => Console.WriteLine("   ✓ Normal cleanup operation"));
                    
                    // Simulate an error condition
                    throw new InvalidOperationException("Simulated error during processing");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Caught exception: {ex.Message}");
                Console.WriteLine("   Note: All defer actions were still executed despite the exception");
            }
            Console.WriteLine();

            Console.WriteLine("3. Complex resource management:");
            
            using (var defer = DeferScope.Start())
            {
                // Allocate multiple resources
                var buffer1 = allocator.AllocateDeferred<Point3D>(defer, 1000);
                var buffer2 = allocator.AllocateDeferred<Vector3>(defer, 500);
                var buffer3 = allocator.AllocateDeferred<float>(defer, 10000);
                
                // Initialize data
                InitializePointData(buffer1);
                InitializeVectorData(buffer2);
                InitializeFloatData(buffer3);
                
                Console.WriteLine($"   Allocated and initialized:");
                Console.WriteLine($"   - {buffer1.Length} Point3D structures");
                Console.WriteLine($"   - {buffer2.Length} Vector3 structures");
                Console.WriteLine($"   - {buffer3.Length} float values");
                
                // Add custom cleanup for external resources
                defer.Defer(() => Console.WriteLine("   ✓ External resource cleanup"));
                defer.Defer(() => Console.WriteLine("   ✓ Logging cleanup"));
                defer.Defer(() => Console.WriteLine("   ✓ Metrics collection cleanup"));
            }
            
            Console.WriteLine("\nAll resources properly cleaned up in reverse order.");
        }
        
        static void InitializePointData(UnmanagedBuffer<Point3D> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = new Point3D
                {
                    X = i * 0.1f,
                    Y = i * 0.2f,
                    Z = i * 0.3f
                };
            }
        }
        
        static void InitializeVectorData(UnmanagedBuffer<Vector3> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = new Vector3
                {
                    X = i * 0.01f,
                    Y = i * 0.02f,
                    Z = i * 0.03f
                };
            }
        }
        
        static void InitializeFloatData(UnmanagedBuffer<float> buffer)
        {
            Span<float> span = buffer;
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = (float)Math.Sin(i * 0.01);
            }
        }
    }
    
    public struct Point3D
    {
        public float X;
        public float Y;
        public float Z;
    }
    
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;
    }
}