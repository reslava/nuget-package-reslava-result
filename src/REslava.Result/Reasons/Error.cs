using System.Collections.Immutable;
using System.Runtime.CompilerServices;
namespace REslava.Result;

// ============================================================================
// Error
// ============================================================================
public class Error : Reason<Error>, IError, IErrorFactory<Error>
{
    // NO parameterless constructor

    /// <inheritdoc/>
    public static Error Create(string message) => new(message);

    /// <summary>Creates an error reason with a specific message.</summary>
    public Error(
        string message,
        [CallerMemberName] string? callerMember = null,
        [CallerFilePath]   string? callerFile   = null,
        [CallerLineNumber] int     callerLine   = 0)
        : base(message, ImmutableDictionary<string, object>.Empty,
               ReasonMetadata.FromCaller(callerMember, callerFile, callerLine)) { }

    protected Error(string message, ImmutableDictionary<string, object> tags)
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