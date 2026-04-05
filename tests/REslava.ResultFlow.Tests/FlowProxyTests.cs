using REslava.ResultFlow.Generators.ResultFlow;
using System.Collections.Immutable;
using System.Linq;

namespace REslava.ResultFlow.Tests;

/// <summary>
/// Tests for the <c>FlowProxy</c> partial class emitted by REslava.ResultFlow (syntax-only).
/// Parity tests against REslava.Result.Flow.Tests.FlowProxyTests.
/// </summary>
[TestClass]
public class FlowProxyTests
{
    // ── Instance class — FlowProxy pattern ───────────────────────────────────

    [TestMethod]
    public void FlowProxy_InstanceClass_EmitsFlowProperty()
    {
        var output = RunGenerator(CreatePartialInstanceSource("OrderService", "Process",
            "Result<Order>.Ok(new Order()).Bind(o => Result<Order>.Ok(o))"));

        Assert.IsTrue(output.Contains("public FlowProxy Flow =>"),
            "Instance class should emit 'public FlowProxy Flow' property");
    }

    [TestMethod]
    public void FlowProxy_InstanceClass_EmitsFlowProxyClass()
    {
        var output = RunGenerator(CreatePartialInstanceSource("OrderService", "Process",
            "Result<Order>.Ok(new Order()).Bind(o => Result<Order>.Ok(o))"));

        Assert.IsTrue(output.Contains("public sealed class FlowProxy"),
            "Generator should emit FlowProxy sealed class");
    }

    [TestMethod]
    public void FlowProxy_InstanceClass_CallsSelfDotMethod()
    {
        var output = RunGenerator(CreatePartialInstanceSource("OrderService", "Process",
            "Result<Order>.Ok(new Order()).Bind(o => Result<Order>.Ok(o))"));

        Assert.IsTrue(output.Contains("_self.Process("),
            "Instance FlowProxy should call _self.Process(...)");
    }

    [TestMethod]
    public void FlowProxy_InstanceClass_NoTracedExtensions()
    {
        var output = RunGenerator(CreatePartialInstanceSource("OrderService", "Process",
            "Result<Order>.Ok(new Order()).Bind(o => Result<Order>.Ok(o))"));

        Assert.IsFalse(output.Contains("_Traced_Extensions"),
            "_Traced_Extensions should NOT be emitted — FlowProxy replaces it");
    }

    [TestMethod]
    public void FlowProxy_AlwaysEmitsDebugProxy()
    {
        var output = RunGenerator(CreatePartialInstanceSource("OrderService", "Process",
            "Result<Order>.Ok(new Order()).Bind(o => Result<Order>.Ok(o))"));

        Assert.IsTrue(output.Contains("public sealed class DebugProxy"),
            "DebugProxy should always be emitted for partial classes");
        Assert.IsTrue(output.Contains("buf.Save(\"debug-"),
            "DebugProxy should save to a method-named debug file (debug-{method}.json)");
    }

    // ── Static class — static FlowProxy pattern ───────────────────────────────

    [TestMethod]
    public void FlowProxy_StaticClass_EmitsStaticFlowProperty()
    {
        var output = RunGenerator(CreatePartialStaticSource());

        Assert.IsTrue(output.Contains("public static FlowProxy Flow =>"),
            "Static class should emit 'public static FlowProxy Flow' property");
    }

    [TestMethod]
    public void FlowProxy_StaticClass_CallsClassNameDotMethod()
    {
        var output = RunGenerator(CreatePartialStaticSource());

        Assert.IsTrue(output.Contains("MathService.Triple("),
            "Static FlowProxy should call MathService.Triple(...)");
    }

    [TestMethod]
    public void FlowProxy_StaticClass_NoSelfField()
    {
        var output = RunGenerator(CreatePartialStaticSource());

        Assert.IsFalse(output.Contains("private readonly MathService _self"),
            "Static FlowProxy should NOT have a _self field");
    }

    // ── Value-type T — .ToString() not ?.ToString() ──────────────────────────

    [TestMethod]
    public void FlowProxy_ValueTypeT_UsesNonNullableToString()
    {
        var source = $@"
using System;

namespace TestNS
{{
    public class Result<T>
    {{
        public bool IsSuccess {{ get; }}
        public bool IsFailure {{ get; }}
        public T? Value {{ get; }}
        public System.Collections.Generic.List<object> Errors => new();
        public static Result<T> Ok(T value) => new Result<T>();
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> f) => new Result<TOut>();
    }}
    public partial class OrderService
    {{
        [ResultFlow]
        public Result<int> PlaceOrder() =>
            Result<int>.Ok(1).Bind(n => Result<int>.Ok(n));
    }}
}}";

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("result.Value.ToString()"),
            "Value-type T should use result.Value.ToString() in FlowProxy");
        Assert.IsFalse(output.Contains("result.Value?.ToString()"),
            "Value-type T should NOT use result.Value?.ToString() in FlowProxy");
    }

    // ── REF004 diagnostic ────────────────────────────────────────────────────

    [TestMethod]
    public void FlowProxy_REF004_FiresForNonPartialClass()
    {
        var source = $@"
using System;

namespace TestNS
{{
    public class Order {{ }}
    public class Result<T>
    {{
        public static Result<T> Ok(T value) => new Result<T>();
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> f) => new Result<TOut>();
    }}
    public class OrderService  // NOT partial
    {{
        [ResultFlow]
        public Result<Order> Process() =>
            Result<Order>.Ok(new Order()).Bind(o => Result<Order>.Ok(o));
    }}
}}";

        var diagnostics = RunGeneratorDiagnostics(source);

        Assert.IsTrue(diagnostics.Any(d => d.Id == "REF004"),
            "REF004 should be emitted for a non-partial class with [ResultFlow] methods");
    }

    [TestMethod]
    public void FlowProxy_REF004_NotEmittedForPartialClass()
    {
        var diagnostics = RunGeneratorDiagnostics(CreatePartialInstanceSource("OrderService", "Process",
            "Result<Order>.Ok(new Order()).Bind(o => Result<Order>.Ok(o))"));

        Assert.IsFalse(diagnostics.Any(d => d.Id == "REF004"),
            "REF004 should NOT be emitted for a partial class");
    }

    // ── Code fix targets REF004 ───────────────────────────────────────────────

    [TestMethod]
    public void CodeFix_Targets_REF004()
    {
        var fix = new REslava.ResultFlow.Analyzers.ResultFlowPartialCodeFix();
        Assert.IsTrue(fix.FixableDiagnosticIds.Contains("REF004"),
            "ResultFlowPartialCodeFix should target REF004");
    }

    // ── Emits in user namespace (REslava.ResultFlow specific) ────────────────

    [TestMethod]
    public void FlowProxy_EmittedInUserNamespace()
    {
        var output = RunGenerator(CreatePartialInstanceSource("OrderService", "Process",
            "Result<Order>.Ok(new Order()).Bind(o => Result<Order>.Ok(o))"));

        Assert.IsTrue(output.Contains("namespace TestNS"),
            "FlowProxy partial class should be emitted in the user's namespace (TestNS)");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string CreatePartialInstanceSource(string className, string methodName, string returnExpression) => $@"
using System;

namespace TestNS
{{
    public class Order {{ }}

    public class Result<T>
    {{
        public bool IsSuccess {{ get; }}
        public bool IsFailure {{ get; }}
        public T? Value {{ get; }}
        public System.Collections.Generic.List<object> Errors => new();
        public static Result<T> Ok(T value) => new Result<T>();
        public static Result<T> Fail(string msg) => new Result<T>();
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> f) => new Result<TOut>();
        public Result<TOut> Map<TOut>(Func<T, TOut> f) => new Result<TOut>();
        public Result<T> Tap(Action<T> a) => this;
    }}

    public partial class {className}
    {{
        [ResultFlow]
        public Result<Order> {methodName}() => {returnExpression};
    }}
}}";

    private static string CreatePartialStaticSource() => $@"
using System;

namespace TestNS
{{
    public class Result<T>
    {{
        public bool IsSuccess {{ get; }}
        public bool IsFailure {{ get; }}
        public T? Value {{ get; }}
        public System.Collections.Generic.List<object> Errors => new();
        public static Result<T> Ok(T value) => new Result<T>();
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> f) => new Result<TOut>();
        public Result<TOut> Map<TOut>(Func<T, TOut> f) => new Result<TOut>();
    }}

    public static partial class MathService
    {{
        [ResultFlow]
        public static Result<int> Triple(int i) =>
            Result<int>.Ok(i).Map(x => x * 3);
    }}
}}";

    private static string RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));

        var references = new System.Collections.Generic.List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "FlowProxySyntaxTestCompilation",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ResultFlowGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        var updatedDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var sb = new System.Text.StringBuilder();
        foreach (var tree in updatedDriver.GetRunResult().GeneratedTrees)
        {
            using var writer = new System.IO.StringWriter();
            tree.GetText().Write(writer);
            sb.AppendLine(writer.ToString());
        }
        return sb.ToString();
    }

    private static System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.Diagnostic> RunGeneratorDiagnostics(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));

        var references = new System.Collections.Generic.List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "FlowProxyDiagSyntaxTestCompilation",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ResultFlowGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);
        return diagnostics;
    }
}
