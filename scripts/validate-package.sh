#!/bin/bash

# Bash script to validate NuGet package contents
PACKAGE_PATH=${1:-"ZiggyAlloc.Main/bin/Release/ZiggyAlloc.1.3.0.nupkg"}

echo -e "\033[32mValidating NuGet package: $PACKAGE_PATH\033[0m"

# Check if package exists
if [ ! -f "$PACKAGE_PATH" ]; then
    echo -e "\033[31mPackage not found at $PACKAGE_PATH\033[0m"
    echo -e "\033[33mBuilding package first...\033[0m"

    # Build and pack the project
    dotnet build --configuration Release ZiggyAlloc.Main.csproj
    dotnet pack --configuration Release --output ./artifacts ZiggyAlloc.Main.csproj

    PACKAGE_PATH="artifacts/ZiggyAlloc.1.3.0.nupkg"
fi

# Extract package contents
EXTRACT_PATH="temp_package_contents"
if [ -d "$EXTRACT_PATH" ]; then
    rm -rf "$EXTRACT_PATH"
fi

echo -e "\033[33mExtracting package contents...\033[0m"
unzip -q "$PACKAGE_PATH" -d "$EXTRACT_PATH"

# Check for README.md in the root
README_PATH="$EXTRACT_PATH/README.md"
if [ -f "$README_PATH" ]; then
    echo -e "\033[32m✅ README.md found in package root\033[0m"

    if grep -q "LargeBlockAllocator" "$README_PATH"; then
        echo -e "\033[32m✅ README.md contains LargeBlockAllocator documentation\033[0m"
    else
        echo -e "\033[33m⚠️ README.md does not contain LargeBlockAllocator documentation\033[0m"
    fi

    if grep -q "SIMD Memory Operations" "$README_PATH"; then
        echo -e "\033[32m✅ README.md contains SIMD documentation\033[0m"
    else
        echo -e "\033[33m⚠️ README.md does not contain SIMD documentation\033[0m"
    fi
else
    echo -e "\033[31m❌ README.md NOT found in package root!\033[0m"
fi

# List all files in the package
echo -e "\n\033[36mPackage contents:\033[0m"
find "$EXTRACT_PATH" -type f | while read -r file; do
    relative_path="${file#$EXTRACT_PATH/}"
    echo "  $relative_path"
done

# Cleanup
rm -rf "$EXTRACT_PATH"

echo -e "\n\033[32mValidation complete!\033[0m"