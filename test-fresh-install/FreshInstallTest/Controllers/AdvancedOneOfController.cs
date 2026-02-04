using Microsoft.AspNetCore.Mvc;
using REslava.Result.AdvancedPatterns;
using Generated.OneOfExtensions;

namespace FreshInstallTest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdvancedOneOfController : ControllerBase
{
    // Test OneOf<ValidationError, Error, User> with 3 different error types
    [HttpGet("advanced-user/{id}")]
    public IResult GetAdvancedUser(int id)
    {
        if (id <= 0)
        {
            var result = OneOf<ValidationError, SystemError, User>.FromT1(new ValidationError("Invalid ID format"));
            return result.ToIResult(); // Should return 400
        }
        
        if (id == 404)
        {
            var result = OneOf<ValidationError, SystemError, User>.FromT2(new SystemError("User not found"));
            return result.ToIResult(); // Should return 500 (Database/System error)
        }
        
        // Success case
        var user = new User(id, "John Doe");
        var successResult = OneOf<ValidationError, SystemError, User>.FromT3(user);
        return successResult.ToIResult(); // Should return 200
    }

    // NEW: Test intelligent HTTP mapping with specific error types
    [HttpGet("intelligent-mapping/{errorType}")]
    public IResult TestIntelligentMapping(string errorType)
    {
        return errorType.ToLowerInvariant() switch
        {
            "validation" => OneOf<ValidationError, User>.FromT1(new ValidationError("Validation failed")).ToIResult(),
            "notfound" => OneOf<UserNotFoundError, User>.FromT1(new UserNotFoundError("User not found")).ToIResult(),
            "conflict" => OneOf<ConflictError, User>.FromT1(new ConflictError("Resource conflict")).ToIResult(),
            "database" => OneOf<SystemError, User>.FromT1(new SystemError("Database connection failed")).ToIResult(),
            _ => OneOf<ValidationError, User>.FromT2(new User(1, "Success")).ToIResult()
        };
    }

    // Test multi-error intelligent mapping
    [HttpPost("multi-error")]
    public IResult TestMultiErrorIntelligentMapping([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrEmpty(request.Name))
            return OneOf<ValidationError, User>.FromT1(new ValidationError("Name is required")).ToIResult();
        
        if (request.Email == "existing@user.com")
            return OneOf<ConflictError, User>.FromT1(new ConflictError("User already exists")).ToIResult();
        
        if (request.Email == "dberror@user.com")
            return OneOf<SystemError, User>.FromT1(new SystemError("Database connection failed")).ToIResult();
        
        // Success case
        var user = new User(1, request.Name);
        return OneOf<ValidationError, User>.FromT2(user).ToIResult();
    }
}
