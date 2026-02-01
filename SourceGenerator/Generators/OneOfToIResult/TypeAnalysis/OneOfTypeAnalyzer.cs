using Microsoft.CodeAnalysis;
using REslava.Result.SourceGenerators.Core.OneOf.Models;
using REslava.Result.SourceGenerators.Core.OneOf.Utilities;
using REslava.Result.SourceGenerators.Generators.OneOfToIResult.TypeAnalysis.Interfaces;
using System.Collections.Generic;

namespace REslava.Result.SourceGenerators.Generators.OneOfToIResult.TypeAnalysis;

/// <summary>
/// Visitor for finding all types in namespaces.
/// </summary>
internal class NamespaceTypeVisitor : SymbolVisitor
{
    public List<INamedTypeSymbol> Types { get; } = new();

    public override void VisitNamespace(INamespaceSymbol namespaceSymbol)
    {
        // Visit all types in this namespace
        foreach (var typeSymbol in namespaceSymbol.GetTypeMembers())
        {
            Types.Add(typeSymbol);
        }

        // Visit child namespaces
        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            VisitNamespace(childNamespace);
        }
    }
}

/// <summary>
/// Analyzes OneOf types and extracts type information.
/// Single Responsibility: Only analyzes OneOf types.
/// Following the same pattern as ResultTypeAnalyzer.
/// </summary>
public class OneOfTypeAnalyzer : IOneOfTypeAnalyzer
{
    /// <summary>
    /// Analyzes a OneOf type and extracts its type information.
    /// </summary>
    public OneOfTypeInfo? AnalyzeOneOfType(INamedTypeSymbol oneOfTypeSymbol, Compilation compilation)
    {
        if (!OneOfTypeHelper.IsOneOfType(oneOfTypeSymbol))
            return null;

        var typeArguments = OneOfTypeHelper.ExtractTypeArguments(oneOfTypeSymbol);
        
        // For Phase 1, we'll focus on simple OneOf<T1, T2>
        if (typeArguments.Count != 2)
        {
            // TODO: In Phase 2, support more complex OneOf types
            return null;
        }

        return new OneOfTypeInfo
        {
            OneOfType = oneOfTypeSymbol,
            TypeArguments = typeArguments
        };
    }

    /// <summary>
    /// Finds all constructed OneOf types in the compilation.
    /// This looks for method return types that are constructed OneOf types.
    /// </summary>
    public IEnumerable<INamedTypeSymbol> FindOneOfTypes(Compilation compilation)
    {
        var oneOfTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        
        // Look through all types in the compilation
        var namespaceVisitor = new NamespaceTypeVisitor();
        namespaceVisitor.VisitNamespace(compilation.GlobalNamespace);
        
        foreach (var typeSymbol in namespaceVisitor.Types)
        {
            // Look through all methods in each type
            foreach (var methodSymbol in typeSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                // Check if the return type is a constructed OneOf type
                if (methodSymbol.ReturnType is INamedTypeSymbol returnType && 
                    returnType.Name.StartsWith("OneOf") && 
                    returnType.TypeArguments.Length >= 2)
                {
                    // Only add if it's a constructed type (has actual type arguments, not generic parameters)
                    if (HasConcreteTypeArguments(returnType))
                    {
                        oneOfTypes.Add(returnType);
                    }
                }
            }
        }
        
        return oneOfTypes;
    }

    /// <summary>
    /// Checks if the type arguments are concrete types, not generic parameters.
    /// </summary>
    private static bool HasConcreteTypeArguments(INamedTypeSymbol typeSymbol)
    {
        foreach (var typeArg in typeSymbol.TypeArguments)
        {
            // If any type argument is a generic parameter (like T0, T1), skip it
            if (typeArg.TypeKind == TypeKind.TypeParameter)
            {
                System.Diagnostics.Debug.WriteLine($"Type argument {typeArg.Name} is a generic parameter, skipping.");
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Debug method to show type argument details (temporary)
    /// </summary>
    private static string DebugTypeArguments(INamedTypeSymbol typeSymbol)
    {
        var debugInfo = new List<string>();
        foreach (var typeArg in typeSymbol.TypeArguments)
        {
            debugInfo.Add($"{typeArg.Name} ({typeArg.TypeKind})");
        }
        return string.Join(", ", debugInfo);
    }

    /// <summary>
    /// Validates if a type should be processed for generation.
    /// </summary>
    public bool ShouldProcessType(INamedTypeSymbol oneOfTypeSymbol, Compilation compilation)
    {
        // Basic validation for Phase 1
        if (!OneOfTypeHelper.IsOneOfType(oneOfTypeSymbol))
            return false;

        var typeArguments = OneOfTypeHelper.ExtractTypeArguments(oneOfTypeSymbol);
        
        // Phase 1: Only process simple OneOf<T1, T2>
        if (typeArguments.Count != 2)
            return false;

        // TODO: Add more validation in Phase 2
        return true;
    }

    /// <summary>
    /// Symbol visitor to find OneOf types in the compilation.
    /// </summary>
    private class OneOfTypeSymbolVisitor : SymbolVisitor
    {
        private readonly List<INamedTypeSymbol> _oneOfTypes = new();

        public IReadOnlyList<INamedTypeSymbol> OneOfTypes => _oneOfTypes;

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            // Visit all namespace members
            foreach (var member in symbol.GetMembers())
            {
                member.Accept(this);
            }
            
            // Visit child namespaces
            foreach (var childNamespace in symbol.GetNamespaceMembers())
            {
                childNamespace.Accept(this);
            }
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            // Check if this type uses OneOf in any way
            if (OneOfTypeHelper.IsOneOfType(symbol))
            {
                _oneOfTypes.Add(symbol);
            }

            // Also check nested types
            foreach (var member in symbol.GetMembers())
            {
                member.Accept(this);
            }
        }
    }
}
