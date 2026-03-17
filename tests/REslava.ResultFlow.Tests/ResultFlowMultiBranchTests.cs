using REslava.ResultFlow.Generators.ResultFlow;
using System.Linq;

namespace REslava.ResultFlow.Tests;

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

    // ── 3. Match emits failure node ───────────────────────────────────────────
    [TestMethod]
    public void Match_EmitsFailureNode()
    {
        var output = RunGenerator(CreateMatchSource());

        Assert.IsTrue(output.Contains("[\"\"Failure\"\"]:::failure"), "Match must emit a Failure node");
        Assert.IsTrue(output.Contains("-->|fail|"), "Match must emit a -->|fail| edge");
    }

    // ── 4. Non-Match pipeline: SUCCESS terminal still emitted ─────────────────
    [TestMethod]
    public void NonMatch_SuccessTerminalStillEmitted()
    {
        var output = RunGenerator(CreateNoMatchSource());

        Assert.IsTrue(output.Contains("-->|ok| SUCCESS"), "Non-Match pipeline must still emit -->|ok| SUCCESS");
        Assert.IsTrue(output.Contains("SUCCESS([success]):::success"), "SUCCESS terminal node must be emitted");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string CreateMatchSource() => @"
using System;

namespace TestNS
{
    public class Order { public int Id { get; } }

    public class Result<T>
    {
        public static Result<T> Ok(T value) => new Result<T>();
        public Result<TOut> Map<TOut>(Func<T, TOut> f) => new Result<TOut>();
        public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<string, TOut> onFailure) => default!;
    }

    public class OrderService
    {
        [ResultFlow(MaxDepth = 2)]
        public string PlaceOrder() =>
            Result<Order>.Ok(new Order())
                .Map(x => x)
                .Match(o => o.Id.ToString(), errors => ""fail"");
    }
}";

    private static string CreateNoMatchSource() => @"
using System;

namespace TestNS
{
    public class Order { public int Id { get; } }

    public class Result<T>
    {
        public static Result<T> Ok(T value) => new Result<T>();
        public Result<TOut> Map<TOut>(Func<T, TOut> f) => new Result<TOut>();
    }

    public class OrderService
    {
        [ResultFlow(MaxDepth = 2)]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order()).Map(x => x);
    }
}";

    private static string RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            new[] { syntaxTree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
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
