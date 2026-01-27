using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using REslava.Result;
using REslava.Result.SourceGenerators;

// ============================================================================
// STEP 1: Enable the source generator with the assembly attribute
// ============================================================================
[assembly: GenerateResultExtensions(
    Namespace = "MyApp.Extensions",
    IncludeErrorTags = true,
    GenerateHttpMethodExtensions = true,
    CustomErrorMappings = new[]
    {
        "ValidationError:422",
        "NotFoundError:404",
        "DuplicateError:409",
        "UnauthorizedError:401"
    }
)]

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<IUserService, UserService>();

var app = builder.Build();

// ============================================================================
// STEP 2: Use the generated extensions in your endpoints
// ============================================================================

// Example 1: Simple GET - automatic Result<T> to IResult conversion
app.MapGet("/users/{id}", async (int id, IUserService service) =>
    await service.GetUserAsync(id));
    // ✅ That's it! No more .Match() calls
    // The generator automatically converts:
    // - Success → 200 OK with user data
    // - NotFoundError → 404 Not Found with ProblemDetails
    // - Other errors → appropriate status code

// Example 2: POST with location header
app.MapPost("/users", async (CreateUserRequest request, IUserService service) =>
{
    var result = await service.CreateUserAsync(request);
    return result.ToPostResult(user => $"/users/{user.Id}");
});
    // ✅ Automatically returns 201 Created with Location header on success

// Example 3: Non-generic Result (no return value)
app.MapDelete("/users/{id}", async (int id, IUserService service) =>
    await service.DeleteUserAsync(id));
    // ✅ Returns 204 No Content on success, 404 on not found

// Example 4: PUT operation
app.MapPut("/users/{id}", async (int id, UpdateUserRequest request, IUserService service) =>
    await service.UpdateUserAsync(id, request));
    // ✅ Returns 200 OK on success, 404 if user doesn't exist

// Example 5: Complex error handling with tags
app.MapPost("/users/register", async (RegisterUserRequest request, IUserService service) =>
    await service.RegisterUserAsync(request));
    // ✅ Error tags automatically included in ProblemDetails.Extensions["context"]

app.Run();

// ============================================================================
// Domain Models
// ============================================================================

public record User(int Id, string Name, string Email);
public record CreateUserRequest(string Name, string Email);
public record UpdateUserRequest(string Name, string Email);
public record RegisterUserRequest(string Name, string Email, string Password);

// ============================================================================
// Custom Error Types (for CustomErrorMappings)
// ============================================================================

/// <summary>
/// Error type for validation failures. Maps to 422 via CustomErrorMappings.
/// </summary>
public class ValidationError : Error
{
    public ValidationError(string message) : base(message) { }
}

/// <summary>
/// Error type for resource not found. Maps to 404 via CustomErrorMappings.
/// </summary>
public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message) { }
}

/// <summary>
/// Error type for duplicate resources. Maps to 409 via CustomErrorMappings.
/// </summary>
public class DuplicateError : Error
{
    public DuplicateError(string message) : base(message) { }
}

/// <summary>
/// Error type for unauthorized access. Maps to 401 via CustomErrorMappings.
/// </summary>
public class UnauthorizedError : Error
{
    public UnauthorizedError(string message) : base(message) { }
}

// ============================================================================
// Service Layer - Returns Results
// ============================================================================

public interface IUserService
{
    Task<Result<User>> GetUserAsync(int id);
    Task<Result<User>> CreateUserAsync(CreateUserRequest request);
    Task<Result<User>> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<Result> DeleteUserAsync(int id);
    Task<Result<User>> RegisterUserAsync(RegisterUserRequest request);
}

public class UserService : IUserService
{
    private readonly List<User> _users = new()
    {
        new User(1, "John Doe", "john@example.com"),
        new User(2, "Jane Smith", "jane@example.com")
    };

    /// <summary>
    /// Get user by ID. Returns NotFoundError if user doesn't exist.
    /// </summary>
    public Task<Result<User>> GetUserAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        
        if (user == null)
        {
            // Custom error type - will map to 404 via CustomErrorMappings
            return Task.FromResult(
                Result<User>.Fail(new NotFoundError($"User {id} not found")));
        }

        return Task.FromResult(Result<User>.Ok(user));
    }

    /// <summary>
    /// Create new user. Returns DuplicateError if email already exists.
    /// </summary>
    public Task<Result<User>> CreateUserAsync(CreateUserRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Task.FromResult(
                Result<User>.Fail(new ValidationError("Email is required")));
        }

        // Check for duplicates
        if (_users.Any(u => u.Email == request.Email))
        {
            return Task.FromResult(
                Result<User>.Fail(new DuplicateError($"User with email {request.Email} already exists")));
        }

        // Create user
        var newUser = new User(_users.Max(u => u.Id) + 1, request.Name, request.Email);
        _users.Add(newUser);

        return Task.FromResult(Result<User>.Ok(newUser));
    }

    /// <summary>
    /// Update user. Returns NotFoundError if user doesn't exist.
    /// </summary>
    public Task<Result<User>> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        
        if (user == null)
        {
            return Task.FromResult(
                Result<User>.Fail(new NotFoundError($"User {id} not found")));
        }

        // Update user (in real app, would update in database)
        var updatedUser = user with { Name = request.Name, Email = request.Email };
        var index = _users.IndexOf(user);
        _users[index] = updatedUser;

        return Task.FromResult(Result<User>.Ok(updatedUser));
    }

    /// <summary>
    /// Delete user. Returns NotFoundError if user doesn't exist.
    /// </summary>
    public Task<Result> DeleteUserAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        
        if (user == null)
        {
            return Task.FromResult(
                Result.Fail(new NotFoundError($"User {id} not found")));
        }

        _users.Remove(user);
        return Task.FromResult(Result.Ok());
    }

    /// <summary>
    /// Register user with rich error context using tags.
    /// </summary>
    public Task<Result<User>> RegisterUserAsync(RegisterUserRequest request)
    {
        // Validation with tagged errors
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            var error = new ValidationError("Email is required")
                .WithTag("Field", "Email")
                .WithTag("Validator", "Required")
                .WithTag("AttemptedAt", DateTime.UtcNow.ToString("O"));
            
            return Task.FromResult(Result<User>.Fail(error));
        }

        if (request.Password.Length < 8)
        {
            var error = new ValidationError("Password must be at least 8 characters")
                .WithTag("Field", "Password")
                .WithTag("Validator", "MinLength")
                .WithTag("MinLength", 8)
                .WithTag("ActualLength", request.Password.Length);
            
            return Task.FromResult(Result<User>.Fail(error));
        }

        // Check duplicate
        if (_users.Any(u => u.Email == request.Email))
        {
            var error = new DuplicateError($"Email {request.Email} is already registered")
                .WithTag("Field", "Email")
                .WithTag("ConflictingEmail", request.Email)
                .WithTag("ConflictingUserId", _users.First(u => u.Email == request.Email).Id);
            
            return Task.FromResult(Result<User>.Fail(error));
        }

        // Create user
        var newUser = new User(_users.Max(u => u.Id) + 1, request.Name, request.Email);
        _users.Add(newUser);

        return Task.FromResult(Result<User>.Ok(newUser));
    }
}

// ============================================================================
// Generated Code (by the source generator) - Example of what gets created
// ============================================================================
/*
// This is what the source generator creates for you automatically:

namespace MyApp.Extensions
{
    public static class ResultToIResultExtensions
    {
        /// <summary>
        /// Converts a Result<T> to IResult for use in Minimal API endpoints.
        /// </summary>
        public static IResult ToIResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }

            var statusCode = DetermineStatusCode(result.Errors);
            var problemDetails = CreateProblemDetails(result.Errors, statusCode, null);

            return Results.Problem(
                detail: problemDetails.Detail,
                instance: problemDetails.Instance,
                statusCode: problemDetails.Status,
                title: problemDetails.Title,
                type: problemDetails.Type,
                extensions: problemDetails.Extensions);
        }

        // ... more methods
    }
}
*/
