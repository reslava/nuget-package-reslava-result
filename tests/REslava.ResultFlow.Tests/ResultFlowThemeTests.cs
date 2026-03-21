using REslava.ResultFlow.Generators.ResultFlow.CodeGeneration;
using REslava.ResultFlow.Generators.ResultFlow.Models;
using System.Collections.Generic;

namespace REslava.ResultFlow.Tests;

/// <summary>
/// Tests for Light/Dark theme emission in ResultFlowMermaidRenderer (REslava.ResultFlow package).
/// Parity tests with REslava.Result.Flow.Tests.ResultFlowThemeTests.
/// </summary>
[TestClass]
public class ResultFlowThemeTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static PipelineNode MakeNode(string name, NodeKind kind)
        => new PipelineNode(name, kind);

    private static IReadOnlyList<PipelineNode> SimpleChain()
        => new List<PipelineNode>
        {
            MakeNode("ValidateOrder", NodeKind.Gatekeeper),
            MakeNode("ProcessPayment", NodeKind.TransformWithRisk),
        };

    private static IReadOnlyList<PipelineNode> ChainWithSubgraph()
    {
        var inner = new List<PipelineNode>
        {
            MakeNode("CheckFunds", NodeKind.Gatekeeper),
        };
        var outer = new PipelineNode("ProcessPayment", NodeKind.TransformWithRisk)
        {
            SubNodes = inner
        };
        return new List<PipelineNode> { outer };
    }

    // ── MermaidInit ──────────────────────────────────────────────────────────

    [TestMethod]
    public void Render_LightTheme_ContainsMermaidInit()
    {
        var output = ResultFlowMermaidRenderer.Render(SimpleChain());
        Assert.IsTrue(output.Contains(ResultFlowThemes.MermaidInit), "MermaidInit must be present");
        Assert.IsTrue(output.Contains("'theme': 'base'"), "theme:base must be present");
    }

    [TestMethod]
    public void Render_DarkTheme_ContainsMermaidInit()
    {
        var output = ResultFlowMermaidRenderer.Render(SimpleChain(), darkTheme: true);
        Assert.IsTrue(output.Contains(ResultFlowThemes.MermaidInitDark), "MermaidInitDark must be present in dark theme");
        Assert.IsTrue(output.Contains("primaryTextColor"), "dark init must include primaryTextColor for white title text");
    }

    [TestMethod]
    public void Render_MermaidInit_AppearsBeforeFlowchart()
    {
        var output = ResultFlowMermaidRenderer.Render(SimpleChain(), methodTitle: "PlaceOrder");
        var initPos = output.IndexOf(ResultFlowThemes.MermaidInit);
        var flowchartPos = output.IndexOf("flowchart LR");
        Assert.IsTrue(initPos < flowchartPos, "MermaidInit must appear before flowchart LR");
        var frontmatterEnd = output.IndexOf("---", output.IndexOf("---") + 3) + 3;
        Assert.IsTrue(initPos > frontmatterEnd, "MermaidInit must appear after frontmatter");
    }

    // ── Light theme ──────────────────────────────────────────────────────────

    [TestMethod]
    public void Render_DefaultTheme_EmitsLightClassDefs()
    {
        var output = ResultFlowMermaidRenderer.Render(SimpleChain());
        Assert.IsTrue(output.Contains("classDef operation  fill:#faf0e3"), "Light operation classDef");
        Assert.IsTrue(output.Contains("classDef gatekeeper fill:#e3e9fa"), "Light gatekeeper classDef");
        Assert.IsTrue(output.Contains("classDef bind       fill:#e3f0e8"), "Light bind classDef");
        Assert.IsTrue(output.Contains("classDef success    fill:#e8f4f0"), "Light success classDef (aligned to extra.css)");
        Assert.IsTrue(output.Contains("classDef failure    fill:#f8e3e3"), "Light failure classDef");
    }

    [TestMethod]
    public void Render_LightTheme_LinkStyleIsLight()
    {
        var output = ResultFlowMermaidRenderer.Render(SimpleChain());
        Assert.IsTrue(output.Contains("linkStyle default stroke:#888"), "Light linkStyle");
    }

    [TestMethod]
    public void Render_LightTheme_DoesNotContainDarkColors()
    {
        var output = ResultFlowMermaidRenderer.Render(SimpleChain());
        Assert.IsFalse(output.Contains("#3a2b1f"), "Dark operation bg must not appear in light theme");
        Assert.IsFalse(output.Contains("#1f3a2d"), "Dark bind bg must not appear in light theme");
    }

    // ── Dark theme ───────────────────────────────────────────────────────────

    [TestMethod]
    public void Render_DarkTheme_EmitsDarkClassDefs()
    {
        var output = ResultFlowMermaidRenderer.Render(SimpleChain(), darkTheme: true);
        Assert.IsTrue(output.Contains("classDef operation  fill:#3a2b1f"), "Dark operation classDef");
        Assert.IsTrue(output.Contains("classDef gatekeeper fill:#1f263a"), "Dark gatekeeper classDef");
        Assert.IsTrue(output.Contains("classDef bind       fill:#1f3a2d"), "Dark bind classDef");
        Assert.IsTrue(output.Contains("classDef success    fill:#1f3a36"), "Dark success classDef");
        Assert.IsTrue(output.Contains("classDef failure    fill:#3a1f1f"), "Dark failure classDef");
    }

    [TestMethod]
    public void Render_DarkTheme_LinkStyleIsDark()
    {
        var output = ResultFlowMermaidRenderer.Render(SimpleChain(), darkTheme: true);
        Assert.IsTrue(output.Contains("linkStyle default stroke:#666"), "Dark linkStyle");
    }

    [TestMethod]
    public void Render_DarkTheme_DoesNotContainLightColors()
    {
        var output = ResultFlowMermaidRenderer.Render(SimpleChain(), darkTheme: true);
        Assert.IsFalse(output.Contains("#faf0e3"), "Light operation bg must not appear in dark theme");
        Assert.IsFalse(output.Contains("#e3e9fa"), "Light gatekeeper bg must not appear in dark theme");
    }

    // ── Subgraph class assignment ─────────────────────────────────────────────

    [TestMethod]
    public void Render_WithSubgraph_EmitsClassSubgraphStyle()
    {
        var output = ResultFlowMermaidRenderer.Render(ChainWithSubgraph(), darkTheme: false);
        Assert.IsTrue(output.Contains("class sg_"), "class sg_* subgraphStyle must be emitted");
        Assert.IsTrue(output.Contains("subgraphStyle"), "subgraphStyle class name must be referenced");
    }

    [TestMethod]
    public void Render_WithSubgraph_LightSubgraphColor()
    {
        var output = ResultFlowMermaidRenderer.Render(ChainWithSubgraph());
        Assert.IsTrue(output.Contains("classDef subgraphStyle fill:#ffffde"), "Light subgraph bg");
    }

    [TestMethod]
    public void Render_WithSubgraph_DarkSubgraphColor()
    {
        var output = ResultFlowMermaidRenderer.Render(ChainWithSubgraph(), darkTheme: true);
        Assert.IsTrue(output.Contains("classDef subgraphStyle fill:#252520"), "Dark subgraph bg");
    }

    [TestMethod]
    public void Render_FlatPipeline_NoSubgraphClassAssignment()
    {
        var output = ResultFlowMermaidRenderer.Render(SimpleChain());
        Assert.IsFalse(output.Contains("class sg_"), "No class sg_* line for flat pipeline");
    }

    // ── Theme block appears after diagram content ─────────────────────────────

    [TestMethod]
    public void Render_ThemeBlock_AppearsAfterNodes()
    {
        var output = ResultFlowMermaidRenderer.Render(SimpleChain());
        var nodePos = output.IndexOf(":::gatekeeper");
        var themePos = output.IndexOf("classDef entry");
        Assert.IsTrue(themePos > nodePos, "Theme block must appear after node declarations");
    }
}
