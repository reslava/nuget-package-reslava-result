using Microsoft.AspNetCore.Mvc;
using NuGetValidationTest.Api.Models;
using REslava.Result;
using Generated.ResultExtensions;

namespace NuGetValidationTest.Api.Endpoints
{
    /// <summary>
    /// Test endpoints to validate all generated HTTP method extensions
    /// Tests REslava.Result.SourceGenerators v1.9.4 with SOLID architecture
    /// </summary>
    public static class TestEndpoints
    {
        /// <summary>
        /// Test basic REslava.Result functionality
        /// </summary>
        public static IResult TestBasicResult()
        {
            // Test basic Result pattern
            var success = Result<string>.Ok("Basic Result test successful!");
            var failure = Result<string>.Fail("Basic Result test failed");

            return Results.Ok(new
            {
                BasicResult = "Working",
                Success = success.IsSuccess,
                Failure = failure.IsFailed,
                SuccessValue = success.IsSuccess ? success.Value : null,
                HasSuccesses = success.Successes.Any(),
                HasErrors = failure.Errors.Any(),
                TotalReasons = success.Reasons.Count + failure.Reasons.Count
            });
        }

        /// <summary>
        /// Test ToIResult() - GET requests
        /// </summary>
        public static IResult TestGetResult(int id)
        {
            if (id <= 0)
                return Result<Product>.Fail("Invalid product ID").ToIResult();

            var product = new Product
            {
                Id = id,
                Name = $"Test Product {id}",
                Price = 99.99m,
                Description = "Test product for GET validation"
            };

            return Result<Product>.Ok(product).ToIResult();
        }

        /// <summary>
        /// Test ToPostResult() - POST requests
        /// </summary>
        public static IResult TestPostResult([FromBody] CreateProductRequest request)
        {
            if (string.IsNullOrEmpty(request.Name))
                return Result<Product>.Fail("Product name is required").ToPostResult();

            if (request.Price <= 0)
                return Result<Product>.Fail("Product price must be positive").ToPostResult();

            var product = new Product
            {
                Id = new Random().Next(1, 1000),
                Name = request.Name,
                Price = request.Price,
                Description = request.Description
            };

            return Result<Product>.Ok(product).ToPostResult();
        }

        /// <summary>
        /// Test ToPutResult() - PUT requests
        /// </summary>
        public static IResult TestPutResult(int id, [FromBody] UpdateProductRequest request)
        {
            if (id <= 0)
                return Result<Product>.Fail("Invalid product ID").ToPutResult();

            if (string.IsNullOrEmpty(request.Name))
                return Result<Product>.Fail("Product name is required").ToPutResult();

            var product = new Product
            {
                Id = id,
                Name = request.Name,
                Price = request.Price,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow.AddDays(-1) // Simulate existing product
            };

            return Result<Product>.Ok(product).ToPutResult();
        }

        /// <summary>
        /// Test ToDeleteResult() - DELETE requests
        /// </summary>
        public static IResult TestDeleteResult(int id)
        {
            if (id <= 0)
                return Result<object>.Fail("Invalid product ID").ToDeleteResult();

            // Simulate successful deletion
            var result = new
            {
                Deleted = true,
                ProductId = id,
                DeletedAt = DateTime.UtcNow,
                Message = $"Product {id} deleted successfully"
            };

            return Result<object>.Ok(result).ToDeleteResult();
        }

        /// <summary>
        /// Test ToPatchResult() - PATCH requests
        /// </summary>
        public static IResult TestPatchResult(int id, [FromBody] UpdateProductRequest request)
        {
            if (id <= 0)
                return Result<Product>.Fail("Invalid product ID").ToPatchResult();

            // Simulate partial update
            var product = new Product
            {
                Id = id,
                Name = string.IsNullOrEmpty(request.Name) ? $"Updated Product {id}" : request.Name,
                Price = request.Price > 0 ? request.Price : 49.99m,
                Description = string.IsNullOrEmpty(request.Description) ? "Partially updated" : request.Description,
                CreatedAt = DateTime.UtcNow.AddDays(-5) // Simulate existing product
            };

            return Result<Product>.Ok(product).ToPatchResult();
        }

        /// <summary>
        /// Test error scenarios with different HTTP status codes
        /// </summary>
        public static IResult TestErrorScenarios(string errorType)
        {
            return errorType.ToLower() switch
            {
                "notfound" => Result<Product>.Fail("Product not found").ToIResult(),
                "validation" => Result<Product>.Fail("Validation failed: Invalid input").ToIResult(),
                "unauthorized" => Result<Product>.Fail("Unauthorized access").ToIResult(),
                "forbidden" => Result<Product>.Fail("Access forbidden").ToIResult(),
                "conflict" => Result<Product>.Fail("Resource conflict").ToIResult(),
                _ => Result<Product>.Fail("Unknown error occurred").ToIResult()
            };
        }
    }
}
