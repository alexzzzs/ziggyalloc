# Changelog

All notable changes to ZiggyAlloc will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-28

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

### Features
- Zero-cost abstractions using ref structs
- Bounds-checked slice access
- Implicit conversions to `Span<T>` and `ReadOnlySpan<T>`
- Thread-safe debug allocator with lock-based tracking
- Caller information capture for leak detection
- Support for both .NET 6+ NativeMemory and legacy Marshal APIs
- Unsafe code optimizations for performance-critical paths