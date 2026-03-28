using REslava.Result.Flow.Generators.ResultFlow;
using System.Collections.Immutable;
using System.Linq;

namespace REslava.Result.Flow.Tests;

[TestClass]
public class ResultFlowCrossMethodTests
{
    // ── 1. Basic cross-method tracing ─────────────────────────────────────────
    // Bind lambda calls a same-project method → subgraph block emitted
    [TestMethod]
    public void CrossMethod_Bind_IntoSubMethod_EmitsSubgraph()
    {
        var source = CreateSource("OrderService", "Process",
            "CreateOrder().Bind(x => Validate(x)).Map(ToDto)",
            extraMethods: @"
        static Result<Order> CreateOrder() => Result<Order>.Ok(new Order());
        static Result<Order> Validate(Order o) =>
            Result<Order>.Ok(o).Ensure(x => x.Id > 0, u => new ValidationError(""invalid""));
        static OrderDto ToDto(Order o) => new OrderDto();");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("subgraph"), "Should emit a subgraph block");
        Assert.IsTrue(output.Contains("sg_"),      "Subgraph ID should use sg_ prefix");
        Assert.IsTrue(output.Contains("Validate"), "Subgraph should be titled Validate");
    }

    // ── 2. Subgraph connects to next step via ok edge ─────────────────────────
    [TestMethod]
    public void CrossMethod_Subgraph_ConnectsToNextStep()
    {
        var source = CreateSource("OrderService", "Process",
            "CreateOrder().Bind(x => Validate(x)).Map(ToDto)",
            extraMethods: @"
        static Result<Order> CreateOrder() => Result<Order>.Ok(new Order());
        static Result<Order> Validate(Order o) =>
            Result<Order>.Ok(o).Ensure(x => x.Id > 0, u => new ValidationError(""invalid""));
        static OrderDto ToDto(Order o) => new OrderDto();");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("-->|ok|"), "Subgraph should connect to next step with ok edge");
        Assert.IsTrue(output.Contains("Map"),      "Map node should still be present after subgraph");
    }

    // ── 3. Inner Gatekeeper (Ensure) inside subgraph has fail edge ────────────
    [TestMethod]
    public void CrossMethod_InnerGatekeeper_HasFailEdge()
    {
        var source = CreateSource("OrderService", "Process",
            "CreateOrder().Bind(x => Validate(x))",
            extraMethods: @"
        static Result<Order> CreateOrder() => Result<Order>.Ok(new Order());
        static Result<Order> Validate(Order o) =>
            Result<Order>.Ok(o).Ensure(x => x.Id > 0, u => new ValidationError(""invalid""));");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("subgraph"),  "Subgraph emitted");
        Assert.IsTrue(output.Contains("gatekeeper"), "Inner Ensure renders as gatekeeper");
        Assert.IsTrue(output.Contains("FAIL"),       "Inner Ensure has fail edge to shared FAIL terminal");
    }

    // ── 4. MaxDepth = 0 disables cross-method tracing ────────────────────────
    [TestMethod]
    public void CrossMethod_MaxDepthZero_NoSubgraph()
    {
        var source = CreateSourceWithMaxDepth("OrderService", "Process", maxDepth: 0,
            "CreateOrder().Bind(x => Validate(x)).Map(ToDto)",
            extraMethods: @"
        static Result<Order> CreateOrder() => Result<Order>.Ok(new Order());
        static Result<Order> Validate(Order o) => Result<Order>.Ok(o);
        static OrderDto ToDto(Order o) => new OrderDto();");

        var output = RunGenerator(source);

        Assert.IsFalse(output.Contains("subgraph sg_"), "MaxDepth=0 should disable subgraph expansion");
        Assert.IsTrue(output.Contains("Validate"),   "Validate still shown as leaf node label (Gap 1)");
    }

    // ── 5. Cycle guard: mutual recursion terminates without hanging ───────────
    [TestMethod]
    public void CrossMethod_MutualRecursion_TerminatesCleanly()
    {
        // PipelineA calls PipelineB via Bind, PipelineB calls PipelineA via Bind.
        // Cycle guard (HashSet<ISymbol>) must stop re-entry; MaxDepth is a second safety net.
        var source = CreateSource("CycleService", "PipelineA",
            "Seed().Bind(x => PipelineB(x))",
            extraMethods: @"
        static Result<Order> Seed() => Result<Order>.Ok(new Order());
        static Result<Order> PipelineB(Order o) => Seed().Bind(x => PipelineA(x));");

        // Must complete (no StackOverflowException) and produce some output
        var output = RunGenerator(source);

        Assert.IsTrue(output.Length > 0, "Generator should complete and produce output");
    }

    // ── 6. Non-lambda arg (method group) → no subgraph expansion ─────────────
    [TestMethod]
    public void CrossMethod_MethodGroup_NoSubgraph()
    {
        // .Bind(Validate) — not a lambda → TryGetLambdaBodyInvocation returns null → no tracing
        var source = CreateSource("OrderService", "Process",
            "CreateOrder().Bind(Validate)",
            extraMethods: @"
        static Result<Order> CreateOrder() => Result<Order>.Ok(new Order());
        static Result<Order> Validate(Order o) => Result<Order>.Ok(o);");

        var output = RunGenerator(source);

        Assert.IsFalse(output.Contains("subgraph sg_"), "Method group Bind should not expand into subgraph");
        // Method group: Gap 1 does not apply — node label stays "Bind" (outer pipeline method name)
        Assert.IsTrue(output.Contains("Bind"),       "Bind node still rendered");
    }

    // ── 7. MaxDepth = 1: first level expands, nested level does not ──────────
    [TestMethod]
    public void CrossMethod_MaxDepthOne_OnlyFirstLevelExpands()
    {
        var source = CreateSourceWithMaxDepth("OrderService", "Process", maxDepth: 1,
            "CreateOrder().Bind(x => Validate(x))",
            extraMethods: @"
        static Result<Order> CreateOrder() => Result<Order>.Ok(new Order());
        static Result<Order> Validate(Order o) =>
            Result<Order>.Ok(o).Bind(x => DeepCheck(x));
        static Result<Order> DeepCheck(Order o) =>
            Result<Order>.Ok(o).Ensure(x => x.Id > 0, u => new ValidationError(""x""));");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("subgraph"), "First level (Validate) should expand into subgraph");
        // Exactly 1 subgraph in the diagram — DeepCheck expanding would give 2+.
        // Deduplicate lines: _Diagram and _TypeFlow emit identical content; collapsing prevents false doubling.
        var deduped = string.Join("\n", output.Split('\n').Distinct());
        var sgCount = CountOccurrences(deduped, "subgraph sg_");
        Assert.AreEqual(1, sgCount, $"Expected exactly 1 'subgraph sg_' occurrence, got {sgCount}");
    }

    // ── 8. Cross-class qualified call (x => SomeClass.Method(x)) expands ────
    [TestMethod]
    public void CrossMethod_QualifiedCall_CrossClass_EmitsSubgraph()
    {
        // Phase 1b: TryGetLambdaBodyInvocation must handle MemberAccessExpressionSyntax
        // so that x => UserService.ValidateUser(x) traces into UserService.ValidateUser.
        var source = $@"
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
    public class OrderDto {{ }}

    public class ValidationError : IError
    {{
        public string Message {{ get; }}
        public ValidationError(string msg) {{ Message = msg; }}
    }}

    // Separate class — qualified call x => UserService.ValidateUser(x)
    public static class UserService
    {{
        public static Result<Order> ValidateUser(Order o) =>
            Result<Order>.Ok(o).Ensure(x => x.Id > 0, u => new ValidationError(""invalid""));
    }}

    public class OrderService
    {{
        [REslava.Result.Flow.ResultFlow(MaxDepth = 2)]
        public Result<Order> PlaceOrder() =>
            Result<Order>.Ok(new Order())
                .Bind(u => UserService.ValidateUser(u));
    }}
}}";

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("subgraph"),    "Cross-class qualified call should expand into subgraph");
        Assert.IsTrue(output.Contains("ValidateUser"), "Subgraph should be titled ValidateUser");
        Assert.IsTrue(output.Contains("sg_"),          "Subgraph ID should use sg_ prefix");
    }

    // ── 9. Subgraph emits entry arrow ==> pointing to first inner node ────────
    [TestMethod]
    public void CrossMethod_Subgraph_EmitsEntryArrow()
    {
        var source = CreateSource("OrderService", "Process",
            "CreateOrder().Bind(x => Validate(x)).Map(ToDto)",
            extraMethods: @"
        static Result<Order> CreateOrder() => Result<Order>.Ok(new Order());
        static Result<Order> Validate(Order o) =>
            Result<Order>.Ok(o).Ensure(x => x.Id > 0, u => new ValidationError(""invalid""));
        static OrderDto ToDto(Order o) => new OrderDto();");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("ENTRY_"),        "Entry node should be emitted inside subgraph");
        Assert.IsTrue(output.Contains("==>"),           "Thick arrow ==> should be emitted for entry");
        Assert.IsTrue(output.Contains("classDef entry"), "classDef entry fill:none,stroke:none should be declared");
    }

    // ── 10. Flat pipeline (no subgraph) does NOT emit entry arrow ─────────────
    [TestMethod]
    public void FlatPipeline_NoSubgraph_NoEntryArrow()
    {
        var source = CreateSourceWithMaxDepth("OrderService", "Process", maxDepth: 0,
            "CreateOrder().Map(ToDto)",
            extraMethods: @"
        static Result<Order> CreateOrder() => Result<Order>.Ok(new Order());
        static OrderDto ToDto(Order o) => new OrderDto();");

        var output = RunGenerator(source);

        // ENTRY_ROOT is expected (seed node) — only subgraph-style ENTRY_{nodeId} should be absent
        Assert.IsFalse(output.Contains("ENTRY_N"), "Flat pipeline should not emit subgraph entry arrows");
        Assert.IsTrue(output.Contains("ENTRY_ROOT"), "Flat pipeline should still emit seed entry node");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string CreateSource(
        string className,
        string methodName,
        string returnExpression,
        string extraMethods = "")
        => CreateSourceWithMaxDepth(className, methodName, maxDepth: 2, returnExpression, extraMethods);

    private static string CreateSourceWithMaxDepth(
        string className,
        string methodName,
        int maxDepth,
        string returnExpression,
        string extraMethods = "")
    {
        return $@"
using System;
using System.Threading.Tasks;
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
    public class OrderDto {{ }}

    public class ValidationError : IError
    {{
        public string Message {{ get; }}
        public ValidationError(string msg) {{ Message = msg; }}
    }}

    public class {className}
    {{
        [REslava.Result.Flow.ResultFlow(MaxDepth = {maxDepth})]
        public Result<OrderDto> {methodName}() => {returnExpression};
        {extraMethods}
    }}
}}";
    }

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
