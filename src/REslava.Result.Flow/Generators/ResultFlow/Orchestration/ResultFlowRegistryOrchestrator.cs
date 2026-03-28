using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.Flow.Core.Interfaces;
using REslava.Result.Flow.Generators.ResultFlow.CodeGeneration;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace REslava.Result.Flow.Generators.ResultFlow.Orchestration
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

            // Stage 2: collect all method declarations (cheap syntax predicate, semantic in transform)
            var methodsProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (n, _) => n is MethodDeclarationSyntax,
                    transform: GetMethodModelIfResultBase)
                .Where(m => m != null);

            // Stage 3: resolve IResultBase/IError once per compilation
            var compilationProvider = context.CompilationProvider.Select((compilation, _) =>
            {
                var resultBase = compilation.GetTypeByMetadataName("REslava.Result.IResultBase");
                var iError     = compilation.GetTypeByMetadataName("REslava.Result.IError");
                return (Compilation: compilation, ResultBase: resultBase, IError: iError);
            });

            // Stage 4: combine
            var combined = methodsProvider.Collect().Combine(enabledProvider).Combine(compilationProvider);

            // Stage 5: generate
            context.RegisterSourceOutput(combined, (spc, source) =>
            {
                var ((methods, enabled), compSymbols) = source;
                if (!enabled) return;
                if (methods.IsEmpty) return;

                var compilation  = compSymbols.Compilation;
                var resultBase   = compSymbols.ResultBase;
                var iError       = compSymbols.IError;

                var byClass = methods.GroupBy(m => m!.ClassName);
                foreach (var group in byClass)
                {
                    var className = group.Key;
                    var list = group.ToList();
                    spc.AddSource(
                        $"{className}_PipelineRegistry.g.cs",
                        ResultFlowRegistryCodeGenerator.Generate(className, list!, compilation, resultBase, iError));
                }
            });
        }

        // ── Transform: collect a method model if its return type implements IResultBase
        //              OR the method has a [ResultFlow] attribute (e.g. Match-terminal pipelines
        //              where the declared return type is non-Result, like string or void). ──

        private static MethodRegistryModel? GetMethodModelIfResultBase(
            GeneratorSyntaxContext ctx,
            CancellationToken ct)
        {
            if (!(ctx.Node is MethodDeclarationSyntax method)) return null;

            var symbol = ctx.SemanticModel.GetDeclaredSymbol(method, ct) as IMethodSymbol;
            if (symbol == null) return null;

            // Check for [ResultFlow] attribute at syntax level early — needed for filter below
            var hasDiagram = method.AttributeLists.SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString().Contains(AttributeShortName));

            // Resolve IResultBase from compilation (cached internally by Roslyn)
            var resultBase = ctx.SemanticModel.Compilation
                .GetTypeByMetadataName("REslava.Result.IResultBase");
            if (resultBase == null) return null;

            // Unwrap Task<T> / ValueTask<T> before checking IResultBase
            var returnType = symbol.ReturnType;
            var isAsync = returnType is INamedTypeSymbol { Name: "Task" or "ValueTask" };
            if (returnType is INamedTypeSymbol { Name: "Task" or "ValueTask" } asyncType &&
                asyncType.TypeArguments.Length == 1)
                returnType = asyncType.TypeArguments[0];

            var implementsResultBase = ResultTypeExtractor.ImplementsInterface(returnType, resultBase);

            // Include if: return type is Result-based OR method has [ResultFlow] attribute
            // (e.g. ConfirmOrder returns string via .Match() but still has [ResultFlow])
            if (!implementsResultBase && !hasDiagram) return null;

            // Inner type T from Result<T>; use raw type name for non-Result returns; "Unit" for non-generic Result
            string innerType;
            if (implementsResultBase)
            {
                innerType = "Unit";
                if (returnType is INamedTypeSymbol namedReturn && namedReturn.TypeArguments.Length >= 1)
                    innerType = namedReturn.TypeArguments[0].Name;
            }
            else
            {
                // Non-Result return (e.g. string from Match-terminal) — use the type name directly
                innerType = returnType.Name;
            }

            // Source location — stored as 1-based so VSIX can do (sourceLine - 1) to get 0-based
            var span = method.GetLocation().GetLineSpan();
            var sourceLine = span.StartLinePosition.Line + 1;

            // Class name from syntax parent
            var className = (method.Parent as TypeDeclarationSyntax)?.Identifier.ValueText ?? "Unknown";

            // Namespace
            var ns = symbol.ContainingNamespace?.IsGlobalNamespace == true
                ? ""
                : (symbol.ContainingNamespace?.ToDisplayString() ?? "");

            // PipelineId — deterministic hash of fully-qualified method signature + assembly name
            var pipelineId = ShortHash.Compute(
                symbol.ContainingType.ToDisplayString(),
                symbol.ContainingAssembly?.Name ?? "",
                symbol.Name,
                string.Join(",", symbol.Parameters.Select(p => p.Type.ToDisplayString())));

            // MaxDepth from [ResultFlow] args
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
                MethodName         = symbol.Name,
                SourceLine         = sourceLine,
                ReturnType         = innerType,
                ReturnTypeFullName = symbol.ReturnType.ToDisplayString(),
                IsAsync            = isAsync,
                HasDiagram         = hasDiagram,
                Syntax             = method,
                MaxDepth           = maxDepth,
                PipelineId         = pipelineId,
                Namespace          = ns,
            };
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
