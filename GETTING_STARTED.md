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
using ZiggyAlloc;

// Simple example showing core concepts
var allocator = new SystemMemoryAllocator();

// Allocate a buffer for integers
using var numbers = allocator.Allocate<int>(5, zeroMemory: true);

// Fill with square numbers
for (int i = 0; i < numbers.Length; i++)
    numbers[i] = i * i;

// Use implicit conversion to ReadOnlySpan
PrintNumbers(numbers);

Console.WriteLine($"Buffer info:");
Console.WriteLine($"  Length: {numbers.Length} elements");
Console.WriteLine($"  Size: {numbers.SizeInBytes} bytes");
Console.WriteLine($"  Pointer: 0x{numbers.RawPointer:X}");

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
Numbers: 0 1 4 9 16
Buffer info:
  Length: 5 elements
  Size: 20 bytes
  Pointer: 0x1A2B3C4D5E6F
```

## 🧠 Core Concepts

### Memory Allocators

All memory allocation in ZiggyAlloc goes through implementations of `IUnmanagedMemoryAllocator`:

```csharp
public interface IUnmanagedMemoryAllocator
{
    UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false) where T : unmanaged;
    void Free(IntPtr pointer);
    bool SupportsIndividualDeallocation { get; }
    long TotalAllocatedBytes { get; }
}
```

### Available Allocators

1. **SystemMemoryAllocator** - Direct system memory allocation
2. **ScopedMemoryAllocator** - Arena-style allocator that frees all memory when disposed
3. **DebugMemoryAllocator** - Tracks allocations and detects memory leaks with caller information
4. **UnmanagedMemoryPool** - Reduces allocation overhead by reusing previously allocated buffers
5. **HybridAllocator** - Automatically chooses between managed and unmanaged allocation based on size and type
6. **SlabAllocator** - Pre-allocates large blocks and sub-allocates for high-frequency small allocations
7. **LargeBlockAllocator** - Specialized allocator for large memory blocks (>64KB) with pooling and alignment

### UnmanagedBuffer<T>

The core type for working with unmanaged memory:

```csharp
using var buffer = allocator.Allocate<int>(100);

// Array-like access with bounds checking
buffer[0] = 42;
int value = buffer[99];

// Convert to Span<T> for high-performance operations
Span<int> span = buffer;
span.Fill(123);

// Access raw pointer for native interop
IntPtr ptr = buffer.RawPointer;
```

### Automatic Memory Management

Use `using` statements for deterministic cleanup:

```csharp
using var buffer1 = allocator.Allocate<byte>(1024);
using var buffer2 = allocator.Allocate<double>(500);

// Both buffers automatically freed when they go out of scope
// No manual Free() calls needed
```

### Memory Safety Features

Built-in safety without performance cost:

```csharp
using var buffer = allocator.Allocate<int>(10);

// Bounds checking prevents buffer overruns
try 
{
    buffer[15] = 42; // Throws IndexOutOfRangeException
}
catch (IndexOutOfRangeException)
{
    Console.WriteLine("Index out of bounds caught!");
}

// Safe span conversion for high-performance operations
Span<int> span = buffer; // No copying, direct memory access
```

## Common Patterns

### Working with Native APIs
```csharp
var allocator = new SystemMemoryAllocator();
using var points = allocator.Allocate<Point3D>(1000);

// Fill with data
for (int i = 0; i < points.Length; i++)
{
    points[i] = new Point3D { X = i, Y = i * 2, Z = i * 3 };
}

// Pass raw pointer to native function
NativeApi.ProcessPoints(points.RawPointer, points.Length);
```

### High-Performance Buffers
```csharp
var allocator = new SystemMemoryAllocator();
using var buffer = allocator.Allocate<byte>(1024 * 1024, zeroMemory: true);

// Zero-cost conversion to Span<T>
Span<byte> span = buffer;

// Fast operations using span
span.Fill(0xFF);

// Check memory usage
Console.WriteLine($"Allocated: {allocator.TotalAllocatedBytes} bytes");
```

### Memory Leak Detection
```csharp
using var debug = new DebugMemoryAllocator("Test", Z.DefaultAllocator, 
    MemoryLeakReportingMode.Throw);

using var buffer1 = debug.Allocate<int>(10); // Properly disposed

var buffer2 = debug.Allocate<int>(5);
// Forgot to dispose buffer2 - will throw exception with file/line info when debug allocator is disposed
```

### Scoped Memory Management
```csharp
using var scopedAllocator = new ScopedMemoryAllocator();

// Multiple allocations that will all be freed together
using var buffer1 = scopedAllocator.Allocate<int>(100);
using var buffer2 = scopedAllocator.Allocate<double>(200);
using var buffer3 = scopedAllocator.Allocate<byte>(1000);

// All memory freed when scopedAllocator is disposed
```

## Best Practices

1. **Use appropriate allocators**: 
   - `SystemMemoryAllocator` for general use
   - `ScopedMemoryAllocator` for temporary allocations
   - `DebugMemoryAllocator` during development
2. **Always use `using` statements**: Ensures deterministic cleanup
3. **Leverage Span<T> conversion**: Get high performance without copying
4. **Check for leaks**: Use `DebugMemoryAllocator` during development
5. **Monitor memory usage**: Use `TotalAllocatedBytes` for tracking

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

## 🚀 Advanced Features

### Memory Pooling

Reduce allocation overhead by reusing buffers:

```csharp
var systemAllocator = new SystemMemoryAllocator();
using var pool = new UnmanagedMemoryPool(systemAllocator);

// First allocation - creates new buffer
using var buffer1 = pool.Allocate<int>(100);

// Second allocation - reuses buffer from pool if available
using var buffer2 = pool.Allocate<int>(100);

// Buffers are returned to the pool when disposed
```

### Slab Allocation

Optimize high-frequency small allocations:

```csharp
var systemAllocator = new SystemMemoryAllocator();
using var slabAllocator = new SlabAllocator(systemAllocator);

// Small allocations are extremely fast
using var smallBuffer = slabAllocator.Allocate<int>(100);

// Large allocations are delegated to the base allocator
using var largeBuffer = slabAllocator.Allocate<int>(10000);
```

### Hybrid Allocation

Intelligent allocation strategy selection:

```csharp
var systemAllocator = new SystemMemoryAllocator();
using var hybridAllocator = new HybridAllocator(systemAllocator);

// Small allocations may use managed arrays for better performance
using var smallBuffer = hybridAllocator.Allocate<int>(100);

// Large allocations will use unmanaged memory to avoid GC pressure
using var largeBuffer = hybridAllocator.Allocate<int>(10000);
```
