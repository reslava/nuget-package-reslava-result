using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace REslava.Result;

/// <summary>
/// Represents a validation error (HTTP 400/422 equivalent).
/// Use when input data fails validation rules.
/// </summary>
/// <example>
/// <code>
/// Result&lt;User&gt;.Fail(ValidationError.Field("Email", "Invalid email format"));
/// Result&lt;User&gt;.Fail(new ValidationError("Name is required"));
/// </code>
/// </example>
public class ValidationError : Reason<ValidationError>, IError, IErrorFactory<ValidationError>
{
    /// <inheritdoc/>
    public static ValidationError Create(string message) => new(message);

    public string? FieldName { get; }

    public ValidationError(
        string message,
        [CallerMemberName] string? callerMember = null,
        [CallerFilePath]   string? callerFile   = null,
        [CallerLineNumber] int     callerLine   = 0)
        : base(message, CreateDefaultTags(),
               ReasonMetadata.FromCaller(callerMember, callerFile, callerLine))
    {
    }

    [Obsolete("Use ValidationError.Field(fieldName, message) for field-specific validation errors. This constructor will be removed in a future version.")]
    public ValidationError(string fieldName, string message)
        : base(
            message,
            CreateDefaultTags().Add(DomainTags.Field.Name, fieldName))
    {
        FieldName = fieldName;
    }

    /// <summary>Creates a validation error for a specific field, capturing the calling method name automatically.</summary>
    public static ValidationError Field(
        string fieldName,
        string message,
        [CallerMemberName] string? callerMember = null,
        [CallerFilePath]   string? callerFile   = null,
        [CallerLineNumber] int     callerLine   = 0)
    {
        var tags = CreateDefaultTags().Add(DomainTags.Field.Name, fieldName);
        return new ValidationError(message, tags, fieldName,
                                   ReasonMetadata.FromCaller(callerMember, callerFile, callerLine));
    }

    private ValidationError(string message, ImmutableDictionary<string, object> tags, string? fieldName = null)
        : base(message, tags)
    {
        FieldName = fieldName;
    }

    private ValidationError(string message, ImmutableDictionary<string, object> tags, string? fieldName, ReasonMetadata metadata)
        : base(message, tags, metadata)
    {
        FieldName = fieldName;
    }

    protected override ValidationError CreateNew(string message, ImmutableDictionary<string, object> tags)
        => new(message, tags, FieldName);

    private static ImmutableDictionary<string, object> CreateDefaultTags()
        => ImmutableDictionary<string, object>.Empty
            .Add("ErrorType", "Validation")
            .Add("HttpStatusCode", 422);
}
