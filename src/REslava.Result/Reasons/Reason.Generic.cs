using System.Collections.Immutable;

namespace REslava.Result;

//============================================================================
// Generic Reason with CRTP (Fluent API)
// ============================================================================
public abstract class Reason<TReason> : Reason
    where TReason : Reason<TReason>
{    
    protected Reason(string message) : base(message) { }

    protected Reason(string message, ImmutableDictionary<string, object> tags)
        : base(message, tags) { }

    protected Reason(string message, ImmutableDictionary<string, object> tags, ReasonMetadata metadata)
        : base(message, tags, metadata) { }

    /// <summary>
    /// Creates a new instance with updated message (immutable).
    /// Preserves existing <see cref="Reason.Metadata"/>.
    /// </summary>
    public TReason WithMessage(string message)
    {
        message = message.EnsureNotNullOrWhiteSpace(nameof(message));
        var copy = CreateNew(message, Tags);
        copy.Metadata = Metadata;
        return copy;
    }

    /// <summary>
    /// Creates a new instance with an additional tag (immutable).
    /// Throws if key already exists. Preserves existing <see cref="Reason.Metadata"/>.
    /// </summary>
    public TReason WithTag(string key, object value)
    {
        key = key.EnsureValidDictionaryKey(Tags, nameof(key));
        var copy = CreateNew(Message, Tags.Add(key, value));
        copy.Metadata = Metadata;
        return copy;
    }

    /// <summary>
    /// Creates a new instance with an additional typed tag (immutable).
    /// Throws if key already exists. Preserves existing <see cref="Reason.Metadata"/>.
    /// </summary>
    public TReason WithTag<T>(TagKey<T> key, T value)
    {
        key = key.EnsureNotNull(nameof(key));
        var validKey = key.Name.EnsureValidDictionaryKey(Tags, nameof(key));
        var copy = CreateNew(Message, Tags.Add(validKey, value!));
        copy.Metadata = Metadata;
        return copy;
    }

    /// <summary>
    /// Creates a new instance with multiple additional tags (immutable).
    /// Throws if any key already exists. Preserves existing <see cref="Reason.Metadata"/>.
    /// </summary>
    public TReason WithTags(params (string key, object value)[] tags)
    {
        if (tags is null || tags.Length == 0)
        {
            return (TReason)this; // No changes needed
        }

        var builder = Tags.ToBuilder();

        foreach (var (key, value) in tags)
        {
            var validKey = key.EnsureValidDictionaryKey(builder, nameof(key));
            builder.Add(validKey, value);
        }

        var copy = CreateNew(Message, builder.ToImmutable());
        copy.Metadata = Metadata;
        return copy;
    }

    /// <summary>
    /// Creates a new instance with additional tags from a dictionary (immutable).
    /// Throws if any key already exists. Preserves existing <see cref="Reason.Metadata"/>.
    /// </summary>
    /// <param name="tags">Dictionary containing tags to add.</param>
    /// <returns>A new instance with the added tags.</returns>
    /// <example>
    /// <code>
    /// var error = new ValidationError("Invalid input")
    ///     .WithTagsFrom(new Dictionary&lt;string, object&gt;
    ///     {
    ///         ["Field"] = "Email",
    ///         ["Value"] = "invalid-email"
    ///     });
    /// </code>
    /// </example>
    public TReason WithTagsFrom(ImmutableDictionary<string, object> tags)
    {
        if (tags is null || tags.Count == 0)
        {
            return (TReason)this; // No changes needed
        }

        var copy = CreateNew(Message, Tags.AddRange(tags));
        copy.Metadata = Metadata;
        return copy;
    }

    /// <summary>
    /// Creates a new instance with the specified metadata (immutable).
    /// Use this to attach or replace diagnostic/caller information.
    /// </summary>
    /// <param name="metadata">The metadata to attach. Null is treated as <see cref="ReasonMetadata.Empty"/>.</param>
    public TReason WithMetadata(ReasonMetadata metadata)
    {
        var copy = CreateNew(Message, Tags);
        copy.Metadata = metadata ?? ReasonMetadata.Empty;
        return copy;
    }

    /// <summary>
    /// Factory method for creating new instances (maintains immutability).
    /// Must be implemented by derived classes.
    /// </summary>
    protected abstract TReason CreateNew(string message, ImmutableDictionary<string, object> tags);
}
