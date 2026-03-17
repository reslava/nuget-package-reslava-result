using System;

namespace REslava.Result;

/// <summary>
/// Marks a method or class as a domain boundary crossing point.
/// When applied to a class, all methods in the class inherit the layer annotation.
/// </summary>
/// <remarks>
/// When a <c>Result&lt;T, TError&gt;</c> with a typed error union is passed to a
/// <c>[DomainBoundary]</c>-marked method without calling <c>.MapError()</c> first,
/// the RESL1030 analyzer reports a warning. This enforces that domain-specific typed
/// error surfaces are translated before crossing architectural layers.
/// <para>
/// The optional <c>layer</c> parameter is also used by <c>REslava.Result.Flow</c> source
/// generator to annotate diagram nodes with their architectural layer
/// (<c>_LayerView</c>, <c>_Stats</c>, <c>_ErrorSurface</c>, <c>_ErrorPropagation</c>).
/// Priority order: method annotation → class annotation → namespace heuristics.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Class-level annotation — all methods in UserService are tagged as "Domain"
/// [DomainBoundary("Domain")]
/// static class UserService { ... }
///
/// // Method-level annotation — takes precedence over class annotation
/// [DomainBoundary("Application")]
/// public void ProcessOrder(Result&lt;Order, ErrorsOf&lt;DomainError&gt;&gt; result) { ... }
///
/// // Caller must map first:
/// ProcessOrder(domainResult.MapError(e => new AppError(e.Message)));
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
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
