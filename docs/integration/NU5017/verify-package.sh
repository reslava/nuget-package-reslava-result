#!/bin/bash
# Package Structure Verifier - Checks for NU5017 issues
# Usage: ./verify-package.sh [package-file.nupkg]

PACKAGE="${1:-REslava.Result.SourceGenerators.*.nupkg}"

if [ ! -f "$PACKAGE" ]; then
    echo "❌ Package not found: $PACKAGE"
    echo "Usage: $0 <package-file.nupkg>"
    exit 1
fi

echo "========================================="
echo "📦 Package Structure Verifier"
echo "========================================="
echo "Package: $PACKAGE"
echo ""

# Initialize counters
ERRORS=0
WARNINGS=0

# Check 1: lib/ folder (MUST NOT EXIST)
echo "🔍 Check 1: lib/ folder (should NOT exist)"
LIB_FILES=$(unzip -l "$PACKAGE" 2>/dev/null | grep "lib/" | wc -l)
if [ $LIB_FILES -gt 0 ]; then
    echo "  ❌ CRITICAL ERROR: lib/ folder found!"
    echo "     This will cause NU5017 error"
    echo "     Files found:"
    unzip -l "$PACKAGE" 2>/dev/null | grep "lib/" | sed 's/^/     /'
    echo ""
    echo "     FIX: Add <IncludeBuildOutput>false</IncludeBuildOutput>"
    ERRORS=$((ERRORS + 1))
else
    echo "  ✅ PASS: No lib/ folder"
fi
echo ""

# Check 2: analyzers/ folder (MUST EXIST with DLLs)
echo "🔍 Check 2: analyzers/dotnet/cs/*.dll (must exist)"
ANALYZER_DLLS=$(unzip -l "$PACKAGE" 2>/dev/null | grep "analyzers/dotnet/cs/.*\.dll" | wc -l)
if [ $ANALYZER_DLLS -lt 1 ]; then
    echo "  ❌ CRITICAL ERROR: No DLLs in analyzers/dotnet/cs/"
    echo "     Generator will not load"
    echo ""
    echo "     FIX: Add <None Include=... PackagePath=\"analyzers/dotnet/cs\" />"
    ERRORS=$((ERRORS + 1))
else
    echo "  ✅ PASS: Found $ANALYZER_DLLS DLL(s) in analyzers folder"
    unzip -l "$PACKAGE" 2>/dev/null | grep "analyzers/dotnet/cs/.*\.dll" | awk '{print $4}' | sed 's/^/     - /'
fi
echo ""

# Check 3: Core DLL (should be in analyzers/)
echo "🔍 Check 3: Core.dll in analyzers/ folder"
CORE_DLL=$(unzip -l "$PACKAGE" 2>/dev/null | grep "analyzers/.*Core\.dll" | wc -l)
if [ $CORE_DLL -lt 1 ]; then
    echo "  ⚠️  WARNING: Core.dll not found in analyzers/"
    echo "     Generator may fail at runtime"
    echo ""
    echo "     FIX: Include Core DLL in analyzers path"
    WARNINGS=$((WARNINGS + 1))
else
    echo "  ✅ PASS: Core.dll included"
fi
echo ""

# Check 4: Build props files
echo "🔍 Check 4: Build .props files"
PROPS_COUNT=$(unzip -l "$PACKAGE" 2>/dev/null | grep -E "build.*\.props|buildTransitive.*\.props" | wc -l)
if [ $PROPS_COUNT -lt 1 ]; then
    echo "  ⚠️  WARNING: No .props files found"
    echo "     IDE integration may not work"
    echo ""
    echo "     Recommended: Add build/ and buildTransitive/ .props files"
    WARNINGS=$((WARNINGS + 1))
else
    echo "  ✅ PASS: Found $PROPS_COUNT .props file(s)"
    unzip -l "$PACKAGE" 2>/dev/null | grep -E "build.*\.props|buildTransitive.*\.props" | awk '{print $4}' | sed 's/^/     - /'
fi
echo ""

# Check 5: Package type (should be analyzer only)
echo "🔍 Check 5: Package type verification"
if [ $LIB_FILES -gt 0 ] && [ $ANALYZER_DLLS -gt 0 ]; then
    echo "  ❌ CRITICAL ERROR: Package has BOTH lib/ and analyzers/"
    echo "     This is EXACTLY the NU5017 error!"
    echo "     Package type: Dependency + Analyzer (INVALID)"
    ERRORS=$((ERRORS + 1))
elif [ $LIB_FILES -gt 0 ]; then
    echo "  ❌ ERROR: Package type: Dependency only"
    echo "     Should be: Analyzer"
    ERRORS=$((ERRORS + 1))
elif [ $ANALYZER_DLLS -gt 0 ]; then
    echo "  ✅ PASS: Package type: Analyzer only (CORRECT)"
else
    echo "  ❌ ERROR: Package type: Unknown (no lib/ or analyzers/)"
    ERRORS=$((ERRORS + 1))
fi
echo ""

# Check 6: Dependencies in package
echo "🔍 Check 6: Package dependencies"
NUSPEC_DEPS=$(unzip -p "$PACKAGE" "*.nuspec" 2>/dev/null | grep -c "<dependency" || echo "0")
if [ $NUSPEC_DEPS -gt 0 ]; then
    echo "  ⚠️  WARNING: Package declares $NUSPEC_DEPS dependencies"
    echo "     Generator packages should suppress dependencies"
    echo ""
    echo "     Recommended: <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>"
    WARNINGS=$((WARNINGS + 1))
else
    echo "  ✅ PASS: No dependencies in package"
fi
echo ""

# Summary
echo "========================================="
echo "📊 Verification Summary"
echo "========================================="
if [ $ERRORS -eq 0 ] && [ $WARNINGS -eq 0 ]; then
    echo "🎉 SUCCESS: Package structure is PERFECT!"
    echo ""
    echo "✅ No lib/ folder (prevents NU5017)"
    echo "✅ DLLs in analyzers/ folder"
    echo "✅ Core dependencies included"
    echo "✅ Build props for IDE support"
    echo "✅ Analyzer package type only"
    echo "✅ No unwanted dependencies"
    echo ""
    echo "This package will work correctly!"
    exit 0
elif [ $ERRORS -eq 0 ]; then
    echo "⚠️  WARNINGS: $WARNINGS warning(s) found"
    echo ""
    echo "Package will work but could be improved."
    echo "Review warnings above."
    exit 0
else
    echo "❌ FAILED: $ERRORS critical error(s), $WARNINGS warning(s)"
    echo ""
    echo "🚨 This package will NOT work correctly!"
    echo ""
    if [ $LIB_FILES -gt 0 ]; then
        echo "CRITICAL FIX NEEDED:"
        echo "  Add to .csproj: <IncludeBuildOutput>false</IncludeBuildOutput>"
        echo ""
    fi
    if [ $ANALYZER_DLLS -lt 1 ]; then
        echo "CRITICAL FIX NEEDED:"
        echo "  Add to .csproj:"
        echo "  <None Include=\"\$(OutputPath)\$(AssemblyName).dll\""
        echo "        Pack=\"true\""
        echo "        PackagePath=\"analyzers/dotnet/cs\" />"
        echo ""
    fi
    echo "See NU5017-TROUBLESHOOTING.md for detailed fixes"
    exit 1
fi
