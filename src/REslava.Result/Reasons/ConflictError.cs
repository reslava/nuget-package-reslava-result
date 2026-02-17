using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Represents a conflict error (HTTP 409 equivalent).
/// Use when an operation conflicts with existing state (duplicates, version conflicts).
/// </summary>
/// <example>
/// <code>
/// Result&lt;User&gt;.Fail(new ConflictError("User", "email", email));
/// Result&lt;User&gt;.Fail(new ConflictError("A user with this email already exists"));
/// </code>
/// </example>
public class ConflictError : Reason<ConflictError>, IError
{
    public ConflictError(string message)
        : base(message, CreateDefaultTags())
    {
    }

    public ConflictError(string entityName, string conflictField, object conflictValue)
        : base(
            $"{entityName} with {conflictField} '{conflictValue}' already exists",
            CreateDefaultTags()
                .Add("EntityName", entityName)
                .Add("ConflictField", conflictField)
                .Add("ConflictValue", conflictValue?.ToString() ?? "null"))
    {
    }

    private ConflictError(string message, ImmutableDictionary<string, object> tags)
        : base(message, tags)
    {
    }

    protected override ConflictError CreateNew(string message, ImmutableDictionary<string, object> tags)
        => new(message, tags);

    private static ImmutableDictionary<string, object> CreateDefaultTags()
        => ImmutableDictionary<string, object>.Empty
            .Add("ErrorType", "Conflict")
            .Add("HttpStatusCode", 409);
}
