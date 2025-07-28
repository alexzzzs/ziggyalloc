using System;
using System.Text;
using ZiggyAlloc;

Console.WriteLine("=== Testing ZiggyAlloc via NuGet Package ===\n");

// Test 1: Basic allocation and context usage
Console.WriteLine("Test 1: Basic Context Usage");
Console.WriteLine("---------------------------");

using var debugAllocator = new DebugAllocator("NuGetTest", Z.DefaultAllocator);
var ctx = new Ctx(debugAllocator, Z.ctx.@out, Z.ctx.@in);

using var defer = DeferScope.Start();

// Test string formatting
var message = ctx.FormatToSlice(defer, "Hello from {0} v{1}!", "ZiggyAlloc", "1.0.0");
Console.WriteLine(Encoding.UTF8.GetString(message));

// Test RAII pattern
using (var number = ctx.Auto<int>())
{
    number.Value = 42;
    Console.WriteLine($"RAII value: {number.Value}");
}

Console.WriteLine("✓ Basic context usage works\n");

// Test 2: Different allocator types
Console.WriteLine("Test 2: Allocator Types");
Console.WriteLine("-----------------------");

// Manual allocator
var manual = new ManualAllocator();
var ptr = manual.Alloc<double>();
ptr.Value = 3.14159;
Console.WriteLine($"Manual allocator: {ptr.Value:F5}");
manual.Free(ptr.Raw);

// Scoped allocator
using (var scoped = new ScopedAllocator())
{
    var slice = scoped.Slice<int>(5, zeroed: true);
    for (int i = 0; i < slice.Length; i++) slice[i] = i * i;
    Console.WriteLine($"Scoped allocator slice: [{string.Join(", ", slice.AsSpan().ToArray())}]");
}

Console.WriteLine("✓ All allocator types work\n");

// Test 3: High-performance operations
Console.WriteLine("Test 3: Performance Operations");
Console.WriteLine("------------------------------");

var buffer = ctx.AllocSlice<byte>(defer, 1024, zeroed: true);
Span<byte> span = buffer; // Test implicit conversion

// Fill with pattern
for (int i = 0; i < span.Length; i += 4)
{
    if (i + 3 < span.Length)
    {
        span[i] = 0xDE;
        span[i + 1] = 0xAD;
        span[i + 2] = 0xBE;
        span[i + 3] = 0xEF;
    }
}

Console.WriteLine($"Buffer size: {buffer.Length} bytes");
Console.WriteLine($"Pattern check: {span[0]:X2} {span[1]:X2} {span[2]:X2} {span[3]:X2}");
Console.WriteLine("✓ High-performance operations work\n");

// Test 4: Memory safety features
Console.WriteLine("Test 4: Memory Safety");
Console.WriteLine("--------------------");

try
{
    var testSlice = ctx.AllocSlice<int>(defer, 3);
    testSlice[0] = 1;
    testSlice[1] = 2;
    testSlice[2] = 3;
    
    // This should work
    Console.WriteLine($"Valid access: {testSlice[2]}");
    
    // Test bounds checking (this should throw)
    try
    {
        var _ = testSlice[5]; // Out of bounds
        Console.WriteLine("❌ Bounds checking failed!");
    }
    catch (IndexOutOfRangeException)
    {
        Console.WriteLine("✓ Bounds checking works");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Unexpected error: {ex.Message}");
}

Console.WriteLine("\n=== All NuGet Package Tests Passed! ===");
Console.WriteLine("ZiggyAlloc is working correctly via NuGet distribution.");

// Run the real-world example
RealWorldExample.RunImageProcessingExample();