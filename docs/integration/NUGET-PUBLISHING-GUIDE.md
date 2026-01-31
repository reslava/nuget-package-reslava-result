# 🎯 Complete NuGet Publishing & Integration Guide
## REslava.Result Multi-Package Solution

**Last Updated:** January 29, 2026  
**Solution:** Multi-package NuGet with Source Generators

---

## 📦 Package Architecture

### **Package Overview**

Your solution consists of 3 NuGet packages:

```
1. REslava.Result (Main Library)
   ├─ Core Result types and utilities
   ├─ Target: net8.0, net9.0, net10.0
   └─ Used by: All consumers

2. REslava.Result.SourceGenerators.Core (Infrastructure)
   ├─ Base classes and utilities for generators
   ├─ Target: netstandard2.0
   ├─ Used by: Generator authors only
   └─ Not referenced directly by consumers

3. REslava.Result.SourceGenerators (Generator)
   ├─ Actual source generator implementation
   ├─ Target: netstandard2.0
   ├─ Depends on: Core (embedded)
   └─ Used by: API projects wanting code generation
```

### **Dependency Flow**

```
Consumer Project
│
├─ REslava.Result (required, runtime dependency)
│
└─ REslava.Result.SourceGenerators (optional, dev-only)
    └─ REslava.Result.SourceGenerators.Core (embedded, not visible)
```

---

## ⚠️ Common Problems & Solutions

### **Problem 1: "Generator doesn't run in consuming projects"**

**Cause:** Generator DLL not in `analyzers/dotnet/cs` folder in NuGet package

**Solution:** In SourceGenerators.csproj:
```xml
<ItemGroup>
  <None Include="$(OutputPath)\$(AssemblyName).dll" 
        Pack="true" 
        PackagePath="analyzers/dotnet/cs" 
        Visible="false" />
</ItemGroup>
```

### **Problem 2: "Missing Core dependencies at generation time"**

**Cause:** Core package DLL not included in analyzer path

**Solution:** In SourceGenerators.csproj:
```xml
<ItemGroup>
  <PackageReference Include="REslava.Result.SourceGenerators.Core" 
                    Version="1.0.0" 
                    PrivateAssets="all" 
                    GeneratePathProperty="true" />
</ItemGroup>

<ItemGroup>
  <None Include="$(PkgREslava_Result_SourceGenerators_Core)\lib\netstandard2.0\REslava.Result.SourceGenerators.Core.dll" 
        Pack="true" 
        PackagePath="analyzers/dotnet/cs" 
        Visible="false" />
</ItemGroup>
```

### **Problem 3: "Consumers get unwanted Roslyn dependencies"**

**Cause:** CodeAnalysis packages not marked as PrivateAssets

**Solution:** In both Core and SourceGenerators:
```xml
<PackageReference Include="Microsoft.CodeAnalysis.CSharp" 
                  Version="4.3.1" 
                  PrivateAssets="all" />
```

### **Problem 4: "Source generator appears as runtime dependency"**

**Cause:** Missing development dependency flags

**Solution:** In SourceGenerators.csproj:
```xml
<PropertyGroup>
  <DevelopmentDependency>true</DevelopmentDependency>
  <IncludeBuildOutput>false</IncludeBuildOutput>
  <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
</PropertyGroup>
```

### **Problem 5: "Version conflicts between packages"**

**Cause:** Packages use different Roslyn versions

**Solution:** Use consistent versions across all projects:
```xml
<!-- Use these specific versions everywhere -->
<PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" />
<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.3.1" />
```

---

## 🔧 Corrected Project Files

### **1. REslava.Result.csproj (Main Library)**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
    <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
    <PackageId>REslava.Result</PackageId>
    <!-- ... other metadata ... -->
  </PropertyGroup>

  <ItemGroup>
    <None Include="..\README.md" Pack="true" PackagePath="\" />
    <None Include="..\images\icon.png" Pack="true" PackagePath="\" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="All"/>
  </ItemGroup>

  <!-- NO generator references here -->
</Project>
```

**Key Points:**
- ✅ Multi-target framework (net8.0, net9.0, net10.0)
- ✅ No source generator dependencies
- ✅ Standalone runtime library
- ✅ Source Link for debugging

### **2. REslava.Result.SourceGenerators.Core.csproj (Infrastructure)**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>REslava.Result.SourceGenerators.Core</PackageId>
    <DevelopmentDependency>true</DevelopmentDependency>
    <!-- ... other metadata ... -->
  </PropertyGroup>

  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
  </ItemGroup>

  <ItemGroup>
    <!-- CRITICAL: All PrivateAssets=all -->
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.3.1" PrivateAssets="all" />
    <PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="All" />
  </ItemGroup>
</Project>
```

**Key Points:**
- ✅ netstandard2.0 for maximum compatibility
- ✅ All Roslyn dependencies marked PrivateAssets
- ✅ DevelopmentDependency flag
- ✅ This is a regular library, not an analyzer

### **3. REslava.Result.SourceGenerators.csproj (Generator)**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>REslava.Result.SourceGenerators</PackageId>
    <IsRoslynComponent>true</IsRoslynComponent>
    
    <!-- CRITICAL: Source Generator Flags -->
    <DevelopmentDependency>true</DevelopmentDependency>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
  </PropertyGroup>

  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
  </ItemGroup>

  <ItemGroup>
    <!-- Core package with GeneratePathProperty -->
    <PackageReference Include="REslava.Result.SourceGenerators.Core" 
                      Version="1.0.0" 
                      PrivateAssets="all" 
                      GeneratePathProperty="true" />
    
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.3.1" PrivateAssets="all" />
    <PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="All" />
  </ItemGroup>

  <!-- CRITICAL: Package as analyzer -->
  <ItemGroup>
    <!-- Include generator DLL -->
    <None Include="$(OutputPath)\$(AssemblyName).dll" 
          Pack="true" 
          PackagePath="analyzers/dotnet/cs" 
          Visible="false" />
    
    <!-- Include Core DLL (embedded dependency) -->
    <None Include="$(PkgREslava_Result_SourceGenerators_Core)\lib\netstandard2.0\REslava.Result.SourceGenerators.Core.dll" 
          Pack="true" 
          PackagePath="analyzers/dotnet/cs" 
          Visible="false" />
  </ItemGroup>

  <!-- Build props for IDE support -->
  <ItemGroup>
    <None Include="build\REslava.Result.SourceGenerators.props" 
          Pack="true" 
          PackagePath="build" />
    <None Include="buildTransitive\REslava.Result.SourceGenerators.props" 
          Pack="true" 
          PackagePath="buildTransitive" />
  </ItemGroup>
</Project>
```

**Key Points:**
- ✅ IncludeBuildOutput=false (don't include in bin)
- ✅ SuppressDependenciesWhenPacking=true (no runtime deps)
- ✅ Both generator and Core DLLs in analyzers folder
- ✅ GeneratePathProperty=true enables $(PkgPackageName) variable
- ✅ Build props for IDE integration

### **4. build/REslava.Result.SourceGenerators.props**

```xml
<Project>
  <PropertyGroup>
    <IsRoslynComponent>true</IsRoslynComponent>
  </PropertyGroup>
  
  <ItemGroup>
    <Analyzer Include="$(MSBuildThisFileDirectory)..\analyzers\dotnet\cs\REslava.Result.SourceGenerators.dll" />
  </ItemGroup>
</Project>
```

**Key Points:**
- ✅ Ensures analyzer is loaded
- ✅ Improves IDE experience
- ✅ Include in both build and buildTransitive folders

---

## 📝 Publishing Steps

### **Step 1: Version Alignment**

Ensure all three projects have aligned versions:

```xml
<!-- In ALL three .csproj files -->
<Version>1.0.0</Version>
```

### **Step 2: Build Packages**

```bash
# Clean previous builds
dotnet clean
rm -rf bin obj

# Build Core first
cd SourceGenerator/Core
dotnet pack -c Release

# Then build SourceGenerators (depends on Core)
cd ../
dotnet pack -c Release

# Finally build main library (independent)
cd ../../src
dotnet pack -c Release
```

### **Step 3: Verify Package Structure**

```bash
# Install NuGet Package Explorer or use command line
unzip -l REslava.Result.SourceGenerators.1.0.0.nupkg

# Should see:
# analyzers/dotnet/cs/REslava.Result.SourceGenerators.dll
# analyzers/dotnet/cs/REslava.Result.SourceGenerators.Core.dll
# build/REslava.Result.SourceGenerators.props
# buildTransitive/REslava.Result.SourceGenerators.props
```

### **Step 4: Local Testing**

```bash
# Create local NuGet feed
mkdir ~/local-nuget

# Copy packages
cp bin/Release/*.nupkg ~/local-nuget/

# Add as source
dotnet nuget add source ~/local-nuget --name LocalFeed

# Test in new project
cd /tmp
dotnet new webapi -n TestApi
cd TestApi
dotnet add package REslava.Result --version 1.0.0 --source LocalFeed
dotnet add package REslava.Result.SourceGenerators --version 1.0.0 --source LocalFeed
dotnet build
```

### **Step 5: Publish to NuGet**

```bash
# Get your API key from nuget.org
dotnet nuget push REslava.Result.1.0.0.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push REslava.Result.SourceGenerators.Core.1.0.0.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push REslava.Result.SourceGenerators.1.0.0.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
```

---

## 🧪 Consumer Usage

### **Minimal API Project**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- Main library (required) -->
    <PackageReference Include="REslava.Result" Version="1.0.0" />
    
    <!-- Source generator (optional) -->
    <PackageReference Include="REslava.Result.SourceGenerators" Version="1.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

### **Program.cs**

```csharp
using REslava.Result;

// Enable code generation
[assembly: REslava.Result.SourceGenerators.GenerateResultExtensions(
    Namespace = "MyApp.Generated",
    DefaultErrorStatusCode = 400)]

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Use generated extension methods
app.MapGet("/user/{id}", (int id) =>
{
    var result = GetUser(id);
    return result.ToIResult(); // Generated method
});

app.Run();

Result<User> GetUser(int id) => 
    id > 0 ? Result<User>.Ok(new User { Id = id }) 
           : Result<User>.Fail(new Error("Invalid ID"));

record User(int Id);
```

---

## 🔍 Troubleshooting

### **Issue: "ToIResult() method not found"**

**Check:**
1. ✅ SourceGenerators package is referenced
2. ✅ Assembly attribute is present
3. ✅ Project builds successfully
4. ✅ Check `obj/Generated` folder for generated files

**Fix:**
```bash
# Clean and rebuild
dotnet clean
dotnet build
```

### **Issue: "Generator runs locally but not from NuGet"**

**Check:**
1. ✅ DLLs are in `analyzers/dotnet/cs` folder in .nupkg
2. ✅ Core.dll is included alongside Generator.dll
3. ✅ build props file is present

**Verify:**
```bash
unzip -l package.nupkg | grep analyzers
# Should show both DLLs
```

### **Issue: "Consumers get Roslyn dependencies"**

**Check:**
1. ✅ All CodeAnalysis packages have `PrivateAssets="all"`
2. ✅ `SuppressDependenciesWhenPacking=true` in generator

**Fix:**
Update .csproj files as shown in corrected versions above.

### **Issue: "Version conflicts"**

**Solution:**
Use Directory.Build.props for consistent versions:

```xml
<Project>
  <PropertyGroup>
    <RoslynVersion>4.3.1</RoslynVersion>
    <AnalyzersVersion>3.3.4</AnalyzersVersion>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Update="Microsoft.CodeAnalysis.CSharp" Version="$(RoslynVersion)" />
    <PackageReference Update="Microsoft.CodeAnalysis.Analyzers" Version="$(AnalyzersVersion)" />
  </ItemGroup>
</Project>
```

---

## 💡 Should You Merge Packages?

### **Option 1: Keep Separate (Recommended)**

**Pros:**
- ✅ Core can be used by other generators
- ✅ Users choose if they want generators
- ✅ Cleaner separation of concerns
- ✅ Easier to version independently

**Cons:**
- ❌ More complex to publish
- ❌ Must coordinate versions

**Recommendation:** Keep separate if you plan to:
- Build more generators
- Let others build generators
- Version independently

### **Option 2: Merge Core into SourceGenerators**

**Pros:**
- ✅ Simpler publishing
- ✅ Fewer packages to manage
- ✅ Single version to track

**Cons:**
- ❌ Can't reuse Core infrastructure
- ❌ Larger generator package
- ❌ Less modular

**How to Merge:**

1. Move all Core files into SourceGenerators project
2. Remove Core project reference
3. Only publish Main + SourceGenerators

**Recommendation:** Only merge if:
- You won't build other generators
- You want simplicity over modularity

---

## ✅ Checklist Before Publishing

- [ ] All versions aligned (1.0.0)
- [ ] All Roslyn packages marked PrivateAssets=all
- [ ] Generator DLL in analyzers/dotnet/cs
- [ ] Core DLL in analyzers/dotnet/cs
- [ ] build props file included
- [ ] SuppressDependenciesWhenPacking=true
- [ ] Tested locally with local NuGet feed
- [ ] Generated code compiles
- [ ] No unwanted dependencies in consumers
- [ ] README files included in packages
- [ ] Icon file included (for main package)

---

## 📚 Additional Resources

### **Official Documentation**
- [Source Generators Cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md)
- [Creating NuGet Packages](https://learn.microsoft.com/en-us/nuget/create-packages/creating-a-package)

### **Your Documentation**
- Core package: `Core/README.md`
- SourceGenerators: `SourceGenerators/README.md`
- Main library: `README.md`

---

## 🎯 Quick Reference

### **Build Order**
```
1. Core (infrastructure)
2. SourceGenerators (depends on Core)
3. Main library (independent)
```

### **Package Relationships**
```
Consumer → Result (runtime)
Consumer → SourceGenerators (dev-only) → Core (embedded)
```

### **Critical Settings**
```xml
<!-- In Generator -->
<IncludeBuildOutput>false</IncludeBuildOutput>
<SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>

<!-- In all projects -->
<PackageReference Include="Microsoft.CodeAnalysis.*" PrivateAssets="all" />
```

---

**Last Updated:** January 29, 2026  
**Status:** Production Ready ✅
