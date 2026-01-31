# NU5017 QUICK FIX - Copy & Paste Solution

## ⚡ 30-Second Fix

Add these 4 lines to your `SourceGenerators.csproj`:

```xml
<PropertyGroup>
  <IncludeBuildOutput>false</IncludeBuildOutput>
  <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
  <DevelopmentDependency>true</DevelopmentDependency>
  <ProduceReferenceAssembly>false</ProduceReferenceAssembly>
</PropertyGroup>
```

Then:
```bash
dotnet clean
rm -rf bin obj
dotnet pack -c Release
```

---

## 🎯 What Each Setting Does

| Setting | Purpose | Effect |
|---------|---------|--------|
| `IncludeBuildOutput=false` | Don't put DLL in lib/ | **Fixes NU5017** |
| `SuppressDependenciesWhenPacking=true` | Don't include dependencies | Keeps package clean |
| `DevelopmentDependency=true` | Mark as dev-only | Better NuGet metadata |
| `ProduceReferenceAssembly=false` | No ref assembly | Cleaner package |

**The critical one is `IncludeBuildOutput=false`** - this alone fixes NU5017!

---

## ✅ Verification (2 commands)

```bash
# Check package structure
unzip -l YourPackage.nupkg | grep -E "lib/|analyzers/"

# Should see:
#   analyzers/dotnet/cs/YourGenerator.dll  ✅
#   (NO lib/ entries)                      ✅
```

---

## 🔧 Complete .csproj Template

Replace your entire `<PropertyGroup>` section:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    
    <!-- Package Info -->
    <PackageId>REslava.Result.SourceGenerators</PackageId>
    <Version>1.0.0</Version>
    
    <!-- CRITICAL: Analyzer Package Settings -->
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
    <DevelopmentDependency>true</DevelopmentDependency>
    <IsRoslynComponent>true</IsRoslynComponent>
  </PropertyGroup>

  <!-- Dependencies (all PrivateAssets) -->
  <ItemGroup>
    <PackageReference Include="REslava.Result.SourceGenerators.Core" Version="1.0.0" 
                      PrivateAssets="all" 
                      GeneratePathProperty="true" />
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.3.1" 
                      PrivateAssets="all" />
  </ItemGroup>

  <!-- Package as Analyzer -->
  <ItemGroup>
    <!-- This generator -->
    <None Include="$(OutputPath)\$(AssemblyName).dll" 
          Pack="true" 
          PackagePath="analyzers/dotnet/cs" />
    
    <!-- Core dependency -->
    <None Include="$(PkgREslava_Result_SourceGenerators_Core)\lib\netstandard2.0\*.dll" 
          Pack="true" 
          PackagePath="analyzers/dotnet/cs" />
  </ItemGroup>

</Project>
```

---

## 🚨 If It Still Fails

### Nuclear Clean:
```bash
dotnet clean
rm -rf bin obj nupkgs
rm -rf ~/.nuget/packages/REslava.Result*
dotnet restore --force
dotnet pack -c Release
```

### Check Package:
```bash
# Should show ONLY analyzers/, NO lib/
unzip -l bin/Release/*.nupkg
```

### Verify Settings:
```bash
# Add this to .csproj temporarily
<Target Name="ShowSettings" BeforeTargets="Pack">
  <Message Text="IncludeBuildOutput: $(IncludeBuildOutput)" Importance="high" />
  <Message Text="SuppressDependencies: $(SuppressDependenciesWhenPacking)" Importance="high" />
</Target>

dotnet pack
# Should output:
# IncludeBuildOutput: false
# SuppressDependencies: true
```

---

## 📋 Checklist

Before packing:
- [ ] `IncludeBuildOutput=false` is set
- [ ] `SuppressDependenciesWhenPacking=true` is set
- [ ] All PackageReferences have `PrivateAssets="all"`
- [ ] Core package has `GeneratePathProperty="true"`
- [ ] Deleted bin/ and obj/ folders
- [ ] Ran `dotnet clean`

After packing:
- [ ] `unzip -l` shows NO lib/ folder
- [ ] `unzip -l` shows analyzers/dotnet/cs/*.dll
- [ ] No NU5017 error during pack
- [ ] Consumer project can reference package

---

## 🎯 The One Thing That Matters

```xml
<IncludeBuildOutput>false</IncludeBuildOutput>
```

**This single line fixes NU5017.** Everything else is optimization.

---

## 📞 Still Stuck?

Run verification script:
```bash
chmod +x verify-package.sh
./verify-package.sh YourPackage.nupkg
```

The script will tell you exactly what's wrong.

---

**Quick Fix:** Add `<IncludeBuildOutput>false</IncludeBuildOutput>` → Clean → Rebuild → Done! ✅
