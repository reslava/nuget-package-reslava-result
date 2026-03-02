using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.SourceGenerators.Generators.OneOf4ToIResult;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.SourceGenerators.Tests
{
    [TestClass]
    public class OneOf4ToIResultGeneratorTests
    {
        [TestMethod]
        public void OneOf4ToIResultGenerator_Should_Initialize_Successfully()
        {
            // Arrange
            var generator = new OneOf4ToIResultGenerator();
            // Note: We can't fully test Initialize without a proper context
            // But we can verify the generator can be created

            // Act & Assert
            Assert.IsNotNull(generator);
        }

        [TestMethod]
        public void OneOf4ToIResultGenerator_Should_Detect_OneOf4_Types()
        {
            // Arrange
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using REslava.Result.AdvancedPatterns;

namespace TestNamespace
{
    public class TestClass
    {
        public OneOf<ValidationError, NotFoundError, ConflictError, ServerError> TestMethod()
        {
            return new ValidationError(""Test error"");
        }
    }
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
");

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(OneOf<,,>).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: references);

            var generator = new OneOf4ToIResultGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            
            // Act
            var runResult = driver.RunGenerators(compilation);
            var result = runResult.GetRunResult();

            // Assert
            Assert.AreEqual(0, result.Diagnostics.Length, "Generator should not produce diagnostics");
            Assert.IsTrue(result.GeneratedTrees.Length > 0, "Should generate source files");
        }

        [TestMethod]
        public void OneOf4ToIResultGenerator_Should_Generate_Attributes()
        {
            // Arrange
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using REslava.Result.AdvancedPatterns;

namespace TestNamespace
{
    public class TestClass
    {
        public OneOf<ValidationError, NotFoundError, ConflictError, ServerError> TestMethod()
        {
            return new ValidationError(""Test error"");
        }
    }
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
");

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(OneOf<,,>).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: references);

            var generator = new OneOf4ToIResultGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            
            // Act
            var runResult = driver.RunGenerators(compilation);
            var result = runResult.GetRunResult();
            
            // Assert
            Assert.IsTrue(result.GeneratedTrees.Length > 0, "Should generate source files");
            
            var hasAttribute = result.GeneratedTrees.Any(t => 
                t.FilePath.Contains("GenerateOneOf4ExtensionsAttribute") ||
                t.FilePath.Contains("MapToProblemDetailsAttribute"));
            
            Assert.IsTrue(hasAttribute, "Should generate OneOf4 attributes");
        }

        [TestMethod]
        public void OneOf4ToIResultGenerator_Should_Generate_Extension_Methods()
        {
            // Arrange
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using REslava.Result.AdvancedPatterns;

namespace TestNamespace
{
    public class TestClass
    {
        public OneOf<ValidationError, NotFoundError, ConflictError, ServerError> TestMethod()
        {
            return new ValidationError(""Test error"");
        }
    }
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
");

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(OneOf<,,>).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: references);

            var generator = new OneOf4ToIResultGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            
            // Act
            var runResult = driver.RunGenerators(compilation);
            var result = runResult.GetRunResult();
            
            // Assert
            Assert.IsTrue(result.GeneratedTrees.Length > 0, "Should generate extension sources");
            
            var hasExtensions = result.GeneratedTrees.Any(t => 
                t.FilePath.Contains("OneOf4ToIResultExtensions"));
            
            Assert.IsTrue(hasExtensions, "Should generate OneOf4 extension methods");
        }

        [TestMethod]
        public void OneOf4ToIResultGenerator_Should_Skip_When_No_OneOf4_Types()
        {
            // Arrange - Compilation without OneOf4 types
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod()
        {
            // No OneOf4 types here
        }
    }
}");
            
            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });

            var generator = new OneOf4ToIResultGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            
            // Act
            var runResult = driver.RunGenerators(compilation);
            var result = runResult.GetRunResult();
            
            // Assert
            // Should not generate any files when no OneOf4 types detected
            Assert.AreEqual(0, result.GeneratedTrees.Length, "Should not generate files when no OneOf4 types");
        }

        [TestMethod]
        public void OneOf4ToIResultGenerator_Should_Handle_Multiple_OneOf4_Usages()
        {
            // Arrange - Multiple OneOf4 types in same compilation
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using REslava.Result.AdvancedPatterns;

namespace TestNamespace
{
    public class TestClass
    {
        public OneOf<ValidationError, NotFoundError, ConflictError, ServerError> Method1()
        {
            return new ValidationError(""Error 1"");
        }
        
        public OneOf<ValidationError, NotFoundError, ConflictError, ServerError> Method2()
        {
            return new NotFoundError(""Error 2"");
        }
    }
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
");
            
            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(OneOf<,,>).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: references);

            var generator = new OneOf4ToIResultGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            
            // Act
            var runResult = driver.RunGenerators(compilation);
            var result = runResult.GetRunResult();
            
            // Assert
            Assert.AreEqual(0, result.Diagnostics.Length, "Should not produce diagnostics");
            Assert.IsTrue(result.GeneratedTrees.Length > 0, "Should generate sources for multiple OneOf4 usages");
        }
    }
}
