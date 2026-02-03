using REslava.Result.AdvancedPatterns;
using REslavaResultMinimalApi.Models;

namespace REslavaResultMinimalApi.Services;

/// <summary>
/// Service implementation demonstrating REslava.Result patterns.
/// </summary>
public class UserService : IUserService
{
    private static readonly List<User> _users = new();

    static UserService()
    {
        // Add sample data
        _users.Add(new User 
        { 
            Id = 1, 
            Name = "Sample User", 
            Email = "sample@example.com", 
            CreatedAt = DateTime.UtcNow 
        });
    }

    public OneOf<UserNotFoundError, User> GetUser(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return new UserNotFoundError(id);
        }
        return user;
    }

    public OneOf<ValidationError, User> CreateUser(CreateUserRequest request)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new ValidationError("Name", "Name is required");
        }

        if (request.Name.Length < 2)
        {
            return new ValidationError("Name", "Name must be at least 2 characters");
        }

        // Validate email
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new ValidationError("Email", "Email is required");
        }

        if (!request.Email.Contains("@"))
        {
            return new ValidationError("Email", "Email must be valid");
        }

        // Check for duplicate email
        if (_users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return new ValidationError("Email", "Email already exists");
        }

        // Create user
        var user = new User
        {
            Id = _users.Count + 1,
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _users.Add(user);
        return user;
    }
}
