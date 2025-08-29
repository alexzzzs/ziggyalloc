@echo off
title ZiggyAlloc Benchmarks Runner

echo ZiggyAlloc Benchmarks Runner
echo ============================

REM Check if dotnet is available
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: .NET SDK not found. Please install .NET SDK to run benchmarks.
    pause
    exit /b 1
)

REM Get .NET version for display
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo Found .NET SDK version: %DOTNET_VERSION%

REM Check if we're in the right directory
if not exist "ZiggyAlloc.Benchmarks.csproj" (
    echo Error: ZiggyAlloc.Benchmarks.csproj not found. Please run this script from the benchmarks directory.
    pause
    exit /b 1
)

REM Build the project
echo Building benchmarks...
dotnet build -c Release
if %errorlevel% neq 0 (
    echo Error: Build failed.
    pause
    exit /b 1
)

REM Run benchmarks
echo Running benchmarks...
dotnet run -c Release

echo.
echo Benchmarks completed!
pause