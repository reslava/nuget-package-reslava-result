using Microsoft.CodeAnalysis;

namespace REslava.Result.SourceGenerators.Core.OneOf.Utilities;

/// <summary>
/// Helper utilities for working with OneOf types.
/// </summary>
public static class OneOfTypeHelper
{
    /// <summary>
    /// Checks if a type is a OneOf type.
    /// </summary>
    public static bool IsOneOfType(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.Name.StartsWith("OneOf") && 
               typeSymbol.TypeArguments.Length >= 2;
    }

    /// <summary>
    /// Extracts the generic type arguments from a OneOf type.
    /// </summary>
    public static IReadOnlyList<ITypeSymbol> ExtractTypeArguments(INamedTypeSymbol oneOfTypeSymbol)
    {
        if (!IsOneOfType(oneOfTypeSymbol))
            return Array.Empty<ITypeSymbol>();

        return oneOfTypeSymbol.TypeArguments;
    }

    /// <summary>
    /// Gets a clean type name for code generation.
    /// </summary>
    public static string GetCleanTypeName(ITypeSymbol typeSymbol)
    {
        // Handle generic types
        if (typeSymbol is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 0)
        {
            var args = string.Join(", ", namedType.TypeArguments.Select(GetCleanTypeName));
            return $"{namedType.Name}<{args}>";
        }

        return typeSymbol.Name;
    }

    /// <summary>
    /// Gets the full namespace-qualified type name.
    /// </summary>
    public static string GetFullTypeName(ITypeSymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString();
    }

    /// <summary>
    /// Checks if a type implements a specific interface.
    /// </summary>
    public static bool ImplementsInterface(ITypeSymbol typeSymbol, string interfaceName)
    {
        return typeSymbol.AllInterfaces
            .Any(i => i.Name.Equals(interfaceName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a type inherits from a specific base class.
    /// </summary>
    public static bool InheritsFrom(ITypeSymbol typeSymbol, string baseClassName)
    {
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if (baseType.Name.Equals(baseClassName, StringComparison.OrdinalIgnoreCase))
                return true;
            baseType = baseType.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Gets the namespace for a type symbol.
    /// </summary>
    public static string GetNamespace(ITypeSymbol typeSymbol)
    {
        return typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
    }

    /// <summary>
    /// Checks if a type is from the current compilation (user code).
    /// </summary>
    public static bool IsUserDefinedType(ITypeSymbol typeSymbol, Compilation compilation)
    {
        return compilation.References
            .Any(r => r.Display == typeSymbol.ContainingAssembly?.Name);
    }
}
