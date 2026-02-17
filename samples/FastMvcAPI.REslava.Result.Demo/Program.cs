using FastMvcAPI.REslava.Result.Demo.Data;
using FastMvcAPI.REslava.Result.Demo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add MVC controllers
builder.Services.AddControllers();

// Add OpenAPI support (.NET 10 built-in)
builder.Services.AddOpenApi();

// Configure OpenAPI document metadata
builder.Services.Configure<Microsoft.AspNetCore.OpenApi.OpenApiOptions>(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "Fast MVC API - REslava.Result Demo",
            Version = "v1.21.0",
            Description = """
            Production-ready demonstration of type-safe error handling in ASP.NET Core MVC Controllers.

            Features:
            - Result<T>.ToActionResult() for convention-based HTTP mapping
            - OneOf<T1,T2,T3,T4> discriminated unions with .Match()
            - Domain errors (NotFoundError, ValidationError, ConflictError) auto-mapped to HTTP status codes
            - Zero exception-based control flow
            - Compare with FastMinimalAPI demo (port 5000) for side-by-side comparison
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
    options.UseInMemoryDatabase("MvcDemoDb"));

// Register services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// JWT Bearer authentication
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

// Use port 5001 (Minimal API demo uses 5000)
builder.WebHost.UseUrls("http://localhost:5001");

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
    await context.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline
app.MapOpenApi();
app.MapScalarApiReference();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map MVC controllers
app.MapControllers();

// Health check endpoint (Minimal API — kept for consistency)
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.21.0",
    description = "Fast MVC API - REslava.Result Demo"
}))
.WithName("HealthCheck")
.WithTags("Health")
.Produces<object>(200);

// Auth token endpoint — generates test JWTs
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
.WithSummary("Generate a test JWT token")
.Produces<object>(200);

// Welcome endpoint
app.MapGet("/", () => Results.Ok(new
{
    title = "Fast MVC API - REslava.Result Demo",
    version = "1.21.0",
    description = "MVC Controller demo showcasing Result<T>.ToActionResult() with domain error auto-mapping",
    endpoints = new
    {
        scalar = "/scalar",
        openapi = "/openapi/v1.json",
        health = "/health",
        controllers = new { users = "/api/users", products = "/api/products", orders = "/api/orders" }
    },
    documentation = "https://github.com/reslava/nuget-package-reslava-result",
    features = new[]
    {
        "Result<T>.ToActionResult() — convention-based HTTP mapping",
        "ToPostActionResult(), ToDeleteActionResult() — HTTP verb variants",
        "ToActionResult(onSuccess, onFailure) — explicit overload escape hatch",
        "OneOf<T1,T2>.Match() for discriminated union handling",
        "Domain errors auto-mapped: NotFoundError→404, ValidationError→422, ConflictError→409",
        "Compare with FastMinimalAPI demo (port 5000) for side-by-side comparison"
    }
}));

app.Run();
