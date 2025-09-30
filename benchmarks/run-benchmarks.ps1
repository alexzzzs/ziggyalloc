# ZiggyAlloc Comprehensive Benchmarks Runner
# This script runs all ZiggyAlloc benchmarks with various configurations

param(
    [string]$Mode = "all",           # all, quick, specific, experimental
    [string]$Filter = "",            # Filter specific benchmark classes
    [string]$Configuration = "Release", # Debug, Release
    [switch]$Parallel,               # Run benchmarks in parallel
    [switch]$SaveResults,            # Save results to files
    [string]$OutputDir = "benchmark-results",
    [int]$MaxJobs = 4,               # Max parallel jobs
    [switch]$SkipBuild,              # Skip build step
    [switch]$Verbose                 # Verbose output
)

# Configuration
$ScriptName = "ZiggyAlloc Benchmarks Runner"
$BenchmarkProject = "ZiggyAlloc.Benchmarks.csproj"
$StartTime = Get-Date

# Available benchmark classes
$AllBenchmarks = @(
    "AllocationBenchmarks",
    "AllocatorBenchmarks",
    "AllocatorComparisonBenchmarks",
    "DataTypeBenchmarks",
    "ExperimentalOptimizationsBenchmarks",
    "HybridAllocatorBenchmarks",
    "LargeArrayOptimizationBenchmarks",
    "MultithreadingBenchmarks",
    "PoolingBenchmarks",
    "RealWorldScenarioBenchmarks",
    "SimdPerformanceBenchmarks",
    "SlabAllocatorBenchmarks",
    "ThreadLocalMemoryPoolBenchmarks"
)

# Benchmark configurations for different modes
$BenchmarkModes = @{
    "all" = $AllBenchmarks
    "quick" = @("AllocationBenchmarks", "AllocatorBenchmarks", "PoolingBenchmarks")
    "experimental" = @("ExperimentalOptimizationsBenchmarks")
    "specific" = @($Filter)
    "performance" = @("AllocationBenchmarks", "MultithreadingBenchmarks", "PoolingBenchmarks")
    "comparison" = @("AllocatorComparisonBenchmarks", "DataTypeBenchmarks")
    "threading" = @("MultithreadingBenchmarks", "ThreadLocalMemoryPoolBenchmarks")
    "memory" = @("PoolingBenchmarks", "SlabAllocatorBenchmarks", "HybridAllocatorBenchmarks")
    "optimization" = @("SimdPerformanceBenchmarks", "LargeArrayOptimizationBenchmarks")
    "allocators" = @("AllocatorBenchmarks", "AllocatorComparisonBenchmarks", "HybridAllocatorBenchmarks", "SlabAllocatorBenchmarks")
    "interactive" = @()  # Special mode for interactive selection
}

# Display header
Write-Host "$ScriptName" -ForegroundColor Green
Write-Host "=".PadRight($ScriptName.Length, "=") -ForegroundColor Green
Write-Host ""

# Check if dotnet is available
try {
    $dotnetVersion = & dotnet --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Found .NET SDK version: $dotnetVersion" -ForegroundColor Cyan
    } else {
        throw "dotnet command failed"
    }
} catch {
    Write-Host "Error: .NET SDK not found or not in PATH." -ForegroundColor Red
    Write-Host "Please ensure .NET SDK is installed and available in your PATH." -ForegroundColor Red
    Write-Host "You can test this by running: dotnet --version" -ForegroundColor Yellow
    exit 1
}

# Check if we're in the right directory
if (-not (Test-Path $BenchmarkProject)) {
    Write-Host "Error: $BenchmarkProject not found. Please run this script from the benchmarks directory." -ForegroundColor Red
    exit 1
}

# Validate mode
if (-not $BenchmarkModes.ContainsKey($Mode)) {
    Write-Host "Error: Invalid mode '$Mode'. Available modes: $($BenchmarkModes.Keys -join ', ')" -ForegroundColor Red
    exit 1
}

# Interactive mode implementation
if ($Mode -eq "interactive") {
    Write-Host "Interactive Benchmark Selection Mode" -ForegroundColor Cyan
    Write-Host "===================================" -ForegroundColor Cyan
    Write-Host ""

    # Display available categories
    Write-Host "Available benchmark categories:" -ForegroundColor Yellow
    $categories = @{
        "1" = @("AllocationBenchmarks", "AllocatorBenchmarks")
        "2" = @("MultithreadingBenchmarks", "ThreadLocalMemoryPoolBenchmarks")
        "3" = @("PoolingBenchmarks", "SlabAllocatorBenchmarks", "HybridAllocatorBenchmarks")
        "4" = @("SimdPerformanceBenchmarks", "LargeArrayOptimizationBenchmarks")
        "5" = @("AllocatorComparisonBenchmarks", "DataTypeBenchmarks")
        "6" = @("ExperimentalOptimizationsBenchmarks", "RealWorldScenarioBenchmarks")
    }

    for ($i = 1; $i -le 6; $i++) {
        $categoryBenchmarks = $categories[$i.ToString()]
        Write-Host "  $i. $($categoryBenchmarks -join ', ')" -ForegroundColor White
    }

    Write-Host ""
    $selectedCategories = Read-Host "Enter category numbers to run (e.g., '1,3,5' for multiple categories, or 'all' for all benchmarks)"

    if ($selectedCategories -eq "all") {
        $BenchmarksToRun = $AllBenchmarks
    } else {
        $BenchmarksToRun = @()
        $categoryNumbers = $selectedCategories.Split(',') | ForEach-Object { $_.Trim() }

        foreach ($categoryNum in $categoryNumbers) {
            if ($categories.ContainsKey($categoryNum)) {
                $BenchmarksToRun += $categories[$categoryNum]
            } else {
                Write-Host "Warning: Invalid category number '$categoryNum' ignored" -ForegroundColor Yellow
            }
        }

        if ($BenchmarksToRun.Count -eq 0) {
            Write-Host "No valid categories selected. Exiting." -ForegroundColor Red
            exit 1
        }
    }
} else {
    # Get benchmarks to run
    $BenchmarksToRun = $BenchmarkModes[$Mode]
    if ($Mode -eq "specific" -and $Filter -eq "") {
        Write-Host "Error: Filter parameter required when using 'specific' mode" -ForegroundColor Red
        exit 1
    }
}

# Create output directory if saving results
if ($SaveResults) {
    if (-not (Test-Path $OutputDir)) {
        New-Item -ItemType Directory -Path $OutputDir | Out-Null
    }
    Write-Host "Results will be saved to: $OutputDir" -ForegroundColor Cyan
}

# Build the project
if (-not $SkipBuild) {
    Write-Host "Building benchmarks in $Configuration configuration..." -ForegroundColor Yellow
    $buildStart = Get-Date
    dotnet build -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: Build failed." -ForegroundColor Red
        exit 1
    }
    $buildTime = (Get-Date) - $buildStart
    Write-Host "Build completed in $($buildTime.TotalSeconds.ToString("F2"))s" -ForegroundColor Green
} else {
    Write-Host "Skipping build step..." -ForegroundColor Yellow
}

# Display run configuration
Write-Host ""
Write-Host "Run Configuration:" -ForegroundColor Cyan
Write-Host "  Mode: $Mode" -ForegroundColor White
Write-Host "  Configuration: $Configuration" -ForegroundColor White
Write-Host "  Benchmarks: $($BenchmarksToRun.Count) classes" -ForegroundColor White
if ($Parallel) {
    Write-Host "  Parallel: Yes" -ForegroundColor White
} else {
    Write-Host "  Parallel: No" -ForegroundColor White
}
if ($SaveResults) {
    Write-Host "  Save Results: Yes" -ForegroundColor White
} else {
    Write-Host "  Save Results: No" -ForegroundColor White
}
if ($Verbose) {
    Write-Host "  Selected Benchmarks:" -ForegroundColor White
    $BenchmarksToRun | ForEach-Object { Write-Host "    - $_" -ForegroundColor Gray }
}

# Run benchmarks
Write-Host ""
Write-Host "Running benchmarks..." -ForegroundColor Yellow
$runStart = Get-Date

$benchmarkArgs = @("-c", $Configuration, "--join")

if ($Parallel) {
    $benchmarkArgs += "--parallel"
    $benchmarkArgs += "--maxJobs", $MaxJobs
}

if ($SaveResults) {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $resultsFile = Join-Path $OutputDir "benchmarks_$timestamp.md"
    $benchmarkArgs += "--exporters", "markdown"
    $benchmarkArgs += "--outPath", $resultsFile
}

# Add filter if specified
if ($Filter -ne "") {
    $benchmarkArgs += "--filter", "*$Filter*"
}

# Run benchmarks
if ($BenchmarksToRun.Count -eq 1) {
    # Single benchmark class
    $benchmarkArgs += "--filter", "*$($BenchmarksToRun[0])*"
    Write-Host "Running: dotnet run $benchmarkArgs" -ForegroundColor Gray
    dotnet run @benchmarkArgs
} else {
    # Multiple benchmark classes - run all
    Write-Host "Running: dotnet run $benchmarkArgs" -ForegroundColor Gray
    dotnet run @benchmarkArgs
}

$runTime = (Get-Date) - $runStart
$exitCode = $LASTEXITCODE

# Display results
Write-Host ""
if ($exitCode -eq 0) {
    Write-Host "Benchmarks completed successfully!" -ForegroundColor Green
    Write-Host "Total execution time: $($runTime.TotalMinutes.ToString("F2")) minutes" -ForegroundColor Cyan

    if ($SaveResults) {
        Write-Host "Results saved to: $resultsFile" -ForegroundColor Green
    }
} else {
    Write-Host "Benchmarks failed with exit code: $exitCode" -ForegroundColor Red
}

$endTime = Get-Date
$totalTime = $endTime - $StartTime
Write-Host ""
Write-Host "Total script execution time: $($totalTime.TotalMinutes.ToString("F2")) minutes" -ForegroundColor Cyan

# Display usage examples
Write-Host ""
Write-Host "Usage Examples:" -ForegroundColor Magenta
Write-Host "  .\run-benchmarks.ps1                           # Run all benchmarks" -ForegroundColor White
Write-Host "  .\run-benchmarks.ps1 -Mode quick               # Run quick benchmarks only" -ForegroundColor White
Write-Host "  .\run-benchmarks.ps1 -Mode experimental        # Run experimental benchmarks" -ForegroundColor White
Write-Host "  .\run-benchmarks.ps1 -Mode threading           # Run threading-related benchmarks" -ForegroundColor White
Write-Host "  .\run-benchmarks.ps1 -Mode memory              # Run memory-related benchmarks" -ForegroundColor White
Write-Host "  .\run-benchmarks.ps1 -Mode interactive         # Interactive benchmark selection" -ForegroundColor White
Write-Host "  .\run-benchmarks.ps1 -Mode specific -Filter AllocationBenchmarks" -ForegroundColor White
Write-Host "  .\run-benchmarks.ps1 -Parallel -SaveResults   # Run in parallel and save results" -ForegroundColor White
Write-Host "  .\run-benchmarks.ps1 -Verbose -Configuration Release" -ForegroundColor White

exit $exitCode