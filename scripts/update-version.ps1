# Version Update Script for ZiggyAlloc
# Usage: .\scripts\update-version.ps1 -Version "1.1.0" [-UpdateChangelog]

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [switch]$UpdateChangelog = $false
)

$ErrorActionPreference = "Stop"

# Validate version format (semantic versioning)
if (-not ($Version -match '^\d+\.\d+\.\d+$')) {
    Write-Error "Version must be in format X.Y.Z (e.g., 1.1.0)"
    exit 1
}

Write-Host "Updating ZiggyAlloc to version $Version..." -ForegroundColor Green

# Update project file
$projectFile = "ZiggyAlloc.csproj"
$content = Get-Content $projectFile -Raw

# Update version properties
$content = $content -replace '<Version>[\d\.]+</Version>', "<Version>$Version</Version>"
$content = $content -replace '<AssemblyVersion>[\d\.]+\.0</AssemblyVersion>', "<AssemblyVersion>$Version.0</AssemblyVersion>"
$content = $content -replace '<FileVersion>[\d\.]+\.0</FileVersion>', "<FileVersion>$Version.0</FileVersion>"

Set-Content $projectFile $content -NoNewline
Write-Host "✓ Updated $projectFile" -ForegroundColor Green

# Update CHANGELOG.md if requested
if ($UpdateChangelog) {
    $changelogFile = "CHANGELOG.md"
    $changelogContent = Get-Content $changelogFile -Raw
    $date = Get-Date -Format "yyyy-MM-dd"
    
    # Add new version section at the top
    $newSection = @"
## [$Version] - $date

### Added
- 

### Changed
- 

### Fixed
- 

"@
    
    # Insert after the first line (title)
    $lines = $changelogContent -split "`n"
    $newLines = @($lines[0], "", $newSection) + $lines[1..($lines.Length-1)]
    $newContent = $newLines -join "`n"
    
    Set-Content $changelogFile $newContent -NoNewline
    Write-Host "✓ Updated $changelogFile with new version section" -ForegroundColor Green
    Write-Host "  Please edit CHANGELOG.md to add your changes!" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Update CHANGELOG.md with your changes (if not done already)"
Write-Host "2. Commit your changes: git add . && git commit -m 'Bump version to $Version'"
Write-Host "3. Create a tag: git tag v$Version"
Write-Host "4. Push: git push && git push --tags"
Write-Host "5. Create a GitHub release to trigger NuGet publishing"
Write-Host ""
Write-Host "Version update complete! 🎉" -ForegroundColor Green