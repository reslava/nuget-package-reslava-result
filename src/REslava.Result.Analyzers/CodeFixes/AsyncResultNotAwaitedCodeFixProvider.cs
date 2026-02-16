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

namespace REslava.Result.Analyzers.CodeFixes
{
    /// <summary>
    /// Code fix provider for RESL1004: Adds 'await' to unawaited Task&lt;Result&lt;T&gt;&gt; expressions.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AsyncResultNotAwaitedCodeFixProvider))]
    [Shared]
    public class AsyncResultNotAwaitedCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Add 'await'";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create("RESL1004");

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

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: ct => AddAwaitAsync(context.Document, node, ct),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static async Task<Document> AddAwaitAsync(
            Document document,
            SyntaxNode node,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root is null) return document;

            // The node is the expression (e.g., GetFromDb(id))
            // We need to wrap it with await
            if (node is not ExpressionSyntax expression)
                return document;

            var awaitExpression = SyntaxFactory.AwaitExpression(expression)
                .WithLeadingTrivia(expression.GetLeadingTrivia())
                .WithTrailingTrivia(expression.GetTrailingTrivia());

            // Remove leading trivia from the original expression since it's now on the await
            var expressionWithoutTrivia = expression.WithoutLeadingTrivia();
            var awaitWithInner = SyntaxFactory.AwaitExpression(expressionWithoutTrivia)
                .WithLeadingTrivia(expression.GetLeadingTrivia())
                .WithTrailingTrivia(expression.GetTrailingTrivia());

            var newRoot = root.ReplaceNode(expression, awaitWithInner);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
