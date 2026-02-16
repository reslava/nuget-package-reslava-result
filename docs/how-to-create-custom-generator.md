# How to Create Custom Generators

## 🎯 Introduction

REslava.Result.SourceGenerators is built on a **SOLID, extensible architecture** that makes it easy to create your own source generators. Whether you want to generate validation code, caching layers, logging utilities, or custom HTTP mappings, this guide will show you how.

## 📐 Understanding the Architecture

### 📦 Core Components

The generator architecture follows SOLID principles with clear separation of concerns:

```
Your Custom Generator/
├── Attributes/                    # 🏷️ Attribute generation
│   └── YourCustomAttributeGenerator.cs
├── CodeGeneration/               # 💻 Code generation logic
│   └── YourCustomCodeGenerator.cs
├── Orchestration/                # 🎼 Pipeline coordination
│   └── YourCustomOrchestrator.cs
└── YourCustomGenerator.cs        # 🚀 Main entry point
```

### 🔌 Key Interfaces

| Interface | Responsibility | When to Implement |
|-----------|----------------|------------------|
| `IAttributeGenerator` | Generates C# attributes | When you need custom attributes |
| `ICodeGenerator` | Generates C# code | When you need extension methods or classes |
| `IOrchestrator` | Coordinates generation pipeline | Always - this is your main coordinator |

## 🚀 Creating Your First Generator

### Step 1: Define the Generator Entry Point

```csharp
using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Core.Interfaces;

namespace YourProject.Generators
{
    [Generator]
    public class YourCustomGenerator : IIncrementalGenerator
    {
        private readonly YourCustomOrchestrator _orchestrator;
        
        public YourCustomGenerator()
        {
            _orchestrator = new YourCustomOrchestrator();
        }
        
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            System.Diagnostics.Debug.WriteLine("🔥 YourCustomGenerator.Initialize called!");
            _orchestrator.Initialize(context);
        }
    }
}
```

### Step 2: Create the Orchestrator

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.SourceGenerators.Core.Interfaces;
using REslava.Result.SourceGenerators.Generators.YourCustom.Attributes;
using REslava.Result.SourceGenerators.Generators.YourCustom.CodeGeneration;

namespace YourProject.Generators.Orchestration
{
    public class YourCustomOrchestrator
    {
        private readonly IAttributeGenerator _attributeGenerator;
        private readonly ICodeGenerator _codeGenerator;
        
        public YourCustomOrchestrator()
        {
            _attributeGenerator = new YourCustomAttributeGenerator();
            _codeGenerator = new YourCustomCodeGenerator();
        }
        
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Step 1: Register attributes for immediate availability
            var attributePipeline = context.CompilationProvider.Select((compilation, _) =>
            {
                // Check if your target types are available
                var hasTargetTypes = compilation.SyntaxTrees
                    .SelectMany(st => st.GetRoot().DescendantNodes())
                    .OfType<GenericNameSyntax>()
                    .Any(gns => gns.Identifier.ValueText == "YourTargetType");
                
                return hasTargetTypes ? compilation : null;
            });

            context.RegisterSourceOutput(attributePipeline, (spc, compilation) =>
            {
                if (compilation == null) return;
                
                // Generate attributes
                spc.AddSource("YourCustomAttribute.g.cs", 
                    _attributeGenerator.GenerateAttribute());
            });

            // Step 2: Register code generation pipeline
            var codePipeline = context.CompilationProvider.Select((compilation, _) =>
            {
                // Same detection logic as attributes
                var hasTargetTypes = compilation.SyntaxTrees
                    .SelectMany(st => st.GetRoot().DescendantNodes())
                    .OfType<GenericNameSyntax>()
                    .Any(gns => gns.Identifier.ValueText == "YourTargetType");
                
                return hasTargetTypes ? compilation : null;
            });

            context.RegisterSourceOutput(codePipeline, (spc, compilation) =>
            {
                if (compilation == null) return;
                
                // Generate extension methods
                var extensionCode = _codeGenerator.GenerateCode(compilation, null);
                spc.AddSource("YourCustomExtensions.g.cs", extensionCode);
            });
        }
    }
}
```

### Step 3: Implement Attribute Generator

```csharp
using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Text;

namespace YourProject.Generators.Attributes
{
    public class YourCustomAttributeGenerator : IAttributeGenerator
    {
        public SourceText GenerateAttribute()
        {
            var source = @"
using System;

namespace YourProject.Generators
{
    /// <summary>
    /// Indicates that this type should have custom extensions generated.
    /// Generated by YourCustomGenerator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class GenerateYourCustomExtensionsAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the namespace for generated extensions.
        /// </summary>
        public string Namespace { get; set; } = ""Generated.YourCustomExtensions"";
        
        /// <summary>
        /// Gets or sets whether to enable advanced features.
        /// </summary>
        public bool EnableAdvancedFeatures { get; set; } = true;
    }
}
";
            return SourceText.From(source, Encoding.UTF8);
        }
    }
}
```

### Step 4: Implement Code Generator

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Core.Interfaces;
using System.Text;

namespace YourProject.Generators.CodeGeneration
{
    public class YourCustomCodeGenerator : ICodeGenerator
    {
        public SourceText GenerateCode(Compilation compilation, object config)
        {
            var builder = new StringBuilder();
            
            // Add using statements
            builder.AppendLine("using Microsoft.AspNetCore.Http;");
            builder.AppendLine("using YourProject.Models;");
            builder.AppendLine("using System.Linq;");
            builder.AppendLine();
            
            // Generate the class and methods
            builder.AppendLine("namespace Generated.YourCustomExtensions");
            builder.AppendLine("{");
            builder.AppendLine("    /// <summary>");
            builder.AppendLine("    /// Extension methods for your custom types.");
            builder.AppendLine("    /// Generated by YourCustomGenerator.");
            builder.AppendLine("    /// </summary>");
            builder.AppendLine("    public static class YourCustomExtensions");
            builder.AppendLine("    {");
            
            // Generate extension method
            builder.AppendLine("        /// <summary>");
            builder.AppendLine("        /// Converts YourTargetType to IResult with intelligent mapping.");
            builder.AppendLine("        /// </summary>");
            builder.AppendLine("        public static IResult ToCustomResult(this YourTargetType target)");
            builder.AppendLine("        {");
            builder.AppendLine("            // Your custom logic here");
            builder.AppendLine("            if (target.IsValid)");
            builder.AppendLine("                return Results.Ok(target);");
            builder.AppendLine("            else");
            builder.AppendLine("                return Results.BadRequest(target.ValidationErrors);");
            builder.AppendLine("        }");
            builder.AppendLine("    }");
            builder.AppendLine("}");
            
            return SourceText.From(builder.ToString(), Encoding.UTF8);
        }
    }
}
```

## 📚 Real-World Examples

### Example 1: Validation Generator

```csharp
// Target: Automatically generate validation methods for DTOs
[GenerateValidationExtensions]
public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [MinLength(2)]
    public string Name { get; set; }
}

// Generated output:
public static class ValidationExtensions
{
    public static Result<CreateUserRequest> Validate(this CreateUserRequest request)
    {
        return Result<CreateUserRequest>.Ok(request)
            .Ensure(r => !string.IsNullOrWhiteSpace(r.Email), "Email required")
            .Ensure(r => r.Email.Contains("@"), "Invalid email format")
            .Ensure(r => !string.IsNullOrWhiteSpace(r.Name), "Name required")
            .Ensure(r => r.Name.Length >= 2, "Name must be at least 2 characters");
    }
}
```

### Example 2: Caching Generator

```csharp
// Target: Automatically generate caching decorators
[GenerateCacheExtensions(TtlSeconds = 300)]
public class UserService
{
    public async Task<User> GetUserAsync(int id) { /* implementation */ }
}

// Generated output:
public static class CacheExtensions
{
    public static async Task<User> GetUserCachedAsync(
        this UserService service, 
        int id, 
        IMemoryCache cache)
    {
        string key = $"User_{id}";
        if (cache.TryGetValue(key, out User cachedUser))
            return cachedUser;
            
        var user = await service.GetUserAsync(id);
        cache.Set(key, user, TimeSpan.FromSeconds(300));
        return user;
    }
}
```

### Example 3: Logging Generator

```csharp
// Target: Automatically generate logging decorators
[GenerateLoggingExtensions]
public class OrderService
{
    public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request) { /* implementation */ }
}

// Generated output:
public static class LoggingExtensions
{
    public static async Task<Result<Order>> CreateOrderWithLoggingAsync(
        this OrderService service, 
        CreateOrderRequest request,
        ILogger<OrderService> logger)
    {
        logger.LogInformation("Creating order for user {UserId}", request.UserId);
        
        var result = await service.CreateOrderAsync(request);
        
        if (result.IsSuccess)
            logger.LogInformation("Order {OrderId} created successfully", result.Value.Id);
        else
            logger.LogWarning("Order creation failed: {Errors}", string.Join(", ", result.Errors));
            
        return result;
    }
}
```

## 🧪 Testing Your Generator

### Unit Testing Strategy

```csharp
[TestClass]
public class YourCustomGeneratorTests
{
    [TestMethod]
    public async Task Generator_Should_Generate_Extensions_For_Target_Type()
    {
        // Arrange
        var source = @"
using YourProject.Generators;

[GenerateYourCustomExtensions]
public class YourTargetType
{
    public bool IsValid { get; set; }
    public string[] ValidationErrors { get; set; }
}
";

        // Act
        var generatedOutput = await RunGenerator(source);

        // Assert
        Assert.IsNotNull(generatedOutput, "Generator should produce output");
        Assert.IsTrue(generatedOutput.Contains("YourCustomExtensions"), "Should generate extension class");
        Assert.IsTrue(generatedOutput.Contains("ToCustomResult"), "Should generate extension method");
    }

    [TestMethod]
    public async Task Generator_Should_Not_Generate_Without_Attribute()
    {
        // Arrange
        var source = @"
public class YourTargetType
{
    public bool IsValid { get; set; }
}
";

        // Act
        var generatedOutput = await RunGenerator(source);

        // Assert
        Assert.IsTrue(string.IsNullOrEmpty(generatedOutput), "Generator should not produce output without attribute");
    }

    private async Task<string> RunGenerator(string source)
    {
        var compilation = CreateCompilation(source);
        var generator = new YourCustomGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        var runResult = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        var generatedFiles = runResult.GetRunResult().GeneratedTrees;
        
        if (generatedFiles.IsEmpty)
            return string.Empty;

        var combinedOutput = new StringBuilder();
        foreach (var tree in generatedFiles)
        {
            using var writer = new StringWriter();
            tree.GetText().Write(writer);
            combinedOutput.AppendLine(writer.ToString());
        }

        return combinedOutput.ToString();
    }

    private Compilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        };

        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class YourCustomGeneratorIntegrationTests
{
    [TestMethod]
    public async Task Generated_Code_Should_Compile_And_Work()
    {
        // Arrange
        var source = @"
using YourProject.Generators;
using Microsoft.AspNetCore.Http;

[GenerateYourCustomExtensions]
public class TestTarget
{
    public bool IsValid { get; set; } = true;
    public string[] ValidationErrors { get; set; } = Array.Empty<string>();
}

public class TestController
{
    public IResult GetTestResult()
    {
        var target = new TestTarget();
        return target.ToCustomResult();
    }
}
";

        // Act
        var compilation = CreateCompilationWithAspNetCore(source);
        var generator = new YourCustomGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        var runResult = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        // Assert
        Assert.IsFalse(diagnostics.HasAnyErrors(), "Generated code should compile without errors");
        Assert.IsTrue(outputCompilation.SyntaxTrees.Count() > compilation.SyntaxTrees.Count(), "Should have generated source files");
    }
}
```

## 🚀 Advanced Patterns

### Conditional Generation

```csharp
// Only generate when specific conditions are met
var conditionalPipeline = context.CompilationProvider.Select((compilation, _) =>
{
    // Check for specific using directives
    var hasUsingDirective = compilation.SyntaxTrees
        .SelectMany(st => st.GetRoot().DescendantNodes())
        .OfType<UsingDirectiveSyntax>()
        .Any(uds => uds.Name.ToString() == "YourProject.SpecialFeatures");
    
    // Check for specific attributes
    var hasAttribute = compilation.SyntaxTrees
        .SelectMany(st => st.GetRoot().DescendantNodes())
        .OfType<AttributeSyntax>()
        .Any(attr => attr.Name.ToString() == "EnableSpecialFeatures");
    
    return hasUsingDirective && hasAttribute ? compilation : null;
});
```

### Configuration-Based Generation

```csharp
public class YourCustomConfig
{
    public string Namespace { get; set; } = "Generated.Extensions";
    public bool IncludeAsyncMethods { get; set; } = true;
    public bool IncludeLogging { get; set; } = false;
    public int CacheTtlSeconds { get; set; } = 300;
}

// In your orchestrator
var configPipeline = context.AnalyzerConfigOptionsProvider
    .Select((options, _) =>
    {
        var config = new YourCustomConfig();
        
        if (options.GlobalOptions.TryGetValue("build_property.YourCustom.Namespace", out var ns))
            config.Namespace = ns;
            
        if (options.GlobalOptions.TryGetValue("build_property.YourCustom.IncludeAsync", out var async) && 
            bool.TryParse(async, out var includeAsync))
            config.IncludeAsyncMethods = includeAsync;
            
        return config;
    });
```

## 🔧 Best Practices

### ✅ DO

- **Follow SOLID principles** - Single responsibility for each component
- **Use dependency injection** - Makes your generators testable
- **Add comprehensive tests** - Unit tests for each component, integration tests for the whole pipeline
- **Use meaningful names** - Clear, descriptive names for generators, attributes, and methods
- **Document your code** - XML documentation for generated code
- **Handle edge cases** - Null checks, empty collections, invalid inputs

### ❌ DON'T

- **Generate duplicate code** - Check if code already exists
- **Hardcode values** - Use configuration and parameters
- **Ignore compilation errors** - Handle and report them gracefully
- **Generate too much code** - Be selective and efficient
- **Break existing functionality** - Test thoroughly before releasing

## 📦 Packaging Your Generator

### Project Structure

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>YourProject.SourceGenerators</PackageId>
    <PackageVersion>1.0.0</PackageVersion>
    <Description>Your custom source generators</Description>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.0.0" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.0" PrivateAssets="all" />
  </ItemGroup>

  <ItemGroup>
    <None Include="$(OutputPath)\$(AssemblyName).dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
  </ItemGroup>
</Project>
```

### Publishing to NuGet

```bash
# Build and pack
dotnet pack -c Release

# Publish to NuGet
dotnet nuget push YourProject.SourceGenerators.1.0.0.nupkg --source https://api.nuget.org/v3/index.json
```

## 🎯 Conclusion

Creating custom generators with REslava.Result.SourceGenerators is straightforward and powerful. By following the SOLID architecture and best practices outlined in this guide, you can create generators that:

- ✅ **Reduce boilerplate code** significantly
- ✅ **Improve developer productivity** 
- ✅ **Maintain type safety** and compile-time checking
- ✅ **Integrate seamlessly** with existing codebases
- ✅ **Follow established patterns** from the core library

The key is to start simple, test thoroughly, and iterate based on real-world usage. Happy generating! 🚀
