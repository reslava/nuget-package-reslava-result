using REslava.Result.Flow.Generators.ResultFlow;
using REslava.Result.Flow.Generators.ResultFlow.CodeGeneration;
using System.Linq;

namespace REslava.Result.Flow.Tests;

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
        // Segment matching: Controllers wins over Application
        Assert.AreEqual("Presentation", LayerDetector.DetectFromNamespace("MyApp.Controllers.Application"));
    }

    // ── DomainBoundary attribute + subgraph node — via generator ────────────

    [TestMethod]
    public void Layer_Subgraph_NodePopulated_ViaNamespaceHeuristic()
    {
        // Sub-method in MyApp.Domain → node.Layer should be "Domain"
        var source = @"
using System;

namespace REslava.Result
{
    public interface IReason { string Message { get; } }
    public interface IError : IReason { }
    public interface IResultBase { }
    public interface IResultBase<out T> : IResultBase { T? Value { get; } }
    public class Result<T> : IResultBase<T>
    {
        public T? Value { get; }
        public static Result<T> Ok(T value) => new Result<T>();
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> f) => new Result<TOut>();
        public Result<T> Ensure(Func<T, bool> p, string msg) => this;
    }
}

namespace MyApp.Domain
{
    using REslava.Result;
    public class Order { public int Id { get; } }
    public static class DomainService
    {
        public static Result<Order> ValidateUser(Order o) =>
            Result<Order>.Ok(o).Ensure(x => x.Id > 0, ""invalid"");
    }
}

namespace MyApp.Application
{
    using REslava.Result;
    using MyApp.Domain;
    public class OrderService
    {
        [REslava.Result.Flow.ResultFlow(MaxDepth = 2)]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order())
                .Bind(u => DomainService.ValidateUser(u));
    }
}";

        var output = RunGenerator(source);

        // Layer is detected → _LayerView emitted with Domain subgraph
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

        var generator = new REslava.Result.Flow.Generators.ResultFlow.ResultFlowGenerator();
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
