using System.Collections.Immutable;
namespace REslava.Result;

public interface IReason
{
    string Message { get; }
    ImmutableDictionary<string, object> Tags { get; }
}
