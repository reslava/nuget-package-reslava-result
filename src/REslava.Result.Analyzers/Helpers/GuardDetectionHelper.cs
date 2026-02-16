using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace REslava.Result.Analyzers.Helpers
{
    /// <summary>
    /// Shared guard-detection logic for analyzers that check property access
    /// is preceded by a boolean guard check (e.g., IsSuccess→Value, IsT1→AsT1).
    /// </summary>
    internal static class GuardDetectionHelper
    {
        /// <summary>
        /// Defines guard-property names for positive/negative checks.
        /// </summary>
        internal readonly struct GuardConfig
        {
            /// <summary>Property names that mean "safe to access" (e.g., "IsSuccess", "IsT1").</summary>
            public string[] PositiveProperties { get; }

            /// <summary>Property names that mean "NOT safe to access" (e.g., "IsFailed").</summary>
            public string[] NegativeProperties { get; }

            public GuardConfig(string[] positiveProperties, string[] negativeProperties)
            {
                PositiveProperties = positiveProperties;
                NegativeProperties = negativeProperties;
            }
        }

        /// <summary>
        /// Checks whether a member access is inside a guard:
        /// - if (x.PositiveProp) { x.Access }
        /// - if (!x.PositiveProp) return; x.Access  (early return on negated positive)
        /// - if (x.NegativeProp) { ... } else { x.Access }  (else branch)
        /// - if (!x.NegativeProp) { x.Access }
        /// </summary>
        public static bool IsGuardedByCheck(
            MemberAccessExpressionSyntax accessNode,
            GuardConfig config)
        {
            var expressionText = GetExpressionIdentifier(accessNode.Expression);
            if (expressionText is null)
                return false;

            foreach (var ancestor in accessNode.Ancestors())
            {
                if (ancestor is IfStatementSyntax ifStatement)
                {
                    if (IsPositiveGuard(ifStatement.Condition, expressionText, config))
                    {
                        if (IsInThenBranch(accessNode, ifStatement))
                            return true;
                    }

                    if (IsNegativeGuard(ifStatement.Condition, expressionText, config))
                    {
                        if (IsInElseBranch(accessNode, ifStatement))
                            return true;
                    }
                }

                if (ancestor is BlockSyntax block && block.Parent is MethodDeclarationSyntax)
                {
                    if (HasEarlyReturnGuard(accessNode, block, expressionText, config))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the identifier text for the expression (e.g., "result" from "result.Value").
        /// </summary>
        internal static string GetExpressionIdentifier(ExpressionSyntax expression)
        {
            return expression switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.Text,
                _ => expression.ToString()
            };
        }

        /// <summary>
        /// Checks if condition is a positive guard: x.PositiveProp or !x.NegativeProp
        /// </summary>
        private static bool IsPositiveGuard(ExpressionSyntax condition, string variableName, GuardConfig config)
        {
            // x.PositiveProp (e.g., x.IsSuccess, x.IsT1)
            foreach (var prop in config.PositiveProperties)
            {
                if (IsPropertyCheck(condition, variableName, prop))
                    return true;
            }

            // !x.NegativeProp (e.g., !x.IsFailed)
            if (condition is PrefixUnaryExpressionSyntax prefix
                && prefix.IsKind(SyntaxKind.LogicalNotExpression))
            {
                foreach (var prop in config.NegativeProperties)
                {
                    if (IsPropertyCheck(prefix.Operand, variableName, prop))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if condition is a negative guard: x.NegativeProp or !x.PositiveProp
        /// </summary>
        private static bool IsNegativeGuard(ExpressionSyntax condition, string variableName, GuardConfig config)
        {
            // x.NegativeProp (e.g., x.IsFailed)
            foreach (var prop in config.NegativeProperties)
            {
                if (IsPropertyCheck(condition, variableName, prop))
                    return true;
            }

            // !x.PositiveProp (e.g., !x.IsSuccess, !x.IsT1)
            if (condition is PrefixUnaryExpressionSyntax prefix
                && prefix.IsKind(SyntaxKind.LogicalNotExpression))
            {
                foreach (var prop in config.PositiveProperties)
                {
                    if (IsPropertyCheck(prefix.Operand, variableName, prop))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the expression is "variableName.propertyName".
        /// </summary>
        internal static bool IsPropertyCheck(ExpressionSyntax expression, string variableName, string propertyName)
        {
            if (expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name.Identifier.Text == propertyName)
            {
                var exprText = GetExpressionIdentifier(memberAccess.Expression);
                return exprText == variableName;
            }

            return false;
        }

        internal static bool IsInThenBranch(SyntaxNode node, IfStatementSyntax ifStatement)
        {
            var statement = ifStatement.Statement;
            return node.SpanStart >= statement.SpanStart && node.Span.End <= statement.Span.End;
        }

        internal static bool IsInElseBranch(SyntaxNode node, IfStatementSyntax ifStatement)
        {
            var elseClause = ifStatement.Else;
            if (elseClause is null)
                return false;

            return node.SpanStart >= elseClause.SpanStart && node.Span.End <= elseClause.Span.End;
        }

        /// <summary>
        /// Checks for early-return pattern: if (negativeGuard) return/throw; followed by access.
        /// </summary>
        private static bool HasEarlyReturnGuard(
            MemberAccessExpressionSyntax accessNode,
            BlockSyntax methodBlock,
            string variableName,
            GuardConfig config)
        {
            foreach (var statement in methodBlock.Statements)
            {
                if (statement.SpanStart >= accessNode.SpanStart)
                    break;

                if (statement is IfStatementSyntax ifStatement
                    && IsNegativeGuard(ifStatement.Condition, variableName, config))
                {
                    if (AlwaysExits(ifStatement.Statement))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a statement block always exits (return or throw).
        /// </summary>
        internal static bool AlwaysExits(StatementSyntax statement)
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
