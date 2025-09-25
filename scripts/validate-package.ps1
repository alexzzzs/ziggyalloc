# PowerShell script to validate NuGet package contents
param(
    [string]$PackagePath = "ZiggyAlloc.Main/bin/Release/ZiggyAlloc.1.3.0.nupkg"
)

Write-Host "Validating NuGet package: $PackagePath" -ForegroundColor Green

# Check if package exists
if (-not (Test-Path $PackagePath)) {
    Write-Host "Package not found at $PackagePath" -ForegroundColor Red
    Write-Host "Building package first..." -ForegroundColor Yellow

    # Build and pack the project
    dotnet build --configuration Release ZiggyAlloc.Main.csproj
    dotnet pack --configuration Release --output ./artifacts ZiggyAlloc.Main.csproj

    $PackagePath = "artifacts/ZiggyAlloc.1.3.0.nupkg"
}

# Extract package contents
$extractPath = "temp_package_contents"
if (Test-Path $extractPath) {
    Remove-Item -Recurse -Force $extractPath
}

Write-Host "Extracting package contents..." -ForegroundColor Yellow
# .nupkg files are zip files, so we need to copy and rename temporarily
$tempZipPath = $PackagePath + ".zip"
Copy-Item -Path $PackagePath -Destination $tempZipPath
Expand-Archive -Path $tempZipPath -DestinationPath $extractPath
Remove-Item -Path $tempZipPath

# Check for README.md in the root
$readmePath = Join-Path $extractPath "README.md"
if (Test-Path $readmePath) {
    Write-Host "✅ README.md found in package root" -ForegroundColor Green

    $readmeContent = Get-Content $readmePath -Raw
    if ($readmeContent -like "*LargeBlockAllocator*") {
        Write-Host "✅ README.md contains LargeBlockAllocator documentation" -ForegroundColor Green
    } else {
        Write-Host "⚠️ README.md does not contain LargeBlockAllocator documentation" -ForegroundColor Yellow
    }

    if ($readmeContent -like "*SIMD Memory Operations*") {
        Write-Host "✅ README.md contains SIMD documentation" -ForegroundColor Green
    } else {
        Write-Host "⚠️ README.md does not contain SIMD documentation" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ README.md NOT found in package root!" -ForegroundColor Red
}

# List all files in the package
Write-Host "`nPackage contents:" -ForegroundColor Cyan
Get-ChildItem -Recurse $extractPath | ForEach-Object {
    $relativePath = $_.FullName.Replace($extractPath, "").TrimStart("\")
    Write-Host "  $relativePath"
}

# Cleanup
Remove-Item -Recurse -Force $extractPath

Write-Host "`nValidation complete!" -ForegroundColor Green