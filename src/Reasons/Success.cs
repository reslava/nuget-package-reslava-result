using System.Collections.Immutable;
namespace REslava.Result;

// ============================================================================
// Success
// ============================================================================
public class Success : Reason<Success>, ISuccess
{       
    // NO parameterless constructor
    
    /// <summary>
    /// Creates a success reason with a specific message.
    /// </summary>
    /// <param name="message">Description of the successful operation.</param>
    public Success(string message) : base(message) { }

    protected Success(string message, ImmutableDictionary<string, object> tags)
        : base(message, tags) { }

    protected override Success CreateNew(string message, ImmutableDictionary<string, object> tags)
    {
        return new Success(message, tags);
    }
}
