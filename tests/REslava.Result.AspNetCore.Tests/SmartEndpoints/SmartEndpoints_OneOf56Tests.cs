using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace REslava.Result.SourceGenerators.Tests
{
    [TestClass]
    public class SmartEndpoints_OneOf56Tests
    {
        // ── Metadata tests (no compilation needed) ──────────────────────────────

        [TestMethod]
        public void OneOf5_EndpointMetadata_ShouldHaveArity5()
        {
            var endpoint = new EndpointMetadata
            {
                MethodName = "GetOrders",
                ClassName = "OrderController",
                IsOneOf = true,
                OneOfArity = 5,
                GenericTypeArguments = new List<string>
                {
                    "AuthError", "ValidationError", "NotFoundError", "ConflictError", "Order"
                }
            };

            Assert.IsTrue(endpoint.IsOneOf, "Should be IsOneOf");
            Assert.AreEqual(5, endpoint.OneOfArity, "OneOfArity should be 5");
            Assert.IsFalse(endpoint.IsOneOf4, "IsOneOf4 should be false for arity 5");
            Assert.AreEqual(5, endpoint.GenericTypeArguments.Count);
        }

        [TestMethod]
        public void OneOf6_EndpointMetadata_ShouldHaveArity6()
        {
            var endpoint = new EndpointMetadata
            {
                MethodName = "CreateOrder",
                ClassName = "OrderController",
                IsOneOf = true,
                OneOfArity = 6,
                GenericTypeArguments = new List<string>
                {
                    "AuthError", "ValidationError", "NotFoundError", "ConflictError", "ServerError", "Order"
                }
            };

            Assert.IsTrue(endpoint.IsOneOf, "Should be IsOneOf");
            Assert.AreEqual(6, endpoint.OneOfArity, "OneOfArity should be 6");
            Assert.IsFalse(endpoint.IsOneOf4, "IsOneOf4 should be false for arity 6");
            Assert.AreEqual(6, endpoint.GenericTypeArguments.Count);
        }

        [TestMethod]
        public void OneOf4_EndpointMetadata_IsOneOf4ComputedProperty_ShouldBeTrue()
        {
            var endpoint = new EndpointMetadata
            {
                IsOneOf = true,
                OneOfArity = 4
            };

            Assert.IsTrue(endpoint.IsOneOf4, "IsOneOf4 computed property should be true when IsOneOf && OneOfArity == 4");
        }

        [TestMethod]
        public void OneOf5_EndpointMetadata_IsOneOf4ComputedProperty_ShouldBeFalse()
        {
            var endpoint = new EndpointMetadata
            {
                IsOneOf = true,
                OneOfArity = 5
            };

            Assert.IsFalse(endpoint.IsOneOf4, "IsOneOf4 should be false for arity 5");
        }

        [TestMethod]
        public void OneOf6_EndpointMetadata_IsOneOf4ComputedProperty_ShouldBeFalse()
        {
            var endpoint = new EndpointMetadata
            {
                IsOneOf = true,
                OneOfArity = 6
            };

            Assert.IsFalse(endpoint.IsOneOf4, "IsOneOf4 should be false for arity 6");
        }

        // ── Generator-level tests ────────────────────────────────────────────────

        [TestMethod]
        public void OneOf5_ReturnType_ShouldGenerateToIResultCall()
        {
            var source = @"
using REslava.Result.AdvancedPatterns;
using REslava.Result.SourceGenerators.SmartEndpoints;
using System.Threading.Tasks;

namespace TestApp
{
    public class AuthError { }
    public class ValidationError { }
    public class NotFoundError { }
    public class ConflictError { }
    public class Order { public int Id { get; set; } }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/orders"")]
    public class OrderController
    {
        public Task<OneOf<AuthError, ValidationError, NotFoundError, ConflictError, Order>> GetOrder(int id)
            => throw new System.NotImplementedException();
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("result.ToIResult()"),
                "Should emit result.ToIResult() for OneOf5 return type");
            Assert.IsTrue(code.Contains("MapGet"),
                "Should map GET endpoint");
        }

        [TestMethod]
        public void OneOf6_ReturnType_ShouldGenerateToIResultCall()
        {
            var source = @"
using REslava.Result.AdvancedPatterns;
using REslava.Result.SourceGenerators.SmartEndpoints;
using System.Threading.Tasks;

namespace TestApp
{
    public class AuthError { }
    public class ValidationError { }
    public class NotFoundError { }
    public class ConflictError { }
    public class ServerError { }
    public class Order { public int Id { get; set; } }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/orders"")]
    public class OrderController
    {
        public Task<OneOf<AuthError, ValidationError, NotFoundError, ConflictError, ServerError, Order>> CreateOrder()
            => throw new System.NotImplementedException();
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("result.ToIResult()"),
                "Should emit result.ToIResult() for OneOf6 return type");
            Assert.IsTrue(code.Contains("MapPost"),
                "Should map POST endpoint (CreateOrder* → POST)");
        }

        [TestMethod]
        public void OneOf5_And_OneOf6_CanCoexistInSameController()
        {
            var source = @"
using REslava.Result.AdvancedPatterns;
using REslava.Result.SourceGenerators.SmartEndpoints;
using System.Threading.Tasks;

namespace TestApp
{
    public class AuthError { }
    public class ValidationError { }
    public class NotFoundError { }
    public class ConflictError { }
    public class ServerError { }
    public class Order { public int Id { get; set; } }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/orders"")]
    public class OrderController
    {
        public Task<OneOf<AuthError, ValidationError, NotFoundError, ConflictError, Order>> GetOrder(int id)
            => throw new System.NotImplementedException();

        public Task<OneOf<AuthError, ValidationError, NotFoundError, ConflictError, ServerError, Order>> CreateOrder()
            => throw new System.NotImplementedException();
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            // Both endpoints should be in the generated code
            Assert.IsTrue(code.Contains("GetOrder"),
                "Should generate GetOrder endpoint");
            Assert.IsTrue(code.Contains("CreateOrder"),
                "Should generate CreateOrder endpoint");

            // Both should use ToIResult()
            var toIResultCount = code.Split(new[] { "result.ToIResult()" }, System.StringSplitOptions.None).Length - 1;
            Assert.AreEqual(2, toIResultCount,
                "Should emit result.ToIResult() for both endpoints");
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

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

            var driver = CSharpGeneratorDriver.Create(new SmartEndpointsGenerator());
            var runResult = driver.RunGenerators(compilation);
            var result = runResult.GetRunResult();

            Assert.AreEqual(0, result.Diagnostics.Length,
                $"Generator should produce no diagnostics. Got: {string.Join(", ", result.Diagnostics.Select(d => d.ToString()))}");

            var extensionsTree = result.GeneratedTrees
                .FirstOrDefault(t => t.FilePath.Contains("SmartEndpointExtensions"));

            Assert.IsNotNull(extensionsTree, "Should generate SmartEndpointExtensions.g.cs");
            return extensionsTree!.ToString();
        }
    }
}
