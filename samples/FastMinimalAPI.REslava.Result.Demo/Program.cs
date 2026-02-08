using FastMinimalAPI.REslava.Result.Demo.Data;
using FastMinimalAPI.REslava.Result.Demo.Endpoints;
using FastMinimalAPI.REslava.Result.Demo.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// Add OpenAPI support (.NET 10 built-in)
builder.Services.AddOpenApi();

// Configure OpenAPI document metadata
builder.Services.Configure<Microsoft.AspNetCore.OpenApi.OpenApiOptions>(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "Fast Minimal API - REslava.Result Demo",
            Version = "v1.12.1",
            Description = """
            Production-ready demonstration of type-safe error handling in ASP.NET Core Minimal APIs

            Features:
            - Result<T> pattern for railway-oriented programming
            - OneOf<T1,T2,T3,T4> discriminated unions
            - Zero exception-based control flow
            - Rich error context with metadata
            - Automatic HTTP status code mapping
            - Type-safe error handling at compile time
            """,
            Contact = new()
            {
                Name = "REslava.Result Library",
                Url = new Uri("https://github.com/reslava/nuget-package-reslava-result")
            }
        };
        return Task.CompletedTask;
    });
});

// Configure Entity Framework Core with In-Memory Database
builder.Services.AddDbContext<DemoDbContext>(options =>
    options.UseInMemoryDatabase("DemoDb"));

// Register services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
    await context.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline
// Map OpenAPI specification endpoint (available in all environments for demo purposes)
app.MapOpenApi();

// Map Scalar UI for beautiful API documentation
// Scalar.AspNetCore default endpoint is /scalar/v1
app.MapScalarApiReference();

app.UseCors();

// Map endpoints
app.MapUserEndpoints();
app.MapProductEndpoints();
app.MapOrderEndpoints();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0",
    description = "Fast Minimal API - REslava.Result Demo"
}))
.WithName("HealthCheck")
.WithTags("Health")
.Produces<object>(200);

// Welcome endpoint
app.MapGet("/", () => Results.Ok(new
{
    title = "Fast Minimal API - REslava.Result Demo",
    version = "1.0.0",
    description = "Production-ready demo showcasing REslava.Result library with type-safe error handling",
    endpoints = new
    {
        scalar = "/scalar",
        openapi = "/openapi/v1.json",
        health = "/health",
        users = "/api/users",
        products = "/api/products",
        orders = "/api/orders"
    },
    documentation = "https://github.com/reslava/nuget-package-reslava-result",
    features = new[]
    {
        "Result<T> pattern for type-safe error handling",
        "OneOf<T1,T2> for multiple return types (2 types)",
        "OneOf<T1,T2,T3> for complex scenarios (3 types)",
        "OneOf<T1,T2,T3,T4> for advanced patterns (4 types)",
        "Railway-oriented programming",
        "Zero exception-based control flow",
        "Production-ready error responses",
        "OpenAPI 3.0 specification with Scalar UI"
    }
}));

app.Run();
