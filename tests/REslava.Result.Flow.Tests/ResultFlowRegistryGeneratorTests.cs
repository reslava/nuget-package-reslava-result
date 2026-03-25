using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using REslava.Result.Flow.Generators.ResultFlow;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace REslava.Result.Flow.Tests;

/// <summary>
/// Tests for ResultFlowRegistryGenerator (C.1) and ENTRY_ROOT click directive (step 7).
/// Uses the same inline-source pattern as ResultFlowGeneratorTests:
/// inline IResultBase / Result&lt;T&gt; stubs in the REslava.Result namespace, no real assembly reference.
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

    private static List<MetadataReference> CommonRefs() => new List<MetadataReference>
    {
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(ImmutableList<>).Assembly.Location),
    };

    private static IReadOnlyList<(string hint, string text)> RunRegistry(
        string source,
        Dictionary<string, string>? buildProps = null,
        string? filePath = null)
    {
        var syntaxTree = filePath != null
            ? CSharpSyntaxTree.ParseText(SourceText.From(source), path: filePath)
            : CSharpSyntaxTree.ParseText(SourceText.From(source));

        var compilation = CSharpCompilation.Create(
            "TestRegistryCompilation",
            new[] { syntaxTree },
            CommonRefs(),
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

        var compilation = CSharpCompilation.Create(
            "TestFlowCompilation",
            new[] { syntaxTree },
            CommonRefs(),
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

    // ── Shared inline source stubs ────────────────────────────────────────────
    // Same inline-only pattern as ResultFlowGeneratorTests (no real assembly ref).

    private static string BaseSource => @"
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
        public static Result<T> Fail(string msg) => new Result<T>();
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> f) => new Result<TOut>();
        public Result<TOut> Map<TOut>(Func<T, TOut> f) => new Result<TOut>();
    }
    public class Result : IResultBase
    {
        public bool IsSuccess { get; }
        public bool IsFailure { get; }
        public ImmutableList<IReason> Reasons => ImmutableList<IReason>.Empty;
        public ImmutableList<IError> Errors => ImmutableList<IError>.Empty;
        public ImmutableList<ISuccess> Successes => ImmutableList<ISuccess>.Empty;
    }
}
namespace TestNS { public class Order {} public class ValidationError : REslava.Result.IError { public string Message { get; } = """"; } }
";

    // ── Scenario 1: [ResultFlow] method → full _Info ─────────────────────────

    [TestMethod]
    public void Registry_ResultFlow_Method_Emits_Full_Info()
    {
        var source = BaseSource + @"
namespace TestNS {
    using REslava.Result;
    public class OrderService {
        [REslava.Result.Flow.ResultFlow]
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
        Assert.IsTrue(registry.text.Contains("\\\"errorTypes\\\":"), "errorTypes should be present");
        Assert.IsTrue(registry.text.Contains("\\\"nodeKindFlags\\\":"), "nodeKindFlags should be present");
    }

    // ── Scenario 2: non-[ResultFlow] method → minimal _Info ─────────────────

    [TestMethod]
    public void Registry_NonResultFlow_Method_Emits_Minimal_Info()
    {
        var source = BaseSource + @"
namespace TestNS {
    using REslava.Result;
    public class OrderService {
        public Result CancelOrder() => new Result();
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
namespace TestNS {
    using REslava.Result;
    public class OrderService {
        [REslava.Result.Flow.ResultFlow]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order()).Map(o => o);
        public Result CancelOrder() => new Result();
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
namespace TestNS {
    using REslava.Result;
    public class OrderService {
        [REslava.Result.Flow.ResultFlow]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order()).Map(o => o);
    }
}";
        var props = new Dictionary<string, string>
        {
            ["build_property.ResultFlowRegistry"] = "false"
        };

        var files = RunRegistry(source, buildProps: props);

        var registry = files.FirstOrDefault(f => f.hint.Contains("OrderService_PipelineRegistry"));
        Assert.IsNull(registry.hint, "No PipelineRegistry file should be emitted when opt-out is set");
    }

    // ── Scenario 5: ENTRY_ROOT click directive when linkMode=vscode ──────────

    [TestMethod]
    public void EntryRoot_Click_Directive_Emitted_When_LinkMode_Vscode()
    {
        var source = BaseSource + @"
namespace TestNS {
    using REslava.Result;
    public class OrderService {
        [REslava.Result.Flow.ResultFlow]
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

    // ── DIAGNOSTIC: inspect exactly what the generator produces ──────────────
    // This test is intentionally loose — it collects evidence about file paths,
    // GetTypeByMetadataName, and ImplementsInterface so we can fix the real tests.

    [TestMethod]
    public void Diag_Registry_InspectAllGeneratedFiles()
    {
        var source = BaseSource + @"
namespace TestNS {
    using REslava.Result;
    public class OrderService {
        public Result CancelOrder() => new Result();
    }
}";
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));

        var compilation = CSharpCompilation.Create(
            "TestRegistryCompilation",
            new[] { syntaxTree },
            CommonRefs(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // ── 1. Check GetTypeByMetadataName directly ─────────────────────────
        var iResultBase = compilation.GetTypeByMetadataName("REslava.Result.IResultBase");
        var iResultBaseGeneric = compilation.GetTypeByMetadataName("REslava.Result.IResultBase`1");
        var iError = compilation.GetTypeByMetadataName("REslava.Result.IError");

        Assert.IsNotNull(iResultBase,        "GetTypeByMetadataName(IResultBase) must not be null");
        Assert.IsNotNull(iResultBaseGeneric, "GetTypeByMetadataName(IResultBase`1) must not be null");

        // ── 2. Check ImplementsInterface for Result (non-generic) ────────────
        var resultType = compilation.GetTypeByMetadataName("REslava.Result.Result");
        Assert.IsNotNull(resultType, "GetTypeByMetadataName(Result) must not be null");
        var resultImplements = REslava.Result.Flow.Generators.ResultFlow.CodeGeneration
            .ResultTypeExtractor.ImplementsInterface(resultType!, iResultBase!);
        Assert.IsTrue(resultImplements, "Result (non-generic) must implement IResultBase");

        // ── 3. Run the generator and list ALL generated files ────────────────
        var generator = new ResultFlowRegistryGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var result = driver.GetRunResult();

        var allPaths = string.Join("\n", result.GeneratedTrees.Select(t => t.FilePath));
        Assert.IsTrue(result.GeneratedTrees.Length > 0,
            $"Expected at least one generated file. Got 0.\n" +
            $"IResultBase null? {iResultBase == null}\n" +
            $"Result type null? {resultType == null}\n" +
            $"Result implements IResultBase? {resultImplements}");

        // ── 4. Walk the semantic model for every method — simulate the transform ──
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var diagLines = new System.Text.StringBuilder();
        var allMethods = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .ToList();
        diagLines.AppendLine($"Total MethodDeclarationSyntax nodes: {allMethods.Count}");

        foreach (var method in allMethods)
        {
            var symbol = semanticModel.GetDeclaredSymbol(method) as IMethodSymbol;
            if (symbol == null) { diagLines.AppendLine($"  {method.Identifier}: GetDeclaredSymbol=NULL"); continue; }

            var returnType = symbol.ReturnType;
            var unwrapped = returnType is INamedTypeSymbol { Name: "Task" or "ValueTask" } t &&
                            t.TypeArguments.Length == 1 ? (ITypeSymbol)t.TypeArguments[0] : returnType;

            var implements = REslava.Result.Flow.Generators.ResultFlow.CodeGeneration
                .ResultTypeExtractor.ImplementsInterface(unwrapped, iResultBase!);

            diagLines.AppendLine(
                $"  {symbol.ContainingType?.Name}.{symbol.Name}: " +
                $"ReturnType={returnType.ToDisplayString()}, " +
                $"Implements={implements}, " +
                $"AllInterfaces=[{string.Join(", ", unwrapped.AllInterfaces.Select(i => i.Name))}]");
        }

        // Dump content of every generated file
        var fileContents = new System.Text.StringBuilder();
        foreach (var tree in result.GeneratedTrees)
        {
            using var w = new System.IO.StringWriter();
            tree.GetText().Write(w);
            fileContents.AppendLine($"=== {tree.FilePath} ===");
            fileContents.AppendLine(w.ToString());
        }

        // Report all paths so we know the exact format
        Assert.IsTrue(allPaths.Contains("OrderService"),
            $"Expected an OrderService file.\n\nSemantic walk:\n{diagLines}\nGenerated paths:\n{allPaths}\n\nContent:\n{fileContents}");
    }
}
