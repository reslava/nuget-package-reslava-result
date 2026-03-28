using REslava.Result.Flow.Generators.ResultFlow;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace REslava.Result.Flow.Tests;

/// <summary>
/// Tests for the _TypeFlow diagram constant (v1.51.0):
/// emits edge labels carrying the Result&lt;T&gt; success type at each chain step.
/// </summary>
[TestClass]
public class TypeFlowTests
{
    // ── Infrastructure ────────────────────────────────────────────────────────

    private static string RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));
        var compilation = CSharpCompilation.Create(
            "TypeFlowTestCompilation",
            new[] { syntaxTree },
            new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ImmutableList<>).Assembly.Location),
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var driver = CSharpGeneratorDriver.Create(new ResultFlowGenerator());
        var updatedDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var sb = new System.Text.StringBuilder();
        foreach (var tree in updatedDriver.GetRunResult().GeneratedTrees)
        {
            using var w = new System.IO.StringWriter();
            tree.GetText().Write(w);
            sb.AppendLine(w.ToString());
        }
        return sb.ToString();
    }

    /// <summary>
    /// Builds source with full stubs that include Bind/Map/Ensure so the semantic
    /// model can resolve method calls and extract output types for TypeFlow labels.
    /// </summary>
    private static string CreateSource(string chain, string extraMethods = "")
    {
        return $@"
using System;
using System.Collections.Immutable;
namespace REslava.Result
{{
    public interface IReason {{ string Message {{ get; }} }}
    public interface IError : IReason {{ }}
    public interface IResultBase {{ bool IsSuccess {{ get; }} bool IsFailure {{ get; }} ImmutableList<IReason> Reasons {{ get; }} ImmutableList<IError> Errors {{ get; }} }}
    public interface IResultBase<out T> : IResultBase {{ T? Value {{ get; }} }}
    public class Result<T> : IResultBase<T>
    {{
        public bool IsSuccess {{ get; }}
        public bool IsFailure {{ get; }}
        public T? Value {{ get; }}
        public ImmutableList<IReason> Reasons => ImmutableList<IReason>.Empty;
        public ImmutableList<IError> Errors => ImmutableList<IError>.Empty;
        public static Result<T> Ok(T value) => new Result<T>();
        public static Result<T> Fail(IError error) => new Result<T>();
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> f) => new Result<TOut>();
        public Result<T> Ensure(Func<T, bool> p, Func<T, IError> e) => new Result<T>();
        public Result<TOut> Map<TOut>(Func<T, TOut> f) => new Result<TOut>();
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
    public class TypeFlowService
    {{
        [REslava.Result.Flow.ResultFlow]
        public Result<UserDto> Process() => {chain};
        {extraMethods}
    }}
}}";
    }

    // ── TypeFlow constant presence ─────────────────────────────────────────────

    [TestMethod]
    public void TypeFlow_Constant_IsEmitted()
    {
        // _TypeFlow constant must appear alongside _Diagram in the generated file
        var output = RunGenerator(CreateSource(
            "GetUser().Bind(ValidateUser).Map(ToDto)",
            extraMethods: @"
        static Result<User> GetUser() => Result<User>.Ok(new User());
        static Result<User> ValidateUser(User u) => Result<User>.Ok(u);
        static UserDto ToDto(User u) => new UserDto();"));
        Assert.IsTrue(output.Contains("Process_TypeFlow"),
            "TypeFlow constant must be emitted for [ResultFlow] method");
    }

    // ── TypeFlow edge labels ──────────────────────────────────────────────────

    [TestMethod]
    public void TypeFlow_SuccessEdges_CarryTypeName()
    {
        // Bind(ValidateUser) returns Result<User> → OutputType = "User"
        // In TypeFlow the intermediate edge carries the type: -->|"User"| nextId
        // In verbatim-string notation quotes are doubled: -->|""User""|
        var output = RunGenerator(CreateSource(
            "GetUser().Bind(ValidateUser).Map(ToDto)",
            extraMethods: @"
        static Result<User> GetUser() => Result<User>.Ok(new User());
        static Result<User> ValidateUser(User u) => Result<User>.Ok(u);
        static UserDto ToDto(User u) => new UserDto();"));
        Assert.IsTrue(output.Contains("\"\"User\"\""),
            "TypeFlow Bind success edge must carry the output type name (\"\"User\"\")");
    }

    [TestMethod]
    public void TypeFlow_FailureEdges_NotDoubleQuoted()
    {
        // Failure edge labels (|fail| or |ValidationError|) must never be double-quoted.
        // Type labels in verbatim strings look like -->|""TypeName""| — failure edges
        // must NOT use this format; they keep their plain label (no surrounding quotes).
        var output = RunGenerator(CreateSource(
            "GetUser().Ensure(u => u != null, u => new ValidationError(\"x\"))",
            extraMethods: @"
        static Result<User> GetUser() => Result<User>.Ok(new User());"));
        Assert.IsFalse(output.Contains("|\"\"fail\"\""),
            "Failure 'fail' edge must not be double-quoted in TypeFlow");
        Assert.IsFalse(output.Contains("|\"\"Validation"),
            "Typed error edge ValidationError must not be double-quoted in TypeFlow");
    }

    // ── Structural parity ────────────────────────────────────────────────────

    [TestMethod]
    public void TypeFlow_ContainsFlowchart()
    {
        // Both _Diagram and _TypeFlow must be valid Mermaid diagrams (flowchart LR header)
        var output = RunGenerator(CreateSource(
            "GetUser().Bind(ValidateUser).Map(ToDto)",
            extraMethods: @"
        static Result<User> GetUser() => Result<User>.Ok(new User());
        static Result<User> ValidateUser(User u) => Result<User>.Ok(u);
        static UserDto ToDto(User u) => new UserDto();"));
        var count = 0;
        var pos = 0;
        while ((pos = output.IndexOf("flowchart LR", pos, System.StringComparison.Ordinal)) >= 0)
        {
            count++;
            pos++;
        }
        Assert.IsTrue(count >= 2, "Both _Diagram and _TypeFlow must contain 'flowchart LR'");
    }
}
