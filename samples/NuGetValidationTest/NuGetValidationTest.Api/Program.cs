using NuGetValidationTest.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 🧪 Health Check Endpoint
app.MapGet("/api/health", () => new
{
    Status = "Healthy",
    Generator = "REslava.Result.SourceGenerators v1.9.4",
    Architecture = "SOLID",
    Timestamp = DateTime.UtcNow
})
.WithName("HealthCheck");

// 🧪 Test Generated Extensions
app.MapGet("/api/test/get/{id}", TestEndpoints.TestGetResult)
   .WithName("TestGetResult");

app.MapPost("/api/test/post", TestEndpoints.TestPostResult)
   .WithName("TestPostResult");

app.MapPut("/api/test/put/{id}", TestEndpoints.TestPutResult)
   .WithName("TestPutResult");

app.MapDelete("/api/test/delete/{id}", TestEndpoints.TestDeleteResult)
   .WithName("TestDeleteResult");

app.MapPatch("/api/test/patch/{id}", TestEndpoints.TestPatchResult)
   .WithName("TestPatchResult");

app.Run();
