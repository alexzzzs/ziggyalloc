// Save this as Program.cs in your test project after installing ZiggyAlloc
using System;
using System.Text;
using ZiggyAlloc;

Console.WriteLine("Testing ZiggyAlloc from NuGet.org!");

using var allocator = new DebugAllocator("PublicTest", Z.DefaultAllocator);
var ctx = new Ctx(allocator, Z.ctx.@out, Z.ctx.@in);

using var defer = DeferScope.Start();

var message = ctx.FormatToSlice(defer, "ZiggyAlloc v{0} installed from NuGet.org!", "1.0.0");
Console.WriteLine(Encoding.UTF8.GetString(message));

var numbers = ctx.AllocSlice<int>(defer, 3);
numbers[0] = 1;
numbers[1] = 2; 
numbers[2] = 3;

Console.WriteLine($"Test array: [{string.Join(", ", numbers.AsSpan().ToArray())}]");
Console.WriteLine("✅ ZiggyAlloc is working from public NuGet!");