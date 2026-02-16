using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using REslava.Result.Analyzers.Helpers;

namespace REslava.Result.Analyzers.Analyzers
{
    /// <summary>
    /// RESL1001: Warns when Result&lt;T&gt;.Value is accessed without checking IsSuccess first.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnsafeValueAccessAnalyzer : DiagnosticAnalyzer
    {
        private static readonly GuardDetectionHelper.GuardConfig ResultGuardConfig = new(
            positiveProperties: new[] { "IsSuccess" },
            negativeProperties: new[] { "IsFailed" });

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Descriptors.RESL1001_UnsafeValueAccess);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var resultType = compilationContext.Compilation
                    .GetTypeByMetadataName("REslava.Result.Result`1");

                if (resultType is null)
                    return;

                compilationContext.RegisterSyntaxNodeAction(
                    ctx => AnalyzeMemberAccess(ctx, resultType),
                    SyntaxKind.SimpleMemberAccessExpression);
            });
        }

        private static void AnalyzeMemberAccess(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol resultType)
        {
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;

            // Quick syntax check: is this ".Value"?
            if (memberAccess.Name.Identifier.Text != "Value")
                return;

            // Get the type of the expression before .Value
            var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken);
            var expressionType = typeInfo.Type as INamedTypeSymbol;

            if (expressionType is null)
                return;

            // Is this Result<T>.Value?
            if (!IsResultType(expressionType, resultType))
                return;

            // Check if .Value is guarded by IsSuccess/IsFailed
            if (GuardDetectionHelper.IsGuardedByCheck(memberAccess, ResultGuardConfig))
                return;

            context.ReportDiagnostic(
                Diagnostic.Create(
                    Descriptors.RESL1001_UnsafeValueAccess,
                    memberAccess.Name.GetLocation()));
        }

        private static bool IsResultType(INamedTypeSymbol type, INamedTypeSymbol resultType)
        {
            if (!type.IsGenericType)
                return false;

            return SymbolEqualityComparer.Default.Equals(
                type.OriginalDefinition, resultType);
        }
    }
}
