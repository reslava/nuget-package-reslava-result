using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace REslava.Result.Analyzers.Analyzers
{
    /// <summary>
    /// RESL1001: Warns when Result&lt;T&gt;.Value is accessed without checking IsSuccess first.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnsafeValueAccessAnalyzer : DiagnosticAnalyzer
    {
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
            if (IsGuardedByCheck(memberAccess))
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

        /// <summary>
        /// Checks whether the .Value access is inside a guard:
        /// - if (result.IsSuccess) { result.Value }
        /// - if (!result.IsFailed) { result.Value }
        /// - if (result.IsFailed) { ... } else { result.Value }  (else branch)
        /// - if (!result.IsSuccess) return; result.Value  (early return)
        /// </summary>
        private static bool IsGuardedByCheck(MemberAccessExpressionSyntax valueAccess)
        {
            var expressionText = GetExpressionIdentifier(valueAccess.Expression);
            if (expressionText is null)
                return false;

            // Walk up ancestors looking for if-statements that guard this .Value
            foreach (var ancestor in valueAccess.Ancestors())
            {
                if (ancestor is IfStatementSyntax ifStatement)
                {
                    // Check if this if-statement's condition is checking our result variable
                    if (IsPositiveGuard(ifStatement.Condition, expressionText))
                    {
                        // .Value is inside the then-block of if (IsSuccess) or if (!IsFailed)
                        if (IsInThenBranch(valueAccess, ifStatement))
                            return true;
                    }

                    if (IsNegativeGuard(ifStatement.Condition, expressionText))
                    {
                        // .Value is inside the else-block of if (IsFailed) or if (!IsSuccess)
                        if (IsInElseBranch(valueAccess, ifStatement))
                            return true;
                    }
                }

                if (ancestor is BlockSyntax block && block.Parent is MethodDeclarationSyntax)
                {
                    // Check for early-return pattern:
                    // if (result.IsFailed) return ...;
                    // result.Value  <-- safe after early return
                    if (HasEarlyReturnGuard(valueAccess, block, expressionText))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the identifier text for the expression (e.g., "result" from "result.Value").
        /// </summary>
        private static string? GetExpressionIdentifier(ExpressionSyntax expression)
        {
            return expression switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.Text,
                _ => expression.ToString()
            };
        }

        /// <summary>
        /// Checks if condition is: x.IsSuccess or !x.IsFailed
        /// </summary>
        private static bool IsPositiveGuard(ExpressionSyntax condition, string variableName)
        {
            // x.IsSuccess
            if (IsPropertyCheck(condition, variableName, "IsSuccess"))
                return true;

            // !x.IsFailed
            if (condition is PrefixUnaryExpressionSyntax prefix
                && prefix.IsKind(SyntaxKind.LogicalNotExpression)
                && IsPropertyCheck(prefix.Operand, variableName, "IsFailed"))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if condition is: x.IsFailed or !x.IsSuccess
        /// </summary>
        private static bool IsNegativeGuard(ExpressionSyntax condition, string variableName)
        {
            // x.IsFailed
            if (IsPropertyCheck(condition, variableName, "IsFailed"))
                return true;

            // !x.IsSuccess
            if (condition is PrefixUnaryExpressionSyntax prefix
                && prefix.IsKind(SyntaxKind.LogicalNotExpression)
                && IsPropertyCheck(prefix.Operand, variableName, "IsSuccess"))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if the expression is "variableName.propertyName".
        /// </summary>
        private static bool IsPropertyCheck(ExpressionSyntax expression, string variableName, string propertyName)
        {
            if (expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name.Identifier.Text == propertyName)
            {
                var exprText = GetExpressionIdentifier(memberAccess.Expression);
                return exprText == variableName;
            }

            return false;
        }

        private static bool IsInThenBranch(SyntaxNode node, IfStatementSyntax ifStatement)
        {
            var statement = ifStatement.Statement;
            return node.SpanStart >= statement.SpanStart && node.Span.End <= statement.Span.End;
        }

        private static bool IsInElseBranch(SyntaxNode node, IfStatementSyntax ifStatement)
        {
            var elseClause = ifStatement.Else;
            if (elseClause is null)
                return false;

            return node.SpanStart >= elseClause.SpanStart && node.Span.End <= elseClause.Span.End;
        }

        /// <summary>
        /// Checks for early-return pattern: if (result.IsFailed) return/throw; followed by .Value access.
        /// </summary>
        private static bool HasEarlyReturnGuard(
            MemberAccessExpressionSyntax valueAccess,
            BlockSyntax methodBlock,
            string variableName)
        {
            foreach (var statement in methodBlock.Statements)
            {
                // Only look at statements BEFORE the .Value access
                if (statement.SpanStart >= valueAccess.SpanStart)
                    break;

                if (statement is IfStatementSyntax ifStatement
                    && IsNegativeGuard(ifStatement.Condition, variableName))
                {
                    // Check if the if-body always exits (return or throw)
                    if (AlwaysExits(ifStatement.Statement))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a statement block always exits (return or throw).
        /// </summary>
        private static bool AlwaysExits(StatementSyntax statement)
        {
            if (statement is ReturnStatementSyntax || statement is ThrowStatementSyntax)
                return true;

            if (statement is BlockSyntax block && block.Statements.Count > 0)
            {
                var last = block.Statements.Last();
                return last is ReturnStatementSyntax || last is ThrowStatementSyntax;
            }

            return false;
        }
    }
}
