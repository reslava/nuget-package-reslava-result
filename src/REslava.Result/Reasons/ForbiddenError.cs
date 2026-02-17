using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Represents an authorization error (HTTP 403 equivalent).
/// Use when the caller is authenticated but lacks permission.
/// </summary>
/// <example>
/// <code>
/// Result&lt;User&gt;.Fail(new ForbiddenError());
/// Result&lt;User&gt;.Fail(new ForbiddenError("Admin role required"));
/// Result&lt;User&gt;.Fail(new ForbiddenError("Delete", "Order"));
/// </code>
/// </example>
public class ForbiddenError : Reason<ForbiddenError>, IError
{
    public ForbiddenError()
        : base("Access denied", CreateDefaultTags())
    {
    }

    public ForbiddenError(string message)
        : base(message, CreateDefaultTags())
    {
    }

    public ForbiddenError(string action, string resource)
        : base(
            $"Access denied: insufficient permissions to {action} {resource}",
            CreateDefaultTags()
                .Add("Action", action)
                .Add("Resource", resource))
    {
    }

    private ForbiddenError(string message, ImmutableDictionary<string, object> tags)
        : base(message, tags)
    {
    }

    protected override ForbiddenError CreateNew(string message, ImmutableDictionary<string, object> tags)
        => new(message, tags);

    private static ImmutableDictionary<string, object> CreateDefaultTags()
        => ImmutableDictionary<string, object>.Empty
            .Add("ErrorType", "Forbidden")
            .Add("HttpStatusCode", 403);
}
