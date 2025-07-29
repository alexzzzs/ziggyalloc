# ZiggyAlloc Refactor Plan

## Core Issues Identified

1. **Naming Problems**
   - `Z` - meaningless abbreviation
   - `Ctx` - unclear abbreviation  
   - `@in/@out` - confusing C# keywords
   - Inconsistent method names (ReadChar vs Read)

2. **Design Problems**
   - Context mixing allocator + I/O is confusing
   - No clear value proposition over existing .NET features
   - Examples show trivial int allocation (no GC benefit)
   - Missing XML documentation

3. **Value Proposition Unclear**
   - How is this better than Span<T>?
   - What's the real performance benefit?
   - When would you actually use this?

## New Focus: High-Performance Unmanaged Memory Management

### Core Value Propositions

1. **Interop Scenarios**: Allocating memory for native API calls
2. **Large Buffer Management**: Avoiding GC pressure for large allocations
3. **Custom Memory Layouts**: Struct-of-arrays patterns
4. **Performance-Critical Code**: Game engines, scientific computing
5. **Memory Pool Management**: Reusable buffer pools

### Simplified Architecture

```
UnmanagedMemoryAllocator (interface)
├── SystemAllocator (malloc/free wrapper)
├── PoolAllocator (object pooling)
├── ArenaAllocator (bump allocator)
└── DebugAllocator (leak detection)

UnmanagedBuffer<T> (main type)
├── Direct memory access
├── Span<T> conversion
├── Native interop support
└── Automatic cleanup options
```

### Remove Confusing Parts
- Remove I/O abstractions (not core to memory management)
- Remove "Context" concept (mixing concerns)
- Focus purely on memory allocation
- Clear naming throughout

### Real-World Examples
- Native API interop
- Large scientific datasets
- Game engine buffers
- Image/audio processing
- Network packet buffers