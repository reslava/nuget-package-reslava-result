using REslava.ResultFlow.Generators.ResultFlow;
using System.Linq;

namespace REslava.ResultFlow.Tests;

[TestClass]
public class ResultFlowErrorSurfaceTests
{
    // ── 1. _ErrorSurface emitted alongside _LayerView ─────────────────────────
    [TestMethod]
    public void ErrorSurface_Emitted_AlongsideLayerView()
    {
        var output = RunGenerator(CreateLayerSource());

        Assert.IsTrue(output.Contains("PlaceOrder_LayerView"), "_LayerView must be emitted (prerequisite)");
        Assert.IsTrue(output.Contains("PlaceOrder_ErrorSurface"), "_ErrorSurface must be emitted alongside _LayerView");
    }

    // ── 2. _ErrorSurface not emitted when no layer ────────────────────────────
    [TestMethod]
    public void ErrorSurface_NotEmitted_WhenNoLayerDetected()
    {
        var output = RunGenerator(CreateNoLayerSource());

        Assert.IsFalse(output.Contains("_ErrorSurface ="), "_ErrorSurface must not be emitted when no layer is detected");
    }

    // ── 3. _ErrorSurface contains FAIL terminal ───────────────────────────────
    [TestMethod]
    public void ErrorSurface_ContainsFailTerminal()
    {
        var output = RunGenerator(CreateLayerSource());

        Assert.IsTrue(output.Contains("FAIL([fail]):::failure"), "_ErrorSurface must contain FAIL([fail]):::failure terminal");
    }

    // ── 4. _ErrorSurface uses flowchart LR ────────────────────────────────────
    [TestMethod]
    public void ErrorSurface_UsesFlowchartLR()
    {
        var output = RunGenerator(CreateLayerSource());

        // _ErrorSurface uses flowchart LR (horizontal); _LayerView uses flowchart TD (vertical)
        Assert.IsTrue(output.Contains("flowchart LR"), "_ErrorSurface must use flowchart LR");
        Assert.IsTrue(output.Contains("flowchart TD"), "_LayerView must use flowchart TD");
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

    private static int CountOccurrences(string text, string pattern)
    {
        int count = 0, index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }
}
