# Changelog

All notable changes to ZiggyAlloc will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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