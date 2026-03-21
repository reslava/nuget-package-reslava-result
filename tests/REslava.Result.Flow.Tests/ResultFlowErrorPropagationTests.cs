using REslava.Result.Flow.Generators.ResultFlow;
using System.Collections.Immutable;
using System.Linq;

namespace REslava.Result.Flow.Tests;

[TestClass]
public class ResultFlowErrorPropagationTests
{
    // ── 1. Not emitted when no layer detected ─────────────────────────────────
    [TestMethod]
    public void ErrorPropagation_NotEmitted_WhenNoLayerDetected()
    {
        var source = CreateNoLayerSource();
        var output = RunGenerator(source);

        Assert.IsFalse(output.Contains("_ErrorPropagation"),
            "_ErrorPropagation must not be emitted when no layer is detected");
    }

    // ── 2. Emitted when layer and error detected ───────────────────────────────
    [TestMethod]
    public void ErrorPropagation_Emitted_WhenLayerAndErrorDetected()
    {
        var source = CreateLayerSource();
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("PlaceOrder_LayerView"), "_LayerView must be emitted (prerequisite)");
        Assert.IsTrue(output.Contains("PlaceOrder_ErrorPropagation"),
            "_ErrorPropagation must be emitted when layer + error detected");
    }

    // ── 3. Uses flowchart TD ──────────────────────────────────────────────────
    [TestMethod]
    public void ErrorPropagation_FlowchartTD()
    {
        var source = CreateLayerSource();
        var output = RunGenerator(source);

        // _ErrorPropagation and _LayerView use flowchart TD; _ErrorSurface uses flowchart LR
        var tdCount = CountOccurrences(output, "flowchart TD");
        Assert.IsTrue(tdCount >= 2,
            $"Expected at least 2 'flowchart TD' (_LayerView + _ErrorPropagation), got {tdCount}");
    }

    // ── 4. Subgraph per layer ─────────────────────────────────────────────────
    [TestMethod]
    public void ErrorPropagation_SubgraphPerLayer()
    {
        var source = CreateLayerSource();
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("\"Domain\""),
            "_ErrorPropagation must emit a Domain subgraph");
    }

    // ── 5. Error node per type ────────────────────────────────────────────────
    [TestMethod]
    public void ErrorPropagation_ErrorNodePerType()
    {
        var source = CreateLayerSource();
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("ValidationError"),
            "_ErrorPropagation must contain a ValidationError node");
    }

    // ── 6. FAIL terminal ──────────────────────────────────────────────────────
    [TestMethod]
    public void ErrorPropagation_FailTerminal()
    {
        var source = CreateLayerSource();
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("FAIL([fail]):::failure"),
            "_ErrorPropagation must contain FAIL([fail]):::failure terminal");
    }

    // ── 7. Edge from error node to FAIL ───────────────────────────────────────
    [TestMethod]
    public void ErrorPropagation_EdgeFromErrorToFail()
    {
        var source = CreateLayerSource();
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("--> FAIL([fail]):::failure"),
            "_ErrorPropagation must have edges from error nodes to FAIL");
    }

    // ── 8. Two layers → two subgraphs ─────────────────────────────────────────
    [TestMethod]
    public void ErrorPropagation_TwoLayers_TwoSubgraphs()
    {
        var source = CreateTwoLayerSource();
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("PlaceOrder_ErrorPropagation"), "_ErrorPropagation must be emitted");
        Assert.IsTrue(output.Contains("\"Application\""), "Application subgraph expected");
        Assert.IsTrue(output.Contains("\"Domain\""), "Domain subgraph expected");
    }

    // ── 9. Not emitted when layer detected but no errors ─────────────────────
    [TestMethod]
    public void ErrorPropagation_NotEmitted_WhenNoErrors()
    {
        var source = CreateLayerNoErrorsSource();
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("PlaceOrder_LayerView"), "_LayerView must be emitted (prerequisite)");
        Assert.IsFalse(output.Contains("_ErrorPropagation"),
            "_ErrorPropagation must not be emitted when no errors found in any layer");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string ReslavaResultStub() => @"
using System;
using System.Collections.Immutable;

namespace REslava.Result
{
    public interface IReason { string Message { get; } }
    public interface IError : IReason { }
    public interface ISuccess : IReason { }
    public interface IResultBase
    {
        bool IsSuccess { get; }
        bool IsFailure { get; }
        ImmutableList<IReason> Reasons { get; }
        ImmutableList<IError> Errors { get; }
        ImmutableList<ISuccess> Successes { get; }
    }
    public interface IResultBase<out T> : IResultBase { T? Value { get; } }
    public class Result<T> : IResultBase<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure { get; }
        public T? Value { get; }
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
    }
}";

    /// <summary>Root in Application, sub-method in Domain — ValidationError in Domain.</summary>
    private static string CreateLayerSource() => ReslavaResultStub() + @"

namespace SharedTypes
{
    using REslava.Result;
    public class Order { public int Id { get; } }
    public class ValidationError : IError
    {
        public string Message { get; }
        public ValidationError(string msg) { Message = msg; }
    }
}

namespace MyApp.Domain
{
    using REslava.Result;
    using SharedTypes;
    public static class DomainService
    {
        public static Result<Order> ValidateUser(Order o) =>
            Result<Order>.Ok(o).Ensure(x => x.Id > 0, u => new ValidationError(""invalid""));
    }
}

namespace MyApp.Application
{
    using REslava.Result;
    using SharedTypes;
    using MyApp.Domain;
    public class OrderService
    {
        [REslava.Result.Flow.ResultFlow(MaxDepth = 2)]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order())
                .Bind(u => DomainService.ValidateUser(u));
    }
}";

    /// <summary>No layer namespace — no _LayerView, no _ErrorPropagation.</summary>
    private static string CreateNoLayerSource() => ReslavaResultStub() + @"

namespace TestNS
{
    using REslava.Result;
    public class Order { public int Id { get; } }
    public class OrderService
    {
        [REslava.Result.Flow.ResultFlow(MaxDepth = 2)]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order()).Map(x => x);
    }
}";

    /// <summary>Errors in two layers: Application (root Ensure) + Domain (sub-method Ensure).</summary>
    private static string CreateTwoLayerSource() => ReslavaResultStub() + @"

namespace SharedTypes
{
    using REslava.Result;
    public class Order { public int Id { get; } }
    public class ValidationError : IError
    {
        public string Message { get; }
        public ValidationError(string msg) { Message = msg; }
    }
    public class ApplicationError : IError
    {
        public string Message { get; }
        public ApplicationError(string msg) { Message = msg; }
    }
}

namespace MyApp.Domain
{
    using REslava.Result;
    using SharedTypes;
    public static class DomainService
    {
        public static Result<Order> ValidateUser(Order o) =>
            Result<Order>.Ok(o).Ensure(x => x.Id > 0, u => new ValidationError(""invalid""));
    }
}

namespace MyApp.Application
{
    using REslava.Result;
    using SharedTypes;
    using MyApp.Domain;
    public class OrderService
    {
        [REslava.Result.Flow.ResultFlow(MaxDepth = 2)]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order())
                .Ensure(x => x.Id > 0, _ => new ApplicationError(""bad request""))
                .Bind(u => DomainService.ValidateUser(u));
    }
}";

    /// <summary>Layer detected (Domain sub-method) but no errors produced anywhere.</summary>
    private static string CreateLayerNoErrorsSource() => ReslavaResultStub() + @"

namespace SharedTypes
{
    using REslava.Result;
    public class Order { public int Id { get; } }
}

namespace MyApp.Domain
{
    using REslava.Result;
    using SharedTypes;
    public static class DomainService
    {
        public static Result<Order> Transform(Order o) =>
            Result<Order>.Ok(o).Map(x => x);
    }
}

namespace MyApp.Application
{
    using REslava.Result;
    using SharedTypes;
    using MyApp.Domain;
    public class OrderService
    {
        [REslava.Result.Flow.ResultFlow(MaxDepth = 2)]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order())
                .Bind(u => DomainService.Transform(u));
    }
}";

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
