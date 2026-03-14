using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace REslava.Result.Analyzers.Analyzers
{
    /// <summary>
    /// RESL2002: Warns when ErrorsOf&lt;T1..Tn&gt;.Match() is called with fewer handler lambdas than type arguments.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExhaustiveMatchAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Descriptors.RESL2002_ExhaustiveMatch);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var errorsOfTypes = new[]
                {
                    compilationContext.Compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.ErrorsOf`2"),
                    compilationContext.Compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.ErrorsOf`3"),
                    compilationContext.Compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.ErrorsOf`4"),
                    compilationContext.Compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.ErrorsOf`5"),
                    compilationContext.Compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.ErrorsOf`6"),
                    compilationContext.Compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.ErrorsOf`7"),
                    compilationContext.Compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.ErrorsOf`8"),
                }.Where(t => t != null).ToArray();

                if (errorsOfTypes.Length == 0)
                    return;

                compilationContext.RegisterSyntaxNodeAction(
                    ctx => AnalyzeInvocation(ctx, errorsOfTypes),
                    SyntaxKind.InvocationExpression);
            });
        }

        private static void AnalyzeInvocation(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol[] errorsOfTypes)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            // Must be a member access: receiver.Match(...)
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                return;

            // Must be .Match
            if (memberAccess.Name.Identifier.Text != "Match")
                return;

            // Resolve receiver type
            var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken);
            if (typeInfo.Type is not INamedTypeSymbol receiverType || !receiverType.IsGenericType)
                return;

            // Is the receiver an ErrorsOf<...>?
            if (!errorsOfTypes.Any(t => SymbolEqualityComparer.Default.Equals(receiverType.OriginalDefinition, t)))
                return;

            int arity = receiverType.TypeArguments.Length;
            int handlerCount = invocation.ArgumentList.Arguments.Count;

            if (handlerCount >= arity)
                return;

            var typeArgs = string.Join(", ", receiverType.TypeArguments.Select(t => t.Name));

            context.ReportDiagnostic(
                Diagnostic.Create(
                    Descriptors.RESL2002_ExhaustiveMatch,
                    memberAccess.Name.GetLocation(),
                    typeArgs,
                    handlerCount,
                    arity));
        }
    }
}
