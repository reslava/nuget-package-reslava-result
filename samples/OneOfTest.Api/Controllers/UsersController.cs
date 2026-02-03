using Microsoft.AspNetCore.Mvc;
using REslava.Result.AdvancedPatterns;  // Using REslava.Result OneOf
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

    static UsersController()
    {
        // Add test data
        _users.Add(new User { Id = 1, Name = "Test User", Email = "test@example.com", CreatedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Gets a user by ID.
    /// Returns OneOf of UserNotFoundError and User converted to IResult.
    /// </summary>
    [HttpGet("{id}")]
    public IResult GetUser(int id)
    {
        //REslava.Result.AdvancedPatterns.OneOf<UserNotFoundError, User> result = _users.FirstOrDefault(u => u.Id == id) switch
        OneOf<UserNotFoundError, User> result = _users.FirstOrDefault(u => u.Id == id) switch
        {
            null => new UserNotFoundError(id),
            var user => user
        };

        return result.ToIResult();
    }

    /// <summary>
    /// Creates a new user.
    /// Returns OneOf of ValidationError and CreatedUser converted to IResult.
    /// </summary>
    [HttpPost]
    public IResult CreateUser([FromBody] CreateUserRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return OneOf<ValidationError, CreatedUser>.FromT1(new ValidationError("Name", "Name is required")).ToIResult();
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
        {
            return OneOf<ValidationError, CreatedUser>.FromT1(new ValidationError("Email", "Valid email is required")).ToIResult();
        }

        // Check for duplicate email
        if (_users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return OneOf<ValidationError, CreatedUser>.FromT1(new ValidationError("Email", "Email already exists")).ToIResult();
        }

        // Create new user
        var newUser = new User
        {
            Id = _users.Max(u => u.Id) + 1,
            Name = request.Name,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        _users.Add(newUser);

        var createdUser = new CreatedUser(newUser.Id, newUser.Name, newUser.Email, newUser.CreatedAt);
        return OneOf<ValidationError, CreatedUser>.FromT2(createdUser).ToIResult();
    }

    /// <summary>
    /// Updates an existing user.
    /// Returns OneOf of UserNotFoundError and UpdatedUser converted to IResult.
    /// </summary>
    [HttpPut("{id}")]
    public IResult UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == id);
        if (existingUser == null)
        {
            return OneOf<UserNotFoundError, UpdatedUser>.FromT1(new UserNotFoundError(id)).ToIResult();
        }

        // Validate input
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return OneOf<UserNotFoundError, UpdatedUser>.FromT1(new UserNotFoundError(id)).ToIResult();
        }

        // Update user
        existingUser.Name = request.Name;
        existingUser.Email = request.Email ?? existingUser.Email;
        existingUser.UpdatedAt = DateTime.UtcNow;

        var updatedUser = new UpdatedUser(existingUser.Id, existingUser.Name, existingUser.Email, existingUser.UpdatedAt ?? DateTime.UtcNow);
        return OneOf<UserNotFoundError, UpdatedUser>.FromT2(updatedUser).ToIResult();
    }

    /// <summary>
    /// Deletes a user.
    /// Returns OneOf of UserNotFoundError and DeleteSuccess converted to IResult.
    /// </summary>
    [HttpDelete("{id}")]
    public IResult DeleteUser(int id)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == id);
        if (existingUser == null)
        {
            return OneOf<UserNotFoundError, DeleteSuccess>.FromT1(new UserNotFoundError(id)).ToIResult();
        }

        _users.Remove(existingUser);
        var deleteSuccess = new DeleteSuccess(id, $"User {id} deleted successfully");
        return OneOf<UserNotFoundError, DeleteSuccess>.FromT2(deleteSuccess).ToIResult();
    }

    /// <summary>
    /// Test endpoint with different OneOf2 types.
    /// Returns OneOf of ServiceError and SuccessMessage converted to IResult.
    /// </summary>
    [HttpGet("test-service")]
    public IResult TestServiceEndpoint()
    {
        // Simulate service logic - 50% chance of success
        var random = new Random();
        if (random.Next(2) == 0)
        {
            return OneOf<ServiceError, SuccessMessage>.FromT1(new ServiceError("SERVICE_UNAVAILABLE", "Service temporarily unavailable")).ToIResult();
        }
        else
        {
            return OneOf<ServiceError, SuccessMessage>.FromT2(new SuccessMessage("Service is running normally")).ToIResult();
        }
    }

    /// <summary>
    /// Test endpoint with numeric and string types.
    /// Returns OneOf of ErrorMessage and CountResult converted to IResult.
    /// </summary>
    [HttpGet("count")]
    public IResult GetUserCount()
    {
        try
        {
            var count = _users.Count;
            var countResult = new CountResult(count, $"Total users: {count}");
            return OneOf<ErrorMessage, CountResult>.FromT2(countResult).ToIResult();
        }
        catch (Exception ex)
        {
            return OneOf<ErrorMessage, CountResult>.FromT1(new ErrorMessage("COUNT_ERROR", $"Failed to get user count: {ex.Message}")).ToIResult();
        }
    }

    /// <summary>
    /// Test endpoint for OneOf3ToIResult functionality.
    /// Returns OneOf of ValidationError, UserNotFoundError, User converted to IResult.
    /// </summary>
    [HttpGet("test-oneof3/{id}")]
    public IResult TestOneOf3(int id)
    {
        // Simulate different scenarios based on id
        if (id <= 0)
        {
            return OneOf<ValidationError, UserNotFoundError, User>.FromT1(
                new ValidationError("Id", "Id must be positive")
            ).ToIResult();
        }
        
        var existingUser = _users.FirstOrDefault(u => u.Id == id);
        if (existingUser == null)
        {
            return OneOf<ValidationError, UserNotFoundError, User>.FromT2(
                new UserNotFoundError(id)
            ).ToIResult();
        }
        
        return OneOf<ValidationError, UserNotFoundError, User>.FromT3(existingUser).ToIResult();
    }

    // Dummy method to trigger OneOfToIResult generator
    private REslava.Result.AdvancedPatterns.OneOf<UserNotFoundError, User> TriggerGenerator()
    {
        return new UserNotFoundError(0);
    }

    // Dummy method to trigger T1,T2,T3 generator
    private REslava.Result.AdvancedPatterns.OneOf<ValidationError, UserNotFoundError, User> TriggerT1T2T3Generator()
    {
        return new ValidationError("Test", "Test error");
    }

    // Public method to trigger T1,T2,T3 generator (must be public to be detected)
    [HttpGet("test-t1t2t3-trigger")]
    public REslava.Result.AdvancedPatterns.OneOf<ValidationError, UserNotFoundError, User> TestT1T2T3Trigger()
    {
        return new ValidationError("Test", "Test error");
    }

    // Simple test method to verify T1,T2,T3 extension
    // [HttpGet("test-t1t2t3")]
    // public IResult TestT1T2T3()
    // {
    //     REslava.Result.AdvancedPatterns.OneOf<ValidationError, UserNotFoundError, User> result = new ValidationError("Test", "Test error");
        
    //     try {
    //         return result.ToIResult();
    //     }
    //     catch (Exception ex)
    //     {
    //         return Results.Text($"T1,T2,T3 Error: {ex.Message}", "text/plain", statusCode: 500);
    //     }
    // }
}

/// <summary>
/// Request model for creating users.
/// </summary>
public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating users.
/// </summary>
public class UpdateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
}

/// <summary>
/// Represents a successfully created user.
/// </summary>
public class CreatedUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public CreatedUser(int id, string name, string email, DateTime createdAt)
    {
        Id = id;
        Name = name;
        Email = email;
        CreatedAt = createdAt;
    }
}

/// <summary>
/// Represents a successfully updated user.
/// </summary>
public class UpdatedUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }

    public UpdatedUser(int id, string name, string email, DateTime updatedAt)
    {
        Id = id;
        Name = name;
        Email = email;
        UpdatedAt = updatedAt;
    }
}

/// <summary>
/// Represents a successful deletion.
/// </summary>
public class DeleteSuccess
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;

    public DeleteSuccess(int id, string message)
    {
        Id = id;
        Message = message;
    }
}

/// <summary>
/// Represents a service error.
/// </summary>
public class ServiceError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public ServiceError(string code, string message)
    {
        Code = code;
        Message = message;
    }
}

/// <summary>
/// Represents a success message.
/// </summary>
public class SuccessMessage
{
    public string Message { get; set; } = string.Empty;

    public SuccessMessage(string message)
    {
        Message = message;
    }
}

/// <summary>
/// Represents a count result.
/// </summary>
public class CountResult
{
    public int Count { get; set; }
    public string Message { get; set; } = string.Empty;

    public CountResult(int count, string message)
    {
        Count = count;
        Message = message;
    }
}

/// <summary>
/// Represents an error message.
/// </summary>
public class ErrorMessage
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public ErrorMessage(string code, string message)
    {
        Code = code;
        Message = message;
    }
}
