# ZiggyAlloc Test Suite

This directory contains the comprehensive test suite for the ZiggyAlloc library. Due to resource constraints and parallel execution conflicts in the test environment, specific guidelines should be followed when running tests.

## Running Tests

### Recommended Approach

Due to resource constraints and parallel execution conflicts, it's recommended to run tests with limited parallelization:

```bash
# Run all tests with limited parallelization
dotnet test tests/ZiggyAlloc.Tests.csproj -- NUnit.NumberOfTestWorkers=1

# Run specific test class
dotnet test tests/ZiggyAlloc.Tests.csproj --filter "HybridAllocatorTests" -- NUnit.NumberOfTestWorkers=1

# Run tests by category
dotnet test tests/ZiggyAlloc.Tests.csproj --filter "TestCategory!=Performance" -- NUnit.NumberOfTestWorkers=1
```

### Running Individual Test Classes

For better stability and resource management, you can run specific test classes:

```bash
# Run allocator tests
dotnet test tests/ZiggyAlloc.Tests.csproj --filter "AllocatorTests" -- NUnit.NumberOfTestWorkers=1

# Run lifetime management tests
dotnet test tests/ZiggyAlloc.Tests.csproj --filter "LifetimeTests" -- NUnit.NumberOfTestWorkers=1

# Run specific test method
dotnet test tests/ZiggyAlloc.Tests.csproj --filter "HybridAllocator_AllocatesSmallArraysUsingManagedMemory" -- NUnit.NumberOfTestWorkers=1
```

### Test Categories

Tests are organized into several categories:

- **AllocatorTests**: Core allocator functionality tests
- **LifetimeTests**: Memory lifetime and disposal tests
- **InteropTests**: Native interop and pointer operation tests
- **PerformanceOptimizationTests**: Performance-related tests
- **AdditionalTests**: Extended test scenarios

## Test Structure

The test suite is organized as follows:

```
tests/
├── AllocatorTests.cs              # Core allocator tests
├── LifetimeTests.cs               # Memory lifetime tests
├── PointerAndSliceTests.cs        # Pointer and slice operation tests
├── PerformanceOptimizationTests.cs # Performance tests
├── HybridAllocatorTests.cs        # HybridAllocator specific tests
├── ScopedMemoryAllocatorTests.cs  # ScopedMemoryAllocator tests
├── DebugMemoryAllocatorTests.cs   # DebugMemoryAllocator tests
├── UnmanagedBufferTests.cs        # UnmanagedBuffer tests
├── DeferScopeTests.cs             # DeferScope tests
├── UnmanagedMemoryPoolTests.cs    # UnmanagedMemoryPool tests
├── Additional test files...       # Extended test scenarios
└── README.md                      # This file
```

## Troubleshooting

### Test Runner Crashes

If you encounter test runner crashes when running the full test suite, this is typically due to resource constraints. Try running tests with limited parallelization:

```bash
dotnet test tests/ZiggyAlloc.Tests.csproj -- NUnit.NumberOfTestWorkers=1
```

### Resource Constraints

Some tests create large memory allocations or run concurrent operations. If you're experiencing issues:

1. Run smaller test groups at a time
2. Use limited parallelization
3. Ensure adequate system memory is available

## Continuous Integration

The GitHub Actions CI workflow runs tests with specific configurations to ensure stability:

```bash
dotnet test tests/ZiggyAlloc.Tests.csproj -- NUnit.NumberOfTestWorkers=1
```

This approach ensures that tests run reliably in both development and CI environments.