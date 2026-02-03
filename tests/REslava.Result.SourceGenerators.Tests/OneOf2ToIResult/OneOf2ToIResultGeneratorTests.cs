using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using REslava.Result.SourceGenerators.Generators.OneOf2ToIResult;

namespace REslava.Result.SourceGenerators.Tests.OneOf2ToIResult;

[TestClass]
public class OneOf2ToIResultGeneratorTests
{
    private const string TestSourceTemplate = @"
using System;
using REslava.Result.AdvancedPatterns;

namespace TestNamespace
{{
    public class TestClass
    {{
        public void TestMethod()
        {{
            // This should trigger the OneOf2ToIResult generator
            OneOf<{T1}, {T2}> result = {Initialization};
            var iResult = result.ToIResult();
        }}
    }}
}}

// Test classes
public class ValidationError 
{{
    public string Field {{ get; set; }} = string.Empty;
    public string Message {{ get; set; }} = string.Empty;
    public ValidationError(string field, string message) 
    {{
        Field = field;
        Message = message;
    }}
}}

public class UserNotFoundError 
{{
    public int Id {{ get; set; }}
    public UserNotFoundError(int id) {{ Id = id; }}
}}

public class User 
{{
    public int Id {{ get; set; }}
    public string Name {{ get; set; }} = string.Empty;
    public User(int id, string name) {{ Id = id; Name = name; }}
}}

public class CreatedUser 
{{
    public int Id {{ get; set; }}
    public string Name {{ get; set; }} = string.Empty;
    public DateTime CreatedAt {{ get; set; }}
    public CreatedUser(int id, string name, DateTime createdAt) 
    {{
        Id = id;
        Name = name;
        CreatedAt = createdAt;
    }}
}}";

    [TestMethod]
    public async Task Generator_Should_Generate_Extensions_For_OneOf2()
    {
        // Arrange
        var source = @"
using System;
using REslava.Result.AdvancedPatterns;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod()
        {
            // This should trigger the OneOf2ToIResult generator
            OneOf<ValidationError, User> result = new ValidationError(""test"", ""test error"");
            var iResult = result.ToIResult();
        }
    }
}

// Test classes
public class ValidationError 
{
    public string Field = string.Empty;
    public string Message = string.Empty;
    public ValidationError(string field, string message) 
    {
        Field = field;
        Message = message;
    }
}

public class User 
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public User(int id, string name) { Id = id; Name = name; }
}";

        // Act
        var generatedOutput = await RunGenerator(source);

        // Assert
        Assert.IsNotNull(generatedOutput, "Generator should produce output");
        Assert.IsTrue(generatedOutput.Contains("OneOfExtensions"), "Should generate extension class");
        Assert.IsTrue(generatedOutput.Contains("ToIResult<T1, T2>"), "Should generate generic extension method");
        Assert.IsTrue(generatedOutput.Contains("Results.BadRequest"), "Should map T1 to BadRequest");
        Assert.IsTrue(generatedOutput.Contains("Results.Ok"), "Should map T2 to Ok");
    }

    [TestMethod]
    public async Task Generator_Should_Generate_Attributes()
    {
        // Arrange
        var source = TestSourceTemplate
            .Replace("{T1}", "ValidationError")
            .Replace("{T2}", "User")
            .Replace("{Initialization}", "new ValidationError(\"test\", \"test error\")");

        // Act
        var generatedOutput = await RunGenerator(source);

        // Assert
        Assert.IsNotNull(generatedOutput, "Generator should produce output");
        Assert.IsTrue(generatedOutput.Contains("GenerateOneOf2ExtensionsAttribute"), "Should generate attribute class");
        Assert.IsTrue(generatedOutput.Contains("MapToProblemDetailsAttribute"), "Should generate mapping attribute");
    }

    [TestMethod]
    public async Task Generator_Should_Handle_Different_Type_Combinations()
    {
        // Test different type combinations
        var testCases = new[]
        {
            ("ValidationError", "CreatedUser", "new ValidationError(\"field\", \"error\")"),
            ("UserNotFoundError", "User", "new UserNotFoundError(1)"),
            ("ValidationError", "CreatedUser", "new CreatedUser(1, \"test\", DateTime.UtcNow)")
        };

        foreach (var (t1, t2, initialization) in testCases)
        {
            // Arrange
            var source = TestSourceTemplate
                .Replace("{T1}", t1)
                .Replace("{T2}", t2)
                .Replace("{Initialization}", initialization);

            // Act
            var generatedOutput = await RunGenerator(source);

            // Assert
            Assert.IsNotNull(generatedOutput, $"Generator should produce output for {t1}, {t2}");
            Assert.IsTrue(generatedOutput.Contains("OneOfExtensions"), $"Should generate extensions for {t1}, {t2}");
        }
    }

    [TestMethod]
    public async Task Generator_Should_Not_Generate_Without_OneOf2_Usage()
    {
        // Arrange - source without OneOf2 usage
        var source = @"
using System;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod()
        {
            // No OneOf2 usage - should not trigger generator
            var result = ""test"";
            // Using different namespace - should not trigger
            var oneOfResult = System.Collections.Generic.List<int>.Empty;
        }
    }
}";

        // Act
        var generatedOutput = await RunGenerator(source);

        // Assert
        Assert.IsTrue(string.IsNullOrEmpty(generatedOutput), "Generator should not produce output without OneOf2 usage");
    }

    private async Task<string> RunGenerator(string source)
    {
        // Create a compilation with the source
        var compilation = CreateCompilation(source);

        // Create the generator
        var generator = new OneOf2ToIResultGenerator();

        // Create a driver to run the generator
        var driver = CSharpGeneratorDriver.Create(generator);

        // Run the generator
        var runResult = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        // Get the generated files
        var generatedFiles = runResult.GetRunResult().GeneratedTrees;
        
        if (generatedFiles.IsEmpty)
        {
            return string.Empty;
        }

        // Combine all generated source
        var combinedOutput = new System.Text.StringBuilder();
        foreach (var tree in generatedFiles)
        {
            using var writer = new System.IO.StringWriter();
            tree.GetText().Write(writer);
            combinedOutput.AppendLine(writer.ToString());
        }

        return combinedOutput.ToString();
    }

    private Compilation CreateCompilation(string source)
    {
        // Create syntax tree
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));

        // Add references to commonly used assemblies
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.DateTime).Assembly.Location),
        };

        // Create compilation
        return CSharpCompilation.Create(
            "TestCompilation",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
