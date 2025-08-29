using System;
using ZiggyAlloc.Examples.Advanced;

namespace ZiggyAlloc.Examples.Advanced
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ZiggyAlloc Advanced Examples");
            Console.WriteLine("==========================\n");
            
            Console.WriteLine("Running Memory Leak Detection Example:");
            MemoryLeakDetection.Run();
            
            Console.WriteLine("\n" + new string('-', 50) + "\n");
            
            Console.WriteLine("Running Defer Patterns Example:");
            DeferPatterns.Run();
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}