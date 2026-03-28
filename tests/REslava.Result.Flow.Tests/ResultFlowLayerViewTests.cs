using REslava.Result.Flow.Generators.ResultFlow;
using System.Collections.Immutable;
using System.Linq;

namespace REslava.Result.Flow.Tests;

[TestClass]
public class ResultFlowLayerViewTests
{
    // ── 1. No layer → _LayerView NOT emitted ─────────────────────────────────
    [TestMethod]
    public void LayerView_NotEmitted_WhenNoLayerDetected()
    {
        // Both root and sub-method are in "TestNS" — no Domain/Application/etc. keyword
        var source = CreateCrossMethodSource(
            rootNamespace: "TestNS", rootClass: "OrderService",
            subNamespace: "TestNS", subClass: "DomainService");

        var output = RunGenerator(source);

        Assert.IsFalse(output.Contains("_LayerView ="), "No layer detected → _LayerView constant must not be emitted");
    }

    // ── 2. Domain layer → _LayerView emitted ─────────────────────────────────
    [TestMethod]
    public void LayerView_Emitted_WhenLayerDetected()
    {
        var source = CreateCrossMethodSource(
            rootNamespace: "MyApp.Application", rootClass: "OrderService",
            subNamespace: "MyApp.Domain", subClass: "DomainService");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("PlaceOrder_LayerView"), "_LayerView constant must be emitted when layer is detected");
    }

    // ── 3. _LayerView uses flowchart TD ──────────────────────────────────────
    [TestMethod]
    public void LayerView_FlowchartTD()
    {
        var source = CreateCrossMethodSource(
            rootNamespace: "MyApp.Application", rootClass: "OrderService",
            subNamespace: "MyApp.Domain", subClass: "DomainService");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("flowchart TD"), "LayerView diagram must use flowchart TD (top-down)");
    }

    // ── 4. Sub-method's layer appears as a subgraph ───────────────────────────
    [TestMethod]
    public void LayerView_SubgraphPerLayer()
    {
        var source = CreateCrossMethodSource(
            rootNamespace: "MyApp.Application", rootClass: "OrderService",
            subNamespace: "MyApp.Domain", subClass: "DomainService");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Domain"), "Domain layer subgraph must be emitted");
        Assert.IsTrue(output.Contains("Application"), "Application layer subgraph must be emitted");
    }

    // ── 5. Class subgraph always emitted ─────────────────────────────────────
    [TestMethod]
    public void LayerView_ClassSubgraphAlways()
    {
        var source = CreateCrossMethodSource(
            rootNamespace: "MyApp.Application", rootClass: "OrderService",
            subNamespace: "MyApp.Domain", subClass: "DomainService");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("DomainService"), "Class subgraph must be emitted for sub-method's class");
        Assert.IsTrue(output.Contains("OrderService"), "Class subgraph must be emitted for root method's class");
    }

    // ── 6. SUCCESS terminal emitted ───────────────────────────────────────────
    [TestMethod]
    public void LayerView_SuccessTerminal()
    {
        var source = CreateCrossMethodSource(
            rootNamespace: "MyApp.Application", rootClass: "OrderService",
            subNamespace: "MyApp.Domain", subClass: "DomainService");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("SUCCESS"), "SUCCESS terminal must be emitted in _LayerView");
        Assert.IsTrue(output.Contains("success"), "success classDef must be emitted in _LayerView");
    }

    // ── 7. Edge from root to sub-method present ──────────────────────────────
    [TestMethod]
    public void LayerView_EdgeFromRootToSubMethod()
    {
        var source = CreateCrossMethodSource(
            rootNamespace: "MyApp.Application", rootClass: "OrderService",
            subNamespace: "MyApp.Domain", subClass: "DomainService");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("N_PlaceOrder"), "Root method node must appear in _LayerView");
        Assert.IsTrue(output.Contains("N_ValidateUser"), "Sub-method node must appear in _LayerView");
    }

    // ── 8. Pipeline diagram (Level 2) still also emitted ─────────────────────
    [TestMethod]
    public void LayerView_PipelineDiagramStillEmitted()
    {
        var source = CreateCrossMethodSource(
            rootNamespace: "MyApp.Application", rootClass: "OrderService",
            subNamespace: "MyApp.Domain", subClass: "DomainService");

        var output = RunGenerator(source);

        // Both the pipeline constant and _LayerView constant are present
        Assert.IsTrue(output.Contains("flowchart LR"), "Level-2 pipeline diagram (flowchart LR) must still be emitted");
        Assert.IsTrue(output.Contains("flowchart TD"), "_LayerView (flowchart TD) must also be emitted");
    }

    // ── 9. SUCCESS terminal in pipeline diagram (Level 2) ────────────────────
    [TestMethod]
    public void PipelineDiagram_SuccessTerminal()
    {
        var source = CreateCrossMethodSource(
            rootNamespace: "MyApp.Application", rootClass: "OrderService",
            subNamespace: "MyApp.Domain", subClass: "DomainService");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("SUCCESS([success]):::success"),
            "Level-2 pipeline diagram must emit SUCCESS([success]):::success terminal");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a two-namespace source: root method in <paramref name="rootNamespace"/>,
    /// sub-method in <paramref name="subNamespace"/>.
    /// The sub-method namespace controls which architectural layer is detected.
    /// </summary>
    private static string CreateCrossMethodSource(
        string rootNamespace, string rootClass,
        string subNamespace, string subClass)
    {
        return $@"
using System;
using System.Collections.Immutable;

namespace REslava.Result
{{
    public interface IReason {{ string Message {{ get; }} }}
    public interface IError : IReason {{ }}
    public interface ISuccess : IReason {{ }}
    public interface IResultBase
    {{
        bool IsSuccess {{ get; }}
        bool IsFailure {{ get; }}
        ImmutableList<IReason> Reasons {{ get; }}
        ImmutableList<IError> Errors {{ get; }}
        ImmutableList<ISuccess> Successes {{ get; }}
    }}
    public interface IResultBase<out T> : IResultBase {{ T? Value {{ get; }} }}
    public class Result<T> : IResultBase<T>
    {{
        public bool IsSuccess {{ get; }}
        public bool IsFailure {{ get; }}
        public T? Value {{ get; }}
        public ImmutableList<IReason> Reasons => ImmutableList<IReason>.Empty;
        public ImmutableList<IError> Errors => ImmutableList<IError>.Empty;
        public ImmutableList<ISuccess> Successes => ImmutableList<ISuccess>.Empty;
        public static Result<T> Ok(T value) => new Result<T>();
        public static Result<T> Fail(IError error) => new Result<T>();
        public static Result<T> Fail(string msg) => new Result<T>();
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> f) => new Result<TOut>();
        public Result<T> Ensure(Func<T, bool> p, Func<T, IError> e) => new Result<T>();
        public Result<TOut> Map<TOut>(Func<T, TOut> f) => new Result<TOut>();
        public Result<T> Tap(Action<T> a) => this;
    }}
}}

namespace SharedTypes
{{
    using REslava.Result;
    public class Order {{ public int Id {{ get; }} }}
    public class ValidationError : IError
    {{
        public string Message {{ get; }}
        public ValidationError(string msg) {{ Message = msg; }}
    }}
}}

namespace {subNamespace}
{{
    using REslava.Result;
    using SharedTypes;
    public static class {subClass}
    {{
        public static Result<Order> ValidateUser(Order o) =>
            Result<Order>.Ok(o).Ensure(x => x.Id > 0, u => new ValidationError(""invalid""));
    }}
}}

namespace {rootNamespace}
{{
    using REslava.Result;
    using SharedTypes;
    using {subNamespace};
    public class {rootClass}
    {{
        [REslava.Result.Flow.ResultFlow(MaxDepth = 2)]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order())
                .Bind(u => {subClass}.ValidateUser(u));
    }}
}}";
    }

    private static string RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));

        var references = new System.Collections.Generic.List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ImmutableList<>).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            new[] { syntaxTree },
            references,
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
