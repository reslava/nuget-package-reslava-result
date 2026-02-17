using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Represents a validation error (HTTP 400/422 equivalent).
/// Use when input data fails validation rules.
/// </summary>
/// <example>
/// <code>
/// Result&lt;User&gt;.Fail(new ValidationError("Email", "Invalid email format"));
/// Result&lt;User&gt;.Fail(new ValidationError("Name is required"));
/// </code>
/// </example>
public class ValidationError : Reason<ValidationError>, IError
{
    public string? FieldName { get; }

    public ValidationError(string message)
        : base(message, CreateDefaultTags())
    {
    }

    public ValidationError(string fieldName, string message)
        : base(
            message,
            CreateDefaultTags().Add("FieldName", fieldName))
    {
        FieldName = fieldName;
    }

    private ValidationError(string message, ImmutableDictionary<string, object> tags, string? fieldName = null)
        : base(message, tags)
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
