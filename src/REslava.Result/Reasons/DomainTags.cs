namespace REslava.Result;

/// <summary>
/// Predefined <see cref="TagKey{T}"/> constants for domain error context.
/// Used by built-in error factories and available for custom errors and queries.
/// </summary>
/// <example>
/// <code>
/// // Read with type safety
/// if (error.TryGet(DomainTags.Entity, out var entity))
///     logger.LogError("Entity {Entity} failed", entity);
///
/// // Write with type safety
/// var error = new Error("Something failed")
///     .WithTag(DomainTags.Entity, "Order")
///     .WithTag(DomainTags.EntityId, orderId.ToString());
/// </code>
/// </example>
public static class DomainTags
{
    /// <summary>The domain entity type name (e.g., "Order", "User").</summary>
    public static readonly TagKey<string> Entity = new("Entity");

    /// <summary>The domain entity identifier.</summary>
    public static readonly TagKey<string> EntityId = new("EntityId");

    /// <summary>The field name involved in a validation or conflict error.</summary>
    public static readonly TagKey<string> Field = new("Field");

    /// <summary>The conflicting or invalid value. Can be any type.</summary>
    public static readonly TagKey<object?> Value = new("Value");

    /// <summary>The operation or action name (e.g., "Delete", "Publish").</summary>
    public static readonly TagKey<string> Operation = new("Operation");

    /// <summary>The correlation / trace identifier for distributed tracing.</summary>
    public static readonly TagKey<string> CorrelationId = new("CorrelationId");

    /// <summary>The pipeline operation name used when injecting <see cref="ResultContext.OperationName"/> into error tags.</summary>
    public static readonly TagKey<string> OperationName = new("OperationName");

    /// <summary>The tenant identifier used when injecting <see cref="ResultContext.TenantId"/> into error tags.</summary>
    public static readonly TagKey<string> TenantId = new("TenantId");
}
