using REslava.Result.Flow.Generators.ResultFlow;
using System.Collections.Immutable;
using System.Linq;

namespace REslava.Result.Flow.Tests;

[TestClass]
public class ResultFlowStatsTests
{
    // ── 1. _Stats emitted alongside _LayerView ────────────────────────────────
    [TestMethod]
    public void Stats_Emitted_AlongsideLayerView()
    {
        var source = CreateLayerSource();
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("PlaceOrder_LayerView"), "_LayerView must be emitted (prerequisite)");
        Assert.IsTrue(output.Contains("PlaceOrder_Stats"), "_Stats must be emitted alongside _LayerView");
    }

    // ── 2. _Stats not emitted when no layer ──────────────────────────────────
    [TestMethod]
    public void Stats_NotEmitted_WhenNoLayerDetected()
    {
        var source = CreateNoLayerSource();
        var output = RunGenerator(source);

        Assert.IsFalse(output.Contains("_Stats ="), "_Stats must not be emitted when no layer is detected");
    }

    // ── 3. _Stats contains Steps row ─────────────────────────────────────────
    [TestMethod]
    public void Stats_ContainsStepsRow()
    {
        var source = CreateLayerSource();
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Steps"), "_Stats must contain a Steps row");
    }

    // ── 4. _Stats contains Layers crossed row ────────────────────────────────
    [TestMethod]
    public void Stats_ContainsLayersCrossedRow()
    {
        var source = CreateLayerSource();
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Layers crossed"), "_Stats must contain a Layers crossed row");
    }

    // ── 5. _Stats contains Possible errors row ───────────────────────────────
    [TestMethod]
    public void Stats_ContainsPossibleErrorsRow()
    {
        var source = CreateLayerSource();
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Possible errors"), "_Stats must contain a Possible errors row");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string CreateLayerSource() => $@"
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

namespace MyApp.Domain
{{
    using REslava.Result;
    using SharedTypes;
    public static class DomainService
    {{
        public static Result<Order> ValidateUser(Order o) =>
            Result<Order>.Ok(o).Ensure(x => x.Id > 0, u => new ValidationError(""invalid""));
    }}
}}

namespace MyApp.Application
{{
    using REslava.Result;
    using SharedTypes;
    public class OrderService
    {{
        [REslava.Result.Flow.ResultFlow(MaxDepth = 2)]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order())
                .Bind(u => DomainService.ValidateUser(u));
    }}
}}";

    private static string CreateNoLayerSource() => $@"
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

namespace TestNS
{{
    using REslava.Result;
    public class Order {{ public int Id {{ get; }} }}
    public class OrderService
    {{
        [REslava.Result.Flow.ResultFlow(MaxDepth = 2)]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order()).Map(x => x);
    }}
}}";

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
