using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace REslava.Result.Analyzers.Analyzers
{
    /// <summary>
    /// RESL1030: warns when a <c>Result&lt;T, TError&gt;</c> with a typed error union is passed
    /// directly to a method marked <c>[DomainBoundary]</c> without calling <c>.MapError()</c> first.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DomainBoundaryAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Descriptors.RESL1030_DomainBoundaryTypedErrorCrossing);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var resultTError = compilationContext.Compilation
                    .GetTypeByMetadataName("REslava.Result.Result`2");
                var boundaryAttr = compilationContext.Compilation
                    .GetTypeByMetadataName("REslava.Result.DomainBoundaryAttribute");

                if (resultTError is null || boundaryAttr is null)
                    return;

                compilationContext.RegisterSyntaxNodeAction(
                    ctx => AnalyzeInvocation(ctx, resultTError, boundaryAttr),
                    SyntaxKind.InvocationExpression);
            });
        }

        private static void AnalyzeInvocation(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol resultTErrorDef,
            INamedTypeSymbol boundaryAttrType)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            var methodSymbol = context.SemanticModel
                .GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;

            if (methodSymbol is null)
                return;

            if (!HasDomainBoundaryAttribute(methodSymbol, boundaryAttrType))
                return;

            foreach (var arg in invocation.ArgumentList.Arguments)
            {
                var typeInfo = context.SemanticModel
                    .GetTypeInfo(arg.Expression, context.CancellationToken);

                if (typeInfo.Type is not INamedTypeSymbol argType || !argType.IsGenericType)
                    continue;

                if (!SymbolEqualityComparer.Default.Equals(argType.OriginalDefinition, resultTErrorDef))
                    continue;

                // It's a Result<T, TError> — report with method name and error type
                var errorTypeName = argType.TypeArguments[1].Name;

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.RESL1030_DomainBoundaryTypedErrorCrossing,
                        arg.Expression.GetLocation(),
                        methodSymbol.Name,
                        errorTypeName));
            }
        }

        private static bool HasDomainBoundaryAttribute(
            IMethodSymbol method,
            INamedTypeSymbol boundaryAttrType)
        {
            foreach (var attr in method.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, boundaryAttrType))
                    return true;
            }
            return false;
        }
    }
}
