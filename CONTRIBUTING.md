# Contributing to ZiggyAlloc

Thank you for your interest in contributing to ZiggyAlloc! We welcome contributions from the community.

## 🚀 Quick Start

1. **Fork** the repository on GitHub
2. **Clone** your fork locally
3. **Create** a feature branch
4. **Make** your changes
5. **Test** your changes
6. **Submit** a pull request

## 📋 Ways to Contribute

### 🐛 Bug Reports
- Use GitHub Issues to report bugs
- Include minimal reproduction steps
- Specify your .NET version and platform
- Check if the issue already exists

### 💡 Feature Requests
- Open a GitHub Issue with the "enhancement" label
- Describe the use case and expected behavior
- Discuss the design before implementing

### 🔧 Code Contributions
- Fix bugs or implement new features
- Follow the coding standards below
- Add tests for new functionality
- Update documentation as needed

### 📚 Documentation
- Improve README, examples, or API docs
- Fix typos or unclear explanations
- Add usage examples or tutorials

## 🛠️ Development Setup

### Prerequisites
- .NET 8.0 SDK or later
- Git
- Your favorite IDE (Visual Studio, VS Code, Rider, etc.)

### Getting Started
```bash
# Fork the repo on GitHub, then:
git clone https://github.com/YOUR_USERNAME/ziggyalloc.git
cd ziggyalloc

# Build the project
dotnet build

# Run tests
dotnet test

# Run examples
dotnet run --project examples/
```

## 🧪 Testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run with coverage (if you have tools installed)
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "ClassName=AllocatorTests"
```

### Adding Tests
- Add tests for new features in the `tests/` directory
- Follow existing test patterns
- Test both success and failure cases
- Include memory leak detection tests for allocators

## 📝 Coding Standards

### General Guidelines
- Follow C# naming conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and small

### Memory Management
- Always test with `DebugAllocator` during development
- Ensure proper cleanup in `Dispose` methods
- Use `unsafe` blocks only when necessary
- Document any unsafe operations

### Performance
- Prefer `ref struct` for zero-cost abstractions
- Use `[MethodImpl(MethodImplOptions.AggressiveInlining)]` for hot paths
- Avoid allocations in performance-critical code
- Benchmark significant changes

## 🔄 Pull Request Process

### Before Submitting
1. **Create an issue** first for significant changes
2. **Fork and branch** from `main`
3. **Write tests** for your changes
4. **Update documentation** if needed
5. **Run all tests** and ensure they pass

### PR Requirements
- Clear description of what the PR does
- Reference any related issues
- Include tests for new functionality
- Update CHANGELOG.md for user-facing changes
- Ensure CI passes

### Review Process
1. Maintainers will review your PR
2. Address any feedback or requested changes
3. Once approved, maintainers will merge
4. Your contribution will be in the next release!

## 🏷️ Issue Labels

- `bug` - Something isn't working
- `enhancement` - New feature or improvement
- `documentation` - Documentation improvements
- `good first issue` - Good for newcomers
- `help wanted` - Extra attention needed
- `performance` - Performance-related changes
- `breaking change` - Would break existing code

## 📊 Release Process

### Versioning
We follow [Semantic Versioning](https://semver.org/):
- `MAJOR.MINOR.PATCH`
- Major: Breaking changes
- Minor: New features (backward compatible)
- Patch: Bug fixes (backward compatible)

### Release Workflow
1. Update version in `ZiggyAlloc.csproj`
2. Update `CHANGELOG.md`
3. Create GitHub release with tag
4. CI automatically publishes to NuGet.org

## 🤝 Code of Conduct

### Our Standards
- Be respectful and inclusive
- Focus on constructive feedback
- Help others learn and grow
- Assume good intentions

### Unacceptable Behavior
- Harassment or discrimination
- Trolling or inflammatory comments
- Personal attacks
- Publishing private information

## 💬 Getting Help

- **GitHub Issues**: For bugs and feature requests
- **GitHub Discussions**: For questions and general discussion
- **Code Review**: Ask questions in PR comments

## 🎉 Recognition

Contributors will be:
- Listed in the project README
- Mentioned in release notes
- Given credit in the NuGet package metadata

Thank you for helping make ZiggyAlloc better! 🚀