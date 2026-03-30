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
                        var darkTheme = false;
                        var themeExplicitlySet = false;
                        if (attr?.ArgumentList != null)
                        {
                            foreach (var arg in attr.ArgumentList.Arguments)
                            {
                                if (arg.NameEquals?.Name.Identifier.ValueText == "MaxDepth" &&
                                    arg.Expression is LiteralExpressionSyntax lit &&
                                    int.TryParse(lit.Token.ValueText, out var d))
                                    maxDepth = d;

                                if (arg.NameEquals?.Name.Identifier.ValueText == "Theme" &&
                                    arg.Expression is MemberAccessExpressionSyntax mem &&
                                    mem.Name.Identifier.ValueText == "Dark")
                                {
                                    darkTheme = true;
                                    themeExplicitlySet = true;
                                }
                            }
                        }
                        return (Method: method, MaxDepth: maxDepth, DarkTheme: darkTheme, ThemeExplicitlySet: themeExplicitlySet);
                    })
                .Where(t => t.Method != null);

            // Stage 3: resolve IResultBase and IError once per compilation
            var compilationWithSymbols = context.CompilationProvider.Select((compilation, _) =>
            {
                var resultBase = compilation.GetTypeByMetadataName("REslava.Result.IResultBase");
                var iError = compilation.GetTypeByMetadataName("REslava.Result.IError");
                return (Compilation: compilation, ResultBase: resultBase, IError: iError);
            });

            // Stage 3b: read build properties (LinkMode + DefaultTheme)
            var buildPropsProvider = context.AnalyzerConfigOptionsProvider.Select((options, _) =>
            {
                options.GlobalOptions.TryGetValue("build_property.ResultFlowLinkMode", out var mode);
                options.GlobalOptions.TryGetValue("build_property.ResultFlowDefaultTheme", out var theme);
                var defaultDark = string.Equals(theme?.Trim(), "Dark", System.StringComparison.OrdinalIgnoreCase);
                return (LinkMode: (mode ?? string.Empty).Trim().ToLowerInvariant(), DefaultDarkTheme: defaultDark);
            });

            // Stage 4: combine
            var combined = compilationWithSymbols.Combine(annotatedMethods.Collect()).Combine(buildPropsProvider);

            // Stage 5: generate
            context.RegisterSourceOutput(combined, (spc, source) =>
            {
                var ((compWithSymbols, methods), buildProps) = source;
                var linkMode = buildProps.LinkMode;
                var defaultDarkTheme = buildProps.DefaultDarkTheme;
                if (!methods.Any()) return;

                var compilation = compWithSymbols.Compilation;
                var resultBaseSymbol = compWithSymbols.ResultBase;
                var iErrorSymbol = compWithSymbols.IError;

                foreach (var group in methods.GroupBy(t => t.Method.Parent))
                {
                    if (!(group.Key is TypeDeclarationSyntax typeDecl)) continue;

                    var className = typeDecl.Identifier.ValueText;
                    var diagrams = new List<(string methodName, string mermaid, string? layerView, string? stats, string? errorSurface, string? errorPropagation, string? typeFlow)>();
                    var tracedMethods = new List<CodeGeneration.TracedMethodInfo>();

                    foreach (var (methodDecl, maxDepth, darkTheme, themeExplicitlySet) in group)
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

                        var effectiveDarkTheme = themeExplicitlySet ? darkTheme : defaultDarkTheme;
                        var (opName, corrId) = ResultFlowChainExtractor.TryExtractContextHints(methodDecl);
                        var methodName = methodDecl.Identifier.ValueText;
                        var seedMethodName = ResultFlowChainExtractor.TryGetSeedMethodName(methodDecl);

                        // Entry source location — the [ResultFlow] method declaration itself
                        var entrySpan = methodDecl.GetLocation().GetLineSpan();
                        var entrySourceFile = entrySpan.Path;
                        var entrySourceLine = entrySpan.StartLinePosition.Line + 1; // 1-based for vscode://

                        // Compute pipelineId so diagram node IDs match registry _Info nodeIds
                        string? rootLayer = null;
                        var rootSymbol = semanticModel.GetDeclaredSymbol(methodDecl) as Microsoft.CodeAnalysis.IMethodSymbol;
                        var pipelineId = rootSymbol != null
                            ? ShortHash.Compute(
                                rootSymbol.ContainingType.ToDisplayString(),
                                rootSymbol.ContainingAssembly?.Name ?? "",
                                rootSymbol.Name,
                                string.Join(",", rootSymbol.Parameters.Select(p => p.Type.ToDisplayString())))
                            : ShortHash.Compute(className, methodName);

                        var mermaid = ResultFlowMermaidRenderer.Render(chain, methodTitle: methodName, seedMethodName: seedMethodName, operationName: opName, correlationId: corrId, linkMode: linkMode, darkTheme: effectiveDarkTheme, entrySourceFile: entrySourceFile, entrySourceLine: entrySourceLine, pipelineId: pipelineId);
                        var typeFlow = ResultFlowMermaidRenderer.Render(chain, methodTitle: methodName, seedMethodName: seedMethodName, operationName: opName, correlationId: corrId, linkMode: linkMode, darkTheme: effectiveDarkTheme, entrySourceFile: entrySourceFile, entrySourceLine: entrySourceLine, pipelineId: pipelineId, typeLabels: true);

                        // Detect root method layer for LayerView / Stats
                        if (rootSymbol != null)
                            rootLayer = LayerDetector.Detect(rootSymbol);
                        var layerView = ResultFlowLayerViewRenderer.Render(chain, methodName, className, rootLayer, opName, linkMode, darkTheme: effectiveDarkTheme, pipelineId: pipelineId);
                        var stats = layerView != null ? ResultFlowStatsRenderer.Render(chain, rootLayer, pipelineId: pipelineId) : null;
                        var errorSurface = layerView != null ? ResultFlowErrorSurfaceRenderer.Render(chain, darkTheme: effectiveDarkTheme, pipelineId: pipelineId) : null;
                        var errorPropagation = layerView != null ? ResultFlowErrorPropagationRenderer.Render(chain, rootLayer, darkTheme: effectiveDarkTheme, pipelineId: pipelineId) : null;

                        diagrams.Add((methodName, mermaid, layerView, stats, errorSurface, errorPropagation, typeFlow));

                        // ── Collect TracedMethodInfo for _Traced extension ────
                        if (rootSymbol != null && !rootSymbol.IsStatic)
                        {
                            var fmt = Microsoft.CodeAnalysis.SymbolDisplayFormat.FullyQualifiedFormat;
                            var returnType = rootSymbol.ReturnType as Microsoft.CodeAnalysis.INamedTypeSymbol;
                            var returnsTask = returnType != null && returnType.Name == "Task"
                                && returnType.ContainingNamespace?.ToDisplayString() == "System.Threading.Tasks";
                            Microsoft.CodeAnalysis.INamedTypeSymbol? innerType = returnsTask
                                && returnType!.TypeArguments.Length > 0
                                    ? returnType.TypeArguments[0] as Microsoft.CodeAnalysis.INamedTypeSymbol
                                    : returnType;
                            var resultIsGeneric = innerType?.Arity > 0;

                            // nodeIds: one entry per non-Invisible chain node that has source info
                            var nodeIds = new System.Collections.Generic.List<string>();
                            foreach (var node in chain)
                            {
                                if (node.Kind == Models.NodeKind.Invisible) continue;
                                if (node.SourceFile != null && node.SourceLine.HasValue)
                                    nodeIds.Add($"{System.IO.Path.GetFileName(node.SourceFile)}:{node.SourceLine}");
                                else
                                    nodeIds.Add($"{pipelineId}:{nodeIds.Count}");
                            }

                            var parameters = new System.Collections.Generic.List<(string TypeFqn, string ParamName, bool IsValueType)>();
                            foreach (var p in rootSymbol.Parameters)
                                parameters.Add((p.Type.ToDisplayString(fmt), p.Name, p.Type.IsValueType));

                            tracedMethods.Add(new CodeGeneration.TracedMethodInfo
                            {
                                MethodName = methodName,
                                ContainingTypeFqn = rootSymbol.ContainingType.ToDisplayString(fmt),
                                ReturnTypeFqn = rootSymbol.ReturnType.ToDisplayString(fmt),
                                ReturnsTask = returnsTask,
                                ResultIsGeneric = resultIsGeneric,
                                Parameters = parameters,
                                PipelineId = pipelineId,
                                NodeIds = nodeIds.ToArray(),
                            });
                        }
                    }

                    if (diagrams.Count > 0)
                        spc.AddSource($"{className}_Flows.g.cs", ResultFlowCodeGenerator.Generate(className, diagrams, tracedMethods));
                }
            });
        }
    }
}
