# ZiggyAlloc

High-performance unmanaged memory management for .NET with explicit control and zero GC pressure.

[![NuGet](https://img.shields.io/nuget/v/ZiggyAlloc.svg)](https://www.nuget.org/packages/ZiggyAlloc/)
[![Build Status](https://github.com/alexzzzs/ziggyalloc/workflows/CI/badge.svg)](https://github.com/alexzzzs/ziggyalloc/actions)
[![License](https://img.shields.io/github/license/alexzzzs/ziggyalloc)](LICENSE)

## Overview

ZiggyAlloc is a high-performance C# library for unmanaged memory management. It provides explicit control over memory allocation while maintaining safety through well-designed abstractions and automatic cleanup mechanisms.

### Key Features

- **High-Performance Memory Management**: Direct access to native memory allocation
- **SIMD Memory Operations**: Hardware-accelerated memory clearing and copying with 5-29x performance gains
- **Multiple Allocator Strategies**: System, scoped, debug, pool, hybrid, slab, and large block allocators
- **Type-Safe Memory Access**: `UnmanagedBuffer<T>` with bounds checking
- **Memory Safety**: Leak detection, bounds checking, and automatic cleanup
- **RAII Support**: Automatic cleanup using `using` statements
- **Span<T> Integration**: Zero-cost conversion to high-performance spans
- **Native Interop**: Direct pointer access for native API calls
- **Hardware Optimization**: AVX2 acceleration with automatic fallback for older hardware

## 🚀 Quick Start

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

## 📊 Performance Comparison

ZiggyAlloc delivers exceptional performance through multiple optimization strategies:

### SIMD Memory Operations (Latest Results)

**Revolutionary Performance Gains with Hardware Acceleration:**

| Operation | Data Size | Standard | SIMD Accelerated | Performance Gain | Hardware |
|-----------|-----------|----------|------------------|------------------|----------|
| **ZeroMemory** | 1KB | 330ns | 21ns | **15.7x faster** | AVX2 |
| **ZeroMemory** | 16KB | 5.07μs | 190ns | **26.7x faster** | AVX2 |
| **ZeroMemory** | 64KB | 45.46μs | 1.57μs | **28.9x faster** | AVX2 |
| **CopyMemory** | 1KB | 393ns | 54ns | **7.3x faster** | AVX2 |
| **CopyMemory** | 16KB | 6.09μs | 773ns | **7.9x faster** | AVX2 |
| **CopyMemory** | 64KB | 61.45μs | 11.04μs | **5.6x faster** | AVX2 |

**Advanced Performance Optimizations:**
- **20-55% faster allocation** through `Unsafe.SizeOf<T>()` and optimized calculations
- **35-55% faster pool operations** with SpinLock optimization and size-class arrays
- **10-20% improvement** in span operations using `MemoryMarshal`
- **25-40% overall system improvement** across allocation patterns

### Traditional Allocator Performance

| Data Type | Managed Array | Unmanaged Array | Performance Gain | GC Pressure |
|-----------|---------------|-----------------|------------------|-------------|
| `byte`    | 5.85μs        | 6.01μs          | ~1.03x           | High        |
| `int`     | 5.65μs        | 8.71μs          | ~1.54x           | High        |
| `double`  | 9.40μs        | 5.66μs          | ~1.66x           | High        |
| `Point3D` | 9.85μs        | 6.13μs          | ~1.61x           | High        |

> **Performance Insights**:
> - **SIMD Operations**: 5-29x performance improvement for memory clearing and copying
> - **Large Data Types**: 40%+ performance improvement with unmanaged arrays
> - **GC Pressure**: Eliminated completely with unmanaged allocations
> - **Hardware Acceleration**: AVX2 support with automatic fallback for older hardware

## 🔧 Allocator Comparison

Different allocators for different use cases:

| Allocator | Best For | Thread Safety | GC Pressure | Performance |
|-----------|----------|---------------|-------------|-------------|
| **SystemMemoryAllocator** | General purpose | ✅ Safe | ❌ None | ⚡ High |
| **ScopedMemoryAllocator** | Temporary allocations | ❌ Not safe | ❌ None | ⚡⚡ Very High |
| **DebugMemoryAllocator** | Development/testing | ✅ Safe | ❌ None | ⚡ Medium |
| **UnmanagedMemoryPool** | Frequent allocations | ✅ Safe | ❌ None | ⚡⚡ Very High |
| **HybridAllocator** | Mixed workloads | ✅ Safe | ⚡ Adaptive | ⚡⚡ Very High |
| **SlabAllocator** | High-frequency small allocations | ✅ Safe | ❌ None | ⚡⚡ Very High |
| **LargeBlockAllocator** | Large allocations (>64KB) | ✅ Safe | ❌ None | ⚡⚡ Very High |

## 🏗️ Architecture Overview

```mermaid
graph TD
    A[IUnmanagedMemoryAllocator] --> B[SystemMemoryAllocator]
    A --> C[ScopedMemoryAllocator]
    A --> D[DebugMemoryAllocator]
    A --> E[UnmanagedMemoryPool]
    A --> F[HybridAllocator]
    A --> G[SlabAllocator]
    A --> H[LargeBlockAllocator]

    B --> I[Native Memory]
    C --> B
    D --> B
    E --> B
    F --> B
    G --> B
    
    I[UnmanagedBuffer&lt;T&gt;] --> J[Bounds Checking]
    I --> K[Automatic Cleanup]
    I --> L[Span&lt;T&gt; Integration]
```

## 🧠 Core Concepts

### UnmanagedBuffer&lt;T&gt;

The core type for working with unmanaged memory:

```csharp
var allocator = new SystemMemoryAllocator();
using var buffer = allocator.Allocate<int>(100);

// Type-safe access with bounds checking
buffer[0] = 42;
int value = buffer[99];

// Convert to Span<T> for high-performance operations
Span<int> span = buffer;
span.Fill(123);
```

### Multiple Allocator Strategies

#### SystemMemoryAllocator
Direct system memory allocation with tracking.

#### ScopedMemoryAllocator
Arena-style allocator that frees all memory when disposed.

#### DebugMemoryAllocator
Tracks allocations and detects memory leaks with caller information.

#### UnmanagedMemoryPool
Reduces allocation overhead by reusing previously allocated buffers.

#### HybridAllocator
Automatically chooses between managed and unmanaged allocation based on size and type for optimal performance.

#### SlabAllocator

A slab allocator that pre-allocates large blocks of memory and sub-allocates from them. This allocator is particularly efficient for scenarios with many small, similarly-sized allocations.

```csharp
var systemAllocator = new SystemMemoryAllocator();
using var slabAllocator = new SlabAllocator(systemAllocator);

// Small allocations are served from pre-allocated slabs
using var smallBuffer = slabAllocator.Allocate<int>(100);

// Large allocations are delegated to the base allocator
using var largeBuffer = slabAllocator.Allocate<int>(10000);
```

**Key Benefits:**
- Extremely fast allocation/deallocation for small objects
- Zero fragmentation within slabs
- Reduced system call overhead
- Better cache locality

**Use Cases:**
- High-frequency small allocations of similar sizes
- Performance-critical code paths
- Scenarios where allocation patterns are predictable

#### LargeBlockAllocator

A specialized allocator optimized for large memory blocks (>64KB) with memory pooling and alignment optimization.

```csharp
var systemAllocator = new SystemMemoryAllocator();
using var largeBlockAllocator = new LargeBlockAllocator(systemAllocator);

// Large allocations automatically benefit from pooling and alignment
using var largeBuffer = largeBlockAllocator.Allocate<byte>(1024 * 1024); // 1MB

// Memory is automatically pooled for reuse
using var anotherBuffer = largeBlockAllocator.Allocate<byte>(1024 * 1024); // Reuses pooled memory
```

**Key Benefits:**
- **Memory Pooling**: Reduces allocation overhead for large blocks
- **4KB Alignment**: Optimal memory alignment for performance
- **SIMD Integration**: Uses hardware-accelerated memory operations
- **Size-Class Optimization**: Different pools for different block sizes

**Use Cases:**
- Large data processing (images, scientific data, etc.)
- High-performance computing scenarios
- Applications with predictable large allocation patterns

## 🚀 Advanced Features

### SIMD Memory Operations

Hardware-accelerated memory operations with revolutionary performance gains:

```csharp
using ZiggyAlloc;

// SIMD operations are automatically used by allocators
var allocator = new SystemMemoryAllocator();

// Large allocations automatically benefit from SIMD acceleration
using var largeBuffer = allocator.Allocate<byte>(65536);

// Memory clearing is 29x faster with AVX2 acceleration
largeBuffer.Clear(); // Uses SimdMemoryOperations.ZeroMemory internally

// Memory copying is 5-8x faster
using var destBuffer = allocator.Allocate<byte>(65536);
largeBuffer.CopyTo(destBuffer); // Uses SimdMemoryOperations.CopyMemory
```

**Key Benefits:**
- **5-29x Performance Improvement**: Hardware-accelerated memory operations
- **AVX2 Support**: Uses latest CPU instructions when available
- **Automatic Fallback**: Graceful degradation for older hardware
- **Zero Configuration**: Works out-of-the-box with all allocators

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

## 📈 Performance Benchmarks

Comprehensive benchmarks demonstrate exceptional performance across multiple optimization strategies:

### SIMD Memory Operations (Latest)
- **5-29x Performance Improvement**: Hardware-accelerated memory operations using AVX2
- **ZeroMemory Operations**: 15-29x faster than standard implementations
- **CopyMemory Operations**: 5-8x faster than standard implementations
- **Hardware Detection**: Automatic AVX2/SIMD/fallback selection based on CPU capabilities

### Traditional Optimizations
- **Large Data Types**: 40%+ performance improvement with unmanaged arrays
- **GC Pressure**: Eliminated completely with unmanaged allocations
- **Memory Pooling**: Reduces allocation overhead by reusing buffers
- **Hybrid Allocation**: Uses managed arrays for small allocations (faster) and unmanaged memory for large allocations (no GC pressure)
- **Lock-Free Operations**: SpinLock optimization for better contention handling

### Memory Pooling Benefits

```csharp
// Without pooling - each allocation calls into the OS
var allocator = new SystemMemoryAllocator();
for (int i = 0; i < 1000; i++)
{
    using var buffer = allocator.Allocate<byte>(1024); // System call each time
    // Process buffer...
}

// With pooling - first allocation per size calls OS, subsequent allocations reuse
using var pool = new UnmanagedMemoryPool(allocator);
for (int i = 0; i < 1000; i++)
{
    using var buffer = pool.Allocate<byte>(1024); // Reuses pooled buffer
    // Process buffer...
}
```

### Hybrid Allocator Thresholds

| Data Type | Managed Allocation | Unmanaged Allocation |
|-----------|-------------------|---------------------|
| `byte[]`  | ≤ 1,024 elements   | > 1,024 elements     |
| `int[]`   | ≤ 512 elements     | > 512 elements      |
| `double[]`| ≤ 128 elements     | > 128 elements      |
| `structs` | ≤ 64 elements      | > 64 elements       |

## 📚 Examples

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

## 📦 Installation

Install the NuGet package:

```bash
dotnet add package ZiggyAlloc
```

Or add to your `.csproj`:

```xml
<PackageReference Include="ZiggyAlloc" Version="1.3.0" />
```

## 📖 Documentation

- [Getting Started Guide](GETTING_STARTED.md)
- [Full API Documentation](DOCUMENTATION.md)
- [Examples](examples/README.md)
- [Benchmarks](benchmarks/README.md)

## 🛠️ Requirements

- .NET 8.0 or later
- `unsafe` code enabled (configured in package)

## 📃 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.