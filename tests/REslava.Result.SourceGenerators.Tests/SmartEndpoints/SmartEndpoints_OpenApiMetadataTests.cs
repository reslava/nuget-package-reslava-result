using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace REslava.Result.SourceGenerators.Tests
{
    /// <summary>
    /// Tests for SmartEndpoints OpenAPI metadata auto-generation:
    /// MapGroup, WithName, WithSummary, WithTags, Produces.
    /// </summary>
    [TestClass]
    public class SmartEndpoints_OpenApiMetadataTests
    {
        /// <summary>
        /// Runs the SmartEndpoints generator on the given source and returns
        /// the content of the SmartEndpointExtensions.g.cs generated file.
        /// </summary>
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

            var generator = new SmartEndpointsGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            var runResult = driver.RunGenerators(compilation);
            var result = runResult.GetRunResult();

            Assert.AreEqual(0, result.Diagnostics.Length,
                $"Generator should not produce diagnostics. Got: {string.Join(", ", result.Diagnostics.Select(d => d.ToString()))}");

            var extensionsTree = result.GeneratedTrees
                .FirstOrDefault(t => t.FilePath.Contains("SmartEndpointExtensions"));

            Assert.IsNotNull(extensionsTree, "Should generate SmartEndpointExtensions.g.cs");
            return extensionsTree!.ToString();
        }

        // ─── MapGroup generation ───

        [TestMethod]
        public void Should_Generate_MapGroup_With_RoutePrefix()
        {
            var source = @"
using REslava.Result;

namespace TestApp
{
    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/products"")]
    public class ProductController
    {
        public Result<string> GetProducts()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("MapGroup(\"/api/products\")"),
                "Should generate MapGroup with the route prefix");
            Assert.IsTrue(code.Contains("MapGet(\"/\""),
                "Should generate relative route '/' for parameterless Get method");
        }

        [TestMethod]
        public void Should_Generate_Relative_Routes_Within_Group()
        {
            var source = @"
using REslava.Result;

namespace TestApp
{
    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/items"")]
    public class ItemController
    {
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""all"");
        }

        public Result<string> GetItem(int id)
        {
            return Result<string>.Ok(""one"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("MapGroup(\"/api/items\")"),
                "Should have MapGroup for the route prefix");
            Assert.IsTrue(code.Contains("MapGet(\"/{id}\""),
                "GetItem(int id) should use relative route /{id}");
        }

        // ─── WithName generation ───

        [TestMethod]
        public void Should_Generate_WithName_For_Each_Endpoint()
        {
            var source = @"
using REslava.Result;

namespace TestApp
{
    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/users"")]
    public class UserController
    {
        public Result<string> GetUsers()
        {
            return Result<string>.Ok(""users"");
        }

        public Result<string> GetUser(int id)
        {
            return Result<string>.Ok(""user"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".WithName(\"User_GetUsers\")"),
                "Should generate .WithName(\"User_GetUsers\") with controller prefix");
            Assert.IsTrue(code.Contains(".WithName(\"User_GetUser\")"),
                "Should generate .WithName(\"User_GetUser\") with controller prefix");
        }

        // ─── WithSummary generation (from method name) ───

        [TestMethod]
        public void Should_Generate_WithSummary_From_PascalCase_MethodName()
        {
            var source = @"
using REslava.Result;

namespace TestApp
{
    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/products"")]
    public class ProductController
    {
        public Result<string> GetProducts()
        {
            return Result<string>.Ok(""test"");
        }

        public Result<string> CreateProduct(string request)
        {
            return Result<string>.Ok(""created"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".WithSummary(\"Get products\")"),
                "Should split PascalCase 'GetProducts' into 'Get products'");
            Assert.IsTrue(code.Contains(".WithSummary(\"Create product\")"),
                "Should split PascalCase 'CreateProduct' into 'Create product'");
        }

        // ─── WithTags auto-generation ───

        [TestMethod]
        public void Should_Generate_WithTags_From_ClassName()
        {
            var source = @"
using REslava.Result;

namespace TestApp
{
    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/products"")]
    public class SmartProductController
    {
        public Result<string> GetProducts()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".WithTags(\"Smart Product\")"),
                "Should strip 'Controller' suffix and split PascalCase for tag");
        }

        [TestMethod]
        public void Should_Generate_WithTags_Stripping_Service_Suffix()
        {
            var source = @"
using REslava.Result;

namespace TestApp
{
    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/orders"")]
    public class OrderService
    {
        public Result<string> GetOrders()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".WithTags(\"Order\")"),
                "Should strip 'Service' suffix for tag");
        }

        // ─── Produces<T> for Result<T> ───

        [TestMethod]
        public void Should_Generate_Produces_For_ResultT()
        {
            var source = @"
using REslava.Result;

namespace TestApp
{
    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/items"")]
    public class ItemController
    {
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".Produces<") && code.Contains(">(200)"),
                "Result<string> should produce typed .Produces<T>(200)");
            Assert.IsTrue(code.Contains(".Produces(400)"),
                "Result<T> should always include .Produces(400) for default error");
        }

        // ─── Produces for OneOf error types ───

        [TestMethod]
        public void Should_Generate_Produces_For_OneOf_NotFoundError()
        {
            var source = @"
using REslava.Result.AdvancedPatterns;

namespace TestApp
{
    public class UserNotFoundError { public string Message { get; set; } }
    public class UserResponse { public string Name { get; set; } }

    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/users"")]
    public class UserController
    {
        public OneOf<UserNotFoundError, UserResponse> GetUser(int id)
        {
            return new UserResponse { Name = ""Test"" };
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".Produces(404)"),
                "UserNotFoundError should map to 404");
            Assert.IsTrue(code.Contains("Produces<") && code.Contains("UserResponse"),
                "UserResponse (success type) should have typed .Produces<T>(200)");
        }

        [TestMethod]
        public void Should_Generate_Produces_For_OneOf_ConflictError()
        {
            var source = @"
using REslava.Result.AdvancedPatterns;

namespace TestApp
{
    public class DuplicateError { public string Message { get; set; } }
    public class ItemResponse { public string Name { get; set; } }

    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/items"")]
    public class ItemController
    {
        public OneOf<DuplicateError, ItemResponse> CreateItem(string request)
        {
            return new ItemResponse { Name = ""Test"" };
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".Produces(409)"),
                "DuplicateError should map to 409 Conflict");
        }

        [TestMethod]
        public void Should_Generate_Produces_For_OneOf4_With_Multiple_Error_Types()
        {
            var source = @"
using REslava.Result.AdvancedPatterns;

namespace TestApp
{
    public class ValidationError { public string Message { get; set; } }
    public class UserNotFoundError { public string Message { get; set; } }
    public class InsufficientStockError { public string Message { get; set; } }
    public class OrderResponse { public int Id { get; set; } }

    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/orders"")]
    public class OrderController
    {
        public OneOf<UserNotFoundError, InsufficientStockError, ValidationError, OrderResponse> CreateOrder(string request)
        {
            return new OrderResponse { Id = 1 };
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            // OneOf4 with 3 error types + 1 success
            Assert.IsTrue(code.Contains("Produces<") && code.Contains("OrderResponse"),
                "OrderResponse should be the success type with .Produces<T>(200)");
            Assert.IsTrue(code.Contains(".Produces(400)"),
                "ValidationError should map to 400");
            Assert.IsTrue(code.Contains(".Produces(404)"),
                "UserNotFoundError should map to 404");
            Assert.IsTrue(code.Contains(".Produces(409)"),
                "InsufficientStockError should map to 409");
        }

        // ─── Status code deduplication ───

        [TestMethod]
        public void Should_Deduplicate_Same_Status_Codes()
        {
            var source = @"
using REslava.Result.AdvancedPatterns;

namespace TestApp
{
    public class ValidationError { public string Message { get; set; } }
    public class InvalidFormatError { public string Message { get; set; } }
    public class SuccessResponse { public string Data { get; set; } }

    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/data"")]
    public class DataController
    {
        public OneOf<ValidationError, InvalidFormatError, SuccessResponse> CreateData(string request)
        {
            return new SuccessResponse { Data = ""test"" };
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            // Both ValidationError and InvalidFormatError map to 400 — should only appear once
            var count400 = CountOccurrences(code, ".Produces(400)");
            Assert.AreEqual(1, count400,
                "Two errors mapping to 400 should produce only one .Produces(400)");
        }

        // ─── HTTP method inference ───

        [TestMethod]
        public void Should_Infer_Correct_HTTP_Methods()
        {
            var source = @"
using REslava.Result;

namespace TestApp
{
    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/items"")]
    public class ItemController
    {
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""list"");
        }

        public Result<string> CreateItem(string request)
        {
            return Result<string>.Ok(""created"");
        }

        public Result<string> UpdateItem(int id, string request)
        {
            return Result<string>.Ok(""updated"");
        }

        public Result<string> DeleteItem(int id)
        {
            return Result<string>.Ok(""deleted"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("MapGet"),
                "GetItems should use MapGet");
            Assert.IsTrue(code.Contains("MapPost"),
                "CreateItem should use MapPost");
            Assert.IsTrue(code.Contains("MapPut"),
                "UpdateItem should use MapPut");
            Assert.IsTrue(code.Contains("MapDelete"),
                "DeleteItem should use MapDelete");
        }

        // ─── Async methods ───

        [TestMethod]
        public void Should_Handle_Async_Methods_With_TaskT()
        {
            var source = @"
using REslava.Result;
using System.Threading.Tasks;

namespace TestApp
{
    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/async"")]
    public class AsyncController
    {
        public Task<Result<string>> GetData()
        {
            return Task.FromResult(Result<string>.Ok(""test""));
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("async"),
                "Should generate async lambda for Task<T> methods");
            Assert.IsTrue(code.Contains("await"),
                "Should generate await for async methods");
            Assert.IsTrue(code.Contains(".WithName(\"Async_GetData\")"),
                "Should still generate correct WithName for async method");
        }

        // ─── Multiple controllers ───

        [TestMethod]
        public void Should_Generate_Separate_MapGroups_Per_Controller()
        {
            var source = @"
using REslava.Result;

namespace TestApp
{
    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/users"")]
    public class UserController
    {
        public Result<string> GetUsers()
        {
            return Result<string>.Ok(""users"");
        }
    }

    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/products"")]
    public class ProductController
    {
        public Result<string> GetProducts()
        {
            return Result<string>.Ok(""products"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("MapGroup(\"/api/users\")"),
                "Should generate MapGroup for UserController");
            Assert.IsTrue(code.Contains("MapGroup(\"/api/products\")"),
                "Should generate MapGroup for ProductController");
            Assert.IsTrue(code.Contains(".WithTags(\"User\")"),
                "UserController should generate 'User' tag");
            Assert.IsTrue(code.Contains(".WithTags(\"Product\")"),
                "ProductController should generate 'Product' tag");
        }

        // ─── Non-Result/OneOf methods should be skipped ───

        [TestMethod]
        public void Should_Skip_Methods_Not_Returning_Result_Or_OneOf()
        {
            var source = @"
using REslava.Result;

namespace TestApp
{
    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/test"")]
    public class TestController
    {
        public Result<string> GetData()
        {
            return Result<string>.Ok(""data"");
        }

        public string GetPlainString()
        {
            return ""plain"";
        }

        public void DoNothing()
        {
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("GetData"),
                "Should generate endpoint for Result<T> method");
            Assert.IsFalse(code.Contains("GetPlainString"),
                "Should NOT generate endpoint for plain string method");
            Assert.IsFalse(code.Contains("DoNothing"),
                "Should NOT generate endpoint for void method");
        }

        // ─── Error type → status code mapping ───

        [TestMethod]
        public void Should_Map_UnauthorizedError_To_401()
        {
            var source = @"
using REslava.Result.AdvancedPatterns;

namespace TestApp
{
    public class UnauthorizedError { }
    public class UserResponse { public string Name { get; set; } }

    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/secure"")]
    public class SecureController
    {
        public OneOf<UnauthorizedError, UserResponse> GetSecureData(int id)
        {
            return new UserResponse { Name = ""test"" };
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);
            Assert.IsTrue(code.Contains(".Produces(401)"),
                "UnauthorizedError should map to 401");
        }

        [TestMethod]
        public void Should_Map_ForbiddenError_To_403()
        {
            var source = @"
using REslava.Result.AdvancedPatterns;

namespace TestApp
{
    public class ForbiddenError { }
    public class DataResponse { public string Data { get; set; } }

    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/admin"")]
    public class AdminController
    {
        public OneOf<ForbiddenError, DataResponse> GetAdminData(int id)
        {
            return new DataResponse { Data = ""test"" };
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);
            Assert.IsTrue(code.Contains(".Produces(403)"),
                "ForbiddenError should map to 403");
        }

        [TestMethod]
        public void Should_Map_DatabaseError_To_500()
        {
            var source = @"
using REslava.Result.AdvancedPatterns;

namespace TestApp
{
    public class DatabaseError { }
    public class DataResponse { public string Data { get; set; } }

    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/data"")]
    public class DataController
    {
        public OneOf<DatabaseError, DataResponse> GetData(int id)
        {
            return new DataResponse { Data = ""test"" };
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);
            Assert.IsTrue(code.Contains(".Produces(500)"),
                "DatabaseError should map to 500");
        }

        // ─── Variable naming ───

        [TestMethod]
        public void Should_Generate_Correct_GroupVariable_Name()
        {
            var source = @"
using REslava.Result;

namespace TestApp
{
    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/products"")]
    public class SmartProductController
    {
        public Result<string> GetProducts()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("var smartProductGroup"),
                "Should derive group variable from class name: 'SmartProductController' → 'smartProductGroup'");
        }

        // ─── Fluent chain structure ───

        [TestMethod]
        public void Should_Generate_Complete_Fluent_Chain()
        {
            var source = @"
using REslava.Result;

namespace TestApp
{
    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/items"")]
    public class ItemController
    {
        public Result<string> GetItem(int id)
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            // Verify the complete fluent chain is present
            Assert.IsTrue(code.Contains(".WithName(\"Item_GetItem\")"), "Missing .WithName()");
            Assert.IsTrue(code.Contains(".WithSummary("), "Missing .WithSummary()");
            Assert.IsTrue(code.Contains(".Produces<"), "Missing typed .Produces<T>()");
            Assert.IsTrue(code.Contains(".Produces(400)"), "Missing error .Produces()");
        }

        // ─── Using statements ───

        [TestMethod]
        public void Should_Include_Required_Using_Statements()
        {
            var source = @"
using REslava.Result;

namespace TestApp
{
    [REslava.Result.SourceGenerators.SmartEndpoints.AutoGenerateEndpoints(RoutePrefix = ""/api/test"")]
    public class TestController
    {
        public Result<string> GetData()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("using Generated.ResultExtensions;"),
                "Should include ResultExtensions using");
            Assert.IsTrue(code.Contains("using Generated.OneOfExtensions;"),
                "Should include OneOfExtensions using");
            Assert.IsTrue(code.Contains("using Microsoft.AspNetCore.Builder;"),
                "Should include AspNetCore.Builder using");
        }

        // ─── Helpers ───

        private static int CountOccurrences(string text, string pattern)
        {
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(pattern, index, System.StringComparison.Ordinal)) != -1)
            {
                count++;
                index += pattern.Length;
            }
            return count;
        }
    }
}
