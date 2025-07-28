using System;
using System.Text;
using ZiggyAlloc;

namespace ZiggyAlloc.Examples
{
    public static class BasicUsage
    {
        public static void Main()
        {
            Console.WriteLine("=== ZiggyAlloc Basic Usage Example ===\n");
            
            RunBasicExample();
            
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            AdvancedUsage.RunAdvancedExamples();
        }

        static void RunBasicExample()
        {
            // Use a DebugAllocator to automatically catch memory leaks.
            using var debugAllocator = new DebugAllocator("Main", Z.DefaultAllocator);
            var ctx = new Ctx(debugAllocator, Z.ctx.@out, Z.ctx.@in);

            using var defer = DeferScope.Start();

            // 1. Unmanaged, deferred formatting (like std.fmt.allocPrint)
            var message = ctx.FormatToSlice(defer, "Hello, {0}!", "Ziggy");
            ctx.PrintLine(Encoding.UTF8.GetString(message)); // Use the slice

            // 2. RAII-style allocation via `using` for automatic cleanup
            using (var number = ctx.Auto<int>())
            {
                number.Value = 123;
                ctx.PrintLine(number.Value);
            } // number is freed here

            // 3. Deferred slice allocation
            var slice = ctx.AllocSlice<int>(defer, 5, zeroed: true);
            for (int i = 0; i < slice.Length; i++) slice[i] = i;

            // 4. Implicit conversion from Slice<T> to ReadOnlySpan<T>
            PrintSpan(slice);

            ctx.PrintLine("Done. Deferred/using actions have run. Checking for leaks...");
        } // `debugAllocator.Dispose()` is called here, reporting any leaks.

        static void PrintSpan(ReadOnlySpan<int> span)
        {
            foreach (var item in span) Console.Write(item + " ");
            Console.WriteLine();
        }
    }
}