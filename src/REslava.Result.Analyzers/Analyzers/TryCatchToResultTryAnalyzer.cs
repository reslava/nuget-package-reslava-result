using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace REslava.Result.Analyzers.Analyzers
{
    /// <summary>
    /// RESL1009: Suggests replacing a try/catch block with Result&lt;T&gt;.Try()
    /// when the method returns Result&lt;T&gt; or Task&lt;Result&lt;T&gt;&gt;
    /// and the try body returns a plain T while the catch returns Result&lt;T&gt;.Fail(...).
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TryCatchToResultTryAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Descriptors.RESL1009_TryCatchToResultTry);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var resultType = compilationContext.Compilation
                    .GetTypeByMetadataName("REslava.Result.Result`1");
                if (resultType is null) return;

                var taskType = compilationContext.Compilation
                    .GetTypeByMetadataName("System.Threading.Tasks.Task`1");

                compilationContext.RegisterSyntaxNodeAction(
                    ctx => Analyze(ctx, resultType, taskType),
                    SyntaxKind.TryStatement);
            });
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol resultType,
            INamedTypeSymbol? taskType)
        {
            var tryStmt = (TryStatementSyntax)context.Node;

            // Must have exactly one catch clause and no finally
            if (tryStmt.Catches.Count != 1 || tryStmt.Finally != null)
                return;

            var catchClause = tryStmt.Catches[0];

            // Catch must catch bare Exception (not a specific subtype like SqlException)
            if (!CatchesBaseException(catchClause))
                return;

            // Catch body must be a single: return Result<T>.Fail(...)
            if (!IsSingleReturnFail(catchClause.Block))
                return;

            // Try block must have a single return statement
            if (tryStmt.Block.Statements.Count != 1)
                return;
            if (tryStmt.Block.Statements[0] is not ReturnStatementSyntax returnStmt)
                return;
            if (returnStmt.Expression is null)
                return;

            // Unwrap await for async context
            var tryReturnExpr = returnStmt.Expression;
            if (tryReturnExpr is AwaitExpressionSyntax awaitExpr)
                tryReturnExpr = awaitExpr.Expression;

            // Try return expression must NOT already be Result<T>
            var tryReturnType = context.SemanticModel
                .GetTypeInfo(tryReturnExpr, context.CancellationToken).Type as INamedTypeSymbol;
            if (tryReturnType is not null && IsResultType(tryReturnType, resultType))
                return;

            // Enclosing method must return Result<T> or Task<Result<T>>
            var enclosingMethod = tryStmt.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (enclosingMethod is null)
                return;

            var returnTypeSymbol = context.SemanticModel
                .GetTypeInfo(enclosingMethod.ReturnType, context.CancellationToken).Type as INamedTypeSymbol;
            if (returnTypeSymbol is null)
                return;

            string? tTypeName = null;
            if (IsResultType(returnTypeSymbol, resultType))
            {
                tTypeName = returnTypeSymbol.TypeArguments[0].Name;
            }
            else if (taskType is not null && IsTaskOfResult(returnTypeSymbol, resultType, taskType))
            {
                var innerResult = returnTypeSymbol.TypeArguments[0] as INamedTypeSymbol;
                tTypeName = innerResult?.TypeArguments[0].Name;
            }
            else
            {
                return; // not Result<T> or Task<Result<T>>
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    Descriptors.RESL1009_TryCatchToResultTry,
                    tryStmt.TryKeyword.GetLocation(),
                    tTypeName ?? "T"));
        }

        internal static bool CatchesBaseException(CatchClauseSyntax catchClause)
        {
            if (catchClause.Declaration is null)
                return true; // bare catch {} — catches everything
            var typeName = catchClause.Declaration.Type.ToString();
            return typeName == "Exception" || typeName == "System.Exception";
        }

        internal static bool IsSingleReturnFail(BlockSyntax block)
        {
            if (block.Statements.Count != 1)
                return false;
            if (block.Statements[0] is not ReturnStatementSyntax ret)
                return false;
            return ContainsFailCall(ret.Expression);
        }

        private static bool ContainsFailCall(ExpressionSyntax? expr)
        {
            if (expr is null) return false;
            return expr is InvocationExpressionSyntax inv &&
                   inv.Expression is MemberAccessExpressionSyntax ma &&
                   ma.Name.Identifier.Text == "Fail";
        }

        internal static bool IsResultType(INamedTypeSymbol type, INamedTypeSymbol resultType)
            => type.IsGenericType &&
               SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, resultType);

        internal static bool IsTaskOfResult(
            INamedTypeSymbol type, INamedTypeSymbol resultType, INamedTypeSymbol taskType)
        {
            if (!type.IsGenericType ||
                !SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, taskType))
                return false;
            if (type.TypeArguments.Length != 1) return false;
            var inner = type.TypeArguments[0] as INamedTypeSymbol;
            return inner is not null && IsResultType(inner, resultType);
        }
    }
}
