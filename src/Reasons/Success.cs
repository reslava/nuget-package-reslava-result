using System.Collections.Immutable;
namespace REslava.Result;

// ============================================================================
// Success
// ============================================================================
public class Success : Reason<Success>, ISuccess
{
    public Success() : base(string.Empty) { }
    
    public Success(string message) : base(message) { }

    private Success(string message, ImmutableDictionary<string, object> tags)
        : base(message, tags) { }

    protected override Success CreateNew(string message, ImmutableDictionary<string, object> tags)
    {
        return new Success(message, tags);
    }
}
