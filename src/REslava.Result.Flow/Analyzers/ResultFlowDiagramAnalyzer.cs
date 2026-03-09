using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using REslava.Result.Flow.Generators.ResultFlow.CodeGeneration;
using System.Collections.Immutable;
using System.Linq;

namespace REslava.Result.Flow.Analyzers
{
    /// <summary>
    /// Emits REF002 (Info) for every <c>[ResultFlow]</c>-annotated method whose fluent chain
    /// can be extracted. The paired <see cref="ResultFlowInsertCommentFix"/> turns REF002 into
    /// a Code Action that inserts the Mermaid diagram (with full type travel and typed error
    /// edges) as a block comment above the method — no build required to view the diagram.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ResultFlowDiagramAnalyzer : DiagnosticAnalyzer
    {
        private const string AttributeShortName = "ResultFlow";

#pragma warning disable RS2008 // Enable analyzer release tracking
        internal static readonly DiagnosticDescriptor REF002 = new DiagnosticDescriptor(
            id: "REF002",
            title: "ResultFlow diagram ready",
            messageFormat: "[ResultFlow] diagram ready for '{0}' — use the 'Insert diagram as comment' code action to view it",
            category: "REslava.Result.Flow",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true);
#pragma warning restore RS2008

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(REF002);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;

            // Syntax-only check: look for an attribute whose name contains "ResultFlow"
            bool hasResultFlow = method.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString().Contains(AttributeShortName));

            if (!hasResultFlow) return;

            // Syntax-only chain detection — fast, no semantic model needed
            var chain = ResultFlowChainExtractor.ExtractSyntaxOnly(method);
            if (chain == null) return; // REF001 handles undetectable chains

            context.ReportDiagnostic(Diagnostic.Create(
                REF002,
                method.Identifier.GetLocation(),
                method.Identifier.ValueText));
        }
    }
}
