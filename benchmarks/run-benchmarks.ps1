# ZiggyAlloc Benchmarks Runner
# This script runs the ZiggyAlloc benchmarks

Write-Host "ZiggyAlloc Benchmarks Runner" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green

# Check if dotnet is available
try {
    $dotnetVersion = dotnet --version
    Write-Host "Found .NET SDK version: $dotnetVersion" -ForegroundColor Cyan
} catch {
    Write-Host "Error: .NET SDK not found. Please install .NET SDK to run benchmarks." -ForegroundColor Red
    exit 1
}

# Check if we're in the right directory
if (-not (Test-Path "ZiggyAlloc.Benchmarks.csproj")) {
    Write-Host "Error: ZiggyAlloc.Benchmarks.csproj not found. Please run this script from the benchmarks directory." -ForegroundColor Red
    exit 1
}

# Build the project
Write-Host "Building benchmarks..." -ForegroundColor Yellow
dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Build failed." -ForegroundColor Red
    exit 1
}

# Run benchmarks
Write-Host "Running benchmarks..." -ForegroundColor Yellow
dotnet run -c Release

Write-Host "Benchmarks completed!" -ForegroundColor Green