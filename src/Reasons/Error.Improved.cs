using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Improved Error base class with better tag handling.
/// </summary>
public abstract class ImprovedError : Error
{
    protected ImprovedError(string message) : base(message) { }
    
    protected ImprovedError(string message, ImmutableDictionary<string, object> tags) 
        : base(message, tags) { }

    /// <summary>
    /// Creates a new error with tags using factory pattern.
    /// This solves the constructor chaining issue.
    /// </summary>
    protected static T CreateWithErrorTags<T>(
        Func<string, T> factory,
        string message,
        params (string key, object value)[] tags) where T : Error
    {
        var error = factory(message);
        return (T)error.WithTags(tags);
    }

    /// <summary>
    /// Creates a new error with predefined tag structure.
    /// </summary>
    protected static T CreateWithStandardTags<T>(
        Func<string, T> factory,
        string message,
        string errorType,
        string severity = "Error",
        string? component = null) where T : Error
    {
        var tags = new List<(string key, object value)>
        {
            ("ErrorType", errorType),
            ("Severity", severity),
            ("Timestamp", DateTime.UtcNow)
        };

        if (!string.IsNullOrEmpty(component))
            tags.Add(("Component", component));

        return CreateWithErrorTags(factory, message, tags.ToArray());
    }
}

/// <summary>
/// Improved ValidationError with proper tag handling.
/// </summary>
public class ImprovedValidationError : ImprovedError
{
    public string Field { get; }
    public string Value { get; }

    private ImprovedValidationError(string field, string value, string message) 
        : base(message)
    {
        Field = field;
        Value = value;
    }

    private ImprovedValidationError(string field, string value, string message, ImmutableDictionary<string, object> tags) 
        : base(message, tags)
    {
        Field = field;
        Value = value;
    }

    /// <summary>
    /// Factory method for creating ValidationError with proper tags.
    /// </summary>
    public static ImprovedValidationError Create(string field, string value, string message)
    {
        var errorMessage = $"{field}: {message}";
        var error = new ImprovedValidationError(field, value, errorMessage);
        
        // Add tags using the immutable pattern correctly
        return (ImprovedValidationError)error
            .WithTag("Field", field)
            .WithTag("Value", value)
            .WithTag("ErrorType", "Validation")
            .WithTag("Severity", "Warning");
    }

    protected override Error CreateNew(string message, ImmutableDictionary<string, object> tags)
    {
        return new ImprovedValidationError(Field, Value, message, tags);
    }
}

/// <summary>
/// Improved NotFoundError with proper tag handling.
/// </summary>
public class ImprovedNotFoundError : ImprovedError
{
    public string EntityType { get; }
    public string EntityId { get; }

    private ImprovedNotFoundError(string entityType, string entityId, string message) 
        : base(message)
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    private ImprovedNotFoundError(string entityType, string entityId, string message, ImmutableDictionary<string, object> tags) 
        : base(message, tags)
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    /// <summary>
    /// Factory method for User not found error.
    /// </summary>
    public static ImprovedNotFoundError User(string userId)
    {
        var message = $"User with id '{userId}' not found";
        var error = new ImprovedNotFoundError("User", userId, message);
        
        return (ImprovedNotFoundError)error
            .WithTag("EntityType", "User")
            .WithTag("EntityId", userId)
            .WithTag("StatusCode", 404)
            .WithTag("ErrorType", "NotFound");
    }

    protected override Error CreateNew(string message, ImmutableDictionary<string, object> tags)
    {
        return new ImprovedNotFoundError(EntityType, EntityId, message, tags);
    }
}
