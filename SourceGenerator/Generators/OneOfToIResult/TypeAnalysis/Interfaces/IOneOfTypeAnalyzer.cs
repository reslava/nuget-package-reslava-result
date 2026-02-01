using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Core.OneOf.Models;

namespace REslava.Result.SourceGenerators.Generators.OneOfToIResult.TypeAnalysis.Interfaces;

/// <summary>
/// Interface for analyzing OneOf types and extracting type information.
/// Following the same pattern as IResultTypeAnalyzer.
/// </summary>
public interface IOneOfTypeAnalyzer
{
    /// <summary>
    /// Analyzes a OneOf type and extracts its type information.
    /// </summary>
    /// <param name="oneOfTypeSymbol">The OneOf type symbol to analyze.</param>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>Information about the OneOf type, or null if not a valid OneOf type.</returns>
    OneOfTypeInfo? AnalyzeOneOfType(INamedTypeSymbol oneOfTypeSymbol, Compilation compilation);

    /// <summary>
    /// Finds all OneOf types in the compilation.
    /// </summary>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>All OneOf types found in the compilation.</returns>
    IEnumerable<INamedTypeSymbol> FindOneOfTypes(Compilation compilation);

    /// <summary>
    /// Validates if a type should be processed for generation.
    /// </summary>
    /// <param name="oneOfTypeSymbol">The OneOf type symbol to validate.</param>
    /// <param name="compilation">The compilation context.</param>
    /// <returns>True if the type should be processed, false otherwise.</returns>
    bool ShouldProcessType(INamedTypeSymbol oneOfTypeSymbol, Compilation compilation);
}
