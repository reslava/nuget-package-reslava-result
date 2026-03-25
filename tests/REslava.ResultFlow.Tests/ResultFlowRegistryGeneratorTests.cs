using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using REslava.ResultFlow.Generators.ResultFlow;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace REslava.ResultFlow.Tests;

/// <summary>
/// Tests for ResultFlowRegistryGenerator (C.1 syntax-only port) and ENTRY_ROOT click directive.
/// Syntax-only: errorTypes and nodeKindFlags are always []; nodeCount from chain extractor.
/// </summary>
[TestClass]
public class ResultFlowRegistryGeneratorTests
{
    // ── Infrastructure ───────────────────────────────────────────────────────

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

    private static IReadOnlyList<(string hint, string text)> RunRegistry(
        string source,
        Dictionary<string, string>? buildProps = null,
        string? filePath = null)
    {
        var syntaxTree = filePath != null
            ? CSharpSyntaxTree.ParseText(SourceText.From(source), path: filePath)
            : CSharpSyntaxTree.ParseText(SourceText.From(source));

        var refs = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "TestRegistryCompilation",
            new[] { syntaxTree },
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ResultFlowRegistryGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        if (buildProps != null)
            driver = driver.WithUpdatedAnalyzerConfigOptions(
                new TestAnalyzerConfigOptionsProvider(buildProps));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var result = driver.GetRunResult();

        return result.GeneratedTrees
            .Select(t =>
            {
                using var w = new System.IO.StringWriter();
                t.GetText().Write(w);
                return (t.FilePath, w.ToString());
            })
            .ToList();
    }

    private static string RunFlowGenerator(string source, Dictionary<string, string> buildProps,
        string filePath)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source), path: filePath);

        var refs = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "TestFlowCompilation",
            new[] { syntaxTree },
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ResultFlowGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.WithUpdatedAnalyzerConfigOptions(
            new TestAnalyzerConfigOptionsProvider(buildProps));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var result = driver.GetRunResult();

        var sb = new System.Text.StringBuilder();
        foreach (var tree in result.GeneratedTrees)
        {
            using var w = new System.IO.StringWriter();
            tree.GetText().Write(w);
            sb.AppendLine(w.ToString());
        }
        return sb.ToString();
    }

    // ── Shared source with a simple Result<T> stub ───────────────────────────

    private static string BaseSource => @"
using System;
namespace TestNS {
    public class Result<T> {
        public static Result<T> Ok(T value) => new Result<T>();
        public static Result<T> Fail(string msg) => new Result<T>();
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> f) => new Result<TOut>();
        public Result<TOut> Map<TOut>(Func<T, TOut> mapper) => new Result<TOut>();
    }
    public class Result {
        public static Result Ok() => new Result();
    }
    public class Order {}
}
";

    // ── Scenario 1: [ResultFlow] method → full _Info (syntax-only) ───────────

    [TestMethod]
    public void Registry_ResultFlow_Method_Emits_Full_Info()
    {
        var source = BaseSource + @"
using TestNS;
namespace TestNS {
    public class OrderService {
        [ResultFlow]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order()).Bind(o => Result<Order>.Ok(o)).Map(o => o);
    }
}";
        var files = RunRegistry(source);

        var registry = files.FirstOrDefault(f => f.hint.Contains("OrderService_PipelineRegistry"));
        Assert.IsNotNull(registry.text, "PipelineRegistry file should be generated");

        Assert.IsTrue(registry.text.Contains("_Methods"), "_Methods index should be present");
        Assert.IsTrue(registry.text.Contains("PlaceOrder"), "_Methods should list PlaceOrder");
        Assert.IsTrue(registry.text.Contains("PlaceOrder_Info"), "PlaceOrder_Info constant should exist");
        Assert.IsTrue(registry.text.Contains("\\\"hasDiagram\\\":true"), "hasDiagram should be true");
        Assert.IsTrue(registry.text.Contains("\\\"nodeCount\\\":"), "nodeCount should be present");
        // Syntax-only: errorTypes and nodeKindFlags are always empty arrays
        Assert.IsTrue(registry.text.Contains("\\\"errorTypes\\\":[]"), "errorTypes should be empty array");
        Assert.IsTrue(registry.text.Contains("\\\"nodeKindFlags\\\":[]"), "nodeKindFlags should be empty array");
    }

    // ── Scenario 2: non-[ResultFlow] method → minimal _Info ──────────────────

    [TestMethod]
    public void Registry_NonResultFlow_Method_Emits_Minimal_Info()
    {
        var source = BaseSource + @"
using TestNS;
namespace TestNS {
    public class OrderService {
        public Result CancelOrder() => Result.Ok();
    }
}";
        var files = RunRegistry(source);

        var registry = files.FirstOrDefault(f => f.hint.Contains("OrderService_PipelineRegistry"));
        Assert.IsNotNull(registry.text, "PipelineRegistry file should be generated");

        Assert.IsTrue(registry.text.Contains("CancelOrder_Info"), "CancelOrder_Info should exist");
        Assert.IsTrue(registry.text.Contains("\\\"hasDiagram\\\":false"), "hasDiagram should be false");
        Assert.IsFalse(registry.text.Contains("\\\"nodeCount\\\":"), "nodeCount should not appear for non-[ResultFlow]");
        Assert.IsFalse(registry.text.Contains("\\\"nodeKindFlags\\\":"), "nodeKindFlags should not appear for non-[ResultFlow]");
    }

    // ── Scenario 3: mixed class ───────────────────────────────────────────────

    [TestMethod]
    public void Registry_Mixed_Class_Emits_Both_Info_Tiers()
    {
        var source = BaseSource + @"
using TestNS;
namespace TestNS {
    public class OrderService {
        [ResultFlow]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order()).Map(o => o);
        public Result CancelOrder() => Result.Ok();
    }
}";
        var files = RunRegistry(source);

        var registry = files.FirstOrDefault(f => f.hint.Contains("OrderService_PipelineRegistry"));
        Assert.IsNotNull(registry.text, "PipelineRegistry file should be generated");

        Assert.IsTrue(registry.text.Contains("PlaceOrder"), "_Methods should list PlaceOrder");
        Assert.IsTrue(registry.text.Contains("CancelOrder"), "_Methods should list CancelOrder");
        Assert.IsTrue(registry.text.Contains("\\\"hasDiagram\\\":true"), "PlaceOrder_Info hasDiagram should be true");
        Assert.IsTrue(registry.text.Contains("\\\"hasDiagram\\\":false"), "CancelOrder_Info hasDiagram should be false");
    }

    // ── Scenario 4: opt-out → no registry file emitted ───────────────────────

    [TestMethod]
    public void Registry_OptOut_Property_Suppresses_Generation()
    {
        var source = BaseSource + @"
using TestNS;
namespace TestNS {
    public class OrderService {
        [ResultFlow]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order()).Map(o => o);
    }
}";
        var props = new Dictionary<string, string>
        {
            ["build_property.ResultFlowRegistry"] = "false"
        };

        var files = RunRegistry(source, buildProps: props);

        var registry = files.FirstOrDefault(f => f.hint.Contains("_PipelineRegistry"));
        Assert.IsNull(registry.hint, "No PipelineRegistry file should be emitted when opt-out is set");
    }

    // ── Scenario 5: ENTRY_ROOT click directive when linkMode=vscode ──────────

    [TestMethod]
    public void EntryRoot_Click_Directive_Emitted_When_LinkMode_Vscode()
    {
        var source = BaseSource + @"
using TestNS;
namespace TestNS {
    public class OrderService {
        [ResultFlow]
        public Result<Order> PlaceOrder() =>
            GetOrder().Bind(o => Result<Order>.Ok(o)).Map(o => o);
        private Result<Order> GetOrder() => Result<Order>.Ok(new Order());
    }
}";
        var props = new Dictionary<string, string>
        {
            ["build_property.ResultFlowLinkMode"] = "vscode"
        };

        var output = RunFlowGenerator(source, props, filePath: "OrderService.cs");

        Assert.IsTrue(output.Contains("click ENTRY_ROOT"), "ENTRY_ROOT click directive should be emitted");
        Assert.IsTrue(output.Contains("vscode://file/"), "ENTRY_ROOT click should use vscode:// protocol");
        Assert.IsTrue(output.Contains("OrderService.cs"), "Click URL should contain source file path");
    }
}
