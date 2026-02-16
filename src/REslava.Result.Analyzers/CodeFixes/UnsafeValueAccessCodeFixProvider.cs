using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace REslava.Result.Analyzers.CodeFixes
{
    /// <summary>
    /// Code fix provider for RESL1001: Unsafe Result&lt;T&gt;.Value access.
    /// Offers two fixes: wrap in if-guard, or replace with Match().
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnsafeValueAccessCodeFixProvider))]
    [Shared]
    public class UnsafeValueAccessCodeFixProvider : CodeFixProvider
    {
        private const string TitleIfGuard = "Add 'if (IsSuccess)' guard";
        private const string TitleMatch = "Replace with 'Match()'";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create("RESL1001");

        public sealed override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root is null) return;

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var node = root.FindNode(diagnosticSpan);
            if (node is null) return;

            var memberAccess = node.FirstAncestorOrSelf<MemberAccessExpressionSyntax>();
            if (memberAccess is null) return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: TitleIfGuard,
                    createChangedDocument: ct => FixWithIfGuardAsync(context.Document, memberAccess, ct),
                    equivalenceKey: TitleIfGuard),
                diagnostic);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: TitleMatch,
                    createChangedDocument: ct => FixWithMatchAsync(context.Document, memberAccess, ct),
                    equivalenceKey: TitleMatch),
                diagnostic);
        }

        /// <summary>
        /// Fix A: Wrap the containing statement in if (result.IsSuccess) { ... }
        /// </summary>
        private static async Task<Document> FixWithIfGuardAsync(
            Document document,
            MemberAccessExpressionSyntax memberAccess,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root is null) return document;

            var containingStatement = memberAccess.FirstAncestorOrSelf<StatementSyntax>();
            if (containingStatement is null) return document;

            var expressionText = memberAccess.Expression.ToString();

            // Detect line ending style from source
            var sourceText = root.ToFullString();
            var eol = sourceText.Contains("\r\n") ? "\r\n" : "\n";

            // Get the leading whitespace of the containing statement
            var leadingTrivia = containingStatement.GetLeadingTrivia().ToString();

            // Build the if-guard as text to preserve line endings
            var innerIndent = leadingTrivia + "    ";
            var ifText =
                $"{leadingTrivia}if ({expressionText}.IsSuccess){eol}" +
                $"{leadingTrivia}{{{eol}" +
                $"{innerIndent}{containingStatement.WithoutLeadingTrivia().ToFullString().TrimEnd()}{eol}" +
                $"{leadingTrivia}}}";

            var ifStatement = SyntaxFactory.ParseStatement(ifText)
                .WithTrailingTrivia(containingStatement.GetTrailingTrivia());

            var newRoot = root.ReplaceNode(containingStatement, ifStatement);
            return document.WithSyntaxRoot(newRoot);
        }

        /// <summary>
        /// Fix B: Replace result.Value with result.Match(v => v, e => default)
        /// </summary>
        private static async Task<Document> FixWithMatchAsync(
            Document document,
            MemberAccessExpressionSyntax memberAccess,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root is null) return document;

            // Build: result.Match(v => v, e => default)
            var matchAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                memberAccess.Expression,
                SyntaxFactory.IdentifierName("Match"));

            var successLambda = SyntaxFactory.SimpleLambdaExpression(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("v")),
                SyntaxFactory.IdentifierName("v"));

            var failureLambda = SyntaxFactory.SimpleLambdaExpression(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("e")),
                SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression));

            var matchInvocation = SyntaxFactory.InvocationExpression(
                matchAccess,
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.Argument(successLambda),
                        SyntaxFactory.Argument(failureLambda)
                    })));

            var newRoot = root.ReplaceNode(memberAccess, matchInvocation);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
