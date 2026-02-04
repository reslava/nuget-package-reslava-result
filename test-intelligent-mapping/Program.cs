using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using REslava.Result.AdvancedPatterns;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Manual test of intelligent HTTP mapping
app.MapGet("/test/manual-mapping", () => 
{
    // Test the intelligent mapping logic manually
    var validationError = new ValidationError("Invalid input");
    var notFoundError = new UserNotFoundError(123);
    var conflictError = new ConflictError("User already exists");
    var databaseError = new DatabaseError("Connection failed");
    var unauthorizedError = new UnauthorizedError("Not authenticated");
    var forbiddenError = new ForbiddenError("Access denied");
    
    var results = new
    {
        ValidationError = new
        {
            Type = validationError.GetType().Name,
            ExpectedStatus = 400,
            Message = validationError.Message
        },
        UserNotFoundError = new
        {
            Type = notFoundError.GetType().Name,
            ExpectedStatus = 404,
            Message = notFoundError.Message
        },
        ConflictError = new
        {
            Type = conflictError.GetType().Name,
            ExpectedStatus = 409,
            Message = conflictError.Message
        },
        DatabaseError = new
        {
            Type = databaseError.GetType().Name,
            ExpectedStatus = 500,
            Message = databaseError.Message
        },
        UnauthorizedError = new
        {
            Type = unauthorizedError.GetType().Name,
            ExpectedStatus = 401,
            Message = unauthorizedError.Message
        },
        ForbiddenError = new
        {
            Type = forbiddenError.GetType().Name,
            ExpectedStatus = 403,
            Message = forbiddenError.Message
        }
    };
    
    return Results.Ok(results);
});

app.MapGet("/test/success", () => 
{
    var user = new User(1, "John", "john@example.com");
    return Results.Ok(user);
});

app.Run();
