# Getting Started with ZiggyAlloc

## Installation

### Option 1: NuGet Package (Recommended)

Install via .NET CLI:
```bash
dotnet add package ZiggyAlloc
```

Or via Package Manager Console in Visual Studio:
```powershell
Install-Package ZiggyAlloc
```

### Option 2: Local Build

Clone and build from source:
```bash
git clone https://github.com/ziggyalloc/ziggyalloc.git
cd ziggyalloc
dotnet build --configuration Release
dotnet pack --configuration Release --output ./nupkg
```

## Your First ZiggyAlloc Program

Create a new console application:
```bash
dotnet new console -n MyZiggyApp
cd MyZiggyApp
dotnet add package ZiggyAlloc
```

Replace the contents of `Program.cs`:

```csharp
using System;
using System.Text;
using ZiggyAlloc;

// Simple example showing core concepts
using var allocator = new DebugAllocator("MyApp", Z.DefaultAllocator);
var ctx = new Ctx(allocator, Z.ctx.@out, Z.ctx.@in);

using var defer = DeferScope.Start();

// Allocate and format a message
var message = ctx.FormatToSlice(defer, "Welcome to {0}!", "ZiggyAlloc");
Console.WriteLine(Encoding.UTF8.GetString(message));

// Create a numeric array
var numbers = ctx.AllocSlice<int>(defer, 5, zeroed: true);
for (int i = 0; i < numbers.Length; i++)
    numbers[i] = i * i;

// Use implicit conversion to ReadOnlySpan
PrintNumbers(numbers);

static void PrintNumbers(ReadOnlySpan<int> nums)
{
    Console.Write("Numbers: ");
    foreach (var n in nums) Console.Write($"{n} ");
    Console.WriteLine();
}
```

Run it:
```bash
dotnet run
```

Expected output:
```
Welcome to ZiggyAlloc!
Numbers: 0 1 4 9 16
```

## Key Concepts

### 1. Allocators
Choose the right allocator for your needs:

```csharp
// Manual control - you manage memory
var manual = new ManualAllocator();

// Automatic cleanup on scope exit
using var scoped = new ScopedAllocator();

// Debug mode with leak detection
using var debug = new DebugAllocator("Component", Z.DefaultAllocator);
```

### 2. Context Pattern
Pass allocators through your application:

```csharp
void ProcessData(Ctx ctx)
{
    var buffer = ctx.AllocSlice<byte>(1024);
    // Use buffer...
    // Memory management handled by caller's allocator choice
}
```

### 3. Defer Scopes
Automatic cleanup with deterministic ordering:

```csharp
using var defer = DeferScope.Start();

var ptr1 = ctx.Alloc<int>(defer);
var ptr2 = ctx.Alloc<double>(defer);

defer.Defer(() => Console.WriteLine("Custom cleanup"));
// Cleanup order: custom action, ptr2, ptr1
```

### 4. RAII Pattern
Automatic resource management:

```csharp
using (var auto = ctx.Auto<MyStruct>())
{
    auto.Value = new MyStruct { Field = 42 };
    // Use auto.Value...
} // Automatically freed here
```

## Common Patterns

### Working with Native APIs
```csharp
// Prepare data for native interop
var points = ctx.AllocSlice<Point3D>(defer, 1000);
// Fill points...

// Pass to native function
NativeApi.ProcessPoints(points.Ptr.Raw, points.Length);
```

### High-Performance Buffers
```csharp
// Large buffer with zero-cost span access
var buffer = ctx.AllocSlice<byte>(1024 * 1024, zeroed: true);
Span<byte> span = buffer; // Zero-cost conversion

// Fast operations using span
span.Fill(0xFF);
```

### Memory Leak Detection
```csharp
using var debug = new DebugAllocator("Test", Z.DefaultAllocator, LeakReportingMode.Throw);

var ptr = debug.Alloc<int>();
// Forgot to free - will throw exception with file/line info
```

## Best Practices

1. **Use appropriate allocators**: Debug for development, Manual/Scoped for production
2. **Prefer defer scopes**: Automatic cleanup with clear ordering
3. **Leverage context pattern**: Pass allocators through your API
4. **Use RAII when possible**: `using` statements for automatic cleanup
5. **Check for leaks**: Always use DebugAllocator during development

## Next Steps

- Check out the [examples/](examples/) directory for more complex scenarios
- Read the [API documentation](README.md#api-reference) for detailed reference
- Explore the [test suite](tests/) for usage patterns
- Join the community discussions for questions and contributions

## Troubleshooting

### Common Issues

**"Cannot use ref local inside lambda"**
- Solution: Copy the value before using in defer/lambda
```csharp
var ptr = alloc.Alloc<int>();
var ptrCopy = ptr.Raw;
defer.Defer(() => alloc.Free(ptrCopy));
```

**"Unsafe code not allowed"**
- Solution: Enable unsafe code in your project file
```xml
<PropertyGroup>
  <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
</PropertyGroup>
```

**Memory access violations**
- Solution: Ensure you don't use freed memory or access out of bounds
- Use DebugAllocator to catch issues early

Need help? Check the [GitHub issues](https://github.com/ziggyalloc/ziggyalloc/issues) or create a new one!