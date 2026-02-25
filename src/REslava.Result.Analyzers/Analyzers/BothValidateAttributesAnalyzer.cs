using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace REslava.Result.Analyzers.Analyzers
{
    /// <summary>
    /// RESL1006: Reports an error when both [Validate] and [FluentValidate] are applied
    /// to the same type. They generate conflicting .Validate() extension methods.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BothValidateAttributesAnalyzer : DiagnosticAnalyzer
    {
        private const string ValidateAttributeFqn = "REslava.Result.SourceGenerators.ValidateAttribute";
        private const string FluentValidateAttributeFqn = "REslava.Result.FluentValidation.FluentValidateAttribute";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Descriptors.RESL1006_BothValidateAttributes);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var typeSymbol = (INamedTypeSymbol)context.Symbol;
            var attributes = typeSymbol.GetAttributes();

            var hasValidate = attributes.Any(a =>
                a.AttributeClass?.ToDisplayString() == ValidateAttributeFqn);
            var hasFluentValidate = attributes.Any(a =>
                a.AttributeClass?.ToDisplayString() == FluentValidateAttributeFqn);

            if (!hasValidate || !hasFluentValidate)
                return;

            // Report on each declaration location (partial classes may have multiple)
            foreach (var location in typeSymbol.Locations)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.RESL1006_BothValidateAttributes,
                        location,
                        typeSymbol.Name));
            }
        }
    }
}
