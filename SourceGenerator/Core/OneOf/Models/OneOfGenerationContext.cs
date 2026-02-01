using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Generators.OneOfToIResult;

namespace REslava.Result.SourceGenerators.Core.OneOf.Models;

/// <summary>
/// Context for OneOf extension method generation.
/// </summary>
public class OneOfGenerationContext
{
    /// <summary>
    /// Gets the compilation context.
    /// </summary>
    public Compilation Compilation { get; set; } = null!;

    /// <summary>
    /// Gets the OneOf type information.
    /// </summary>
    public OneOfTypeInfo OneOfTypeInfo { get; set; } = null!;

    /// <summary>
    /// Gets the generation configuration.
    /// </summary>
    public OneOfToIResultConfig Config { get; set; } = null!;

    /// <summary>
    /// Gets the mapping results for each type argument.
    /// </summary>
    public IReadOnlyList<OneOfMappingResult> MappingResults { get; set; } = Array.Empty<OneOfMappingResult>();

    /// <summary>
    /// Gets the namespace for generated code.
    /// </summary>
    public string TargetNamespace => Config.DefaultNamespace;

    /// <summary>
    /// Gets the extension method name.
    /// </summary>
    public string ExtensionMethodName => "ToIResult";

    /// <summary>
    /// Gets the generated class name.
    /// </summary>
    public string GeneratedClassName => "OneOfToIResultExtensions";

    /// <summary>
    /// Gets whether this context is valid for generation.
    /// </summary>
    public bool IsValid => 
        Compilation != null && 
        OneOfTypeInfo != null && 
        Config != null && 
        MappingResults.Count > 0;

    /// <summary>
    /// Gets a string representation for debugging.
    /// </summary>
    public override string ToString()
    {
        return $"{OneOfTypeInfo} → {MappingResults.Count} mappings";
    }
}
