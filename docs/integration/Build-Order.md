# 🏗️ Build Order Guide - REslava.Result Multi-Package Solution

## 🎯 Critical Build Sequence

**The build order is absolutely critical for successful packaging.** The SourceGenerator package depends on the Core package, so we must build them in the correct sequence.

---

## 📋 Required Build Order

### **Step 1/3: Build Core Package (Infrastructure)**
```bash
cd SourceGenerator/Core
dotnet clean
dotnet build -c Release
dotnet pack -c Release -o ../../nupkgs
```

**Why this is critical:**
- Core package contains the infrastructure that SourceGenerator depends on
- SourceGenerator cannot be built without Core package being available
- Core package must be published to local NuGet source before building SourceGenerator

### **Step 2/3: Create Local NuGet Source**
```bash
mkdir local-nuget
cp nupkgs/*.nupkg local-nuget/
```

**Why this is critical:**
- SourceGenerator needs to resolve Core package dependency during build
- Package path property `$(PkgREslava_Result_SourceGenerators_Core)` only works with local source
- Prevents version conflicts and ensures proper dependency resolution

### **Step 3/3: Build SourceGenerator Package**
```bash
cd SourceGenerator
dotnet restore --source ../local-nuget --source https://api.nuget.org/v3/index.json
dotnet clean
dotnet build -c Release
dotnet pack -c Release -o ../nupkgs
```

**Why this is critical:**
- SourceGenerator can now resolve Core package from local source
- Package path properties will resolve correctly
- All analyzer DLLs will be properly included in the package

---

## 🔄 Complete Build Script

```bash
#!/bin/bash
# Complete build script for REslava.Result packages

set -e

echo "======================================"
echo "Building REslava.Result Packages"
echo "======================================"

# Clean previous builds
echo "🧹 Cleaning previous builds..."
rm -rf nupkgs
mkdir -p nupkgs
mkdir -p local-nuget

echo ""
echo "======================================"
echo "Step 1/3: Building Core Package"
echo "======================================"
cd SourceGenerator/Core
dotnet clean
dotnet build -c Release
dotnet pack -c Release -o ../../nupkgs
cd ../..

echo ""
echo "======================================"
echo "Step 2/3: Creating Local NuGet Source"
echo "======================================"
cd ../..
cp nupkgs/*.nupkg local-nuget/

echo ""
echo "======================================"
echo "Step 3/3: Building SourceGenerator Package"
echo "======================================"
cd SourceGenerator
dotnet restore --source ../local-nuget --source https://api.nuget.org/v3/index.json
dotnet clean
dotnet build -c Release
dotnet pack -c Release -o ../nupkgs
cd ..

echo ""
echo "======================================"
echo "✅ Build Complete!"
echo "======================================"
echo ""
echo "📦 Created packages:"
ls -lh nupkgs/*.nupkg
```

---

## 🔍 Why This Order Is Critical

### **Dependency Chain:**
```
Consumer Project
    ↓
REslava.Result.SourceGenerators (depends on Core at build time)
    ↓
REslava.Result.SourceGenerators.Core (embedded dependency)
```

### **Package Path Resolution:**
- `$(PkgREslava_Result_SourceGenerators_Core)` only resolves when Core package is available
- This property is used to embed Core DLL in the analyzer path
- Without proper build order, package creation fails with `NU5017: Cannot create a package that has no dependencies nor content`

### **MSBuild Properties:**
All critical properties must work together:
- `<IsRoslynComponent>true</IsRoslynComponent>` - Marks as Roslyn component
- `<DevelopmentDependency>true</DevelopmentDependency>` - Development-only dependency
- `<IncludeBuildOutput>false</IncludeBuildOutput>` - Prevents runtime dependency
- `<SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>` - Suppresses unwanted dependencies

---

## 🧪 Verification Checklist

After building, verify the package structure:

```bash
# Check SourceGenerators package structure
unzip -l nupkgs/REslava.Result.SourceGenerators.*.nupkg | grep analyzers

# Should show:
# analyzers/dotnet/cs/REslava.Result.SourceGenerators.dll
# analyzers/dotnet/cs/REslava.Result.SourceGenerators.Core.dll
# build/REslava.Result.SourceGenerators.props
# buildTransitive/REslava.Result.SourceGenerators.props
```

**Expected Results:**
- ✅ Generator DLL in analyzers/dotnet/cs folder
- ✅ Core DLL in analyzers/dotnet/cs folder
- ✅ Build props files included
- ✅ No lib folder (correct for analyzer package)

---

## 🚨 Common Issues & Solutions

### **Issue: "NU5017: Cannot create a package that has no dependencies nor content"**
**Cause:** Build order incorrect or Core package not available
**Solution:** Follow the exact 3-step build order above

### **Issue: "Package path property not resolving"**
**Cause:** Core package not in local NuGet source
**Solution:** Create local NuGet source with Core package before building SourceGenerator

### **Issue: "Generator doesn't run after installation"**
**Cause:** DLLs not in correct analyzers folder
**Solution:** Verify package structure with unzip command above

---

## 📦 Final Package Structure

After successful build, you should have:

```
nupkgs/
├── REslava.Result.SourceGenerators.Core.1.9.2.nupkg
├── REslava.Result.SourceGenerators.1.9.2.nupkg
└── REslava.Result.1.9.2.nupkg
```

All packages are ready for local testing or NuGet publication.
