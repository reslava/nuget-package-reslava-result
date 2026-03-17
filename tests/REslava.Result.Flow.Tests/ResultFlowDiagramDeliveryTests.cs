using REslava.Result.Flow.Generators.ResultFlow.CodeGeneration;
using REslava.Result.Flow.Generators.ResultFlow.Models;
using System.Collections.Generic;

namespace REslava.Result.Flow.Tests;

/// <summary>
/// Phase 3 tests for Block 3 (diagram delivery):
/// #9 Clickable Mermaid nodes (click directives) and #8 Sidecar markdown constant.
/// </summary>
[TestClass]
public class ResultFlowDiagramDeliveryTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static PipelineNode MakeNode(string name, NodeKind kind, string? file = null, int? line = null)
        => new PipelineNode(name, kind) { SourceFile = file, SourceLine = line };

    private static IReadOnlyList<PipelineNode> TwoNodeChain(string? file = null)
        => new List<PipelineNode>
        {
            MakeNode("FindUser", NodeKind.Unknown,          file, file != null ? 10 : (int?)null),
            MakeNode("SaveUser", NodeKind.TransformWithRisk, file, file != null ? 11 : (int?)null),
        };

    // ── #9 Clickable nodes — vscode mode ─────────────────────────────────────

    [TestMethod]
    public void ClickDirectives_VscodeMode_EmittedForNodesWithSourceLocation()
    {
        var nodes = TwoNodeChain("/src/OrderService.cs");
        var output = ResultFlowMermaidRenderer.Render(nodes, linkMode: "vscode");

        Assert.IsTrue(output.Contains("click"), "click directive must be present");
        Assert.IsTrue(output.Contains("vscode://file/"), "VS Code URI scheme must be present");
        Assert.IsTrue(output.Contains("/src/OrderService.cs:10"), "File path and line for FindUser");
        Assert.IsTrue(output.Contains("/src/OrderService.cs:11"), "File path and line for SaveUser");
    }

    [TestMethod]
    public void ClickDirectives_VscodeMode_IncludesMethodNameTooltip()
    {
        var nodes = TwoNodeChain("/src/Service.cs");
        var output = ResultFlowMermaidRenderer.Render(nodes, linkMode: "vscode");

        Assert.IsTrue(output.Contains("Go to FindUser"), "Tooltip must reference method name");
        Assert.IsTrue(output.Contains("Go to SaveUser"), "Tooltip must reference method name");
    }

    [TestMethod]
    public void ClickDirectives_NullMode_NotEmitted()
    {
        var nodes = TwoNodeChain("/src/OrderService.cs");
        var output = ResultFlowMermaidRenderer.Render(nodes, linkMode: null);

        Assert.IsFalse(output.Contains("click"), "No click directives when linkMode is null");
    }

    [TestMethod]
    public void ClickDirectives_NoneMode_NotEmitted()
    {
        var nodes = TwoNodeChain("/src/OrderService.cs");
        var output = ResultFlowMermaidRenderer.Render(nodes, linkMode: "none");

        Assert.IsFalse(output.Contains("click"), "No click directives when linkMode is 'none'");
    }

    [TestMethod]
    public void ClickDirectives_NoSourceFile_NotEmitted()
    {
        // Nodes without SourceFile (in-memory compilation) — no click directives even in vscode mode
        var nodes = TwoNodeChain(file: null);
        var output = ResultFlowMermaidRenderer.Render(nodes, linkMode: "vscode");

        Assert.IsFalse(output.Contains("click"), "No click when SourceFile is null");
    }

    [TestMethod]
    public void ClickDirectives_WindowsPath_NormalizedToForwardSlashes()
    {
        var nodes = new List<PipelineNode>
        {
            MakeNode("Process", NodeKind.Unknown, @"C:\src\Service.cs", 42),
        };
        var output = ResultFlowMermaidRenderer.Render(nodes, linkMode: "vscode");

        Assert.IsTrue(output.Contains("vscode://file/C:/src/Service.cs:42"),
            "Backslashes must be converted to forward slashes in VS Code URI");
    }

    // ── #8 Sidecar markdown constant ─────────────────────────────────────────

    [TestMethod]
    public void SidecarConstant_AlwaysGeneratedInOutput()
    {
        var diagrams = new List<(string methodName, string mermaid, string? layerView, string? stats, string? errorSurface, string? errorPropagation)> { ("Process", "flowchart LR\n    N0_Process[\"Process\"]:::operation", null, null, null, null) };
        var output = ResultFlowCodeGenerator.Generate("OrderService", diagrams).ToString();

        Assert.IsTrue(output.Contains("Process_Sidecar"), "Sidecar constant must be generated");
    }

    [TestMethod]
    public void SidecarConstant_ContainsMermaidFencedBlock()
    {
        var mermaid = "flowchart LR\n    N0_Op[\"Op\"]:::operation";
        var diagrams = new List<(string methodName, string mermaid, string? layerView, string? stats, string? errorSurface, string? errorPropagation)> { ("Op", mermaid, null, null, null, null) };
        var output = ResultFlowCodeGenerator.Generate("MyService", diagrams).ToString();

        Assert.IsTrue(output.Contains("```mermaid"), "Sidecar must contain mermaid fenced block");
        Assert.IsTrue(output.Contains("# Pipeline"), "Sidecar must have a heading");
    }

    [TestMethod]
    public void SidecarConstant_ContainsDiagramContent()
    {
        var mermaid = "flowchart LR\n    N0_FindUser[\"FindUser\"]:::operation";
        var diagrams = new List<(string methodName, string mermaid, string? layerView, string? stats, string? errorSurface, string? errorPropagation)> { ("FindUser", mermaid, null, null, null, null) };
        var output = ResultFlowCodeGenerator.Generate("UserService", diagrams).ToString();

        // The sidecar constant should contain the diagram content
        Assert.IsTrue(output.Contains("FindUser_Sidecar"), "Sidecar constant name");
        Assert.IsTrue(output.Contains("N0_FindUser"), "Sidecar must embed the diagram content");
    }

    [TestMethod]
    public void DiagramConstant_UnaffectedBySidecarAddition()
    {
        // The original diagram constant must still be present alongside the sidecar
        var mermaid = "flowchart LR\n    N0_Op[\"Op\"]:::operation";
        var diagrams = new List<(string methodName, string mermaid, string? layerView, string? stats, string? errorSurface, string? errorPropagation)> { ("Op", mermaid, null, null, null, null) };
        var output = ResultFlowCodeGenerator.Generate("Service", diagrams).ToString();

        // Both constants present
        Assert.IsTrue(output.Contains("public const string Op ="), "Original diagram constant must still exist");
        Assert.IsTrue(output.Contains("public const string Op_Sidecar ="), "Sidecar constant alongside it");
    }
}
