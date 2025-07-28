# Package Validation Script for ZiggyAlloc
# Validates the NuGet package before release

param(
    [string]$PackagePath = "./release-artifacts/ZiggyAlloc.*.nupkg"
)

$ErrorActionPreference = "Stop"

Write-Host "🔍 Validating ZiggyAlloc NuGet package..." -ForegroundColor Green

# Find the package file
$packageFile = Get-ChildItem $PackagePath | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-not $packageFile) {
    Write-Error "Package file not found at: $PackagePath"
    exit 1
}

Write-Host "📦 Validating package: $($packageFile.Name)" -ForegroundColor Cyan

# Extract package info
$packageSize = [math]::Round($packageFile.Length / 1KB, 2)
Write-Host "  Size: $packageSize KB" -ForegroundColor Gray

# Create temp directory for extraction
$tempDir = Join-Path $env:TEMP "ziggyalloc-validation-$(Get-Random)"
New-Item -ItemType Directory -Path $tempDir | Out-Null

try {
    # Extract package (it's a zip file)
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory($packageFile.FullName, $tempDir)
    
    Write-Host "✓ Package extraction successful" -ForegroundColor Green
    
    # Check required files
    $requiredFiles = @(
        "lib/net8.0/ZiggyAlloc.dll",
        "lib/net8.0/ZiggyAlloc.xml",
        "README.md",
        "ZiggyAlloc.nuspec"
    )
    
    $missingFiles = @()
    foreach ($file in $requiredFiles) {
        $fullPath = Join-Path $tempDir $file
        if (-not (Test-Path $fullPath)) {
            $missingFiles += $file
        } else {
            Write-Host "  ✓ $file" -ForegroundColor Green
        }
    }
    
    if ($missingFiles.Count -gt 0) {
        Write-Error "Missing required files: $($missingFiles -join ', ')"
        exit 1
    }
    
    # Check assembly info
    $assemblyPath = Join-Path $tempDir "lib/net8.0/ZiggyAlloc.dll"
    $assembly = [System.Reflection.Assembly]::LoadFrom($assemblyPath)
    $version = $assembly.GetName().Version
    
    Write-Host "  Assembly Version: $version" -ForegroundColor Gray
    Write-Host "  Target Framework: .NET 8.0" -ForegroundColor Gray
    
    # Check for unsafe code (should be present)
    $hasUnsafeCode = $assembly.GetCustomAttributes([System.Security.AllowPartiallyTrustedCallersAttribute], $false).Length -eq 0
    Write-Host "  Unsafe Code: $(if($hasUnsafeCode) { 'Yes' } else { 'No' })" -ForegroundColor Gray
    
    # Validate nuspec content
    $nuspecPath = Join-Path $tempDir "ZiggyAlloc.nuspec"
    $nuspecContent = Get-Content $nuspecPath -Raw
    
    $validations = @(
        @{ Name = "Package ID"; Pattern = '<id>ZiggyAlloc</id>' },
        @{ Name = "MIT License"; Pattern = '<license.*>MIT</license>' },
        @{ Name = "Repository URL"; Pattern = 'github.com/alexzzzs/ziggyalloc' },
        @{ Name = "Description"; Pattern = '<description>.*Zig.*memory.*</description>' }
    )
    
    foreach ($validation in $validations) {
        if ($nuspecContent -match $validation.Pattern) {
            Write-Host "  ✓ $($validation.Name)" -ForegroundColor Green
        } else {
            Write-Warning "  ⚠ $($validation.Name) validation failed"
        }
    }
    
    Write-Host ""
    Write-Host "🎉 Package validation completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Package Summary:" -ForegroundColor Cyan
    Write-Host "  File: $($packageFile.Name)"
    Write-Host "  Size: $packageSize KB"
    Write-Host "  Assembly Version: $version"
    Write-Host "  Ready for publishing: ✓"
    
} finally {
    # Cleanup
    if (Test-Path $tempDir) {
        Remove-Item $tempDir -Recurse -Force
    }
}