using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Generators.OneOfToActionResult;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.SourceGenerators.Tests.OneOfToActionResult;

[TestClass]
public class OneOfToActionResultGeneratorTests
{
    #region OneOf2 Tests

    [TestMethod]
    public void OneOf2_Should_Generate_ActionResult_Extensions()
    {
        var source = CreateOneOf2Source();
        var output = RunGenerator(new OneOf2ToActionResultGenerator(), source, includeOneOfRef: true);

        Assert.IsTrue(output.Contains("OneOf2ActionResultExtensions"), "Should generate OneOf2ActionResultExtensions class");
        Assert.IsTrue(output.Contains("ToActionResult<T1, T2>"), "Should generate generic extension method");
        Assert.IsTrue(output.Contains("MapErrorToActionResult"), "Should include error mapping helper");
    }

    [TestMethod]
    public void OneOf2_Should_Generate_Attributes()
    {
        var source = CreateOneOf2Source();
        var output = RunGenerator(new OneOf2ToActionResultGenerator(), source, includeOneOfRef: true);

        Assert.IsTrue(output.Contains("GenerateOneOf2ActionResultExtensionsAttribute"), "Should generate attribute class");
    }

    [TestMethod]
    public void OneOf2_Should_Not_Generate_Without_Usage()
    {
        var source = @"
namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod() { var x = 42; }
    }
}";
        var output = RunGenerator(new OneOf2ToActionResultGenerator(), source, includeOneOfRef: false);
        Assert.IsTrue(string.IsNullOrEmpty(output), "Should not generate without OneOf2 usage");
    }

    [TestMethod]
    public void OneOf2_Should_Generate_MVC_Result_Types()
    {
        var source = CreateOneOf2Source();
        var output = RunGenerator(new OneOf2ToActionResultGenerator(), source, includeOneOfRef: true);

        Assert.IsTrue(output.Contains("OkObjectResult"), "Should use OkObjectResult for success");
        Assert.IsTrue(output.Contains("NotFoundObjectResult"), "Should use NotFoundObjectResult for 404");
        Assert.IsTrue(output.Contains("ConflictObjectResult"), "Should use ConflictObjectResult for 409");
        Assert.IsTrue(output.Contains("UnauthorizedResult"), "Should use UnauthorizedResult for 401");
        Assert.IsTrue(output.Contains("ForbidResult"), "Should use ForbidResult for 403");
    }

    #endregion

    #region OneOf3 Tests

    [TestMethod]
    public void OneOf3_Should_Generate_ActionResult_Extensions()
    {
        var source = CreateOneOf3Source();
        var output = RunGenerator(new OneOf3ToActionResultGenerator(), source, includeOneOfRef: true);

        Assert.IsTrue(output.Contains("OneOf3ActionResultExtensions"), "Should generate OneOf3ActionResultExtensions class");
        Assert.IsTrue(output.Contains("ToActionResult<T1, T2, T3>"), "Should generate 3-param extension method");
    }

    [TestMethod]
    public void OneOf3_Should_Generate_Attributes()
    {
        var source = CreateOneOf3Source();
        var output = RunGenerator(new OneOf3ToActionResultGenerator(), source, includeOneOfRef: true);

        Assert.IsTrue(output.Contains("GenerateOneOf3ActionResultExtensionsAttribute"), "Should generate OneOf3 attribute");
    }

    [TestMethod]
    public void OneOf3_Should_Not_Generate_Without_Usage()
    {
        var source = @"
namespace TestNamespace
{
    public class TestClass { public void TestMethod() { } }
}";
        var output = RunGenerator(new OneOf3ToActionResultGenerator(), source, includeOneOfRef: false);
        Assert.IsTrue(string.IsNullOrEmpty(output), "Should not generate without OneOf3 usage");
    }

    #endregion

    #region OneOf4 Tests

    [TestMethod]
    public void OneOf4_Should_Generate_ActionResult_Extensions()
    {
        var source = CreateOneOf4Source();
        var output = RunGenerator(new OneOf4ToActionResultGenerator(), source, includeOneOfRef: true);

        Assert.IsTrue(output.Contains("OneOf4ActionResultExtensions"), "Should generate OneOf4ActionResultExtensions class");
        Assert.IsTrue(output.Contains("ToActionResult<T1, T2, T3, T4>"), "Should generate 4-param extension method");
    }

    [TestMethod]
    public void OneOf4_Should_Generate_Attributes()
    {
        var source = CreateOneOf4Source();
        var output = RunGenerator(new OneOf4ToActionResultGenerator(), source, includeOneOfRef: true);

        Assert.IsTrue(output.Contains("GenerateOneOf4ActionResultExtensionsAttribute"), "Should generate OneOf4 attribute");
    }

    [TestMethod]
    public void OneOf4_Should_Not_Generate_Without_Usage()
    {
        var source = @"
namespace TestNamespace
{
    public class TestClass { public void TestMethod() { } }
}";
        var output = RunGenerator(new OneOf4ToActionResultGenerator(), source, includeOneOfRef: false);
        Assert.IsTrue(string.IsNullOrEmpty(output), "Should not generate without OneOf4 usage");
    }

    [TestMethod]
    public void OneOf4_Should_Generate_Correct_Namespace()
    {
        var source = CreateOneOf4Source();
        var output = RunGenerator(new OneOf4ToActionResultGenerator(), source, includeOneOfRef: true);

        Assert.IsTrue(output.Contains("namespace Generated.OneOfActionResultExtensions"),
            "Should use Generated.OneOfActionResultExtensions namespace");
    }

    [TestMethod]
    public void OneOf4_Should_Check_IError_Tags()
    {
        var source = CreateOneOf4Source();
        var output = RunGenerator(new OneOf4ToActionResultGenerator(), source, includeOneOfRef: true);

        Assert.IsTrue(output.Contains("IError iError"), "Should check IError interface for tag-based mapping");
        Assert.IsTrue(output.Contains("HttpStatusCode"), "Should read HttpStatusCode tag");
    }

    #endregion

    #region Helpers

    private const string ErrorTypeStubs = @"
public class Error
{
    public string Message { get; }
    public Error(string message) { Message = message; }
}
public class ValidationError : Error
{
    public ValidationError(string message) : base(message) { }
}
public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message) { }
}
public class ConflictError : Error
{
    public ConflictError(string message) : base(message) { }
}
public class ServerError : Error
{
    public ServerError(string message) : base(message) { }
}
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
";

    private static string CreateOneOf2Source() =>
        CreateOneOfSource(2, "NotFoundError", "User");

    private static string CreateOneOf3Source() =>
        CreateOneOfSource(3, "ValidationError", "NotFoundError", "User");

    private static string CreateOneOf4Source() =>
        CreateOneOfSource(4, "ValidationError", "NotFoundError", "ConflictError", "ServerError");

    private static string CreateOneOfSource(int arity, params string[] typeArgs)
    {
        var typeList = string.Join(", ", typeArgs);
        return $@"
using REslava.Result.AdvancedPatterns;

namespace TestNamespace
{{
    public class TestClass
    {{
        public OneOf<{typeList}> TestMethod()
        {{
            return new {typeArgs[0]}(""test error"");
        }}
    }}
}}
" + ErrorTypeStubs;
    }

    private static string RunGenerator(IIncrementalGenerator generator, string source, bool includeOneOfRef)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        };

        if (includeOneOfRef)
        {
            references.Add(MetadataReference.CreateFromFile(typeof(OneOf<,>).Assembly.Location));
        }

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

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
