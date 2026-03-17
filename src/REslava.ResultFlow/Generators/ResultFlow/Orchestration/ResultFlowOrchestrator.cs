using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.ResultFlow.Core.Interfaces;
using REslava.ResultFlow.Generators.ResultFlow.Attributes;
using REslava.ResultFlow.Generators.ResultFlow.CodeGeneration;
using REslava.ResultFlow.Generators.ResultFlow.Config;
using REslava.ResultFlow.Generators.ResultFlow.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace REslava.ResultFlow.Generators.ResultFlow.Orchestration
{
    internal class ResultFlowOrchestrator : IGeneratorOrchestrator
    {
        private const string AttributeShortName = "ResultFlow";
        private const string ConfigFileName = "resultflow.json";

#pragma warning disable RS2008 // Enable analyzer release tracking
        private static readonly DiagnosticDescriptor REF001 = new DiagnosticDescriptor(
            id: "REF001",
            title: "ResultFlow chain not detected",
            messageFormat: "[ResultFlow] could not extract a fluent chain from '{0}'. Only fluent-style return pipelines are supported. Diagram not generated.",
            category: "REslava.Result.Flow",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor REF003 = new DiagnosticDescriptor(
            id: "REF003",
            title: "resultflow.json parse error",
            messageFormat: "resultflow.json could not be parsed: {0}. Falling back to the built-in convention dictionary.",
            category: "REslava.Result.Flow",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
#pragma warning restore RS2008

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Stage 1: Always emit [ResultFlow] attribute (available immediately to user code)
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource("ResultFlowAttribute.g.cs", ResultFlowAttributeGenerator.GenerateAttribute());
            });

            // Stage 2: Find method declarations decorated with [ResultFlow]
            // Also read MaxDepth from the attribute args at the syntax level.
            var annotatedMethods = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is MethodDeclarationSyntax m &&
                        m.AttributeLists.SelectMany(al => al.Attributes)
                            .Any(a => a.Name.ToString().Contains(AttributeShortName)),
                    transform: (ctx, _) =>
                    {
                        var method = (MethodDeclarationSyntax)ctx.Node;
                        var maxDepth = 2; // default
                        var attr = method.AttributeLists
                            .SelectMany(al => al.Attributes)
                            .FirstOrDefault(a => a.Name.ToString().Contains(AttributeShortName));
                        if (attr?.ArgumentList != null)
                        {
                            foreach (var arg in attr.ArgumentList.Arguments)
                            {
                                if (arg.NameEquals?.Name.Identifier.ValueText == "MaxDepth" &&
                                    arg.Expression is LiteralExpressionSyntax lit &&
                                    int.TryParse(lit.Token.ValueText, out var d))
                                    maxDepth = d;
                            }
                        }
                        return (Method: method, MaxDepth: maxDepth);
                    })
                .Where(t => t.Method != null);

            // Stage 2b: Pick up resultflow.json if present in AdditionalFiles
            var configFile = context.AdditionalTextsProvider
                .Where(f => string.Equals(Path.GetFileName(f.Path), ConfigFileName, System.StringComparison.OrdinalIgnoreCase))
                .Collect();

            var compilationAndMethods = context.CompilationProvider.Combine(annotatedMethods.Collect());
            var withConfig = compilationAndMethods.Combine(configFile);

            // Stage 3: Group by containing class, extract chain, render Mermaid, emit constants
            context.RegisterSourceOutput(withConfig, (spc, source) =>
            {
                var methods = source.Left.Right;
                if (!methods.Any()) return;


                // Load custom mappings and linkMode from resultflow.json (if present)
                IReadOnlyDictionary<string, NodeKind>? customMappings = null;
                string linkMode = string.Empty;
                var configAdditionalText = source.Right.FirstOrDefault();
                if (configAdditionalText != null)
                {
                    var configText = configAdditionalText.GetText()?.ToString();
                    if (configText != null)
                    {
                        customMappings = ResultFlowConfigLoader.TryLoad(configText, out var loadError, out var configLinkMode);
                        if (loadError != null)
                        {
                            spc.ReportDiagnostic(Diagnostic.Create(REF003, Location.None, loadError));
                            // customMappings is null here — fallback to convention dictionary
                        }
                        linkMode = configLinkMode ?? string.Empty;
                    }
                }

                var compilation = source.Left.Left;

                foreach (var group in methods.GroupBy(t => t.Method.Parent))
                {
                    // Both ClassDeclarationSyntax and RecordDeclarationSyntax inherit TypeDeclarationSyntax
                    if (!(group.Key is TypeDeclarationSyntax typeDecl)) continue;

                    var className = typeDecl.Identifier.ValueText;
                    var diagrams = new List<(string methodName, string mermaid, string? layerView, string? stats, string? errorSurface)>();

                    foreach (var (methodDecl, maxDepth) in group)
                    {
                        var semanticModel = compilation.GetSemanticModel(methodDecl.SyntaxTree);
                        var chain = ResultFlowChainExtractor.Extract(
                            methodDecl, semanticModel, customMappings,
                            maxDepth: maxDepth, compilation: compilation);
                        if (chain == null)
                        {
                            spc.ReportDiagnostic(Diagnostic.Create(
                                REF001,
                                methodDecl.Identifier.GetLocation(),
                                methodDecl.Identifier.ValueText));
                            continue;
                        }

                        var mermaid = ResultFlowMermaidRenderer.Render(chain, linkMode);

                        // Detect root method layer for LayerView / Stats
                        var containingNs = ResultFlowChainExtractor.GetContainingNamespace(methodDecl);
                        var rootLayer = LayerDetector.Detect(methodDecl, containingNs);

                        var methodName = methodDecl.Identifier.ValueText;
                        var layerView = ResultFlowLayerViewRenderer.Render(chain, methodName, className, rootLayer, linkMode: linkMode);
                        var stats = layerView != null ? ResultFlowStatsRenderer.Render(chain, rootLayer) : null;
                        var errorSurface = layerView != null ? ResultFlowErrorSurfaceRenderer.Render(chain) : null;

                        diagrams.Add((methodName, mermaid, layerView, stats, errorSurface));
                    }

                    if (diagrams.Count > 0)
                    {
                        var code = ResultFlowCodeGenerator.Generate(className, diagrams);
                        spc.AddSource($"{className}_Flows.g.cs", code);
                    }
                }
            });
        }
    }
}
