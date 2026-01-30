# Migration Guide: v1.7.3 → v1.9.0

## 🎯 Overview

This guide helps you migrate from REslava.Result v1.7.3 to v1.9.0, which introduces a revolutionary **Core Library architecture** that provides modular, reusable components for source generator development.

## 🚀 What's New in v1.9.0

### **🏗️ Core Library Architecture**
- **Modular Infrastructure** - Reusable components for generator development
- **Configuration-Driven Generators** - Flexible, type-safe configuration management
- **Enhanced Testing** - 100% test coverage with 32 tests
- **Better Error Handling** - Graceful handling of edge cases and null inputs

### **📦 Package Structure Changes**
- **Core Library** - New `REslava.Result.SourceGenerators.Core` package
- **Refactored Generators** - Updated to use Core library infrastructure
- **Improved Project Structure** - Cleaner, more organized codebase

---

## 🔄 Step-by-Step Migration

### **Step 1: Update Package References**

#### **Before (v1.7.3):**
```xml
<ItemGroup>
  <ProjectReference Include="../SourceGenerator/REslava.Result.SourceGenerators.csproj" 
                   ReferenceOutputAssembly="false" 
                   OutputItemType="Analyzer" />
</ItemGroup>
```

#### **After (v1.9.0):**
```xml
<ItemGroup>
  <!-- Core library infrastructure -->
  <ProjectReference Include="../SourceGenerator/Core/REslava.Result.SourceGenerators.Core.csproj" 
                   ReferenceOutputAssembly="false" 
                   OutputItemType="Analyzer" />
  
  <!-- Refactored generator as analyzer -->
  <ProjectReference Include="../SourceGenerator/Generators/ResultToIResult/ResultToIResultGenerator.csproj" 
                   ReferenceOutputAssembly="false" 
                   OutputItemType="Analyzer" />
  
  <!-- Refactored generator for attribute access -->
  <ProjectReference Include="../SourceGenerator/Generators/ResultToIResult/ResultToIResultGenerator.csproj" 
                   ReferenceOutputAssembly="true" />
</ItemGroup>
```

### **Step 2: Update Assembly Attribute**

#### **Before (v1.7.3):**
```csharp
using REslava.Result.SourceGenerators;

[assembly: GenerateResultExtensions]
```

#### **After (v1.9.0):**
```csharp
using REslava.Result.SourceGenerators;

[assembly: GenerateResultExtensions(
    Namespace = "Generated.ResultExtensions",
    IncludeErrorTags = true,
    GenerateHttpMethodExtensions = true,
    DefaultErrorStatusCode = 400,
    IncludeDetailedErrors = true,
    GenerateAsyncMethods = true,
    CustomErrorMappings = new[] { "CustomError:418", "SpecialCase:429" }
)]
```

### **Step 3: Update Using Statements**

#### **Before (v1.7.3):**
```csharp
using REslava.Result;
using Generated.ResultExtensions; // Default namespace
```

#### **After (v1.9.0):**
```csharp
using REslava.Result;
using Generated.ResultExtensions; // Your custom namespace
```

---

## 📋 Detailed Changes

### **🏗️ Project Structure Changes**

#### **Before (v1.7.3):**
```
SourceGenerator/
├── REslava.Result.SourceGenerators.csproj  # Monolithic generator
├── Attributes/                             # Attribute definitions
└── Generators/                            # Generator implementations
```

#### **After (v1.9.0):**
```
SourceGenerator/
├── Core/                                  # 🏗️ Core Library Infrastructure
│   ├── CodeGeneration/                   # CodeBuilder utilities
│   ├── Utilities/                         # HttpStatusCodeMapper, AttributeParser
│   ├── Configuration/                     # Configuration base classes
│   └── Infrastructure/                    # IncrementalGeneratorBase
├── Generators/                           # 📦 Individual Generators
│   └── ResultToIResult/                  # Refactored ResultToIResult generator
└── Tests/                                # 🧪 Comprehensive Tests
    ├── UnitTests/                         # Core library component tests
    ├── IntegrationTests/                  # Generator integration tests
    └── GeneratorTest/                     # Console verification tests
```

### **⚙️ Configuration Enhancements**

#### **New Configuration Options:**
- **`Namespace`** - Custom namespace for generated code (default: "Generated")
- **`IncludeErrorTags`** - Include error tags in responses (default: true)
- **`GenerateHttpMethodExtensions`** - Generate HTTP method-specific extensions (default: true)
- **`DefaultErrorStatusCode`** - Default HTTP status code for errors (default: 400)
- **`IncludeDetailedErrors`** - Include detailed error information (default: false)
- **`GenerateAsyncMethods`** - Generate async extension methods (default: true)
- **`CustomErrorMappings`** - Custom error type to status code mappings

#### **Example:**
```csharp
[assembly: GenerateResultExtensions(
    Namespace = "MyApp.Generated",
    IncludeErrorTags = true,
    GenerateHttpMethodExtensions = true,
    DefaultErrorStatusCode = 422,
    IncludeDetailedErrors = true,
    GenerateAsyncMethods = true,
    CustomErrorMappings = new[] { 
        "BusinessRuleViolation:422",
        "RateLimitExceeded:429",
        "ServiceUnavailable:503"
    }
)]
```

---

## 🧪 Testing Changes

### **New Test Structure**

#### **Before (v1.7.3):**
- Basic console tests
- Limited test coverage
- No structured testing approach

#### **After (v1.9.0):**
- **32 tests** with 100% success rate
- **Unit Tests** - Individual Core library component testing
- **Integration Tests** - Generator integration scenarios
- **Console Tests** - Quick verification tests

#### **Running Tests:**
```bash
# Unit tests (Core library components)
cd SourceGenerator/Tests/UnitTests
dotnet run --project CoreLibraryTest.csproj

# Integration tests (Generator scenarios)
cd SourceGenerator/Tests/IntegrationTests
dotnet run --project IntegrationTests.csproj

# Console tests (Quick verification)
cd SourceGenerator/Tests/GeneratorTest
dotnet run --project ConsoleTest.csproj
```

---

## 🔄 Breaking Changes

### **📦 Package References**
- **Required:** Core library reference is now mandatory
- **Updated:** Generator project paths have changed
- **Additional:** Need both analyzer and runtime references

### **⚙️ Configuration**
- **Enhanced:** More configuration options available
- **Required:** Some previously default behaviors now need explicit configuration
- **Improved:** Better validation and error handling

### **🏗️ Project Structure**
- **Reorganized:** Source generator structure has been completely reorganized
- **Modular:** Components are now in separate, focused projects
- **Cleaner:** Removed legacy test applications and temporary files

---

## 🛠️ Common Migration Scenarios

### **Scenario 1: Basic ASP.NET Core Application**

#### **Before (v1.7.3):**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <ItemGroup>
    <ProjectReference Include="../SourceGenerator/REslava.Result.SourceGenerators.csproj" 
                     ReferenceOutputAssembly="false" 
                     OutputItemType="Analyzer" />
    <ProjectReference Include="../src/REslava.Result.csproj" />
  </ItemGroup>
</Project>
```

```csharp
using REslava.Result.SourceGenerators;
[assembly: GenerateResultExtensions]
```

#### **After (v1.9.0):**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <ItemGroup>
    <ProjectReference Include="../SourceGenerator/Core/REslava.Result.SourceGenerators.Core.csproj" 
                     ReferenceOutputAssembly="false" 
                     OutputItemType="Analyzer" />
    <ProjectReference Include="../SourceGenerator/Generators/ResultToIResult/ResultToIResultGenerator.csproj" 
                     ReferenceOutputAssembly="false" 
                     OutputItemType="Analyzer" />
    <ProjectReference Include="../SourceGenerator/Generators/ResultToIResult/ResultToIResultGenerator.csproj" 
                     ReferenceOutputAssembly="true" />
    <ProjectReference Include="../src/REslava.Result.csproj" />
  </ItemGroup>
</Project>
```

```csharp
using REslava.Result.SourceGenerators;
[assembly: GenerateResultExtensions(
    Namespace = "Generated.ResultExtensions",
    IncludeErrorTags = true,
    GenerateHttpMethodExtensions = true,
    DefaultErrorStatusCode = 400,
    IncludeDetailedErrors = true,
    GenerateAsyncMethods = true
)]
```

### **Scenario 2: Custom Error Mappings**

#### **Before (v1.7.3):**
```csharp
// Limited custom mapping support
[assembly: GenerateResultExtensions]
```

#### **After (v1.9.0):**
```csharp
[assembly: GenerateResultExtensions(
    CustomErrorMappings = new[] {
        "InventoryOutOfStock:422",
        "PaymentDeclined:402",
        "AccountSuspended:403",
        "MaintenanceMode:503"
    }
)]
```

### **Scenario 3: Class Library Project**

#### **Before (v1.7.3):**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="../SourceGenerator/REslava.Result.SourceGenerators.csproj" 
                     ReferenceOutputAssembly="false" 
                     OutputItemType="Analyzer" />
  </ItemGroup>
</Project>
```

#### **After (v1.9.0):**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="../SourceGenerator/Core/REslava.Result.SourceGenerators.Core.csproj" 
                     ReferenceOutputAssembly="false" 
                     OutputItemType="Analyzer" />
    <ProjectReference Include="../SourceGenerator/Generators/ResultToIResult/ResultToIResultGenerator.csproj" 
                     ReferenceOutputAssembly="false" 
                     OutputItemType="Analyzer" />
  </ItemGroup>
</Project>
```

---

## 🔧 Troubleshooting

### **Issue: Build Errors After Migration**

#### **Problem:**
```
error CS0246: The type or namespace name 'GenerateResultExtensionsAttribute' could not be found
```

#### **Solution:**
Ensure you have both analyzer and runtime references:
```xml
<ProjectReference Include="../SourceGenerator/Generators/ResultToIResult/ResultToIResultGenerator.csproj" 
                 ReferenceOutputAssembly="true" />
```

### **Issue: Generated Code Not Found**

#### **Problem:**
```
error CS0103: The name 'ToIResult' does not exist in the current context
```

#### **Solution:**
1. Check your namespace configuration in the assembly attribute
2. Ensure you have the correct using statement:
```csharp
using Generated.ResultExtensions; // Or your custom namespace
```

### **Issue: Configuration Not Working**

#### **Problem:**
Custom configuration options are not being applied.

#### **Solution:**
1. Ensure all configuration properties are spelled correctly
2. Check that CustomErrorMappings format is correct: "ErrorName:StatusCode"
3. Verify that the assembly attribute is applied correctly

---

## 📊 Migration Checklist

### **✅ Pre-Migration**
- [ ] Backup current project
- [ ] Note current configuration settings
- [ ] Identify all projects using REslava.Result
- [ ] Test current functionality

### **✅ Migration Steps**
- [ ] Update package references in all projects
- [ ] Update assembly attribute with new configuration
- [ ] Update using statements
- [ ] Clean and rebuild solution

### **✅ Post-Migration**
- [ ] Run all tests
- [ ] Verify generated code is working
- [ ] Test all error scenarios
- [ ] Update documentation
- [ ] Commit changes

---

## 🚀 Benefits of Migration

### **🏗️ Improved Architecture**
- **Modular Design** - Components are reusable and focused
- **Better Testing** - 100% test coverage with comprehensive scenarios
- **Cleaner Code** - Organized, maintainable codebase

### **⚙️ Enhanced Configuration**
- **Type Safety** - Compile-time configuration validation
- **Flexibility** - More configuration options
- **Better Defaults** - Sensible defaults for all scenarios

### **🧪 Better Development Experience**
- **Comprehensive Testing** - 32 tests with 100% success rate
- **Better Error Messages** - Clear error reporting
- **Improved Documentation** - Detailed guides and examples

### **🚀 Future-Proof**
- **Extensible** - Easy to add new generators
- **Maintainable** - Clean separation of concerns
- **Performance** - Optimized code generation

---

## 📚 Additional Resources

- **[Core Library Documentation](CORE-LIBRARY.md)** - Detailed Core library guide
- **[Generator Development Guide](GENERATOR-DEVELOPMENT.md)** - Generator development guide
- **[Testing Documentation](TESTING.md)** - Testing strategies and guidelines
- **[API Reference](../SourceGenerator/Core/)** - Full API documentation

---

## 🤝 Getting Help

If you encounter issues during migration:

1. **📚 Check Documentation** - Review this guide and other documentation
2. **🧪 Run Tests** - Ensure all tests are passing
3. **🔍 Check Examples** - Review sample projects
4. **🐛 Report Issues** - Create an issue on GitHub with details

---

## 📄 Version History

- **v1.9.0** - Core Library architecture, modular design, comprehensive testing
- **v1.7.3** - Previous stable version with monolithic generator
- **v1.8.0** - Enhanced metadata discovery system (interim version)

---

## 🎉 Conclusion

Migrating to v1.9.0 provides significant benefits in terms of architecture, testing, configuration, and maintainability. The Core Library architecture makes the system more robust, extensible, and developer-friendly.

The migration process is straightforward and the benefits far outweigh the minimal effort required to update your projects.

**Welcome to REslava.Result v1.9.0!** 🚀
