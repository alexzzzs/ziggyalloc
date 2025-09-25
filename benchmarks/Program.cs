using System;
using System.Reflection;
using BenchmarkDotNet.Running;
using ZiggyAlloc.Benchmarks;

namespace ZiggyAlloc.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ZiggyAlloc Benchmarks");
            Console.WriteLine("====================");

            // Use BenchmarkSwitcher to allow filtering
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                var switcher = new BenchmarkSwitcher(assembly);
                switcher.Run(args);
            }

            Console.WriteLine("Benchmarks completed!");
        }
    }
}