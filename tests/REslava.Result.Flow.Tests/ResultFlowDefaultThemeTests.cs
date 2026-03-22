using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using REslava.Result.Flow.Generators.ResultFlow;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace REslava.Result.Flow.Tests;

/// <summary>
/// Tests for the ResultFlowDefaultTheme MSBuild property.
/// Verifies that build_property.ResultFlowDefaultTheme is picked up by the generator
/// and that a method-level [ResultFlow(Theme = Dark)] always wins over the MSBuild default.
/// </summary>
[TestClass]
public class ResultFlowDefaultThemeTests
{
    // ── Test infrastructure ──────────────────────────────────────────────────

    private sealed class DictAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> _values;
        public DictAnalyzerConfigOptions(Dictionary<string, string> values) => _values = values;
        public override bool TryGetValue(string key, out string value) =>
            _values.TryGetValue(key, out value!);
    }

    private sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private static readonly AnalyzerConfigOptions Empty =
            new DictAnalyzerConfigOptions(new Dictionary<string, string>());

        private readonly AnalyzerConfigOptions _globalOptions;

        public TestAnalyzerConfigOptionsProvider(Dictionary<string, string> globalProps) =>
            _globalOptions = new DictAnalyzerConfigOptions(globalProps);

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => Empty;
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => Empty;
    }

    private static string RunGeneratorWithProps(string source, Dictionary<string, string> buildProps)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ImmutableList<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(REslava.Result.IResultBase).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "TestCompilationDefaultTheme",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ResultFlowGenerator();
        var optionsProvider = new TestAnalyzerConfigOptionsProvider(buildProps);
        var driver = (GeneratorDriver)CSharpGeneratorDriver.Create(generator);
        driver = driver.WithUpdatedAnalyzerConfigOptions(optionsProvider);

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

    // Source with no explicit Theme argument — uses MSBuild default
    private static string SourceNoTheme() => @"
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
        public Result<T> Ensure(Func<T, bool> p, Func<T, string> e) => new Result<T>();
        public Result<TOut> Map<TOut>(Func<T, TOut> f) => new Result<TOut>();
    }
}
namespace TestNS
{
    using REslava.Result;
    public class Order { }
    public class OrderDto { }
    public class OrderService
    {
        [REslava.Result.Flow.ResultFlow]
        public Result<OrderDto> PlaceOrder() =>
            Result<Order>.Ok(new Order())
                .Ensure(o => o != null, _ => ""invalid"")
                .Map(o => new OrderDto());
    }
}";

    // Source with explicit Theme = Dark attribute argument
    private static string SourceDarkThemeAttribute() => @"
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
        public Result<T> Ensure(Func<T, bool> p, Func<T, string> e) => new Result<T>();
        public Result<TOut> Map<TOut>(Func<T, TOut> f) => new Result<TOut>();
    }
}
namespace TestNS
{
    using REslava.Result;
    public class Order { }
    public class OrderDto { }
    public class OrderService
    {
        [REslava.Result.Flow.ResultFlow(Theme = REslava.Result.Flow.ResultFlowTheme.Dark)]
        public Result<OrderDto> PlaceOrder() =>
            Result<Order>.Ok(new Order())
                .Ensure(o => o != null, _ => ""invalid"")
                .Map(o => new OrderDto());
    }
}";

    // ── Tests ────────────────────────────────────────────────────────────────

    [TestMethod]
    public void DefaultTheme_NoBuildProperty_GeneratesLightDiagram()
    {
        var output = RunGeneratorWithProps(SourceNoTheme(), new Dictionary<string, string>());

        Assert.IsTrue(output.Contains("classDef operation  fill:#faf0e3"), "Light operation classDef expected");
        Assert.IsFalse(output.Contains("#3a2b1f"), "Dark operation bg must not appear when no MSBuild property set");
    }

    [TestMethod]
    public void DefaultTheme_BuildPropertyDark_GeneratesDarkDiagram()
    {
        var output = RunGeneratorWithProps(SourceNoTheme(), new Dictionary<string, string>
        {
            ["build_property.ResultFlowDefaultTheme"] = "Dark"
        });

        Assert.IsTrue(output.Contains("classDef operation  fill:#3a2b1f"), "Dark operation classDef expected");
        Assert.IsFalse(output.Contains("#faf0e3"), "Light operation bg must not appear when MSBuild default is Dark");
    }

    [TestMethod]
    public void DefaultTheme_BuildPropertyLight_GeneratesLightDiagram()
    {
        var output = RunGeneratorWithProps(SourceNoTheme(), new Dictionary<string, string>
        {
            ["build_property.ResultFlowDefaultTheme"] = "Light"
        });

        Assert.IsTrue(output.Contains("classDef operation  fill:#faf0e3"), "Light operation classDef expected");
        Assert.IsFalse(output.Contains("#3a2b1f"), "Dark operation bg must not appear when MSBuild default is Light");
    }

    [TestMethod]
    public void DefaultTheme_BuildPropertyCaseInsensitive_Dark()
    {
        var output = RunGeneratorWithProps(SourceNoTheme(), new Dictionary<string, string>
        {
            ["build_property.ResultFlowDefaultTheme"] = "dark"
        });

        Assert.IsTrue(output.Contains("classDef operation  fill:#3a2b1f"), "Dark classDef expected for lowercase 'dark'");
    }

    [TestMethod]
    public void DefaultTheme_MethodAttributeDarkWinsOverLightMSBuildDefault()
    {
        // MSBuild default is absent (= Light), but method explicitly sets Theme = Dark → dark wins
        var output = RunGeneratorWithProps(SourceDarkThemeAttribute(), new Dictionary<string, string>());

        Assert.IsTrue(output.Contains("classDef operation  fill:#3a2b1f"), "Dark classDef expected from method attribute");
        Assert.IsFalse(output.Contains("#faf0e3"), "Light operation bg must not appear when attribute overrides to Dark");
    }

    [TestMethod]
    public void DefaultTheme_MethodAttributeDarkWinsOverDarkMSBuildDefault()
    {
        // Both MSBuild and attribute say Dark — result must still be dark
        var output = RunGeneratorWithProps(SourceDarkThemeAttribute(), new Dictionary<string, string>
        {
            ["build_property.ResultFlowDefaultTheme"] = "Dark"
        });

        Assert.IsTrue(output.Contains("classDef operation  fill:#3a2b1f"), "Dark classDef expected when both MSBuild and attribute are Dark");
    }
}
