using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOfTest.Api.Models;
using Generated.OneOfExtensions;

namespace OneOfTest.Api.Controllers;

/// <summary>
/// Test controller demonstrating OneOf to IResult functionality.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly List<User> _users = new();

    /// <summary>
    /// Gets a user by ID.
    /// Returns OneOf of UserNotFoundError and User.
    /// </summary>
    [HttpGet("{id}")]
    public OneOf<UserNotFoundError, User> GetUser(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return new UserNotFoundError(id);
        }

        return user;
    }

    /// <summary>
    /// Creates a new user.
    /// Returns OneOf of ValidationError and CreatedUser converted to IResult.
    /// </summary>
    [HttpPost]
    public OneOf<UserNotFoundError, User> CreateUser([FromBody] CreateUserRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new UserNotFoundError(-1); // Temporary - using working type
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
        {
            return new UserNotFoundError(-2); // Temporary - using working type
        }

        // Check for duplicate email
        if (_users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return new UserNotFoundError(-3); // Temporary - using working type
        }

        // Create user
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
    /// Demonstrates the magic: OneOf → IResult conversion
    /// This method uses the generated ToIResult() extension method
    /// </summary>
    [HttpGet("{id}/result")]
    public IResult GetUserAsResult(int id)
    {
        // This will be automatically converted to HTTP response:
        // - UserNotFoundError → 404 Not Found
        // - User → 200 OK with user data
        return GetUser(id).ToIResult();
    }

    /// <summary>
    /// Demonstrates the magic: OneOf → IResult conversion for POST
    /// This method uses the generated ToIResult() extension method
    /// </summary>
    [HttpPost("result")]
    public IResult CreateUserAsResult([FromBody] CreateUserRequest request)
    {
        // This will be automatically converted to HTTP response:
        // - UserNotFoundError → 404 Not Found  
        // - User → 200 OK with user data
        return CreateUser(request).ToIResult();
    }

    /// <summary>
    /// Tests T1,T2,T3 OneOf implementation.
    /// Returns OneOf of ValidationError, NotFoundError, and User.
    /// </summary>
    [HttpPut("{id}")]
    public OneOf<ValidationError, UserNotFoundError, User> UpdateUser(int id, [FromBody] CreateUserRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new ValidationError("Name", "Name is required");
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
        {
            return new UserNotFoundError(id);
        }

        // Find user
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return new UserNotFoundError(id);
        }

        // Update user
        user.Name = request.Name;
        user.Email = request.Email;
        user.UpdatedAt = DateTime.UtcNow;

        return user;
    }

    /// <summary>
    /// Demonstrates the magic: T1,T2,T3 OneOf → IResult conversion
    /// This method uses the generated ToIResult() extension method
    /// </summary>
    [HttpPut("{id}/result")]
    public IResult UpdateUserAsResult(int id, [FromBody] CreateUserRequest request)
    {
        // This will be automatically converted to HTTP response:
        // - ValidationError → 400 Bad Request
        // - UserNotFoundError → 404 Not Found
        // - User → 200 OK with updated user data
        // TODO: Uncomment after T1,T2,T3 extension method is generated
        // return UpdateUser(id, request).ToIResult();
        
        // Temporary workaround
        var result = UpdateUser(id, request);
        return result.Match(
            validationError => Results.BadRequest(validationError.Message),
            notFoundError => Results.NotFound(notFoundError.Message),
            user => Results.Ok(user)
        );
    }
}

/// <summary>
/// Request model for creating users.
/// </summary>
public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
