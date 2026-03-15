namespace REslava.Result;

/// <summary>
/// Extension methods for typed tag access on <see cref="IReason"/> using <see cref="TagKey{T}"/>.
/// </summary>
public static class ReasonTagExtensions
{
    /// <summary>
    /// Tries to get the value of a typed tag from the reason's <see cref="IReason.Tags"/> dictionary.
    /// Returns <see langword="false"/> if the key is not present or the value cannot be cast to <typeparamref name="T"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// if (error.TryGet(DomainTags.Entity, out var entity))
    ///     logger.LogError("Entity {Entity} failed", entity);
    /// </code>
    /// </example>
    public static bool TryGet<T>(this IReason reason, TagKey<T> key, out T? value)
    {
        if (reason.Tags.TryGetValue(key.Name, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the reason's <see cref="IReason.Tags"/> contains the given key
    /// with a value castable to <typeparamref name="T"/>.
    /// </summary>
    public static bool Has<T>(this IReason reason, TagKey<T> key)
        => reason.Tags.TryGetValue(key.Name, out var raw) && raw is T;
}
