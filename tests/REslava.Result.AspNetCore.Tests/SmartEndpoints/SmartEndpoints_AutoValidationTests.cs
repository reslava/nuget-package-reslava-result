using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints;
using REslava.Result.SourceGenerators.Generators.Validate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace REslava.Result.SourceGenerators.Tests
{
    /// <summary>
    /// Tests for SmartEndpoints auto-validation:
    /// when a method's body parameter type carries [Validate], the generated lambda
    /// automatically calls .Validate() and returns early on failure.
    /// </summary>
    [TestClass]
    public class SmartEndpoints_AutoValidationTests
    {
        private static string RunGeneratorAndGetExtensions(string sourceCode)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            var coreLib = typeof(object).Assembly.Location;
            var runtimeDir = Path.GetDirectoryName(coreLib)!;

            var references = new[]
            {
                MetadataReference.CreateFromFile(coreLib),
                MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(typeof(REslava.Result.Result<>).Assembly.Location),
            };

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: references);

            // Run both generators: ValidateGenerator registers ValidateAttribute via
            // RegisterPostInitializationOutput so SmartEndpointsGenerator can see [Validate]
            // on parameter types during its RegisterSourceOutput pass.
            var driver = CSharpGeneratorDriver.Create(
                new SmartEndpointsGenerator(),
                new ValidateGenerator());
            var runResult = driver.RunGenerators(compilation);
            var result = runResult.GetRunResult();

            Assert.AreEqual(0, result.Diagnostics.Length,
                $"Generator should not produce diagnostics. Got: {string.Join(", ", result.Diagnostics.Select(d => d.ToString()))}");

            var extensionsTree = result.GeneratedTrees
                .FirstOrDefault(t => t.FilePath.Contains("SmartEndpointExtensions"));

            Assert.IsNotNull(extensionsTree, "Should generate SmartEndpointExtensions.g.cs");
            return extensionsTree!.ToString();
        }

        [TestMethod]
        public void Should_Inject_Validation_For_Validated_Body_Param()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;
using REslava.Result.SourceGenerators;
using System.Threading.Tasks;

namespace TestApp
{
    [Validate]
    public class CreateProductRequest
    {
        public string Name { get; set; }
    }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/products"")]
    public class ProductController
    {
        public Task<Result<string>> CreateProduct(CreateProductRequest request)
        {
            return Task.FromResult(Result<string>.Ok(request.Name));
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("validation = request.Validate()"),
                "Should inject .Validate() call on the body parameter");
            Assert.IsTrue(code.Contains("if (!validation.IsSuccess) return validation.ToIResult()"),
                "Should return early when validation fails");
        }

        [TestMethod]
        public void Should_Not_Inject_Validation_Without_Validate_Attribute()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;
using System.Threading.Tasks;

namespace TestApp
{
    public class CreateOrderRequest
    {
        public string Name { get; set; }
    }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/orders"")]
    public class OrderController
    {
        public Task<Result<string>> CreateOrder(CreateOrderRequest request)
        {
            return Task.FromResult(Result<string>.Ok(request.Name));
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsFalse(code.Contains(".Validate()"),
                "Should NOT inject .Validate() when type is not decorated with [Validate]");
        }

        [TestMethod]
        public void Should_Not_Inject_Validation_For_GET_Query_Param()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;
using REslava.Result.SourceGenerators;

namespace TestApp
{
    [Validate]
    public class SearchRequest
    {
        public string Query { get; set; }
    }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/products"")]
    public class ProductController
    {
        public Result<string> GetProducts(SearchRequest filter)
        {
            return Result<string>.Ok(""results"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            // GET params are ParameterSource.Query — validation is only for Body (POST/PUT)
            Assert.IsFalse(code.Contains(".Validate()"),
                "Should NOT inject .Validate() for GET query parameters, even if type has [Validate]");
        }

        [TestMethod]
        public void Should_Add_Using_ValidationExtensions_When_Validated()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;
using REslava.Result.SourceGenerators;
using System.Threading.Tasks;

namespace TestApp
{
    [Validate]
    public class CreateProductRequest
    {
        public string Name { get; set; }
    }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/products"")]
    public class ProductController
    {
        public Task<Result<string>> CreateProduct(CreateProductRequest request)
        {
            return Task.FromResult(Result<string>.Ok(request.Name));
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("using Generated.ValidationExtensions"),
                "Should add 'using Generated.ValidationExtensions' when any endpoint has a validated body param");
        }

        [TestMethod]
        public void Should_Call_Service_After_Validation_Block()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;
using REslava.Result.SourceGenerators;
using System.Threading.Tasks;

namespace TestApp
{
    [Validate]
    public class CreateProductRequest
    {
        public string Name { get; set; }
    }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/products"")]
    public class ProductController
    {
        public Task<Result<string>> CreateProduct(CreateProductRequest request)
        {
            return Task.FromResult(Result<string>.Ok(request.Name));
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            var validateIndex = code.IndexOf("Validate()");
            var serviceIndex = code.IndexOf("service.");
            Assert.IsTrue(validateIndex >= 0 && serviceIndex > validateIndex,
                "Validation call should appear before the service call in the generated lambda body");
        }
    }
}
