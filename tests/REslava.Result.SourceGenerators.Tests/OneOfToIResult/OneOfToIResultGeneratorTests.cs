using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Generators.OneOfToIResult;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.SourceGenerators.Tests.OneOfToIResult;

[TestClass]
public class OneOfToIResultGeneratorTests
{
    #region OneOf2 Tests

    [TestMethod]
    public void OneOf2_Should_Generate_Extensions()
    {
        var source = CreateOneOf2Source();
        var output = RunGenerator(new OneOf2ToIResultGenerator(), source, includeOneOfRef: true);

        Assert.IsTrue(output.Contains("OneOf2Extensions"), "Should generate OneOf2Extensions class");
        Assert.IsTrue(output.Contains("ToIResult<T1, T2>"), "Should generate generic extension method");
        Assert.IsTrue(output.Contains("MapErrorToHttpResult"), "Should include error mapping helper");
    }

    [TestMethod]
    public void OneOf2_Should_Generate_Attributes()
    {
        var source = CreateOneOf2Source();
        var output = RunGenerator(new OneOf2ToIResultGenerator(), source, includeOneOfRef: true);

        Assert.IsTrue(output.Contains("GenerateOneOf2ExtensionsAttribute"), "Should generate attribute class");
        Assert.IsTrue(output.Contains("MapToProblemDetailsAttribute"), "Should generate mapping attribute");
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
        var output = RunGenerator(new OneOf2ToIResultGenerator(), source, includeOneOfRef: false);
        Assert.IsTrue(string.IsNullOrEmpty(output), "Should not generate without OneOf2 usage");
    }

    [TestMethod]
    public void OneOf2_Should_Handle_Different_Type_Combinations()
    {
        var sources = new[]
        {
            CreateOneOfSource(2, "ValidationError", "User"),
            CreateOneOfSource(2, "UserNotFoundError", "User"),
        };

        foreach (var source in sources)
        {
            var output = RunGenerator(new OneOf2ToIResultGenerator(), source, includeOneOfRef: true);
            Assert.IsTrue(output.Contains("OneOf2Extensions"), $"Should generate for: {source[..80]}...");
        }
    }

    #endregion

    #region OneOf3 Tests

    [TestMethod]
    public void OneOf3_Should_Generate_Extensions()
    {
        var source = CreateOneOf3Source();
        var output = RunGenerator(new OneOf3ToIResultGenerator(), source, includeOneOfRef: true);

        Assert.IsTrue(output.Contains("OneOf3Extensions"), "Should generate OneOf3Extensions class");
        Assert.IsTrue(output.Contains("ToIResult<T1, T2, T3>"), "Should generate 3-param extension method");
    }

    [TestMethod]
    public void OneOf3_Should_Generate_Attributes()
    {
        var source = CreateOneOf3Source();
        var output = RunGenerator(new OneOf3ToIResultGenerator(), source, includeOneOfRef: true);

        Assert.IsTrue(output.Contains("GenerateOneOf3ExtensionsAttribute"), "Should generate OneOf3 attribute");
    }

    [TestMethod]
    public void OneOf3_Should_Not_Generate_Without_Usage()
    {
        var source = @"
namespace TestNamespace
{
    public class TestClass { public void TestMethod() { } }
}";
        var output = RunGenerator(new OneOf3ToIResultGenerator(), source, includeOneOfRef: false);
        Assert.IsTrue(string.IsNullOrEmpty(output), "Should not generate without OneOf3 usage");
    }

    [TestMethod]
    public void OneOf3_Should_Handle_Different_Type_Combinations()
    {
        var sources = new[]
        {
            CreateOneOfSource(3, "ValidationError", "NotFoundError", "User"),
            CreateOneOfSource(3, "ValidationError", "ConflictError", "CreatedUser"),
        };

        foreach (var source in sources)
        {
            var output = RunGenerator(new OneOf3ToIResultGenerator(), source, includeOneOfRef: true);
            Assert.IsTrue(output.Contains("OneOf3Extensions"), $"Should generate for: {source[..80]}...");
        }
    }

    #endregion

    #region OneOf4 Tests

    [TestMethod]
    public void OneOf4_Should_Generate_Extensions()
    {
        var source = CreateOneOf4Source();
        var output = RunGenerator(new OneOf4ToIResultGenerator(), source, includeOneOfRef: true);

        Assert.IsTrue(output.Contains("OneOf4Extensions"), "Should generate OneOf4Extensions class");
        Assert.IsTrue(output.Contains("ToIResult<T1, T2, T3, T4>"), "Should generate 4-param extension method");
    }

    [TestMethod]
    public void OneOf4_Should_Generate_Attributes()
    {
        var source = CreateOneOf4Source();
        var output = RunGenerator(new OneOf4ToIResultGenerator(), source, includeOneOfRef: true);

        Assert.IsTrue(output.Contains("GenerateOneOf4ExtensionsAttribute"), "Should generate OneOf4 attribute");
        // MapToProblemDetailsAttribute is shared and only emitted by OneOf2 generator (tested in OneOf2_Should_Generate_Attributes)
    }

    [TestMethod]
    public void OneOf4_Should_Not_Generate_Without_Usage()
    {
        var source = @"
namespace TestNamespace
{
    public class TestClass { public void TestMethod() { } }
}";
        var output = RunGenerator(new OneOf4ToIResultGenerator(), source, includeOneOfRef: false);
        Assert.IsTrue(string.IsNullOrEmpty(output), "Should not generate without OneOf4 usage");
    }

    [TestMethod]
    public void OneOf4_Should_Handle_Multiple_Usages()
    {
        var source = @"
using REslava.Result.AdvancedPatterns;

namespace TestNamespace
{
    public class TestClass
    {
        public OneOf<ValidationError, NotFoundError, ConflictError, ServerError> Method1()
            => new ValidationError(""err1"");

        public OneOf<ValidationError, NotFoundError, ConflictError, ServerError> Method2()
            => new NotFoundError(""err2"");
    }
}
" + ErrorTypeStubs;

        var output = RunGenerator(new OneOf4ToIResultGenerator(), source, includeOneOfRef: true);
        Assert.IsTrue(output.Contains("OneOf4Extensions"), "Should generate for multiple usages");
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
public class UserNotFoundError : Error
{
    public UserNotFoundError(string message) : base(message) { }
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
public class CreatedUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
";

    private static string CreateOneOf2Source() =>
        CreateOneOfSource(2, "ValidationError", "User");

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
