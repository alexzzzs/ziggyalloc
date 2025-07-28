# Publishing Commands for ZiggyAlloc

## Set API Key (Replace YOUR_API_KEY with actual key)
```bash
dotnet nuget setapikey YOUR_API_KEY_HERE --source https://api.nuget.org/v3/index.json
```

## Verify Package is Ready
```bash
# Check package exists
ls nupkg/ZiggyAlloc.1.0.0.nupkg

# Optional: Inspect package contents
dotnet nuget verify nupkg/ZiggyAlloc.1.0.0.nupkg
```

## Publish to NuGet.org
```bash
dotnet nuget push nupkg/ZiggyAlloc.1.0.0.nupkg --source https://api.nuget.org/v3/index.json
```

## After Publishing - Verification
```bash
# Wait 5-10 minutes, then test installation
mkdir TestPublicInstall
cd TestPublicInstall
dotnet new console
dotnet add package ZiggyAlloc
dotnet run
```