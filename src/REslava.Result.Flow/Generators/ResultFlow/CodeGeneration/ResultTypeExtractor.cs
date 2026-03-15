using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace REslava.Result.Flow.Generators.ResultFlow.CodeGeneration
{
    /// <summary>
    /// Extracts success type and possible error types from pipeline step operations.
    /// Uses IResultBase as the Roslyn anchor for success types and IError body scanning for errors.
    /// </summary>
    internal static class ResultTypeExtractor
    {
        /// <summary>
        /// Returns the T in Result&lt;T&gt; from an IInvocationOperation's return type,
        /// using IResultBase as the anchor. Returns null if not a REslava.Result type.
        /// </summary>
        public static string? GetSuccessType(
            IInvocationOperation step,
            INamedTypeSymbol resultBaseSymbol)
        {
            var returnType = step.Type;
            if (returnType is null) return null;

            // Unwrap Task<T> → T
            if (returnType is INamedTypeSymbol { Name: "Task" } taskType &&
                taskType.TypeArguments.Length == 1)
                returnType = taskType.TypeArguments[0];

            if (!(returnType is INamedTypeSymbol named)) return null;

            // Must implement IResultBase (confirms this is a REslava.Result type)
            if (!ImplementsInterface(named, resultBaseSymbol)) return null;

            // Generic Result<T>: return the first type argument name
            if (named.TypeArguments.Length >= 1)
                return named.TypeArguments[0].Name;

            return null;
        }

        /// <summary>
        /// Scans arguments (method groups and lambdas) of a pipeline step for IError constructions.
        /// Best-effort: only scans methods in the same compilation; lambdas are scanned inline.
        /// </summary>
        public static IReadOnlyCollection<string> GetPossibleErrors(
            IInvocationOperation step,
            Compilation compilation,
            INamedTypeSymbol iErrorSymbol)
        {
            var errors = new HashSet<string>();

            foreach (var argument in step.Arguments)
                CollectErrors(argument.Value, compilation, iErrorSymbol, errors);

            return errors;
        }

        private static void CollectErrors(
            IOperation operand,
            Compilation compilation,
            INamedTypeSymbol iErrorSymbol,
            HashSet<string> errors)
        {
            // Unwrap delegate creation wrapping a method reference or lambda
            if (operand is IDelegateCreationOperation delegateCreation)
            {
                CollectErrors(delegateCreation.Target, compilation, iErrorSymbol, errors);
                return;
            }

            // Method group → scan the referenced method's body
            if (operand is IMethodReferenceOperation methodRef)
            {
                foreach (var syntaxRef in methodRef.Method.DeclaringSyntaxReferences)
                {
                    var syntax = syntaxRef.GetSyntax();
                    var sm = compilation.GetSemanticModel(syntax.SyntaxTree);
                    ScanSyntaxForErrors(syntax, sm, iErrorSymbol, errors);
                }
                return;
            }

            // Anonymous function (lambda) → scan the lambda body directly
            if (operand is IAnonymousFunctionOperation lambda)
            {
                var sm = compilation.GetSemanticModel(lambda.Syntax.SyntaxTree);
                ScanSyntaxForErrors(lambda.Syntax, sm, iErrorSymbol, errors);
                return;
            }
        }

        private static void ScanSyntaxForErrors(
            SyntaxNode body,
            SemanticModel semanticModel,
            INamedTypeSymbol iErrorSymbol,
            HashSet<string> errors)
        {
            // Object creations: new NotFoundError(...), new ValidationError(...)
            foreach (var creation in body.DescendantNodesAndSelf()
                .OfType<BaseObjectCreationExpressionSyntax>())
            {
                var typeInfo = semanticModel.GetTypeInfo(creation);
                if (typeInfo.Type is INamedTypeSymbol createdType &&
                    ImplementsInterface(createdType, iErrorSymbol))
                {
                    errors.Add(createdType.Name);
                }
            }

            // Static factory calls: NotFoundError.For<Order>(...), ValidationError.Field(...)
            // Use the semantic return type to confirm the result implements IError.
            foreach (var invocation in body.DescendantNodesAndSelf()
                .OfType<InvocationExpressionSyntax>())
            {
                if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
                    continue;

                var returnType = semanticModel.GetTypeInfo(invocation).Type as INamedTypeSymbol;
                if (returnType == null || !ImplementsInterface(returnType, iErrorSymbol))
                    continue;

                // If the factory method has a generic type argument (e.g. For<Order>),
                // extract the entity name for a richer edge label: "NotFoundError<Order>".
                var methodName = memberAccess.Name;
                if (methodName is GenericNameSyntax genericMethod &&
                    genericMethod.TypeArgumentList.Arguments.Count == 1 &&
                    genericMethod.TypeArgumentList.Arguments[0] is IdentifierNameSyntax entityType)
                {
                    errors.Add($"{returnType.Name}<{entityType.Identifier.ValueText}>");
                }
                else
                {
                    errors.Add(returnType.Name);
                }
            }
        }

        /// <summary>
        /// Checks whether <paramref name="type"/> implements <paramref name="interfaceSymbol"/>,
        /// comparing original definitions to handle generic interfaces.
        /// </summary>
        internal static bool ImplementsInterface(ITypeSymbol type, INamedTypeSymbol interfaceSymbol)
        {
            return type.AllInterfaces.Any(i =>
                SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, interfaceSymbol));
        }
    }
}
