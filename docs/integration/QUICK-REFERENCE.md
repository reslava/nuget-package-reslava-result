# 🎯 Quick Reference: NuGet Multi-Package Publishing

## Critical .csproj Settings

### **Source Generator Package**
```xml
<!-- These 3 flags are CRITICAL -->
<DevelopmentDependency>true</DevelopmentDependency>
<IncludeBuildOutput>false</IncludeBuildOutput>
<SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>

<!-- Pack generator DLL in analyzers folder -->
<None Include="$(OutputPath)\$(AssemblyName).dll" 
      Pack="true" 
      PackagePath="analyzers/dotnet/cs" />

<!-- Pack Core dependency in analyzers folder -->
<PackageReference Include="REslava.Result.SourceGenerators.Core" 
                  GeneratePathProperty="true" 
                  PrivateAssets="all" />

<None Include="$(PkgREslava_Result_SourceGenerators_Core)\lib\netstandard2.0\*.dll" 
      Pack="true" 
      PackagePath="analyzers/dotnet/cs" />
```

### **All Packages**
```xml
<!-- Hide Roslyn from consumers -->
<PackageReference Include="Microsoft.CodeAnalysis.*" PrivateAssets="all" />
```

---

## Build & Publish Commands

```bash
# Clean
dotnet clean && rm -rf bin obj

# Build in order
cd Core && dotnet pack -c Release
cd ../SourceGenerators && dotnet pack -c Release  
cd ../src && dotnet pack -c Release

# Verify package structure
unzip -l *.nupkg | grep analyzers
# Must show: analyzers/dotnet/cs/YourGenerator.dll
#            analyzers/dotnet/cs/Core.dll

# Test locally
mkdir ~/local-feed
cp bin/Release/*.nupkg ~/local-feed/
dotnet nuget add source ~/local-feed -n Local

# Publish
dotnet nuget push *.nupkg --api-key KEY --source nuget.org
```

---

## Consumer Usage

```xml
<!-- Consumer .csproj -->
<PackageReference Include="REslava.Result" Version="1.0.0" />
<PackageReference Include="REslava.Result.SourceGenerators" Version="1.0.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

```csharp
// Consumer Program.cs
[assembly: GenerateResultExtensions(Namespace = "MyApp.Generated")]
```

---

## Common Mistakes ❌

| Mistake | Result | Fix |
|---------|--------|-----|
| Missing PrivateAssets on CodeAnalysis | Consumers get Roslyn | Add PrivateAssets="all" |
| Generator DLL not in analyzers folder | Generator doesn't run | Pack to analyzers/dotnet/cs |
| Core DLL not in analyzers folder | Runtime errors | Include Core in analyzers path |
| Missing SuppressDependencies | Runtime dependencies | Add to .csproj |
| Wrong target framework | Compatibility issues | Use netstandard2.0 |

---

## Verification Checklist

- [ ] Generator DLL in analyzers/dotnet/cs
- [ ] Core DLL in analyzers/dotnet/cs  
- [ ] build/*.props files included
- [ ] PrivateAssets on all Roslyn packages
- [ ] SuppressDependenciesWhenPacking=true
- [ ] Clean build succeeds
- [ ] Local test works
- [ ] Consumer project builds
- [ ] Generated code appears in obj/Generated

---

## Package Structure (Correct)

```
REslava.Result.SourceGenerators.nupkg
├── analyzers/
│   └── dotnet/
│       └── cs/
│           ├── REslava.Result.SourceGenerators.dll ✅
│           └── REslava.Result.SourceGenerators.Core.dll ✅
├── build/
│   └── REslava.Result.SourceGenerators.props ✅
├── buildTransitive/
│   └── REslava.Result.SourceGenerators.props ✅
└── lib/
    └── (empty - IncludeBuildOutput=false) ✅
```

---

## Package Structure (Wrong)

```
REslava.Result.SourceGenerators.nupkg
├── lib/
│   └── netstandard2.0/
│       └── REslava.Result.SourceGenerators.dll ❌ WRONG!
└── (missing analyzers folder) ❌ WRONG!
```

---

## Emergency Troubleshooting

**Generator doesn't run:**
```bash
# 1. Check package structure
unzip -l package.nupkg | grep -E "(analyzers|build)"

# 2. Check consumer obj folder
ls -la obj/Generated/

# 3. Rebuild consumer
dotnet clean && dotnet build
```

**Wrong dependencies:**
```bash
# Check what consumers see
dotnet list package --include-transitive
# Should NOT show Microsoft.CodeAnalysis.*
```

**Version mismatch:**
```bash
# Update all to same version
<Version>1.0.1</Version>  # in ALL .csproj
```

---

## Need Help?

1. Read: `NUGET-PUBLISHING-GUIDE.md`
2. Check: Corrected .csproj files
3. Test: Use TestProject as reference
4. Verify: Package structure with unzip

---

**Remember:** 
- Core = library package (normal NuGet)
- SourceGenerators = analyzer package (special folder structure)
- Main Result = runtime library (no generator reference)
