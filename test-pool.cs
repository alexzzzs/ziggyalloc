using System;
using ZiggyAlloc;

class Program
{
    static void Main()
    {
        Console.WriteLine("Testing UnmanagedMemoryPool...");
        
        var baseAllocator = new SystemMemoryAllocator();
        using var pool = new UnmanagedMemoryPool(baseAllocator);
        
        // First allocation (should create new buffer)
        Console.WriteLine("Allocating first buffer...");
        using var buffer1 = pool.Allocate<int>(100);
        Console.WriteLine($"Buffer1: IsValid={buffer1.IsValid}, Length={buffer1.Length}");
        
        // Fill buffer with data
        for (int i = 0; i < buffer1.Length; i++)
        {
            buffer1[i] = i;
        }
        
        Console.WriteLine("Disposing first buffer...");
        // Dispose first buffer (should return to pool)
        buffer1.Dispose();
        
        Console.WriteLine("Allocating second buffer...");
        // Second allocation of same size (should reuse from pool)
        using var buffer2 = pool.Allocate<int>(100);
        Console.WriteLine($"Buffer2: IsValid={buffer2.IsValid}, Length={buffer2.Length}");
        
        Console.WriteLine("Test completed successfully!");
    }
}