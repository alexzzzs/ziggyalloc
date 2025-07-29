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

#### SystemMemoryAllocator
High-performance allocator using native system calls:

```csharp
var allocator = new SystemMemoryAllocator();
using var buffer = allocator.Allocate<int>(10, zeroMemory: true);

// Array-like access with bounds checking
buffer[0] = 42;
int value = buffer[9];

// Automatic cleanup with 'using'
// Memory freed when buffer goes out of scope
```

#### ScopedMemoryAllocator  
Automatically frees all allocations when disposed:

```csharp
using var allocator = new ScopedMemoryAllocator();
using var buffer1 = allocator.Allocate<int>(5);
using var buffer2 = allocator.Allocate<double>(10);

// All allocations freed automatically when allocator is disposed
```

#### DebugMemoryAllocator
Tracks allocations and reports memory leaks with caller information:

```csharp
using var debugAlloc = new DebugMemoryAllocator("MyComponent", Z.DefaultAllocator);
using var buffer = debugAlloc.Allocate<int>(1);
buffer[0] = 42;

// If you forget to dispose buffer, leak will be reported with file/line info
```

### Memory Safety

- **Bounds checking**: Buffer access is bounds-checked
- **Automatic disposal**: Buffers integrate with `using` statements
- **Leak detection**: Debug allocator reports unfreed memory with caller info
- **Type safety**: Generic allocations with compile-time type checking
- **Span integration**: Seamless conversion to `Span<T>` for safe operations

## API Reference

### Core Types

- `UnmanagedBuffer<T>` - Type-safe buffer with automatic cleanup
- `IUnmanagedMemoryAllocator` - Interface for custom allocators
- `SystemMemoryAllocator` - High-performance system allocator
- `ScopedMemoryAllocator` - Arena-style allocator with automatic cleanup
- `DebugMemoryAllocator` - Leak-detecting wrapper allocator

### Allocator Interface

```csharp
public interface IUnmanagedMemoryAllocator
{
    UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false) where T : unmanaged;
    void Free(IntPtr pointer);
    bool SupportsIndividualDeallocation { get; }
    long TotalAllocatedBytes { get; }
}
```

### UnmanagedBuffer<T> Key Members

```csharp
// Properties
int Length { get; }                    // Number of elements
int SizeInBytes { get; }              // Total size in bytes
IntPtr RawPointer { get; }            // Raw pointer for interop
bool IsEmpty { get; }                 // True if length is 0
bool IsValid { get; }                 // True if pointer is not null

// Indexing
ref T this[int index] { get; }        // Bounds-checked element access
ref T First { get; }                  // Reference to first element
ref T Last { get; }                   // Reference to last element

// Span conversion
Span<T> AsSpan()                      // Convert to Span<T>
ReadOnlySpan<T> AsReadOnlySpan()      // Convert to ReadOnlySpan<T>
implicit operator Span<T>             // Implicit conversion to Span<T>

// Utility methods
void Fill(T value)                    // Fill buffer with value
void Clear()                          // Zero all bytes
void CopyFrom(ReadOnlySpan<T> source) // Copy from span
void Dispose()                        // Free memory if owned
```

## Performance

ZiggyAlloc is designed for minimal overhead:

- **Direct memory access**: UnmanagedBuffer compiles to efficient pointer operations
- **Inlined operations**: Critical paths are aggressively inlined
- **Native memory**: Uses `NativeMemory` APIs on .NET 6+ for optimal performance
- **No GC pressure**: Unmanaged allocations don't affect garbage collection
- **Zero-copy conversions**: Seamless integration with `Span<T>` without copying

## Requirements

- .NET 8.0 or later
- Unsafe code support (automatically enabled by the package)

## Core API

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

---

**ZiggyAlloc: When you need the performance of unmanaged memory with the safety of .NET** 🚀