using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.ResultFlow.Core.Interfaces;
using REslava.ResultFlow.Generators.ResultFlow.CodeGeneration;
using System.Collections.Generic;
using System.Linq;

namespace REslava.ResultFlow.Generators.ResultFlow.Orchestration
{
    internal class ResultFlowRegistryOrchestrator : IGeneratorOrchestrator
    {
        private const string AttributeShortName = "ResultFlow";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Stage 1: read opt-out property
            var enabledProvider = context.AnalyzerConfigOptionsProvider.Select((o, _) =>
                !(o.GlobalOptions.TryGetValue("build_property.ResultFlowRegistry", out var v) &&
                  string.Equals(v?.Trim(), "false", System.StringComparison.OrdinalIgnoreCase)));

            // Stage 2: collect methods where return type mentions "Result" OR method has [ResultFlow]
            // (syntax-only — no IResultBase semantic check, library-agnostic)
            // The [ResultFlow] check catches Match-terminal pipelines that return non-Result types (e.g. string).
            var methodsProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (n, _) => n is MethodDeclarationSyntax m &&
                        (ReturnTypeMentionsResult(m.ReturnType) ||
                         m.AttributeLists.SelectMany(al => al.Attributes)
                             .Any(a => a.Name.ToString().Contains(AttributeShortName))),
                    transform: (ctx, _) => ExtractModel((MethodDeclarationSyntax)ctx.Node))
                .Where(m => m != null);

            // Stage 3: compilation for chain extractor (nodeCount)
            var compilationProvider = context.CompilationProvider;

            // Stage 4: combine
            var combined = methodsProvider.Collect().Combine(enabledProvider).Combine(compilationProvider);

            // Stage 5: generate
            context.RegisterSourceOutput(combined, (spc, source) =>
            {
                var ((methods, enabled), compilation) = source;
                if (!enabled) return;
                if (methods.IsEmpty) return;

                var byClass = methods.GroupBy(m => m!.ClassName);
                foreach (var group in byClass)
                {
                    var className = group.Key;
                    var list = group.ToList();
                    spc.AddSource(
                        $"{className}_PipelineRegistry.g.cs",
                        ResultFlowRegistryCodeGenerator.Generate(className, list!, compilation));
                }
            });
        }

        // ── Syntax-only heuristic: does the return type name contain "Result"? ──

        private static bool ReturnTypeMentionsResult(TypeSyntax typeSyntax)
        {
            switch (typeSyntax)
            {
                case IdentifierNameSyntax id:
                    return id.Identifier.ValueText.Contains("Result");
                case QualifiedNameSyntax qn:
                    return ReturnTypeMentionsResult(qn.Right);
                case GenericNameSyntax gen:
                    // Result<T> itself, or Task<Result<T>> / ValueTask<Result<T>>
                    if (gen.Identifier.ValueText.Contains("Result")) return true;
                    if (gen.Identifier.ValueText == "Task" || gen.Identifier.ValueText == "ValueTask")
                        return gen.TypeArgumentList.Arguments.Any(a => ReturnTypeMentionsResult(a));
                    return false;
                default:
                    return typeSyntax.ToString().Contains("Result");
            }
        }

        // ── Extract model at syntax level ──────────────────────────────────────

        private static MethodRegistryModel ExtractModel(MethodDeclarationSyntax method)
        {
            var className = (method.Parent as TypeDeclarationSyntax)?.Identifier.ValueText ?? "Unknown";
            // Stored as 1-based so VSIX can do (sourceLine - 1) to get the 0-based line
            var sourceLine = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            // Namespace — walk syntax parent chain
            var ns = GetNamespaceFromSyntax(method);

            // PipelineId — deterministic hash from syntax-level identity
            var pipelineId = ShortHash.Compute(
                ns,
                className,
                method.Identifier.ValueText,
                string.Join(",", method.ParameterList.Parameters.Select(p => p.Type?.ToString() ?? "")));

            // Detect async wrapper (Task/ValueTask)
            var isAsync = IsWrappedInAsync(method.ReturnType);

            // Best-effort inner type extraction from syntax
            var innerType = ExtractInnerTypeName(method.ReturnType);
            var fullName  = method.ReturnType.ToString();

            // [ResultFlow] attribute check
            var hasDiagram = method.AttributeLists.SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString().Contains(AttributeShortName));

            var maxDepth = 2;
            if (hasDiagram)
            {
                var attr = method.AttributeLists.SelectMany(al => al.Attributes)
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
            }

            return new MethodRegistryModel
            {
                ClassName          = className,
                MethodName         = method.Identifier.ValueText,
                SourceLine         = sourceLine,
                ReturnType         = innerType,
                ReturnTypeFullName = fullName,
                IsAsync            = isAsync,
                HasDiagram         = hasDiagram,
                Syntax             = method,
                MaxDepth           = maxDepth,
                PipelineId         = pipelineId,
                Namespace          = ns,
            };
        }

        private static string GetNamespaceFromSyntax(MethodDeclarationSyntax method)
        {
            var parts = new System.Collections.Generic.List<string>();
            var current = method.Parent?.Parent;
            while (current != null)
            {
                if (current is Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax ns)
                    parts.Insert(0, ns.Name.ToString());
                else if (current is Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax fns)
                    parts.Insert(0, fns.Name.ToString());
                current = current.Parent;
            }
            return string.Join(".", parts);
        }

        private static bool IsWrappedInAsync(TypeSyntax typeSyntax)
        {
            if (typeSyntax is GenericNameSyntax gen)
                return gen.Identifier.ValueText == "Task" || gen.Identifier.ValueText == "ValueTask";
            return false;
        }

        /// <summary>
        /// Extracts the inner type name from <c>Result&lt;T&gt;</c> (returns "T"),
        /// <c>Task&lt;Result&lt;T&gt;&gt;</c> (returns "T"), or plain <c>Result</c> (returns "Unit").
        /// </summary>
        private static string ExtractInnerTypeName(TypeSyntax typeSyntax)
        {
            // Unwrap Task/ValueTask first
            if (typeSyntax is GenericNameSyntax outer &&
                (outer.Identifier.ValueText == "Task" || outer.Identifier.ValueText == "ValueTask") &&
                outer.TypeArgumentList.Arguments.Count == 1)
                return ExtractInnerTypeName(outer.TypeArgumentList.Arguments[0]);

            // Result<T> → T
            if (typeSyntax is GenericNameSyntax result &&
                result.Identifier.ValueText.Contains("Result") &&
                result.TypeArgumentList.Arguments.Count == 1)
                return result.TypeArgumentList.Arguments[0].ToString();

            // Non-generic Result or non-Result return (e.g. string from Match-terminal) → type name
            return typeSyntax.ToString();
        }
    }

    // ── Data model ──────────────────────────────────────────────────────────────

    internal sealed class MethodRegistryModel
    {
        public string ClassName          { get; set; } = "";
        public string MethodName         { get; set; } = "";
        public int    SourceLine         { get; set; }
        public string ReturnType         { get; set; } = "Unit";
        public string ReturnTypeFullName { get; set; } = "";
        public bool   IsAsync            { get; set; }
        public bool   HasDiagram         { get; set; }
        public MethodDeclarationSyntax? Syntax { get; set; }
        public int    MaxDepth           { get; set; } = 2;
        public string PipelineId         { get; set; } = "";
        public string Namespace          { get; set; } = "";
    }
}
