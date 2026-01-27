using REslava.Result;
using REslava.Result.SourceGenerators;

[assembly: GenerateResultExtensions]

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/test1", () =>
{
    var result = Result<string>.Ok("Hello from source generator!");
    return result;
});

app.MapGet("/test2/{id}", (int id) =>
{
    if (id <= 0)
        return Result<string>.Fail("Invalid ID: must be positive");
    
    if (id > 100)
        return Result<string>.Fail("Resource not found");
    
    return Result<string>.Ok($"Item {id}");
});

app.MapPost("/test3", (UserRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        var error = new ValidationError("Name is required")
            .WithTag("Field", "Name")
            .WithTag("Timestamp", DateTime.UtcNow);
        return Result<User>.Fail(error);
    }
    
    if (request.Age < 18)
    {
        var error = new ValidationError("Must be 18 or older")
            .WithTag("Field", "Age")
            .WithTag("MinAge", 18)
            .WithTag("ActualAge", request.Age);
        return Result<User>.Fail(error);
    }
    
    var user = new User(request.Name, request.Age);
    return Result<User>.Ok(user);
});

app.Run();

record UserRequest(string Name, int Age);
record User(string Name, int Age);

class ValidationError : Error
{
    public ValidationError(string message) : base(message) { }
}
