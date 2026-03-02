using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Generators.Validate;

namespace REslava.Result.SourceGenerators.Tests.Validate;

[TestClass]
public class ValidateGeneratorTests
{
    [TestMethod]
    public void Validate_Should_Generate_Extension_Method()
    {
        var source = CreateValidatedSource("CreateProductRequest");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("CreateProductRequestValidationExtensions"),
            "Should generate named extensions class");
        Assert.IsTrue(output.Contains("Validate(this"),
            "Should generate Validate extension method");
    }

    [TestMethod]
    public void Validate_Should_Generate_In_Correct_Namespace()
    {
        var source = CreateValidatedSource("CreateProductRequest");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Generated.ValidationExtensions"),
            "Should use Generated.ValidationExtensions namespace");
    }

    [TestMethod]
    public void Validate_Should_Use_Validator_TryValidateObject()
    {
        var source = CreateValidatedSource("CreateProductRequest");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Validator.TryValidateObject"),
            "Should delegate to Validator.TryValidateObject");
        Assert.IsTrue(output.Contains("validateAllProperties: true"),
            "Should validate all properties");
    }

    [TestMethod]
    public void Validate_Should_Return_Result_Ok_On_Success()
    {
        var source = CreateValidatedSource("CreateProductRequest");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Result<"),
            "Should return Result<T>");
        Assert.IsTrue(output.Contains(".Ok(instance)"),
            "Should return Ok(instance) on success");
    }

    [TestMethod]
    public void Validate_Should_Return_Result_Fail_With_ValidationErrors()
    {
        var source = CreateValidatedSource("CreateProductRequest");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains(".Fail("),
            "Should return Fail on validation failure");
        Assert.IsTrue(output.Contains("ValidationError"),
            "Should create ValidationError instances");
    }

    [TestMethod]
    public void Validate_Should_Map_MemberNames_To_FieldName()
    {
        var source = CreateValidatedSource("CreateProductRequest");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("MemberNames"),
            "Should use MemberNames from ValidationResult for FieldName");
    }

    [TestMethod]
    public void Validate_ClassWithout_Attribute_Should_Not_Generate_Extension()
    {
        var source = @"
namespace TestNamespace
{
    public class ProductRequest
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}";
        var output = RunGenerator(source);

        Assert.IsFalse(output.Contains("ProductRequestValidationExtensions"),
            "Should not generate extension for non-decorated class");
    }

    #region Helpers

    private static string CreateValidatedSource(string typeName) => $@"
namespace TestNamespace
{{
    [Validate]
    public class {typeName}
    {{
        public string Name {{ get; set; }}
        public decimal Price {{ get; set; }}
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

        var generator = new ValidateGenerator();
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
