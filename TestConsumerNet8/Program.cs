using System;
using System.Text;
using ZiggyAlloc;

Console.WriteLine("=== Testing ZiggyAlloc on .NET 8.0 ===");

using var allocator = new DebugAllocator("Net8Test", Z.DefaultAllocator);
var ctx = new Ctx(allocator, Z.ctx.@out, Z.ctx.@in);

using var defer = DeferScope.Start();

var message = ctx.FormatToSlice(defer, "ZiggyAlloc works on .NET {0}!", "8.0");
Console.WriteLine(Encoding.UTF8.GetString(message));

var numbers = ctx.AllocSlice<int>(defer, 3);
numbers[0] = 8;
numbers[1] = 0; 
numbers[2] = 0;

Console.WriteLine($"Version test: {numbers[0]}.{numbers[1]}.{numbers[2]}");
Console.WriteLine("✓ .NET 8.0 compatibility confirmed!");