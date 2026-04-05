using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace REslava.ResultFlow.Analyzers
{
    /// <summary>
    /// Code Fix for REF004: adds <c>partial</c> to the class declaration so the generator
    /// can emit the <c>FlowProxy</c> nested class (<c>svc.Flow.Method()</c>).
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ResultFlowPartialCodeFix))]
    [Shared]
    public class ResultFlowPartialCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create("REF004");

        public override FixAllProvider? GetFixAllProvider() => null;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null) return;

            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var typeDecl = token.Parent?.AncestorsAndSelf()
                .OfType<TypeDeclarationSyntax>()
                .FirstOrDefault();
            if (typeDecl == null) return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add 'partial' modifier",
                    createChangedDocument: ct => AddPartialModifierAsync(context.Document, typeDecl, ct),
                    equivalenceKey: "AddPartialModifier"),
                diagnostic);
        }

        private static async Task<Document> AddPartialModifierAsync(
            Document document,
            TypeDeclarationSyntax typeDecl,
            CancellationToken cancellationToken)
        {
            // Append 'partial' to the end of the modifier list — appears right before 'class'/'struct'
            var partialToken = SyntaxFactory.Token(SyntaxKind.PartialKeyword)
                .WithTrailingTrivia(SyntaxFactory.Space);

            var newTypeDecl = typeDecl.AddModifiers(partialToken);

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null) return document;

            return document.WithSyntaxRoot(root.ReplaceNode(typeDecl, newTypeDecl));
        }
    }
}
