# ZiggyAlloc Interactive Benchmark Selector
# This script provides an easy way to select and run specific benchmarks

param(
    [switch]$Help,
    [switch]$List,
    [string]$Run = ""
)

# Available benchmark categories with descriptions
$BenchmarkCategories = @{
    "AllocationBenchmarks" = "Basic allocation performance tests"
    "AllocatorBenchmarks" = "Core allocator performance comparison"
    "AllocatorComparisonBenchmarks" = "Detailed allocator comparisons"
    "DataTypeBenchmarks" = "Different data type performance"
    "ExperimentalOptimizationsBenchmarks" = "Experimental optimization features"
    "HybridAllocatorBenchmarks" = "Hybrid allocator performance"
    "LargeArrayOptimizationBenchmarks" = "Large array optimization tests"
    "MultithreadingBenchmarks" = "Multi-threading performance tests"
    "PoolingBenchmarks" = "Memory pool performance tests"
    "RealWorldScenarioBenchmarks" = "Real-world usage scenarios"
    "SimdPerformanceBenchmarks" = "SIMD optimization performance"
    "SlabAllocatorBenchmarks" = "Slab allocator performance tests"
    "ThreadLocalMemoryPoolBenchmarks" = "Thread-local memory pool tests"
}

# Category groups for easier selection
$CategoryGroups = @{
    "Basic" = @("AllocationBenchmarks", "AllocatorBenchmarks", "PoolingBenchmarks")
    "Threading" = @("MultithreadingBenchmarks", "ThreadLocalMemoryPoolBenchmarks")
    "Memory" = @("PoolingBenchmarks", "SlabAllocatorBenchmarks", "HybridAllocatorBenchmarks")
    "Optimization" = @("SimdPerformanceBenchmarks", "LargeArrayOptimizationBenchmarks", "ExperimentalOptimizationsBenchmarks")
    "Comparison" = @("AllocatorComparisonBenchmarks", "DataTypeBenchmarks")
    "Advanced" = @("RealWorldScenarioBenchmarks", "HybridAllocatorBenchmarks")
}

function Show-Help {
    Write-Host "ZiggyAlloc Interactive Benchmark Selector" -ForegroundColor Green
    Write-Host "=======================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage:" -ForegroundColor Yellow
    Write-Host "  .\select-benchmarks.ps1              # Interactive selection mode" -ForegroundColor White
    Write-Host "  .\select-benchmarks.ps1 -List        # List all available benchmarks" -ForegroundColor White
    Write-Host "  .\select-benchmarks.ps1 -Run Basic   # Run specific category group" -ForegroundColor White
    Write-Host "  .\select-benchmarks.ps1 -Help        # Show this help" -ForegroundColor White
    Write-Host ""
    Write-Host "Available category groups:" -ForegroundColor Yellow
    $CategoryGroups.GetEnumerator() | Sort-Object Key | ForEach-Object {
        Write-Host "  $($_.Key.PadRight(12)) - $($_.Value -join ', ')" -ForegroundColor White
    }
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  .\select-benchmarks.ps1              # Interactive mode" -ForegroundColor White
    Write-Host "  .\select-benchmarks.ps1 -Run Threading  # Run threading benchmarks" -ForegroundColor White
    Write-Host "  .\select-benchmarks.ps1 -Run 'Basic,Memory'  # Run multiple groups" -ForegroundColor White
}

function Show-BenchmarkList {
    Write-Host "Available Benchmark Classes:" -ForegroundColor Green
    Write-Host "===========================" -ForegroundColor Green
    Write-Host ""

    $BenchmarkCategories.GetEnumerator() | Sort-Object Key | ForEach-Object {
        Write-Host "$($_.Key.PadRight(35)) - $($_.Value)" -ForegroundColor White
    }
}

function Get-SelectedBenchmarks {
    Write-Host "Select benchmark categories to run:" -ForegroundColor Yellow
    Write-Host ""

    $index = 1
    $CategoryGroups.GetEnumerator() | Sort-Object Key | ForEach-Object {
        Write-Host "$index. $($_.Key.PadRight(12)) - $($_.Value -join ', ')" -ForegroundColor White
        $index++
    }

    Write-Host ""
    Write-Host "Enter your choices:" -ForegroundColor Cyan
    Write-Host "  - Single category: Enter the number (e.g., '1')" -ForegroundColor White
    Write-Host "  - Multiple categories: Enter numbers separated by commas (e.g., '1,3,5')" -ForegroundColor White
    Write-Host "  - All categories: Enter 'all'" -ForegroundColor White
    Write-Host "  - Back to main menu: Enter 'menu'" -ForegroundColor White

    $choice = Read-Host "`nYour selection"

    if ($choice -eq "all") {
        return $BenchmarkCategories.Keys
    } elseif ($choice -eq "menu") {
        return $null
    } else {
        $selectedBenchmarks = @()
        $choiceNumbers = $choice.Split(',') | ForEach-Object { $_.Trim() }

        $categories = @($CategoryGroups.GetEnumerator() | Sort-Object Key)

        foreach ($choiceNum in $choiceNumbers) {
            if ($choiceNum -match '^\d+$' -and [int]$choiceNum -ge 1 -and [int]$choiceNum -le $categories.Count) {
                $categoryIndex = [int]$choiceNum - 1
                $selectedBenchmarks += $categories[$categoryIndex].Value
            } else {
                Write-Host "Warning: Invalid choice '$choiceNum' ignored" -ForegroundColor Yellow
            }
        }

        return $selectedBenchmarks | Select-Object -Unique
    }
}

# Main logic
if ($Help) {
    Show-Help
    exit 0
}

if ($List) {
    Show-BenchmarkList
    exit 0
}

if ($Run -ne "") {
    $selectedBenchmarks = @()
    $runCategories = $Run.Split(',') | ForEach-Object { $_.Trim() }

    foreach ($category in $runCategories) {
        if ($CategoryGroups.ContainsKey($category)) {
            $selectedBenchmarks += $CategoryGroups[$category]
        } else {
            Write-Host "Warning: Unknown category '$category' ignored" -ForegroundColor Yellow
        }
    }

    if ($selectedBenchmarks.Count -eq 0) {
        Write-Host "No valid categories specified. Use -Help for usage information." -ForegroundColor Red
        exit 1
    }
} else {
    # Interactive mode
    do {
        Write-Host "ZiggyAlloc Benchmark Selector" -ForegroundColor Green
        Write-Host "============================" -ForegroundColor Green
        Write-Host ""
        Write-Host "1. Select benchmarks to run" -ForegroundColor White
        Write-Host "2. List all available benchmarks" -ForegroundColor White
        Write-Host "3. Show help" -ForegroundColor White
        Write-Host "4. Exit" -ForegroundColor White
        Write-Host ""

        $mainChoice = Read-Host "Enter your choice (1-4)"

        switch ($mainChoice) {
            "1" {
                $selectedBenchmarks = Get-SelectedBenchmarks
                if ($selectedBenchmarks -ne $null) {
                    break
                }
            }
            "2" {
                Show-BenchmarkList
                Write-Host ""
                pause
            }
            "3" {
                Show-Help
                Write-Host ""
                pause
            }
            "4" {
                Write-Host "Goodbye!" -ForegroundColor Green
                exit 0
            }
            default {
                Write-Host "Invalid choice. Please enter 1-4." -ForegroundColor Red
                Write-Host ""
                pause
            }
        }
    } while ($null -eq $selectedBenchmarks)
}

# Remove duplicates and validate
$selectedBenchmarks = $selectedBenchmarks | Select-Object -Unique
$validBenchmarks = $selectedBenchmarks | Where-Object { $BenchmarkCategories.ContainsKey($_) }

if ($validBenchmarks.Count -eq 0) {
    Write-Host "No valid benchmarks selected." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Selected benchmarks:" -ForegroundColor Cyan
$validBenchmarks | ForEach-Object {
    Write-Host "  - $_" -ForegroundColor White
}

$confirm = Read-Host "`nRun these benchmarks? (y/N)"
if ($confirm -ne "y" -and $confirm -ne "Y") {
    Write-Host "Operation cancelled." -ForegroundColor Yellow
    exit 0
}

# Run the benchmarks
Write-Host ""
Write-Host "Running benchmarks..." -ForegroundColor Yellow

$benchmarkArgs = @("-c", "Release", "--join", "--filter", "*$($validBenchmarks -join '|')*")

Write-Host "Executing: dotnet run $benchmarkArgs" -ForegroundColor Gray
dotnet run @benchmarkArgs

exit $LASTEXITCODE