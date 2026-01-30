## **Quick Integration Guide**

### **Step 1: Add Package Reference**

In our generator project:

```xml
<ItemGroup>
  <PackageReference Include="REslava.Result.SourceGenerators.Core" Version="1.0.0" />
</ItemGroup>

```

### **Step 2: Create Configuration**

```csharp
using REslava.Result.SourceGenerators.Core.Configuration;

public class MyGeneratorConfig : GeneratorConfigurationBase
{
    public bool IncludeErrorTags { get; set; } = true;
    public string[] CustomMappings { get; set; } = Array.Empty<string>();

    public override bool Validate()
    {
        return base.Validate() && CustomMappings != null;
    }
}

```

### **Step 3: Implement Generator**

```csharp
using REslava.Result.SourceGenerators.Core.Infrastructure;
using REslava.Result.SourceGenerators.Core.CodeGeneration;

[Generator]
public class MyGenerator : IncrementalGeneratorBase<MyGeneratorConfig>
{
    protected override string AttributeFullName =>
        "MyNamespace.GenerateMyCodeAttribute";

    protected override string AttributeShortName => "GenerateMyCode";
    protected override string GeneratedFileName => "MyExtensions";
    protected override string AttributeSourceCode => "..."; // Your attribute

    protected override string GenerateCode(
        Compilation compilation,
        MyGeneratorConfig config)
    {
        var builder = new CodeBuilder();

        builder.AppendFileHeader("MyGenerator")
            .AppendUsings("System", "System.Linq")
            .AppendNamespace(config.Namespace)
            .AppendClassDeclaration("MyClass", modifiers: "public", "static")
            .AppendLine("// Your code here")
            .CloseBrace()
            .CloseNamespace();

        return builder.ToString();
    }
}

```

---

## **🎯 Refactoring Current Generator**

Here's how to refactor `ResultToIResultGenerator.cs` to use the Core:

### **Before (Current Implementation)**

```csharp
[Generator]
public class ResultToIResultGenerator : IIncrementalGenerator
{
    private const string AttributeName = "...";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register attribute
        context.RegisterPostInitializationOutput(ctx => { ... });

        // Find assemblies with attribute
        var assemblyAttributes = context.CompilationProvider.Select(...);

        // Generate code
        context.RegisterSourceOutput(assemblyAttributes, (spc, data) => { ... });
    }

    private GeneratorConfiguration ParseConfiguration(AttributeData attribute) { ... }
    private string GenerateExtensionMethods(GeneratorConfiguration config) { ... }
}

```

### **After (Using Core)**

```csharp
using REslava.Result.SourceGenerators.Core.Infrastructure;
using REslava.Result.SourceGenerators.Core.CodeGeneration;

[Generator]
public class ResultToIResultGenerator : IncrementalGeneratorBase<ResultToIResultConfig>
{
    protected override string AttributeFullName =>
        "REslava.Result.SourceGenerators.GenerateResultExtensionsAttribute";

    protected override string AttributeShortName => "GenerateResultExtensions";
    protected override string GeneratedFileName => "ResultToIResultExtensions";
    protected override string AttributeSourceCode => AttributeSource;

    protected override void ParseAdditionalConfiguration(
        AttributeData attribute,
        ResultToIResultConfig config)
    {
        var args = attribute.NamedArguments;
        config.IncludeErrorTags = args.GetBoolValue("IncludeErrorTags", true);
        config.CustomErrorMappings = args.GetStringArrayValue("CustomErrorMappings");
        config.GenerateHttpMethodExtensions = args.GetBoolValue("GenerateHttpMethodExtensions", true);
        config.DefaultErrorStatusCode = args.GetIntValue("DefaultErrorStatusCode", 400);
    }

    protected override string GenerateCode(Compilation compilation, ResultToIResultConfig config)
    {
        var builder = new CodeBuilder();
        var mapper = new HttpStatusCodeMapper(config.DefaultErrorStatusCode);
        mapper.AddMappings(config.CustomErrorMappings);

        builder.AppendFileHeader("ResultToIResultGenerator")
            .AppendUsings(
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "Microsoft.AspNetCore.Http",
                "Microsoft.AspNetCore.Mvc",
                "REslava.Result")
            .AppendNamespace(config.Namespace);

        GenerateMainExtensionClass(builder, config, mapper);

        if (config.GenerateHttpMethodExtensions)
        {
            builder.BlankLine();
            GenerateHttpMethodExtensions(builder);
        }

        builder.CloseNamespace();
        return builder.ToString();
    }

    private void GenerateMainExtensionClass(
        CodeBuilder builder,
        ResultToIResultConfig config,
        HttpStatusCodeMapper mapper)
    {
        builder.AppendXmlSummary("Extension methods for converting Result<T> to IResult.")
            .AppendClassDeclaration("ResultToIResultExtensions", modifiers: "public", "static");

        // Generate ToIResult method
        builder.AppendXmlSummary("Converts Result<T> to IResult.")
            .AppendMethodDeclaration(
                "ToIResult",
                "IResult",
                "this Result<T> result",
                "public", "static")
            .AppendLine("if (result.IsSuccess)")
            .OpenBrace()
            .AppendLine("return Results.Ok(result.Value);")
            .CloseBrace()
            .BlankLine()
            .AppendLine("var statusCode = DetermineStatusCode(result.Errors);")
            .AppendLine("var problemDetails = CreateProblemDetails(result.Errors, statusCode);")
            .AppendLine("return Results.Problem(problemDetails);")
            .CloseBrace(); // method

        builder.CloseBrace(); // class
    }
}

```