# ZiggyAlloc

[![NuGet](https://img.shields.io/nuget/v/ZiggyAlloc.svg)](https://www.nuget.org/packages/ZiggyAlloc/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**High-performance unmanaged memory management for .NET**

ZiggyAlloc provides explicit control over unmanaged memory allocation, enabling high-performance scenarios where garbage collection overhead must be avoided.

## Why ZiggyAlloc?

### 🚀 **Performance-Critical Scenarios**
- **Large buffer management** without GC pressure
- **Native API interop** with contiguous memory requirements  
- **Scientific computing** with massive datasets
- **Game engines** and real-time applications
- **Image/audio processing** with custom memory layouts

### 💡 **Key Advantages Over Standard .NET**

| Scenario | Standard .NET | ZiggyAlloc |
|----------|---------------|------------|
| 100MB buffer | `new byte[100MB]` → GC pressure | `allocator.Allocate<byte>(100MB)` → No GC |
| Native interop | Complex marshaling | Direct pointer access |
| Custom layouts | Limited options | Full control over memory layout |
| Memory tracking | GC.GetTotalMemory() | Precise allocation tracking |

## Installation

```bash
dotnet add package ZiggyAlloc
```

## Quick Start

```csharp
using ZiggyAlloc;

// Allocate large buffer without GC pressure
var allocator = new SystemMemoryAllocator();
using var buffer = allocator.Allocate<float>(1_000_000); // 4MB, no GC impact

// Use as high-performance Span<T>
Span<float> data = buffer;
data.Fill(3.14159f);

// Direct native API access
ProcessDataNative(buffer.RawPointer, buffer.Length);
```

## Real-World Examples

### 🔧 **Native API Interop**
```csharp
// Allocate memory for native API
using var points = allocator.Allocate<Point3D>(10000);

// Fill with data
for (int i = 0; i < points.Length; i++)
    points[i] = new Point3D(i, i * 2, i * 3);

// Pass directly to native function
NativeLibrary.ProcessPoints(points.RawPointer, points.Length);
```

### 🖼️ **Image Processing**
```csharp
// 4K image buffer (33MB) - no GC pressure
const int width = 3840, height = 2160;
using var imageBuffer = allocator.Allocate<Rgba32>(width * height);

// High-performance processing using Span<T>
Span<Rgba32> pixels = imageBuffer;
ApplyGaussianBlur(pixels, width, height);
```

### 🧪 **Scientific Computing**
```csharp
// Struct-of-Arrays for better cache performance
using var positions = allocator.Allocate<Vector3>(1_000_000);
using var velocities = allocator.Allocate<Vector3>(1_000_000);
using var masses = allocator.Allocate<float>(1_000_000);

// Physics simulation with no GC allocations
UpdateParticlePhysics(positions, velocities, masses, deltaTime);
```
```

## Core Concepts

### Allocators

ZiggyAlloc provides three main allocator types:

#### ManualAllocator
Basic malloc/free style allocation with manual memory management:

```csharp
var allocator = new ManualAllocator();
var ptr = allocator.Alloc<int>();
ptr.Value = 42;
allocator.Free(ptr.Raw); // Manual cleanup required
```

#### ScopedAllocator  
Automatically frees all allocations when disposed:

```csharp
using var allocator = new ScopedAllocator();
var ptr1 = allocator.Alloc<int>();
var ptr2 = allocator.Alloc<double>(10);
// All allocations freed automatically on dispose
```

#### DebugAllocator
Tracks allocations and reports memory leaks with caller information:

```csharp
using var debugAlloc = new DebugAllocator("MyComponent", Z.DefaultAllocator);
var ptr = debugAlloc.Alloc<int>();
// Forgot to free - will report leak with file/line info on dispose
```

### Context System

Pass allocators and I/O through context structures, Zig-style:

```csharp
var ctx = new Ctx(allocator, writer, reader);

// All allocation methods available through context
var data = ctx.AllocSlice<byte>(1024);
ctx.PrintLine("Allocated 1KB");
```

### Defer Scopes

Automatic cleanup with deferred actions:

```csharp
using var defer = DeferScope.Start();

var ptr1 = ctx.Alloc<int>(defer);     // Will be freed automatically
var ptr2 = ctx.Alloc<double>(defer); // Will be freed automatically

defer.Defer(() => Console.WriteLine("Custom cleanup"));
// Cleanup happens in reverse order: custom action, ptr2, ptr1
```

### Memory Safety

- **Bounds checking**: Slice access is bounds-checked
- **Leak detection**: Debug allocator reports unfreed memory
- **Caller information**: Track allocation sites automatically
- **Type safety**: Generic allocations with compile-time type checking

## API Reference

### Core Types

- `Pointer<T>` - Type-safe pointer wrapper
- `Slice<T>` - Bounds-checked array view  
- `AutoFree<T>` - RAII wrapper for automatic cleanup
- `DeferScope` - Manages deferred cleanup actions
- `Ctx` - Context structure for allocator/I/O passing

### Allocator Interface

```csharp
public interface IAllocator
{
    Pointer<T> Alloc<T>(int count = 1, bool zeroed = false) where T : unmanaged;
    void Free(IntPtr ptr);
    Slice<T> Slice<T>(int count, bool zeroed = false) where T : unmanaged;
}
```

## Performance

ZiggyAlloc is designed for minimal overhead:

- **Zero-cost abstractions**: ref structs compile to direct memory access
- **Inlined operations**: Critical paths are aggressively inlined
- **Native memory**: Uses `NativeMemory` APIs on .NET 6+ for optimal performance
- **No GC pressure**: Unmanaged allocations don't affect garbage collection

## Requirements

- .NET 8.0 or later
- Unsafe code support (automatically enabled by the package)

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## Contributing

We welcome contributions from the community! Here's how you can help:

### 🐛 Report Issues
Found a bug? Have a feature request? [Open an issue](https://github.com/ziggyalloc/ziggyalloc/issues/new/choose) on GitHub.

### 🔧 Contribute Code
1. Fork the repository
2. Create a feature branch
3. Make your changes with tests
4. Submit a pull request

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

### 💡 Suggest Improvements
- Performance optimizations
- New allocator types
- API enhancements
- Documentation improvements

### 🧪 Help with Testing
- Test on different platforms
- Performance benchmarking
- Memory usage analysis
- Real-world usage scenarios

## Community

- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: Questions and general discussion
- **Pull Requests**: Code contributions welcome!

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Inspiration

This library is inspired by Zig's approach to memory management and context passing. While C# has garbage collection, there are scenarios where explicit memory control is beneficial:

- High-performance applications
- Interop with native libraries  
- Memory-constrained environments
- Deterministic cleanup requirements

ZiggyAlloc brings Zig's elegant patterns to the .NET ecosystem while maintaining C#'s safety and expressiveness.
## Co
re API

### UnmanagedBuffer<T>
The main type for working with unmanaged memory:

```csharp
using var buffer = allocator.Allocate<int>(1000);

// Array-like access with bounds checking
buffer[0] = 42;
int value = buffer[999];

// High-performance Span<T> conversion
Span<int> span = buffer;
span.Fill(123);

// Properties
Console.WriteLine($"Length: {buffer.Length}");
Console.WriteLine($"Size: {buffer.SizeInBytes} bytes");
Console.WriteLine($"Pointer: 0x{buffer.RawPointer:X}");

// Utility methods
buffer.Clear();           // Zero all bytes
buffer.Fill(42);          // Fill with value
buffer.CopyFrom(source);  // Copy from another buffer/span
```

### SystemMemoryAllocator
High-performance allocator using native system calls:

```csharp
var allocator = new SystemMemoryAllocator();

// Basic allocation
using var buffer = allocator.Allocate<double>(1000, zeroMemory: true);

// Memory tracking
Console.WriteLine($"Total allocated: {allocator.TotalAllocatedBytes} bytes");

// Wrap existing memory (doesn't own it)
var wrapped = SystemMemoryAllocator.WrapExisting<byte>(ptr, length);
var fromSpan = SystemMemoryAllocator.WrapSpan(existingSpan);
```

## Advanced Features

### Memory Tracking & Debugging
```csharp
// Track allocations and detect leaks
var debugAllocator = new DebugMemoryAllocator("MyComponent", baseAllocator);

using (debugAllocator)
{
    var buffer = debugAllocator.Allocate<int>(100);
    // Forgot to dispose - will report leak with file/line info
}
```

### Custom Allocators
Implement `IUnmanagedMemoryAllocator` for specialized allocation strategies:

```csharp
public interface IUnmanagedMemoryAllocator
{
    UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false) where T : unmanaged;
    void Free(IntPtr pointer);
    bool SupportsIndividualDeallocation { get; }
    long TotalAllocatedBytes { get; }
}
```

## Performance Benefits

### Memory Allocation Comparison
```csharp
// ❌ Standard .NET - causes GC pressure
var managedArray = new byte[100_000_000]; // 100MB on managed heap

// ✅ ZiggyAlloc - no GC impact
using var unmanagedBuffer = allocator.Allocate<byte>(100_000_000);
```

### Interop Performance
```csharp
// ❌ Standard marshaling - copying overhead
byte[] managedData = GetData();
IntPtr ptr = Marshal.AllocHGlobal(managedData.Length);
Marshal.Copy(managedData, 0, ptr, managedData.Length);
NativeAPI(ptr, managedData.Length);
Marshal.FreeHGlobal(ptr);

// ✅ ZiggyAlloc - direct access, no copying
using var buffer = allocator.Allocate<byte>(dataSize);
FillBuffer(buffer); // Fill directly in unmanaged memory
NativeAPI(buffer.RawPointer, buffer.Length);
```

## When to Use ZiggyAlloc

### ✅ **Perfect For:**
- Native library interop requiring contiguous memory
- Large buffer allocations (>85KB) to avoid Large Object Heap
- Performance-critical code where GC pauses are unacceptable
- Custom memory layout patterns (struct-of-arrays)
- Scientific computing with massive datasets
- Game engines and real-time applications

### ❌ **Not Ideal For:**
- Small, short-lived allocations (use regular .NET objects)
- General application development (managed memory is fine)
- When you don't need explicit memory control

## Safety Features

- **Bounds checking** on buffer access
- **Automatic disposal** with `using` statements
- **Memory leak detection** with debug allocators
- **Type safety** through generic constraints
- **Integration with Span<T>** for safe operations

## Requirements

- .NET 8.0 or later
- Unsafe code support (automatically enabled by package)

## Contributing

Contributions welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

MIT License - see [LICENSE](LICENSE) for details.

---

**ZiggyAlloc: When you need the performance of unmanaged memory with the safety of .NET** 🚀