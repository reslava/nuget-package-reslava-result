#!/bin/bash
# Build and Package Script for REslava.Result Multi-Package Solution
# Usage: ./build-packages.sh [version]
# Example: ./build-packages.sh 1.0.0

set -e  # Exit on error

VERSION=${1:-"1.0.0"}
OUTPUT_DIR="./nupkgs"

echo "======================================"
echo "Building REslava.Result Packages"
echo "Version: $VERSION"
echo "======================================"
echo ""

# Clean previous builds
echo "🧹 Cleaning previous builds..."
find . -type d -name "bin" -o -name "obj" | xargs rm -rf
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# Update versions
echo "📝 Updating package versions to $VERSION..."
find . -name "*.csproj" -exec sed -i.bak "s/<Version>.*<\/Version>/<Version>$VERSION<\/Version>/" {} \;
find . -name "*.csproj.bak" -delete

# Build order is important!
echo ""
echo "======================================"
echo "Step 1/3: Building Core Package"
echo "======================================"
cd SourceGenerator/Core || exit 1
dotnet clean
dotnet build -c Release
dotnet pack -c Release -o "../../$OUTPUT_DIR"
cd ../..

echo ""
echo "======================================"
echo "Step 2/3: Building SourceGenerators"
echo "======================================"
cd SourceGenerator || exit 1
dotnet clean
dotnet build -c Release
dotnet pack -c Release -o "../$OUTPUT_DIR"
cd ..

echo ""
echo "======================================"
echo "Step 3/3: Building Main Library"
echo "======================================"
cd src || exit 1
dotnet clean
dotnet build -c Release
dotnet pack -c Release -o "../$OUTPUT_DIR"
cd ..

# Verify packages
echo ""
echo "======================================"
echo "✅ Package Build Complete"
echo "======================================"
echo ""
echo "📦 Created packages:"
ls -lh "$OUTPUT_DIR"/*.nupkg

echo ""
echo "======================================"
echo "🔍 Verifying Package Structure"
echo "======================================"

# Check SourceGenerators package structure
GENERATOR_PKG="$OUTPUT_DIR/REslava.Result.SourceGenerators.$VERSION.nupkg"
if [ -f "$GENERATOR_PKG" ]; then
    echo ""
    echo "Checking: REslava.Result.SourceGenerators package..."
    
    if unzip -l "$GENERATOR_PKG" | grep -q "analyzers/dotnet/cs/REslava.Result.SourceGenerators.dll"; then
        echo "  ✅ Generator DLL in analyzers folder"
    else
        echo "  ❌ ERROR: Generator DLL not in analyzers folder!"
        exit 1
    fi
    
    if unzip -l "$GENERATOR_PKG" | grep -q "analyzers/dotnet/cs/REslava.Result.SourceGenerators.Core.dll"; then
        echo "  ✅ Core DLL in analyzers folder"
    else
        echo "  ❌ ERROR: Core DLL not in analyzers folder!"
        exit 1
    fi
    
    if unzip -l "$GENERATOR_PKG" | grep -q "build/.*\.props"; then
        echo "  ✅ Build props file included"
    else
        echo "  ⚠️  WARNING: Build props file not found"
    fi
    
    if unzip -l "$GENERATOR_PKG" | grep -q "lib/"; then
        echo "  ⚠️  WARNING: lib folder found (should be empty)"
    else
        echo "  ✅ No lib folder (correct for analyzer)"
    fi
fi

echo ""
echo "======================================"
echo "📦 Next Steps"
echo "======================================"
echo ""
echo "1. Test locally:"
echo "   mkdir ~/local-nuget"
echo "   cp $OUTPUT_DIR/*.nupkg ~/local-nuget/"
echo "   dotnet nuget add source ~/local-nuget -n LocalFeed"
echo ""
echo "2. Create test project:"
echo "   cd TestProject"
echo "   dotnet add package REslava.Result --version $VERSION --source LocalFeed"
echo "   dotnet add package REslava.Result.SourceGenerators --version $VERSION --source LocalFeed"
echo "   dotnet build"
echo ""
echo "3. Publish to NuGet.org:"
echo "   dotnet nuget push $OUTPUT_DIR/*.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json"
echo ""
echo "✅ Build script completed successfully!"
