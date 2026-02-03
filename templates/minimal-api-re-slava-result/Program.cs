using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using REslava.Result.AdvancedPatterns;  // OneOf support
using Generated.OneOfExtensions;           // Generated extensions
using REslavaResultMinimalApi.Models;
using REslavaResultMinimalApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register our services
builder.Services.AddSingleton<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 🎯 REslava.Result Minimal API Endpoints

// GET /api/users/{id} - Returns OneOf<UserNotFoundError, User>
app.MapGet("/api/users/{id}", (int id, IUserService userService) =>
{
    OneOf<UserNotFoundError, User> result = userService.GetUser(id);
    return result.ToIResult(); // Generated extension method
})
.WithName("GetUser")
.WithOpenApi();

// POST /api/users - Returns OneOf<ValidationError, User>
app.MapPost("/api/users", (CreateUserRequest request, IUserService userService) =>
{
    OneOf<ValidationError, User> result = userService.CreateUser(request);
    return result.ToIResult(); // Generated extension method
})
.WithName("CreateUser")
.WithOpenApi();

// GET /health - Simple health check
app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}))
.WithName("HealthCheck")
.WithOpenApi();

app.Run();
