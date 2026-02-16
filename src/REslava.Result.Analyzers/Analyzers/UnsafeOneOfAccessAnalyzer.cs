using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using REslava.Result.Analyzers.Helpers;

namespace REslava.Result.Analyzers.Analyzers
{
    /// <summary>
    /// RESL2001: Warns when OneOf&lt;...&gt;.AsT* is accessed without checking IsT* first.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnsafeOneOfAccessAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Descriptors.RESL2001_UnsafeOneOfAccess);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var oneOfTypes = new[]
                {
                    compilationContext.Compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.OneOf`2"),
                    compilationContext.Compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.OneOf`3"),
                    compilationContext.Compilation.GetTypeByMetadataName("REslava.Result.AdvancedPatterns.OneOf`4"),
                }.Where(t => t != null).ToArray();

                if (oneOfTypes.Length == 0)
                    return;

                compilationContext.RegisterSyntaxNodeAction(
                    ctx => AnalyzeMemberAccess(ctx, oneOfTypes),
                    SyntaxKind.SimpleMemberAccessExpression);
            });
        }

        private static void AnalyzeMemberAccess(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol[] oneOfTypes)
        {
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;
            var memberName = memberAccess.Name.Identifier.Text;

            // Quick check: is this .AsT1, .AsT2, .AsT3, or .AsT4?
            if (!TryParseAsTIndex(memberName, out int index))
                return;

            // Get the type of the expression before .AsT*
            var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken);
            var expressionType = typeInfo.Type as INamedTypeSymbol;

            if (expressionType is null)
                return;

            // Is this a OneOf type?
            if (!IsOneOfType(expressionType, oneOfTypes))
                return;

            // Build guard config: positive = IsT{index}, no negative properties
            var guardConfig = new GuardDetectionHelper.GuardConfig(
                positiveProperties: new[] { $"IsT{index}" },
                negativeProperties: Array.Empty<string>());

            if (GuardDetectionHelper.IsGuardedByCheck(memberAccess, guardConfig))
                return;

            var guardProperty = $"IsT{index}";
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Descriptors.RESL2001_UnsafeOneOfAccess,
                    memberAccess.Name.GetLocation(),
                    memberName, guardProperty));
        }

        private static bool TryParseAsTIndex(string name, out int index)
        {
            // Matches AsT1, AsT2, AsT3, AsT4
            if (name.Length == 4 && name.StartsWith("AsT") && char.IsDigit(name[3]))
            {
                index = name[3] - '0';
                return index >= 1 && index <= 4;
            }
            index = 0;
            return false;
        }

        private static bool IsOneOfType(INamedTypeSymbol type, INamedTypeSymbol[] oneOfTypes)
        {
            if (!type.IsGenericType)
                return false;

            return oneOfTypes.Any(ot =>
                SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, ot));
        }
    }
}
