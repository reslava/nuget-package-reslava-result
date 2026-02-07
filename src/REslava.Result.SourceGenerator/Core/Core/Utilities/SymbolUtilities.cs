using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace REslava.Result.SourceGenerators.Core.Utilities
{
    /// <summary>
    /// Utilities for working with Roslyn ISymbol objects.
    /// Provides helper methods for common symbol operations.
    /// </summary>
    public static class SymbolUtilities
    {
        /// <summary>
        /// Checks if a type symbol implements the specified interface.
        /// </summary>
        public static bool ImplementsInterface(this ITypeSymbol symbol, string interfaceFullName)
        {
            return symbol.AllInterfaces.Any(i => i.ToDisplayString() == interfaceFullName);
        }

        /// <summary>
        /// Checks if a type symbol inherits from the specified base class.
        /// </summary>
        public static bool InheritsFrom(this ITypeSymbol symbol, string baseClassFullName)
        {
            var current = symbol.BaseType;
            while (current != null)
            {
                if (current.ToDisplayString() == baseClassFullName)
                    return true;
                current = current.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Gets all attributes applied to a symbol.
        /// </summary>
        public static IEnumerable<AttributeData> GetAttributes(this ISymbol symbol)
        {
            return symbol.GetAttributes();
        }

        /// <summary>
        /// Gets attributes of a specific type applied to a symbol.
        /// </summary>
        public static IEnumerable<AttributeData> GetAttributesOfType(this ISymbol symbol, string attributeFullName)
        {
            return symbol.GetAttributes()
                .Where(a => a.AttributeClass?.ToDisplayString() == attributeFullName);
        }

        /// <summary>
        /// Checks if a symbol has an attribute of the specified type.
        /// </summary>
        public static bool HasAttribute(this ISymbol symbol, string attributeFullName)
        {
            return symbol.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == attributeFullName);
        }

        /// <summary>
        /// Gets the namespace of a symbol as a string.
        /// </summary>
        public static string? GetNamespace(this ISymbol symbol)
        {
            return symbol.ContainingNamespace?.ToDisplayString();
        }

        /// <summary>
        /// Gets all public instance methods of a type.
        /// </summary>
        public static IEnumerable<IMethodSymbol> GetPublicInstanceMethods(this ITypeSymbol symbol)
        {
            return symbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.DeclaredAccessibility == Accessibility.Public && !m.IsStatic);
        }

        /// <summary>
        /// Gets all public properties of a type.
        /// </summary>
        public static IEnumerable<IPropertySymbol> GetPublicProperties(this ITypeSymbol symbol)
        {
            return symbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public);
        }

        /// <summary>
        /// Checks if a type is a class (not interface, struct, etc.).
        /// </summary>
        public static bool IsClass(this ITypeSymbol symbol)
        {
            return symbol.TypeKind == TypeKind.Class;
        }

        /// <summary>
        /// Checks if a type is an interface.
        /// </summary>
        public static bool IsInterface(this ITypeSymbol symbol)
        {
            return symbol.TypeKind == TypeKind.Interface;
        }

        /// <summary>
        /// Checks if a type is abstract.
        /// </summary>
        public static bool IsAbstract(this ITypeSymbol symbol)
        {
            return symbol.IsAbstract;
        }

        /// <summary>
        /// Gets the full name with namespace of a symbol.
        /// </summary>
        public static string GetFullName(this ISymbol symbol)
        {
            return symbol.ToDisplayString();
        }

        /// <summary>
        /// Gets the simple name without namespace of a symbol.
        /// </summary>
        public static string GetSimpleName(this ISymbol symbol)
        {
            return symbol.Name;
        }

        /// <summary>
        /// Finds a type by its full name in the compilation.
        /// </summary>
        public static INamedTypeSymbol? FindTypeByFullName(Compilation compilation, string fullName)
        {
            return compilation.GetTypeByMetadataName(fullName);
        }

        /// <summary>
        /// Gets all types in a compilation that match a predicate.
        /// </summary>
        public static IEnumerable<INamedTypeSymbol> GetAllTypes(
            Compilation compilation,
            System.Func<INamedTypeSymbol, bool>? predicate = null)
        {
            var visitor = new TypeCollectorVisitor(predicate);
            
            foreach (var tree in compilation.SyntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(tree);
                var root = tree.GetRoot();
                visitor.Visit(root, semanticModel);
            }

            return visitor.Types;
        }

        private class TypeCollectorVisitor
        {
            private readonly System.Func<INamedTypeSymbol, bool>? _predicate;
            private readonly List<INamedTypeSymbol> _types = new();

            public TypeCollectorVisitor(System.Func<INamedTypeSymbol, bool>? predicate)
            {
                _predicate = predicate;
            }

            public IEnumerable<INamedTypeSymbol> Types => _types;

            public void Visit(SyntaxNode node, SemanticModel semanticModel)
            {
                if (node is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax classDecl)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                    if (symbol != null && (_predicate == null || _predicate(symbol)))
                    {
                        _types.Add(symbol);
                    }
                }

                foreach (var child in node.ChildNodes())
                {
                    Visit(child, semanticModel);
                }
            }
        }
    }
}
