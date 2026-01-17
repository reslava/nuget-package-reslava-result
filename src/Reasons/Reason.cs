using System.Collections.Immutable;

namespace REslava.Result;

// ============================================================================
// Base Reason (Non-generic)
// ============================================================================
public abstract class Reason : IReason
{
    public string Message { get; }
    public ImmutableDictionary<string, object> Tags { get; }

    protected Reason(string message)
    {
        Message = message ?? string.Empty;
        Tags = ImmutableDictionary<string, object>.Empty;
    }

    protected Reason(string message, ImmutableDictionary<string, object> tags)
    {
        Message = message ?? string.Empty;
        Tags = tags ?? ImmutableDictionary<string, object>.Empty;
    }
}

    //public override string ToString()
    //{
    //    var tagsString = Tags.Any() ?
    //        $" {nameof(Tags)}: {string.Join(", ", Tags)}" :
    //        string.Empty;

    //    return $"{GetType().Name}: {Message}{tagsString}";
    //}
