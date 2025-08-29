# ZiggyAlloc Benchmarks

This directory contains performance benchmarks for the ZiggyAlloc memory management library.

## Benchmark Categories

### AllocationBenchmarks.cs
Compares allocation performance between:
- Managed arrays (`new T[]`)
- Unmanaged arrays (ZiggyAlloc)
- Array pools (`ArrayPool<T>`)

Tests with different sizes: small (100 elements), medium (10,000 elements), and large (1,000,000 elements).

### AllocatorBenchmarks.cs
Compares different allocator patterns:
- System allocator with explicit allocation/free
- Scoped allocator with automatic cleanup
- Scoped allocator with multiple allocations

### AllocatorComparisonBenchmarks.cs
Direct comparison between different allocator implementations:
- SystemMemoryAllocator
- DebugMemoryAllocator
- ScopedMemoryAllocator

### DataTypeBenchmarks.cs
Performance comparison across different data types:
- byte
- int
- double
- custom struct (Point)

## Running Benchmarks

To run all benchmarks:

```bash
cd benchmarks
dotnet run -c Release
```

To run a specific benchmark class:

```bash
dotnet run -c Release --filter *AllocationBenchmarks*
```

## Interpreting Results

Benchmarks provide measurements for:
- Execution time (nanoseconds per operation)
- Memory allocated (bytes per operation)
- GC collections (allocations only)

The `Baseline = true` attribute in benchmarks designates a reference point for performance comparisons. Other benchmarks will show relative performance compared to the baseline.