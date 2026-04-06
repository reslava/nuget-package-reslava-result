using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using REslava.Result.Flow.Generators.ResultFlow.CodeGeneration;
using System.Collections.Generic;
using System.Linq;

namespace REslava.Result.Flow.Generators.ErrorTaxonomy
{
    /// <summary>
    /// Scans all public methods in a class for possible error types using the semantic model.
    /// Two passes:
    /// <list type="bullet">
    ///   <item><c>Result&lt;T, TError&gt;</c> return type → error type from generic param (certain)</item>
    ///   <item><c>Result&lt;T&gt;</c> return type → body scan for <c>Fail(new XxxError())</c> /
    ///     <c>Ensure(..., new XxxError())</c> constructions (inferred)</item>
    /// </list>
    /// </summary>
    internal static class ErrorTaxonomyScanner
    {
        public const string ConfidenceCertain = "certain";
        public const string ConfidenceInferred = "inferred";

        /// <summary>Represents one (method, errorType, confidence) row in the taxonomy.</summary>
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
            SemanticModel semanticModel,
            Compilation compilation,
            INamedTypeSymbol? iResultBaseSymbol,
            INamedTypeSymbol? iErrorSymbol)
        {
            var seen = new HashSet<(string, string)>();
            var rows = new List<TaxonomyRow>();

            foreach (var methodDecl in classDecl.Members.OfType<MethodDeclarationSyntax>())
            {
                // Only scan public methods
                if (!methodDecl.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword)))
                    continue;

                var methodSymbol = semanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;
                if (methodSymbol == null) continue;

                var returnType = UnwrapTask(methodSymbol.ReturnType);
                if (!(returnType is INamedTypeSymbol named)) continue;

                // Only handle types that implement IResultBase (REslava.Result types)
                if (iResultBaseSymbol != null && !ResultTypeExtractor.ImplementsInterface(named, iResultBaseSymbol))
                    continue;

                var methodName = methodSymbol.Name;

                if (named.TypeArguments.Length >= 2)
                {
                    // Result<T, TError> — second type arg is the certain error type
                    AddIfNew(seen, rows, methodName, named.TypeArguments[1].Name, ConfidenceCertain);
                }
                else
                {
                    // Result<T> — body scan for inferred errors
                    if (iErrorSymbol != null)
                        ScanMethodBody(methodDecl, methodSymbol, semanticModel, compilation, iErrorSymbol, seen, rows);
                }
            }

            rows.Sort((a, b) =>
            {
                var c = string.Compare(a.MethodName, b.MethodName, System.StringComparison.Ordinal);
                return c != 0 ? c : string.Compare(a.ErrorType, b.ErrorType, System.StringComparison.Ordinal);
            });

            return rows;
        }

        // ── Body scan ──────────────────────────────────────────────────────────

        private static void ScanMethodBody(
            MethodDeclarationSyntax methodDecl,
            IMethodSymbol methodSymbol,
            SemanticModel semanticModel,
            Compilation compilation,
            INamedTypeSymbol iErrorSymbol,
            HashSet<(string, string)> seen,
            List<TaxonomyRow> rows)
        {
            var body = (SyntaxNode?)methodDecl.Body ?? methodDecl.ExpressionBody;
            if (body == null) return;

            // Walk all invocations in the method body looking for Fail / Ensure calls
            foreach (var invocation in body.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
            {
                string? invocationName = null;
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                    invocationName = memberAccess.Name.Identifier.ValueText;
                else if (invocation.Expression is IdentifierNameSyntax identName)
                    invocationName = identName.Identifier.ValueText;

                if (invocationName != "Fail" && invocationName != "Ensure")
                    continue;

                // For Ensure, the error is the second argument; for Fail, the first
                var args = invocation.ArgumentList.Arguments;
                var candidateArgs = invocationName == "Ensure"
                    ? args.Skip(1).Take(1)
                    : args.Take(1);

                foreach (var arg in candidateArgs)
                    ExtractErrorFromArgument(arg.Expression, semanticModel, compilation, iErrorSymbol,
                        methodSymbol.Name, seen, rows);
            }
        }

        private static void ExtractErrorFromArgument(
            ExpressionSyntax argExpr,
            SemanticModel semanticModel,
            Compilation compilation,
            INamedTypeSymbol iErrorSymbol,
            string methodName,
            HashSet<(string, string)> seen,
            List<TaxonomyRow> rows)
        {
            // new XxxError(...) directly in the call
            if (argExpr is BaseObjectCreationExpressionSyntax objCreation)
            {
                var typeInfo = semanticModel.GetTypeInfo(objCreation);
                if (typeInfo.Type is INamedTypeSymbol created &&
                    ResultTypeExtractor.ImplementsInterface(created, iErrorSymbol))
                {
                    AddIfNew(seen, rows, methodName, created.Name, ConfidenceInferred);
                }
                return;
            }

            // Static factory call: XxxError.For<T>(...) or XxxError.Create(...)
            if (argExpr is InvocationExpressionSyntax factoryCall)
            {
                var returnType = semanticModel.GetTypeInfo(factoryCall).Type as INamedTypeSymbol;
                if (returnType != null && ResultTypeExtractor.ImplementsInterface(returnType, iErrorSymbol))
                    AddIfNew(seen, rows, methodName, returnType.Name, ConfidenceInferred);
                return;
            }

            // Lambda or method group — scan the referenced body
            var op = semanticModel.GetOperation(argExpr);
            if (op is IDelegateCreationOperation del)
                op = del.Target;

            if (op is IMethodReferenceOperation methodRef)
            {
                foreach (var syntaxRef in methodRef.Method.DeclaringSyntaxReferences)
                {
                    var syntax = syntaxRef.GetSyntax();
                    var sm = compilation.GetSemanticModel(syntax.SyntaxTree);
                    foreach (var creation in syntax.DescendantNodesAndSelf()
                        .OfType<BaseObjectCreationExpressionSyntax>())
                    {
                        var ti = sm.GetTypeInfo(creation);
                        if (ti.Type is INamedTypeSymbol t && ResultTypeExtractor.ImplementsInterface(t, iErrorSymbol))
                            AddIfNew(seen, rows, methodName, t.Name, ConfidenceInferred);
                    }
                }
            }
            else if (op is IAnonymousFunctionOperation lambda)
            {
                var sm = compilation.GetSemanticModel(lambda.Syntax.SyntaxTree);
                foreach (var creation in lambda.Syntax.DescendantNodesAndSelf()
                    .OfType<BaseObjectCreationExpressionSyntax>())
                {
                    var ti = sm.GetTypeInfo(creation);
                    if (ti.Type is INamedTypeSymbol t && ResultTypeExtractor.ImplementsInterface(t, iErrorSymbol))
                        AddIfNew(seen, rows, methodName, t.Name, ConfidenceInferred);
                }
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static bool AddIfNew(
            HashSet<(string, string)> seen,
            List<TaxonomyRow> rows,
            string methodName,
            string errorType,
            string confidence)
        {
            if (!seen.Add((methodName, errorType))) return false;
            rows.Add(new TaxonomyRow(methodName, errorType, confidence));
            return true;
        }

        private static ITypeSymbol UnwrapTask(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol { Name: "Task" } task && task.TypeArguments.Length == 1)
                return task.TypeArguments[0];
            return type;
        }
    }
}
