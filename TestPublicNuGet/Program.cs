using System;
using System.Text;
using ZiggyAlloc;

Console.WriteLine("🎉 Testing ZiggyAlloc from Public NuGet.org! 🎉");

using var allocator = new DebugAllocator("PublicNuGetTest", Z.DefaultAllocator);
var ctx = new Ctx(allocator, Z.ctx.@out, Z.ctx.@in);

using var defer = DeferScope.Start();

var message = ctx.FormatToSlice(defer, "Successfully installed ZiggyAlloc v{0} from NuGet.org!", "1.0.0");
Console.WriteLine(Encoding.UTF8.GetString(message));

var numbers = ctx.AllocSlice<int>(defer, 5);
for (int i = 0; i < numbers.Length; i++)
    numbers[i] = i * i;

Console.WriteLine($"Test array: [{string.Join(", ", numbers.AsSpan().ToArray())}]");

using (var auto = ctx.Auto<double>())
{
    auto.Value = 3.14159;
    Console.WriteLine($"RAII test: π ≈ {auto.Value:F5}");
}

Console.WriteLine("\n✅ ZiggyAlloc is working perfectly from public NuGet!");
Console.WriteLine("🚀 Anyone can now install it with: dotnet add package ZiggyAlloc");