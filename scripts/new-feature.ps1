# New Feature Setup Script for ZiggyAlloc
# Creates the basic structure for a new feature

param(
    [Parameter(Mandatory=$true)]
    [string]$FeatureName,
    
    [Parameter(Mandatory=$true)]
    [ValidateSet("Allocator", "Core", "Context", "Lifetime", "Utility")]
    [string]$Category
)

$ErrorActionPreference = "Stop"

# Normalize feature name
$FeatureName = $FeatureName -replace '[^a-zA-Z0-9]', ''
$ClassName = $FeatureName
$TestClassName = "${ClassName}Tests"

Write-Host "🚀 Creating new feature: $FeatureName in category: $Category" -ForegroundColor Green

# Determine paths based on category
$srcPath = switch ($Category) {
    "Allocator" { "src/Allocators" }
    "Core" { "src/Core" }
    "Context" { "src/Context" }
    "Lifetime" { "src/Lifetime" }
    "Utility" { "src" }
}

$srcFile = "$srcPath/$ClassName.cs"
$testFile = "tests/${TestClassName}.cs"

# Create source file
$sourceTemplate = @"
using System;

namespace ZiggyAlloc
{
    /// <summary>
    /// TODO: Add description for $ClassName
    /// </summary>
    public class $ClassName
    {
        /// <summary>
        /// TODO: Add constructor documentation
        /// </summary>
        public $ClassName()
        {
            // TODO: Implement constructor
        }
        
        /// <summary>
        /// TODO: Add method documentation
        /// </summary>
        public void ExampleMethod()
        {
            // TODO: Implement method
        }
    }
}
"@

# Create test file
$testTemplate = @"
using System;
using Xunit;
using ZiggyAlloc;

namespace ZiggyAlloc.Tests
{
    public class $TestClassName
    {
        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var instance = new $ClassName();
            
            // Assert
            Assert.NotNull(instance);
            // TODO: Add more assertions
        }
        
        [Fact]
        public void ExampleMethod_ShouldWorkCorrectly()
        {
            // Arrange
            var instance = new $ClassName();
            
            // Act
            instance.ExampleMethod();
            
            // Assert
            // TODO: Add assertions
        }
        
        // TODO: Add more test methods
        // - Test edge cases
        // - Test error conditions
        // - Test memory management (if applicable)
    }
}
"@

# Create directories if they don't exist
$srcDir = Split-Path $srcFile -Parent
if (-not (Test-Path $srcDir)) {
    New-Item -ItemType Directory -Path $srcDir -Force | Out-Null
}

$testDir = Split-Path $testFile -Parent
if (-not (Test-Path $testDir)) {
    New-Item -ItemType Directory -Path $testDir -Force | Out-Null
}

# Write files
Set-Content -Path $srcFile -Value $sourceTemplate -Encoding UTF8
Set-Content -Path $testFile -Value $testTemplate -Encoding UTF8

Write-Host "✓ Created source file: $srcFile" -ForegroundColor Green
Write-Host "✓ Created test file: $testFile" -ForegroundColor Green

# Test that it compiles
Write-Host "🔨 Testing compilation..." -ForegroundColor Cyan
dotnet build --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Compilation successful" -ForegroundColor Green
} else {
    Write-Warning "⚠ Compilation failed - please check the generated files"
}

Write-Host ""
Write-Host "🎉 Feature scaffold created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Implement the functionality in: $srcFile"
Write-Host "2. Add comprehensive tests in: $testFile"
Write-Host "3. Update documentation if needed"
Write-Host "4. Run tests: dotnet test"
Write-Host "5. Create a pull request when ready"
Write-Host ""
Write-Host "Happy coding! 🚀" -ForegroundColor Green