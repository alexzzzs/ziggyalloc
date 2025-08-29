using System;
using ZiggyAlloc.Examples.RealWorld;

namespace ZiggyAlloc.Examples.RealWorld
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ZiggyAlloc Real-World Examples");
            Console.WriteLine("=============================\n");
            
            ImageProcessingExample.Run();
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}