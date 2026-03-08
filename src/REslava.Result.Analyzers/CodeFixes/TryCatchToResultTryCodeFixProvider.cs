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
    /// Code fix for RESL1009: replaces a try/catch block with Result&lt;T&gt;.Try() or TryAsync().
    /// Fix A: Result&lt;T&gt;.Try(() => expr)                      — always offered.
    /// Fix B: Result&lt;T&gt;.Try(() => expr, ex => errorExpr)     — when catch has a custom error.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TryCatchToResultTryCodeFixProvider))]
    [Shared]
    public class TryCatchToResultTryCodeFixProvider : CodeFixProvider
    {
        private const string TitleFixA = "Replace with Result<T>.Try()";
        private const string TitleFixB = "Replace with Result<T>.Try() with custom error handler";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create("RESL1009");

        public sealed override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root is null) return;

            var diagnostic = context.Diagnostics.First();
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var tryStmt = token.Parent?.FirstAncestorOrSelf<TryStatementSyntax>();
            if (tryStmt is null) return;

            // Fix A — always offered
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: TitleFixA,
                    createChangedDocument: ct => ApplyFixAsync(context.Document, tryStmt, fixA: true, ct),
                    equivalenceKey: TitleFixA),
                diagnostic);

            // Fix B — offered when catch body has a custom (non-ExceptionError) error expression
            if (HasCustomErrorExpression(tryStmt.Catches[0]))
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: TitleFixB,
                        createChangedDocument: ct => ApplyFixAsync(context.Document, tryStmt, fixA: false, ct),
                        equivalenceKey: TitleFixB),
                    diagnostic);
            }
        }

        private static bool HasCustomErrorExpression(CatchClauseSyntax catchClause)
        {
            if (catchClause.Block.Statements.Count != 1) return false;
            if (catchClause.Block.Statements[0] is not ReturnStatementSyntax ret) return false;
            if (ret.Expression is not InvocationExpressionSyntax failInv) return false;
            if (failInv.ArgumentList.Arguments.Count != 1) return false;
            var errorExpr = failInv.ArgumentList.Arguments[0].Expression;
            // Trivial = new ExceptionError(ex) → Fix A is sufficient, no need for Fix B
            if (errorExpr is ObjectCreationExpressionSyntax objCreation)
            {
                var typeName = objCreation.Type.ToString();
                if (typeName == "ExceptionError" || typeName == "REslava.Result.ExceptionError")
                    return false;
            }
            return true;
        }

        private static async Task<Document> ApplyFixAsync(
            Document document,
            TryStatementSyntax tryStmt,
            bool fixA,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root is null) return document;

            var methodDecl = tryStmt.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (methodDecl is null) return document;

            var isAsync = methodDecl.Modifiers.Any(SyntaxKind.AsyncKeyword);
            var catchClause = tryStmt.Catches[0];

            // Detect EOL style
            var sourceText = root.ToFullString();
            var eol = sourceText.Contains("\r\n") ? "\r\n" : "\n";

            // Method indent (whitespace-only leading trivia)
            var methodIndent = GetMethodIndent(methodDecl);
            var bodyIndent = methodIndent + "    ";

            // Factory type: for async strip Task<...> wrapper; keep as-is for sync
            var factoryReturnType = isAsync
                ? GetTaskTypeArgument(methodDecl.ReturnType)
                : methodDecl.ReturnType.WithoutTrivia().ToString();
            var factoryMethod = isAsync ? "TryAsync" : "Try";

            // Try body expression (unwrap await for async)
            var tryReturn = (ReturnStatementSyntax)tryStmt.Block.Statements[0];
            var tryExpr = tryReturn.Expression!;
            if (isAsync && tryExpr is AwaitExpressionSyntax awaitExpr)
                tryExpr = awaitExpr.Expression;
            var tryExprText = tryExpr.WithoutTrivia().ToString();

            // Build factory call
            string factoryCall;
            if (fixA)
            {
                factoryCall = $"{factoryReturnType}.{factoryMethod}(() => {tryExprText})";
            }
            else
            {
                var catchParam = catchClause.Declaration?.Identifier.Text ?? "ex";
                var catchReturn = (ReturnStatementSyntax)catchClause.Block.Statements[0];
                var failInv = (InvocationExpressionSyntax)catchReturn.Expression!;
                var errorExprText = failInv.ArgumentList.Arguments[0].Expression.WithoutTrivia().ToString();
                factoryCall = $"{factoryReturnType}.{factoryMethod}(() => {tryExprText}, {catchParam} => {errorExprText})";
            }

            // Build new method modifiers (removing async)
            var newMods = string.Join(" ", methodDecl.Modifiers
                .Where(m => !m.IsKind(SyntaxKind.AsyncKeyword))
                .Select(m => m.WithoutTrivia().ToString()));

            // Build the complete method text (no leading trivia — added via WithLeadingTrivia)
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(newMods))
                sb.Append(newMods).Append(' ');
            sb.Append(methodDecl.ReturnType.WithoutTrivia());
            sb.Append(' ');
            sb.Append(methodDecl.Identifier.WithoutTrivia());
            if (methodDecl.TypeParameterList != null)
                sb.Append(methodDecl.TypeParameterList.WithoutTrivia());
            sb.Append(methodDecl.ParameterList.WithoutTrivia());
            sb.Append(" =>");
            sb.Append(eol);
            sb.Append(bodyIndent);
            sb.Append(factoryCall);
            sb.Append(';');

            var newMethodSource = sb.ToString();
            var parsedMethod = (MethodDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration(newMethodSource)!;

            var newMethod = parsedMethod
                .WithLeadingTrivia(methodDecl.GetLeadingTrivia())
                .WithTrailingTrivia(methodDecl.GetTrailingTrivia());

            var newRoot = root.ReplaceNode(methodDecl, newMethod);
            return document.WithSyntaxRoot(newRoot);
        }

        private static string GetMethodIndent(MethodDeclarationSyntax method)
        {
            var trivia = method.GetLeadingTrivia();
            for (int i = trivia.Count - 1; i >= 0; i--)
            {
                if (trivia[i].IsKind(SyntaxKind.WhitespaceTrivia))
                    return trivia[i].ToString();
            }
            return string.Empty;
        }

        private static string GetTaskTypeArgument(TypeSyntax returnType)
        {
            if (returnType is GenericNameSyntax generic &&
                generic.TypeArgumentList.Arguments.Count == 1)
                return generic.TypeArgumentList.Arguments[0].WithoutTrivia().ToString();
            return returnType.WithoutTrivia().ToString();
        }
    }
}
