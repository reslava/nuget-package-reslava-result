# 🔧 REslava.Result.SourceGenerators Troubleshooting Guide

## 🎯 Purpose

This guide helps you troubleshoot common issues with REslava.Result.SourceGenerators v1.9.4 (SOLID Architecture).

## 🚨 Common Issues

### **1. Generated Code Not Appearing**

#### **Symptoms**
- No generated files in `obj/Debug/netX.X/generated/`
- Extension methods not recognized by IntelliSense
- Build succeeds but `ToIResult()` methods not found

#### **Causes**
- Source generator not loaded
- `<EmitCompilerGeneratedFiles>` not enabled
- Package version conflicts
- .NET version incompatibility

#### **Solutions**
```xml
<!-- Add to your .csproj file -->
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
```

```bash
# Clear NuGet cache
dotnet nuget locals http-cache --clear
dotnet nuget locals global-packages --clear

# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

#### **Verification**
```bash
# Check generated files
ls obj/Debug/netX.X/generated/REslava.Result.SourceGenerators/

# Check package installation
dotnet list package | grep REslava.Result.SourceGenerators
```

---

### **2. Duplicate Generation Errors (CS0101, CS0579)**

#### **Symptoms**
- `CS0101: The namespace already contains a definition`
- `CS0579: Duplicate attribute definition`
- Multiple generated files with same names

#### **Causes**
- Multiple versions of package referenced
- Old generator files still present
- Project reference conflicts

#### **Solutions**
```xml
<!-- Ensure only one package reference -->
<PackageReference Include="REslava.Result.SourceGenerators" Version="1.9.4" />
```

```bash
# Remove old project references
# Check your .csproj for old references like:
# <ProjectReference Include="ResultToIResultGenerator.csproj" />
```

#### **Prevention**
- Use **v1.9.4** (SOLID architecture eliminates duplicates)
- Remove old project references
- Clean solution regularly

---

### **3. Package Installation Issues**

#### **Symptoms**
- Package not found during `dotnet add package`
- Version conflicts during restore
- Missing dependencies

#### **Solutions**
```bash
# Check package availability
dotnet package search REslava.Result.SourceGenerators

# Use specific version
dotnet add package REslava.Result.SourceGenerators --version 1.9.4

# Clear NuGet cache
dotnet nuget locals all --clear
```

#### **Local Package Testing**
```bash
# Add local NuGet source
dotnet nuget add source ./local-nuget --name "Local"

# Install from local source
dotnet add package REslava.Result.SourceGenerators --source ./local-nuget
```

---

### **4. Compilation Errors**

#### **Symptoms**
- `CS1061: 'Result<T>' does not contain a definition for 'ToIResult'`
- Missing extension methods
- Type resolution errors

#### **Causes**
- Missing `[assembly: GenerateResultExtensions]` attribute
- Incorrect namespace
- Package not loaded

#### **Solutions**
```csharp
// Add to any .cs file (usually Program.cs or AssemblyInfo.cs)
using REslava.Result.SourceGenerators;

[assembly: GenerateResultExtensions(
    Namespace = "Generated.ResultExtensions",
    IncludeErrorTags = true,
    GenerateHttpMethodExtensions = true
)]
```

#### **Verification**
```bash
# Check generated namespace
cat obj/Debug/netX.X/generated/REslava.Result.SourceGenerators/*.g.cs | grep namespace
```

---

### **5. Performance Issues**

#### **Symptoms**
- Slow compilation times
- High memory usage during build
- IDE becoming unresponsive

#### **Causes**
- Large number of Result types
- Complex configurations
- Incremental compilation issues

#### **Solutions**
```csharp
// Simplify configuration
[assembly: GenerateResultExtensions(
    Namespace = "Generated.ResultExtensions",
    IncludeErrorTags = true,
    GenerateHttpMethodExtensions = true
    // Remove complex options for better performance
)]
```

```bash
# Clear incremental cache
dotnet clean
rm -rf obj/ bin/
dotnet build
```

---

### **6. IDE Integration Issues**

#### **Symptoms**
- IntelliSense not recognizing generated code
- Debugging issues with generated files
- Error highlighting problems

#### **Solutions**
```xml
<!-- Enable generated file visibility -->
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>

<!-- Enable debugging of generated code -->
<CompilerGeneratedFilesOutputPath>generated</CompilerGeneratedFilesOutputPath>
```

#### **Visual Studio**
1. **Build** the solution
2. **Reload** the project
3. **Clear** IntelliSense cache: `Edit > IntelliSense > Refresh Local Cache`

#### **VS Code**
1. **Reload** window: `Ctrl+Shift+P > Developer: Reload Window`
2. **Restart** OmniSharp server
3. **Clear** cache: `Ctrl+Shift+P > OmniSharp: Restart OmniSharp`

---

## 🔍 Debugging Tools

### **Generated File Inspection**
```bash
# Find generated files
find . -name "*.g.cs" -path "*/generated/*"

# View generated code
cat obj/Debug/netX.X/generated/REslava.Result.SourceGenerators/*.g.cs

# Check generator execution
dotnet build -v normal | grep -i generator
```

### **Package Analysis**
```bash
# Check package contents
unzip -l ~/.nuget/packages/re-slava.result.sourcegenerators/1.9.4/*.nupkg

# Verify package metadata
nuget spec REslava.Result.SourceGenerators.1.9.4.nupkg
```

### **Build Diagnostics**
```bash
# Detailed build output
dotnet build -v detailed

# Check for warnings
dotnet build -warnaserror

# Performance analysis
dotnet build -v diagnostic
```

---

## 📋 Environment Checklist

### **✅ Required Components**
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [REslava.Result package](https://www.nuget.org/packages/REslava.Result/)
- [REslava.Result.SourceGenerators v1.9.4](https://www.nuget.org/packages/REslava.Result.SourceGenerators/)

### **✅ Project Configuration**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="REslava.Result" Version="1.9.0" />
    <PackageReference Include="REslava.Result.SourceGenerators" Version="1.9.4" />
  </ItemGroup>
</Project>
```

### **✅ Assembly Configuration**
```csharp
using REslava.Result.SourceGenerators;

[assembly: GenerateResultExtensions(
    Namespace = "Generated.ResultExtensions",
    IncludeErrorTags = true,
    GenerateHttpMethodExtensions = true,
    DefaultErrorStatusCode = 400
)]
```

---

## 🆘 Getting Help

### **Self-Service**
1. **Check this guide** for common solutions
2. **Search GitHub Issues** for similar problems
3. **Review documentation** for configuration details

### **Community Support**
- **GitHub Issues**: [Create new issue](https://github.com/reslava/nuget-package-reslava-result/issues)
- **GitHub Discussions**: [Ask question](https://github.com/reslava/nuget-package-reslava-result/discussions)
- **Stack Overflow**: Tag with `re-slava-result`

### **Issue Reporting**
When reporting issues, include:
- **Package version**: v1.9.4
- **.NET version**: 8.0/9.0/10.0
- **Project file**: Minimal .csproj that reproduces issue
- **Error messages**: Full build output
- **Steps to reproduce**: Clear, numbered steps

---

## 🎯 Prevention Tips

### **Best Practices**
1. **Use v1.9.4** (SOLID architecture eliminates common issues)
2. **Clean solution regularly**
3. **Single package reference** (avoid mixing versions)
4. **Simple configuration** (start basic, add complexity later)
5. **Test in clean environment** (validate before production)

### **Maintenance**
1. **Update packages** regularly
2. **Review changelog** for breaking changes
3. **Test with new .NET versions**
4. **Monitor performance** as project grows

---

## 🔚 Conclusion

Most issues with REslava.Result.SourceGenerators v1.9.4 are resolved by:
1. **Using the correct version** (v1.9.4)
2. **Proper configuration** (assembly attribute)
3. **Clean environment** (clear cache, rebuild)
4. **SOLID architecture** (eliminates duplicate generation)

The v1.9.4 SOLID architecture has eliminated the most common issues from previous versions. If you're still experiencing problems, please reach out to the community for support.

---

*Happy coding with REslava.Result!* 🎯
