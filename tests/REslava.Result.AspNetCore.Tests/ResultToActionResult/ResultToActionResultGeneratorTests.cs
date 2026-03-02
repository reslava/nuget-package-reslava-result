using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.SourceGenerators.Generators.ResultToActionResult;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace REslava.Result.SourceGenerators.Tests.ResultToActionResult;

[TestClass]
public class ResultToActionResultGeneratorTests
{
    private const string ResultTestSource = @"
using REslava.Result;

namespace TestNamespace
{
    public class TestController
    {
        public Result<string> GetSuccess()
        {
            return Result<string>.Ok(""Success"");
        }

        public Result<int> GetFailure()
        {
            return Result<int>.Fail(""Error occurred"");
        }

        public Result GetUser(int id)
        {
            if (id <= 0)
                return Result.Fail(""Invalid user ID"");

            return Result.Ok(new User { Id = id, Name = ""Test User"" });
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
";

    [TestMethod]
    public async Task ResultToActionResultGenerator_ShouldRun_WithoutErrors()
    {
        // Arrange
        var generator = new ResultToActionResultGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var syntaxTree = CSharpSyntaxTree.ParseText(ResultTestSource);
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.String).Assembly.Location)
            });

        // Act
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // Assert
        Assert.AreEqual(1, result.Results.Length, "Should have exactly one generator result");

        var generatorResult = result.Results[0];
        Assert.IsNotNull(generatorResult.Generator, "Should have a generator");
        Assert.IsTrue(generatorResult.Exception == null, "Generator should not throw exceptions");
        Assert.IsTrue(generatorResult.GeneratedSources.Length > 0, "Should generate source files");
    }

    [TestMethod]
    public async Task ResultToActionResultGenerator_ShouldInitialize_Correctly()
    {
        // Arrange & Act
        var generator = new ResultToActionResultGenerator();

        // Assert
        Assert.IsNotNull(generator, "Generator should be created successfully");
    }

    [TestMethod]
    public async Task ResultToActionResultGenerator_ShouldHandleEmptyCompilation()
    {
        // Arrange
        var generator = new ResultToActionResultGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var compilation = CSharpCompilation.Create(
            assemblyName: "EmptyAssembly",
            syntaxTrees: new SyntaxTree[0],
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            });

        // Act
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // Assert
        Assert.AreEqual(1, result.Results.Length, "Should have exactly one generator result");
        Assert.IsTrue(result.Results[0].Exception == null, "Should handle empty compilation without errors");
    }

    [TestMethod]
    public async Task ResultToActionResultGenerator_ShouldGenerateExtensionMethods()
    {
        // Arrange
        var generator = new ResultToActionResultGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var syntaxTree = CSharpSyntaxTree.ParseText(ResultTestSource);
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.String).Assembly.Location)
            });

        // Act
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // Assert
        var generatorResult = result.Results[0];

        var extensionFile = generatorResult.GeneratedSources
            .FirstOrDefault(f => f.HintName.Contains("ResultToActionResultExtensions"));

        Assert.IsNotNull(extensionFile, "Should generate ResultToActionResultExtensions file");

        var generatedCode = extensionFile.SourceText.ToString();
        Assert.IsTrue(generatedCode.Contains("ToActionResult"), "Should contain ToActionResult method");
        Assert.IsTrue(generatedCode.Contains("ResultToActionResultExtensions"), "Should contain extension class");
        Assert.IsTrue(generatedCode.Contains("public static IActionResult ToActionResult<T>"), "Should contain generic extension method");
        Assert.IsTrue(generatedCode.Contains("ToPostActionResult"), "Should contain ToPostActionResult method");
        Assert.IsTrue(generatedCode.Contains("ToPutActionResult"), "Should contain ToPutActionResult method");
        Assert.IsTrue(generatedCode.Contains("ToPatchActionResult"), "Should contain ToPatchActionResult method");
        Assert.IsTrue(generatedCode.Contains("ToDeleteActionResult"), "Should contain ToDeleteActionResult method");
        Assert.IsTrue(generatedCode.Contains("MapErrorToActionResult"), "Should contain MapErrorToActionResult helper");
    }

    [TestMethod]
    public async Task ResultToActionResultGenerator_ShouldGenerateExplicitOverload()
    {
        // Arrange
        var generator = new ResultToActionResultGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var syntaxTree = CSharpSyntaxTree.ParseText(ResultTestSource);
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.String).Assembly.Location)
            });

        // Act
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // Assert
        var extensionFile = result.Results[0].GeneratedSources
            .FirstOrDefault(f => f.HintName.Contains("ResultToActionResultExtensions"));

        Assert.IsNotNull(extensionFile, "Should generate extension file");

        var generatedCode = extensionFile.SourceText.ToString();
        Assert.IsTrue(generatedCode.Contains("Func<T, IActionResult> onSuccess"), "Should contain onSuccess parameter");
        Assert.IsTrue(generatedCode.Contains("Func<IReadOnlyList<IError>, IActionResult> onFailure"), "Should contain onFailure parameter");
    }

    [TestMethod]
    public async Task ResultToActionResultGenerator_ShouldGenerateCorrectMvcTypes()
    {
        // Arrange
        var generator = new ResultToActionResultGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var syntaxTree = CSharpSyntaxTree.ParseText(ResultTestSource);
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.String).Assembly.Location)
            });

        // Act
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // Assert
        var extensionFile = result.Results[0].GeneratedSources
            .FirstOrDefault(f => f.HintName.Contains("ResultToActionResultExtensions"));

        var generatedCode = extensionFile.SourceText.ToString();
        Assert.IsTrue(generatedCode.Contains("OkObjectResult"), "Should use OkObjectResult for success");
        Assert.IsTrue(generatedCode.Contains("NoContentResult"), "Should use NoContentResult for delete");
        Assert.IsTrue(generatedCode.Contains("CreatedResult"), "Should use CreatedResult for post");
        Assert.IsTrue(generatedCode.Contains("NotFoundObjectResult"), "Should use NotFoundObjectResult for 404");
        Assert.IsTrue(generatedCode.Contains("ConflictObjectResult"), "Should use ConflictObjectResult for 409");
        Assert.IsTrue(generatedCode.Contains("UnauthorizedResult"), "Should use UnauthorizedResult for 401");
        Assert.IsTrue(generatedCode.Contains("ForbidResult"), "Should use ForbidResult for 403");
    }

    [TestMethod]
    public async Task ResultToActionResultGenerator_ShouldGenerateAttributes()
    {
        // Arrange
        var generator = new ResultToActionResultGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var syntaxTree = CSharpSyntaxTree.ParseText(ResultTestSource);
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.String).Assembly.Location)
            });

        // Act
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // Assert
        var generatorResult = result.Results[0];

        var attributeFile = generatorResult.GeneratedSources
            .FirstOrDefault(f => f.HintName.Contains("GenerateActionResultExtensionsAttribute"));

        Assert.IsNotNull(attributeFile, "Should generate GenerateActionResultExtensionsAttribute");

        var attrCode = attributeFile.SourceText.ToString();
        Assert.IsTrue(attrCode.Contains("GenerateActionResultExtensionsAttribute"), "Should contain attribute class");
    }

    [TestMethod]
    public async Task ResultToActionResultGenerator_ShouldNotGenerateWhenNoResultTypes()
    {
        // Arrange
        var generator = new ResultToActionResultGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var sourceWithoutResult = @"
namespace TestNamespace
{
    public class TestController
    {
        public string GetRegularMethod()
        {
            return ""test"";
        }
    }
}
";

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceWithoutResult);
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            });

        // Act
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // Assert
        var generatorResult = result.Results[0];
        Assert.IsTrue(generatorResult.Exception == null, "Should handle compilation without Result types");

        var attributeFiles = generatorResult.GeneratedSources
            .Where(f => f.HintName.Contains("Attribute"));

        Assert.AreEqual(0, attributeFiles.Count(), "Should NOT generate attributes when no Result types are found");

        var extensionFiles = generatorResult.GeneratedSources
            .Where(f => f.HintName.Contains("Extensions"));

        Assert.AreEqual(0, extensionFiles.Count(), "Should NOT generate extensions when no Result types are found");
    }

    [TestMethod]
    public async Task ResultToActionResultGenerator_ShouldGenerateCorrectNamespace()
    {
        // Arrange
        var generator = new ResultToActionResultGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var syntaxTree = CSharpSyntaxTree.ParseText(ResultTestSource);
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.String).Assembly.Location)
            });

        // Act
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // Assert
        var extensionFile = result.Results[0].GeneratedSources
            .FirstOrDefault(f => f.HintName.Contains("ResultToActionResultExtensions"));

        var generatedCode = extensionFile.SourceText.ToString();
        Assert.IsTrue(generatedCode.Contains("namespace Generated.ActionResultExtensions"), "Should use correct namespace");
    }
}
