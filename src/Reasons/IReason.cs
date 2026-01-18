using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
namespace REslava.Result;

public interface IReason
{
    [Required]
    string Message { get; init; } // all reasons must have a message
    ImmutableDictionary<string, object> Tags { get; init; }
}
