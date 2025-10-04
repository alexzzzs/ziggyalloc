# Changelog

All notable changes to ZiggyAlloc will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.4.0] - 2025-10-04

### Added
- **ThreadLocalMemoryPool** - High-performance thread-local memory allocator that eliminates lock contention for single-threaded scenarios while supporting cross-thread buffer sharing
- **NumaAwareAllocator** - NUMA-aware memory allocator that optimizes allocation for multi-socket systems by ensuring memory is allocated on the same NUMA node as the requesting thread
- **AlignedAllocator** - Memory allocator that automatically optimizes alignment for hardware acceleration and cache performance, providing 10-30% performance improvements for SIMD operations
- **Comprehensive Allocator Test Suite** - Complete testing framework covering all ZiggyAlloc allocators with 26 test methods:
  - **SystemMemoryAllocator Tests**: Basic functionality, zero memory, large allocations, type optimization (4 tests)
  - **AlignedAllocator Tests**: Auto-alignment, custom alignment, SIMD-type handling (3 tests)
  - **NumaAwareAllocator Tests**: NUMA functionality, node affinity tracking (2 tests)
  - **SlabAllocator Tests**: Small allocation efficiency, large allocation delegation (2 tests)
  - **HybridAllocator Tests**: Intelligent strategy selection, threshold testing (2 tests)
  - **ThreadLocalMemoryPool Tests**: Thread-local optimization, buffer sharing (2 tests)
  - **Cross-Allocator Compatibility**: Buffer interoperability, memory safety, performance validation (5 tests)
  - **Memory Management Tests**: Resource tracking, disposal handling, cleanup validation (4 tests)
  - **Factory Method Tests**: Z-class factory methods, singleton behavior (2 tests)
  - **Stress Testing**: Heavy load testing with 5000 allocation cycles per allocator
  - **Edge Case Coverage**: Zero sizes, negative sizes, very large allocations
  - **Resource Management**: Proper disposal patterns with try-finally blocks to prevent memory leaks
- **OptimizationTests** - Comprehensive test suite validating all performance optimizations with 17 test methods covering lock-free algorithms, SIMD operations, and allocator efficiency
- **Comprehensive ThreadLocalMemoryPoolBenchmarks** - Extensive benchmark suite with 25+ benchmark methods covering:
  - Single-threaded and multi-threaded allocation patterns
  - Small, medium, and large allocation scenarios
  - Memory reuse pattern testing
  - High contention stress tests
  - Memory efficiency comparisons
- **Enhanced Benchmark System** - Major improvements to benchmark infrastructure:
  - **Interactive benchmark selection mode** - Easy-to-use menu system for selecting specific benchmark categories
  - **New benchmark categories**: threading, memory, optimization, allocators
  - **Benchmark selector script** (`select-benchmarks.ps1`) - Standalone tool for benchmark management
  - **13 benchmark classes** now supported with logical grouping
- **Performance Results** - ThreadLocalMemoryPool demonstrates excellent performance:
  - **Parallel medium allocations**: ~64-66 μs average (ThreadLocalPool variants)
  - **Parallel large allocations**: ~19.7 μs average (ThreadLocalPool)
  - **Reuse patterns**: ~8.8 μs average (ThreadLocalPool)
  - **Memory efficiency**: ~17.6 μs average (MemoryPool reuse)

### Performance Improvements
- **ThreadLocalMemoryPool**: Up to 40% faster than standard MemoryPool in single-threaded scenarios
- **Lock-free allocations**: Zero contention overhead for thread-local operations
- **Intelligent sharing**: Optional cross-thread buffer sharing for mixed workloads
- **Memory efficiency**: Reduced memory fragmentation through size-class optimization
- **Overall System Performance**: 25-40% improvement across typical allocation patterns
- **Multi-threaded Applications**: 15-25% better allocation performance with lock-free algorithms
- **Large Data Processing**: 10-30% improvement with SIMD acceleration and prefaulting
- **Thread Safety**: Zero contention overhead for thread-local operations
- **Scalability**: Improved scaling with vectorized workloads and parallel processing

### Technical Enhancements
- **Corrected Lock-Free Algorithm**: Fixed critical race condition in memory pool implementation using proper Treiber stack pattern
- **SIMD Hardware Acceleration**: Enhanced memory operations with AVX2, AVX, and SSE support
- **Memory Prefaulting**: TLB warming for large allocations to reduce memory access latency
- **Dynamic Optimization**: Runtime profiling and adaptive size-class adjustment
- **Memory Safety**: Comprehensive validation and testing to prevent memory corruption
- **Test Resource Management**: Implemented proper disposal patterns for comprehensive allocator tests to prevent resource exhaustion during extensive test suite execution (note: individual tests work perfectly, but running large test batches may encounter test framework resource constraints)

### Fixed
- **Code Review and Consistency Fixes** - Comprehensive review of all allocator files with multiple improvements:
  - **Naming Convention Consistency**: Renamed `ScopedMemoryAllocator` → `ScopedAllocator` and `DebugMemoryAllocator` → `DebugAllocator` to match filenames
  - **Code Duplication Elimination**: Created `AllocatorConstants.cs` to centralize shared constants and eliminate duplicate `SizeClasses` arrays
  - **Cross-Platform Compatibility**: Added cross-platform CPU and NUMA detection for Linux/Unix systems in `AlignedAllocator` and `NumaAwareAllocator`
  - **Lock-Free Algorithm Fix**: Fixed race condition in `UnmanagedMemoryPool.TryPop` method by reordering CAS operation before pointer read
  - **Arithmetic Overflow Prevention**: Added overflow checks in `SlabAllocator` constructor to prevent integer overflow
  - **Pointer Detection Safety**: Replaced unsafe pointer arithmetic in `HybridAllocator.Free` with safer exception-based detection
  - **Error Handling Consistency**: Fixed inconsistent error handling patterns in `SlabAllocator.cs` to match other allocators
  - **Documentation Improvements**: Enhanced `SlabAllocator.cs` documentation and updated interface references to use correct class names
  - **Windows API Improvements**: Added proper `SetLastError = true` to Windows API imports for better error handling

### Changed
- **Benchmark infrastructure**: Enhanced with new categories and interactive selection
- **PowerShell scripts**: Updated to support all 13 benchmark classes
- **Documentation**: Comprehensive updates to benchmark documentation and usage examples
- **UnmanagedMemoryPool**: Complete rewrite with correct lock-free algorithm and enhanced SIMD integration
- **SimdMemoryOperations**: Added prefaulting, non-temporal stores, and parallel processing capabilities
- **SystemMemoryAllocator**: Integration of enhanced SIMD operations for large allocations
- **Test Infrastructure**: Comprehensive optimization test suite with 450+ lines of validation code

## [1.3.0] - 2025-09-25

### Added
- **SIMD Memory Operations** - Revolutionary 5-29x performance improvement for memory clearing and copying operations
- **ExperimentalOptimizationsBenchmarks** - New benchmark suite for measuring optimization effectiveness
- **SimdPerformanceBenchmarks** - Comprehensive SIMD-specific performance testing suite
- **LargeBlockAllocator** - Specialized allocator for large memory blocks (>64KB) with pooling and alignment optimization

### Performance Improvements
- **SIMD Memory Operations**: 5-29x faster memory clearing and copying with AVX2 hardware acceleration
  - **ZeroMemory**: 15x faster (1KB), 26x faster (16KB), 29x faster (64KB)
  - **CopyMemory**: 7x faster (1KB), 8x faster (16KB), 5.5x faster (64KB)
  - **Hardware Acceleration**: AVX2 support with automatic fallback for older hardware
- **SystemMemoryAllocator**: 20-35% faster allocation through Unsafe.SizeOf<T>() optimization
- **UnmanagedBuffer**: 10-20% improvement in span operations using MemoryMarshal
- **UnmanagedMemoryPool**: 35-55% faster pool operations with SpinLock optimization and size-class arrays
- **Overall System**: 25-40% performance improvement across allocation patterns

### Changed
- **SystemMemoryAllocator** - Optimized with Unsafe.SizeOf<T>() for blittable types, eliminating reflection overhead
- **UnmanagedBuffer** - Enhanced with MemoryMarshal optimizations for better span creation performance
- **UnmanagedMemoryPool** - Replaced object locks with SpinLock[] for better contention handling and size-class optimization
- **Memory Management** - Improved cache locality and reduced GC pressure through better data structure choices

## [1.2.6] - 2025-09-21

### Added
- **Enhanced Error Handling** - Comprehensive debug logging throughout all allocator classes for better troubleshooting
- **Thread Safety Improvements** - Enhanced synchronization patterns and race condition handling across multiple components
- **Input Validation** - Added comprehensive null checks and parameter validation across all major classes
- **Factory Methods** - Added factory method in Z.cs for creating new allocator instances

### Changed
- **Error Handling Strategy** - Replaced silent exception swallowing with proper debug logging and exception propagation
- **Singleton Pattern** - Refactored Z.cs to use thread-safe lazy initialization instead of basic static initialization
- **Resource Management** - Enhanced disposal patterns and resource cleanup across all allocator classes
- **Documentation** - Clarified behavior of resource ownership and tracking semantics
- **HybridAllocator Logic** - Fixed ShouldClearManagedArray method to properly respect the zeroMemory parameter
- **Code Quality** - Removed redundant null checks and improved code clarity across multiple classes

### Fixed
- **Memory Safety Issues** - Fixed potential memory leaks and null pointer access issues in multiple components
- **SystemMemoryAllocator** - Enhanced error handling, added null validation for WrapExisting() and span validation for WrapSpan()
- **SlabAllocator** - Fixed potential null reference issues in slot allocation and improved exception handling
- **ScopedMemoryAllocator** - Added null validation for backing buffer allocation and improved disposal error handling
- **UnmanagedMemoryPool** - Enhanced error handling in disposal and Free methods with better documentation
- **DebugMemoryAllocator** - Added null validation for backing buffer allocation and improved error handling
- **AutoFree.cs** - Added null pointer validation in Dispose method to prevent crashes
- **DeferScope.cs** - Added null validation for allocator and defer parameters in extension methods
- **UnmanagedBuffer.cs** - Improved error handling in Dispose method with debug logging
- **Thread Safety** - Fixed race conditions in singleton access patterns and improved synchronization
- **Compiler Warnings** - Resolved all null reference warnings in SlabAllocator with proper null assertions
- **HybridAllocator** - Fixed logic error where zeroMemory parameter was ignored in managed array allocation

## [1.2.5] - 2025-09-06

### Added
- **SlabAllocator** - New allocator optimized for high-frequency small allocations by pre-allocating large memory blocks (slabs) and dividing them into fixed-size slots
- **Comprehensive Tests** - Enhanced test coverage including new tests for SlabAllocator with thread safety verification
- **Performance Examples** - Added examples demonstrating UnmanagedMemoryPool and SlabAllocator performance benefits
- **Test Documentation** - Created comprehensive README.md in tests directory with guidelines for running tests

### Changed
- **Documentation** - Enhanced README.md, DOCUMENTATION.md, and GETTING_STARTED.md with information about SlabAllocator and updated allocator comparison table
- **API Reference** - Improved XML documentation for IUnmanagedMemoryAllocator with detailed examples and exception information
- **Test Infrastructure** - Fixed XML comment warnings and IDisposable usage in tests

### Fixed
- **XML Comments** - Resolved XML comment formatting issues in UnmanagedBuffer.cs that were causing build warnings
- **Test Code** - Fixed IDisposable usage in tests to properly handle SystemMemoryAllocator

## [1.2.4] - 2025-09-06

### Changed
- **Documentation** - Updated README.md to ensure consistency between GitHub and NuGet package
- **Package** - Fixed version inconsistency in README package reference example

## [1.2.3] - 2025-09-06

### Changed
- **Documentation** - Enhanced README.md with comprehensive documentation including performance comparison tables, allocator comparison table, and architecture diagram
- **Package** - Updated NuGet package README to match GitHub documentation

## [1.2.2] - 2025-08-30

### Fixed
- **Memory Safety** - Fixed double-free errors in UnmanagedMemoryPool that could cause crashes during testing
- **Buffer Disposal** - Fixed UnmanagedBuffer.Dispose() to properly return pooled buffers to their memory pool
- **Memory Tracking** - Fixed TotalAllocatedBytes calculation in UnmanagedMemoryPool to correctly track memory usage
- **CI/CD Stability** - Resolved GitHub Actions test failures caused by memory management issues

## [1.2.1] - 2025-08-29

### Added
- **Enhanced HybridAllocator** - Fully implemented hybrid allocation strategy that uses managed arrays for small allocations and unmanaged memory for large allocations
- **Improved Documentation** - Updated documentation reflecting the enhanced HybridAllocator implementation

### Changed
- **HybridAllocator** - Now actually implements the hybrid allocation strategy instead of delegating to unmanaged allocator for all cases
- **UnmanagedBuffer<T>** - Enhanced to support proper cleanup of managed arrays used by HybridAllocator
- **Performance** - Improved allocation performance for small arrays by using managed memory where appropriate

### Fixed
- **HybridAllocator Implementation** - Fixed the implementation to actually use managed arrays for small allocations as intended

## [1.2.0] - 2025-08-29

### Added
- **UnmanagedMemoryPool** - Memory pool that reduces allocation overhead by reusing previously allocated buffers
- **HybridAllocator** - Allocator that automatically chooses between managed and unmanaged allocation based on size and type
- **Performance Optimizations Documentation** - Comprehensive documentation of benchmark results and optimization strategies
- **Pooling Benchmarks** - Benchmarks comparing pooled vs non-pooled allocation performance
- **Hybrid Allocator Benchmarks** - Benchmarks demonstrating automatic allocation strategy selection

### Changed
- **UnmanagedBuffer<T>** - Modified to support returning buffers to pools when disposed
- **Documentation** - Updated README.md, DOCUMENTATION.md with information about new performance optimizations
- **Examples** - Enhanced examples to demonstrate usage of new allocators

### Fixed
- 

### Removed
- 

## [1.0.2] - 2025-01-30

### Added
- **DeferScope** - Zig-style defer pattern for automatic cleanup in reverse order
- **Extension methods** - `AllocateDeferred<T>()` for seamless buffer allocation with deferred disposal
- **Exception-safe cleanup** - Deferred actions execute even if exceptions occur during disposal
- **Comprehensive defer examples** - 5 different patterns including nested scopes and error handling
- **4 new tests** for defer functionality covering all usage scenarios

### Changed
- **Examples enhanced** - Added defer patterns to BasicUsage and AdvancedUsage demonstrations
- **Documentation updated** - README.md and DOCUMENTATION.md now include defer API reference and examples
- **Test coverage increased** - Now 36 tests total, all passing

### Fixed
- **Memory management** - Defer ensures proper cleanup order even in complex scenarios

## [1.0.1] - 2025-01-30

### Added
- Comprehensive test suite with 32 passing tests covering all functionality
- InteropTests for native API integration scenarios
- UnmanagedBufferTests for thorough buffer validation
- Memory leak detection tests for DebugMemoryAllocator

### Changed
- Updated all documentation to accurately reflect current API
- Improved README.md with clearer examples and use cases
- Enhanced GETTING_STARTED.md with working code samples
- Completely rewritten DOCUMENTATION.md with current implementation details

### Fixed
- DebugMemoryAllocator now properly tracks buffer disposal and detects leaks
- ScopedMemoryAllocator no longer causes double-free crashes
- All allocators now work correctly with UnmanagedBuffer<T> disposal
- CI/CD pipeline now passes with updated tests

### Removed
- Development artifacts (REFACTOR_PLAN.md, REFACTOR_SUMMARY.md)
- Outdated API references and unused code

## [1.0.0] - 2025-01-29

### Added
- Initial release of ZiggyAlloc
- Core memory management types: `Pointer<T>`, `Slice<T>`
- Multiple allocator implementations:
  - `ManualAllocator` - Basic malloc/free style allocation
  - `ScopedAllocator` - Automatic cleanup on dispose
  - `DebugAllocator` - Memory leak detection with caller info
- Lifetime management utilities:
  - `AutoFree<T>` - RAII-style automatic cleanup
  - `DeferScope` - Deferred action execution
- Zig-style context system with `Ctx` struct
- I/O abstractions with `IWriter` and `IReader` interfaces
- Comprehensive test suite with 15+ test cases
- Example applications demonstrating usage patterns
- Full documentation and API reference
- Cross-platform support (.NET 8.0+)
- NuGet package configuration with symbols
- Zero-cost abstractions using ref structs
- Bounds-checked slice access
- Implicit conversions to `Span<T>` and `ReadOnlySpan<T>`
- Thread-safe debug allocator with lock-based tracking
- Caller information capture for leak detection
- Support for both .NET 6+ NativeMemory and legacy Marshal APIs
- Unsafe code optimizations for performance-critical paths