using REslava.Result.Flow.Generators.ResultFlow;
using System.Collections.Immutable;
using System.Linq;

namespace REslava.Result.Flow.Tests;

[TestClass]
public class ResultFlowMultiBranchTests
{
    // ── 1. Match renders as hexagon ───────────────────────────────────────────
    [TestMethod]
    public void Match_RendersAsHexagon()
    {
        var output = RunGenerator(CreateMatchSource());

        Assert.IsTrue(output.Contains("{{\"\"Match\"\"}}"), "Match must render as Mermaid hexagon {{...}}");
    }

    // ── 2. Match emits ok edge to SUCCESS ─────────────────────────────────────
    [TestMethod]
    public void Match_EmitsOkEdgeToSuccess()
    {
        var output = RunGenerator(CreateMatchSource());

        Assert.IsTrue(output.Contains("-->|ok| SUCCESS"), "Match must emit -->|ok| SUCCESS edge");
    }

    // ── 3. Match emits fail edge to FAIL (generic 2-branch) ──────────────────
    [TestMethod]
    public void Match_EmitsFailEdgeToFail()
    {
        var output = RunGenerator(CreateMatchSource());

        Assert.IsTrue(output.Contains("-->|fail| FAIL"), "Match must emit -->|fail| FAIL edge");
    }

    // ── 4. SUCCESS node emitted ───────────────────────────────────────────────
    [TestMethod]
    public void Match_SuccessNodeEmitted()
    {
        var output = RunGenerator(CreateMatchSource());

        Assert.IsTrue(output.Contains("SUCCESS([success]):::success"), "SUCCESS terminal node must be emitted");
    }

    // ── 5. FAIL node emitted ──────────────────────────────────────────────────
    [TestMethod]
    public void Match_FailNodeEmitted()
    {
        var output = RunGenerator(CreateMatchSource());

        Assert.IsTrue(output.Contains("FAIL([fail])"), "FAIL terminal node must be emitted");
    }

    // ── 6. Typed N-branch: N typed fail edges emitted ─────────────────────────
    [TestMethod]
    public void Match_TypedBranches_NEdgesEmitted()
    {
        var output = RunGenerator(CreateTypedBranchSource());

        Assert.IsTrue(output.Contains("|ValidationError| FAIL"), "ValidationError branch edge must be emitted");
        Assert.IsTrue(output.Contains("|InventoryError| FAIL"), "InventoryError branch edge must be emitted");
    }

    // ── 7. Typed N-branch: generic |fail| NOT emitted when typed labels exist ─
    [TestMethod]
    public void Match_TypedBranches_NoGenericFailEdge()
    {
        var output = RunGenerator(CreateTypedBranchSource());

        // Typed branches replace the generic fallback
        Assert.IsFalse(output.Contains("-->|fail| FAIL"), "Generic -->|fail| FAIL must not appear when typed branches are extracted");
    }

    // ── 8. Typed N-branch: all edges converge to same FAIL terminal ───────────
    [TestMethod]
    public void Match_TypedBranches_AllConvergeToSharedFail()
    {
        var output = RunGenerator(CreateTypedBranchSource());

        // Both typed edges point to FAIL — shared terminal appears once
        Assert.IsTrue(output.Contains("|ValidationError| FAIL"), "ValidationError must point to FAIL");
        Assert.IsTrue(output.Contains("|InventoryError| FAIL"), "InventoryError must point to FAIL");
        Assert.IsTrue(output.Contains("FAIL("), "Shared FAIL terminal must be emitted");
    }

    // ── 9. Generic fallback when Match has 2 args (no typed N-branch) ─────────
    [TestMethod]
    public void Match_FallsBackToGeneric_WhenTwoArgMatch()
    {
        var output = RunGenerator(CreateMatchSource());

        // 2-argument Match: no typed labels extracted → generic |fail|
        Assert.IsTrue(output.Contains("-->|fail| FAIL"), "2-arg Match must use generic |fail| FAIL");
        Assert.IsFalse(output.Contains("|ValidationError|"), "No typed labels for 2-arg Match");
    }

    // ── 10. Non-Match pipeline: SUCCESS terminal still emitted ────────────────
    [TestMethod]
    public void NonMatch_SuccessTerminalStillEmitted()
    {
        var output = RunGenerator(CreateNoMatchSource());

        Assert.IsTrue(output.Contains("-->|ok| SUCCESS"), "Non-Match pipeline must still emit -->|ok| SUCCESS");
        Assert.IsTrue(output.Contains("SUCCESS([success]):::success"), "SUCCESS terminal node must be emitted");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Simple pipeline ending with a 2-arg Match (generic fail branch).</summary>
    private static string CreateMatchSource() => $@"
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
        public static Result<T> Fail(string msg) => new Result<T>();
        public Result<TOut> Map<TOut>(Func<T, TOut> f) => new Result<TOut>();
        public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<ImmutableList<IError>, TOut> onFailure) => default!;
    }}
}}

namespace TestNS
{{
    using REslava.Result;
    public class Order {{ public int Id {{ get; }} }}
    public class OrderService
    {{
        [REslava.Result.Flow.ResultFlow]
        public string PlaceOrder() =>
            Result<Order>.Ok(new Order())
                .Map(x => x)
                .Match(o => o.Id.ToString(), errors => ""fail"");
    }}
}}";

    /// <summary>Pipeline ending with a 3-arg Match: 1 success + 2 typed error branches.</summary>
    private static string CreateTypedBranchSource() => $@"
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
        public Result<TOut> Map<TOut>(Func<T, TOut> f) => new Result<TOut>();
        public TOut Match<TOut>(
            Func<T, TOut> onSuccess,
            Func<ValidationError, TOut> onValidation,
            Func<InventoryError, TOut> onInventory) => default!;
    }}
}}

namespace TestNS
{{
    using REslava.Result;
    public class Order {{ public int Id {{ get; }} }}
    public class ValidationError : IError {{ public string Message {{ get; }} = """"; }}
    public class InventoryError  : IError {{ public string Message {{ get; }} = """"; }}

    public class OrderService
    {{
        [REslava.Result.Flow.ResultFlow]
        public string PlaceOrder() =>
            Result<Order>.Ok(new Order())
                .Map(x => x)
                .Match(
                    (Order o)           => o.Id.ToString(),
                    (ValidationError v) => v.Message,
                    (InventoryError i)  => i.Message);
    }}
}}";

    /// <summary>Pipeline with no Match — ends on Map.</summary>
    private static string CreateNoMatchSource() => $@"
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
        public Result<TOut> Map<TOut>(Func<T, TOut> f) => new Result<TOut>();
    }}
}}

namespace TestNS
{{
    using REslava.Result;
    public class Order {{ public int Id {{ get; }} }}
    public class OrderService
    {{
        [REslava.Result.Flow.ResultFlow]
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
