# ZiggyAlloc Development Guide

This guide covers the development workflow for adding features and releasing new versions of ZiggyAlloc.

## Quick Release Process

For a new release, use the automated script:

```powershell
# Prepare a new release (e.g., version 1.1.0)
.\scripts\prepare-release.ps1 -Version "1.1.0"

# Follow the instructions printed by the script
```

## Development Workflow

### Adding New Features

1. **Create a feature branch**:
   ```bash
   git checkout -b feature/new-allocator-type
   ```

2. **Implement your feature**:
   - Add code in appropriate `src/` subdirectories
   - Follow existing code patterns and naming conventions
   - Add comprehensive tests in `tests/`
   - Update examples if needed

3. **Test thoroughly**:
   ```bash
   dotnet test
   dotnet run --project examples/
   ```

4. **Update documentation**:
   - Add XML documentation for public APIs
   - Update README.md if needed
   - Add usage examples

5. **Create pull request**:
   - Use the PR template
   - Ensure CI passes
   - Get code review

### Version Management

ZiggyAlloc follows [Semantic Versioning](https://semver.org/):

- **MAJOR** (X.0.0): Breaking changes
- **MINOR** (1.X.0): New features (backward compatible)
- **PATCH** (1.1.X): Bug fixes (backward compatible)

### Manual Version Update

If you need to update the version manually:

```powershell
# Update version only
.\scripts\update-version.ps1 -Version "1.1.0"

# Update version and add changelog section
.\scripts\update-version.ps1 -Version "1.1.0" -UpdateChangelog
```

## Release Process

### Automated Release (Recommended)

1. **Prepare the release**:
   ```powershell
   .\scripts\prepare-release.ps1 -Version "1.1.0"
   ```

2. **Update CHANGELOG.md** with your changes

3. **Commit and tag**:
   ```bash
   git add .
   git commit -m "Bump version to 1.1.0"
   git tag v1.1.0
   git push origin main --tags
   ```

4. **Create GitHub Release**:
   - Go to https://github.com/alexzzzs/ziggyalloc/releases/new
   - Select the tag you just created
   - Add release notes (copy from CHANGELOG.md)
   - Publish the release

5. **Automatic NuGet Publishing**:
   - GitHub Actions will automatically build and publish to NuGet
   - Monitor the Actions tab for progress

### Manual Release (If Needed)

```bash
# Build and pack
dotnet build --configuration Release
dotnet pack --configuration Release --output ./artifacts

# Publish to NuGet (requires API key)
dotnet nuget push ./artifacts/*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## Testing Strategy

### Unit Tests
- All new features must have comprehensive tests
- Aim for high code coverage
- Test both success and failure scenarios
- Include memory leak detection tests

### Integration Tests
- Test real-world usage scenarios
- Verify cross-platform compatibility
- Performance regression testing

### Manual Testing
```bash
# Run all tests
dotnet test

# Test examples
dotnet run --project examples/

# Test package installation
dotnet pack --output ./test-pkg
dotnet add TestProject package ZiggyAlloc --source ./test-pkg
```

## Project Structure

```
ZiggyAlloc/
├── src/                    # Core library source
│   ├── Allocators/         # Memory allocator implementations
│   ├── Context/            # Context system and I/O
│   ├── Core/               # Pointer and Slice types
│   ├── Lifetime/           # RAII and defer scope
│   └── Z.cs                # Static helpers
├── tests/                  # Unit tests
├── examples/               # Usage examples
├── scripts/                # Development scripts
├── .github/                # GitHub workflows and templates
└── docs/                   # Additional documentation
```

## Development Scripts

### Version Management
- `scripts/update-version.ps1` - Update version numbers
- `scripts/prepare-release.ps1` - Full release preparation

### Future Scripts (Ideas)
- `scripts/benchmark.ps1` - Performance benchmarking
- `scripts/generate-docs.ps1` - Documentation generation
- `scripts/check-api-changes.ps1` - API compatibility checking

## Performance Considerations

When adding new features:

1. **Benchmark critical paths** - Use BenchmarkDotNet for accurate measurements
2. **Memory allocation patterns** - Minimize GC pressure
3. **Unsafe code optimization** - Use aggressive inlining where appropriate
4. **Cross-platform testing** - Verify performance on different platforms

## Debugging Tips

### Memory Issues
- Always test with `DebugAllocator` during development
- Use memory profilers (dotMemory, PerfView)
- Test with different allocation patterns

### Performance Issues
- Profile with dotTrace or Visual Studio Profiler
- Check for boxing/unboxing in hot paths
- Verify inlining is working as expected

## Contributing Guidelines

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed contribution guidelines.

## Documentation Standards

- All public APIs must have XML documentation
- Include usage examples in documentation
- Update README.md for significant features
- Maintain CHANGELOG.md for all releases

## CI/CD Pipeline

The GitHub Actions workflow automatically:

1. **On Pull Request**:
   - Builds the project
   - Runs all tests
   - Checks code formatting

2. **On Release**:
   - Builds with release configuration
   - Runs full test suite
   - Creates NuGet package
   - Publishes to NuGet.org
   - Uploads artifacts to GitHub

