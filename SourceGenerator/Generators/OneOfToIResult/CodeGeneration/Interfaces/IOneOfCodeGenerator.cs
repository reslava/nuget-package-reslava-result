using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Core.OneOf.Models;

namespace REslava.Result.SourceGenerators.Generators.OneOfToIResult.CodeGeneration.Interfaces;

/// <summary>
/// Interface for generating OneOf extension method code.
/// Following the same pattern as ICodeGenerator.
/// </summary>
public interface IOneOfCodeGenerator
{
    /// <summary>
    /// Generates extension method code for a OneOf type.
    /// </summary>
    /// <param name="context">The generation context containing all necessary information.</param>
    /// <returns>The generated source code.</returns>
    string GenerateExtensionMethods(OneOfGenerationContext context);

    /// <summary>
    /// Generates the class structure for the extensions.
    /// </summary>
    /// <param name="context">The generation context.</param>
    /// <returns>The generated class code.</returns>
    string GenerateClassStructure(OneOfGenerationContext context);

    /// <summary>
    /// Generates using statements for the generated code.
    /// </summary>
    /// <param name="context">The generation context.</param>
    /// <returns>The generated using statements.</returns>
    string GenerateUsingStatements(OneOfGenerationContext context);
}
