# ZiggyAlloc v{VERSION}

## 🎉 What's New

<!-- Copy the relevant section from CHANGELOG.md -->

### ✨ Added
- 

### 🔄 Changed
- 

### 🐛 Fixed
- 

### 🗑️ Removed
- 

## 📦 Installation

```bash
dotnet add package ZiggyAlloc --version {VERSION}
```

## 📊 Package Information

- **Package Size**: ~XX KB
- **Target Framework**: .NET 8.0+
- **Dependencies**: None (self-contained)
- **Symbols**: Available for debugging

## 🔗 Links

- **NuGet Package**: https://www.nuget.org/packages/ZiggyAlloc/{VERSION}
- **Documentation**: [README.md](README.md)
- **Getting Started**: [GETTING_STARTED.md](GETTING_STARTED.md)
- **Contributing**: [CONTRIBUTING.md](CONTRIBUTING.md)

## 🚀 Usage Example

```csharp
using ZiggyAlloc;

using var allocator = new DebugAllocator("MyApp", Z.DefaultAllocator);
var ctx = new Ctx(allocator, Z.ctx.@out, Z.ctx.@in);

using var defer = DeferScope.Start();
var data = ctx.AllocSlice<int>(defer, 10, zeroed: true);

// Use your data...
```

## 🙏 Contributors

Thanks to all contributors who made this release possible!

## 📋 Full Changelog

**Full Changelog**: https://github.com/alexzzzs/ziggyalloc/compare/v{PREVIOUS_VERSION}...v{VERSION}