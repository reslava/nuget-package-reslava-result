using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.ResultFlow.Generators.ResultFlow;
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
            { "WithContext",         NodeKind.Invisible },

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
        /// <param name="maxDepth">
        /// Maximum depth to follow Bind/BindAsync calls into sub-methods for cross-method tracing.
        /// 0 disables tracing. Default: 2.
        /// </param>
        /// <param name="compilation">
        /// Optional compilation used for cross-method tracing (syntax-only name lookup).
        /// When null, cross-method tracing is disabled regardless of <paramref name="maxDepth"/>.
        /// </param>
        public static IReadOnlyList<PipelineNode>? Extract(
            MethodDeclarationSyntax method,
            SemanticModel? semanticModel = null,
            IReadOnlyDictionary<string, NodeKind>? customMappings = null,
            int maxDepth = 2,
            Compilation? compilation = null)
            => ExtractCore(method, semanticModel, customMappings, compilation,
                visited: new HashSet<string>(), remainingDepth: maxDepth);

        private static IReadOnlyList<PipelineNode>? ExtractCore(
            MethodDeclarationSyntax method,
            SemanticModel? semanticModel,
            IReadOnlyDictionary<string, NodeKind>? customMappings,
            Compilation? compilation,
            HashSet<string> visited,
            int remainingDepth)
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

                // Source location for clickable Mermaid nodes
                string? sourceFile = null;
                int? sourceLine = null;
                var loc = invocationNode.GetLocation().GetLineSpan();
                if (loc.IsValid && !string.IsNullOrEmpty(loc.Path))
                {
                    sourceFile = loc.Path;
                    sourceLine = loc.StartLinePosition.Line + 1; // 1-indexed
                }

                var node = new PipelineNode(effectiveName, kind)
                {
                    InputType  = prevOutputType,
                    OutputType = outputType,
                    ErrorType  = errorType,
                    ErrorHint  = errorHint,
                    SourceFile = sourceFile,
                    SourceLine = sourceLine,
                };

                // Cross-method tracing: follow Bind/BindAsync lambdas into same-project methods.
                // Syntax-only: look up by name; skip if 0 or 2+ matches (ambiguous).
                if (kind == NodeKind.TransformWithRisk && remainingDepth > 0
                    && firstArg != null && compilation != null)
                {
                    var lambdaBodyInv = TryGetLambdaBodyInvocation(firstArg);
                    string? traceMethodName = null;
                    if (lambdaBodyInv != null)
                    {
                        if (lambdaBodyInv.Expression is IdentifierNameSyntax ident)
                            traceMethodName = ident.Identifier.ValueText;
                        else if (lambdaBodyInv.Expression is MemberAccessExpressionSyntax memberAccessTrace)
                            traceMethodName = memberAccessTrace.Name.Identifier.ValueText;
                    }

                    if (traceMethodName != null)
                    {
                        var (subNodes, subLayer, subClassName) = TraceInto(
                            traceMethodName, compilation,
                            customMappings, visited, remainingDepth - 1);
                        if (subNodes != null)
                        {
                            node.SubNodes = subNodes;
                            node.SubGraphName = effectiveName;
                            node.Layer = subLayer;
                            node.ClassName = subClassName;
                        }
                    }
                }

                nodes.Add(node);
                prevOutputType = outputType;
            }

            return nodes;
        }

        // ── Cross-method tracing (syntax-only) ───────────────────────────────

        /// <summary>
        /// Syntax-only best-effort: finds the unique <see cref="MethodDeclarationSyntax"/>
        /// with <paramref name="methodName"/> across all syntax trees in the compilation,
        /// then recursively extracts its pipeline. Returns (null, null) when:
        /// - depth is exhausted
        /// - the method name is already on the call stack (cycle guard)
        /// - no match, or more than one match (ambiguous — avoids false positives)
        /// The second tuple element is the detected architectural layer of the found method.
        /// </summary>
        private static (IReadOnlyList<PipelineNode>? nodes, string? layer, string? className) TraceInto(
            string methodName,
            Compilation compilation,
            IReadOnlyDictionary<string, NodeKind>? customMappings,
            HashSet<string> visited,
            int remainingDepth)
        {
            if (remainingDepth < 0) return (null, null, null);
            if (!visited.Add(methodName)) return (null, null, null); // cycle guard by name

            try
            {
                MethodDeclarationSyntax? match = null;
                int matchCount = 0;

                foreach (var tree in compilation.SyntaxTrees)
                {
                    foreach (var m in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                    {
                        if (m.Identifier.ValueText == methodName)
                        {
                            match = m;
                            matchCount++;
                            if (matchCount > 1) break;
                        }
                    }
                    if (matchCount > 1) break;
                }

                if (matchCount != 1 || match == null) return (null, null, null);

                // Detect the layer and class name of the found method before tracing into it.
                var containingNs = GetContainingNamespace(match);
                var layer = LayerDetector.Detect(match, containingNs);
                var className = (match.Parent as Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax)?.Identifier.ValueText;

                // Use a fresh semantic model for the target tree to preserve type info.
                var targetModel = compilation.GetSemanticModel(match.SyntaxTree);
                var nodes = ExtractCore(match, targetModel, customMappings, compilation, visited, remainingDepth);
                return (nodes, layer, className);
            }
            finally
            {
                visited.Remove(methodName); // allow same method in sibling branches
            }
        }

        /// <summary>
        /// Walks the syntax tree upward from <paramref name="method"/> to reconstruct
        /// the fully-qualified namespace string. Handles both block-scoped
        /// (<c>namespace Foo.Bar { }</c>) and file-scoped (<c>namespace Foo.Bar;</c>) declarations.
        /// Returns an empty string when no enclosing namespace is found.
        /// </summary>
        internal static string GetContainingNamespace(MethodDeclarationSyntax method)
        {
            var parts = new System.Collections.Generic.Stack<string>();
            SyntaxNode? node = method.Parent;

            while (node != null)
            {
                if (node is NamespaceDeclarationSyntax ns)
                    parts.Push(ns.Name.ToString());
                else if (node is FileScopedNamespaceDeclarationSyntax fsns)
                    parts.Push(fsns.Name.ToString());
                node = node.Parent;
            }

            return string.Join(".", parts);
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
        /// Returns the <see cref="InvocationExpressionSyntax"/> from a single-expression lambda
        /// arg whose body is a standalone method call (e.g. <c>x => SaveUser(x)</c>).
        /// Used to obtain the target method name for cross-method tracing.
        /// Returns null for member-access calls, method groups, or non-lambda args.
        /// </summary>
        private static InvocationExpressionSyntax? TryGetLambdaBodyInvocation(ArgumentSyntax arg)
        {
            ExpressionSyntax? body = null;

            if (arg.Expression is SimpleLambdaExpressionSyntax simple)
                body = simple.Body as ExpressionSyntax;
            else if (arg.Expression is ParenthesizedLambdaExpressionSyntax parens)
                body = parens.Body as ExpressionSyntax;

            if (body == null) return null;
            body = Unwrap(body);

            // Trace standalone calls (x => DoThing(x)) AND qualified calls (x => SomeClass.DoThing(x)).
            if (body is InvocationExpressionSyntax inv &&
                (inv.Expression is IdentifierNameSyntax || inv.Expression is MemberAccessExpressionSyntax))
                return inv;

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

            if (body is InvocationExpressionSyntax invBody)
            {
                // Standalone call: x => DoThing(x) → "DoThing"
                if (invBody.Expression is IdentifierNameSyntax identName)
                    return identName.Identifier.ValueText;
                // Qualified call: x => SomeClass.DoThing(x) → "DoThing"
                if (invBody.Expression is MemberAccessExpressionSyntax memberAccess)
                    return memberAccess.Name.Identifier.ValueText;
            }

            return null;
        }
    }
}
