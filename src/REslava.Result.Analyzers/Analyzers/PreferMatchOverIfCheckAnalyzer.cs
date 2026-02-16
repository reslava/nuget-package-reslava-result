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
    /// RESL1003: Suggests using Match() when code has if (result.IsSuccess) { ...Value... } else { ...Errors... }.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PreferMatchOverIfCheckAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Descriptors.RESL1003_PreferMatch);

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
                    ctx => AnalyzeIfStatement(ctx, resultType),
                    SyntaxKind.IfStatement);
            });
        }

        private static void AnalyzeIfStatement(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol resultType)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            // Must have an else branch
            if (ifStatement.Else is null)
                return;

            // Extract the variable name and check direction from the condition
            if (!TryGetResultCheckInfo(ifStatement.Condition, out var variableName, out var isPositiveCheck))
                return;

            // Verify the variable is actually a Result<T>
            if (!IsResultVariable(context, ifStatement.Condition, resultType))
                return;

            // Determine which branch is "success" and which is "failure"
            var successBranch = isPositiveCheck ? ifStatement.Statement : ifStatement.Else.Statement;
            var failureBranch = isPositiveCheck ? ifStatement.Else.Statement : ifStatement.Statement;

            // Check if success branch accesses .Value and failure branch accesses .Errors
            if (BranchAccessesMember(successBranch, variableName, "Value")
                && BranchAccessesMember(failureBranch, variableName, "Errors"))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.RESL1003_PreferMatch,
                        ifStatement.IfKeyword.GetLocation()));
            }
        }

        /// <summary>
        /// Extracts variable name and check direction from conditions like:
        /// result.IsSuccess (positive), !result.IsFailed (positive),
        /// result.IsFailed (negative), !result.IsSuccess (negative)
        /// </summary>
        private static bool TryGetResultCheckInfo(
            ExpressionSyntax condition,
            out string variableName,
            out bool isPositiveCheck)
        {
            variableName = null;
            isPositiveCheck = false;

            // Handle negation: !x.Property
            if (condition is PrefixUnaryExpressionSyntax prefix
                && prefix.IsKind(SyntaxKind.LogicalNotExpression))
            {
                if (TryGetMemberCheck(prefix.Operand, out variableName, out var propertyName))
                {
                    // !IsSuccess → negative, !IsFailed → positive
                    isPositiveCheck = propertyName == "IsFailed";
                    return propertyName == "IsSuccess" || propertyName == "IsFailed";
                }
                return false;
            }

            // Handle direct: x.Property
            if (TryGetMemberCheck(condition, out variableName, out var propName))
            {
                // IsSuccess → positive, IsFailed → negative
                isPositiveCheck = propName == "IsSuccess";
                return propName == "IsSuccess" || propName == "IsFailed";
            }

            return false;
        }

        private static bool TryGetMemberCheck(ExpressionSyntax expression, out string variableName, out string propertyName)
        {
            variableName = null;
            propertyName = null;

            if (expression is MemberAccessExpressionSyntax memberAccess)
            {
                propertyName = memberAccess.Name.Identifier.Text;
                variableName = GuardDetectionHelper.GetExpressionIdentifier(memberAccess.Expression);
                return variableName != null;
            }

            return false;
        }

        /// <summary>
        /// Verifies the expression in the condition is actually a Result&lt;T&gt; type.
        /// </summary>
        private static bool IsResultVariable(
            SyntaxNodeAnalysisContext context,
            ExpressionSyntax condition,
            INamedTypeSymbol resultType)
        {
            // Unwrap negation
            if (condition is PrefixUnaryExpressionSyntax prefix)
                condition = prefix.Operand;

            if (condition is MemberAccessExpressionSyntax memberAccess)
            {
                var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken);
                if (typeInfo.Type is INamedTypeSymbol namedType
                    && namedType.IsGenericType
                    && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, resultType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a branch contains an access to variableName.memberName.
        /// </summary>
        private static bool BranchAccessesMember(StatementSyntax branch, string variableName, string memberName)
        {
            return branch.DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .Any(ma => ma.Name.Identifier.Text == memberName
                    && GuardDetectionHelper.GetExpressionIdentifier(ma.Expression) == variableName);
        }
    }
}
