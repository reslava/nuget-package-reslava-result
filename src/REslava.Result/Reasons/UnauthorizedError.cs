using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Represents an authentication error (HTTP 401 equivalent).
/// Use when the caller is not authenticated or credentials are invalid.
/// </summary>
/// <example>
/// <code>
/// Result&lt;User&gt;.Fail(new UnauthorizedError());
/// Result&lt;User&gt;.Fail(new UnauthorizedError("Token has expired"));
/// </code>
/// </example>
public class UnauthorizedError : Reason<UnauthorizedError>, IError
{
    public UnauthorizedError()
        : base("Authentication required", CreateDefaultTags())
    {
    }

    public UnauthorizedError(string message)
        : base(message, CreateDefaultTags())
    {
    }

    private UnauthorizedError(string message, ImmutableDictionary<string, object> tags)
        : base(message, tags)
    {
    }

    protected override UnauthorizedError CreateNew(string message, ImmutableDictionary<string, object> tags)
        => new(message, tags);

    private static ImmutableDictionary<string, object> CreateDefaultTags()
        => ImmutableDictionary<string, object>.Empty
            .Add("ErrorType", "Unauthorized")
            .Add("HttpStatusCode", 401);
}
