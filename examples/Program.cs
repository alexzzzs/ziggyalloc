using System;

namespace ZiggyAlloc.Examples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ZiggyAlloc Organized Examples");
            Console.WriteLine("============================\n");
            
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }
            
            switch (args[0].ToLower())
            {
                case "basic":
                    RunBasicExamples();
                    break;
                case "allocators":
                    RunAllocatorExamples();
                    break;
                case "performance":
                    RunPerformanceExamples();
                    break;
                case "realworld":
                    RunRealWorldExamples();
                    break;
                default:
                    ShowHelp();
                    break;
            }
        }
        
        static void ShowHelp()
        {
            Console.WriteLine("Usage: dotnet run -- [example-type]");
            Console.WriteLine();
            Console.WriteLine("Available example types:");
            Console.WriteLine("  basic       - Basic usage examples");
            Console.WriteLine("  allocators  - Allocator-specific examples");
            Console.WriteLine("  performance - Performance optimization examples");
            Console.WriteLine("  realworld   - Real-world application examples");
            Console.WriteLine();
            Console.WriteLine("Example: dotnet run -- basic");
        }
        
        static void RunBasicExamples()
        {
            Console.WriteLine("Running Basic Examples...");
            Console.WriteLine();
            Examples.Basic.SimpleAllocation.Run();
        }
        
        static void RunAllocatorExamples()
        {
            Console.WriteLine("Running Allocator Examples...");
            Console.WriteLine();
            Examples.Allocators.HybridAllocatorExample.Run();
        }
        
        static void RunPerformanceExamples()
        {
            Console.WriteLine("Running Performance Examples...");
            Console.WriteLine();
            Examples.Performance.UnmanagedMemoryPoolExample.Run();
        }
        
        static void RunRealWorldExamples()
        {
            Console.WriteLine("Running Real-World Examples...");
            Console.WriteLine();
            Examples.RealWorld.ImageProcessingExample.Run();
        }
    }
}