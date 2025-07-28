# ZiggyAlloc

[![NuGet](https://img.shields.io/nuget/v/ZiggyAlloc.svg)](https://www.nuget.org/packages/ZiggyAlloc/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A C# library inspired by Zig's memory and context management, bringing explicit memory control and Zig-style patterns to .NET.

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package ZiggyAlloc
```

Or via Package Manager Console in Visual Studio:

```powershell
Install-Package ZiggyAlloc
```

## Features

- **Explicit Memory Management**: Manual control over allocations with multiple allocator types
- **Zig-style Context System**: Pass allocators and I/O through context structures  
- **Memory Safety**: Debug allocator with leak detection and caller information
- **RAII Support**: Automatic cleanup with `using` statements and defer scopes
- **Zero-cost Abstractions**: Minimal overhead ref structs for pointers and slices
- **Cross-platform**: Works on .NET 8.0+ (Windows, Linux, macOS)

## Quick Start

```csharp
using System;
using System.Text;
using ZiggyAlloc;

class Program
{
    static void Main()
    {
        // Use a DebugAllocator to catch memory leaks
        using var debugAllocator = new DebugAllocator("Main", Z.DefaultAllocator);
        var ctx = new Ctx(debugAllocator, Z.ctx.@out, Z.ctx.@in);

        using var defer = DeferScope.Start();

        // 1. Deferred formatting (like std.fmt.allocPrint in Zig)
        var message = ctx.FormatToSlice(defer, "Hello, {0}!", "Ziggy");
        ctx.PrintLine(Encoding.UTF8.GetString(message));

        // 2. RAII-style allocation
        using (var number = ctx.Auto<int>())
        {
            number.Value = 123;
            ctx.PrintLine(number.Value);
        } // Automatically freed

        // 3. Deferred slice allocation
        var slice = ctx.AllocSlice<int>(defer, 5, zeroed: true);
        for (int i = 0; i < slice.Length; i++) slice[i] = i;
        
        // 4. Implicit conversion to ReadOnlySpan<T>
        PrintNumbers(slice);
    }
    
    static void PrintNumbers(ReadOnlySpan<int> numbers)
    {
        foreach (var num in numbers) Console.Write($"{num} ");
        Console.WriteLine();
    }
}
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