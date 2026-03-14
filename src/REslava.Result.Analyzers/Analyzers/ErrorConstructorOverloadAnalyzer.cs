using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace REslava.Result.Analyzers.Analyzers
{
    /// <summary>
    /// RESL1021: warns when an IError or IReason implementation has a public constructor
    /// with 2 or more required (non-optional) parameters.
    ///
    /// Allowed shapes: (), (string), (string, Exception).
    /// [Obsolete]-marked constructors and non-public constructors are exempt.
    ///
    /// Rationale: multi-arg public constructors prevent [CallerMemberName] capture on
    /// factory methods, which is the recommended pattern for diagnostic metadata.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ErrorConstructorOverloadAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Descriptors.RESL1021_MultiArgErrorConstructor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var iError  = compilationContext.Compilation.GetTypeByMetadataName("REslava.Result.IError");
                var iReason = compilationContext.Compilation.GetTypeByMetadataName("REslava.Result.IReason");

                if (iError == null && iReason == null)
                    return;

                var exceptionType = compilationContext.Compilation
                    .GetTypeByMetadataName("System.Exception");

                compilationContext.RegisterSymbolAction(
                    ctx => AnalyzeNamedType(ctx, iError, iReason, exceptionType),
                    SymbolKind.NamedType);
            });
        }

        private static void AnalyzeNamedType(
            SymbolAnalysisContext context,
            INamedTypeSymbol? iError,
            INamedTypeSymbol? iReason,
            INamedTypeSymbol? exceptionType)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (type.TypeKind != TypeKind.Class)
                return;

            if (!ImplementsInterface(type, iError) && !ImplementsInterface(type, iReason))
                return;

            foreach (var ctor in type.InstanceConstructors)
            {
                if (ctor.DeclaredAccessibility != Accessibility.Public)
                    continue;

                if (ctor.GetAttributes().Any(a =>
                    a.AttributeClass?.Name is "ObsoleteAttribute" or "Obsolete"))
                    continue;

                var required = ctor.Parameters
                    .Where(p => !p.IsOptional && !p.IsParams)
                    .ToArray();

                // Allowed: ()
                if (required.Length == 0) continue;
                // Allowed: (string)
                if (required.Length == 1 && IsString(required[0])) continue;
                // Allowed: (string, Exception)
                if (required.Length == 2 && IsString(required[0]) &&
                    IsExceptionOrDerived(required[1], exceptionType)) continue;

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.RESL1021_MultiArgErrorConstructor,
                        ctor.Locations[0],
                        type.Name));
            }
        }

        private static bool ImplementsInterface(INamedTypeSymbol type, INamedTypeSymbol? iface)
            => iface != null &&
               type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, iface));

        private static bool IsString(IParameterSymbol param)
            => param.Type.SpecialType == SpecialType.System_String;

        private static bool IsExceptionOrDerived(IParameterSymbol param, INamedTypeSymbol? exceptionType)
        {
            if (exceptionType == null || param.Type is not INamedTypeSymbol paramType)
                return false;

            var t = paramType;
            while (t != null)
            {
                if (SymbolEqualityComparer.Default.Equals(t, exceptionType))
                    return true;
                t = t.BaseType;
            }
            return false;
        }
    }
}
