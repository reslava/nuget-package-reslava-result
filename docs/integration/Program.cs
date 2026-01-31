using Microsoft.AspNetCore.Http.HttpResults;
using REslava.Result;

// STEP 1: Add the attribute to enable source generation
[assembly: REslava.Result.SourceGenerators.GenerateResultExtensions(
    Namespace = "TestProject.Generated",
    DefaultErrorStatusCode = 400)]

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// STEP 2: Use Result in your endpoints
app.MapGet("/user/{id}", GetUser);
app.MapPost("/user", CreateUser);

app.Run();

// Example 1: Simple endpoint returning Result -> IResult
Results<Ok<User>, ProblemHttpResult> GetUser(int id)
{
    var result = UserService.GetUser(id);
    
    // The generated ToIResult() extension method converts Result<User> to IResult
    return result.ToIResult();
}

// Example 2: Endpoint with validation
Results<Created<User>, ValidationProblem> CreateUser(CreateUserRequest request)
{
    var result = UserService.CreateUser(request);
    
    return result.ToIResult();
}

// Mock services
public static class UserService
{
    public static Result<User> GetUser(int id)
    {
        if (id <= 0)
            return Result<User>.Fail(new Error("Invalid user ID"));
        
        if (id == 999)
            return Result<User>.Fail(new NotFoundError($"User {id} not found"));
        
        return Result<User>.Ok(new User { Id = id, Name = $"User {id}" });
    }
    
    public static Result<User> CreateUser(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<User>.Fail(new ValidationError("Name is required"));
        
        var user = new User { Id = Random.Shared.Next(1, 1000), Name = request.Name };
        return Result<User>.Ok(user);
    }
}

// Models
public record User
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
}

public record CreateUserRequest(string Name);

// Custom error types
public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message) { }
}

public class ValidationError : Error
{
    public ValidationError(string message) : base(message) { }
}
