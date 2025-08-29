using System;
using ZiggyAlloc;

namespace ZiggyAlloc.Examples.Allocators
{
    /// <summary>
    /// Example demonstrating the HybridAllocator which automatically chooses between 
    /// managed and unmanaged allocation based on size and type.
    /// </summary>
    public class HybridAllocatorExample
    {
        public static void Run()
        {
            Console.WriteLine("=== HybridAllocator Example ===\n");

            // Create a hybrid allocator that uses a system allocator for unmanaged allocations
            var systemAllocator = new SystemMemoryAllocator();
            var hybridAllocator = new HybridAllocator(systemAllocator);

            // Small allocation - will likely use managed arrays for better performance
            Console.WriteLine("1. Small allocation (100 integers):");
            using var smallBuffer = hybridAllocator.Allocate<int>(100);
            FillBuffer(smallBuffer, 1);
            Console.WriteLine($"   Length: {smallBuffer.Length}");
            Console.WriteLine($"   First few values: {smallBuffer[0]}, {smallBuffer[1]}, {smallBuffer[2]}");
            Console.WriteLine($"   Memory strategy: Managed arrays (faster for small allocations)\n");

            // Medium allocation - decision based on type and size
            Console.WriteLine("2. Medium allocation (1000 doubles):");
            using var mediumBuffer = hybridAllocator.Allocate<double>(1000);
            FillBuffer(mediumBuffer, 1.5);
            Console.WriteLine($"   Length: {mediumBuffer.Length}");
            Console.WriteLine($"   First few values: {mediumBuffer[0]:F2}, {mediumBuffer[1]:F2}, {mediumBuffer[2]:F2}");
            Console.WriteLine($"   Memory strategy: Depends on thresholds\n");

            // Large allocation - will use unmanaged memory to avoid GC pressure
            Console.WriteLine("3. Large allocation (100,000 integers):");
            using var largeBuffer = hybridAllocator.Allocate<int>(100000);
            FillBuffer(largeBuffer, 42);
            Console.WriteLine($"   Length: {largeBuffer.Length:N0}");
            Console.WriteLine($"   Size: {largeBuffer.SizeInBytes / 1024:N0} KB");
            Console.WriteLine($"   Memory strategy: Unmanaged memory (avoids GC pressure)\n");

            // Byte array example
            Console.WriteLine("4. Byte array allocation (500 elements):");
            using var byteBuffer = hybridAllocator.Allocate<byte>(500);
            FillBuffer(byteBuffer, (byte)255);
            Console.WriteLine($"   Length: {byteBuffer.Length}");
            Console.WriteLine($"   First few values: {byteBuffer[0]}, {byteBuffer[1]}, {byteBuffer[2]}");
            Console.WriteLine($"   Memory strategy: Managed arrays (≤1024 bytes)\n");

            // Struct array example
            Console.WriteLine("5. Struct array allocation (200 elements):");
            using var structBuffer = hybridAllocator.Allocate<Point3D>(200);
            FillBuffer(structBuffer);
            Console.WriteLine($"   Length: {structBuffer.Length}");
            Console.WriteLine($"   First point: ({structBuffer[0].X:F1}, {structBuffer[0].Y:F1}, {structBuffer[0].Z:F1})");
            Console.WriteLine($"   Memory strategy: Depends on struct size and count\n");

            Console.WriteLine($"Total allocated bytes: {hybridAllocator.TotalAllocatedBytes:N0}");
        }

        static void FillBuffer<T>(UnmanagedBuffer<T> buffer, T value) where T : unmanaged
        {
            // Use Span for efficient filling
            Span<T> span = buffer;
            span.Fill(value);
        }

        static void FillBuffer(UnmanagedBuffer<Point3D> buffer)
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
    }

    /// <summary>
    /// Simple 3D point structure for demonstration.
    /// </summary>
    public struct Point3D
    {
        public float X;
        public float Y;
        public float Z;
    }
}