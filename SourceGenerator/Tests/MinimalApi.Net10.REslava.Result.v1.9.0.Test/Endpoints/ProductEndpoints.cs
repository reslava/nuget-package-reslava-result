using Microsoft.AspNetCore.Mvc;
using REslava.Result;
using Generated.ResultExtensions;
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
            var result = Result<IEnumerable<Product>>.Ok(products);
            return result.ToIResult(); // 🎯 GENERATED: Result<T> to IResult
        })
        .WithName("GetAllProducts")
        .WithSummary("Get all products")
        .WithDescription("Retrieves a list of all available products");

        // GET /api/products/{id}
        productGroup.MapGet("/{id:int}", (int id, Services.ProductService productService) =>
        {
            if (id <= 0)
                return Result<Product>.Fail("Invalid product ID").ToIResult(); // 🎯 GENERATED: Error to 400
                
            var product = productService.GetProductById(id);
            if (product is null)
                return Result<Product>.Fail("Product not found").ToIResult(); // 🎯 GENERATED: Error to 404
                
            return Result<Product>.Ok(product).ToIResult(); // 🎯 GENERATED: Success to 200
        })
        .WithName("GetProductById")
        .WithSummary("Get product by ID")
        .WithDescription("Retrieves a specific product by its ID")
        .Produces(200)
        .Produces(404);

        // POST /api/products
        productGroup.MapPost("/", (CreateProductRequest request, ProductService productService) =>
        {
            // Basic validation - using Result pattern instead of ValidationProblem
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3)
                return Result<Product>.Fail("Product name must be at least 3 characters").ToIResult(); // 🎯 GENERATED: Validation error to 400
            
            if (request.Price <= 0 || request.Price > 10000)
                return Result<Product>.Fail("Price must be between $0.01 and $10,000").ToIResult(); // 🎯 GENERATED: Validation error to 400
            
            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10)
                return Result<Product>.Fail("Description must be at least 10 characters").ToIResult(); // 🎯 GENERATED: Validation error to 400

            var product = productService.CreateProduct(request);
            var result = Result<Product>.Ok(product);
            return result.ToPostResult(); // 🎯 GENERATED: Success to 201
        })
        .WithName("CreateProduct")
        .WithSummary("Create a new product")
        .WithDescription("Creates a new product with the provided details")
        .Accepts<Models.CreateProductRequest>("application/json")
        .Produces(201)
        .Produces(400);

        // PUT /api/products/{id}
        productGroup.MapPut("/{id:int}", (int id, UpdateProductRequest request, ProductService productService) =>
        {
            // Basic validation
            if (id <= 0)
                return Result<Product>.Fail("Invalid product ID").ToIResult(); // 🎯 GENERATED: Error to 400
                
            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3)
                return Result<Product>.Fail("Product name must be at least 3 characters").ToIResult(); // 🎯 GENERATED: Validation error to 400
            
            if (request.Price <= 0 || request.Price > 10000)
                return Result<Product>.Fail("Price must be between $0.01 and $10,000").ToIResult(); // 🎯 GENERATED: Validation error to 400
            
            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10)
                return Result<Product>.Fail("Description must be at least 10 characters").ToIResult(); // 🎯 GENERATED: Validation error to 400

            var product = productService.UpdateProduct(id, request);
            if (product is null)
                return Result<Product>.Fail("Product not found").ToIResult(); // 🎯 GENERATED: Error to 404
                
            var result = Result<Product>.Ok(product);
            return result.ToPutResult(); // 🎯 GENERATED: Success to 200
        })
        .WithName("UpdateProduct")
        .WithSummary("Update an existing product")
        .WithDescription("Updates the details of an existing product")
        .Accepts<Models.UpdateProductRequest>("application/json")
        .Produces(200)
        .Produces(404)
        .Produces(400);

        // DELETE /api/products/{id}
        productGroup.MapDelete("/{id:int}", (int id, ProductService productService) =>
        {
            if (id <= 0)
                return Result<object>.Fail("Invalid product ID").ToIResult(); // 🎯 GENERATED: Error to 400
                
            var deleted = productService.DeleteProduct(id);
            if (!deleted)
                return Result<object>.Fail("Product not found").ToIResult(); // 🎯 GENERATED: Error to 404
                
            var result = Result<object>.Ok(new { Message = $"Product {id} deleted successfully" });
            return result.ToDeleteResult(); // 🎯 GENERATED: Success to 200
        })
        .WithName("DeleteProduct")
        .WithSummary("Delete a product")
        .WithDescription("Deletes a product by its ID")
        .Produces(200)
        .Produces(404);
    }
}
