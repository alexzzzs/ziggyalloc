using System;
using ZiggyAlloc.Examples.Basic;

namespace ZiggyAlloc.Examples.Basic
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ZiggyAlloc Basic Examples");
            Console.WriteLine("========================\n");
            
            SimpleAllocation.Run();
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}