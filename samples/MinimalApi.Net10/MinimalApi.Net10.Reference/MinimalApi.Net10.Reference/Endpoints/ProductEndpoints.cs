using Microsoft.AspNetCore.Mvc;
using MinimalApi.Net10.Reference.Models;
using MinimalApi.Net10.Reference.Services;

namespace MinimalApi.Net10.Reference.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var productGroup = app.MapGroup("/api/products")
            .WithTags("Products")
            .WithOpenApi();

        // GET /api/products
        productGroup.MapGet("/", (Services.ProductService productService) =>
        {
            var products = productService.GetAllProducts();
            return Results.Ok(products);
        })
        .WithName("GetAllProducts")
        .WithSummary("Get all products")
        .WithDescription("Retrieves a list of all available products");

        // GET /api/products/{id}
        productGroup.MapGet("/{id:int}", (int id, Services.ProductService productService) =>
        {
            var product = productService.GetProductById(id);
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
        .WithName("GetProductById")
        .WithSummary("Get product by ID")
        .WithDescription("Retrieves a specific product by its ID")
        .Produces(200)
        .Produces(404);

        // POST /api/products
        productGroup.MapPost("/", (CreateProductRequest request, ProductService productService) =>
        {
            // Basic validation - in a real app, you'd use a proper validation library
            var errors = new Dictionary<string, string[]>();
            
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3)
                errors["Name"] = new[] { "Product name must be at least 3 characters" };
            
            if (request.Price <= 0 || request.Price > 10000)
                errors["Price"] = new[] { "Price must be between $0.01 and $10,000" };
            
            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10)
                errors["Description"] = new[] { "Description must be at least 10 characters" };
            
            if (errors.Any())
                return Results.ValidationProblem(errors);

            var product = productService.CreateProduct(request);
            return Results.Created($"/api/products/{product.Id}", product);
        })
        .WithName("CreateProduct")
        .WithSummary("Create a new product")
        .WithDescription("Creates a new product with the provided details")
        .Accepts<Models.CreateProductRequest>("application/json")
        .Produces(201)
        .Produces<ValidationProblemDetails>(400);

        // PUT /api/products/{id}
        productGroup.MapPut("/{id:int}", (int id, UpdateProductRequest request, ProductService productService) =>
        {
            // Basic validation
            var errors = new Dictionary<string, string[]>();
            
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3)
                errors["Name"] = new[] { "Product name must be at least 3 characters" };
            
            if (request.Price <= 0 || request.Price > 10000)
                errors["Price"] = new[] { "Price must be between $0.01 and $10,000" };
            
            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10)
                errors["Description"] = new[] { "Description must be at least 10 characters" };
            
            if (errors.Any())
                return Results.ValidationProblem(errors);

            var product = productService.UpdateProduct(id, request);
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
        .WithName("UpdateProduct")
        .WithSummary("Update an existing product")
        .WithDescription("Updates the details of an existing product")
        .Accepts<Models.UpdateProductRequest>("application/json")
        .Produces(200)
        .Produces(404)
        .Produces<ValidationProblemDetails>(400);

        // DELETE /api/products/{id}
        productGroup.MapDelete("/{id:int}", (int id, ProductService productService) =>
        {
            var deleted = productService.DeleteProduct(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProduct")
        .WithSummary("Delete a product")
        .WithDescription("Deletes a product by its ID")
        .Produces(204)
        .Produces(404);
    }
}
