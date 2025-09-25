# ZiggyAlloc Benchmarks

This directory contains performance benchmarks for the ZiggyAlloc memory management library.

## 📊 Benchmark Overview

ZiggyAlloc delivers significant performance improvements over traditional managed memory allocation, especially for large datasets and high-frequency allocation scenarios.

### Key Performance Insights

- **Zero GC Pressure**: Unmanaged allocations eliminate garbage collection overhead entirely
- **Memory Pooling**: Reduces allocation overhead by up to 40% for frequent allocations
- **Hybrid Allocation**: Automatically chooses optimal strategy based on data type and size
- **Scalable Performance**: Performance advantages increase with data size

## 🎛️ Benchmark Modes

The comprehensive benchmark runners support several modes for different testing scenarios:

### Available Modes

| Mode | Description | Use Case | Benchmarks Included |
|------|-------------|----------|-------------------|
| **all** | Run all benchmark classes | Complete performance analysis | All 10 benchmark classes |
| **quick** | Fast subset of benchmarks | Quick performance check | Allocation, Allocator, Pooling |
| **experimental** | Latest optimization tests | Testing new features | ExperimentalOptimizationsBenchmarks |
| **performance** | Performance-focused tests | Detailed performance analysis | Allocation, Multithreading, Pooling |
| **comparison** | Compare different allocators | Choosing the right allocator | Comparison, DataType benchmarks |
| **specific** | Single benchmark class | Targeted testing | User-specified class |

### Benchmark Classes Overview

## 🧪 Benchmark Categories

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

### MultithreadingBenchmarks.cs
Performance tests under multithreading scenarios:
- Parallel allocation patterns with different allocator types
- Thread safety verification for thread-safe allocators
- Producer-consumer patterns with memory allocation

### RealWorldScenarioBenchmarks.cs
Benchmarks simulating real-world usage scenarios:
- Image processing workflows with large buffers
- Audio processing with continuous buffers
- Network packet processing with many small allocations
- Database record processing with mixed allocation sizes
- Game engine simulation with various allocation patterns
- Scientific computing with large matrix operations

## 🚀 Running Benchmarks

### Quick Start Scripts

The easiest way to run benchmarks is using the comprehensive runner scripts:

#### PowerShell (Recommended)
```powershell
# Run all benchmarks
.\run-benchmarks.ps1

# Run quick benchmarks only (faster)
.\run-benchmarks.ps1 -Mode quick

# Run experimental benchmarks
.\run-benchmarks.ps1 -Mode experimental

# Run specific benchmark class
.\run-benchmarks.ps1 -Mode specific -Filter AllocationBenchmarks

# Run with parallel execution and save results
.\run-benchmarks.ps1 -Parallel -SaveResults

# Show all options
Get-Help .\run-benchmarks.ps1
```

#### Batch File (Windows)
```cmd
# Run all benchmarks
run-benchmarks.bat

# Run quick benchmarks only
run-benchmarks.bat -mode quick

# Run experimental benchmarks
run-benchmarks.bat -mode experimental

# Run specific benchmark class
run-benchmarks.bat -mode specific -filter AllocationBenchmarks

# Run with parallel execution and save results
run-benchmarks.bat -parallel -save

# Show all options
run-benchmarks.bat -help
```

### Manual BenchmarkDotNet Commands

#### Run All Benchmarks
```bash
cd benchmarks
dotnet run -c Release
```

#### Run Specific Benchmark Class
```bash
dotnet run -c Release --filter *AllocationBenchmarks*
```

#### Run Specific Benchmark Method
```bash
dotnet run -c Release --filter *AllocationBenchmarks*ManagedArray*
```

#### Advanced BenchmarkDotNet Options
```bash
# Run with parallel execution
dotnet run -c Release --parallel --maxJobs 4

# Save results to file
dotnet run -c Release --exporters markdown --outPath results.md

# Run with custom configuration
dotnet run -c Release --filter *Benchmarks* --join
```

## 📈 Performance Results

### Data Type Performance Comparison

| Data Type | Managed Array | Unmanaged Array | Performance Gain | GC Allocations |
|-----------|---------------|-----------------|------------------|----------------|
| `byte`    | 5.85μs        | 6.01μs          | ~1.03x           | 0.26 KB        |
| `int`     | 5.65μs        | 8.71μs          | ~1.54x           | 4.00 KB        |
| `double`  | 9.40μs        | 5.66μs          | ~1.66x           | 8.00 KB        |
| `Point3D` | 9.85μs        | 6.13μs          | ~1.61x           | 8.00 KB        |

### Allocator Performance Comparison

| Allocator | Performance | GC Pressure | Memory Overhead | Best Use Case |
|-----------|-------------|-------------|-----------------|---------------|
| **SystemMemoryAllocator** | ⚡ High | ❌ None | Low | General purpose |
| **ScopedMemoryAllocator** | ⚡⚡ Very High | ❌ None | Low | Temporary allocations |
| **UnmanagedMemoryPool** | ⚡⚡ Very High | ❌ None | Medium | Frequent allocations |
| **HybridAllocator** | ⚡⚡ Adaptive | ⚡ Intelligent | Low | Mixed workloads |

## 🧠 Interpreting Results

Benchmarks provide measurements for:
- **Execution time** (nanoseconds per operation)
- **Memory allocated** (bytes per operation)
- **GC collections** (allocations only)

### Key Metrics

- **Mean**: Arithmetic mean of all measurements
- **Error**: Half of 99.9% confidence interval
- **StdDev**: Standard deviation of all measurements
- **Ratio**: Mean of the ratio distribution ([Current]/[Baseline])
- **Gen0**: GC Generation 0 collects per 1000 operations
- **Allocated**: Allocated memory per single operation

### Reading the Results

```
| Method              | Mean     | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------|---------:|------:|-------:|----------:|------------:|
| ManagedArray_Int    | 5.650 us |  0.97 | 1.0529 |   40024 B |        3.99 |
| UnmanagedArray_Int  | 8.706 us |  1.49 |      - |         - |        0.00 |
```

This shows that while the unmanaged array takes longer per operation (1.49x), it eliminates all GC allocations (0 bytes vs 40KB).

## 🏗️ Benchmark Architecture

```
graph TD
    A[BenchmarkRunner] --> B[AllocationBenchmarks]
    A --> C[AllocatorBenchmarks]
    A --> D[AllocatorComparisonBenchmarks]
    A --> E[DataTypeBenchmarks]
    A --> F[PoolingBenchmarks]
    A --> G[HybridAllocatorBenchmarks]
    
    B --> H[Managed Arrays]
    B --> I[Unmanaged Arrays]
    B --> J[ArrayPool]
    
    C --> K[System Allocator]
    C --> L[Scoped Allocator]
    C --> M[Debug Allocator]
    
    D --> K
    D --> L
    D --> M
    
    E --> N[byte]
    E --> O[int]
    E --> P[double]
    E --> Q[struct]
```

## 🎯 Best Practices for Benchmarking

1. **Use Release Mode**: Always run benchmarks with `-c Release`
2. **Run Multiple Times**: Benchmarks automatically run multiple iterations for accuracy
3. **Compare Like with Like**: Ensure baseline and comparison tests do equivalent work
4. **Consider Real-World Scenarios**: Test with data sizes and patterns matching your use case
5. **Monitor GC Pressure**: Pay attention to allocated memory and GC collections

## ⚙️ Advanced Features

### Command-Line Options

Both PowerShell and batch scripts support these options:

| Option | Description | Example |
|--------|-------------|---------|
| `-Mode` | Benchmark mode to run | `-Mode quick` |
| `-Filter` | Filter specific benchmark class | `-Filter AllocationBenchmarks` |
| `-Configuration` | Build configuration | `-Configuration Release` |
| `-Parallel` | Run benchmarks in parallel | `-Parallel` |
| `-SaveResults` | Save results to markdown files | `-SaveResults` |
| `-OutputDir` | Output directory for results | `-OutputDir results` |
| `-MaxJobs` | Maximum parallel jobs | `-MaxJobs 8` |
| `-SkipBuild` | Skip the build step | `-SkipBuild` |
| `-Verbose` | Show detailed output | `-Verbose` |

### Output and Results

#### Console Output
- Real-time progress updates
- Execution time tracking
- Error reporting with detailed messages
- Usage examples at completion

#### Saved Results (when using `-SaveResults`)
- Timestamped markdown files
- Complete benchmark statistics
- Performance comparisons
- Memory allocation metrics

#### Example Output File
```
benchmark-results/benchmarks_20250121_143022.md
```

### Performance Monitoring

The scripts automatically track:
- **Build time**: Time to compile the benchmark project
- **Execution time**: Total time for all benchmarks
- **Individual benchmark timing**: Each benchmark class duration
- **Success/failure status**: Clear indication of any issues

## 🔧 Troubleshooting

### Common Issues

1. **.NET SDK Not Found**
   - Ensure .NET SDK is installed
   - Verify `dotnet --version` works in terminal
   - Scripts will show the detected version

2. **Build Failures**
   - Check for compilation errors
   - Ensure all dependencies are available
   - Try cleaning and rebuilding: `dotnet clean && dotnet build`

3. **Benchmark Failures**
   - Check console output for specific errors
   - Verify benchmark methods are properly attributed
   - Ensure test data is valid

4. **Permission Issues**
   - Run scripts from the benchmarks directory
   - Ensure write permissions for output directory
   - Use administrator privileges if needed

### Getting Help

```powershell
# PowerShell help
Get-Help .\run-benchmarks.ps1 -Detailed
```

```cmd
# Batch file help
run-benchmarks.bat -help
```

## 📖 Related Documentation

- [Main README](../README.md)
- [Examples](../examples/README.md)
- [API Documentation](../DOCUMENTATION.md)
- [Performance Optimization Guide](../DEVELOPMENT.md#performance-optimization)