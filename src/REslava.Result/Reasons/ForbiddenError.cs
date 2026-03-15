using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace REslava.Result;

/// <summary>
/// Represents an authorization error (HTTP 403 equivalent).
/// Use when the caller is authenticated but lacks permission.
/// </summary>
/// <example>
/// <code>
/// Result&lt;User&gt;.Fail(new ForbiddenError());
/// Result&lt;User&gt;.Fail(new ForbiddenError("Admin role required"));
/// Result&lt;User&gt;.Fail(ForbiddenError.For("Delete", "Order"));
/// </code>
/// </example>
public class ForbiddenError : Reason<ForbiddenError>, IError, IErrorFactory<ForbiddenError>
{
    /// <inheritdoc/>
    public static ForbiddenError Create(string message) => new(message);

    /// <summary>Creates a generic access-denied error. <see cref="ReasonMetadata.CallerMember"/> is not captured.</summary>
    public ForbiddenError()
        : base("Access denied", CreateDefaultTags())
    {
    }

    public ForbiddenError(
        string message,
        [CallerMemberName] string? callerMember = null,
        [CallerFilePath]   string? callerFile   = null,
        [CallerLineNumber] int     callerLine   = 0)
        : base(message, CreateDefaultTags(),
               ReasonMetadata.FromCaller(callerMember, callerFile, callerLine))
    {
    }

    [Obsolete("Use ForbiddenError.For(action, resource) for access control errors. This constructor will be removed in a future version.")]
    public ForbiddenError(string action, string resource)
        : base(
            $"Access denied: insufficient permissions to {action} {resource}",
            CreateDefaultTags()
                .Add("Action", action)
                .Add("Resource", resource))
    {
    }

    /// <summary>Creates an access-denied error for a specific action/resource pair, capturing the calling method name automatically.</summary>
    public static ForbiddenError For(
        string action,
        string resource,
        [CallerMemberName] string? callerMember = null,
        [CallerFilePath]   string? callerFile   = null,
        [CallerLineNumber] int     callerLine   = 0)
    {
        var tags = CreateDefaultTags()
            .Add("Action", action)
            .Add("Resource", resource);
        return new ForbiddenError(
            $"Access denied: insufficient permissions to {action} {resource}",
            tags,
            ReasonMetadata.FromCaller(callerMember, callerFile, callerLine));
    }

    private ForbiddenError(string message, ImmutableDictionary<string, object> tags)
        : base(message, tags)
    {
    }

    private ForbiddenError(string message, ImmutableDictionary<string, object> tags, ReasonMetadata metadata)
        : base(message, tags, metadata)
    {
    }

    protected override ForbiddenError CreateNew(string message, ImmutableDictionary<string, object> tags)
        => new(message, tags);

    private static ImmutableDictionary<string, object> CreateDefaultTags()
        => ImmutableDictionary<string, object>.Empty
            .Add("ErrorType", "Forbidden")
            .Add("HttpStatusCode", 403);
}
