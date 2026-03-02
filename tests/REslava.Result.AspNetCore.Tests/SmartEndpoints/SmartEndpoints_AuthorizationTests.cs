using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace REslava.Result.SourceGenerators.Tests
{
    /// <summary>
    /// Tests for SmartEndpoints Authorization & Policy Support:
    /// RequiresAuth, Policies, Roles, AllowAnonymous, Produces(401).
    /// </summary>
    [TestClass]
    public class SmartEndpoints_AuthorizationTests
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

        // ─── Class-level RequiresAuth ───

        [TestMethod]
        public void Should_Generate_RequireAuthorization_When_Class_RequiresAuth()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", RequiresAuth = true)]
    public class ItemController
    {
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".RequireAuthorization()"),
                "Class-level RequiresAuth=true should emit .RequireAuthorization()");
        }

        [TestMethod]
        public void Should_Not_Generate_Auth_When_No_RequiresAuth()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"")]
    public class ItemController
    {
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsFalse(code.Contains(".RequireAuthorization()"),
                "No auth on class should not emit .RequireAuthorization()");
            Assert.IsFalse(code.Contains(".AllowAnonymous()"),
                "No auth on class should not emit .AllowAnonymous()");
        }

        // ─── Class-level Policies ───

        [TestMethod]
        public void Should_Generate_RequireAuthorization_With_Policy()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", Policies = new[] { ""Admin"" })]
    public class ItemController
    {
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".RequireAuthorization(\"Admin\")"),
                "Policy should emit .RequireAuthorization(\"Admin\")");
        }

        [TestMethod]
        public void Should_Generate_RequireAuthorization_With_Multiple_Policies()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", Policies = new[] { ""Admin"", ""Manager"" })]
    public class ItemController
    {
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".RequireAuthorization(\"Admin\", \"Manager\")"),
                "Multiple policies should emit .RequireAuthorization(\"Admin\", \"Manager\")");
        }

        // ─── Class-level Roles ───

        [TestMethod]
        public void Should_Generate_RequireAuthorization_With_Roles()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", Roles = new[] { ""Admin"", ""Manager"" })]
    public class ItemController
    {
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains("new AuthorizeAttribute { Roles = \"Admin,Manager\" }"),
                "Roles should emit RequireAuthorization with AuthorizeAttribute");
            Assert.IsTrue(code.Contains("using Microsoft.AspNetCore.Authorization;"),
                "Roles should add Authorization using statement");
        }

        // ─── Method-level SmartAllowAnonymous ───

        [TestMethod]
        public void Should_Generate_AllowAnonymous_For_SmartAllowAnonymous_Method()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", RequiresAuth = true)]
    public class ItemController
    {
        [SmartAllowAnonymous]
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }

        public Result<string> GetItem(int id)
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".AllowAnonymous()"),
                "Method with [SmartAllowAnonymous] should emit .AllowAnonymous()");
            Assert.IsTrue(code.Contains(".RequireAuthorization()"),
                "Method without [SmartAllowAnonymous] should still emit .RequireAuthorization()");
        }

        // ─── Produces(401) for auth endpoints ───

        [TestMethod]
        public void Should_Auto_Add_Produces_401_For_Auth_Endpoints()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", RequiresAuth = true)]
    public class ItemController
    {
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".Produces(401)"),
                "Auth-required endpoint should auto-add .Produces(401)");
        }

        [TestMethod]
        public void Should_Not_Add_Produces_401_For_AllowAnonymous_Endpoint()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", RequiresAuth = true)]
    public class ItemController
    {
        [SmartAllowAnonymous]
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsFalse(code.Contains(".Produces(401)"),
                "AllowAnonymous endpoint should NOT get .Produces(401)");
            Assert.IsTrue(code.Contains(".AllowAnonymous()"),
                "Should emit .AllowAnonymous()");
        }

        [TestMethod]
        public void Should_Not_Add_Produces_401_When_No_Auth()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"")]
    public class ItemController
    {
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsFalse(code.Contains(".Produces(401)"),
                "Non-auth endpoint should NOT get .Produces(401)");
        }

        // ─── Auth inheritance ───

        [TestMethod]
        public void Should_Inherit_Class_Auth_To_All_Endpoints()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", RequiresAuth = true)]
    public class ItemController
    {
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }

        public Result<string> CreateItem(string name)
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            // Count occurrences of .RequireAuthorization()
            var count = code.Split(new[] { ".RequireAuthorization()" }, System.StringSplitOptions.None).Length - 1;
            Assert.AreEqual(2, count,
                $"Both endpoints should inherit .RequireAuthorization() from class. Found {count} occurrences.");
        }

        // ─── Mixed auth and anonymous ───

        [TestMethod]
        public void Should_Handle_Mixed_Auth_And_Anonymous_Endpoints()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/orders"", RequiresAuth = true)]
    public class OrderController
    {
        [SmartAllowAnonymous]
        public Result<string> GetOrders()
        {
            return Result<string>.Ok(""test"");
        }

        public Result<string> GetOrder(int id)
        {
            return Result<string>.Ok(""test"");
        }

        public Result<string> CreateOrder(string data)
        {
            return Result<string>.Ok(""test"");
        }

        public Result<string> DeleteOrder(int id)
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            // GetOrders should be AllowAnonymous
            Assert.IsTrue(code.Contains(".AllowAnonymous()"),
                "GetOrders with [SmartAllowAnonymous] should emit .AllowAnonymous()");

            // Other 3 endpoints should RequireAuthorization
            var authCount = code.Split(new[] { ".RequireAuthorization()" }, System.StringSplitOptions.None).Length - 1;
            Assert.AreEqual(3, authCount,
                $"3 endpoints should inherit .RequireAuthorization(). Found {authCount}.");

            // AllowAnonymous endpoint should have exactly 1 occurrence
            var anonCount = code.Split(new[] { ".AllowAnonymous()" }, System.StringSplitOptions.None).Length - 1;
            Assert.AreEqual(1, anonCount,
                $"Should have exactly 1 .AllowAnonymous(). Found {anonCount}.");
        }

        // ─── No Authorization using when not needed ───

        [TestMethod]
        public void Should_Not_Include_Authorization_Using_When_No_Roles()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", RequiresAuth = true)]
    public class ItemController
    {
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsFalse(code.Contains("using Microsoft.AspNetCore.Authorization;"),
                "Should not include Authorization using when no Roles are used");
        }
    }
}
