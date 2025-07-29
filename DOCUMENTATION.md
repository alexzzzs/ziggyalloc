# ZiggyAlloc Documentation

## Table of Contents

1. [Overview](#overview)
2. [Core Concepts](#core-concepts)
3. [Memory Allocators](#memory-allocators)
4. [Memory Types](#memory-types)
5. [Context System](#context-system)
6. [Lifetime Management](#lifetime-management)
7. [API Reference](#api-reference)
8. [Best Practices](#best-practices)
9. [Performance Considerations](#performance-considerations)
10. [Common Patterns](#common-patterns)

## Overview

ZiggyAlloc is a C# library inspired by Zig's approach to memory management and context passing. It provides explicit control over memory allocation while maintaining safety through well-designed abstractions.

### Key Features

- **Explicit Memory Management**: Direct control over allocation and deallocation
- **Multiple Allocator Strategies**: Manual, scoped, and debug allocators
- **Zig-style Context Pattern**: Pass allocators and I/O through application layers
- **Memory Safety**: Bounds checking, leak detection, and caller information tracking
- **RAII Support**: Automatic cleanup using `using` statements and defer scopes
- **Zero-cost Abstractions**: High-performance ref structs with minimal overhead

## Core Concepts

### Memory Allocators

All memory allocation in ZiggyAlloc goes through implementations of `IMemoryAllocator`:

```csharp
public interface IMemoryAllocator
{
    Pointer<T> Allocate<T>(int count = 1, bool zeroed = false) where T : unmanaged;
    void Free(IntPtr pointer);
    Slice<T> AllocateSlice<T>(int count, bool zeroed = false) where T : unmanaged;
}
```

### Context Pattern

ZiggyAlloc uses Zig's context pattern to pass related functionality together:

```csharp
var context = new AllocationContext(allocator, output, input);
// Pass context through your application
ProcessData(context);
```

### Explicit Lifetime Management

Unlike garbage-collected memory, ZiggyAlloc requires explicit lifetime management:

- **Manual**: Explicit `Allocate()` and `Free()` calls
- **RAII**: Automatic cleanup with `using` statements
- **Deferred**: Cleanup actions executed at scope end
- **Scoped**: All allocations freed when allocator is disposed

## Memory Allocators

### ManualMemoryAllocator

Direct control over memory allocation and deallocation.

```csharp
var allocator = new ManualMemoryAllocator();
var ptr = allocator.Allocate<int>();
ptr.Value = 42;
allocator.Free(ptr.Raw); // Must free explicitly
```

**Use Cases:**
- Performance-critical code
- Long-lived allocations
- When you need precise control over memory lifetime

**Thread Safety:** ✅ Thread-safe

### ScopedMemoryAllocator

Automatically frees all allocations when disposed.

```csharp
using var allocator = new ScopedMemoryAllocator();
var ptr1 = allocator.Allocate<int>();
var ptr2 = allocator.Allocate<double>(10);
// All memory freed automatically on dispose
```

**Use Cases:**
- Temporary allocations within a scope
- Arena-style allocation patterns
- When you want automatic cleanup

**Thread Safety:** ❌ Not thread-safe (use separate instances per thread)

### DebugMemoryAllocator

Tracks allocations and detects memory leaks with caller information.

```csharp
using var debugAlloc = new DebugMemoryAllocator("MyComponent", 
    new ManualMemoryAllocator(), MemoryLeakReportingMode.Throw);

var ptr = debugAlloc.Allocate<int>();
// Forgot to free - will report leak with file/line info on dispose
```

**Use Cases:**
- Development and testing
- Memory leak detection
- Debugging memory-related issues

**Thread Safety:** ✅ Thread-safe

## Memory Types

### Pointer<T>

A type-safe wrapper around an unmanaged pointer.

```csharp
var ptr = allocator.Allocate<int>(5);
ptr.Value = 42;           // Access first element
ptr[2] = 100;            // Access by index
var span = ptr.AsSpan(5); // Convert to Span<T>
```

**Key Features:**
- Bounds checking for indexed access
- Type safety at compile time
- Conversion to `Span<T>` for interop
- No implicit conversions to prevent errors

### Slice<T>

A bounds-checked view over allocated memory.

```csharp
var slice = allocator.AllocateSlice<int>(10);
slice[0] = 42;                    // Bounds-checked access
Span<int> span = slice;           // Implicit conversion
ReadOnlySpan<int> roSpan = slice; // Implicit conversion
```

**Key Features:**
- Automatic bounds checking
- Implicit conversion to `Span<T>` and `ReadOnlySpan<T>`
- Length property for safe iteration
- Integration with .NET's span-based APIs

## Context System

### AllocationContext

Combines memory allocation with I/O operations following Zig's context pattern.

```csharp
var context = new AllocationContext(
    new ManualMemoryAllocator(),
    new ConsoleOutputWriter(),
    new ConsoleInputReader()
);

// Memory operations
var data = context.AllocateSlice<byte>(1024);

// I/O operations
context.WriteLine("Processing data...");
var input = context.ReadLine();

// Combined operations
var formatted = context.FormatToSliceDeferred(defer, "Result: {0}", result);
```

**Benefits:**
- Explicit dependency injection
- Easy testing with mock implementations
- Consistent API across different scenarios
- Reduced parameter passing

### I/O Abstractions

```csharp
public interface IOutputWriter
{
    void Write(string text);
    void Write(char character);
    void WriteLine();
    void WriteLine<T>(T value);
}

public interface IInputReader
{
    string? ReadLine();
    int Read();
}
```

## Lifetime Management

### RAII with AutoFreeMemory<T>

Automatic cleanup using the `using` statement:

```csharp
using var auto = context.AllocateAuto<MyStruct>();
auto.Value = new MyStruct { Field = 42 };
// Memory automatically freed when 'auto' goes out of scope
```

### Deferred Cleanup

Zig-style defer pattern for deterministic cleanup:

```csharp
using var defer = DeferredCleanupScope.Create();

var ptr1 = context.AllocateDeferred<int>(defer);
var ptr2 = context.AllocateDeferred<double>(defer, 10);

defer.DeferAction(() => Console.WriteLine("Custom cleanup"));
// Cleanup order: custom action, ptr2, ptr1 (LIFO)
```

## API Reference

### Core Types

#### `IMemoryAllocator`
- `Pointer<T> Allocate<T>(int count = 1, bool zeroed = false)`
- `void Free(IntPtr pointer)`
- `Slice<T> AllocateSlice<T>(int count, bool zeroed = false)`

#### `AllocationContext`
- Memory: `Allocate<T>()`, `AllocateSlice<T>()`, `AllocateAuto<T>()`
- I/O: `Write()`, `WriteLine()`, `ReadLine()`, `ReadCharacter()`
- Deferred: `AllocateDeferred<T>()`, `FormatToSliceDeferred()`

#### `Pointer<T>`
- `ref T Value` - Reference to the pointed value
- `ref T this[int index]` - Indexed access
- `Span<T> AsSpan(int count)` - Convert to span
- `IntPtr Raw` - Raw pointer value
- `bool IsNull` - Check for null pointer

#### `Slice<T>`
- `ref T this[int index]` - Bounds-checked indexed access
- `int Length` - Number of elements
- `Span<T> AsSpan()` - Convert to span
- `bool IsEmpty` - Check if empty
- Implicit conversions to `Span<T>` and `ReadOnlySpan<T>`

### Allocator Implementations

#### `ManualMemoryAllocator`
- Direct memory control
- Thread-safe
- Requires explicit `Free()` calls

#### `ScopedMemoryAllocator`
- Automatic cleanup on dispose
- Not thread-safe
- Cannot free individual allocations

#### `DebugMemoryAllocator`
- Leak detection with caller info
- Thread-safe
- Configurable reporting modes

### Lifetime Management

#### `AutoFreeMemory<T>`
- RAII-style automatic cleanup
- Ref struct (stack-only)
- Use with `using` statements

#### `DeferredCleanupScope`
- Zig-style defer pattern
- LIFO cleanup order
- Exception-safe cleanup

## Best Practices

### 1. Choose the Right Allocator

```csharp
// For performance-critical, long-lived allocations
var manual = new ManualMemoryAllocator();

// For temporary allocations within a scope
using var scoped = new ScopedMemoryAllocator();

// For development and debugging
using var debug = new DebugMemoryAllocator("Component", manual);
```

### 2. Use RAII When Possible

```csharp
// Preferred: Automatic cleanup
using var data = context.AllocateAuto<MyStruct>();

// Avoid: Manual cleanup (error-prone)
var ptr = context.Allocate<MyStruct>();
// ... easy to forget Free()
```

### 3. Leverage Defer Scopes

```csharp
using var defer = DeferredCleanupScope.Create();

var resource1 = AcquireResource1(defer);
var resource2 = AcquireResource2(defer);
// Resources freed in reverse order automatically
```

### 4. Use Context Pattern

```csharp
// Pass context through your application
void ProcessData(AllocationContext context)
{
    var buffer = context.AllocateSlice<byte>(1024);
    context.WriteLine("Processing...");
    // Context provides both memory and I/O
}
```

### 5. Handle Errors Properly

```csharp
try
{
    var ptr = allocator.Allocate<LargeStruct>(1000000);
    // Use ptr...
}
catch (OutOfMemoryException)
{
    // Handle allocation failure
}
finally
{
    // Ensure cleanup in error cases
}
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