using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace REslava.Result;

/// <summary>
/// Abstract base class for all result reasons (errors and successes).
/// Provides immutable <see cref="Message"/> and <see cref="Tags"/> storage
/// with a consistent <see cref="ToString"/> representation.
/// </summary>
/// <remarks>
/// Prefer inheriting <see cref="Reason{TSelf}"/> to get fluent CRTP builders
/// (<c>WithMessage</c>, <c>WithTag</c>, <c>WithTags</c>).
/// </remarks>
public abstract class Reason : IReason
{
    /// <inheritdoc/>
    [Required]
    public string Message { get; init; }

    /// <inheritdoc/>
    public ImmutableDictionary<string, object> Tags { get; init; }

    /// <summary>Initializes a new reason with the given message and empty tags.</summary>
    /// <param name="message">Human-readable description. Must not be null or whitespace.</param>
    protected Reason(string message)
    {
        Message = message.EnsureNotNullOrWhiteSpace(nameof(message));
        Tags = ImmutableDictionary<string, object>.Empty;
    }

    /// <summary>Initializes a new reason with the given message and pre-populated tags.</summary>
    /// <param name="message">Human-readable description. Must not be null or whitespace.</param>
    /// <param name="tags">Initial metadata tags. Null is treated as an empty dictionary.</param>
    protected Reason(string message, ImmutableDictionary<string, object> tags)
    {
        Message = message.EnsureNotNullOrWhiteSpace(nameof(message));
        Tags = tags ?? ImmutableDictionary<string, object>.Empty;
    }

    public override string ToString()
    {
        var tagsString = Tags.Any()
            ? $", Tags: [{string.Join(", ", Tags.Select(t => $"{t.Key}={t.Value}"))}]"
            : string.Empty;

        return $"{GetType().Name}: {Message}{tagsString}";
    }
}    
