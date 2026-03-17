using REslava.ResultFlow.Generators.ResultFlow;
using System.Linq;

namespace REslava.ResultFlow.Tests;

[TestClass]
public class ResultFlowStatsTests
{
    // ── 1. _Stats emitted alongside _LayerView ────────────────────────────────
    [TestMethod]
    public void Stats_Emitted_AlongsideLayerView()
    {
        var output = RunGenerator(CreateLayerSource());

        Assert.IsTrue(output.Contains("PlaceOrder_LayerView"), "_LayerView must be emitted (prerequisite)");
        Assert.IsTrue(output.Contains("PlaceOrder_Stats"), "_Stats must be emitted alongside _LayerView");
    }

    // ── 2. _Stats not emitted when no layer ──────────────────────────────────
    [TestMethod]
    public void Stats_NotEmitted_WhenNoLayerDetected()
    {
        var output = RunGenerator(CreateNoLayerSource());

        Assert.IsFalse(output.Contains("_Stats"), "_Stats must not be emitted when no layer is detected");
    }

    // ── 3. _Stats contains Steps row ─────────────────────────────────────────
    [TestMethod]
    public void Stats_ContainsStepsRow()
    {
        var output = RunGenerator(CreateLayerSource());

        Assert.IsTrue(output.Contains("Steps"), "_Stats must contain a Steps row");
    }

    // ── 4. _Stats contains Layers crossed row ────────────────────────────────
    [TestMethod]
    public void Stats_ContainsLayersCrossedRow()
    {
        var output = RunGenerator(CreateLayerSource());

        Assert.IsTrue(output.Contains("Layers crossed"), "_Stats must contain a Layers crossed row");
    }

    // ── 5. _Stats contains Possible errors row ───────────────────────────────
    [TestMethod]
    public void Stats_ContainsPossibleErrorsRow()
    {
        var output = RunGenerator(CreateLayerSource());

        Assert.IsTrue(output.Contains("Possible errors"), "_Stats must contain a Possible errors row");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string CreateLayerSource() => @"
using System;

namespace MyApp.Domain
{
    public class Order { public int Id { get; } }

    public class Result<T>
    {
        public static Result<T> Ok(T value) => new Result<T>();
        public static Result<T> Fail(string msg) => new Result<T>();
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> f) => new Result<TOut>();
        public Result<T> Ensure(Func<T, bool> predicate, string errorMessage) => new Result<T>();
        public Result<TOut> Map<TOut>(Func<T, TOut> mapper) => new Result<TOut>();
        public Result<T> Tap(Action<T> action) => this;
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

    private static string CreateNoLayerSource() => @"
using System;

namespace TestNS
{
    public class Order { public int Id { get; } }

    public class Result<T>
    {
        public static Result<T> Ok(T value) => new Result<T>();
        public Result<TOut> Map<TOut>(Func<T, TOut> f) => new Result<TOut>();
    }

    public class OrderService
    {
        [ResultFlow(MaxDepth = 2)]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order()).Map(x => x);
    }
}";

    private static string RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            new[] { syntaxTree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ResultFlowGenerator();
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
