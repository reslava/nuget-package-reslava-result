using Microsoft.AspNetCore.Mvc;
using MinimalApi.Net10.Reference.Data;
using MinimalApi.Net10.Reference.Services;
using MinimalApi.Net10.Reference.Endpoints;
using REslava.Result.SourceGenerators;

// 🎯 MAGIC: Enable source generator for automatic Result<T> to HTTP response conversion!
[assembly: GenerateResultExtensions]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// .NET 10 Enhanced API Documentation
builder.Services.Configure<Microsoft.AspNetCore.OpenApi.OpenApiOptions>(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "MinimalAPI .NET 10 Reference",
            Version = "v1.0.0",
            Description = """
            A reference implementation showcasing .NET 10 Minimal API features including:
            - Built-in request validation
            - Enhanced error handling with ValidationProblem
            - OpenAPI integration improvements
            - Modern C# 14 features
            - Clean architecture patterns
            """,
            Contact = new()
            {
                Name = "REslava.Result Reference Project",
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

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MinimalAPI .NET 10 Reference v1.0.0");
        options.RoutePrefix = string.Empty; // Serve Swagger at root
    });
}

app.UseHttpsRedirection();

// Map our endpoints
app.MapProductEndpoints();
app.MapOrderEndpoints();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0",
    Features = new[] { 
        ".NET 10 Minimal APIs", 
        "Built-in Validation", 
        "OpenAPI Documentation",
        "In-Memory Database"
    }
}))
.WithTags("Health")
.WithOpenApi();

// Welcome endpoint
app.MapGet("/", () => Results.Ok(new
{
    Message = "Welcome to MinimalAPI .NET 10 Reference!",
    Description = "This project demonstrates pure .NET 10 Minimal API features without external libraries.",
    Endpoints = new[]
    {
        "/swagger - API Documentation",
        "/api/products - Product Management",
        "/api/orders - Order Management",
        "/health - Health Check"
    },
    Features = new[]
    {
        "Built-in Request Validation",
        "Enhanced Error Handling",
        "OpenAPI/Swagger Integration",
        "Modern C# 14 Features",
        "Clean Architecture"
    }
}))
.WithTags("Home")
.WithOpenApi();

app.Run();
