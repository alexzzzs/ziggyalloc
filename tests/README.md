# ZiggyAlloc Tests

This directory contains unit tests for the ZiggyAlloc memory management library.

## 🧪 Test Organization

Tests are organized by component:

- `AllocatorTests.cs` - Core allocator functionality tests
- `UnmanagedBufferTests.cs` - UnmanagedBuffer functionality tests
- `ScopedMemoryAllocatorTests.cs` - Scoped allocator specific tests
- `DebugMemoryAllocatorTests.cs` - Debug allocator specific tests
- `UnmanagedMemoryPoolTests.cs` - Memory pool specific tests
- `HybridAllocatorTests.cs` - Hybrid allocator specific tests
- `SlabAllocatorTests.cs` - Slab allocator specific tests (new)
- `DeferScopeTests.cs` - Defer scope functionality tests
- `LifetimeTests.cs` - Lifetime management tests

## 🚀 Running Tests

### Prerequisites

- .NET 8.0 SDK or later
- All tests require limited parallelization due to resource constraints

### Run All Tests

```bash
dotnet test
```

### Run Tests with Limited Parallelization

Due to resource constraints, tests should be run with limited parallelization:

```bash
dotnet test -- NUnit.NumberOfTestWorkers=1
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~ZiggyAlloc.Tests.SlabAllocatorTests"
```

### Run Specific Test Method

```bash
dotnet test --filter "Name=SlabAllocator_BasicAllocation_Works"
```

## 🧠 Test Guidelines

### Parallelization

Tests require limited parallelization using `--NUnit.NumberOfTestWorkers=1` to avoid resource overload and test host crashes. While Task.WaitAll is commonly used for concurrent operations, it may lead to deadlocks in parallel execution. Tests should be improved by using async/await patterns instead of blocking calls to ensure better concurrency handling and avoid potential deadlocks.

### Namespace Imports

Test files require proper namespace imports including `using System.Linq;` to support LINQ operations and parallel test execution patterns.

### Test Structure

All tests follow the Arrange-Act-Assert pattern:

```csharp
[Fact]
public void TestName()
{
    // Arrange
    // Set up test data and preconditions
    
    // Act
    // Execute the code under test
    
    // Assert
    // Verify the expected outcomes
}
```

## 🐛 Troubleshooting

### Test Host Crashes

If you experience test host crashes, try running with limited parallelization:

```bash
dotnet test -- NUnit.NumberOfTestWorkers=1
```

### Missing Dependencies

Ensure all dependencies are restored:

```bash
dotnet restore
```

### Compilation Errors

Check for missing namespace imports or incorrect references.

## 🔄 CI/CD Integration

Tests are automatically run in the GitHub Actions CI/CD pipeline. The pipeline uses:

```bash
dotnet test -- NUnit.NumberOfTestWorkers=1
```

To ensure stable test execution.

## 📊 Test Coverage

Current test coverage includes:

1. **Functional Tests** - All allocator types and core functionality
2. **Edge Case Tests** - Boundary conditions and error handling
3. **Performance Tests** - Allocation performance and memory usage
4. **Thread Safety Tests** - Concurrent access scenarios
5. **Memory Leak Tests** - Proper cleanup and disposal

## 🎯 Best Practices

1. **Isolation** - Each test should be independent and not rely on shared state
2. **Speed** - Tests should execute quickly to enable rapid development cycles
3. **Clarity** - Test names should clearly describe what is being tested
4. **Coverage** - Tests should cover both happy paths and error conditions
5. **Maintenance** - Tests should be easy to update when implementation changes