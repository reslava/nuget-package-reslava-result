using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.Models;
using REslava.Result.SourceGenerators.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.SourceGenerators.Tests
{
    [TestClass]
    public class SmartEndpoints_OneOf4Tests
    {
        [TestMethod]
        public void SmartEndpoints_Should_Detect_OneOf4_Return_Types()
        {
            // Arrange
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using REslava.Result.AdvancedPatterns;

namespace TestNamespace
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/users"")]
    public class UserController
    {
        public OneOf<ValidationError, NotFoundError, ConflictError, ServerError> CreateUser(CreateUserRequest request)
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

public class CreateUserRequest
{
    public string Email { get; set; }
    public string Name { get; set; }
}
");

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(OneOf<,,,>).Assembly.Location),
                // Add REslava.Result reference for SmartEndpoints
                MetadataReference.CreateFromFile(typeof(REslava.Result.Result<>).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: references);

            var generator = new SmartEndpointsGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            
            // Act
            var runResult = driver.RunGenerators(compilation);
            var result = runResult.GetRunResult();

            // Assert
            Assert.AreEqual(0, result.Diagnostics.Length, "Generator should not produce diagnostics");
            Assert.IsTrue(result.GeneratedTrees.Length > 0, "Should generate source files");
            
            // Verify OneOf4 detection in generated code
            var generatedCode = result.GeneratedTrees.First().ToString();
            Assert.IsTrue(generatedCode.Contains("CreateUser"), "Should generate CreateUser endpoint");
            Assert.IsTrue(generatedCode.Contains("ToIResult()"), "Should use ToIResult extension");
        }

        [TestMethod]
        public void SmartEndpoints_Should_Generate_Correct_Endpoint_Metadata_For_OneOf4()
        {
            // Arrange
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using REslava.Result.AdvancedPatterns;

namespace TestNamespace
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/products"")]
    public class ProductController
    {
        public OneOf<ValidationError, NotFoundError, ConflictError, DatabaseError> UpdateProduct(int id, UpdateProductRequest request)
        {
            return new ValidationError(""Test validation"");
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

public class DatabaseError : Error
{
    public DatabaseError(string message) : base(message) { }
}

public class UpdateProductRequest
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
");

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(OneOf<,,,>).Assembly.Location),
                // Add REslava.Result reference for SmartEndpoints
                MetadataReference.CreateFromFile(typeof(REslava.Result.Result<>).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: references);

            var generator = new SmartEndpointsGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            
            // Act
            var runResult = driver.RunGenerators(compilation);
            var result = runResult.GetRunResult();

            // Assert
            Assert.AreEqual(0, result.Diagnostics.Length, "Generator should not produce diagnostics");
            Assert.IsTrue(result.GeneratedTrees.Length > 0, "Should generate source files");
            
            var generatedCode = result.GeneratedTrees.First().ToString();
            Assert.IsTrue(generatedCode.Contains("UpdateProduct"), "Should generate UpdateProduct endpoint");
            Assert.IsTrue(generatedCode.Contains("PUT"), "Should infer PUT method from UpdateProduct");
            Assert.IsTrue(generatedCode.Contains("/api/products"), "Should use correct route prefix");
        }

        [TestMethod]
        public void SmartEndpoints_Should_Handle_Mixed_OneOf_Types()
        {
            // Arrange - Mix OneOf2, OneOf3, and OneOf4 in same compilation
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using REslava.Result.AdvancedPatterns;

namespace TestNamespace
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/mixed"")]
    public class MixedController
    {
        public OneOf<ValidationError, User> GetUser(int id)
        {
            return new User(""Test User"");
        }
        
        public OneOf<ValidationError, NotFoundError, User> CreateItem(CreateItemRequest request)
        {
            return new NotFoundError(""Item not found"");
        }
        
        public OneOf<ValidationError, NotFoundError, ConflictError, ServerError> ComplexOperation(ComplexRequest request)
        {
            return new ValidationError(""Complex validation error"");
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

public class User
{
    public User(string name) { Name = name; }
    public string Name { get; }
}

public class CreateItemRequest
{
    public string Name { get; set; }
}

public class ComplexRequest
{
    public string Data { get; set; }
}
");

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(OneOf<,,,>).Assembly.Location),
                // Add REslava.Result reference for SmartEndpoints
                MetadataReference.CreateFromFile(typeof(REslava.Result.Result<>).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: references);

            var generator = new SmartEndpointsGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            
            // Act
            var runResult = driver.RunGenerators(compilation);
            var result = runResult.GetRunResult();

            // Assert
            Assert.AreEqual(0, result.Diagnostics.Length, "Generator should not produce diagnostics");
            Assert.IsTrue(result.GeneratedTrees.Length > 0, "Should generate source files");
            
            var generatedCode = result.GeneratedTrees.First().ToString();
            
            // Should generate all three endpoints
            Assert.IsTrue(generatedCode.Contains("GetUser"), "Should generate GetUser endpoint");
            Assert.IsTrue(generatedCode.Contains("CreateItem"), "Should generate CreateItem endpoint");
            Assert.IsTrue(generatedCode.Contains("ComplexOperation"), "Should generate ComplexOperation endpoint");
            
            // Should use correct HTTP methods
            Assert.IsTrue(generatedCode.Contains("MapGet"), "Should use GET for GetUser");
            Assert.IsTrue(generatedCode.Contains("MapPost"), "Should use POST for CreateItem");
            Assert.IsTrue(generatedCode.Contains("MapPost"), "Should use POST for ComplexOperation");
        }

        [TestMethod]
        public void SmartEndpoints_Should_Maintain_Backward_Compatibility_With_OneOf4()
        {
            // Arrange - Ensure existing OneOf2/OneOf3 still work
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using REslava.Result.AdvancedPatterns;

namespace TestNamespace
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/legacy"")]
    public class LegacyController
    {
        public OneOf<NotFoundError, User> GetLegacyUser(int id)
        {
            return new User(""Legacy User"");
        }
        
        public OneOf<ValidationError, NotFoundError, User> CreateLegacyUser(CreateUserRequest request)
        {
            return new ValidationError(""Legacy validation"");
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

public class User
{
    public User(string name) { Name = name; }
    public string Name { get; }
}

public class CreateUserRequest
{
    public string Email { get; set; }
}
");

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(OneOf<,,,>).Assembly.Location),
                // Add REslava.Result reference for SmartEndpoints
                MetadataReference.CreateFromFile(typeof(REslava.Result.Result<>).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: references);

            var generator = new SmartEndpointsGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            
            // Act
            var runResult = driver.RunGenerators(compilation);
            var result = runResult.GetRunResult();

            // Assert
            Assert.AreEqual(0, result.Diagnostics.Length, "Generator should not produce diagnostics");
            Assert.IsTrue(result.GeneratedTrees.Length > 0, "Should generate source files");
            
            var generatedCode = result.GeneratedTrees.First().ToString();
            
            // Should still generate legacy endpoints correctly
            Assert.IsTrue(generatedCode.Contains("GetLegacyUser"), "Should generate GetLegacyUser endpoint");
            Assert.IsTrue(generatedCode.Contains("CreateLegacyUser"), "Should generate CreateLegacyUser endpoint");
            Assert.IsTrue(generatedCode.Contains("ToIResult()"), "Should use ToIResult extension");
        }

        [TestMethod]
        public void SmartEndpoints_Should_Skip_Non_OneOf4_Methods()
        {
            // Arrange - Methods that don't return OneOf types should be ignored
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using REslava.Result.AdvancedPatterns;

namespace TestNamespace
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/ignored"")]
    public class IgnoredController
    {
        public string GetSimpleString()
        {
            return ""Simple string"";
        }
        
        public int GetSimpleInt()
        {
            return 42;
        }
        
        public void GetVoid()
        {
            // Do nothing
        }
    }
}
");

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(OneOf<,,,>).Assembly.Location),
                // Add REslava.Result reference for SmartEndpoints
                MetadataReference.CreateFromFile(typeof(REslava.Result.Result<>).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: references);

            var generator = new SmartEndpointsGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            
            // Act
            var runResult = driver.RunGenerators(compilation);
            var result = runResult.GetRunResult();

            // Assert
            Assert.AreEqual(0, result.Diagnostics.Length, "Generator should not produce diagnostics");
            Assert.IsTrue(result.GeneratedTrees.Length > 0, "Should still generate attributes");
            
            var generatedCode = result.GeneratedTrees.First().ToString();
            
            // Should not generate endpoints for non-OneOf methods
            Assert.IsFalse(generatedCode.Contains("GetSimpleString"), "Should not generate GetSimpleString endpoint");
            Assert.IsFalse(generatedCode.Contains("GetSimpleInt"), "Should not generate GetSimpleInt endpoint");
            Assert.IsFalse(generatedCode.Contains("GetVoid"), "Should not generate GetVoid endpoint");
        }
    }
}
