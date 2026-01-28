using Microsoft.AspNetCore.Mvc;
using MinimalApi.Net10.Reference.Models;
using MinimalApi.Net10.Reference.Services;
using Generated.ResultExtensions;

namespace MinimalApi.Net10.Reference.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var productGroup = app.MapGroup("/api/products")
            .WithTags("Products")
            .WithOpenApi();

        // GET /api/products
        productGroup.MapGet("/", (ProductService productService) =>
        {
            // 🎯 MAGIC: Result<IEnumerable<Product>> automatically converted to HTTP response!
            // Success -> 200 OK with products
            // Failure -> 500 Internal Server Error with error details
            return productService.GetAllProducts().ToIResult();
        })
        .WithName("GetAllProducts")
        .WithSummary("Get all products")
        .WithDescription("Retrieves a list of all available products");

        // GET /api/products/{id}
        productGroup.MapGet("/{id:int}", (int id, ProductService productService) =>
        {
            // 🎯 MAGIC: Result<Product> automatically converted to HTTP response!
            // Success -> 200 OK with product
            // Failure -> 404 Not Found with error message
            return productService.GetProductById(id).ToIResult();
        })
        .WithName("GetProductById")
        .WithSummary("Get product by ID")
        .WithDescription("Retrieves a specific product by its ID")
        .Produces(200)
        .Produces(404);

        // POST /api/products
        productGroup.MapPost("/", (CreateProductRequest request, ProductService productService) =>
        {
            // 🎯 MAGIC: Result<Product> automatically converted to HTTP response!
            // Success -> 201 Created with product
            // Failure -> 400 Bad Request with validation errors
            return productService.CreateProduct(request).ToIResult();
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
            // 🎯 MAGIC: Result<Product> automatically converted to HTTP response!
            // Success -> 200 OK with updated product
            // Failure -> 404 Not Found or 400 Bad Request
            return productService.UpdateProduct(id, request).ToIResult();
        })
        .WithName("UpdateProduct")
        .WithSummary("Update an existing product")
        .WithDescription("Updates the details of an existing product")
        .Accepts<UpdateProductRequest>("application/json")
        .Produces(200)
        .Produces(404)
        .Produces<ValidationProblemDetails>(400);

        // DELETE /api/products/{id}
        productGroup.MapDelete("/{id:int}", (int id, ProductService productService) =>
        {
            // 🎯 MAGIC: Result<bool> automatically converted to HTTP response!
            // Success -> 204 No Content
            // Failure -> 404 Not Found
            return productService.DeleteProduct(id).ToIResult();
        })
        .WithName("DeleteProduct")
        .WithSummary("Delete a product")
        .WithDescription("Deletes a product by its ID")
        .Produces(204)
        .Produces(404);
    }
}
