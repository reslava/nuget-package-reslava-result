using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.SourceGenerators.Generators.OneOf3ToIResult;

namespace REslava.Result.SourceGenerators.Tests.OneOf3ToIResult;

[TestClass]
public class OneOf3ToIResultGeneratorTests
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
            // This should trigger the OneOf3ToIResult generator
            OneOf<{T1}, {T2}, {T3}> result = {Initialization};
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
    public async Task Generator_Should_Generate_Extensions_For_OneOf3()
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
            // This should trigger the OneOf3ToIResult generator
            OneOf<ValidationError, UserNotFoundError, User> result = new ValidationError(""test"", ""test error"");
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

public class UserNotFoundError 
{
    public int Id { get; set; }
    public UserNotFoundError(int id) { Id = id; }
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
        Assert.IsTrue(generatedOutput.Contains("OneOf3Extensions"), "Should generate extension class");
        Assert.IsTrue(generatedOutput.Contains("ToIResult<T1, T2, T3>"), "Should generate generic extension method");
        Assert.IsTrue(generatedOutput.Contains("Results.BadRequest"), "Should map T1 to BadRequest");
        Assert.IsTrue(generatedOutput.Contains("Results.Ok"), "Should map T3 to Ok");
    }

    [TestMethod]
    public async Task Generator_Should_Generate_Attributes()
    {
        // Arrange
        var source = TestSourceTemplate
            .Replace("{T1}", "ValidationError")
            .Replace("{T2}", "UserNotFoundError")
            .Replace("{T3}", "User")
            .Replace("{Initialization}", "new ValidationError(\"test\", \"test error\")");

        // Act
        var generatedOutput = await RunGenerator(source);

        // Assert
        Assert.IsNotNull(generatedOutput, "Generator should produce output");
        Assert.IsTrue(generatedOutput.Contains("GenerateOneOf3ExtensionsAttribute"), "Should generate attribute class");
        Assert.IsTrue(generatedOutput.Contains("MapToProblemDetailsAttribute"), "Should generate mapping attribute");
    }

    [TestMethod]
    public async Task Generator_Should_Handle_Different_Type_Combinations()
    {
        // Test different type combinations
        var testCases = new[]
        {
            ("ValidationError", "UserNotFoundError", "CreatedUser", "new ValidationError(\"Test\", \"Test error\")"),
            ("ValidationError", "UserNotFoundError", "User", "new UserNotFoundError(1)"),
            ("ValidationError", "UserNotFoundError", "CreatedUser", "new CreatedUser(1, \"test\", DateTime.UtcNow)")
        };

        foreach (var (t1, t2, t3, initialization) in testCases)
        {
            // Arrange
            var source = TestSourceTemplate
                .Replace("{T1}", t1)
                .Replace("{T2}", t2)
                .Replace("{T3}", t3)
                .Replace("{Initialization}", initialization);

            // Act
            var generatedOutput = await RunGenerator(source);

            // Assert
            Assert.IsNotNull(generatedOutput, $"Generator should produce output for {t1}, {t2}, {t3}");
            Assert.IsTrue(generatedOutput.Contains("OneOf3Extensions"), $"Should generate extensions for {t1}, {t2}, {t3}");
        }
    }

    [TestMethod]
    public async Task Generator_Should_Not_Generate_Without_OneOf3_Usage()
    {
        // Arrange - source without OneOf3 usage
        var source = @"
using System;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod()
        {
            // No OneOf3 usage - should not trigger generator
            var result = ""test"";
            // Using different namespace - should not trigger
            var oneOfResult = System.Collections.Generic.List<int>.Empty;
        }
    }
}";

        // Act
        var generatedOutput = await RunGenerator(source);

        // Assert
        Assert.IsTrue(string.IsNullOrEmpty(generatedOutput), "Generator should not produce output without OneOf3 usage");
    }

    private async Task<string> RunGenerator(string source)
    {
        // Create a compilation with the source
        var compilation = CreateCompilation(source);

        // Create the generator
        var generator = new OneOf3ToIResultGenerator();

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
