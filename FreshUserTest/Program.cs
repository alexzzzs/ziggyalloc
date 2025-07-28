using ZiggyAlloc;

Console.WriteLine("Hello from a fresh user trying ZiggyAlloc!");

using var allocator = new ScopedAllocator();
var ctx = new Ctx(allocator, Z.ctx.@out, Z.ctx.@in);

var data = ctx.AllocSlice<int>(10, zeroed: true);
for (int i = 0; i < data.Length; i++)
    data[i] = i + 1;

Console.WriteLine($"Created array: [{string.Join(", ", data.AsSpan().ToArray())}]");
Console.WriteLine("ZiggyAlloc works perfectly! 🎉");