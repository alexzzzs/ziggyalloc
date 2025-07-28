# Release Preparation Script for ZiggyAlloc
# This script helps prepare a new release by running all necessary checks

param(
    [Parameter(Mandatory=$true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Preparing ZiggyAlloc release $Version..." -ForegroundColor Green
Write-Host ""

# Validate version format
if (-not ($Version -match '^\d+\.\d+\.\d+$')) {
    Write-Error "Version must be in format X.Y.Z (e.g., 1.1.0)"
    exit 1
}

# Check if we're on main branch
$currentBranch = git branch --show-current
if ($currentBranch -ne "main") {
    Write-Warning "You're not on the main branch (current: $currentBranch)"
    $continue = Read-Host "Continue anyway? (y/N)"
    if ($continue -ne "y" -and $continue -ne "Y") {
        Write-Host "Release preparation cancelled." -ForegroundColor Yellow
        exit 0
    }
}

# Check for uncommitted changes
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Error "You have uncommitted changes. Please commit or stash them first."
    exit 1
}

Write-Host "✓ Git status clean" -ForegroundColor Green

# Update version
Write-Host "📝 Updating version to $Version..." -ForegroundColor Cyan
& .\scripts\update-version.ps1 -Version $Version -UpdateChangelog
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to update version"
    exit 1
}

# Build and test
Write-Host "🔨 Building project..." -ForegroundColor Cyan
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit 1
}
Write-Host "✓ Build successful" -ForegroundColor Green

Write-Host "🧪 Running tests..." -ForegroundColor Cyan
dotnet test --configuration Release --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Error "Tests failed"
    exit 1
}
Write-Host "✓ All tests passed" -ForegroundColor Green

# Create package
Write-Host "📦 Creating NuGet package..." -ForegroundColor Cyan
dotnet pack --configuration Release --output ./release-artifacts
if ($LASTEXITCODE -ne 0) {
    Write-Error "Package creation failed"
    exit 1
}
Write-Host "✓ Package created successfully" -ForegroundColor Green

# Show package info
$packageFile = Get-ChildItem "./release-artifacts/ZiggyAlloc.$Version.nupkg" -ErrorAction SilentlyContinue
if ($packageFile) {
    $packageSize = [math]::Round($packageFile.Length / 1KB, 2)
    Write-Host "  Package: $($packageFile.Name) ($packageSize KB)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "🎉 Release preparation complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Review CHANGELOG.md and update it with your changes"
Write-Host "2. Commit the version changes:"
Write-Host "   git add ."
Write-Host "   git commit -m 'Bump version to $Version'"
Write-Host "3. Create and push the tag:"
Write-Host "   git tag v$Version"
Write-Host "   git push origin main --tags"
Write-Host "4. Create a GitHub release at:"
Write-Host "   https://github.com/alexzzzs/ziggyalloc/releases/new"
Write-Host "5. The GitHub Action will automatically publish to NuGet"
Write-Host ""
Write-Host "Package artifacts are in: ./release-artifacts/" -ForegroundColor Gray