# ZiggyAlloc Examples

This directory contains organized examples demonstrating various use cases of ZiggyAlloc.

## Organization

The examples are organized into the following categories:

### 01-Basic
Fundamental usage examples for getting started with ZiggyAlloc.

- `01-SimpleAllocation.cs` - Basic memory allocation and usage

### 02-Advanced
Advanced features and patterns.

- `DeferPatterns.cs` - Advanced defer patterns and error handling
- `MemoryLeakDetection.cs` - Memory leak detection with DebugMemoryAllocator 

### 03-Allocators
Examples specific to different allocator types.

- `HybridAllocatorExample.cs` - Demonstrates the enhanced HybridAllocator

### 04-Performance
Performance optimization techniques and benchmarks.

- `UnmanagedMemoryPoolExample.cs` - Shows how to use memory pools for improved performance

### 05-RealWorld
Real-world application scenarios.

- `ImageProcessingExample.cs` - Image processing without GC pressure

## Running Examples

You can run the examples in several ways:

### 1. Run the main examples program with arguments:
```bash
cd examples
dotnet run -- basic
dotnet run -- allocators
dotnet run -- performance
dotnet run -- realworld
```

### 2. Run individual example projects:
```bash
cd examples/01-Basic
dotnet run

cd examples/03-Allocators
dotnet run
```

### 3. Run the original examples:
```bash
cd examples
dotnet run
# Then follow the prompts to run specific example categories
```

## Example Descriptions

### Basic Examples
These examples demonstrate fundamental ZiggyAlloc concepts:
- Simple memory allocation and automatic cleanup
- Using `using` statements for RAII-style memory management
- Converting buffers to `Span<T>` for high-performance operations

### Allocator Examples
These examples show the different allocator types:
- **HybridAllocator**: Automatically chooses between managed and unmanaged allocation based on size and type
- **SystemMemoryAllocator**: Direct system memory allocation
- **ScopedMemoryAllocator**: Arena-style allocation with automatic cleanup
- **DebugMemoryAllocator**: Leak detection and debugging
- **UnmanagedMemoryPool**: Reusing buffers to reduce allocation overhead

### Performance Examples
These examples demonstrate performance optimization techniques:
- Memory pooling for frequent allocations
- Avoiding GC pressure with large allocations
- Efficient buffer operations using `Span<T>`

### Real-World Examples
These examples show practical applications:
- Image processing without GC pressure
- Native API interop scenarios
- Scientific computing with large datasets
- Custom memory layouts for cache-friendly access

## Key Concepts Demonstrated

1. **Automatic Memory Management**: Using `using` statements and `Dispose()` patterns
2. **Performance Optimization**: Choosing the right allocator for the job
3. **Interop**: Working with native APIs and raw pointers
4. **Safety**: Bounds checking and type safety with `UnmanagedBuffer<T>`
5. **Flexibility**: Multiple allocation strategies for different scenarios