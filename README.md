# ZiggyAlloc

High-performance unmanaged memory management for .NET with explicit control and zero GC pressure.

[![NuGet](https://img.shields.io/nuget/v/ZiggyAlloc.svg)](https://www.nuget.org/packages/ZiggyAlloc/)
[![Build Status](https://github.com/alexzzzs/ziggyalloc/workflows/CI/badge.svg)](https://github.com/alexzzzs/ziggyalloc/actions)
[![License](https://img.shields.io/github/license/alexzzzs/ziggyalloc)](LICENSE)

## Overview

ZiggyAlloc is a high-performance C# library for unmanaged memory management. It provides explicit control over memory allocation while maintaining safety through well-designed abstractions and automatic cleanup mechanisms.

### Key Features

- **High-Performance Memory Management**: Direct access to native memory allocation
- **Multiple Allocator Strategies**: System, scoped, debug, pool, and hybrid allocators
- **Type-Safe Memory Access**: `UnmanagedBuffer<T>` with bounds checking
- **Memory Safety**: Leak detection, bounds checking, and automatic cleanup
- **RAII Support**: Automatic cleanup using `using` statements
- **Span<T> Integration**: Zero-cost conversion to high-performance spans
- **Native Interop**: Direct pointer access for native API calls

## Quick Start

```csharp
using ZiggyAlloc;

// Create allocator
var allocator = new SystemMemoryAllocator();

// Allocate memory with automatic cleanup
using var buffer = allocator.Allocate<int>(1000);

// Use like a normal array with bounds checking
buffer[0] = 42;
int value = buffer[0];

// Convert to Span<T> for high-performance operations
Span<int> span = buffer;
span.Fill(123);
```

## Allocators

ZiggyAlloc provides multiple allocator implementations for different scenarios:

### SystemMemoryAllocator
Direct system memory allocation with tracking.

### ScopedMemoryAllocator
Arena-style allocator that frees all memory when disposed.

### DebugMemoryAllocator
Tracks allocations and detects memory leaks with caller information.

### UnmanagedMemoryPool
Reduces allocation overhead by reusing previously allocated buffers.

### HybridAllocator
Automatically chooses between managed and unmanaged allocation based on size and type for optimal performance.

## Performance

Benchmarks show significant performance improvements over managed arrays for large data:

- **Large Data Types**: 40%+ performance improvement with unmanaged arrays
- **GC Pressure**: Eliminated completely with unmanaged allocations
- **Memory Pooling**: Reduces allocation overhead by reusing buffers
- **Hybrid Allocation**: Uses managed arrays for small allocations (faster) and unmanaged memory for large allocations (no GC pressure)

See [PERFORMANCE_OPTIMIZATIONS.md](PERFORMANCE_OPTIMIZATIONS.md) for detailed benchmark results.

## Examples

The [examples](examples/) directory contains organized examples demonstrating various use cases:

### Basic Usage
- Simple memory allocation and automatic cleanup
- Using `using` statements for RAII-style memory management

### Advanced Features
- Different allocator types and their use cases
- Memory leak detection
- High-performance buffer operations
- Native interop scenarios

### Performance Optimization
- Memory pooling for frequent allocations
- Hybrid allocation strategies
- Avoiding GC pressure with large allocations

### Real-World Applications
- Image processing without GC pressure
- Scientific computing with large datasets
- Native API interop

To run examples:
```bash
cd examples
dotnet run -- basic
dotnet run -- allocators
dotnet run -- performance
dotnet run -- realworld
```

## Installation

Install the NuGet package:

```bash
dotnet add package ZiggyAlloc
```

Or add to your `.csproj`:

```xml
<PackageReference Include="ZiggyAlloc" Version="1.2.1" />
```

## Documentation

- [Getting Started Guide](GETTING_STARTED.md)
- [Full API Documentation](DOCUMENTATION.md)
- [Performance Optimizations](PERFORMANCE_OPTIMIZATIONS.md)
- [Examples](examples/README.md)

## Requirements

- .NET 8.0 or later
- `unsafe` code enabled (configured in package)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
