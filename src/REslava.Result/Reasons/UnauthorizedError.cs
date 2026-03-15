using System.Collections.Immutable;
using System.Runtime.CompilerServices;

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
public class UnauthorizedError : Reason<UnauthorizedError>, IError, IErrorFactory<UnauthorizedError>
{
    /// <inheritdoc/>
    public static UnauthorizedError Create(string message) => new(message);

    public UnauthorizedError()
        : base("Authentication required", CreateDefaultTags())
    {
    }

    public UnauthorizedError(
        string message,
        [CallerMemberName] string? callerMember = null,
        [CallerFilePath]   string? callerFile   = null,
        [CallerLineNumber] int     callerLine   = 0)
        : base(message, CreateDefaultTags(),
               ReasonMetadata.FromCaller(callerMember, callerFile, callerLine))
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
