using REslava.Result.Flow.Generators.ErrorTaxonomy;
using System.Collections.Immutable;
using System.Linq;

namespace REslava.Result.Flow.Tests;

[TestClass]
public class ErrorTaxonomyTests
{
    // ── 1. Result<T, TError> method → certain row emitted ────────────────────
    [TestMethod]
    public void Certain_EmittedFor_ResultTTError_ReturnType()
    {
        var source = Wrap(@"
            public Result<Order, NotFoundError> GetOrder(int id) =>
                Result<Order, NotFoundError>.Ok(new Order());");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("_ErrorTaxonomy"), "_ErrorTaxonomy constant must be emitted");
        Assert.IsTrue(output.Contains("GetOrder"), "GetOrder must appear in the table");
        Assert.IsTrue(output.Contains("NotFoundError"), "NotFoundError must appear in the table");
        Assert.IsTrue(output.Contains("certain"), "Confidence must be 'certain' for Result<T,TError>");
    }

    // ── 2. Result<T> with Fail(new XxxError()) → inferred row ────────────────
    [TestMethod]
    public void Inferred_EmittedFor_Fail_WithObjectCreation()
    {
        var source = Wrap(@"
            [ResultFlow]
            public Result<Order> PlaceOrder(int id) =>
                Result<Order>.Fail(new ValidationError(""bad""));");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("_ErrorTaxonomy"), "_ErrorTaxonomy constant must be emitted");
        Assert.IsTrue(output.Contains("PlaceOrder"), "PlaceOrder must appear in the table");
        Assert.IsTrue(output.Contains("ValidationError"), "ValidationError must appear in the table");
        Assert.IsTrue(output.Contains("inferred"), "Confidence must be 'inferred' for body scan");
    }

    // ── 3. Result<T> with Ensure(..., new XxxError()) → inferred row ─────────
    [TestMethod]
    public void Inferred_EmittedFor_Ensure_WithObjectCreation()
    {
        var source = Wrap(@"
            [ResultFlow]
            public Result<Order> ValidateOrder(Order o) =>
                Result<Order>.Ok(o).Ensure(x => x.Id > 0, new ValidationError(""invalid""));");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("_ErrorTaxonomy"), "_ErrorTaxonomy constant must be emitted");
        Assert.IsTrue(output.Contains("ValidateOrder"), "ValidateOrder must appear in the table");
        Assert.IsTrue(output.Contains("ValidationError"), "ValidationError must appear in the table");
        Assert.IsTrue(output.Contains("inferred"), "Confidence must be 'inferred' for body scan");
    }

    // ── 4. Fail("message") string overload → no row ──────────────────────────
    [TestMethod]
    public void NoRow_For_Fail_WithStringMessage()
    {
        var source = Wrap(@"
            [ResultFlow]
            public Result<Order> GetOrder(int id) =>
                Result<Order>.Fail(""not found"");");

        var output = RunGenerator(source);

        // String overload carries no type info — no row, possibly no file at all
        var hasTable = output.Contains("_ErrorTaxonomy") && output.Contains("GetOrder");
        if (hasTable)
        {
            // If a row was emitted it must not be for the string-only Fail
            Assert.IsFalse(output.Contains("| GetOrder |"), "No row should be emitted for Fail(string)");
        }
        // If no _ErrorTaxonomy constant at all, the test trivially passes
    }

    // ── 5. Multiple error types on one method → multiple rows ─────────────────
    [TestMethod]
    public void MultipleRows_For_MultipleErrors_OnOneMethod()
    {
        var source = Wrap(@"
            [ResultFlow]
            public Result<Order> PlaceOrder(int id)
            {
                if (id <= 0) return Result<Order>.Fail(new ValidationError(""bad""));
                return Result<Order>.Fail(new NotFoundError(""missing""));
            }");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("ValidationError"), "ValidationError row must be emitted");
        Assert.IsTrue(output.Contains("NotFoundError"), "NotFoundError row must be emitted");
    }

    // ── 6. Duplicate (method, error) pairs → de-duplicated ───────────────────
    [TestMethod]
    public void Duplicates_AreDeduped()
    {
        var source = Wrap(@"
            [ResultFlow]
            public Result<Order> PlaceOrder(int id)
            {
                if (id <= 0) return Result<Order>.Fail(new ValidationError(""a""));
                if (id > 100) return Result<Order>.Fail(new ValidationError(""b""));
                return Result<Order>.Ok(new Order());
            }");

        var output = RunGenerator(source);

        // ValidationError must appear exactly once in the table (de-duplicated)
        var occurrences = CountOccurrences(output, "ValidationError");
        Assert.AreEqual(1, occurrences, "Duplicate (method, error) pairs must be de-duplicated to one row");
    }

    // ── 7. Class with no detectable errors → no _ErrorTaxonomy generated ──────
    [TestMethod]
    public void NoOutput_WhenNoErrorsDetected()
    {
        var source = Wrap(@"
            [ResultFlow]
            public Result<Order> GetOrder(int id) =>
                Result<Order>.Ok(new Order());");

        var output = RunGenerator(source);

        Assert.IsFalse(output.Contains("_ErrorTaxonomy"),
            "_ErrorTaxonomy must not be emitted when no errors are detected");
    }

    // ── 8. Class with no [ResultFlow] method → no _ErrorTaxonomy generated ────
    [TestMethod]
    public void NoOutput_WhenNoResultFlowMethod()
    {
        // No [ResultFlow] attribute on any method
        var source = BaseStubs() + @"
namespace MyApp {
    using REslava.Result;
    using SharedTypes;
    public class OrderService {
        public Result<Order, NotFoundError> GetOrder(int id) =>
            Result<Order, NotFoundError>.Ok(new Order());
    }
}";
        var output = RunGenerator(source);

        Assert.IsFalse(output.Contains("_ErrorTaxonomy"),
            "_ErrorTaxonomy must not be emitted for classes without [ResultFlow] methods");
    }

    // ── 9. Certain rows appear in output with header ──────────────────────────
    [TestMethod]
    public void Output_ContainsMarkdownTableHeader()
    {
        var source = Wrap(@"
            public Result<Order, NotFoundError> GetOrder(int id) =>
                Result<Order, NotFoundError>.Ok(new Order());");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("| Method | Error Type | Confidence |"),
            "Output must contain the markdown table header");
        Assert.IsTrue(output.Contains("|---|---|---|"),
            "Output must contain the markdown table separator");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Wraps a method body inside a standard OrderService class with [ResultFlow].</summary>
    private static string Wrap(string methodBody) =>
        BaseStubs() + $@"
namespace MyApp {{
    using REslava.Result;
    using SharedTypes;
    public class OrderService {{
        [ResultFlow]
        {methodBody}
    }}
}}";

    private static string BaseStubs() => @"
using System;
using System.Collections.Immutable;

[System.AttributeUsage(System.AttributeTargets.Method)]
public class ResultFlowAttribute : System.Attribute { }

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
        public static Result<T> Fail(IError error) => new Result<T>();
        public static Result<T> Fail(string msg) => new Result<T>();
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> f) => new Result<TOut>();
        public Result<T> Ensure(Func<T, bool> p, IError e) => new Result<T>();
        public Result<TOut> Map<TOut>(Func<T, TOut> f) => new Result<TOut>();
        public Result<T> Tap(Action<T> a) => this;
    }
    public class Result<T, TError> : IResultBase<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure { get; }
        public T? Value { get; }
        public ImmutableList<IReason> Reasons => ImmutableList<IReason>.Empty;
        public ImmutableList<IError> Errors => ImmutableList<IError>.Empty;
        public ImmutableList<ISuccess> Successes => ImmutableList<ISuccess>.Empty;
        public static Result<T, TError> Ok(T value) => new Result<T, TError>();
        public static Result<T, TError> Fail(TError error) => new Result<T, TError>();
    }
}

namespace SharedTypes
{
    using REslava.Result;
    public class Order { public int Id { get; } }
    public class ValidationError : IError
    {
        public string Message { get; }
        public ValidationError(string msg) { Message = msg; }
    }
    public class NotFoundError : IError
    {
        public string Message { get; }
        public NotFoundError(string msg) { Message = msg; }
    }
}
";

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

        var generator = new ErrorTaxonomyGenerator();
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
