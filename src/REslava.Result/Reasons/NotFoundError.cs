using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace REslava.Result;

/// <summary>
/// Represents a "not found" error (HTTP 404 equivalent).
/// Use when a requested resource does not exist.
/// </summary>
/// <example>
/// <code>
/// Result&lt;User&gt;.Fail(new NotFoundError("User", userId));
/// Result&lt;Order&gt;.Fail(new NotFoundError("Order not found"));
/// </code>
/// </example>
#if NET7_0_OR_GREATER
public class NotFoundError : Reason<NotFoundError>, IError, IErrorFactory<NotFoundError>
#else
public class NotFoundError : Reason<NotFoundError>, IError
#endif
{
    /// <inheritdoc/>
    public static NotFoundError Create(string message) => new(message);

    public NotFoundError(
        string message,
        [CallerMemberName] string? callerMember = null,
        [CallerFilePath]   string? callerFile   = null,
        [CallerLineNumber] int     callerLine   = 0)
        : base(message, CreateDefaultTags(),
               ReasonMetadata.FromCaller(callerMember, callerFile, callerLine))
    {
    }

    public NotFoundError(
        string entityName,
        object id,
        [CallerMemberName] string? callerMember = null,
        [CallerFilePath]   string? callerFile   = null,
        [CallerLineNumber] int     callerLine   = 0)
        : base(
            $"{entityName} with id '{id}' was not found",
            CreateDefaultTags()
                .Add(DomainTags.Entity.Name, entityName)
                .Add(DomainTags.EntityId.Name, id?.ToString() ?? "null"),
            ReasonMetadata.FromCaller(callerMember, callerFile, callerLine))
    {
    }

    private NotFoundError(string message, ImmutableDictionary<string, object> tags)
        : base(message, tags)
    {
    }

    protected override NotFoundError CreateNew(string message, ImmutableDictionary<string, object> tags)
        => new(message, tags);

    private static ImmutableDictionary<string, object> CreateDefaultTags()
        => ImmutableDictionary<string, object>.Empty
            .Add("ErrorType", "NotFound")
            .Add("HttpStatusCode", 404);
}
