namespace REslava.Result;

/// <summary>
/// Carries ambient context through a Result pipeline — entity type, runtime identity,
/// correlation, operation name, and tenant. Seeded automatically by Ok/Fail factories
/// and propagated by all pipeline operators (parent-wins).
/// </summary>
public sealed record ResultContext
{
    /// <summary>Shared empty singleton — used when no context has been set.</summary>
    public static readonly ResultContext Empty = new();

    /// <summary>Domain entity type name (e.g. "Order"). Auto-seeded from typeof(T).Name.</summary>
    public string? Entity { get; init; }

    /// <summary>Runtime entity identifier (e.g. order ID, user ID).</summary>
    public string? EntityId { get; init; }

    /// <summary>Distributed trace / correlation identifier.</summary>
    public string? CorrelationId { get; init; }

    /// <summary>Name of the business operation being executed (e.g. "PlaceOrder").</summary>
    public string? OperationName { get; init; }

    /// <summary>Tenant identifier for multi-tenant scenarios.</summary>
    public string? TenantId { get; init; }
}
