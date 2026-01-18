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
        //ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));        
        return CreateNew(message, Tags);
    }

    /// <summary>
    /// Creates a new instance with an additional tag (immutable).
    /// Throws if key already exists.
    /// </summary>
    public TReason WithTags(string key, object value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));        

        if (Tags.ContainsKey(key))
        {
            throw new ArgumentException($"Tag with key '{key}' already exists", nameof(key));
        }

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
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            if (builder.ContainsKey(key))
            {                                
                throw new ArgumentException($"Tag with key '{key}' already exists", nameof(key));
            }

            builder.Add(key, value);
        }

        return CreateNew(Message, builder.ToImmutable());
    }

    /// <summary>
    /// Factory method for creating new instances (maintains immutability).
    /// Must be implemented by derived classes.
    /// </summary>
    protected abstract TReason CreateNew(string message, ImmutableDictionary<string, object> tags);
}
