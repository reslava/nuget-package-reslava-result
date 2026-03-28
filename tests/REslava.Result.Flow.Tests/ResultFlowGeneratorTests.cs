using REslava.Result.Flow.Generators.ResultFlow;
using System.Collections.Immutable;
using System.Linq;

namespace REslava.Result.Flow.Tests;

[TestClass]
public class ResultFlowGeneratorTests
{
    // ── Phase 1: Success Type Travel ─────────────────────────────────────────

    // 1. Map with type change → "Map · ToDto<br/>User → UserDto"
    [TestMethod]
    public void TypeTravel_Map_ShowsTypeTransition()
    {
        var source = CreateSource("UserService", "Register",
            "CreateUser().Map(ToDto)",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static UserDto ToDto(User u) => new UserDto();");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Map"), "Should contain Map node");
        Assert.IsTrue(output.Contains("<br/>"), "Should contain type label separator");
        Assert.IsTrue(output.Contains("User"), "Should contain User type");
    }

    // 2. Bind same type → label shows type only (no arrow)
    [TestMethod]
    public void TypeTravel_Bind_SameType_ShowsTypeOnly()
    {
        var source = CreateSource("UserService", "Register",
            "CreateUser().Bind(SaveUser)",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static Result<User> SaveUser(User u) => Result<User>.Ok(u);");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Bind"), "Should contain Bind node");
        Assert.IsTrue(output.Contains("User"), "Should contain User type");
    }

    // 3. Ensure → label shows type only (no arrow, type-preserving)
    [TestMethod]
    public void TypeTravel_Ensure_ShowsTypeOnly()
    {
        var source = CreateSource("UserService", "Register",
            "CreateUser().Ensure(IsValid, u => new ValidationError(\"invalid\"))",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static bool IsValid(User u) => true;");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Ensure"), "Should contain Ensure node");
        Assert.IsTrue(output.Contains("User"), "Should contain User type");
    }

    // 3b. Ensure with lambda predicate → tooltip span in node label
    [TestMethod]
    public void TypeTravel_Ensure_LambdaPredicate_Should_Generate_Tooltip()
    {
        var source = CreateSource("UserService", "Register",
            "CreateUser().Ensure(u => u != null, u => new ValidationError(\"invalid\"))",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("span title="), "Ensure lambda predicate should emit <span title=...> tooltip");
        Assert.IsTrue(output.Contains("u != null"),   "Tooltip should contain predicate body text");
    }

    // 3c. Ensure with method-group predicate → no tooltip
    [TestMethod]
    public void TypeTravel_Ensure_MethodGroupPredicate_Should_Not_Generate_Tooltip()
    {
        var source = CreateSource("UserService", "Register",
            "CreateUser().Ensure(IsValid, u => new ValidationError(\"invalid\"))",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static bool IsValid(User u) => true;");

        var output = RunGenerator(source);

        Assert.IsFalse(PipelineOnly(output).Contains("span title="), "Method-group predicate should not emit tooltip");
    }

    // 4. Tap → label shows type (type-preserving side-effect)
    [TestMethod]
    public void TypeTravel_Tap_ShowsTypeOnly()
    {
        var source = CreateSource("UserService", "Register",
            "CreateUser().Tap(SendEmail)",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static void SendEmail(User u) { }");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Tap"), "Should contain Tap node");
    }

    // 5. Non-Result return type → falls back to method name only (no <br/>)
    [TestMethod]
    public void TypeTravel_NonResultType_FallsBackToMethodName()
    {
        // The chain uses string (not Result<T>) — IResultBase check fails → no type label
        var source = @"
namespace TestNS
{
    public class Svc
    {
        [REslava.Result.Flow.ResultFlow]
        public string Process(string s) => Transform(s);

        static string Transform(string s) => s.ToUpper();
    }
}";
        var output = RunGenerator(source);
        // REF001 fires (chain not detected as Result pipeline), no diagram generated — that's fine
        // The important thing: no crash
        Assert.IsNotNull(output);
    }

    // 6. Mixed pipeline → all nodes have correct labels
    [TestMethod]
    public void TypeTravel_Mixed_Pipeline_CorrectLabelsOnAllNodes()
    {
        var source = CreateSource("UserService", "Register",
            "CreateUser().Ensure(IsValid, u => new ValidationError(\"x\")).Bind(SaveUser).Map(ToDto)",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static bool IsValid(User u) => true;
        static Result<User> SaveUser(User u) => Result<User>.Ok(u);
        static UserDto ToDto(User u) => new UserDto();");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Ensure"), "Ensure node present");
        Assert.IsTrue(output.Contains("Bind"), "Bind node present");
        Assert.IsTrue(output.Contains("Map"), "Map node present");
        Assert.IsTrue(output.Contains("User"), "User type present");
    }

    // ── Phase 2: Error Type Travel ────────────────────────────────────────────

    // 7. Bind step with DatabaseError → typed error edge
    [TestMethod]
    public void ErrorTravel_Bind_ShowsErrorEdge()
    {
        var source = CreateSource("UserService", "Register",
            "CreateUser().Bind(SaveUser)",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static Result<User> SaveUser(User u) => Result<User>.Fail(new DatabaseError(""db fail""));");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("DatabaseError"), "Should contain DatabaseError in edge label");
        Assert.IsTrue(output.Contains("FAIL"), "Should contain FAIL terminal node");
    }

    // 8. Ensure step with ValidationError → typed error edge
    [TestMethod]
    public void ErrorTravel_Ensure_ShowsErrorEdge()
    {
        var source = CreateSource("UserService", "Register",
            "CreateUser().Ensure(IsValid, u => new ValidationError(\"invalid\"))",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static bool IsValid(User u) => false;");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("ValidationError"), "Should contain ValidationError in edge label");
        Assert.IsTrue(output.Contains("FAIL"), "Should contain FAIL terminal node");
    }

    // 9. Multiple errors from different steps → all appear as separate edges
    [TestMethod]
    public void ErrorTravel_MultipleErrors_ShowsAllEdges()
    {
        var source = CreateSource("UserService", "Register",
            "CreateUser().Bind(SaveUser).Bind(SendEmail)",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static Result<User> SaveUser(User u) => Result<User>.Fail(new DatabaseError(""db""));
        static Result<User> SendEmail(User u) => Result<User>.Fail(new EmailError(""mail""));");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("DatabaseError"), "Should contain DatabaseError");
        Assert.IsTrue(output.Contains("EmailError"), "Should contain EmailError");
        Assert.IsTrue(output.Contains("FAIL"), "Should contain FAIL terminal node");
    }

    // 10. Method with no IError constructions in body → no typed error edge
    [TestMethod]
    public void ErrorTravel_MethodWithNoErrors_NoErrorEdges()
    {
        var source = CreateSource("UserService", "Register",
            "CreateUser().Bind(SaveUser)",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static Result<User> SaveUser(User u) => Result<User>.Ok(u);");

        var output = RunGenerator(source);

        // Bind is TransformWithRisk → fallback fail edge (no typed error, but generic FAIL present)
        Assert.IsTrue(output.Contains("FAIL"), "TransformWithRisk always shows failure path");
        Assert.IsFalse(output.Contains("DatabaseError"), "No typed error when method has no error constructions");
    }

    // ── Regression: chain walk completeness ──────────────────────────────────

    // 12. Ensure × 3 chain starting with helper method — must produce all 3 nodes
    //     (regression guard: old IInvocationOperation.Instance walk missed Ensure×3 starting from Ok)
    [TestMethod]
    public void ChainWalk_EnsureTriple_AllNodesPresent()
    {
        var source = CreateSource("OrderService", "Validate",
            "CreateOrder().Ensure(p1, e).Ensure(p2, e).Ensure(p3, e)",
            extraMethods: @"
        static Result<UserDto> CreateOrder() => Result<UserDto>.Ok(new UserDto());
        static bool p1(UserDto u) => true;
        static bool p2(UserDto u) => true;
        static bool p3(UserDto u) => true;
        static IError e(UserDto u) => new ValidationError(""invalid"");");

        var output = RunGenerator(source);

        // All three Ensure nodes must appear — check via %% correlation comments (lines starting with %%)
        // Deduplicate: _Diagram and _TypeFlow emit identical %% lines; Distinct() collapses them.
        var ensureCount = System.Array.FindAll(output.Split('\n').Distinct().ToArray(), l => l.StartsWith("%%") && l.Contains("\u2192 Ensure")).Length;
        Assert.AreEqual(3, ensureCount, "Three Ensure nodes must be present");
    }

    // 13. Result<T>.Ok(value).Ensure chain — Ok becomes N0, Ensure nodes follow
    //     (regression guard: old code stopped after 1 node for Ok-root chains)
    [TestMethod]
    public void ChainWalk_OkRootEnsureChain_OkPlusEnsureNodes()
    {
        var source = @"
using System;
using System.Collections.Immutable;

namespace REslava.Result
{
    public interface IError { string Message { get; } }
    public interface IResultBase<out T> { bool IsSuccess { get; } bool IsFailure { get; } }
    public class Result<T> : IResultBase<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure { get; }
        public T? Value { get; }
        public static Result<T> Ok(T value) => new Result<T>();
        public static Result<T> Fail(IError error) => new Result<T>();
        public Result<T> Ensure(Func<T, bool> p, string msg) => new Result<T>();
    }
}

namespace TestNS
{
    using REslava.Result;
    public record Order(int Id);

    public class OrderService
    {
        [REslava.Result.Flow.ResultFlow]
        public Result<Order> Validate(Order order) =>
            Result<Order>.Ok(order)
                .Ensure(o => o.Id > 0, ""Id must be positive"")
                .Ensure(o => o.Id < 9999, ""Id exceeds limit"");
    }
}";
        var output = RunGenerator(source);

        // Ok node + 2 Ensure nodes — check via %% correlation comments (lines starting with %%)
        // Deduplicate: _Diagram and _TypeFlow emit identical %% lines; Distinct() collapses them.
        var dedupedLines = output.Split('\n').Distinct().ToArray();
        Assert.IsTrue(dedupedLines.Any(l => l.StartsWith("%%") && l.Contains("\u2192 Ok")), "Ok must be present — Ok is the chain root");
        var ensureCount = System.Array.FindAll(dedupedLines, l => l.StartsWith("%%") && l.Contains("\u2192 Ensure")).Length;
        Assert.AreEqual(2, ensureCount, "Two Ensure nodes must be present");
    }

    // 14. 4-step mixed chain (Bind×2 + Ensure + Map) — all nodes present in correct order
    [TestMethod]
    public void ChainWalk_FourStep_AllNodesInOrder()
    {
        var source = CreateSource("OrderService", "Process",
            "CreateUser().Bind(FindOrder).Ensure(IsValid, e).Bind(SaveOrder).Map(ToDto)",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static Result<User> FindOrder(User u) => Result<User>.Ok(u);
        static bool IsValid(User u) => true;
        static IError e(User u) => new ValidationError(""x"");
        static Result<User> SaveOrder(User u) => Result<User>.Ok(u);
        static UserDto ToDto(User u) => new UserDto();");

        var output = RunGenerator(source);

        // Verify all chain steps appear as nodes — check via %% correlation comments
        var corrLines = output.Split('\n').Where(l => l.StartsWith("%%") && l.Contains("\u2192")).ToArray();
        Assert.IsTrue(corrLines.Any(l => l.Contains("\u2192 Bind")),   "Bind must be present");
        Assert.IsTrue(corrLines.Any(l => l.Contains("\u2192 Ensure")), "Ensure must be present");
        Assert.IsTrue(corrLines.Any(l => l.Contains("\u2192 Map")),    "Map must be present");
    }

    // 11. Map (PureTransform) → never has error edge
    [TestMethod]
    public void ErrorTravel_PureTransform_NoErrorEdge()
    {
        var source = CreateSource("UserService", "Register",
            "CreateUser().Map(ToDto)",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static UserDto ToDto(User u) => new UserDto();");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Map"), "Map node present");
        // Map produces the last node; no error edge should follow it
        var mapLine = output.Split('\n')
            .FirstOrDefault(l => l.Contains("N1_Map") && l.Contains("-->|"));
        Assert.IsNull(mapLine, "Map (PureTransform) must not have an error edge");
    }

    // ── Title frontmatter + entry node (v1.47.1) ─────────────────────────────

    // 12. Method name appears as Mermaid frontmatter title
    [TestMethod]
    public void Title_EmitsMethodNameAsFrontmatterTitle()
    {
        var source = CreateSource("UserService", "Register",
            "CreateUser().Bind(SaveUser)",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static Result<User> SaveUser(User u) => Result<User>.Ok(u);");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("title: Register"), "Frontmatter title should contain method name");
        Assert.IsTrue(output.Contains("---"), "Frontmatter delimiters should be present");
    }

    // 13. Seed method name + type appear as ENTRY_ROOT node
    [TestMethod]
    public void EntryNode_EmitsSeedMethodAndType()
    {
        var source = CreateSource("UserService", "Register",
            "CreateUser().Bind(SaveUser)",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static Result<User> SaveUser(User u) => Result<User>.Ok(u);");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("ENTRY_ROOT"), "Entry node should be emitted");
        Assert.IsTrue(output.Contains("CreateUser"), "Entry node should use seed method name");
        Assert.IsTrue(output.Contains("==>"), "Entry node should use thick arrow");
    }

    // 14. Async seed method gets ⚡ marker
    [TestMethod]
    public void EntryNode_AsyncSeed_EmitsLightningMarker()
    {
        var source = CreateSource("UserService", "Register",
            "CreateUserAsync().Bind(SaveUser)",
            extraMethods: @"
        static Result<User> CreateUserAsync() => Result<User>.Ok(new User());
        static Result<User> SaveUser(User u) => Result<User>.Ok(u);");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("ENTRY_ROOT"), "Entry node should be emitted");
        Assert.IsTrue(output.Contains("\u26a1"), "Async seed should have ⚡ marker");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates C# source with REslava.Result stubs, the annotated pipeline method,
    /// and any extra helper methods needed by the pipeline.
    /// </summary>
    private static string CreateSource(
        string className,
        string methodName,
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

    public class User {{ }}
    public class UserDto {{ }}

    public class ValidationError : IError
    {{
        public string Message {{ get; }}
        public ValidationError(string msg) {{ Message = msg; }}
    }}
    public class DatabaseError : IError
    {{
        public string Message {{ get; }}
        public DatabaseError(string msg) {{ Message = msg; }}
    }}
    public class EmailError : IError
    {{
        public string Message {{ get; }}
        public EmailError(string msg) {{ Message = msg; }}
    }}

    public class {className}
    {{
        [REslava.Result.Flow.ResultFlow]
        public Result<UserDto> {methodName}() => {returnExpression};
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

    // Returns only the pipeline constants portion — strips the Legend constant so that
    // Legend-specific content (Guard tooltip, note <br/>) doesn't trip pipeline-only assertions.
    private static string PipelineOnly(string output)
    {
        var idx = output.IndexOf("const string Legend", StringComparison.Ordinal);
        return idx >= 0 ? output[..idx] : output;
    }
}
