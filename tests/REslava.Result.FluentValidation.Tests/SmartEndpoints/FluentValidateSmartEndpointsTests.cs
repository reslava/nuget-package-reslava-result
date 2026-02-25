using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.FluentValidation.Generators.FluentValidate;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints;

namespace REslava.Result.FluentValidation.Tests.SmartEndpoints;

/// <summary>
/// Tests for SmartEndpoints integration with [FluentValidate].
/// Both generators must run together so SmartEndpoints can see the FluentValidateAttribute.
/// </summary>
[TestClass]
public class FluentValidateSmartEndpointsTests
{
    // ── IValidator<T> injection ───────────────────────────────────────────────

    [TestMethod]
    public void SmartEndpoints_Should_Inject_IValidator_For_FluentValidate_Body_Param()
    {
        var output = RunBothGenerators(ServiceWithFluentValidateBody());

        Assert.IsTrue(output.Contains("IValidator<"),
            "Generated lambda should include IValidator<T> parameter");
    }

    [TestMethod]
    public void SmartEndpoints_Should_Inject_Correct_Validator_Type()
    {
        var output = RunBothGenerators(ServiceWithFluentValidateBody());

        Assert.IsTrue(output.Contains("IValidator<TestNamespace.CreateOrderRequest>") ||
                      output.Contains("IValidator<CreateOrderRequest>"),
            "Validator parameter should be typed IValidator<CreateOrderRequest>");
    }

    [TestMethod]
    public void SmartEndpoints_Should_Name_Validator_Param_With_Suffix()
    {
        var output = RunBothGenerators(ServiceWithFluentValidateBody());

        Assert.IsTrue(output.Contains("reqValidator") || output.Contains("Validator"),
            "Validator parameter should use '{paramName}Validator' naming");
    }

    // ── Validation block ─────────────────────────────────────────────────────

    [TestMethod]
    public void SmartEndpoints_Should_Emit_Validate_Call_With_Validator()
    {
        var output = RunBothGenerators(ServiceWithFluentValidateBody());

        Assert.IsTrue(output.Contains(".Validate(") && output.Contains("Validator)"),
            "Should emit req.Validate(reqValidator) block");
    }

    [TestMethod]
    public void SmartEndpoints_Should_Emit_IsSuccess_Guard()
    {
        var output = RunBothGenerators(ServiceWithFluentValidateBody());

        Assert.IsTrue(output.Contains("!validation.IsSuccess"),
            "Should emit early-return guard after validation");
    }

    [TestMethod]
    public void SmartEndpoints_Should_Emit_ToIResult_On_Failure()
    {
        var output = RunBothGenerators(ServiceWithFluentValidateBody());

        Assert.IsTrue(output.Contains("return validation.ToIResult()"),
            "Should return validation.ToIResult() on failure");
    }

    // ── Using statements ─────────────────────────────────────────────────────

    [TestMethod]
    public void SmartEndpoints_Should_Add_FluentValidation_Using()
    {
        var output = RunBothGenerators(ServiceWithFluentValidateBody());

        Assert.IsTrue(output.Contains("using FluentValidation;"),
            "Generated file should include using FluentValidation");
    }

    [TestMethod]
    public void SmartEndpoints_Should_Add_FluentValidationExtensions_Using()
    {
        var output = RunBothGenerators(ServiceWithFluentValidateBody());

        Assert.IsTrue(output.Contains("using Generated.FluentValidationExtensions;"),
            "Generated file should include using Generated.FluentValidationExtensions");
    }

    // ── No FluentValidation for GET params ───────────────────────────────────

    [TestMethod]
    public void SmartEndpoints_Should_Not_Inject_Validator_For_GET_Query_Params()
    {
        var source = @"
using REslava.Result.FluentValidation;
using REslava.Result.SourceGenerators.SmartEndpoints;
using REslava.Result;
using System.Threading.Tasks;
namespace TestNamespace
{
    [FluentValidate]
    public class SearchOrdersRequest { public string Status { get; set; } }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/orders"")]
    public class OrderService
    {
        public Task<Result<SearchOrdersRequest>> GetOrders(SearchOrdersRequest req)
            => Task.FromResult(Result<SearchOrdersRequest>.Ok(req));
    }
}";
        var output = RunBothGenerators(source);

        // GET params are not body params — no validator injection
        Assert.IsFalse(output.Contains("IValidator<") && output.Contains("reqValidator"),
            "GET query params should not get IValidator injection");
    }

    // ── Coexistence: [Validate] and [FluentValidate] on different types ───────

    [TestMethod]
    public void SmartEndpoints_Supports_Both_Validate_And_FluentValidate_On_Different_Types()
    {
        var source = @"
using REslava.Result.FluentValidation;
using REslava.Result.SourceGenerators;
using REslava.Result.SourceGenerators.SmartEndpoints;
using REslava.Result;
using System.Threading.Tasks;
namespace TestNamespace
{
    [Validate]
    public class CreateProductRequest { public string Name { get; set; } }

    [FluentValidate]
    public class CreateOrderRequest { public string CustomerId { get; set; } }

    [AutoGenerateEndpoints(RoutePrefix = ""/api"")]
    public class ComboService
    {
        public Task<Result<CreateProductRequest>> CreateProduct(CreateProductRequest req)
            => Task.FromResult(Result<CreateProductRequest>.Ok(req));

        public Task<Result<CreateOrderRequest>> CreateOrder(CreateOrderRequest req)
            => Task.FromResult(Result<CreateOrderRequest>.Ok(req));
    }
}";
        var output = RunBothGenerators(source, includeValidateGenerator: true);

        Assert.IsTrue(output.Contains("using Generated.ValidationExtensions;"),
            "Should include DataAnnotations validation using for [Validate] endpoint");
        Assert.IsTrue(output.Contains("using Generated.FluentValidationExtensions;"),
            "Should include FluentValidation using for [FluentValidate] endpoint");
    }

    #region Helpers

    private static string ServiceWithFluentValidateBody() => @"
using REslava.Result.FluentValidation;
using REslava.Result.SourceGenerators.SmartEndpoints;
using REslava.Result;
using System.Threading.Tasks;
namespace TestNamespace
{
    [FluentValidate]
    public class CreateOrderRequest
    {
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
    }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/orders"")]
    public class OrderService
    {
        public Task<Result<CreateOrderRequest>> CreateOrder(CreateOrderRequest req)
            => Task.FromResult(Result<CreateOrderRequest>.Ok(req));
    }
}";

    private static string RunBothGenerators(string source, bool includeValidateGenerator = false)
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

        // Run FluentValidateGenerator first so [FluentValidate] attribute is in compilation
        var fvGenerator = new FluentValidateGenerator();
        var smartGenerator = new SmartEndpointsGenerator();
        var validateGenerator = new REslava.Result.SourceGenerators.Generators.Validate.ValidateGenerator();

        ISourceGenerator[] generators = includeValidateGenerator
            ? new ISourceGenerator[] { fvGenerator.AsSourceGenerator(), validateGenerator.AsSourceGenerator(), smartGenerator.AsSourceGenerator() }
            : new ISourceGenerator[] { fvGenerator.AsSourceGenerator(), smartGenerator.AsSourceGenerator() };

        var driver = CSharpGeneratorDriver.Create(generators);
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
