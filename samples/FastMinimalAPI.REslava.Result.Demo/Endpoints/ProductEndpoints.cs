using FastMinimalAPI.REslava.Result.Demo.Models;
using FastMinimalAPI.REslava.Result.Demo.Services;
using Microsoft.AspNetCore.Mvc;
using REslava.Result;

namespace FastMinimalAPI.REslava.Result.Demo.Endpoints;

/// <summary>
/// Product endpoints demonstrating inventory management with REslava.Result.
/// Uses library domain errors — no custom error classes needed.
/// Showcases how ValidationError.FieldName distinguishes different validation failures
/// without needing separate error types (InvalidPriceError, InvalidStockError, etc.)
/// </summary>
public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .WithOpenApi();

        // GET /api/products - Get all products
        // Returns: Result<List<ProductResponse>> → 200 OK
        group.MapGet("/", async (ProductService service) =>
        {
            var result = await service.GetAllProductsAsync();
            return Results.Ok(result.Value);
        })
        .WithName("GetAllProducts")
        .WithSummary("Get all products")
        .WithDescription("Returns a list of all products with stock information")
        .Produces<List<ProductResponse>>(200);

        // GET /api/products/{id} - Get product by ID
        // Returns: OneOf<NotFoundError, ProductResponse>
        // HTTP: 404 Not Found | 200 OK
        group.MapGet("/{id:int}", async (int id, ProductService service) =>
        {
            var result = await service.GetProductByIdAsync(id);

            return result.Match(
                case1: error => Results.NotFound(new
                {
                    error = error.Message,
                    productId = id,
                    statusCode = 404
                }),
                case2: product => Results.Ok(product)
            );
        })
        .WithName("GetProductById")
        .WithSummary("Get product by ID")
        .WithDescription("Returns a single product with stock and availability information")
        .Produces<ProductResponse>(200)
        .Produces(404);

        // GET /api/products/category/{category} - Get products by category
        // Returns: Result<List<ProductResponse>> → 200 OK
        group.MapGet("/category/{category}", async (string category, ProductService service) =>
        {
            var result = await service.GetProductsByCategoryAsync(category);
            return Results.Ok(result.Value);
        })
        .WithName("GetProductsByCategory")
        .WithSummary("Get products by category")
        .WithDescription("Returns all products in a specific category (Electronics, Accessories, Peripherals)")
        .Produces<List<ProductResponse>>(200);

        // POST /api/products - Create new product
        // Returns: OneOf<ValidationError, ProductResponse> (simplified from OneOf3!)
        // HTTP: 422 Unprocessable | 201 Created
        group.MapPost("/", async ([FromBody] CreateProductRequest request, ProductService service) =>
        {
            var result = await service.CreateProductAsync(request);

            return result.Match(
                case1: validationError => Results.UnprocessableEntity(new
                {
                    error = validationError.Message,
                    field = validationError.FieldName,
                    statusCode = 422
                }),
                case2: product => Results.Created($"/api/products/{product.Id}", product)
            );
        })
        .WithName("CreateProduct")
        .WithSummary("Create a new product")
        .WithDescription("Creates a new product with inventory tracking. ValidationError.FieldName distinguishes price vs stock vs required field failures.")
        .Produces<ProductResponse>(201)
        .Produces(422);

        // PUT /api/products/{id} - Update product
        // Returns: OneOf<ValidationError, NotFoundError, ProductResponse> (simplified from OneOf4!)
        // HTTP: 422 Unprocessable | 404 Not Found | 200 OK
        group.MapPut("/{id:int}", async (int id, [FromBody] UpdateProductRequest request, ProductService service) =>
        {
            var result = await service.UpdateProductAsync(id, request);

            return result.Match(
                case1: validationError => Results.UnprocessableEntity(new
                {
                    error = validationError.Message,
                    field = validationError.FieldName,
                    statusCode = 422
                }),
                case2: notFoundError => Results.NotFound(new
                {
                    error = notFoundError.Message,
                    productId = id,
                    statusCode = 404
                }),
                case3: product => Results.Ok(product)
            );
        })
        .WithName("UpdateProduct")
        .WithSummary("Update an existing product")
        .WithDescription("Updates product information including price and stock. Simplified from OneOf4 to OneOf3 by using ValidationError for all validation failures.")
        .Produces<ProductResponse>(200)
        .Produces(404)
        .Produces(422);

        // DELETE /api/products/{id} - Delete product
        // Returns: Result<bool>
        // HTTP: 404 Not Found | 204 No Content
        group.MapDelete("/{id:int}", async (int id, ProductService service) =>
        {
            var result = await service.DeleteProductAsync(id);

            if (result.IsFailed)
            {
                var error = result.Errors.First();
                return Results.NotFound(new
                {
                    error = error.Message,
                    productId = id,
                    statusCode = 404
                });
            }

            return Results.NoContent();
        })
        .WithName("DeleteProduct")
        .WithSummary("Delete a product")
        .WithDescription("Deletes a product from the catalog")
        .Produces(204)
        .Produces(404);

        // PATCH /api/products/{id}/stock - Update product stock
        // Returns: OneOf<NotFoundError, ValidationError, ProductResponse>
        // HTTP: 404 Not Found | 422 Unprocessable | 200 OK
        group.MapPatch("/{id:int}/stock", async (int id, [FromBody] UpdateStockRequest request, ProductService service) =>
        {
            var product = await service.GetProductByIdAsync(id);

            return await product.Match(
                case1: error => Task.FromResult<IResult>(Results.NotFound(new
                {
                    error = error.Message,
                    productId = id,
                    statusCode = 404
                })),
                case2: async productData =>
                {
                    // Create update request with new stock (only update StockQuantity)
                    var updateRequest = new UpdateProductRequest(
                        Name: null,
                        Description: null,
                        Price: null,
                        StockQuantity: request.Stock,
                        Category: null,
                        IsAvailable: null
                    );

                    var updateResult = await service.UpdateProductAsync(id, updateRequest);

                    return updateResult.Match(
                        case1: validationError => Results.UnprocessableEntity(new { error = validationError.Message, statusCode = 422 }),
                        case2: notFoundError => Results.NotFound(new { error = notFoundError.Message, statusCode = 404 }),
                        case3: updatedProduct => Results.Ok(updatedProduct)
                    );
                }
            );
        })
        .WithName("UpdateProductStock")
        .WithSummary("Update product stock quantity")
        .WithDescription("Updates the stock quantity for a product and automatically updates availability status")
        .Produces<ProductResponse>(200)
        .Produces(404)
        .Produces(422);
    }
}

/// <summary>
/// Request model for updating stock
/// </summary>
public record UpdateStockRequest(int Stock);
