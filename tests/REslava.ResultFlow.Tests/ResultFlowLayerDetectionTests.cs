using REslava.ResultFlow.Generators.ResultFlow;
using System.Linq;

namespace REslava.ResultFlow.Tests;

[TestClass]
public class ResultFlowLayerDetectionTests
{
    // ── DetectFromNamespace — namespace heuristics ───────────────────────────

    [TestMethod]
    public void Layer_Namespace_Controllers_ReturnsPresentation()
    {
        Assert.AreEqual("Presentation", LayerDetector.DetectFromNamespace("MyApp.Controllers"));
        Assert.AreEqual("Presentation", LayerDetector.DetectFromNamespace("MyApp.Controllers.Orders"));
        Assert.AreEqual("Presentation", LayerDetector.DetectFromNamespace("MyApp.Web.Controllers.Orders"));
    }

    [TestMethod]
    public void Layer_Namespace_Application_ReturnsApplication()
    {
        Assert.AreEqual("Application", LayerDetector.DetectFromNamespace("MyApp.Application"));
        Assert.AreEqual("Application", LayerDetector.DetectFromNamespace("MyApp.Application.Orders"));
        Assert.AreEqual("Application", LayerDetector.DetectFromNamespace("MyApp.UseCases.PlaceOrder"));
        Assert.AreEqual("Application", LayerDetector.DetectFromNamespace("MyApp.UseCases"));
    }

    [TestMethod]
    public void Layer_Namespace_Domain_ReturnsDomain()
    {
        Assert.AreEqual("Domain", LayerDetector.DetectFromNamespace("MyApp.Domain"));
        Assert.AreEqual("Domain", LayerDetector.DetectFromNamespace("MyApp.Domain.Entities"));
        Assert.AreEqual("Domain", LayerDetector.DetectFromNamespace("Company.App.Domain.Orders"));
    }

    [TestMethod]
    public void Layer_Namespace_Infrastructure_ReturnsInfrastructure()
    {
        Assert.AreEqual("Infrastructure", LayerDetector.DetectFromNamespace("MyApp.Infrastructure"));
        Assert.AreEqual("Infrastructure", LayerDetector.DetectFromNamespace("MyApp.Infrastructure.Repositories"));
        Assert.AreEqual("Infrastructure", LayerDetector.DetectFromNamespace("MyApp.Repositories"));
        Assert.AreEqual("Infrastructure", LayerDetector.DetectFromNamespace("MyApp.Repositories.Orders"));
    }

    [TestMethod]
    public void Layer_Namespace_NoMatch_ReturnsNull()
    {
        Assert.IsNull(LayerDetector.DetectFromNamespace("MyApp.Services"));
        Assert.IsNull(LayerDetector.DetectFromNamespace("TestNS"));
        Assert.IsNull(LayerDetector.DetectFromNamespace(""));
    }

    [TestMethod]
    public void Layer_Namespace_Presentation_BeatsApplication_WhenBothPresent()
    {
        Assert.AreEqual("Presentation", LayerDetector.DetectFromNamespace("MyApp.Controllers.Application"));
    }

    // ── DomainBoundary attribute detection — via syntax parse ───────────────

    [TestMethod]
    public void Layer_DomainBoundaryAttribute_WithLayer_ReturnsLayer()
    {
        var source = @"
public class MyClass
{
    [DomainBoundary(""Domain"")]
    public void MyMethod() { }
}";
        var tree = CSharpSyntaxTree.ParseText(SourceText.From(source));
        var method = tree.GetRoot().DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var result = LayerDetector.Detect(method, "MyApp.Application");

        Assert.AreEqual("Domain", result, "[DomainBoundary(\"Domain\")] must override namespace heuristic");
    }

    [TestMethod]
    public void Layer_DomainBoundaryAttribute_NoArg_ReturnsNull()
    {
        var source = @"
public class MyClass
{
    [DomainBoundary]
    public void MyMethod() { }
}";
        var tree = CSharpSyntaxTree.ParseText(SourceText.From(source));
        var method = tree.GetRoot().DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var result = LayerDetector.Detect(method, "MyApp.Domain");

        Assert.IsNull(result, "[DomainBoundary] with no arg must return null even when namespace matches");
    }

    // ── End-to-end: subgraph node.Layer populated via generator ─────────────

    [TestMethod]
    public void Layer_Subgraph_NodePopulated_ViaNamespaceHeuristic()
    {
        var source = @"
using System;

namespace MyApp.Domain
{
    public class Order { public int Id { get; } }

    public class Result<T>
    {
        public static Result<T> Ok(T value) => new Result<T>();
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> f) => new Result<TOut>();
        public Result<T> Ensure(Func<T, bool> p, string msg) => this;
    }

    public static class DomainService
    {
        public static Result<Order> ValidateUser(Order o) =>
            Result<Order>.Ok(o).Ensure(x => x.Id > 0, ""invalid"");
    }
}

namespace MyApp.Application
{
    using MyApp.Domain;

    public class OrderService
    {
        [ResultFlow(MaxDepth = 2)]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order())
                .Bind(u => DomainService.ValidateUser(u));
    }
}";

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("PlaceOrder_LayerView"), "_LayerView must be emitted when Domain layer detected");
        Assert.IsTrue(output.Contains("Domain"), "Domain layer subgraph must appear in _LayerView");
        Assert.IsTrue(output.Contains("DomainService"), "DomainService class subgraph must appear");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            new[] { syntaxTree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new REslava.ResultFlow.Generators.ResultFlow.ResultFlowGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        var updatedDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var runResult = updatedDriver.GetRunResult();

        var sb = new System.Text.StringBuilder();
        foreach (var tree in runResult.GeneratedTrees)
        {
            using var writer = new System.IO.StringWriter();
            tree.GetText().Write(writer);
            sb.AppendLine(writer.ToString());
        }

        return sb.ToString();
    }
}
