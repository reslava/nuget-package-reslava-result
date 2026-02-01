using Microsoft.CodeAnalysis;

namespace REslava.Result.SourceGenerators.Core.OneOf.Models;

/// <summary>
    /// Represents information about a OneOf type and its generic arguments.
    /// </summary>
public class OneOfTypeInfo
{
    /// <summary>
    /// Gets the OneOf type symbol (e.g., OneOf of T1 and T2).
    /// </summary>
    public INamedTypeSymbol OneOfType { get; set; } = null!;

    /// <summary>
    /// Gets the generic type arguments (T1, T2, ...).
    /// </summary>
    public IReadOnlyList<ITypeSymbol> TypeArguments { get; set; } = Array.Empty<ITypeSymbol>();

    /// <summary>
    /// Gets the full name of the OneOf type.
    /// </summary>
    public string FullName => OneOfType.ToDisplayString();

    /// <summary>
    /// Gets the number of type arguments.
    /// </summary>
    public int TypeArgumentCount => TypeArguments.Count;

    /// <summary>
    /// Gets whether this is a simple OneOf of two types.
    /// </summary>
    public bool IsSimpleTwoType => TypeArguments.Count == 2;

    /// <summary>
    /// Gets whether this is a OneOf of three types.
    /// </summary>
    public bool IsThreeType => TypeArguments.Count == 3;

    /// <summary>
    /// Gets a string representation for debugging.
    /// </summary>
    public override string ToString()
    {
        var args = string.Join(", ", TypeArguments.Select(t => t.Name));
        return $"{OneOfType.Name}<{args}>";
    }
}
