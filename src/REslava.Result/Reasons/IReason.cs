using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
namespace REslava.Result;

/// <summary>
/// Base contract for all result reasons — errors, successes, and custom outcomes.
/// All reasons carry a human-readable message and an optional metadata dictionary.
/// </summary>
public interface IReason
{
    /// <summary>Gets the human-readable description of this reason.</summary>
    [Required]
    string Message { get; init; }

    /// <summary>
    /// Gets the structured metadata attached to this reason.
    /// Keys are string labels; values are arbitrary objects (e.g. HTTP status codes, field names).
    /// Use <c>WithTag</c> on any concrete reason to attach metadata fluently.
    /// </summary>
    ImmutableDictionary<string, object> Tags { get; init; }
}
