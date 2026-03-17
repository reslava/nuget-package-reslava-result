using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace REslava.ResultFlow.Generators.ResultFlow
{
    /// <summary>
    /// Detects the architectural layer of a method from two sources (in priority order):
    /// <list type="number">
    ///   <item><c>[DomainBoundary("LayerName")]</c> annotation — explicit, always wins.</item>
    ///   <item>Namespace heuristics — Presentation / Application / Domain / Infrastructure.</item>
    /// </list>
    /// Returns null when neither source yields a layer name.
    /// Syntax-only — no semantic model required.
    /// </summary>
    internal static class LayerDetector
    {
        /// <summary>
        /// Detects the layer for <paramref name="method"/> using its attribute list and
        /// <paramref name="containingNamespace"/>.
        /// </summary>
        public static string? Detect(MethodDeclarationSyntax method, string containingNamespace)
        {
            // 1. [DomainBoundary] attribute — explicit layer takes precedence over heuristics.
            foreach (var attrList in method.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    var attrName = GetAttributeSimpleName(attr);
                    if (attrName == "DomainBoundary" || attrName == "DomainBoundaryAttribute")
                    {
                        // [DomainBoundary("Application")] → return the layer string
                        var argExpr = attr.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
                        if (argExpr is LiteralExpressionSyntax lit &&
                            lit.Token.Value is string layer)
                            return layer;

                        // [DomainBoundary] with no arg → boundary marker only, no label
                        return null;
                    }
                }
            }

            // 2. [DomainBoundary] on the containing class — class annotation takes precedence over heuristics.
            if (method.Parent is TypeDeclarationSyntax containingType)
            {
                foreach (var attrList in containingType.AttributeLists)
                {
                    foreach (var attr in attrList.Attributes)
                    {
                        var attrName = GetAttributeSimpleName(attr);
                        if (attrName == "DomainBoundary" || attrName == "DomainBoundaryAttribute")
                        {
                            var argExpr = attr.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
                            if (argExpr is LiteralExpressionSyntax lit &&
                                lit.Token.Value is string classLayer)
                                return classLayer;
                            return null;
                        }
                    }
                }
            }

            // 3. Namespace heuristics fallback.
            return DetectFromNamespace(containingNamespace);
        }

        /// <summary>
        /// Matches a fully-qualified namespace string against the standard layer conventions.
        /// Both segment-contained (e.g. <c>.Application.</c>) and suffix forms
        /// (e.g. <c>ends with .Application</c>) are handled to cover both
        /// <c>MyApp.Application.Orders.OrderService</c> and <c>MyApp.Application</c>.
        /// </summary>
        internal static string? DetectFromNamespace(string ns)
        {
            if (ns.Contains(".Controllers.") || ns.EndsWith(".Controllers"))
                return "Presentation";

            if (ns.Contains(".Application.") || ns.EndsWith(".Application") ||
                ns.Contains(".UseCases.")    || ns.EndsWith(".UseCases"))
                return "Application";

            if (ns.Contains(".Domain.") || ns.EndsWith(".Domain"))
                return "Domain";

            if (ns.Contains(".Infrastructure.") || ns.EndsWith(".Infrastructure") ||
                ns.Contains(".Repositories.")   || ns.EndsWith(".Repositories"))
                return "Infrastructure";

            return null;
        }

        private static string? GetAttributeSimpleName(AttributeSyntax attr)
        {
            return attr.Name switch
            {
                IdentifierNameSyntax id     => id.Identifier.ValueText,
                QualifiedNameSyntax qual    => qual.Right.Identifier.ValueText,
                _                          => null,
            };
        }
    }
}
