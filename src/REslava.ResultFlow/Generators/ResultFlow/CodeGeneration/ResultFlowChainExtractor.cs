using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.ResultFlow.Generators.ResultFlow.Models;
using System.Collections.Generic;
using System.Linq;

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

            // ── Gap 2: Async LINQ patterns ────────────────────────────────────
            { "SelectAsync",         NodeKind.PureTransform },
            { "WhereAsync",          NodeKind.Gatekeeper },
            { "SelectMany",          NodeKind.TransformWithRisk },
        };

        /// <summary>
        /// Walks the fluent chain in <paramref name="method"/> and returns an ordered list
        /// of <see cref="PipelineNode"/>. Returns <c>null</c> if the body cannot be parsed
        /// as a fluent chain (triggers REF001 diagnostic).
        /// </summary>
        /// <param name="method">The method declaration to analyse.</param>
        /// <param name="semanticModel">
        /// Optional semantic model used to extract generic type arguments from each step's return type.
        /// When provided, nodes are populated with <see cref="PipelineNode.InputType"/> and
        /// <see cref="PipelineNode.OutputType"/> for inline type-travel labels in the Mermaid diagram.
        /// </param>
        /// <param name="customMappings">
        /// Optional entries loaded from <c>resultflow.json</c>.
        /// These <b>override</b> the built-in convention dictionary — allowing full substitution
        /// of any built-in classification for custom or non-standard libraries.
        /// </param>
        public static IReadOnlyList<PipelineNode>? Extract(
            MethodDeclarationSyntax method,
            SemanticModel? semanticModel = null,
            IReadOnlyDictionary<string, NodeKind>? customMappings = null)
        {
            var rootExpr = GetRootExpression(method);
            if (rootExpr == null) return null;

            rootExpr = Unwrap(rootExpr);

            // Gap 3: if the root is a variable reference (e.g. `return result;`), resolve its
            // initializer in the method body so the chain walk finds the actual fluent expression.
            if (rootExpr is IdentifierNameSyntax identRoot && method.Body != null)
            {
                var resolved = ResolveVariableInitializer(identRoot.Identifier.ValueText, method.Body);
                if (resolved != null)
                    rootExpr = Unwrap(resolved);
            }

            if (!(rootExpr is InvocationExpressionSyntax))
                return null;

            // Walk the chain bottom-up, collecting (method name, invocation node) pairs
            var collected = new List<(string name, InvocationExpressionSyntax invocationNode)>();
            ExpressionSyntax? current = rootExpr;

            while (current is InvocationExpressionSyntax invocation)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    collected.Add((memberAccess.Name.Identifier.ValueText, invocation));
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

            // Seed prevOutputType from the root expression (the call before the chain begins)
            // e.g. CreateUser() in  CreateUser().Bind(SaveUser).Map(ToDto)
            string? prevOutputType = null;
            if (semanticModel != null)
            {
                if (current is InvocationExpressionSyntax rootCall)
                    prevOutputType = GenericTypeExtractor.GetFirstTypeArgument(rootCall, semanticModel);
                // Gap 3: root is a variable — look up its initializer to seed the type
                else if (current is IdentifierNameSyntax identSeed && method.Body != null)
                {
                    var initExpr = ResolveVariableInitializer(identSeed.Identifier.ValueText, method.Body);
                    if (initExpr is InvocationExpressionSyntax initCall)
                        prevOutputType = GenericTypeExtractor.GetFirstTypeArgument(initCall, semanticModel);
                }
            }

            var nodes = new List<PipelineNode>(collected.Count);

            foreach (var (name, invocationNode) in collected)
            {
                // customMappings override built-ins (explicit user config wins)
                NodeKind kind;
                if (customMappings != null && customMappings.TryGetValue(name, out var custom))
                    kind = custom;
                else if (MethodKindMap.TryGetValue(name, out var builtin))
                    kind = builtin;
                else
                    kind = NodeKind.Unknown;

                // Gap 1: if the argument is a single-expression lambda calling a standalone
                // method (e.g. .Bind(x => DoThing(x))), use the inner method name as the
                // step label — the Kind (TransformWithRisk etc.) is still derived from the
                // outer pipeline method (Bind), so fail edges remain correct.
                string effectiveName = name;
                var firstArg = invocationNode.ArgumentList.Arguments.FirstOrDefault();
                if (firstArg != null)
                {
                    var lambdaName = TryGetLambdaBodyMethodName(firstArg);
                    if (lambdaName != null)
                        effectiveName = lambdaName;
                }

                string? outputType = null;
                string? errorType = null;
                if (semanticModel != null)
                {
                    outputType = GenericTypeExtractor.GetFirstTypeArgument(invocationNode, semanticModel);
                    errorType  = GenericTypeExtractor.GetErrorTypeArgument(invocationNode, semanticModel);
                }

                // Try to extract the error type name from arguments (body-scan fallback for FailLabel)
                string? errorHint = null;
                foreach (var arg in invocationNode.ArgumentList.Arguments)
                {
                    errorHint = TryExtractErrorTypeName(arg);
                    if (errorHint != null) break;
                }

                var node = new PipelineNode(effectiveName, kind)
                {
                    InputType = prevOutputType,
                    OutputType = outputType,
                    ErrorType  = errorType,
                    ErrorHint  = errorHint
                };

                nodes.Add(node);
                prevOutputType = outputType;
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

        /// <summary>
        /// Gap 3: scans a method block body for a local variable declaration whose name matches
        /// <paramref name="variableName"/> and returns its initializer expression (if any).
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
        /// Inspects <paramref name="arg"/> and attempts to extract the error type name for use
        /// as a failure-edge hint on the Mermaid diagram. Handles two syntactic patterns:
        /// <list type="bullet">
        ///   <item><c>new NotFoundError(...)</c> — object-creation expression</item>
        ///   <item><c>NotFoundError.For(...)</c> / <c>ValidationError.Field(...)</c> — static factory on a type
        ///         whose simple name ends with "Error" or "Reason"</item>
        /// </list>
        /// Returns <c>null</c> when neither pattern matches, so the caller silently skips it.
        /// </summary>
        private static string? TryExtractErrorTypeName(ArgumentSyntax arg)
        {
            var expr = arg.Expression;

            // pattern: new SomeError(...) or new SomeError<T>(...)
            if (expr is ObjectCreationExpressionSyntax objCreation)
            {
                var typeName = objCreation.Type switch
                {
                    IdentifierNameSyntax id      => id.Identifier.ValueText,
                    GenericNameSyntax    generic  => generic.Identifier.ValueText,
                    QualifiedNameSyntax  qual     => (qual.Right as IdentifierNameSyntax)?.Identifier.ValueText,
                    _                            => null
                };
                if (typeName != null && (typeName.EndsWith("Error") || typeName.EndsWith("Reason")))
                    return typeName;
            }

            // pattern: SomeError.Factory(...) or SomeError.Factory<T>(...)
            if (expr is InvocationExpressionSyntax inv &&
                inv.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var receiverName = memberAccess.Expression switch
                {
                    IdentifierNameSyntax id     => id.Identifier.ValueText,
                    GenericNameSyntax    generic => generic.Identifier.ValueText,
                    _                           => null
                };
                if (receiverName != null && (receiverName.EndsWith("Error") || receiverName.EndsWith("Reason")))
                    return receiverName;
            }

            return null;
        }

        /// <summary>
        /// Gap 1: if <paramref name="arg"/> is a single-expression lambda whose body is a call
        /// to a standalone method (identifier, not member access), returns that method's name;
        /// otherwise returns null so the caller falls back to the outer pipeline method name.
        /// </summary>
        private static string? TryGetLambdaBodyMethodName(ArgumentSyntax arg)
        {
            ExpressionSyntax? body = null;

            if (arg.Expression is SimpleLambdaExpressionSyntax simple)
                body = simple.Body as ExpressionSyntax;
            else if (arg.Expression is ParenthesizedLambdaExpressionSyntax parens)
                body = parens.Body as ExpressionSyntax;

            if (body == null) return null;

            body = Unwrap(body);

            // Only rename when the lambda calls a standalone function (x => DoThing(x)).
            // Ignore member-access calls (x => x.Name, x => x.ToString()) — those don't
            // represent meaningful business-step renames.
            if (body is InvocationExpressionSyntax invBody &&
                invBody.Expression is IdentifierNameSyntax identName)
                return identName.Identifier.ValueText;

            return null;
        }
    }
}
