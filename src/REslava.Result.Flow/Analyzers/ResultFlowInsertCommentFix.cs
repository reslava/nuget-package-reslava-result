using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.Result.Flow.Generators.ResultFlow.CodeGeneration;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace REslava.Result.Flow.Analyzers
{
    /// <summary>
    /// Code Action for REF002: inserts the Mermaid pipeline diagram (with full type travel and
    /// typed error edges) as a <c>/* ... */</c> block comment directly above the
    /// <c>[ResultFlow]</c>-annotated method. Uses semantic model for rich diagrams when
    /// available; falls back to syntax-only extraction otherwise.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ResultFlowInsertCommentFix))]
    [Shared]
    public class ResultFlowInsertCommentFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create("REF002");

        // Fix-all is not supported: each diagram is method-specific
        public override FixAllProvider? GetFixAllProvider() => null;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null) return;

            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var methodDecl = token.Parent?.AncestorsAndSelf()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault();
            if (methodDecl == null) return;

            var hasExisting = methodDecl.GetLeadingTrivia().Any(t =>
                t.IsKind(SyntaxKind.MultiLineCommentTrivia) &&
                t.ToFullString().Contains("```mermaid"));

            var title = hasExisting
                ? "Refresh [ResultFlow] diagram comment"
                : "Insert [ResultFlow] diagram as comment";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => InsertDiagramCommentAsync(context.Document, methodDecl, ct),
                    equivalenceKey: "InsertResultFlowDiagram"),
                diagnostic);
        }

        private static async Task<Document> InsertDiagramCommentAsync(
            Document document,
            MethodDeclarationSyntax methodDecl,
            CancellationToken cancellationToken)
        {
            // Attempt full semantic extraction (type travel + typed error edges)
            var chain = await TryExtractWithSemanticModelAsync(document, methodDecl, cancellationToken)
                .ConfigureAwait(false);

            // Fall back to syntax-only if semantic model is unavailable
            if (chain == null)
                chain = ResultFlowChainExtractor.ExtractSyntaxOnly(methodDecl);

            if (chain == null) return document;

            var methodName = methodDecl.Identifier.Text;
            var seedMethodName = ResultFlowChainExtractor.TryGetSeedMethodName(methodDecl);
            var (operationName, correlationId) = ResultFlowChainExtractor.TryExtractContextHints(methodDecl);
            var mermaid = ResultFlowMermaidRenderer.Render(
                chain,
                methodTitle: methodName,
                seedMethodName: seedMethodName,
                operationName: operationName,
                correlationId: correlationId);

            // Build: /*\n```mermaid\n{mermaid}\n```*/
            // Fence markers are flush-left (no indent) so all Markdown renderers activate preview.
            // Normalize to \n to avoid CRLF-on-save → double-blank-line on next refresh.
            var sb = new StringBuilder();
            sb.Append("/*\n");
            sb.Append("```mermaid\n");
            sb.Append(mermaid.Replace("\r\n", "\n"));
            sb.Append("\n```");
            sb.Append("*/");
            var commentTrivia = SyntaxFactory.Comment(sb.ToString());

            var leadingTrivia = methodDecl.GetLeadingTrivia();

            // If an existing generated mermaid block is present, replace it in-place.
            // Surrounding whitespace/newlines are preserved unchanged.
            int existingIndex = -1;
            for (int i = 0; i < leadingTrivia.Count; i++)
            {
                if (leadingTrivia[i].IsKind(SyntaxKind.MultiLineCommentTrivia) &&
                    leadingTrivia[i].ToFullString().Contains("```mermaid"))
                {
                    existingIndex = i;
                    break;
                }
            }

            SyntaxTriviaList newLeadingTrivia;
            if (existingIndex >= 0)
            {
                // Replace the stale comment — all other trivia (indentation, newlines) stay intact
                newLeadingTrivia = leadingTrivia.Replace(leadingTrivia[existingIndex], commentTrivia);
            }
            else
            {
                // No existing block → insert above the method (original behavior)
                var indent = "";
                int lastWsIndex = -1;
                for (int i = leadingTrivia.Count - 1; i >= 0; i--)
                {
                    if (leadingTrivia[i].IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                        indent = leadingTrivia[i].ToFullString();
                        lastWsIndex = i;
                        break;
                    }
                }

                // Insert at lastWsIndex+1 (just after indentation whitespace) — in reverse order:
                //   Before: [..., ws]
                //   After:  [..., ws, comment, newline, ws_copy]
                // which renders: {indent}/* ... */\n{indent}[ResultFlow]...
                if (lastWsIndex >= 0)
                {
                    int insertAt = lastWsIndex + 1;
                    newLeadingTrivia = leadingTrivia
                        .Insert(insertAt, SyntaxFactory.Whitespace(indent))  // duplicate indent for method line
                        .Insert(insertAt, SyntaxFactory.EndOfLine("\n"))     // newline after comment
                        .Insert(insertAt, commentTrivia);                    // the comment itself
                }
                else
                {
                    // No indentation found — prepend comment + newline
                    newLeadingTrivia = leadingTrivia
                        .Insert(0, SyntaxFactory.EndOfLine("\n"))
                        .Insert(0, commentTrivia);
                }
            }

            var newMethodDecl = methodDecl.WithLeadingTrivia(newLeadingTrivia);
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null) return document;

            var newRoot = root.ReplaceNode(methodDecl, newMethodDecl);
            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<System.Collections.Generic.IReadOnlyList<Generators.ResultFlow.Models.PipelineNode>?> TryExtractWithSemanticModelAsync(
            Document document,
            MethodDeclarationSyntax methodDecl,
            CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (semanticModel == null) return null;

            var compilation = await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            if (compilation == null) return null;

            var resultBaseSymbol = compilation.GetTypeByMetadataName("REslava.Result.IResultBase");
            var iErrorSymbol = compilation.GetTypeByMetadataName("REslava.Result.IError");

            return ResultFlowChainExtractor.Extract(
                methodDecl,
                semanticModel,
                compilation,
                resultBaseSymbol,
                iErrorSymbol);
        }
    }
}
