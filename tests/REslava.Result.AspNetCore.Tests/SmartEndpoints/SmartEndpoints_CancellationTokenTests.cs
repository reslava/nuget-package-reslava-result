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
    /// Tests for CancellationToken threading in SmartEndpoints:
    /// when a service method declares CancellationToken, the generated lambda
    /// must include it as a parameter and forward it to the service call.
    /// </summary>
    [TestClass]
    public class SmartEndpoints_CancellationTokenTests
    {
        private static string RunGeneratorAndGetExtensions(string sourceCode, bool withValidate = false)
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

            var driver = withValidate
                ? CSharpGeneratorDriver.Create(new SmartEndpointsGenerator(), new ValidateGenerator())
                : CSharpGeneratorDriver.Create(new SmartEndpointsGenerator());

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
        public void Should_Include_CancellationToken_In_Lambda_Params()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{
    public class CreateOrderRequest { public string Name { get; set; } }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/orders"")]
    public class OrderController
    {
        public Task<Result<string>> CreateOrder(CreateOrderRequest request, CancellationToken cancellationToken)
            => Task.FromResult(Result<string>.Ok(""created""));
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("CancellationToken cancellationToken"),
                "Lambda should include 'CancellationToken cancellationToken' as a parameter");
        }

        [TestMethod]
        public void Should_Pass_CancellationToken_To_Service_Call()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{
    public class CreateOrderRequest { public string Name { get; set; } }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/orders"")]
    public class OrderController
    {
        public Task<Result<string>> CreateOrder(CreateOrderRequest request, CancellationToken cancellationToken)
            => Task.FromResult(Result<string>.Ok(""created""));
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("service.CreateOrder(request, cancellationToken)"),
                "Service call should forward cancellationToken as an argument");
        }

        [TestMethod]
        public void Should_Thread_CancellationToken_On_GET_With_Id()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/products"")]
    public class ProductController
    {
        public Task<Result<string>> GetProduct(int id, CancellationToken cancellationToken)
            => Task.FromResult(Result<string>.Ok(""product""));
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("{id}"),
                "Route should still include {id} segment");
            Assert.IsTrue(code.Contains("CancellationToken cancellationToken"),
                "Lambda should include CancellationToken parameter");
            Assert.IsTrue(code.Contains("service.GetProduct(id, cancellationToken)"),
                "Service call should forward both id and cancellationToken");
        }

        [TestMethod]
        public void Should_Not_Add_CancellationToken_When_Not_In_Service_Method()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;
using System.Threading.Tasks;

namespace TestApp
{
    public class CreateOrderRequest { public string Name { get; set; } }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/orders"")]
    public class OrderController
    {
        public Task<Result<string>> CreateOrder(CreateOrderRequest request)
            => Task.FromResult(Result<string>.Ok(""created""));
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsFalse(code.Contains("CancellationToken cancellationToken"),
                "Lambda should NOT include CancellationToken when not declared in the service method");
        }

        [TestMethod]
        public void Should_Thread_CancellationToken_Alongside_ValidatedBodyParam()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;
using REslava.Result.SourceGenerators;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{
    [Validate]
    public class CreateProductRequest { public string Name { get; set; } }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/products"")]
    public class ProductController
    {
        public Task<Result<string>> CreateProduct(CreateProductRequest request, CancellationToken cancellationToken)
            => Task.FromResult(Result<string>.Ok(request.Name));
    }
}";
            var code = RunGeneratorAndGetExtensions(source, withValidate: true);

            Assert.IsTrue(code.Contains("validation = request.Validate()"),
                "Validation block should still be injected for the [Validate] body param");
            Assert.IsTrue(code.Contains("CancellationToken cancellationToken"),
                "Lambda should include CancellationToken parameter");
            Assert.IsTrue(code.Contains("service.CreateProduct(request, cancellationToken)"),
                "Service call should forward both request and cancellationToken");
        }
    }
}
