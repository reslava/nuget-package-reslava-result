using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.SourceGenerators.Generators.ResultToIResult;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace REslava.Result.SourceGenerators.Tests.UnitTests;

[TestClass]
public class ResultToIResultGeneratorTests
{
    private const string ResultTestSource = @"
using REslava.Result;

namespace TestNamespace
{
    public class TestController
    {
        // This should trigger generation of Result extension methods
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
    public async Task ResultToIResultGenerator_ShouldRun_WithoutErrors()
    {
        // Arrange
        var generator = new ResultToIResultRefactoredGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        // Create compilation with test source
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
        
        // Should generate extension methods
        Assert.IsTrue(generatorResult.GeneratedSources.Length > 0, "Should generate source files");
    }

    [TestMethod]
    public async Task ResultToIResultGenerator_ShouldInitialize_Correctly()
    {
        // Arrange & Act
        var generator = new ResultToIResultRefactoredGenerator();
        
        // Assert
        Assert.IsNotNull(generator, "Generator should be created successfully");
    }

    [TestMethod]
    public async Task ResultToIResultGenerator_ShouldHandleEmptyCompilation()
    {
        // Arrange
        var generator = new ResultToIResultRefactoredGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        // Create empty compilation
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
    public async Task ResultToIResultGenerator_ShouldGenerateExtensionMethods()
    {
        // Arrange
        var generator = new ResultToIResultRefactoredGenerator();
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
        
        // Should generate extension methods file
        var extensionFile = generatorResult.GeneratedSources
            .FirstOrDefault(f => f.HintName.Contains("ResultToIResultExtensions"));
        
        Assert.IsNotNull(extensionFile, "Should generate ResultToIResultExtensions file");
        
        // Verify generated content
        var generatedCode = extensionFile.SourceText.ToString();
        Assert.IsTrue(generatedCode.Contains("ToIResult"), "Should contain ToIResult method");
        Assert.IsTrue(generatedCode.Contains("ResultToIResultExtensions"), "Should contain extension class");
        Assert.IsTrue(generatedCode.Contains("public static IResult ToIResult<T>"), "Should contain generic extension method");
    }

    [TestMethod]
    public async Task ResultToIResultGenerator_ShouldGenerateAttributes()
    {
        // Arrange
        var generator = new ResultToIResultRefactoredGenerator();
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
        
        // Should generate attribute files
        var generateResultExtensionsAttribute = generatorResult.GeneratedSources
            .FirstOrDefault(f => f.HintName.Contains("GenerateResultExtensionsAttribute"));
        
        var mapToProblemDetailsAttribute = generatorResult.GeneratedSources
            .FirstOrDefault(f => f.HintName.Contains("MapToProblemDetailsAttribute"));
        
        Assert.IsNotNull(generateResultExtensionsAttribute, "Should generate GenerateResultExtensionsAttribute");
        Assert.IsNotNull(mapToProblemDetailsAttribute, "Should generate MapToProblemDetailsAttribute");
        
        // Verify attribute content
        var generateAttrCode = generateResultExtensionsAttribute.SourceText.ToString();
        Assert.IsTrue(generateAttrCode.Contains("GenerateResultExtensionsAttribute"), "Should contain attribute class");
        
        var mapAttrCode = mapToProblemDetailsAttribute.SourceText.ToString();
        Assert.IsTrue(mapAttrCode.Contains("MapToProblemDetailsAttribute"), "Should contain attribute class");
    }

    [TestMethod]
    public async Task ResultToIResultGenerator_ShouldHandleCompilationWithNoResultTypes()
    {
        // Arrange
        var generator = new ResultToIResultRefactoredGenerator();
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
        
        // Should still generate attributes (they're always generated)
        var attributeFiles = generatorResult.GeneratedSources
            .Where(f => f.HintName.Contains("Attribute"));
        
        Assert.IsTrue(attributeFiles.Count() >= 2, "Should generate attributes even without Result types");
    }
}
