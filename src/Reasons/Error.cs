using System.Collections.Immutable;
namespace REslava.Result;

// ============================================================================
// Error
// ============================================================================
public class Error : Reason<Error>, IError
{        
    // NO parameterless constructor
    
    /// <summary>
    /// Creates an error reason with a specific message.
    /// </summary>
    /// <param name="message">Description of the error.</param>
    
    public Error(string message) : base(message) { }

    private Error(string message, ImmutableDictionary<string, object> tags)
        : base(message, tags) { }

    protected override Error CreateNew(string message, ImmutableDictionary<string, object> tags)
    {
        return new Error(message, tags);
    }
    
    /// <summary>
    /// Creates a new error with additional tags.
    /// Error tags must not be null to ensure proper error tracking.
    /// </summary>
    /// <param name="tags">The tags to add to the error.</param>
    /// <returns>A new error instance with the added tags.</returns>
    public new Error WithTags(params (string key, object value)[] tags)
    {
        tags = tags.EnsureNotNull(nameof(tags));
        return base.WithTags(tags);
    }
}