using System;
using ZiggyAlloc.Examples.Performance;

namespace ZiggyAlloc.Examples.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ZiggyAlloc Performance Examples");
            Console.WriteLine("===============================\n");
            
            UnmanagedMemoryPoolExample.Run();
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}