using REslava.Result.Flow.Generators.ResultFlow;
using System.Collections.Immutable;
using System.Linq;

namespace REslava.Result.Flow.Tests;

[TestClass]
public class ResultFlowErrorSurfaceTests
{
    // ── 1. _ErrorSurface emitted alongside _LayerView ────────────────────────
    [TestMethod]
    public void ErrorSurface_Emitted_AlongsideLayerView()
    {
        var source = CreateLayerSource();
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("PlaceOrder_LayerView"), "_LayerView must be emitted (prerequisite)");
        Assert.IsTrue(output.Contains("PlaceOrder_ErrorSurface"), "_ErrorSurface must be emitted alongside _LayerView");
    }

    // ── 2. _ErrorSurface not emitted when no layer ────────────────────────────
    [TestMethod]
    public void ErrorSurface_NotEmitted_WhenNoLayerDetected()
    {
        var source = CreateNoLayerSource();
        var output = RunGenerator(source);

        Assert.IsFalse(output.Contains("_ErrorSurface"), "_ErrorSurface must not be emitted when no layer is detected");
    }

    // ── 3. _ErrorSurface contains FAIL terminal ───────────────────────────────
    [TestMethod]
    public void ErrorSurface_ContainsFailTerminal()
    {
        var source = CreateLayerSource();
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("FAIL([fail]):::failure"), "_ErrorSurface must contain FAIL([fail]):::failure terminal");
    }

    // ── 4. _ErrorSurface uses flowchart TD ────────────────────────────────────
    [TestMethod]
    public void ErrorSurface_UsesFlowchartTD()
    {
        var source = CreateLayerSource();
        var output = RunGenerator(source);

        // _ErrorSurface is a flowchart TD — count occurrences to confirm it's there alongside _LayerView
        var tdCount = CountOccurrences(output, "flowchart TD");
        Assert.IsTrue(tdCount >= 2, $"Expected at least 2 'flowchart TD' occurrences (_LayerView + _ErrorSurface), got {tdCount}");
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
