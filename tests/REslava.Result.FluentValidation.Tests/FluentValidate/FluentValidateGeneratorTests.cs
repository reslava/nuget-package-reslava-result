using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.FluentValidation.Generators.FluentValidate;

namespace REslava.Result.FluentValidation.Tests.FluentValidate;

[TestClass]
public class FluentValidateGeneratorTests
{
    // ── Attribute emission ───────────────────────────────────────────────────

    [TestMethod]
    public void Generator_Should_Emit_FluentValidateAttribute()
    {
        var output = RunGenerator(EmptySource());

        Assert.IsTrue(output.Contains("FluentValidateAttribute"),
            "Attribute source should always be emitted");
        Assert.IsTrue(output.Contains("namespace REslava.Result.FluentValidation"),
            "Attribute should be in REslava.Result.FluentValidation namespace");
    }

    [TestMethod]
    public void Generator_Attribute_Should_Target_Class_And_Struct()
    {
        var output = RunGenerator(EmptySource());

        Assert.IsTrue(output.Contains("AttributeTargets.Class | AttributeTargets.Struct"),
            "Attribute should target Class and Struct");
    }

    // ── Extension method generation ──────────────────────────────────────────

    [TestMethod]
    public void Generator_Should_Generate_Extensions_Class_For_Decorated_Type()
    {
        var output = RunGenerator(FluentValidatedSource("CreateOrderRequest"));

        Assert.IsTrue(output.Contains("CreateOrderRequestFluentValidationExtensions"),
            "Should generate named extensions class");
    }

    [TestMethod]
    public void Generator_Should_Generate_In_Correct_Namespace()
    {
        var output = RunGenerator(FluentValidatedSource("CreateOrderRequest"));

        Assert.IsTrue(output.Contains("Generated.FluentValidationExtensions"),
            "Should use Generated.FluentValidationExtensions namespace");
    }

    [TestMethod]
    public void Generator_Should_Generate_Sync_Validate_Method()
    {
        var output = RunGenerator(FluentValidatedSource("CreateOrderRequest"));

        Assert.IsTrue(output.Contains("public static Result<"),
            "Should generate static Result<T> method");
        Assert.IsTrue(output.Contains("Validate("),
            "Should generate Validate method");
        Assert.IsTrue(output.Contains("IValidator<"),
            "Validate should accept IValidator<T> parameter");
    }

    [TestMethod]
    public void Generator_Should_Generate_Async_ValidateAsync_Method()
    {
        var output = RunGenerator(FluentValidatedSource("CreateOrderRequest"));

        Assert.IsTrue(output.Contains("ValidateAsync("),
            "Should generate ValidateAsync overload");
        Assert.IsTrue(output.Contains("Task<Result<"),
            "ValidateAsync should return Task<Result<T>>");
        Assert.IsTrue(output.Contains("CancellationToken cancellationToken = default"),
            "ValidateAsync should accept optional CancellationToken");
    }

    [TestMethod]
    public void Generator_Sync_Should_Call_Validator_Validate()
    {
        var output = RunGenerator(FluentValidatedSource("CreateOrderRequest"));

        Assert.IsTrue(output.Contains("validator.Validate(instance)"),
            "Sync method should call validator.Validate(instance)");
    }

    [TestMethod]
    public void Generator_Async_Should_Call_ValidateAsync()
    {
        var output = RunGenerator(FluentValidatedSource("CreateOrderRequest"));

        Assert.IsTrue(output.Contains("await validator.ValidateAsync(instance, cancellationToken)"),
            "Async method should await validator.ValidateAsync");
    }

    [TestMethod]
    public void Generator_Should_Return_Ok_When_Valid()
    {
        var output = RunGenerator(FluentValidatedSource("CreateOrderRequest"));

        Assert.IsTrue(output.Contains("result.IsValid"),
            "Should check result.IsValid");
        Assert.IsTrue(output.Contains(".Ok(instance)"),
            "Should return Ok(instance) on success");
    }

    [TestMethod]
    public void Generator_Should_Return_Fail_With_ValidationErrors()
    {
        var output = RunGenerator(FluentValidatedSource("CreateOrderRequest"));

        Assert.IsTrue(output.Contains(".Fail("),
            "Should return Fail on invalid");
        Assert.IsTrue(output.Contains("ValidationError"),
            "Should create ValidationError instances");
    }

    [TestMethod]
    public void Generator_Should_Map_PropertyName_And_ErrorMessage()
    {
        var output = RunGenerator(FluentValidatedSource("CreateOrderRequest"));

        Assert.IsTrue(output.Contains("e.PropertyName"),
            "Should use PropertyName as field name");
        Assert.IsTrue(output.Contains("e.ErrorMessage"),
            "Should use ErrorMessage as error message");
    }

    [TestMethod]
    public void Generator_Should_Include_FluentValidation_Using()
    {
        var output = RunGenerator(FluentValidatedSource("CreateOrderRequest"));

        Assert.IsTrue(output.Contains("using FluentValidation;"),
            "Generated file should include using FluentValidation");
    }

    [TestMethod]
    public void Generator_Should_Include_REslavaResult_Using()
    {
        var output = RunGenerator(FluentValidatedSource("CreateOrderRequest"));

        Assert.IsTrue(output.Contains("using REslava.Result;"),
            "Generated file should include using REslava.Result");
    }

    // ── No generation without attribute ──────────────────────────────────────

    [TestMethod]
    public void Generator_Should_Not_Generate_Extension_Without_Attribute()
    {
        var source = @"
namespace TestNamespace
{
    public class CreateOrderRequest
    {
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
    }
}";
        var output = RunGenerator(source);

        Assert.IsFalse(output.Contains("CreateOrderRequestFluentValidationExtensions"),
            "Should not generate extension for non-decorated class");
    }

    // ── Multiple types ───────────────────────────────────────────────────────

    [TestMethod]
    public void Generator_Should_Generate_Extensions_For_Multiple_Decorated_Types()
    {
        var source = @"
using REslava.Result.FluentValidation;
namespace TestNamespace
{
    [FluentValidate]
    public class CreateOrderRequest { }

    [FluentValidate]
    public class UpdateOrderRequest { }
}";
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("CreateOrderRequestFluentValidationExtensions"),
            "Should generate extension for CreateOrderRequest");
        Assert.IsTrue(output.Contains("UpdateOrderRequestFluentValidationExtensions"),
            "Should generate extension for UpdateOrderRequest");
    }

    // ── Record support ───────────────────────────────────────────────────────

    [TestMethod]
    public void Generator_Should_Generate_For_Record_Types()
    {
        var source = @"
using REslava.Result.FluentValidation;
namespace TestNamespace
{
    [FluentValidate]
    public record CreateOrderRequest(string CustomerId, decimal Amount);
}";
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("CreateOrderRequestFluentValidationExtensions"),
            "Should generate extension for record types");
    }

    #region Helpers

    private static string EmptySource() => @"namespace TestNamespace { }";

    private static string FluentValidatedSource(string typeName) => $@"
using REslava.Result.FluentValidation;
namespace TestNamespace
{{
    [FluentValidate]
    public class {typeName}
    {{
        public string CustomerId {{ get; set; }}
        public decimal Amount {{ get; set; }}
    }}
}}";

    private static string RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new FluentValidateGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        var runResult = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var generatedTrees = runResult.GetRunResult().GeneratedTrees;

        if (generatedTrees.IsEmpty)
            return string.Empty;

        var sb = new System.Text.StringBuilder();
        foreach (var tree in generatedTrees)
        {
            using var writer = new System.IO.StringWriter();
            tree.GetText().Write(writer);
            sb.AppendLine(writer.ToString());
        }

        return sb.ToString();
    }

    #endregion
}
