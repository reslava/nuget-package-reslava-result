using Microsoft.CodeAnalysis;

namespace REslava.Result.Flow.Generators.ResultFlow
{
    /// <summary>
    /// Detects the architectural layer of a method from two sources (in priority order):
    /// <list type="number">
    ///   <item><c>[DomainBoundary("LayerName")]</c> annotation — explicit, always wins.</item>
    ///   <item>Namespace heuristics — Presentation / Application / Domain / Infrastructure.</item>
    /// </list>
    /// Returns null when neither source yields a layer name.
    /// </summary>
    internal static class LayerDetector
    {
        /// <summary>
        /// Detects the layer for <paramref name="method"/> using its attributes and containing namespace.
        /// </summary>
        public static string? Detect(IMethodSymbol method)
        {
            // 1. [DomainBoundary] attribute — explicit layer takes precedence over heuristics.
            foreach (var attr in method.GetAttributes())
            {
                if (attr.AttributeClass?.Name == "DomainBoundaryAttribute")
                {
                    // [DomainBoundary("Application")] → return the layer string
                    if (attr.ConstructorArguments.Length > 0 &&
                        attr.ConstructorArguments[0].Value is string layer)
                        return layer;

                    // [DomainBoundary] with no arg → boundary marker only, no label
                    return null;
                }
            }

            // 2. [DomainBoundary] on the containing class — class annotation takes precedence over heuristics.
            if (method.ContainingType != null)
            {
                foreach (var attr in method.ContainingType.GetAttributes())
                {
                    if (attr.AttributeClass?.Name == "DomainBoundaryAttribute")
                    {
                        if (attr.ConstructorArguments.Length > 0 &&
                            attr.ConstructorArguments[0].Value is string classLayer)
                            return classLayer;
                        return null;
                    }
                }
            }

            // 3. Namespace heuristics fallback.
            var ns = method.ContainingNamespace?.ToDisplayString() ?? string.Empty;
            return DetectFromNamespace(ns);
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
    }
}
