@echo off
setlocal enabledelayedexpansion
title ZiggyAlloc Comprehensive Benchmarks Runner

echo ZiggyAlloc Comprehensive Benchmarks Runner
echo ==========================================

REM Check if dotnet is available
C:\Users\alex3\scoop\shims\dotnet.exe --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: .NET SDK not found at expected location.
    echo Please ensure .NET SDK is installed in the expected location.
    echo You can test this by running: C:\Users\alex3\scoop\shims\dotnet.exe --version
    pause
    exit /b 1
)

REM Get .NET version for display
for /f "tokens=*" %%i in ('C:\Users\alex3\scoop\shims\dotnet.exe --version') do set DOTNET_VERSION=%%i
echo Found .NET SDK version: %DOTNET_VERSION%

REM Add dotnet to PATH for BenchmarkDotNet
set PATH=C:\Users\alex3\scoop\shims;%PATH%

REM Check if we're in the right directory
if not exist "ZiggyAlloc.Benchmarks.csproj" (
    echo Error: ZiggyAlloc.Benchmarks.csproj not found. Please run this script from the benchmarks directory.
    pause
    exit /b 1
)

REM Parse command line arguments
set MODE=all
set FILTER=
set CONFIG=Release
set PARALLEL=0
set SAVE_RESULTS=0
set OUTPUT_DIR=benchmark-results
set MAX_JOBS=4
set SKIP_BUILD=0
set VERBOSE=0

:parse_args
if "%~1"=="" goto :end_parse
if "%~1"=="-mode" (
    set MODE=%~2
    shift & shift
    goto :parse_args
)
if "%~1"=="-filter" (
    set FILTER=%~2
    shift & shift
    goto :parse_args
)
if "%~1"=="-config" (
    set CONFIG=%~2
    shift & shift
    goto :parse_args
)
if "%~1"=="-parallel" (
    set PARALLEL=1
    shift
    goto :parse_args
)
if "%~1"=="-save" (
    set SAVE_RESULTS=1
    shift
    goto :parse_args
)
if "%~1"=="-output" (
    set OUTPUT_DIR=%~2
    shift & shift
    goto :parse_args
)
if "%~1"=="-maxjobs" (
    set MAX_JOBS=%~2
    shift & shift
    goto :parse_args
)
if "%~1"=="-skipbuild" (
    set SKIP_BUILD=1
    shift
    goto :parse_args
)
if "%~1"=="-verbose" (
    set VERBOSE=1
    shift
    goto :parse_args
)
if "%~1"=="-help" (
    goto :show_help
)
shift
goto :parse_args

:end_parse
goto :main

:show_help
echo Usage: run-benchmarks.bat [options]
echo.
echo Options:
echo   -mode ^<mode^>        Benchmark mode: all, quick, experimental, specific, performance, comparison
echo   -filter ^<name^>      Filter specific benchmark class (for specific mode)
echo   -config ^<config^>    Build configuration: Debug, Release (default: Release)
echo   -parallel           Run benchmarks in parallel
echo   -save               Save results to files
echo   -output ^<dir^>       Output directory for results (default: benchmark-results)
echo   -maxjobs ^<num^>      Max parallel jobs (default: 4)
echo   -skipbuild          Skip build step
echo   -verbose            Verbose output
echo   -help               Show this help
echo.
echo Examples:
echo   run-benchmarks.bat
echo   run-benchmarks.bat -mode quick
echo   run-benchmarks.bat -mode experimental
echo   run-benchmarks.bat -mode specific -filter AllocationBenchmarks
echo   run-benchmarks.bat -parallel -save
pause
exit /b 0

:main
REM Build the project
if %SKIP_BUILD%==0 (
    echo Building benchmarks in %CONFIG% configuration...
    C:\Users\alex3\scoop\shims\dotnet.exe build -c %CONFIG%
    if !errorlevel! neq 0 (
        echo Error: Build failed.
        pause
        exit /b 1
    )
) else (
    echo Skipping build step...
)

REM Prepare arguments for dotnet run
set ARGS=-c %CONFIG% --join
if %PARALLEL%==1 (
    set ARGS=!ARGS! --parallel --maxJobs %MAX_JOBS%
)
if %SAVE_RESULTS%==1 (
    if not exist %OUTPUT_DIR% mkdir %OUTPUT_DIR%
    for /f "tokens=1-3 delims=/:. " %%a in ("%date% %time%") do set TIMESTAMP=%%a%%b%%c_%%d%%e%%f
    set RESULTS_FILE=%OUTPUT_DIR%\benchmarks_!TIMESTAMP!.md
    set ARGS=!ARGS! --exporters markdown --artifacts !OUTPUT_DIR!
)
if defined FILTER (
    set ARGS=!ARGS! --filter "*!FILTER!*"
)

REM Run benchmarks
echo Running: C:\Users\alex3\scoop\shims\dotnet.exe run !ARGS!
C:\Users\alex3\scoop\shims\dotnet.exe run !ARGS!

if !errorlevel! equ 0 (
    echo.
    echo Benchmarks completed successfully!
    if %SAVE_RESULTS%==1 (
        echo Results saved to: !RESULTS_FILE!
    )
) else (
    echo.
    echo Benchmarks failed with exit code: !errorlevel!
)

echo.
echo Usage Examples:
echo   run-benchmarks.bat
echo   run-benchmarks.bat -mode quick
echo   run-benchmarks.bat -mode experimental
echo   run-benchmarks.bat -mode specific -filter AllocationBenchmarks
echo   run-benchmarks.bat -parallel -save
pause
exit /b !errorlevel!