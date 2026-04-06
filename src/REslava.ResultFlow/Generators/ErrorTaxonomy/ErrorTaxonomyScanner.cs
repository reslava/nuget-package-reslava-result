using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using REslava.ResultFlow.Generators.ResultFlow.CodeGeneration;
using System.Collections.Generic;
using System.Linq;

namespace REslava.ResultFlow.Generators.ErrorTaxonomy
{
    /// <summary>
    /// Syntax-only (library-agnostic) scanner for error types in public methods.
    /// Two passes:
    /// <list type="bullet">
    ///   <item><c>Result&lt;T, TError&gt;</c> return type → <c>TError</c> from generic param via semantic model (certain)</item>
    ///   <item><c>Result&lt;T&gt;</c> / any Result-like return → body scan for <c>Fail(new XxxError())</c> /
    ///     <c>Ensure(..., new XxxError())</c> using type-name heuristic (inferred)</item>
    /// </list>
    /// Does not reference REslava.Result types — uses name heuristics and semantic type resolution only.
    /// </summary>
    internal static class ErrorTaxonomyScanner
    {
        public const string ConfidenceCertain = "certain";
        public const string ConfidenceInferred = "inferred";

        internal readonly struct TaxonomyRow
        {
            internal readonly string MethodName;
            internal readonly string ErrorType;
            internal readonly string Confidence;

            internal TaxonomyRow(string methodName, string errorType, string confidence)
            {
                MethodName = methodName;
                ErrorType = errorType;
                Confidence = confidence;
            }
        }

        /// <summary>
        /// Scans all public methods in <paramref name="classDecl"/> and returns de-duplicated
        /// taxonomy rows sorted by method name then error type.
        /// </summary>
        public static IReadOnlyList<TaxonomyRow> Scan(
            TypeDeclarationSyntax classDecl,
            SemanticModel semanticModel)
        {
            var seen = new HashSet<(string, string)>();
            var rows = new List<TaxonomyRow>();

            foreach (var methodDecl in classDecl.Members.OfType<MethodDeclarationSyntax>())
            {
                if (!methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                    continue;

                var methodName = methodDecl.Identifier.ValueText;

                // Check if return type looks like a Result — name heuristic: "Result" in the type name
                if (!ReturnTypeLooksLikeResult(methodDecl.ReturnType))
                    continue;

                // Pass 1: try certain extraction via semantic model (Result<T, TError> → TError)
                var certainError = TryGetCertainErrorType(methodDecl, semanticModel);
                if (certainError != null)
                {
                    AddIfNew(seen, rows, methodName, certainError, ConfidenceCertain);
                    continue; // certain type found — skip body scan for this method
                }

                // Pass 2: syntax body scan for Fail / Ensure with object creations
                ScanMethodBody(methodDecl, semanticModel, methodName, seen, rows);
            }

            rows.Sort((a, b) =>
            {
                var c = string.Compare(a.MethodName, b.MethodName, System.StringComparison.Ordinal);
                return c != 0 ? c : string.Compare(a.ErrorType, b.ErrorType, System.StringComparison.Ordinal);
            });

            return rows;
        }

        // ── Pass 1 — certain via semantic model ────────────────────────────────

        private static string? TryGetCertainErrorType(
            MethodDeclarationSyntax methodDecl,
            SemanticModel semanticModel)
        {
            // Find the first invocation in the method body that returns Result<T, TError>
            // (the seed call, e.g. the first fluent step or an Ok/Fail factory)
            var body = (SyntaxNode?)methodDecl.Body ?? methodDecl.ExpressionBody;
            if (body == null) return null;

            // Try the return type of the method symbol itself — most reliable for Result<T, TError>
            var symbol = semanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;
            if (symbol == null) return null;

            var returnType = UnwrapTask(symbol.ReturnType);
            if (returnType is INamedTypeSymbol named && named.TypeArguments.Length >= 2)
            {
                // Confirm this looks like a Result type (name heuristic)
                if (named.Name == "Result")
                    return named.TypeArguments[1].Name;
            }

            // Fallback: scan invocations for one returning Result<T, TError>
            foreach (var invocation in body.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var errorType = GenericTypeExtractor.GetErrorTypeArgument(invocation, semanticModel);
                if (errorType != null)
                    return errorType;
            }

            return null;
        }

        // ── Pass 2 — inferred via syntax heuristic ─────────────────────────────

        private static void ScanMethodBody(
            MethodDeclarationSyntax methodDecl,
            SemanticModel semanticModel,
            string methodName,
            HashSet<(string, string)> seen,
            List<TaxonomyRow> rows)
        {
            var body = (SyntaxNode?)methodDecl.Body ?? methodDecl.ExpressionBody;
            if (body == null) return;

            foreach (var invocation in body.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
            {
                string? invocationName = null;
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                    invocationName = memberAccess.Name.Identifier.ValueText;
                else if (invocation.Expression is IdentifierNameSyntax ident)
                    invocationName = ident.Identifier.ValueText;

                if (invocationName != "Fail" && invocationName != "Ensure")
                    continue;

                var args = invocation.ArgumentList.Arguments;
                var candidateArgs = invocationName == "Ensure"
                    ? args.Skip(1).Take(1)
                    : args.Take(1);

                foreach (var arg in candidateArgs)
                    ExtractErrorFromArgument(arg.Expression, semanticModel, methodName, seen, rows);
            }
        }

        private static void ExtractErrorFromArgument(
            ExpressionSyntax argExpr,
            SemanticModel semanticModel,
            string methodName,
            HashSet<(string, string)> seen,
            List<TaxonomyRow> rows)
        {
            // new XxxError(...) or new XxxError { ... }
            if (argExpr is BaseObjectCreationExpressionSyntax objCreation)
            {
                var typeName = ExtractCreationTypeName(objCreation, semanticModel);
                if (typeName != null && LooksLikeErrorType(typeName))
                    AddIfNew(seen, rows, methodName, typeName, ConfidenceInferred);
                return;
            }

            // Static factory: XxxError.For<T>(...), XxxError.Create(...)
            if (argExpr is InvocationExpressionSyntax factoryCall)
            {
                // Use semantic model to get the return type
                var returnType = semanticModel.GetTypeInfo(factoryCall).Type;
                if (returnType != null && LooksLikeErrorType(returnType.Name))
                {
                    AddIfNew(seen, rows, methodName, returnType.Name, ConfidenceInferred);
                    return;
                }

                // Syntax fallback: XxxError.MethodName(...)
                if (factoryCall.Expression is MemberAccessExpressionSyntax ma &&
                    ma.Expression is IdentifierNameSyntax receiver &&
                    LooksLikeErrorType(receiver.Identifier.ValueText))
                {
                    AddIfNew(seen, rows, methodName, receiver.Identifier.ValueText, ConfidenceInferred);
                }
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string? ExtractCreationTypeName(
            BaseObjectCreationExpressionSyntax creation,
            SemanticModel semanticModel)
        {
            // Prefer semantic type for accuracy
            var typeInfo = semanticModel.GetTypeInfo(creation);
            if (typeInfo.Type != null)
                return typeInfo.Type.Name;

            // Syntax fallback for new XxxError(...)
            if (creation is ObjectCreationExpressionSyntax oc)
            {
                if (oc.Type is IdentifierNameSyntax id)
                    return id.Identifier.ValueText;
                if (oc.Type is QualifiedNameSyntax qn)
                    return qn.Right.Identifier.ValueText;
                if (oc.Type is GenericNameSyntax gn)
                    return gn.Identifier.ValueText;
            }

            return null;
        }

        /// <summary>
        /// Heuristic: a type name ending in "Error" or "Exception" is treated as an error type.
        /// This is the library-agnostic substitute for checking IError interface.
        /// </summary>
        private static bool LooksLikeErrorType(string typeName)
        {
            return typeName.EndsWith("Error", System.StringComparison.Ordinal)
                || typeName.EndsWith("Exception", System.StringComparison.Ordinal);
        }

        private static bool ReturnTypeLooksLikeResult(TypeSyntax returnType)
        {
            var name = returnType switch
            {
                IdentifierNameSyntax id => id.Identifier.ValueText,
                GenericNameSyntax gn => gn.Identifier.ValueText,
                QualifiedNameSyntax qn when qn.Right is IdentifierNameSyntax ri => ri.Identifier.ValueText,
                QualifiedNameSyntax qn when qn.Right is GenericNameSyntax rg => rg.Identifier.ValueText,
                // Unwrap Task<T>
                _ => null
            };

            if (name == "Task" && returnType is GenericNameSyntax taskGeneric &&
                taskGeneric.TypeArgumentList.Arguments.Count == 1)
            {
                return ReturnTypeLooksLikeResult(taskGeneric.TypeArgumentList.Arguments[0]);
            }

            return name != null && name.Contains("Result");
        }

        private static void AddIfNew(
            HashSet<(string, string)> seen,
            List<TaxonomyRow> rows,
            string methodName,
            string errorType,
            string confidence)
        {
            if (seen.Add((methodName, errorType)))
                rows.Add(new TaxonomyRow(methodName, errorType, confidence));
        }

        private static ITypeSymbol UnwrapTask(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol { Name: "Task" } task && task.TypeArguments.Length == 1)
                return task.TypeArguments[0];
            return type;
        }
    }
}
