using Generated.SmartEndpoints;

using MinimalApi.Net10.REslavaResult.Models;
using Microsoft.AspNetCore.Mvc;

using REslava.Result.SourceGenerators.SmartEndpoints;
using REslava.Result.AdvancedPatterns;
using REslava.Result;
using MinimalApi.Net10.REslavaResult.Models;

namespace MinimalApi.Net10.REslavaResult.Controllers;

/// <summary>
/// Test controller for SmartEndpoints generator
/// Tests automatic Minimal API endpoint generation
/// </summary>
[AutoGenerateEndpoints(RoutePrefix = "/api/smarttest")]
public class SmartTestController
{
    private static readonly List<User> _users = new();

    public SmartTestController()
    {
        // Add test data
        _users.Add(new User { Id = 1, Name = "Smart Test User", Email = "smart@example.com", CreatedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Gets a user by ID - should generate GET /api/smarttest/{id}
    /// </summary>
    public OneOf<UserNotFoundError, User> GetUser(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return user != null ? user : new UserNotFoundError(id);
    }

    /// <summary>
    /// Creates a user - should generate POST /api/smarttest
    /// </summary>
    public OneOf<ValidationError, UserNotFoundError, User> CreateUser(CreateUserRequest request)
    {
        // Validation
        if (string.IsNullOrEmpty(request.Name))
            return new ValidationError("Name is required", "Name");

        if (string.IsNullOrEmpty(request.Email))
            return new ValidationError("Email is required", "Email");

        // Duplicate check
        if (_users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
            return new UserNotFoundError(0); // ID 0 for duplicate email error

        var user = new User
        {
            Id = _users.Count + 1,
            Name = request.Name,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        _users.Add(user);
        return user;
    }

    /// <summary>
    /// Updates a user - should generate PUT /api/smarttest/{id}
    /// </summary>
    public OneOf<UserNotFoundError, ValidationError, User> UpdateUser(int id, CreateUserRequest request)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return new UserNotFoundError(id);

        // Validation
        if (string.IsNullOrEmpty(request.Name))
            return new ValidationError("Name is required", "Name");

        user.Name = request.Name;
        return user;
    }

    /// <summary>
    /// Deletes a user - should generate DELETE /api/smarttest/{id}
    /// </summary>
    public OneOf<UserNotFoundError, bool> DeleteUser(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return new UserNotFoundError(id);

        _users.Remove(user);
        return true;
    }
}
