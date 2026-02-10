using FastMinimalAPI.REslava.Result.Demo.Data;
using FastMinimalAPI.REslava.Result.Demo.Endpoints;
using FastMinimalAPI.REslava.Result.Demo.Services;
using FastMinimalAPI.REslava.Result.Demo.SmartEndpoints;
using Generated.SmartEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
            Version = "v1.13.0",
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

// Register SmartEndpoint controllers (resolved via DI by the generated endpoints)
builder.Services.AddScoped<SmartProductController>();
builder.Services.AddScoped<SmartOrderController>();

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

// JWT Bearer authentication for SmartEndpoints auth demo
var jwtKey = "REslava-Demo-SuperSecret-Key-Min32Chars!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

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
app.UseAuthentication();
app.UseAuthorization();

// Manual endpoints (traditional approach — full control, more boilerplate)
app.MapUserEndpoints();
app.MapProductEndpoints();
app.MapOrderEndpoints();

// SmartEndpoints (auto-generated via source generator — zero boilerplate!)
// Compare /api/smart/products with /api/products — same behavior, ~85% less code
app.MapSmartEndpoints();

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

// Auth token endpoint — generates test JWTs for SmartEndpoints auth demo
app.MapPost("/auth/token", (string? role) =>
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, "demo-user"),
        new(ClaimTypes.Email, "demo@reslava.dev"),
        new("sub", "demo-user-001")
    };
    if (!string.IsNullOrEmpty(role))
        claims.Add(new Claim(ClaimTypes.Role, role));

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var token = new JwtSecurityToken(
        expires: DateTime.UtcNow.AddHours(1),
        claims: claims,
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

    return Results.Ok(new
    {
        token = new JwtSecurityTokenHandler().WriteToken(token),
        expires = token.ValidTo,
        role = role ?? "none"
    });
})
.WithName("GenerateToken")
.WithTags("Auth")
.WithSummary("Generate a test JWT token for SmartEndpoints auth demo")
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
        manual = new { users = "/api/users", products = "/api/products", orders = "/api/orders" },
        smart = new { products = "/api/smart/products", orders = "/api/smart/orders" }
    },
    documentation = "https://github.com/reslava/nuget-package-reslava-result",
    features = new[]
    {
        "Result<T> pattern for type-safe error handling",
        "OneOf<T1,T2> for multiple return types (2 types)",
        "OneOf<T1,T2,T3> for complex scenarios (3 types)",
        "OneOf<T1,T2,T3,T4> for advanced patterns (4 types)",
        "SmartEndpoints: auto-generated endpoints via source generator (~85% less code)",
        "Railway-oriented programming",
        "Zero exception-based control flow",
        "Production-ready error responses",
        "OpenAPI 3.0 specification with Scalar UI"
    }
}));

app.Run();
