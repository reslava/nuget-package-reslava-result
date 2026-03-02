using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace REslava.Result.SourceGenerators.Tests
{
    /// <summary>
    /// Tests for SmartEndpoints Filters, Output Caching, and Rate Limiting:
    /// [SmartFilter], CacheSeconds, RateLimitPolicy on both class and method level.
    /// </summary>
    [TestClass]
    public class SmartEndpoints_FiltersAndCachingTests
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

        // ─── Endpoint Filters ───

        [TestMethod]
        public void Should_Generate_AddEndpointFilter_For_Single_Filter()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    public class LoggingFilter { }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"")]
    public class ItemController
    {
        [SmartFilter(typeof(LoggingFilter))]
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".AddEndpointFilter<global::TestApp.LoggingFilter>()"),
                "Method with [SmartFilter(typeof(LoggingFilter))] should emit .AddEndpointFilter<global::TestApp.LoggingFilter>()");
        }

        [TestMethod]
        public void Should_Generate_Multiple_Filters_In_Declaration_Order()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    public class LoggingFilter { }
    public class ValidationFilter { }

    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"")]
    public class ItemController
    {
        [SmartFilter(typeof(LoggingFilter))]
        [SmartFilter(typeof(ValidationFilter))]
        public Result<string> CreateItem(string name)
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".AddEndpointFilter<global::TestApp.LoggingFilter>()"),
                "Should emit .AddEndpointFilter for LoggingFilter");
            Assert.IsTrue(code.Contains(".AddEndpointFilter<global::TestApp.ValidationFilter>()"),
                "Should emit .AddEndpointFilter for ValidationFilter");

            // Verify declaration order: LoggingFilter before ValidationFilter
            var loggingIndex = code.IndexOf(".AddEndpointFilter<global::TestApp.LoggingFilter>()");
            var validationIndex = code.IndexOf(".AddEndpointFilter<global::TestApp.ValidationFilter>()");
            Assert.IsTrue(loggingIndex < validationIndex,
                "LoggingFilter should appear before ValidationFilter (declaration order)");
        }

        [TestMethod]
        public void Should_Not_Generate_AddEndpointFilter_When_No_SmartFilter()
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

            Assert.IsFalse(code.Contains(".AddEndpointFilter"),
                "Method without [SmartFilter] should not emit .AddEndpointFilter");
        }

        // ─── Output Caching ───

        [TestMethod]
        public void Should_Generate_CacheOutput_For_GET_With_CacheSeconds()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", CacheSeconds = 60)]
    public class ItemController
    {
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".CacheOutput(x => x.Expire(global::System.TimeSpan.FromSeconds(60)))"),
                "GET with CacheSeconds=60 should emit .CacheOutput(x => x.Expire(...60...))");
        }

        [TestMethod]
        public void Should_Not_Generate_CacheOutput_For_POST()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", CacheSeconds = 60)]
    public class ItemController
    {
        public Result<string> CreateItem(string name)
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsFalse(code.Contains(".CacheOutput("),
                "POST method should NOT emit .CacheOutput() even with CacheSeconds=60");
        }

        [TestMethod]
        public void Should_Suppress_CacheOutput_When_Method_CacheSeconds_Is_Minus_One()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", CacheSeconds = 60)]
    public class ItemController
    {
        [AutoMapEndpoint(""/nocache"", CacheSeconds = -1)]
        public Result<string> GetNoCacheItem()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsFalse(code.Contains(".CacheOutput("),
                "Method with CacheSeconds=-1 should suppress cache even when class sets CacheSeconds=60");
        }

        [TestMethod]
        public void Should_Inherit_CacheSeconds_From_Class_Attribute()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", CacheSeconds = 120)]
    public class ItemController
    {
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".CacheOutput(x => x.Expire(global::System.TimeSpan.FromSeconds(120)))"),
                "GET without method CacheSeconds should inherit class CacheSeconds=120");
        }

        // ─── Rate Limiting ───

        [TestMethod]
        public void Should_Generate_RequireRateLimiting()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", RateLimitPolicy = ""api"")]
    public class ItemController
    {
        public Result<string> GetItems()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".RequireRateLimiting(\"api\")"),
                "Class RateLimitPolicy=\"api\" should emit .RequireRateLimiting(\"api\")");
        }

        [TestMethod]
        public void Should_Override_RateLimitPolicy_At_Method_Level()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", RateLimitPolicy = ""api"")]
    public class ItemController
    {
        [AutoMapEndpoint(""/strict"", RateLimitPolicy = ""strict"")]
        public Result<string> GetStrictItem()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsTrue(code.Contains(".RequireRateLimiting(\"strict\")"),
                "Method RateLimitPolicy=\"strict\" should override class \"api\"");
            Assert.IsFalse(code.Contains(".RequireRateLimiting(\"api\")"),
                "Class \"api\" policy should not appear when method overrides it");
        }

        [TestMethod]
        public void Should_Suppress_RateLimiting_With_None_Value()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", RateLimitPolicy = ""api"")]
    public class ItemController
    {
        [AutoMapEndpoint(""/nolimit"", RateLimitPolicy = ""none"")]
        public Result<string> GetUnlimitedItem()
        {
            return Result<string>.Ok(""test"");
        }
    }
}";
            var code = RunGeneratorAndGetExtensions(source);

            Assert.IsFalse(code.Contains(".RequireRateLimiting("),
                "Method RateLimitPolicy=\"none\" should suppress rate limiting even when class sets \"api\"");
        }

        [TestMethod]
        public void Should_Inherit_RateLimitPolicy_From_Class_Attribute()
        {
            var source = @"
using REslava.Result;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace TestApp
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/items"", RateLimitPolicy = ""api"")]
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

            // Both endpoints should inherit the class rate limit policy
            var count = code.Split(new[] { ".RequireRateLimiting(\"api\")" }, System.StringSplitOptions.None).Length - 1;
            Assert.AreEqual(2, count,
                $"Both endpoints should inherit .RequireRateLimiting(\"api\") from class. Found {count} occurrences.");
        }
    }
}
