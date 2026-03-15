using System;

namespace REslava.Result;

/// <summary>
/// Marks a method as a domain boundary crossing point.
/// </summary>
/// <remarks>
/// When a <c>Result&lt;T, TError&gt;</c> with a typed error union is passed to a
/// <c>[DomainBoundary]</c>-marked method without calling <c>.MapError()</c> first,
/// the RESL1030 analyzer reports a warning. This enforces that domain-specific typed
/// error surfaces are translated before crossing architectural layers.
/// </remarks>
/// <example>
/// <code>
/// // Application layer — receives results from Domain, but should not see domain errors directly
/// [DomainBoundary("Application")]
/// public void ProcessOrder(Result&lt;Order, ErrorsOf&lt;DomainError&gt;&gt; result) { ... }
///
/// // Caller must map first:
/// ProcessOrder(domainResult.MapError(e => new AppError(e.Message)));
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false)]
public sealed class DomainBoundaryAttribute : Attribute
{
    /// <summary>Optional layer label for diagram annotation (e.g. "Application", "Infrastructure").</summary>
    public string? Layer { get; }

    /// <summary>Marks this method as a domain boundary.</summary>
    public DomainBoundaryAttribute() { }

    /// <summary>Marks this method as a domain boundary with a named layer label.</summary>
    /// <param name="layer">The architectural layer this method belongs to (e.g. "Application").</param>
    public DomainBoundaryAttribute(string layer) { Layer = layer; }
}
