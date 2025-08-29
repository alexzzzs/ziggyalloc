# ZiggyAlloc Documentation

## Table of Contents

1. [Overview](#overview)
2. [Core Concepts](#core-concepts)
3. [Memory Allocators](#memory-allocators)
4. [UnmanagedBuffer<T>](#unmanagedbuffert)
5. [Memory Safety](#memory-safety)
6. [API Reference](#api-reference)
7. [Best Practices](#best-practices)
8. [Performance Considerations](#performance-considerations)
9. [Common Patterns](#common-patterns)

## Overview

ZiggyAlloc is a high-performance C# library for unmanaged memory management. It provides explicit control over memory allocation while maintaining safety through well-designed abstractions and automatic cleanup mechanisms.

### Key Features

- **High-Performance Memory Management**: Direct access to native memory allocation
- **Multiple Allocator Strategies**: System, scoped, and debug allocators
- **Type-Safe Memory Access**: `UnmanagedBuffer<T>` with bounds checking
- **Memory Safety**: Leak detection, bounds checking, and automatic cleanup
- **RAII Support**: Automatic cleanup using `using` statements
- **Span<T> Integration**: Zero-cost conversion to high-performance spans
- **Native Interop**: Direct pointer access for native API calls

## Core Concepts

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

### UnmanagedBuffer<T>

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

### Automatic Memory Management

ZiggyAlloc uses RAII principles for deterministic cleanup:

- **Using statements**: Automatic cleanup when buffers go out of scope
- **Scoped allocators**: All allocations freed when allocator is disposed
- **Debug tracking**: Leak detection with caller information

## Memory Allocators

### SystemMemoryAllocator

High-performance allocator using native system calls.

```csharp
var allocator = new SystemMemoryAllocator();
using var buffer = allocator.Allocate<int>(100, zeroMemory: true);

// Use buffer...
// Memory automatically freed when buffer is disposed

Console.WriteLine($"Total allocated: {allocator.TotalAllocatedBytes} bytes");
```

**Use Cases:**
- General-purpose unmanaged memory allocation
- Performance-critical code
- Long-lived allocations
- Native API interop

**Thread Safety:** ✅ Thread-safe

### ScopedMemoryAllocator

Automatically frees all allocations when disposed.

```csharp
using var allocator = new ScopedMemoryAllocator();
using var buffer1 = allocator.Allocate<int>(50);
using var buffer2 = allocator.Allocate<double>(100);

// All memory freed automatically when allocator is disposed
```

**Use Cases:**
- Temporary allocations within a scope
- Arena-style allocation patterns
- When you want automatic cleanup of multiple allocations

**Thread Safety:** ❌ Not thread-safe (use separate instances per thread)

### DebugMemoryAllocator

Tracks allocations and detects memory leaks with caller information.

```csharp
using var debugAlloc = new DebugMemoryAllocator("MyComponent", 
    Z.DefaultAllocator, MemoryLeakReportingMode.Throw);

using var buffer1 = debugAlloc.Allocate<int>(10); // Properly disposed

var buffer2 = debugAlloc.Allocate<int>(5);
// Forgot to dispose buffer2 - will report leak with file/line info
```

**Use Cases:**
- Development and testing
- Memory leak detection
- Debugging memory-related issues

**Thread Safety:** ✅ Thread-safe

### UnmanagedMemoryPool

A memory pool for unmanaged buffers that reduces allocation overhead by reusing previously allocated buffers.

```csharp
var systemAllocator = new SystemMemoryAllocator();
using var pool = new UnmanagedMemoryPool(systemAllocator);

// First allocation - creates new buffer
using var buffer1 = pool.Allocate<int>(100);

// Second allocation - reuses buffer from pool if available
using var buffer2 = pool.Allocate<int>(100);

// Buffers are returned to the pool when disposed
```

**Key Benefits:**
- Reduces P/Invoke overhead for frequent allocations
- Eliminates GC pressure for unmanaged allocations
- Thread-safe implementation
- Automatic cleanup of unused buffers

**Use Cases:**
- Frequent allocations of similar-sized buffers
- Performance-critical code with allocation hotspots
- Scenarios where buffer sizes are predictable

**Thread Safety:** ✅ Thread-safe

### HybridAllocator

An enhanced allocator that automatically chooses between managed and unmanaged allocation based on size and type to optimize performance for different scenarios. For small allocations where managed arrays are faster, it uses managed memory. For larger allocations where GC pressure is a concern, it uses unmanaged memory.

```csharp
var systemAllocator = new SystemMemoryAllocator();
using var hybridAllocator = new HybridAllocator(systemAllocator);

// Allocation strategy chosen automatically based on type and size
// Small allocations may use managed arrays for better performance
using var smallBuffer = hybridAllocator.Allocate<int>(100);

// Large allocations will use unmanaged memory to avoid GC pressure
using var largeBuffer = hybridAllocator.Allocate<int>(10000);
```

**Enhanced Features:**
- Intelligent allocation strategy selection based on benchmark data
- Uses managed arrays for small allocations (faster access)
- Uses unmanaged memory for large allocations (eliminates GC pressure)
- Automatic cleanup of both managed and unmanaged resources
- Thread-safe implementation
- Implements IDisposable for proper resource management

**Performance Thresholds:**
- Byte arrays: Managed allocation for ≤1024 elements
- Int arrays: Managed allocation for ≤512 elements
- Double arrays: Managed allocation for ≤128 elements
- Structs: Managed allocation for ≤64 elements
- Other types: Calculated thresholds based on size

**Use Cases:**
- Applications that handle various data types and sizes
- When you want optimal performance without manual tuning
- Mixed workloads with different allocation patterns
- Scenarios where you're unsure which allocation strategy to use

**Thread Safety:** ✅ Thread-safe

## UnmanagedBuffer<T>

The core type for working with unmanaged memory in ZiggyAlloc.

```csharp
var allocator = new SystemMemoryAllocator();
using var buffer = allocator.Allocate<int>(100, zeroMemory: true);

// Array-like access with bounds checking
buffer[0] = 42;
buffer[99] = 100;

// Properties
Console.WriteLine($"Length: {buffer.Length}");
Console.WriteLine($"Size: {buffer.SizeInBytes} bytes");
Console.WriteLine($"Pointer: 0x{buffer.RawPointer:X}");

// Span conversion for high-performance operations
Span<int> span = buffer;
span.Fill(123);

// Utility methods
buffer.Clear();           // Zero all bytes
buffer.Fill(42);          // Fill with value
buffer.CopyFrom(source);  // Copy from another buffer/span
```

**Key Features:**
- **Bounds checking**: Prevents buffer overruns and underruns
- **Type safety**: Compile-time type checking for unmanaged types
- **Span integration**: Zero-cost conversion to `Span<T>` and `ReadOnlySpan<T>`
- **Automatic cleanup**: Integrates with `using` statements
- **Native interop**: Direct pointer access for native API calls
- **Memory utilities**: Built-in methods for common operations

**Properties:**
- `int Length` - Number of elements in the buffer
- `int SizeInBytes` - Total size in bytes
- `IntPtr RawPointer` - Raw pointer for native interop
- `bool IsEmpty` - True if length is 0
- `bool IsValid` - True if pointer is not null
- `ref T First` - Reference to first element
- `ref T Last` - Reference to last element

**Methods:**
- `ref T this[int index]` - Bounds-checked element access
- `Span<T> AsSpan()` - Convert to Span<T>
- `ReadOnlySpan<T> AsReadOnlySpan()` - Convert to ReadOnlySpan<T>
- `void Fill(T value)` - Fill buffer with value
- `void Clear()` - Zero all bytes
- `void CopyFrom(ReadOnlySpan<T> source)` - Copy from span
- `void Dispose()` - Free memory if owned

## Memory Safety

ZiggyAlloc provides multiple layers of memory safety without sacrificing performance:

### Bounds Checking

All buffer access is bounds-checked to prevent buffer overruns:

```csharp
using var buffer = allocator.Allocate<int>(10);

try 
{
    buffer[15] = 42; // Throws IndexOutOfRangeException
}
catch (IndexOutOfRangeException ex)
{
    Console.WriteLine($"Bounds check prevented buffer overrun: {ex.Message}");
}
```

### Automatic Cleanup

Buffers integrate with `using` statements for deterministic cleanup:

```csharp
using var buffer = allocator.Allocate<byte>(1024);
// Memory automatically freed when buffer goes out of scope
// No manual Free() calls needed
```

### Leak Detection

Debug allocator tracks allocations and reports leaks with caller information:

```csharp
using var debugAlloc = new DebugMemoryAllocator("Test", Z.DefaultAllocator, 
    MemoryLeakReportingMode.Throw);

var buffer = debugAlloc.Allocate<int>(10);
// Forgot to dispose - will throw with file/line information
```

### Type Safety

Generic constraints ensure only unmanaged types can be allocated:

```csharp
// ✅ Valid - int is unmanaged
using var intBuffer = allocator.Allocate<int>(100);

// ❌ Compile error - string is managed
// using var stringBuffer = allocator.Allocate<string>(100);
```

## API Reference

### IUnmanagedMemoryAllocator Interface

```csharp
public interface IUnmanagedMemoryAllocator
{
    // Allocate memory for elements
    UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false) where T : unmanaged;
    
    // Free previously allocated memory
    void Free(IntPtr pointer);
    
    // Whether individual deallocation is supported
    bool SupportsIndividualDeallocation { get; }
    
    // Total bytes currently allocated
    long TotalAllocatedBytes { get; }
}
```

### SystemMemoryAllocator

```csharp
public sealed class SystemMemoryAllocator : IUnmanagedMemoryAllocator
{
    // Allocate memory using native system calls
    public UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false);
    
    // Free memory
    public void Free(IntPtr pointer);
    
    // Properties
    public bool SupportsIndividualDeallocation => true;
    public long TotalAllocatedBytes { get; }
    
    // Static utility methods
    public static UnmanagedBuffer<T> WrapExisting<T>(IntPtr pointer, int elementCount);
    public static UnmanagedBuffer<T> WrapSpan<T>(Span<T> span);
}
```

### ScopedMemoryAllocator

```csharp
public sealed class ScopedMemoryAllocator : IUnmanagedMemoryAllocator, IDisposable
{
    // Allocate memory (freed when allocator is disposed)
    public UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false);
    
    // Individual deallocation not supported
    public void Free(IntPtr pointer); // Throws NotSupportedException
    
    // Properties
    public bool SupportsIndividualDeallocation => false;
    public long TotalAllocatedBytes { get; }
    
    // Dispose frees all allocations
    public void Dispose();
}
```

### DebugMemoryAllocator

```csharp
public sealed class DebugMemoryAllocator : IUnmanagedMemoryAllocator, IDisposable
{
    // Constructor
    public DebugMemoryAllocator(string name, IUnmanagedMemoryAllocator backingAllocator, 
        MemoryLeakReportingMode reportingMode = MemoryLeakReportingMode.Log);
    
    // Allocate with caller tracking
    public UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false);
    
    // Free with tracking removal
    public void Free(IntPtr pointer);
    
    // Properties
    public bool SupportsIndividualDeallocation { get; }
    public long TotalAllocatedBytes { get; }
    
    // Leak detection
    public void ReportMemoryLeaks();
    public int GetTrackedAllocationCount();
    public void Dispose(); // Reports leaks
}
```

### Z Static Class

```csharp
public static class Z
{
    // Default allocator instance
    public static readonly SystemMemoryAllocator DefaultAllocator;
}
```

### DeferScope

```csharp
public sealed class DeferScope : IDisposable
{
    // Start a new defer scope
    public static DeferScope Start();
    
    // Register cleanup action (executed in reverse order)
    public void Defer(Action action);
    
    // Number of deferred actions
    public int Count { get; }
    
    // Execute all deferred actions in reverse order
    public void Dispose();
}

// Extension methods for convenient allocation with defer
public static class DeferExtensions
{
    // Allocate buffer and automatically defer its disposal
    public static UnmanagedBuffer<T> AllocateDeferred<T>(
        this IUnmanagedMemoryAllocator allocator,
        DeferScope defer,
        int elementCount,
        bool zeroMemory = false) where T : unmanaged;
        
    // Allocate single element buffer with deferred disposal
    public static UnmanagedBuffer<T> AllocateDeferred<T>(
        this IUnmanagedMemoryAllocator allocator,
        DeferScope defer,
        bool zeroMemory = false) where T : unmanaged;
}
```

### UnmanagedMemoryPool

```csharp
public sealed class UnmanagedMemoryPool : IUnmanagedMemoryAllocator, IDisposable
{
    // Constructor
    public UnmanagedMemoryPool(IUnmanagedMemoryAllocator baseAllocator);
    
    // Allocate memory, reusing from pool if available
    public UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false) where T : unmanaged;
    
    // Free memory by returning to pool
    public void Free(IntPtr pointer);
    
    // Properties
    public bool SupportsIndividualDeallocation { get; }
    public long TotalAllocatedBytes { get; }
    
    // Clear all pooled buffers
    public void Clear();
    
    // Dispose the pool
    public void Dispose();
}
```

### HybridAllocator

```csharp
public sealed class HybridAllocator : IUnmanagedMemoryAllocator
{
    // Constructor
    public HybridAllocator(IUnmanagedMemoryAllocator unmanagedAllocator);
    
    // Allocate memory using optimal strategy based on type and size
    public UnmanagedBuffer<T> Allocate<T>(int elementCount, bool zeroMemory = false) where T : unmanaged;
    
    // Free previously allocated memory
    public void Free(IntPtr pointer);
    
    // Properties
    public bool SupportsIndividualDeallocation { get; }
    public long TotalAllocatedBytes { get; }
}
```

## Best Practices

### 1. Choose the Right Allocator

```csharp
// For general-purpose, high-performance allocations
var system = new SystemMemoryAllocator();

// For temporary allocations within a scope
using var scoped = new ScopedMemoryAllocator();

// For development and debugging
using var debug = new DebugMemoryAllocator("Component", Z.DefaultAllocator);
```

### 2. Always Use `using` Statements

```csharp
// ✅ Preferred: Automatic cleanup
using var buffer = allocator.Allocate<int>(100);

// ❌ Avoid: Manual cleanup (error-prone)
var buffer = allocator.Allocate<int>(100);
// ... easy to forget Dispose()
```

### 3. Leverage Span<T> for Performance

```csharp
using var buffer = allocator.Allocate<int>(1000);

// Zero-cost conversion to Span<T>
Span<int> span = buffer;

// Use span for high-performance operations
span.Fill(42);
span.Sort();
```

### 4. Use Bounds Checking Wisely

```csharp
using var buffer = allocator.Allocate<int>(100);

// Bounds checking for safety
for (int i = 0; i < buffer.Length; i++)
{
    buffer[i] = i * i;
}

// Or use spans for bulk operations
Span<int> span = buffer;
for (int i = 0; i < span.Length; i++)
{
    span[i] = i * i;
}
```

### 5. Handle Allocation Failures

```csharp
try
{
    using var buffer = allocator.Allocate<LargeStruct>(1_000_000);
    // Use buffer...
}
catch (OutOfMemoryException ex)
{
    Console.WriteLine($"Allocation failed: {ex.Message}");
    // Handle gracefully
}
```

### 6. Monitor Memory Usage

```csharp
var allocator = new SystemMemoryAllocator();

using var buffer1 = allocator.Allocate<byte>(1024 * 1024); // 1MB
using var buffer2 = allocator.Allocate<int>(100_000);      // 400KB

Console.WriteLine($"Total allocated: {allocator.TotalAllocatedBytes:N0} bytes");
```

### 7. Use UnmanagedMemoryPool for Frequent Allocations

```csharp
// For scenarios with frequent allocations of similar-sized buffers
using var pool = new UnmanagedMemoryPool(Z.DefaultAllocator);

// Pool reuses buffers, reducing allocation overhead
for (int i = 0; i < 1000; i++)
{
    using var buffer = pool.Allocate<byte>(1024);
    // Process buffer...
    // Buffer returned to pool when disposed
}
```

### 8. Use HybridAllocator for Mixed Workloads

```csharp
// For applications handling various data types and sizes
var hybridAllocator = new HybridAllocator(Z.DefaultAllocator);

// Allocation strategy chosen automatically based on benchmarks
using var smallBuffer = hybridAllocator.Allocate<byte>(100);   // May use managed
using var largeBuffer = hybridAllocator.Allocate<double>(1000); // Will use unmanaged
```

## Performance Considerations

### 1. Allocation Patterns

```csharp
// Efficient: Single large allocation
var buffer = context.AllocateSlice<byte>(1024 * 1024);

// Inefficient: Many small allocations
for (int i = 0; i < 1000; i++)
{
    var small = context.Allocate<int>(); // Avoid this pattern
}
```

### 2. Use Spans for Performance

```csharp
var slice = context.AllocateSlice<int>(1000);
Span<int> span = slice; // Zero-cost conversion

// Fast operations using span
span.Fill(42);
span.Sort();
```

### 3. Minimize Allocations in Hot Paths

```csharp
// Pre-allocate buffers outside loops
var workBuffer = context.AllocateSlice<byte>(4096);

for (int i = 0; i < iterations; i++)
{
    // Reuse buffer instead of allocating
    ProcessData(workBuffer);
}
```

### 4. Choose Appropriate Zeroing

```csharp
// Only zero when necessary
var buffer = context.AllocateSlice<byte>(size, zeroed: false);
if (needsZeroing)
{
    buffer.AsSpan().Clear(); // Manual zeroing when needed
}
```

## Performance Optimizations

ZiggyAlloc includes advanced performance optimizations to address specific use cases:

### UnmanagedMemoryPool

The UnmanagedMemoryPool reduces allocation overhead by reusing previously allocated buffers. This is particularly effective for:

1. **Frequent Allocations**: Scenarios with high-frequency allocations of similar-sized buffers
2. **Allocation Hotspots**: Code paths where allocation performance is critical
3. **Predictable Buffer Sizes**: Applications that work with consistent buffer sizes

**Performance Benefits:**
- Eliminates P/Invoke overhead for pooled allocations
- Reduces system memory allocation calls
- Maintains zero GC pressure for unmanaged allocations

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

### HybridAllocator

The HybridAllocator automatically chooses between allocation strategies based on benchmark-driven heuristics:

1. **Small Data Types**: For small allocations where managed arrays are faster
2. **Large Data Types**: For large allocations where unmanaged arrays eliminate GC pressure
3. **Mixed Workloads**: Applications that handle various data types and sizes

**Decision Criteria:**
- Byte arrays: Managed allocation for sizes ≤ 1KB
- Int arrays: Managed allocation for sizes ≤ 2KB (512 elements)
- Double arrays: Unmanaged allocation preferred for all but smallest sizes
- Struct arrays: Unmanaged allocation generally preferred

```csharp
var hybrid = new HybridAllocator(Z.DefaultAllocator);

// Small byte array - may use managed allocation
using var smallBytes = hybrid.Allocate<byte>(100);

// Large byte array - will use unmanaged allocation
using var largeBytes = hybrid.Allocate<byte>(10000);

// Any double array - will use unmanaged allocation to avoid GC pressure
using var anyDoubles = hybrid.Allocate<double>(100);
```

## Common Patterns

### 1. Resource Management

```csharp
public class ResourceManager
{
    private readonly AllocationContext _context;
    
    public ResourceManager(AllocationContext context)
    {
        _context = context;
    }
    
    public void ProcessWithCleanup()
    {
        using var defer = DeferredCleanupScope.Create();
        
        var resource1 = AcquireResource(_context, defer);
        var resource2 = AcquireResource(_context, defer);
        
        // Use resources...
        // Automatic cleanup in reverse order
    }
}
```

### 2. Buffer Management

```csharp
public class BufferPool
{
    private readonly ScopedMemoryAllocator _allocator = new();
    
    public Slice<T> GetBuffer<T>(int size) where T : unmanaged
    {
        return _allocator.AllocateSlice<T>(size);
    }
    
    public void Dispose() => _allocator.Dispose(); // Frees all buffers
}
```

### 3. String Processing

```csharp
public string ProcessStrings(AllocationContext context, string[] inputs)
{
    using var defer = DeferredCleanupScope.Create();
    
    var totalLength = inputs.Sum(s => Encoding.UTF8.GetByteCount(s));
    var buffer = context.AllocateSliceDeferred<byte>(defer, totalLength);
    
    // Process strings using the buffer...
    return Encoding.UTF8.GetString(buffer.AsSpan());
}
```

### 4. Interop with Native Code

```csharp
public void CallNativeFunction(AllocationContext context)
{
    using var defer = DeferredCleanupScope.Create();
    
    var data = context.AllocateSliceDeferred<NativeStruct>(defer, 1000);
    
    // Fill data...
    
    // Pass to native function
    NativeLibrary.ProcessData(data.Ptr.Raw, data.Length);
}
```

---

For more examples and advanced usage patterns, see the [examples](examples/) directory and [GETTING_STARTED.md](GETTING_STARTED.md).