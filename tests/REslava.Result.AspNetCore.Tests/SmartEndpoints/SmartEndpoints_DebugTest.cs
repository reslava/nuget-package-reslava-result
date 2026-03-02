using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace REslava.Result.SourceGenerators.Tests
{
    [TestClass]
    public class SmartEndpoints_DebugTest
    {
        [TestMethod]
        public void Debug_SmartEndpoints_Basic_Generation()
        {
            // Arrange - Simple test with Result<T>
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
using REslava.Result;
using REslava.Result.SourceGenerators.Generators.SmartEndpoints.Attributes;

namespace TestNamespace
{
    [AutoGenerateEndpoints(RoutePrefix = ""/api/test"")]
    public class TestController
    {
        public Result<string> GetString()
        {
            return Result<string>.Ok(""test"");
        }
    }
}
");

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Result<>).Assembly.Location),
                // Add the SourceGenerator assembly to get the attributes
                MetadataReference.CreateFromFile(typeof(REslava.Result.SourceGenerators.Generators.SmartEndpoints.SmartEndpointsGenerator).Assembly.Location)
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

            // Debug - Print what we got
            System.Diagnostics.Debug.WriteLine($"🔍 GeneratedTrees count: {result.GeneratedTrees.Length}");
            System.Diagnostics.Debug.WriteLine($"🔍 Diagnostics count: {result.Diagnostics.Length}");
            
            foreach (var diagnostic in result.Diagnostics)
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Diagnostic: {diagnostic}");
            }

            foreach (var tree in result.GeneratedTrees)
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Generated file: {tree.FilePath}");
                System.Diagnostics.Debug.WriteLine($"🔍 Content length: {tree.ToString().Length}");
            }

            // Assert
            Assert.AreEqual(0, result.Diagnostics.Length, "Generator should not produce diagnostics");
            Assert.IsTrue(result.GeneratedTrees.Length > 0, "Should generate source files");
        }
    }
}
