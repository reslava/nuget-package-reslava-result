using REslava.Result.Flow.Generators.ResultFlow;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace REslava.Result.Flow.Tests;

/// <summary>
/// Tests for Pipeline Metadata Foundation (v1.51.0):
/// NodeId stability, NodeId shift on reorder, PipelineId uniqueness across overloads,
/// namespace in _Info, sourceLine correctness on vertical chains.
/// </summary>
[TestClass]
public class PipelineMetadataTests
{
    // ── Infrastructure ────────────────────────────────────────────────────────

    private sealed class DictAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> _values;
        public DictAnalyzerConfigOptions(Dictionary<string, string> v) => _values = v;
        public override bool TryGetValue(string key, out string value) =>
            _values.TryGetValue(key, out value!);
    }

    private sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private static readonly AnalyzerConfigOptions Empty =
            new DictAnalyzerConfigOptions(new Dictionary<string, string>());
        private readonly AnalyzerConfigOptions _global;
        public TestAnalyzerConfigOptionsProvider(Dictionary<string, string> props) =>
            _global = new DictAnalyzerConfigOptions(props);
        public override AnalyzerConfigOptions GlobalOptions => _global;
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => Empty;
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => Empty;
    }

    private static List<MetadataReference> CommonRefs() => new List<MetadataReference>
    {
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(ImmutableList<>).Assembly.Location),
    };

    private static string RunFlowDiagram(string source,
        Dictionary<string, string>? buildProps = null, string? filePath = null)
    {
        var syntaxTree = filePath != null
            ? CSharpSyntaxTree.ParseText(SourceText.From(source), path: filePath)
            : CSharpSyntaxTree.ParseText(SourceText.From(source));

        var compilation = CSharpCompilation.Create(
            "PipelineMetaFlowCompilation",
            new[] { syntaxTree },
            CommonRefs(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ResultFlowGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        if (buildProps != null)
            driver = driver.WithUpdatedAnalyzerConfigOptions(
                new TestAnalyzerConfigOptionsProvider(buildProps));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var sb = new System.Text.StringBuilder();
        foreach (var tree in driver.GetRunResult().GeneratedTrees)
        {
            using var w = new System.IO.StringWriter();
            tree.GetText().Write(w);
            sb.AppendLine(w.ToString());
        }
        return sb.ToString();
    }

    private static IReadOnlyList<(string hint, string text)> RunRegistry(
        string source, Dictionary<string, string>? buildProps = null, string? filePath = null)
    {
        var syntaxTree = filePath != null
            ? CSharpSyntaxTree.ParseText(SourceText.From(source), path: filePath)
            : CSharpSyntaxTree.ParseText(SourceText.From(source));

        var compilation = CSharpCompilation.Create(
            "PipelineMetaRegistryCompilation",
            new[] { syntaxTree },
            CommonRefs(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ResultFlowRegistryGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        if (buildProps != null)
            driver = driver.WithUpdatedAnalyzerConfigOptions(
                new TestAnalyzerConfigOptionsProvider(buildProps));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        return driver.GetRunResult().GeneratedTrees
            .Select(t => { using var w = new System.IO.StringWriter(); t.GetText().Write(w); return (t.FilePath, w.ToString()); })
            .ToList();
    }

    // ── NodeId stability ──────────────────────────────────────────────────────

    [TestMethod]
    public void NodeId_SameSource_SameIdsAcrossRuns()
    {
        // Identical source run twice → identical %% correlation lines (FNV-1a is deterministic)
        var chain = SimpleSource("OrderService", "Process", "GetOrder(cmd).Bind(Validate).Map(ToDto)");
        var output1 = RunFlowDiagram(chain);
        var output2 = RunFlowDiagram(chain);

        var lines1 = output1.Split('\n').Where(l => l.StartsWith("%%") && l.Contains("\u2192")).Distinct().OrderBy(x => x).ToArray();
        var lines2 = output2.Split('\n').Where(l => l.StartsWith("%%") && l.Contains("\u2192")).Distinct().OrderBy(x => x).ToArray();

        Assert.IsTrue(lines1.Length > 0, "At least one correlation line expected");
        CollectionAssert.AreEqual(lines1, lines2, "NodeIds must be deterministic across runs");
    }

    [TestMethod]
    public void NodeId_NodeIds_PresentInRegistryInfo()
    {
        // nodeIds[] in _Info must be non-empty for a chain with Bind + Map steps.
        // Using using-inside-namespace per §8.3; class-specific lookup per §8.4.
        var outputs = RunRegistry(BaseSource("TestNS", "OrderService", "Process",
            "GetOrder(cmd).Bind(Validate).Map(ToDto)"));
        var registry = outputs.FirstOrDefault(o => o.hint.Contains("OrderService_PipelineRegistry")).text;

        Assert.IsNotNull(registry, "OrderService_PipelineRegistry file must be generated");
        Assert.IsTrue(registry.Contains("\\\"nodeIds\\\":["), "nodeIds array must appear in _Info");
        Assert.IsFalse(registry.Contains("\\\"nodeIds\\\":[]"), "nodeIds must not be empty for a 2-step chain");
    }

    // ── NodeId reorder ────────────────────────────────────────────────────────

    [TestMethod]
    public void NodeId_Reordered_ProducesDifferentIds()
    {
        // .Bind(A).Map(B) vs .Map(B).Bind(A) — same step names, different positions
        // → different NodeIds because the hash includes the step index
        var outputBM = RunFlowDiagram(SimpleSource("Svc", "Op", "Get(cmd).Bind(A).Map(B)"));
        var outputMB = RunFlowDiagram(SimpleSource("Svc", "Op", "Get(cmd).Map(B).Bind(A)"));

        var idsBM = outputBM.Split('\n').Where(l => l.StartsWith("%%") && l.Contains("\u2192")).Distinct().ToHashSet();
        var idsMB = outputMB.Split('\n').Where(l => l.StartsWith("%%") && l.Contains("\u2192")).Distinct().ToHashSet();

        Assert.IsTrue(idsBM.Count > 0, "BM chain must have correlation lines");
        Assert.IsFalse(idsBM.SetEquals(idsMB),
            "Reordered steps must produce different NodeIds (index is part of the hash)");
    }

    // ── PipelineId uniqueness across overloads ────────────────────────────────

    [TestMethod]
    public void PipelineId_Overloads_AreUnique()
    {
        // Two methods with the same name but different parameter types → different pipelineIds.
        // using inside namespace per §8.3; class-specific lookup per §8.4.
        const string source = Stubs + @"
namespace TestNS
{
    using REslava.Result;
    public class OrderService
    {
        [REslava.Result.Flow.ResultFlow]
        public Result<Order> Process(string cmd) => Result<Order>.Fail(new UserError(""x""));
        [REslava.Result.Flow.ResultFlow]
        public Result<Order> Process(int id) => Result<Order>.Fail(new UserError(""x""));
    }
}";
        var outputs = RunRegistry(source);
        var registry = outputs.FirstOrDefault(o => o.hint.Contains("OrderService_PipelineRegistry")).text;

        Assert.IsNotNull(registry, "OrderService registry must be generated");
        // In the raw .g.cs source, the JSON string is escaped as \"pipelineId\":\"xxxxxxxx\"
        var pipelineIds = Regex.Matches(registry, "\\\\\"pipelineId\\\\\":\\\\\"([0-9a-f]{8})\\\\\"")
            .Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .ToArray();

        Assert.AreEqual(2, pipelineIds.Length, "Two overloads must produce two _Info constants");
        Assert.AreNotEqual(pipelineIds[0], pipelineIds[1],
            "Overloads with different parameter types must have different pipelineIds");
    }

    // ── Namespace in _Info ────────────────────────────────────────────────────

    [TestMethod]
    public void Namespace_InInfo_IsCorrect()
    {
        // using inside namespace per §8.3; class-specific lookup per §8.4.
        const string source = Stubs + @"
namespace MyApp.Services.Orders
{
    using REslava.Result;
    public class OrderService
    {
        [REslava.Result.Flow.ResultFlow]
        public Result<Order> Process(string cmd) => Result<Order>.Fail(new UserError(""x""));
    }
}";
        var outputs = RunRegistry(source);
        var registry = outputs.FirstOrDefault(o => o.hint.Contains("OrderService_PipelineRegistry")).text;

        Assert.IsNotNull(registry, "OrderService registry must be generated");
        // In raw .g.cs text: \"namespace\":\"MyApp.Services.Orders\"
        Assert.IsTrue(registry.Contains("\\\"namespace\\\":\\\"MyApp.Services.Orders\\\""),
            "Namespace must appear verbatim in _Info JSON");
    }

    // ── SourceLine correctness on vertical chains ─────────────────────────────

    [TestMethod]
    public void SourceLine_VerticalChain_EachStepHasDistinctLine()
    {
        // Each chain step on its own line → click directives must all have different line numbers.
        // Both _Diagram and _TypeFlow emit identical click lines → deduplicate before checking.
        const string source = @"
namespace TestNS
{
    public class Svc
    {
        [REslava.Result.Flow.ResultFlow]
        public string Process(string cmd)
            => GetResult(cmd)
                .Bind(Step1)
                .Map(Step2);
    }
}";
        var output = RunFlowDiagram(source,
            buildProps: new Dictionary<string, string> { ["build_property.ResultFlowLinkMode"] = "vscode" },
            filePath: "C:/src/Svc.cs");

        // vscode://file/ URLs appear in both _Diagram and _TypeFlow → deduplicate
        var clickLines = Regex.Matches(output, "vscode://file/[^\"]+:(\\d+)")
            .Cast<Match>()
            .Select(m => int.Parse(m.Groups[1].Value))
            .Distinct()
            .ToArray();

        Assert.IsTrue(clickLines.Length >= 2, "At least 2 distinct click directives for Bind + Map steps");
        Assert.AreEqual(clickLines.Length, clickLines.Distinct().Count(),
            "Each step on its own line must produce a distinct source line in the click directive");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string SimpleSource(string cls, string method, string chain) => $@"
namespace TestNS
{{
    public class {cls}
    {{
        [REslava.Result.Flow.ResultFlow]
        public string {method}(string cmd) => {chain};
    }}
}}";

    /// <summary>
    /// Builds a registry test source with stubs + using inside namespace (§8.3) for proper
    /// return-type resolution, and includes helper methods for the Bind/Map steps.
    /// </summary>
    private static string BaseSource(string ns, string cls, string method, string chain) =>
        Stubs + $@"
namespace {ns}
{{
    using REslava.Result;
    public class {cls}
    {{
        [REslava.Result.Flow.ResultFlow]
        public Result<Order> {method}(string cmd) => {chain};
        static Result<Order> GetOrder(string cmd) => Result<Order>.Ok(new Order(1));
        static Result<Order> Validate(Order o) => Result<Order>.Ok(o);
        static Order ToDto(Order o) => o;
    }}
}}";

    // ── PipelineId comment in diagram ─────────────────────────────────────────

    [TestMethod]
    public void PipelineId_Comment_PresentInDiagram()
    {
        // %% pipelineId: must be emitted in every generated _Diagram constant
        var output = RunFlowDiagram(BaseSource("TestNS", "Svc", "Op",
            "GetOrder(cmd).Bind(Validate).Map(ToDto)"));
        Assert.IsTrue(output.Contains("%% pipelineId:"),
            "Generated diagram must contain %% pipelineId: comment");
    }

    // ── FAIL node annotation ──────────────────────────────────────────────────

    [TestMethod]
    public void FailNode_OneToThreeErrors_InlineFormat()
    {
        // 2 distinct error types → FAIL node uses inline "fail\nErr1\nErr2" (no tooltip span)
        // Note: ℹ️ appears in Gatekeeper predicate tooltips — check the FAIL node format directly.
        // In verbatim-string notation: FAIL([""fail\nNotFound\nValidation""])
        const string source = EnsureStubs + @"
namespace TestNS
{
    using REslava.Result;
    public class Svc
    {
        [REslava.Result.Flow.ResultFlow]
        public Result<Order> Process(Order o) =>
            GetOrder(o)
                .Ensure(x => x.Id > 0,   x => new NotFoundError(""a""))
                .Ensure(x => x.Id < 100, x => new ValidationError(""b""));
        static Result<Order> GetOrder(Order o) => Result<Order>.Ok(o);
    }
}";
        var output = RunFlowDiagram(source);
        Assert.IsTrue(output.Contains("fail\\n"),
            "1-3 errors must use inline newline format (fail\\nErr)");
        // FAIL node tooltip format uses <span — must NOT be present for 1-3 errors
        Assert.IsFalse(output.Contains("FAIL([\"\"<span"),
            "1-3 errors FAIL node must not use tooltip <span format");
    }

    [TestMethod]
    public void FailNode_FourPlusErrors_TooltipFormat()
    {
        // 4 distinct error types → FAIL node uses tooltip <span title='...'>ℹ️fail</span>
        const string source = EnsureStubs + @"
namespace TestNS
{
    using REslava.Result;
    public class Svc
    {
        [REslava.Result.Flow.ResultFlow]
        public Result<Order> Process(Order o) =>
            GetOrder(o)
                .Ensure(x => x.Id > 0,   x => new NotFoundError(""a""))
                .Ensure(x => x.Id < 100, x => new ValidationError(""b""))
                .Ensure(x => x.Id != 42, x => new DatabaseError(""c""))
                .Ensure(x => x.Id != 99, x => new AuthError(""d""));
        static Result<Order> GetOrder(Order o) => Result<Order>.Ok(o);
    }
}";
        var output = RunFlowDiagram(source);
        Assert.IsTrue(output.Contains("\u2139\uFE0F"),
            "4+ errors must use tooltip ℹ️ emoji");
        Assert.IsTrue(output.Contains("span title="),
            "4+ errors must use <span title=...> tooltip");
        Assert.IsFalse(output.Contains("fail\\n"),
            "4+ errors must not use inline newline format");
    }

    // ── Depth-2 error scan ─────────────────────────────────────────────────────

    [TestMethod]
    public void ErrorTypes_DepthTwoScan_PopulatesErrorTypes()
    {
        // The depth-2 body scan descends into methods called by the pipeline method.
        // UserService.ValidateUser contains Fail(new TestScanError(...)) one level deep.
        // Bind is defined in EnsureStubs so the semantic model can resolve the lambda body
        // and descend into ValidateUser.
        // → registry _Info errorTypes must include TestScanError even though it is not
        //   directly in the Bind lambda body.
        const string source = EnsureStubs + @"
namespace TestNS
{
    using REslava.Result;
    public class TestScanError : IError { public string Message { get; } public TestScanError(string m) {} }
    public class UserService
    {
        public static Result<Order> ValidateUser(Order o) =>
            o.Id > 0 ? Result<Order>.Ok(o) : Result<Order>.Fail(new TestScanError(""invalid""));
    }
    public class OrderService
    {
        [REslava.Result.Flow.ResultFlow]
        public Result<Order> Process(string cmd) =>
            GetOrder(cmd).Bind(o => UserService.ValidateUser(o));
        static Result<Order> GetOrder(string cmd) => Result<Order>.Ok(new Order(1));
    }
}";
        var outputs = RunRegistry(source);
        var registry = outputs.FirstOrDefault(o => o.hint.Contains("OrderService_PipelineRegistry")).text;

        Assert.IsNotNull(registry, "OrderService_PipelineRegistry must be generated");
        // In raw .g.cs text: \"errorTypes\":[\"TestScanError\"]
        Assert.IsTrue(registry.Contains("\\\"TestScanError\\\""),
            "Depth-2 body scan must detect TestScanError from called method ValidateUser");
    }

    // ── Stubs ────────────────────────────────────────────────────────────────

    private const string Stubs = @"
using System;
using System.Collections.Immutable;
namespace REslava.Result
{
    public interface IReason { string Message { get; } }
    public interface IError : IReason { }
    public interface IResultBase { bool IsSuccess { get; } bool IsFailure { get; } ImmutableList<IReason> Reasons { get; } ImmutableList<IError> Errors { get; } }
    public interface IResultBase<out T> : IResultBase { T? Value { get; } }
    public class Result<T> : IResultBase<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure { get; }
        public T? Value { get; }
        public ImmutableList<IReason> Reasons => ImmutableList<IReason>.Empty;
        public ImmutableList<IError> Errors => ImmutableList<IError>.Empty;
        public static Result<T> Ok(T value) => new Result<T>();
        public static Result<T> Fail(IError error) => new Result<T>();
    }
    public class UserError : IError { public UserError(string m) { } public string Message => """"; }
    public record Order(int Id);
}
";

    /// <summary>
    /// Extended stubs that add Ensure + four named error types — used for FAIL node annotation tests.
    /// Ensure must be defined so the semantic model resolves the call and GetPossibleErrors runs.
    /// </summary>
    private const string EnsureStubs = @"
using System;
using System.Collections.Immutable;
namespace REslava.Result
{
    public interface IReason { string Message { get; } }
    public interface IError : IReason { }
    public interface IResultBase { bool IsSuccess { get; } bool IsFailure { get; } ImmutableList<IReason> Reasons { get; } ImmutableList<IError> Errors { get; } }
    public interface IResultBase<out T> : IResultBase { T? Value { get; } }
    public class Result<T> : IResultBase<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure { get; }
        public T? Value { get; }
        public ImmutableList<IReason> Reasons => ImmutableList<IReason>.Empty;
        public ImmutableList<IError> Errors => ImmutableList<IError>.Empty;
        public static Result<T> Ok(T value) => new Result<T>();
        public static Result<T> Fail(IError error) => new Result<T>();
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> f) => new Result<TOut>();
        public Result<T> Ensure(Func<T, bool> p, Func<T, IError> e) => new Result<T>();
        public Result<TOut> Map<TOut>(Func<T, TOut> f) => new Result<TOut>();
    }
    public class NotFoundError   : IError { public string Message { get; } public NotFoundError(string m) {} }
    public class ValidationError : IError { public string Message { get; } public ValidationError(string m) {} }
    public class DatabaseError   : IError { public string Message { get; } public DatabaseError(string m) {} }
    public class AuthError       : IError { public string Message { get; } public AuthError(string m) {} }
    public record Order(int Id);
}
";
}
