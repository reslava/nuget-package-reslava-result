using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using REslava.Result.Flow.Generators.ResultFlow.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace REslava.Result.Flow.Generators.ResultFlow.CodeGeneration
{
    /// <summary>
    /// Extracts pipeline nodes from a [ResultFlow]-decorated method using the IOperation model.
    /// Includes success type travel (IResultBase anchor) and error body scanning (IError).
    /// Falls back to syntax-only extraction when semantic model is unavailable.
    /// </summary>
    internal static class ResultFlowChainExtractor
    {
        private static readonly Dictionary<string, NodeKind> MethodKindMap = new Dictionary<string, NodeKind>
        {
            // ── REslava.Result ──────────────────────────────────────────────
            { "Ensure",            NodeKind.Gatekeeper },
            { "EnsureAsync",       NodeKind.Gatekeeper },
            { "Bind",              NodeKind.TransformWithRisk },
            { "BindAsync",         NodeKind.TransformWithRisk },
            { "Map",               NodeKind.PureTransform },
            { "MapAsync",          NodeKind.PureTransform },
            { "Tap",               NodeKind.SideEffectSuccess },
            { "TapAsync",          NodeKind.SideEffectSuccess },
            { "TapOnFailure",      NodeKind.SideEffectFailure },
            { "TapOnFailureAsync", NodeKind.SideEffectFailure },
            { "TapBoth",           NodeKind.SideEffectBoth },
            { "MapError",          NodeKind.SideEffectFailure },
            { "MapErrorAsync",     NodeKind.SideEffectFailure },
            { "Or",                NodeKind.TransformWithRisk },
            { "OrElse",            NodeKind.TransformWithRisk },
            { "OrElseAsync",       NodeKind.TransformWithRisk },
            { "Match",             NodeKind.Terminal },
            { "MatchAsync",        NodeKind.Terminal },
            { "WithSuccess",       NodeKind.Invisible },
            { "WithSuccessAsync",  NodeKind.Invisible },
            { "WithError",         NodeKind.Invisible },
            { "WithSuccessIf",     NodeKind.Invisible },
            { "WithContext",       NodeKind.Invisible },
        };

        private static readonly IReadOnlyCollection<string> EmptyErrors =
            new ReadOnlyCollection<string>(new List<string>());

        /// <summary>
        /// Extracts an ordered list of pipeline nodes from the method body.
        /// Uses syntax-walk to discover the full chain, then calls
        /// <c>semanticModel.GetOperation()</c> per node for type/error info.
        /// This avoids fragile <c>IInvocationOperation.Instance</c> traversal
        /// which breaks for extension methods on <c>Task&lt;Result&lt;T&gt;&gt;</c>
        /// and chains starting with static calls like <c>Result&lt;T&gt;.Ok()</c>.
        /// Returns null if the body cannot be parsed as a fluent chain.
        /// </summary>
        public static IReadOnlyList<PipelineNode>? Extract(
            MethodDeclarationSyntax method,
            SemanticModel semanticModel,
            Compilation compilation,
            INamedTypeSymbol? resultBaseSymbol,
            INamedTypeSymbol? iErrorSymbol,
            IReadOnlyDictionary<string, NodeKind>? customMappings = null)
        {
            var rootExpr = GetRootExpression(method);
            if (rootExpr == null) return null;

            rootExpr = UnwrapSyntax(rootExpr);

            // Gap 3: if root is a variable reference (e.g. `return result;`), resolve its
            // initializer in the method body so the chain walk finds the actual fluent expression.
            if (rootExpr is IdentifierNameSyntax identRoot && method.Body != null)
            {
                var resolved = ResolveVariableInitializer(identRoot.Identifier.ValueText, method.Body);
                if (resolved != null)
                    rootExpr = UnwrapSyntax(resolved);
            }

            if (!(rootExpr is InvocationExpressionSyntax rootInv)) return null;

            // Walk the syntax chain (outermost → root), collecting each member-access invocation.
            // This reliably finds all steps regardless of whether they are instance methods,
            // extension methods on Result<T>, or extension methods on Task<Result<T>>.
            var syntaxChain = new List<InvocationExpressionSyntax>();
            ExpressionSyntax? cur = rootInv;

            while (cur is InvocationExpressionSyntax inv)
            {
                if (inv.Expression is MemberAccessExpressionSyntax)
                {
                    syntaxChain.Add(inv);
                    cur = UnwrapSyntax(((MemberAccessExpressionSyntax)inv.Expression).Expression);
                }
                else
                    break;
            }

            if (syntaxChain.Count == 0) return null;
            syntaxChain.Reverse(); // root-first order

            var nodes = new List<PipelineNode>(syntaxChain.Count);

            // Seed the initial input type from the chain root (the receiver of the first step).
            // e.g. for FindUser(userId).Bind(f), the root is FindUser(userId) → Result<User> → "User".
            // This preserves "User → Product" labels on the first transform node.
            string? previousOutputType = null;
            if (resultBaseSymbol != null)
            {
                var firstStepReceiver = UnwrapSyntax(
                    ((MemberAccessExpressionSyntax)syntaxChain[0].Expression).Expression);
                var rootOp = semanticModel.GetOperation(firstStepReceiver) as IInvocationOperation;
                if (rootOp != null)
                    previousOutputType = ResultTypeExtractor.GetSuccessType(rootOp, resultBaseSymbol);
            }

            for (int i = 0; i < syntaxChain.Count; i++)
            {
                var invSyntax = syntaxChain[i];
                var memberAccess = (MemberAccessExpressionSyntax)invSyntax.Expression;
                var methodName = memberAccess.Name.Identifier.ValueText;

                // Resolve NodeKind: custom mappings override built-ins
                NodeKind kind;
                if (customMappings != null && customMappings.TryGetValue(methodName, out var custom))
                    kind = custom;
                else if (MethodKindMap.TryGetValue(methodName, out var builtin))
                    kind = builtin;
                else
                    kind = NodeKind.Unknown;

                // Gap 1: if the first argument is a single-expression lambda calling a standalone
                // method (e.g. .Bind(x => SaveUser(x))), use the inner method name as the step
                // label. Kind is still derived from the outer pipeline method (Bind), so fail
                // edges remain correct.
                string effectiveName = methodName;
                var firstArg = invSyntax.ArgumentList.Arguments.FirstOrDefault();
                if (firstArg != null)
                {
                    var lambdaName = TryGetLambdaBodyMethodName(firstArg);
                    if (lambdaName != null)
                        effectiveName = lambdaName;
                }

                // Per-node semantic operation — works for each step independently
                var op = semanticModel.GetOperation(invSyntax) as IInvocationOperation;

                // Success type via IResultBase anchor
                string? outputType = null;
                if (op != null && resultBaseSymbol != null)
                    outputType = ResultTypeExtractor.GetSuccessType(op, resultBaseSymbol);

                // Error types via IError body scanning (skip PureTransform and Invisible — they cannot fail)
                IReadOnlyCollection<string> possibleErrors = EmptyErrors;
                if (op != null && kind != NodeKind.PureTransform && kind != NodeKind.Invisible && iErrorSymbol != null)
                    possibleErrors = ResultTypeExtractor.GetPossibleErrors(op, compilation, iErrorSymbol);

                // Source location for clickable Mermaid nodes
                string? sourceFile = null;
                int? sourceLine = null;
                var loc = invSyntax.GetLocation().GetLineSpan();
                if (loc.IsValid && !string.IsNullOrEmpty(loc.Path))
                {
                    sourceFile = loc.Path;
                    sourceLine = loc.StartLinePosition.Line + 1; // 1-indexed
                }

                nodes.Add(new PipelineNode(effectiveName, kind)
                {
                    InputType = previousOutputType,
                    OutputType = outputType,
                    PossibleErrors = possibleErrors,
                    SourceFile = sourceFile,
                    SourceLine = sourceLine,
                });

                previousOutputType = outputType;
            }

            return nodes.Count == 0 ? null : nodes;
        }

        // ── Syntax-only public overload (used by the live Roslyn analyzer) ─────

        /// <summary>
        /// Syntax-only extraction — no semantic model required.
        /// Used by <c>ResultFlowDiagramAnalyzer</c> to check chain detectability at design time.
        /// </summary>
        public static IReadOnlyList<PipelineNode>? ExtractSyntaxOnly(MethodDeclarationSyntax method)
        {
            var rootExpr = GetRootExpression(method);
            if (rootExpr == null) return null;
            rootExpr = UnwrapSyntax(rootExpr);
            return ExtractSyntaxOnly(rootExpr as InvocationExpressionSyntax, null);
        }

        // ── Syntax-only fallback (no type info, no error info) ───────────────

        private static IReadOnlyList<PipelineNode>? ExtractSyntaxOnly(
            InvocationExpressionSyntax? rootInvocation,
            IReadOnlyDictionary<string, NodeKind>? customMappings)
        {
            if (rootInvocation == null) return null;

            var collected = new List<string>();
            ExpressionSyntax? current = rootInvocation;

            while (current is InvocationExpressionSyntax inv)
            {
                if (inv.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    collected.Add(memberAccess.Name.Identifier.ValueText);
                    current = UnwrapSyntax(memberAccess.Expression);
                }
                else
                    break;
            }

            if (collected.Count == 0) return null;
            collected.Reverse();

            var nodes = new List<PipelineNode>(collected.Count);
            foreach (var name in collected)
            {
                NodeKind kind;
                if (customMappings != null && customMappings.TryGetValue(name, out var custom))
                    kind = custom;
                else if (MethodKindMap.TryGetValue(name, out var builtin))
                    kind = builtin;
                else
                    kind = NodeKind.Unknown;

                nodes.Add(new PipelineNode(name, kind));
            }

            return nodes;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static ExpressionSyntax? GetRootExpression(MethodDeclarationSyntax method)
        {
            if (method.ExpressionBody != null)
                return method.ExpressionBody.Expression;

            if (method.Body != null)
            {
                ReturnStatementSyntax? lastReturn = null;
                foreach (var stmt in method.Body.Statements)
                    if (stmt is ReturnStatementSyntax ret) lastReturn = ret;
                return lastReturn?.Expression;
            }

            return null;
        }

        private static ExpressionSyntax UnwrapSyntax(ExpressionSyntax expr)
        {
            while (true)
            {
                if (expr is AwaitExpressionSyntax awaitExpr) expr = awaitExpr.Expression;
                else if (expr is ParenthesizedExpressionSyntax parenExpr) expr = parenExpr.Expression;
                else return expr;
            }
        }

        /// <summary>
        /// Gap 3: scans a method block body for a local variable whose name matches
        /// <paramref name="variableName"/> and returns its initializer expression, if any.
        /// Only one resolution level — multi-hop variable chains are not followed.
        /// </summary>
        private static ExpressionSyntax? ResolveVariableInitializer(string variableName, BlockSyntax body)
        {
            foreach (var stmt in body.Statements)
            {
                if (stmt is LocalDeclarationStatementSyntax localDecl)
                {
                    foreach (var variable in localDecl.Declaration.Variables)
                    {
                        if (variable.Identifier.ValueText == variableName && variable.Initializer != null)
                            return variable.Initializer.Value;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Scans the method body for a <c>.WithContext(...)</c> call and extracts any string-literal
        /// named arguments for <c>operationName</c> and <c>correlationId</c>.
        /// Returns <c>(null, null)</c> if no <c>WithContext</c> call or no literals are found.
        /// </summary>
        public static (string? OperationName, string? CorrelationId) TryExtractContextHints(
            MethodDeclarationSyntax method)
        {
            string? operationName = null;
            string? correlationId = null;

            foreach (var inv in method.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (!(inv.Expression is MemberAccessExpressionSyntax ma)) continue;
                if (ma.Name.Identifier.ValueText != "WithContext") continue;

                foreach (var arg in inv.ArgumentList.Arguments)
                {
                    if (arg.NameColon == null) continue;
                    var argName = arg.NameColon.Name.Identifier.ValueText;
                    var literal = arg.Expression as LiteralExpressionSyntax;
                    if (literal == null) continue;
                    var value = literal.Token.ValueText;

                    if (argName == "operationName" && operationName == null)
                        operationName = value;
                    else if (argName == "correlationId" && correlationId == null)
                        correlationId = value;
                }

                if (operationName != null && correlationId != null) break; // found both
            }

            return (operationName, correlationId);
        }

        /// <summary>
        /// Gap 1: if <paramref name="arg"/> is a single-expression lambda whose body calls a
        /// standalone method (identifier, not member access), returns that method's name so the
        /// caller can use it as the step label. Returns null to fall back to the outer pipeline
        /// method name (Bind, Ensure, etc.).
        /// </summary>
        private static string? TryGetLambdaBodyMethodName(ArgumentSyntax arg)
        {
            ExpressionSyntax? body = null;

            if (arg.Expression is SimpleLambdaExpressionSyntax simple)
                body = simple.Body as ExpressionSyntax;
            else if (arg.Expression is ParenthesizedLambdaExpressionSyntax parens)
                body = parens.Body as ExpressionSyntax;

            if (body == null) return null;

            body = UnwrapSyntax(body);

            // Only rename when the lambda calls a standalone function (x => DoThing(x)).
            // Ignore member-access calls (x => x.Name, x => obj.Method()) — not meaningful renames.
            if (body is InvocationExpressionSyntax invBody &&
                invBody.Expression is IdentifierNameSyntax identName)
                return identName.Identifier.ValueText;

            return null;
        }
    }
}
