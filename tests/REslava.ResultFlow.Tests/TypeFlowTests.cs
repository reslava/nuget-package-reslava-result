using REslava.ResultFlow.Generators.ResultFlow;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace REslava.ResultFlow.Tests;

/// <summary>
/// Tests for the _TypeFlow diagram constant (v1.51.0).
/// Syntax-only variant (REslava.ResultFlow — no semantic model).
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

    private const string SimpleSource = @"
namespace TestNS
{
    public class TypeFlowService
    {
        [REslava.ResultFlow.ResultFlow]
        public string Process(string cmd)
            => GetUser(cmd)
                .Bind(Validate)
                .Map(ToDto);
    }
}";

    // ── TypeFlow constant presence ─────────────────────────────────────────────

    [TestMethod]
    public void TypeFlow_Constant_IsEmitted()
    {
        // _TypeFlow constant must appear alongside _Diagram in the generated file
        var output = RunGenerator(SimpleSource);
        Assert.IsTrue(output.Contains("Process_TypeFlow"),
            "TypeFlow constant must be emitted for [ResultFlow] method");
    }

    // ── Structural parity ─────────────────────────────────────────────────────

    [TestMethod]
    public void TypeFlow_ContainsFlowchart()
    {
        // Both _Diagram and _TypeFlow must contain 'flowchart LR' (two separate constants)
        var output = RunGenerator(SimpleSource);
        var count = 0;
        var pos = 0;
        while ((pos = output.IndexOf("flowchart LR", pos, System.StringComparison.Ordinal)) >= 0)
        {
            count++;
            pos++;
        }
        Assert.IsTrue(count >= 2, "Both _Diagram and _TypeFlow must contain 'flowchart LR'");
    }

    [TestMethod]
    public void TypeFlow_ContainsSameNodes_AsMainDiagram()
    {
        // _TypeFlow has the same pipeline node labels as _Diagram.
        // Method-group arguments like .Bind(Validate) keep the pipeline method name as label.
        // Lambda arguments like .Bind(x => Validate(x)) use the inner method name via Gap 1.
        var output = RunGenerator(SimpleSource);
        Assert.IsTrue(output.Contains("Bind"),
            "TypeFlow must contain node label for Bind step");
        Assert.IsTrue(output.Contains("Map"),
            "TypeFlow must contain node label for Map step");
    }
}
