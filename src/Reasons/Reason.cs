using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace REslava.Result;

// ============================================================================
// Base Reason (Non-generic)
// ============================================================================
public abstract class Reason : IReason
{
    [Required]
    public string Message { get; init; }
    public ImmutableDictionary<string, object> Tags { get; init; }    
    
    protected Reason(string message)
    {
        Message = message.EnsureNotNullOrWhiteSpace(nameof(message));
        Tags = ImmutableDictionary<string, object>.Empty;
    }

    protected Reason(string message, ImmutableDictionary<string, object> tags)
    {
        Message = message.EnsureNotNullOrWhiteSpace(nameof(message));
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
