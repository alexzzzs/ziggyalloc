using System;
using System.Linq;

namespace ZiggyAlloc.Examples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ZiggyAlloc Examples");
            Console.WriteLine("==================\n");
            
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }
            
            var section = args[0].ToLower();
            
            try
            {
                switch (section)
                {
                    case "basic":
                        RunBasicExamples();
                        break;
                    case "advanced":
                        RunAdvancedExamples();
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
                        Console.WriteLine($"Unknown section: {section}");
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running examples: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
        
        static void ShowHelp()
        {
            Console.WriteLine("Usage: dotnet run -- [section]");
            Console.WriteLine();
            Console.WriteLine("Available sections:");
            Console.WriteLine("  basic       - Basic usage examples");
            Console.WriteLine("  advanced    - Advanced features and patterns");
            Console.WriteLine("  allocators  - Allocator-specific examples");
            Console.WriteLine("  performance - Performance optimization examples");
            Console.WriteLine("  realworld   - Real-world application examples");
            Console.WriteLine();
            Console.WriteLine("Example: dotnet run -- basic");
        }
        
        static void RunBasicExamples()
        {
            Console.WriteLine("Running Basic Examples...");
            Examples.Basic.SimpleAllocation.Run();
        }
        
        static void RunAdvancedExamples()
        {
            Console.WriteLine("Running Advanced Examples...");
            Examples.Advanced.MemoryLeakDetection.Run();
            Console.WriteLine();
            Examples.Advanced.DeferPatterns.Run();
        }
        
        static void RunAllocatorExamples()
        {
            Console.WriteLine("Running Allocator Examples...");
            Examples.Allocators.HybridAllocatorExample.Run();
        }
        
        static void RunPerformanceExamples()
        {
            Console.WriteLine("Running Performance Examples...");
            Examples.Performance.UnmanagedMemoryPoolExample.Run();
        }
        
        static void RunRealWorldExamples()
        {
            Console.WriteLine("Running Real-World Examples...");
            Examples.RealWorld.ImageProcessingExample.Run();
        }
    }
}