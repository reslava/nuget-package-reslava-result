using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
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
    /// Code fix provider for RESL2001: Unsafe OneOf.AsT* access.
    /// Replaces .AsT* with a complete .Match() call.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnsafeOneOfAccessCodeFixProvider))]
    [Shared]
    public class UnsafeOneOfAccessCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Replace with 'Match()'";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create("RESL2001");

        public sealed override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root is null) return;

            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(diagnostic.Location.SourceSpan);

            var memberAccess = node.FirstAncestorOrSelf<MemberAccessExpressionSyntax>();
            if (memberAccess is null) return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: ct => FixWithMatchAsync(context.Document, memberAccess, ct),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static async Task<Document> FixWithMatchAsync(
            Document document,
            MemberAccessExpressionSyntax memberAccess,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root is null) return document;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (semanticModel is null) return document;

            // Get the OneOf type to determine arity
            var typeInfo = semanticModel.GetTypeInfo(memberAccess.Expression, cancellationToken);
            var oneOfType = typeInfo.Type as INamedTypeSymbol;
            if (oneOfType is null || !oneOfType.IsGenericType) return document;

            var arity = oneOfType.TypeArguments.Length;
            var memberName = memberAccess.Name.Identifier.Text;

            // Parse which AsT* index (1-based)
            if (memberName.Length != 4 || !memberName.StartsWith("AsT") || !char.IsDigit(memberName[3]))
                return document;
            var targetIndex = memberName[3] - '0';

            // Build Match() arguments: one lambda per type argument
            var arguments = new SeparatedSyntaxList<ArgumentSyntax>();
            for (int i = 1; i <= arity; i++)
            {
                var paramName = $"t{i}";
                ExpressionSyntax body;

                if (i == targetIndex)
                {
                    // Matching lambda returns the value: t1 => t1
                    body = SyntaxFactory.IdentifierName(paramName);
                }
                else
                {
                    // Other lambdas throw: t2 => throw new NotImplementedException()
                    body = SyntaxFactory.ThrowExpression(
                        SyntaxFactory.ObjectCreationExpression(
                            SyntaxFactory.ParseTypeName("System.NotImplementedException"))
                        .WithArgumentList(SyntaxFactory.ArgumentList()));
                }

                var lambda = SyntaxFactory.SimpleLambdaExpression(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier(paramName)),
                    body);

                arguments = arguments.Add(SyntaxFactory.Argument(lambda));
            }

            var matchAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                memberAccess.Expression,
                SyntaxFactory.IdentifierName("Match"));

            var matchInvocation = SyntaxFactory.InvocationExpression(
                matchAccess,
                SyntaxFactory.ArgumentList(arguments));

            var newRoot = root.ReplaceNode(memberAccess, matchInvocation);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
