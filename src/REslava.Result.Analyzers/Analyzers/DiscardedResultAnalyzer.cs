using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace REslava.Result.Analyzers.Analyzers
{
    /// <summary>
    /// RESL1002: Warns when a method returning Result&lt;T&gt; or Task&lt;Result&lt;T&gt;&gt;
    /// is called and the return value is ignored.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DiscardedResultAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Descriptors.RESL1002_DiscardedResult);

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

                var taskType = compilationContext.Compilation
                    .GetTypeByMetadataName("System.Threading.Tasks.Task`1");

                compilationContext.RegisterSyntaxNodeAction(
                    ctx => AnalyzeExpressionStatement(ctx, resultType, taskType),
                    SyntaxKind.ExpressionStatement);
            });
        }

        private static void AnalyzeExpressionStatement(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol resultType,
            INamedTypeSymbol? taskType)
        {
            var expressionStatement = (ExpressionStatementSyntax)context.Node;
            var expression = expressionStatement.Expression;

            // Skip assignments (var x = Method())
            if (expression is AssignmentExpressionSyntax)
                return;

            // Get the return type of the expression
            var typeInfo = context.SemanticModel.GetTypeInfo(expression, context.CancellationToken);
            var returnType = typeInfo.Type as INamedTypeSymbol;

            if (returnType is null)
                return;

            // Check if it's Result<T>
            if (IsResultType(returnType, resultType))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.RESL1002_DiscardedResult,
                        expression.GetLocation()));
                return;
            }

            // Check if it's Task<Result<T>>
            if (taskType is not null
                && returnType.IsGenericType
                && SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, taskType)
                && returnType.TypeArguments.Length == 1
                && returnType.TypeArguments[0] is INamedTypeSymbol innerType
                && IsResultType(innerType, resultType))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.RESL1002_DiscardedResult,
                        expression.GetLocation()));
            }
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
