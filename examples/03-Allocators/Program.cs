using System;
using ZiggyAlloc.Examples.Allocators;

namespace ZiggyAlloc.Examples.Allocators
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ZiggyAlloc Allocator Examples");
            Console.WriteLine("============================\n");
            
            HybridAllocatorExample.Run();
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}