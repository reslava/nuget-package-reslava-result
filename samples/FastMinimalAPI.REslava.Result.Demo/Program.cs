using FastMinimalAPI.REslava.Result.Demo.Data;
using FastMinimalAPI.REslava.Result.Demo.Endpoints;
using FastMinimalAPI.REslava.Result.Demo.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// NOTE: Swagger is disabled due to .NET 10 compatibility issues with Swashbuckle.AspNetCore
// The app uses Microsoft.AspNetCore.OpenApi instead for OpenAPI spec generation
// builder.Services.AddSwaggerGen();

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
if (app.Environment.IsDevelopment())
{
    // Swagger UI is currently disabled - API can be tested via:
    // - Direct HTTP requests (curl, Postman, etc.)
    // - Root endpoint (/) for API documentation
    // - Health check endpoint (/health)
}

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
        swagger = "/swagger",
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
        "OpenAPI/Swagger documentation"
    }
}))
.ExcludeFromDescription();

app.Run();
