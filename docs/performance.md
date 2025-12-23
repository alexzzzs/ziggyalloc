# Performance Benchmarks

## SIMD Memory Operations

Uses SIMD instructions when supported by hardware.

| Operation | Data Size | Standard | SIMD Accelerated | Notes | Hardware |
|-----------|-----------|----------|------------------|-------|----------|
| ZeroMemory | 1KB | 330ns | 21ns | Uses AVX2 when available | AVX2 |
| ZeroMemory | 16KB | 5.07μs | 190ns | Uses AVX2 when available | AVX2 |
| ZeroMemory | 64KB | 45.46μs | 1.57μs | Uses AVX2 when available | AVX2 |
| CopyMemory | 1KB | 393ns | 54ns | Uses AVX2 when available | AVX2 |
| CopyMemory | 16KB | 6.09μs | 773ns | Uses AVX2 when available | AVX2 |
| CopyMemory | 64KB | 61.45μs | 11.04μs | Uses AVX2 when available | AVX2 |

## Traditional Allocator Performance

| Data Type | Managed Array | Unmanaged Array | Notes | GC Pressure |
|-----------|---------------|-----------------|-------|-------------|
| `byte`    | 5.85μs        | 6.01μs          | Depends on workload | High        |
| `int`     | 5.65μs        | 8.71μs          | Depends on workload | High        |
| `double`  | 9.40μs        | 5.66μs          | Depends on workload | High        |
| `Point3D` | 9.85μs        | 6.13μs          | Depends on workload | High        |

Performance varies by hardware and workload. Unmanaged allocations eliminate GC pressure.

## Memory Pooling Benefits

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

## Hybrid Allocator Thresholds

| Data Type | Managed Allocation | Unmanaged Allocation |
|-----------|-------------------|---------------------|
| `byte[]`  | ≤ 1,024 elements   | > 1,024 elements     |
| `int[]`   | ≤ 512 elements     | > 512 elements      |
| `double[]`| ≤ 128 elements     | > 128 elements      |
| `structs` | ≤ 64 elements      | > 64 elements       |
