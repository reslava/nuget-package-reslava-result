using System.Collections.Immutable;
namespace REslava.Result;

// ============================================================================
// Error
// ============================================================================
public class Error : Reason<Error>, IError
{
    public Error() : base(string.Empty) { }
    
    public Error(string message) : base(message) { }

    private Error(string message, ImmutableDictionary<string, object> tags)
        : base(message, tags) { }

    protected override Error CreateNew(string message, ImmutableDictionary<string, object> tags)
    {
        return new Error(message, tags);
    }
}