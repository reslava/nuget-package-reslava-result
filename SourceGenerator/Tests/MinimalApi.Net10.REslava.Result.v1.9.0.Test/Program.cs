using Microsoft.AspNetCore.Mvc;
using REslava.Result.SourceGenerators;
using REslava.Result;
using Generated.ResultExtensions;
using MinimalApi.Net10.Reference.Data;
using MinimalApi.Net10.Reference.Services;
using MinimalApi.Net10.Reference.Endpoints;

// 🎯 MAGIC: Enable source generator for automatic Result<T> to HTTP response conversion!
[assembly: GenerateResultExtensions(
    Namespace = "Generated.ResultExtensions",
    IncludeErrorTags = true,
    GenerateHttpMethodExtensions = true,
    DefaultErrorStatusCode = 400
)]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// .NET 10 Enhanced API Documentation
builder.Services.Configure<Microsoft.AspNetCore.OpenApi.OpenApiOptions>(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "MinimalAPI .NET 10 v1.9.0 - REslava.Result + Core Library Source Generator",
            Version = "v1.9.0",
            Description = """
            A reference implementation showcasing REslava.Result v1.9.0 with Core library source generators:
            - Automatic Result<T> to IResult conversion using Core infrastructure
            - Generated extension methods (ToIResult, ToPostResult, ToDeleteResult, etc.)
            - Smart error classification with HttpStatusCodeMapper
            - Configuration-driven code generation with ResultToIResultConfig
            - Reusable Core library components (CodeBuilder, AttributeParser, etc.)
            - Clean Minimal API patterns
            - Source generator magic!
            """,
            Contact = new()
            {
                Name = "REslava.Result Source Generator Sample",
                Url = new("https://github.com/reslava/nuget-package-reslava-result")
            }
        };
        return Task.CompletedTask;
    });
});

// .NET 10 Validation Problem Details Configuration
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new Microsoft.AspNetCore.Mvc.ValidationProblemDetails(context.ModelState)
        {
            Title = "Validation Failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred.",
            Instance = context.HttpContext.Request.Path
        };

        // Add custom extensions for better error reporting
        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        problemDetails.Extensions["errors"] = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        return new BadRequestObjectResult(problemDetails);
    };
});

// Register our services
builder.Services.AddSingleton<InMemoryDatabase>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

app.UseHttpsRedirection();

// Map our endpoints
app.MapProductEndpoints();
app.MapOrderEndpoints();

// Health check endpoint - using source generator
app.MapGet("/health", () =>
{
    var healthData = new { 
        Status = "Healthy", 
        Timestamp = DateTime.UtcNow,
        Version = "1.7.3",
        Features = new[] { 
            ".NET 10 Minimal APIs", 
            "REslava.Result Pattern", 
            "Source Generator Magic",
            "Smart Error Classification",
            "In-Memory Database"
        }
    };
    
    var result = Result<object>.Ok(healthData);
    return result.ToIResult(); // 🎯 GENERATED: Result<T> to IResult
})
.WithName("HealthCheck");

// Welcome endpoint - using source generator
app.MapGet("/", () =>
{
    var welcomeData = new
    {
        Message = "Welcome to MinimalAPI .NET 10 v1.7.3 - REslava.Result + Source Generator!",
        Description = "This project demonstrates REslava.Result v1.7.3 with source generators for automatic HTTP response conversion.",
        Endpoints = new[]
        {
            "/api/products - Product Management",
            "/api/orders - Order Management", 
            "/health - Health Check"
        },
        Features = new[]
        {
            ".NET 10 Minimal APIs",
            "REslava.Result Pattern",
            "Source Generator Magic",
            "Smart Error Classification",
            "In-Memory Database"
        }
    };
    
    var result = Result<object>.Ok(welcomeData);
    return result.ToIResult(); // 🎯 GENERATED: Result<T> to IResult
})
.WithName("Welcome");

app.Run();
