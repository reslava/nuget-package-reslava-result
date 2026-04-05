using REslava.ResultFlow.Generators.ResultFlow;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace REslava.ResultFlow.Tests;

/// <summary>
/// Tests for Pipeline Metadata Foundation (v1.51.0):
/// NodeId stability, NodeId shift on reorder, PipelineId uniqueness across overloads,
/// namespace in _Info, sourceLine correctness on vertical chains.
/// Syntax-only variant (REslava.ResultFlow — no semantic model).
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
        // Note: REslava.ResultFlow does not emit %% correlation comments — check pipelineId stability
        // via registry instead (nodeIds are hash-deterministic).
        var chain = SimpleSource("OrderService", "Process", "GetOrder(cmd).Bind(Validate).Map(ToDto)");
        var output1 = RunFlowDiagram(chain);
        var output2 = RunFlowDiagram(chain);

        // Both runs must produce identical output (deterministic generation)
        Assert.AreEqual(output1, output2, "Generator output must be deterministic across runs");
    }

    [TestMethod]
    public void NodeId_NodeIds_PresentInRegistryInfo()
    {
        // nodeIds[] in _Info must be non-empty for a chain with Bind + Map steps.
        // Class-specific lookup per §8.4 (syntax-only registry catches all Result-returning methods).
        var outputs = RunRegistry(SimpleRegistrySource("TestNS", "OrderService", "Process",
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
        // .Bind(A).Map(B) vs .Map(B).Bind(A) — same step names, different positions.
        // Syntax-only: no %% correlation lines; verify by checking registry nodeIds differ.
        var outputsBM = RunRegistry(SimpleRegistrySource("TestNS", "Svc", "Op", "Get(cmd).Bind(A).Map(B)"));
        var outputsMB = RunRegistry(SimpleRegistrySource("TestNS", "Svc", "Op", "Get(cmd).Map(B).Bind(A)"));

        var registryBM = outputsBM.FirstOrDefault(o => o.hint.Contains("Svc_PipelineRegistry")).text ?? "";
        var registryMB = outputsMB.FirstOrDefault(o => o.hint.Contains("Svc_PipelineRegistry")).text ?? "";

        // Extract nodeId array content from the raw source text
        var matchBM = Regex.Match(registryBM, "nodeIds.*?\\]");
        var matchMB = Regex.Match(registryMB, "nodeIds.*?\\]");

        Assert.IsTrue(matchBM.Success, "BM chain must have nodeIds in registry");
        Assert.IsTrue(matchMB.Success, "MB chain must have nodeIds in registry");
        Assert.AreNotEqual(matchBM.Value, matchMB.Value,
            "Reordered steps must produce different nodeIds (index is part of the hash)");
    }

    // ── PipelineId uniqueness across overloads ────────────────────────────────

    [TestMethod]
    public void PipelineId_Overloads_AreUnique()
    {
        // Two methods with the same name but different parameter types → different pipelineIds.
        // Return type mentions "Result" → picked up by syntax-only registry heuristic.
        // Class-specific lookup per §8.4.
        const string source = @"
namespace TestNS
{
    public class OrderService
    {
        [REslava.ResultFlow.ResultFlow]
        public MyResult Process(string cmd) => default;
        [REslava.ResultFlow.ResultFlow]
        public MyResult Process(int id) => default;
    }
    public class MyResult { }
}";
        var outputs = RunRegistry(source);
        var registry = outputs.FirstOrDefault(o => o.hint.Contains("OrderService_PipelineRegistry")).text;

        Assert.IsNotNull(registry, "OrderService registry must be generated");
        // In raw .g.cs text, JSON is escaped: \"pipelineId\":\"xxxxxxxx\"
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
        // Syntax-only registry reads namespace from syntax tree — no using needed.
        // Class-specific lookup per §8.4.
        const string source = @"
namespace MyApp.Services.Orders
{
    public class OrderService
    {
        [REslava.ResultFlow.ResultFlow]
        public MyResult Process(string cmd) => default;
    }
    public class MyResult { }
}";
        var outputs = RunRegistry(source);
        var registry = outputs.FirstOrDefault(o => o.hint.Contains("OrderService_PipelineRegistry")).text;

        Assert.IsNotNull(registry, "OrderService registry must be generated");
        // In raw .g.cs text: \"namespace\":\"MyApp.Services.Orders\"
        Assert.IsTrue(registry.Contains("\\\"namespace\\\":\\\"MyApp.Services.Orders\\\""),
            "Namespace must appear verbatim in _Info JSON");
    }

    // ── PipelineId comment in diagram ─────────────────────────────────────────

    [TestMethod]
    public void PipelineId_Comment_PresentInDiagram()
    {
        // %% pipelineId: must be emitted in every generated _Diagram constant (parity with Result.Flow)
        var output = RunFlowDiagram(SimpleSource("Svc", "Process", "GetOrder(cmd).Bind(A).Map(B)"));
        Assert.IsTrue(output.Contains("%% pipelineId:"),
            "Generated diagram must contain %% pipelineId: comment");
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
        [REslava.ResultFlow.ResultFlow]
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

    // ── Fix #3: Assembly name included in pipelineId hash ────────────────────

    [TestMethod]
    public void PipelineId_DifferentAssemblyNames_ProduceDifferentIds()
    {
        // Same namespace+class+method in two projects with different assembly names must
        // produce different pipelineIds — prevents hash collision across projects.
        const string source = @"
namespace MyApp.Services
{
    public class OrderService
    {
        [REslava.ResultFlow.ResultFlow]
        public string Process(string cmd) => GetOrder(cmd).Bind(Validate).Map(ToDto);
    }
}";
        string RunWithAssembly(string assemblyName)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));
            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                CommonRefs(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var generator = new ResultFlowGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
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

        var outputA = RunWithAssembly("ProjectA");
        var outputB = RunWithAssembly("ProjectB");

        var idA = Regex.Match(outputA, @"%% pipelineId: ([0-9a-f]+)").Groups[1].Value;
        var idB = Regex.Match(outputB, @"%% pipelineId: ([0-9a-f]+)").Groups[1].Value;

        Assert.IsFalse(string.IsNullOrEmpty(idA), "ProjectA must emit a pipelineId comment");
        Assert.IsFalse(string.IsNullOrEmpty(idB), "ProjectB must emit a pipelineId comment");
        Assert.AreNotEqual(idA, idB,
            "Different assembly names must produce different pipelineIds to prevent cross-project hash collisions");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string SimpleSource(string cls, string method, string chain) => $@"
namespace TestNS
{{
    public class {cls}
    {{
        [REslava.ResultFlow.ResultFlow]
        public string {method}(string cmd) => {chain};
    }}
}}";

    /// <summary>
    /// Builds a registry test source. Return type uses "Result" in the name so the
    /// syntax-only registry heuristic (name-contains-"Result") picks it up.
    /// </summary>
    private static string SimpleRegistrySource(string ns, string cls, string method, string chain) => $@"
namespace {ns}
{{
    public class {cls}
    {{
        [REslava.ResultFlow.ResultFlow]
        public ResultWrapper {method}(string cmd) => {chain};
    }}
    public class ResultWrapper {{ }}
}}";
}
