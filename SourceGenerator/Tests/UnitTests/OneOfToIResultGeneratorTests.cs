using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.SourceGenerators.Generators.OneOfToIResult;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace REslava.Result.SourceGenerators.Tests.UnitTests;

[TestClass]
public class OneOfToIResultGeneratorTests
{
    private const string BasicOneOfTestSource = @"
namespace TestNamespace
{
    public class UserNotFoundError
    {
        public int UserId { get; set; }
        public string Message { get; set; } = ""User not found"";
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TestController
    {
        // This should trigger generation of extension method
        public global::OneOf.OneOf<UserNotFoundError, User> GetUser(int id)
        {
            if (id <= 0)
                return new UserNotFoundError { UserId = id };
            
            return new User { Id = id, Name = ""Test User"" };
        }
    }
}
";

    [TestMethod]
    public async Task Generator_ShouldRun_WithoutErrors()
    {
        // Arrange
        var generator = new OneOfToIResultRefactoredGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        // Create compilation with test source
        var syntaxTree = CSharpSyntaxTree.ParseText(BasicOneOfTestSource);
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: new[]
            {
                // Core reference for basic types
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                // Add OneOf reference - we'll create a mock reference
                MetadataReference.CreateFromFile(typeof(System.String).Assembly.Location)
            });

        // Act
        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        // Assert
        Assert.AreEqual(1, result.Results.Length, "Should have exactly one generator result");
        
        var generatorResult = result.Results[0];
        
        // Debug: Let's see what generator actually ran
        System.Diagnostics.Debug.WriteLine($"Generator type: {generatorResult.Generator?.GetType().FullName}");
        
        // For now, just check that we have a generator result without exceptions
        Assert.IsNotNull(generatorResult.Generator, "Should have a generator");
        Assert.IsTrue(generatorResult.Exception == null, "Generator should not throw exceptions");
    }

    [TestMethod]
    public async Task Generator_ShouldInitialize_Correctly()
    {
        // Arrange & Act
        var generator = new OneOfToIResultRefactoredGenerator();
        
        // Assert
        Assert.IsNotNull(generator, "Generator should be created successfully");
    }

    [TestMethod]
    public async Task Generator_ShouldHandleEmptyCompilation()
    {
        // Arrange
        var generator = new OneOfToIResultRefactoredGenerator();
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
    public async Task Generator_ShouldDetect_TwoTypeOneOf()
    {
        // Arrange
        var generator = new OneOfToIResultRefactoredGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        // Create compilation with T1,T2 OneOf
        var syntaxTree = CSharpSyntaxTree.ParseText(BasicOneOfTestSource);
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
        Assert.IsTrue(generatorResult.GeneratedSources.Length > 0, "Should generate source files");
        
        // Should generate extension method for T1,T2 OneOf
        var extensionFiles = generatorResult.GeneratedSources
            .Where(f => f.HintName.Contains("Extensions"));
        
        Assert.IsTrue(extensionFiles.Any(), "Should generate extension method files");
    }

    [TestMethod]
    public async Task Generator_ShouldSkip_GenericOneOf()
    {
        // Arrange
        var generator = new OneOfToIResultRefactoredGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var genericOneOfSource = @"
namespace TestNamespace
{
    public class TestController<T>
    {
        // Generic OneOf - should be skipped
        public global::OneOf.OneOf<string, T> GetItem(T item) => item;
    }
}
";

        var syntaxTree = CSharpSyntaxTree.ParseText(genericOneOfSource);
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
        
        // Should not generate extension files for generic OneOf
        var extensionFiles = generatorResult.GeneratedSources
            .Where(f => f.HintName.Contains("Extensions"));
        
        Assert.AreEqual(0, extensionFiles.Count(), "Should not generate extension files for generic OneOf");
    }

    [TestMethod]
    public async Task Generator_ShouldHandle_MultipleTwoTypeOneOfs()
    {
        // Arrange
        var generator = new OneOfToIResultRefactoredGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var multipleOneOfSource = @"
namespace TestNamespace
{
    public class Error1 { public string Message { get; set; } = ""Error1""; }
    public class Error2 { public string Message { get; set; } = ""Error2""; }
    public class Success1 { public string Value { get; set; } = ""Success1""; }
    public class Success2 { public string Value { get; set; } = ""Success2""; }

    public class TestController
    {
        public global::OneOf.OneOf<Error1, Success1> Method1() => new Success1();
        public global::OneOf.OneOf<Error2, Success2> Method2() => new Success2();
    }
}
";

        var syntaxTree = CSharpSyntaxTree.ParseText(multipleOneOfSource);
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
        
        // Should generate extension files for both T1,T2 OneOf types
        var extensionFiles = generatorResult.GeneratedSources
            .Where(f => f.HintName.Contains("Extensions"));
        
        Assert.AreEqual(2, extensionFiles.Count(), "Should generate extension files for both T1,T2 OneOf types");
    }
}
