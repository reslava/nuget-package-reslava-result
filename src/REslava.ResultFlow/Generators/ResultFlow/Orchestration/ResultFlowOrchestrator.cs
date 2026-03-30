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

            // Stage 2b: Pick up resultflow.json if present in AdditionalFiles
            var configFile = context.AdditionalTextsProvider
                .Where(f => string.Equals(Path.GetFileName(f.Path), ConfigFileName, System.StringComparison.OrdinalIgnoreCase))
                .Collect();

            // Stage 2c: read build properties (DefaultTheme + LinkMode)
            var buildPropsProvider = context.AnalyzerConfigOptionsProvider.Select((options, _) =>
            {
                options.GlobalOptions.TryGetValue("build_property.ResultFlowDefaultTheme", out var theme);
                options.GlobalOptions.TryGetValue("build_property.ResultFlowLinkMode", out var mode);
                var defaultDark = string.Equals(theme?.Trim(), "Dark", System.StringComparison.OrdinalIgnoreCase);
                var linkModeFromProps = (mode ?? string.Empty).Trim().ToLowerInvariant();
                return (DefaultDarkTheme: defaultDark, LinkModeFromProps: linkModeFromProps);
            });

            var compilationAndMethods = context.CompilationProvider.Combine(annotatedMethods.Collect());
            var withConfig = compilationAndMethods.Combine(configFile).Combine(buildPropsProvider);

            // Stage 3: Group by containing class, extract chain, render Mermaid, emit constants
            context.RegisterSourceOutput(withConfig, (spc, source) =>
            {
                var (defaultDarkTheme, linkModeFromProps) = source.Right;
                var methods = source.Left.Left.Right;
                if (!methods.Any()) return;

                // Load custom mappings and linkMode from resultflow.json (if present).
                // JSON config linkMode takes precedence over the MSBuild property.
                IReadOnlyDictionary<string, NodeKind>? customMappings = null;
                string linkMode = string.Empty;
                var configAdditionalText = source.Left.Right.FirstOrDefault();
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
                // Fall back to MSBuild property when JSON config does not specify linkMode
                if (string.IsNullOrEmpty(linkMode))
                    linkMode = linkModeFromProps;

                var compilation = source.Left.Left.Left;

                foreach (var group in methods.GroupBy(t => t.Method.Parent))
                {
                    // Both ClassDeclarationSyntax and RecordDeclarationSyntax inherit TypeDeclarationSyntax
                    if (!(group.Key is TypeDeclarationSyntax typeDecl)) continue;

                    var className = typeDecl.Identifier.ValueText;
                    var diagrams = new List<(string methodName, string mermaid, string? layerView, string? stats, string? errorSurface, string? typeFlow)>();
                    var tracedMethods = new List<CodeGeneration.TracedMethodInfo>();

                    foreach (var (methodDecl, maxDepth, darkTheme, themeExplicitlySet) in group)
                    {
                        var effectiveDarkTheme = themeExplicitlySet ? darkTheme : defaultDarkTheme;
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

                        var methodName = methodDecl.Identifier.ValueText;
                        var seedMethodName = ResultFlowChainExtractor.TryGetSeedMethodName(methodDecl);

                        // Entry source location — the [ResultFlow] method declaration itself
                        var entrySpan = methodDecl.GetLocation().GetLineSpan();
                        var entrySourceFile = entrySpan.Path;
                        var entrySourceLine = entrySpan.StartLinePosition.Line + 1; // 1-based for vscode://

                        // Compute pipelineId (syntax-only) so diagram node IDs match registry _Info nodeIds
                        var containingNs = ResultFlowChainExtractor.GetContainingNamespace(methodDecl);
                        var pipelineId = ShortHash.Compute(
                            containingNs,
                            className,
                            methodName,
                            string.Join(",", methodDecl.ParameterList.Parameters.Select(p => p.Type?.ToString() ?? "")));

                        var mermaid = ResultFlowMermaidRenderer.Render(chain, methodTitle: methodName, seedMethodName: seedMethodName, linkMode: linkMode, darkTheme: effectiveDarkTheme, entrySourceFile: entrySourceFile, entrySourceLine: entrySourceLine, pipelineId: pipelineId);
                        var typeFlow = ResultFlowMermaidRenderer.Render(chain, methodTitle: methodName, seedMethodName: seedMethodName, linkMode: linkMode, darkTheme: effectiveDarkTheme, entrySourceFile: entrySourceFile, entrySourceLine: entrySourceLine, pipelineId: pipelineId, typeLabels: true);

                        // Detect root method layer for LayerView / Stats
                        var rootLayer = LayerDetector.Detect(methodDecl, containingNs);
                        var layerView = ResultFlowLayerViewRenderer.Render(chain, methodName, className, rootLayer, linkMode: linkMode, darkTheme: effectiveDarkTheme, pipelineId: pipelineId);
                        var stats = layerView != null ? ResultFlowStatsRenderer.Render(chain, rootLayer, pipelineId: pipelineId) : null;
                        var errorSurface = layerView != null ? ResultFlowErrorSurfaceRenderer.Render(chain, darkTheme: effectiveDarkTheme, pipelineId: pipelineId) : null;

                        diagrams.Add((methodName, mermaid, layerView, stats, errorSurface, typeFlow));

                        // ── Collect TracedMethodInfo for _Traced extension ────
                        // Skip static methods — extension method requires an instance receiver.
                        var isStatic = methodDecl.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword));
                        if (!isStatic)
                        {
                            var returnTypeSyntax = methodDecl.ReturnType.ToFullString().Trim();
                            var returnsTask = returnTypeSyntax.StartsWith("Task<") || returnTypeSyntax == "Task";
                            // Unwrap Task<...> to check inner type
                            var innerTypeSyntax = returnsTask && returnTypeSyntax.StartsWith("Task<") && returnTypeSyntax.EndsWith(">")
                                ? returnTypeSyntax.Substring(5, returnTypeSyntax.Length - 6).Trim()
                                : returnTypeSyntax;
                            var resultIsGeneric = innerTypeSyntax.Contains("<");

                            var nodeIds = new System.Collections.Generic.List<string>();
                            foreach (var node in chain)
                            {
                                if (node.Kind == Models.NodeKind.Invisible) continue;
                                if (node.SourceFile != null && node.SourceLine.HasValue)
                                    nodeIds.Add($"{System.IO.Path.GetFileName(node.SourceFile)}:{node.SourceLine}");
                                else
                                    nodeIds.Add($"{pipelineId}:{nodeIds.Count}");
                            }

                            var parameters = new System.Collections.Generic.List<(string TypeSyntax, string ParamName, bool IsValueType)>();
                            foreach (var p in methodDecl.ParameterList.Parameters)
                            {
                                var typeSyntax = p.Type?.ToFullString().Trim() ?? "object";
                                parameters.Add((typeSyntax, p.Identifier.ValueText, IsKnownValueType(typeSyntax)));
                            }

                            tracedMethods.Add(new CodeGeneration.TracedMethodInfo
                            {
                                MethodName = methodName,
                                ContainingNamespace = containingNs,
                                ContainingTypeName = className,
                                ReturnTypeSyntax = returnTypeSyntax,
                                ReturnsTask = returnsTask,
                                ResultIsGeneric = resultIsGeneric,
                                Parameters = parameters,
                                PipelineId = pipelineId,
                                NodeIds = nodeIds.ToArray(),
                            });
                        }
                    }

                    if (diagrams.Count > 0)
                    {
                        var code = ResultFlowCodeGenerator.Generate(className, diagrams, tracedMethods);
                        spc.AddSource($"{className}_Flows.g.cs", code);
                    }
                }
            });
        }

        private static readonly System.Collections.Generic.HashSet<string> _valueTypeKeywords =
            new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal)
            {
                "bool", "byte", "char", "decimal", "double", "float", "int", "long",
                "sbyte", "short", "uint", "ulong", "ushort",
                "System.Boolean", "System.Byte", "System.Char", "System.Decimal",
                "System.Double", "System.Single", "System.Int32", "System.Int64",
                "System.SByte", "System.Int16", "System.UInt32", "System.UInt64", "System.UInt16",
                "global::System.Boolean", "global::System.Int32", "global::System.Int64",
            };

        private static bool IsKnownValueType(string typeSyntax)
        {
            // Strip trailing '?' (nullable value types are still value types for ToString purposes)
            var t = typeSyntax.TrimEnd('?').Trim();
            return _valueTypeKeywords.Contains(t);
        }
    }
}
