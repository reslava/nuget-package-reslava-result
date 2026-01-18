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

    /// <summary>
    /// Creates a new instance with updated message (immutable).
    /// </summary>
    public TReason WithMessage(string message)
    {
        message = message.EnsureNotNullOrWhiteSpace(nameof(message));
        return CreateNew(message, Tags);
    }

    /// <summary>
    /// Creates a new instance with an additional tag (immutable).
    /// Throws if key already exists.
    /// </summary>
    public TReason WithTags(string key, object value)
    {
        key = key.EnsureValidDictionaryKey(Tags, nameof(key));        
        return CreateNew(Message, Tags.Add(key, value));
    }

    /// <summary>
    /// Creates a new instance with multiple additional tags (immutable).
    /// Throws if any key already exists.
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

        return CreateNew(Message, builder.ToImmutable());
    }

    /// <summary>
    /// Factory method for creating new instances (maintains immutability).
    /// Must be implemented by derived classes.
    /// </summary>
    protected abstract TReason CreateNew(string message, ImmutableDictionary<string, object> tags);
}
