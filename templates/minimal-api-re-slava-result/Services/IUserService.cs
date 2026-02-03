using REslava.Result.AdvancedPatterns;
using REslavaResultMinimalApi.Models;

namespace REslavaResultMinimalApi.Services;

/// <summary>
/// Service interface demonstrating REslava.Result patterns.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets a user by ID.
    /// Returns OneOf of UserNotFoundError and User.
    /// </summary>
    OneOf<UserNotFoundError, User> GetUser(int id);
    
    /// <summary>
    /// Creates a new user.
    /// Returns OneOf of ValidationError and User.
    /// </summary>
    OneOf<ValidationError, User> CreateUser(CreateUserRequest request);
}
