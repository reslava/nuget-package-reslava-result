using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using REslava.ResultFlow.Generators.ResultFlow;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace REslava.ResultFlow.Tests;

/// <summary>
/// Tests for the ResultFlowDefaultTheme MSBuild property (REslava.ResultFlow package).
/// Parity tests with REslava.Result.Flow.Tests.ResultFlowDefaultThemeTests.
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

    // Source with no explicit Theme argument — uses MSBuild default (syntax-only, no REslava.Result dependency)
    private static string SourceNoTheme() => @"
using System;
namespace TestNS
{
    public class Order { }
    public class OrderDto { }
    public class OrderService
    {
        [REslava.ResultFlow.ResultFlow]
        public OrderDto PlaceOrder() => GetOrder().Bind(Validate).Map(ToDto);
        private OrderDto GetOrder() => new OrderDto();
        private OrderDto Validate(Order o) => new OrderDto();
        private OrderDto ToDto(Order o) => new OrderDto();
    }
}";

    // Source with explicit Theme = Dark attribute argument
    private static string SourceDarkThemeAttribute() => @"
using System;
namespace TestNS
{
    public class Order { }
    public class OrderDto { }
    public class OrderService
    {
        [REslava.ResultFlow.ResultFlow(Theme = REslava.ResultFlow.ResultFlowTheme.Dark)]
        public OrderDto PlaceOrder() => GetOrder().Bind(Validate).Map(ToDto);
        private OrderDto GetOrder() => new OrderDto();
        private OrderDto Validate(Order o) => new OrderDto();
        private OrderDto ToDto(Order o) => new OrderDto();
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
