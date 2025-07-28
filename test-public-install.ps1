# Test script to verify ZiggyAlloc is available from NuGet.org
# Run this after 5-10 minutes

Write-Host "Testing ZiggyAlloc installation from public NuGet..." -ForegroundColor Green

# Try to install the package
try {
    Set-Location TestPublicNuGet
    dotnet add package ZiggyAlloc
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Package installed successfully!" -ForegroundColor Green
        
        # Try to run the test
        dotnet run
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "🎉 ZiggyAlloc is working from public NuGet!" -ForegroundColor Green
        } else {
            Write-Host "❌ Package installed but runtime failed" -ForegroundColor Red
        }
    } else {
        Write-Host "⏳ Package not yet available - try again in a few minutes" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Error testing installation: $_" -ForegroundColor Red
} finally {
    Set-Location ..
}

Write-Host "`nPackage URL: https://www.nuget.org/packages/ZiggyAlloc" -ForegroundColor Cyan