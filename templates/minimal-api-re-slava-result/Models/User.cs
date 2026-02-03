using REslava.Result;

namespace REslavaResultMinimalApi.Models;

/// <summary>
/// User model for demonstrating REslava.Result patterns.
/// </summary>
public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
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
        WithTag("ErrorType", "NotFound");
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
/// Request model for creating users.
/// </summary>
public class CreateUserRequest
{
    public required string Name { get; set; }
    public required string Email { get; set; }
}
