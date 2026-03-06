using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.ResultFlow.Generators.ResultFlow.Models;
using System.Collections.Generic;

namespace REslava.ResultFlow.Generators.ResultFlow.CodeGeneration
{
    internal static class ResultFlowChainExtractor
    {
        private static readonly Dictionary<string, NodeKind> MethodKindMap = new Dictionary<string, NodeKind>
        {
            // ── REslava.Result ────────────────────────────────────────────────
            { "Ensure",              NodeKind.Gatekeeper },
            { "EnsureAsync",         NodeKind.Gatekeeper },
            { "Bind",                NodeKind.TransformWithRisk },
            { "BindAsync",           NodeKind.TransformWithRisk },
            { "Map",                 NodeKind.PureTransform },
            { "MapAsync",            NodeKind.PureTransform },
            { "Tap",                 NodeKind.SideEffectSuccess },
            { "TapAsync",            NodeKind.SideEffectSuccess },
            { "TapOnFailure",        NodeKind.SideEffectFailure },
            { "TapOnFailureAsync",   NodeKind.SideEffectFailure },
            { "TapBoth",             NodeKind.SideEffectBoth },
            { "MapError",            NodeKind.SideEffectFailure },
            { "MapErrorAsync",       NodeKind.SideEffectFailure },
            { "Or",                  NodeKind.TransformWithRisk },
            { "OrElse",              NodeKind.TransformWithRisk },
            { "OrElseAsync",         NodeKind.TransformWithRisk },
            { "Match",               NodeKind.Terminal },
            { "MatchAsync",          NodeKind.Terminal },
            { "WithSuccess",         NodeKind.Invisible },
            { "WithSuccessAsync",    NodeKind.Invisible },
            { "WithError",           NodeKind.Invisible },
            { "WithSuccessIf",       NodeKind.Invisible },

            // ── ErrorOr ───────────────────────────────────────────────────────
            { "Then",                NodeKind.TransformWithRisk },
            { "ThenAsync",           NodeKind.TransformWithRisk },
            { "Switch",              NodeKind.Terminal },
            { "SwitchAsync",         NodeKind.Terminal },

            // ── LanguageExt ───────────────────────────────────────────────────
            { "Filter",              NodeKind.Gatekeeper },
            { "Do",                  NodeKind.SideEffectSuccess },
            { "DoAsync",             NodeKind.SideEffectSuccess },
            { "DoLeft",              NodeKind.SideEffectFailure },
            { "DoLeftAsync",         NodeKind.SideEffectFailure },
        };

        /// <summary>
        /// Walks the fluent chain in <paramref name="method"/> and returns an ordered list
        /// of <see cref="PipelineNode"/>. Returns <c>null</c> if the body cannot be parsed
        /// as a fluent chain (triggers REF001 diagnostic).
        /// </summary>
        /// <param name="method">The method declaration to analyse.</param>
        /// <param name="customMappings">
        /// Optional entries loaded from <c>resultflow.json</c>.
        /// These <b>override</b> the built-in convention dictionary — allowing full substitution
        /// of any built-in classification for custom or non-standard libraries.
        /// </param>
        public static IReadOnlyList<PipelineNode>? Extract(
            MethodDeclarationSyntax method,
            IReadOnlyDictionary<string, NodeKind>? customMappings = null)
        {
            var rootExpr = GetRootExpression(method);
            if (rootExpr == null) return null;

            rootExpr = Unwrap(rootExpr);

            if (!(rootExpr is InvocationExpressionSyntax))
                return null;

            // Walk the chain bottom-up, collecting method names
            var collected = new List<string>();
            ExpressionSyntax? current = rootExpr;

            while (current is InvocationExpressionSyntax invocation)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    collected.Add(memberAccess.Name.Identifier.ValueText);
                    current = Unwrap(memberAccess.Expression);
                }
                else
                {
                    break; // reached root call (e.g. CreateUser(cmd) or a variable)
                }
            }

            if (collected.Count == 0) return null;

            // Reverse: bottom-up → top-down order
            collected.Reverse();

            var nodes = new List<PipelineNode>(collected.Count);
            foreach (var name in collected)
            {
                // customMappings override built-ins (explicit user config wins)
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

        private static ExpressionSyntax? GetRootExpression(MethodDeclarationSyntax method)
        {
            // Expression body: method => expr
            if (method.ExpressionBody != null)
                return method.ExpressionBody.Expression;

            // Block body: find the last return statement
            if (method.Body != null)
            {
                ReturnStatementSyntax? lastReturn = null;
                foreach (var stmt in method.Body.Statements)
                {
                    if (stmt is ReturnStatementSyntax ret)
                        lastReturn = ret;
                }
                return lastReturn?.Expression;
            }

            return null;
        }

        private static ExpressionSyntax Unwrap(ExpressionSyntax expr)
        {
            while (true)
            {
                if (expr is AwaitExpressionSyntax awaitExpr)
                    expr = awaitExpr.Expression;
                else if (expr is ParenthesizedExpressionSyntax parenExpr)
                    expr = parenExpr.Expression;
                else
                    return expr;
            }
        }
    }
}
