using REslava.Result;

namespace OneOfTest.Api.Models;

/// <summary>
/// User model for testing OneOf functionality.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Error type for user not found scenarios.
/// </summary>
public class UserNotFoundError : Error
{
    public UserNotFoundError(int userId) 
        : base($"User with id '{userId}' not found")
    {
        WithTag("UserId", userId);
    }
}

/// <summary>
/// Error type for validation failures.
/// </summary>
public class ValidationError : Error
{
    public ValidationError(string field, string message) 
        : base($"{field}: {message}")
    {
        WithTag("Field", field);
        WithTag("ErrorType", "Validation");
    }
}

/// <summary>
/// Success type for created user responses.
/// </summary>
public class CreatedUser
{
    public User Value { get; set; } = null!;
    public string Location => $"/api/users/{Value.Id}";
}
