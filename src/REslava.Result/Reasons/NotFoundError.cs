using System.Collections.Immutable;

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
public class NotFoundError : Reason<NotFoundError>, IError
{
    public NotFoundError(string message)
        : base(message, CreateDefaultTags())
    {
    }

    public NotFoundError(string entityName, object id)
        : base(
            $"{entityName} with id '{id}' was not found",
            CreateDefaultTags()
                .Add("EntityName", entityName)
                .Add("EntityId", id?.ToString() ?? "null"))
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
