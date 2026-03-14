using System.Collections.Immutable;
using System.Runtime.CompilerServices;

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
    public ConflictError(
        string message,
        [CallerMemberName] string? callerMember = null,
        [CallerFilePath]   string? callerFile   = null,
        [CallerLineNumber] int     callerLine   = 0)
        : base(message, CreateDefaultTags(),
               ReasonMetadata.FromCaller(callerMember, callerFile, callerLine))
    {
    }

    [Obsolete("Use ConflictError.Duplicate(entity, field, value) instead — it also captures the calling step name.")]
    public ConflictError(
        string entityName,
        string conflictField,
        object conflictValue)
        : base(
            $"{entityName} with {conflictField} '{conflictValue}' already exists",
            CreateDefaultTags()
                .Add("EntityName", entityName)
                .Add("ConflictField", conflictField)
                .Add("ConflictValue", conflictValue?.ToString() ?? "null"))
    {
    }

    /// <summary>Creates a conflict error for a duplicate field value, capturing the calling step name.</summary>
    public static ConflictError Duplicate(
        string entityName,
        string conflictField,
        object conflictValue,
        [CallerMemberName] string? callerMember = null,
        [CallerFilePath]   string? callerFile   = null,
        [CallerLineNumber] int     callerLine   = 0)
    {
        var tags = CreateDefaultTags()
            .Add("EntityName", entityName)
            .Add("ConflictField", conflictField)
            .Add("ConflictValue", conflictValue?.ToString() ?? "null");
        return new ConflictError(
            $"{entityName} with {conflictField} '{conflictValue}' already exists", tags)
        {
            Metadata = ReasonMetadata.FromCaller(callerMember, callerFile, callerLine)
        };
    }

    /// <summary>Creates a conflict error for a duplicate field value, inferring entity name from <typeparamref name="T"/>.</summary>
    public static ConflictError Duplicate<T>(
        string conflictField,
        object conflictValue,
        [CallerMemberName] string? callerMember = null,
        [CallerFilePath]   string? callerFile   = null,
        [CallerLineNumber] int     callerLine   = 0)
        => Duplicate(typeof(T).Name, conflictField, conflictValue, callerMember, callerFile, callerLine);

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
