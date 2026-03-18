using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.Flow.Core.Interfaces;
using REslava.Result.Flow.Generators.ResultFlow.Attributes;
using REslava.Result.Flow.Generators.ResultFlow.CodeGeneration;
using REslava.Result.Flow.Generators.ResultFlow.Models;
using System.Collections.Generic;
using System.Linq;

namespace REslava.Result.Flow.Generators.ResultFlow.Orchestration
{
    internal class ResultFlowOrchestrator : IGeneratorOrchestrator
    {
        private const string AttributeShortName = "ResultFlow";

#pragma warning disable RS2008
        private static readonly DiagnosticDescriptor REF001 = new DiagnosticDescriptor(
            id: "REF001",
            title: "ResultFlow chain not detected",
            messageFormat: "[ResultFlow] could not extract a fluent chain from '{0}'. Only fluent-style return pipelines are supported. Diagram not generated.",
            category: "REslava.Result.Flow",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true);
#pragma warning restore RS2008

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Stage 1: emit [ResultFlow] attribute
            context.RegisterPostInitializationOutput(ctx =>
                ctx.AddSource("ResultFlowAttribute.g.cs", ResultFlowAttributeGenerator.GenerateAttribute()));

            // Stage 2: find [ResultFlow]-decorated methods (syntax only — cheap)
            // Also read MaxDepth from the attribute args at the syntax level (no semantic model needed).
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

            // Stage 3: resolve IResultBase and IError once per compilation
            var compilationWithSymbols = context.CompilationProvider.Select((compilation, _) =>
            {
                var resultBase = compilation.GetTypeByMetadataName("REslava.Result.IResultBase");
                var iError = compilation.GetTypeByMetadataName("REslava.Result.IError");
                return (Compilation: compilation, ResultBase: resultBase, IError: iError);
            });

            // Stage 3b: read ResultFlowLinkMode build property ("vscode" | "github" | "none" | "")
            var linkModeProvider = context.AnalyzerConfigOptionsProvider.Select((options, _) =>
            {
                options.GlobalOptions.TryGetValue("build_property.ResultFlowLinkMode", out var mode);
                return (mode ?? string.Empty).Trim().ToLowerInvariant();
            });

            // Stage 4: combine
            var combined = compilationWithSymbols.Combine(annotatedMethods.Collect()).Combine(linkModeProvider);

            // Stage 5: generate
            context.RegisterSourceOutput(combined, (spc, source) =>
            {
                var ((compWithSymbols, methods), linkMode) = source;
                if (!methods.Any()) return;

                var compilation = compWithSymbols.Compilation;
                var resultBaseSymbol = compWithSymbols.ResultBase;
                var iErrorSymbol = compWithSymbols.IError;

                foreach (var group in methods.GroupBy(t => t.Method.Parent))
                {
                    if (!(group.Key is TypeDeclarationSyntax typeDecl)) continue;

                    var className = typeDecl.Identifier.ValueText;
                    var diagrams = new List<(string methodName, string mermaid, string? layerView, string? stats, string? errorSurface, string? errorPropagation)>();

                    foreach (var (methodDecl, maxDepth) in group)
                    {
                        var semanticModel = compilation.GetSemanticModel(methodDecl.SyntaxTree);

                        var chain = ResultFlowChainExtractor.Extract(
                            methodDecl,
                            semanticModel,
                            compilation,
                            resultBaseSymbol,
                            iErrorSymbol,
                            maxDepth: maxDepth);

                        if (chain == null)
                        {
                            spc.ReportDiagnostic(Diagnostic.Create(
                                REF001,
                                methodDecl.Identifier.GetLocation(),
                                methodDecl.Identifier.ValueText));
                            continue;
                        }

                        var (opName, corrId) = ResultFlowChainExtractor.TryExtractContextHints(methodDecl);
                        var methodName = methodDecl.Identifier.ValueText;
                        var seedMethodName = ResultFlowChainExtractor.TryGetSeedMethodName(methodDecl);
                        var mermaid = ResultFlowMermaidRenderer.Render(chain, methodTitle: methodName, seedMethodName: seedMethodName, operationName: opName, correlationId: corrId, linkMode: linkMode);

                        // Detect root method layer for LayerView / Stats
                        string? rootLayer = null;
                        var rootSymbol = semanticModel.GetDeclaredSymbol(methodDecl) as Microsoft.CodeAnalysis.IMethodSymbol;
                        if (rootSymbol != null)
                            rootLayer = LayerDetector.Detect(rootSymbol);
                        var layerView = ResultFlowLayerViewRenderer.Render(chain, methodName, className, rootLayer, opName, linkMode);
                        var stats = layerView != null ? ResultFlowStatsRenderer.Render(chain, rootLayer) : null;
                        var errorSurface = layerView != null ? ResultFlowErrorSurfaceRenderer.Render(chain) : null;
                        var errorPropagation = layerView != null ? ResultFlowErrorPropagationRenderer.Render(chain, rootLayer) : null;

                        diagrams.Add((methodName, mermaid, layerView, stats, errorSurface, errorPropagation));
                    }

                    if (diagrams.Count > 0)
                        spc.AddSource($"{className}_Flows.g.cs", ResultFlowCodeGenerator.Generate(className, diagrams));
                }
            });
        }
    }
}
