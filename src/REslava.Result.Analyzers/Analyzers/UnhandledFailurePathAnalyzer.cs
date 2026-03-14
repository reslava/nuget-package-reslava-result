using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using REslava.Result.Analyzers.Helpers;

namespace REslava.Result.Analyzers.Analyzers
{
    /// <summary>
    /// RESL1010 (Phase 2 — lexical scan): warns when a local Result&lt;T&gt; or Result&lt;T,TError&gt;
    /// variable has no failure-aware usage in the enclosing block and is not returned.
    ///
    /// Phase 2 is intentionally permissive: any access to a member in <see cref="HandledMemberNames"/>
    /// suppresses the diagnostic. The "IsSuccess without else" case (a Phase 3 refinement) is a
    /// known false-negative that will be addressed in a future iteration.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnhandledFailurePathAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Member names that indicate the caller is aware of the failure path.
        /// Any access to one of these on the result variable suppresses RESL1010.
        /// </summary>
        private static readonly ImmutableHashSet<string> HandledMemberNames = ImmutableHashSet.Create(
            // Explicit failure checks
            "IsFailure",
            // Explicit success awareness (counted as handled in Phase 2 — Phase 3 will refine IsSuccess-without-else)
            "IsSuccess",
            // Terminal consumers — handle both branches
            "Match", "MatchAsync",
            "Switch", "SwitchAsync",
            // Failure-specific side-effects
            "TapOnFailure", "TapOnFailureAsync",
            // Pipeline propagation — failure flows through the chain
            "Bind", "BindAsync",
            "Map", "MapAsync",
            "Ensure", "EnsureAsync",
            "Tap", "TapAsync",
            "TapBoth", "TapBothAsync",
            "Or", "OrElse", "OrElseAsync",
            "MapError", "MapErrorAsync",
            // Safe value extraction — acknowledges failure by providing fallback
            "GetValueOr", "TryGetValue"
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Descriptors.RESL1010_UnhandledFailurePath);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var resultT = compilationContext.Compilation
                    .GetTypeByMetadataName("REslava.Result.Result`1");
                var resultTError = compilationContext.Compilation
                    .GetTypeByMetadataName("REslava.Result.Result`2");

                if (resultT == null && resultTError == null)
                    return;

                var resultTypes = new[] { resultT, resultTError }
                    .Where(t => t != null)
                    .ToArray();

                compilationContext.RegisterSyntaxNodeAction(
                    ctx => AnalyzeLocalDeclaration(ctx, resultTypes),
                    SyntaxKind.LocalDeclarationStatement);
            });
        }

        private static void AnalyzeLocalDeclaration(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol[] resultTypes)
        {
            var localDecl = (LocalDeclarationStatementSyntax)context.Node;

            // Must be inside a block (method body, not expression-bodied)
            if (localDecl.Parent is not BlockSyntax block)
                return;

            foreach (var variable in localDecl.Declaration.Variables)
            {
                // Resolve the declared type via the local symbol
                var localSymbol = context.SemanticModel
                    .GetDeclaredSymbol(variable, context.CancellationToken) as ILocalSymbol;

                if (localSymbol?.Type is not INamedTypeSymbol varType || !varType.IsGenericType)
                    continue;

                // Only act on Result<T> or Result<T, TError>
                if (!resultTypes.Any(rt =>
                    SymbolEqualityComparer.Default.Equals(varType.OriginalDefinition, rt)))
                    continue;

                var variableName = variable.Identifier.ValueText;

                // Collect block statements that come AFTER this declaration
                var subsequent = GetSubsequentStatements(block, localDecl);

                if (!IsHandled(variableName, subsequent))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.RESL1010_UnhandledFailurePath,
                            variable.Identifier.GetLocation(),
                            variableName));
                }
            }
        }

        /// <summary>
        /// Returns all statements in <paramref name="block"/> that appear after
        /// <paramref name="declaration"/>.
        /// </summary>
        private static IEnumerable<StatementSyntax> GetSubsequentStatements(
            BlockSyntax block,
            LocalDeclarationStatementSyntax declaration)
        {
            bool found = false;
            foreach (var stmt in block.Statements)
            {
                if (found) yield return stmt;
                if (stmt == declaration) found = true;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if any of the subsequent statements indicate that
        /// <paramref name="variableName"/> is failure-handled (via a member in
        /// <see cref="HandledMemberNames"/>) or is returned.
        /// </summary>
        private static bool IsHandled(
            string variableName,
            IEnumerable<StatementSyntax> subsequentStatements)
        {
            foreach (var stmt in subsequentStatements)
            {
                // Explicit return of the variable — failure propagates to caller
                if (stmt is ReturnStatementSyntax returnStmt &&
                    returnStmt.Expression is IdentifierNameSyntax retId &&
                    retId.Identifier.ValueText == variableName)
                    return true;

                // Any member access on the variable whose name is in the handled set
                foreach (var memberAccess in stmt.DescendantNodesAndSelf()
                    .OfType<MemberAccessExpressionSyntax>())
                {
                    var receiverText = GuardDetectionHelper
                        .GetExpressionIdentifier(memberAccess.Expression);

                    if (receiverText == variableName &&
                        HandledMemberNames.Contains(memberAccess.Name.Identifier.Text))
                        return true;
                }
            }

            return false;
        }
    }
}
