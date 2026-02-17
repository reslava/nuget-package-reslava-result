using FastMinimalAPI.REslava.Result.Demo.Models;
using FastMinimalAPI.REslava.Result.Demo.Services;
using Microsoft.AspNetCore.Mvc;
using REslava.Result;

namespace FastMinimalAPI.REslava.Result.Demo.Endpoints;

/// <summary>
/// User endpoints demonstrating REslava.Result patterns with library domain errors.
/// All methods automatically convert to IResult with proper HTTP status codes.
/// </summary>
public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi();

        // GET /api/users - Get all users
        // Returns: Result<List<UserResponse>> → 200 OK
        group.MapGet("/", async (UserService service) =>
        {
            var result = await service.GetAllUsersAsync();
            return Results.Ok(result.Value);
        })
        .WithName("GetAllUsers")
        .WithSummary("Get all users")
        .WithDescription("Returns a list of all users in the system")
        .Produces<List<UserResponse>>(200);

        // GET /api/users/{id} - Get user by ID
        // Returns: OneOf<NotFoundError, UserResponse>
        // HTTP: 404 Not Found | 200 OK
        group.MapGet("/{id:int}", async (int id, UserService service) =>
        {
            var result = await service.GetUserByIdAsync(id);

            return result.Match(
                case1: error => Results.NotFound(new
                {
                    error = error.Message,
                    userId = id,
                    statusCode = 404
                }),
                case2: user => Results.Ok(user)
            );
        })
        .WithName("GetUserById")
        .WithSummary("Get user by ID")
        .WithDescription("Returns a single user by their ID")
        .Produces<UserResponse>(200)
        .Produces(404);

        // POST /api/users - Create new user
        // Returns: OneOf<ValidationError, ConflictError, UserResponse>
        // HTTP: 422 Unprocessable | 409 Conflict | 201 Created
        group.MapPost("/", async ([FromBody] CreateUserRequest request, UserService service) =>
        {
            var result = await service.CreateUserAsync(request);

            return result.Match(
                case1: validationError => Results.UnprocessableEntity(new
                {
                    error = validationError.Message,
                    field = validationError.FieldName,
                    statusCode = 422
                }),
                case2: conflictError => Results.Conflict(new
                {
                    error = conflictError.Message,
                    statusCode = 409
                }),
                case3: user => Results.Created($"/api/users/{user.Id}", user)
            );
        })
        .WithName("CreateUser")
        .WithSummary("Create a new user")
        .WithDescription("Creates a new user with the provided information")
        .Produces<UserResponse>(201)
        .Produces(409)
        .Produces(422);

        // PUT /api/users/{id} - Update user
        // Returns: OneOf<ValidationError, NotFoundError, ConflictError, UserResponse> (OneOf4!)
        // HTTP: 422 Unprocessable | 404 Not Found | 409 Conflict | 200 OK
        group.MapPut("/{id:int}", async (int id, [FromBody] UpdateUserRequest request, UserService service) =>
        {
            var result = await service.UpdateUserAsync(id, request);

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
                    userId = id,
                    statusCode = 404
                }),
                case3: conflictError => Results.Conflict(new
                {
                    error = conflictError.Message,
                    statusCode = 409
                }),
                case4: user => Results.Ok(user)
            );
        })
        .WithName("UpdateUser")
        .WithSummary("Update an existing user")
        .WithDescription("Updates user information. Demonstrates OneOf4 pattern with 4 possible outcomes.")
        .Produces<UserResponse>(200)
        .Produces(404)
        .Produces(409)
        .Produces(422);

        // DELETE /api/users/{id} - Delete user
        // Returns: Result<bool>
        // HTTP: 409 Conflict (has orders) | 404 Not Found | 204 No Content
        group.MapDelete("/{id:int}", async (int id, UserService service) =>
        {
            var result = await service.DeleteUserAsync(id);

            if (result.IsFailed)
            {
                var error = result.Errors.First();

                // Domain errors carry HttpStatusCode tag — use it
                if (error.Tags.TryGetValue("HttpStatusCode", out var code) && code is int statusCode)
                {
                    return statusCode switch
                    {
                        404 => Results.NotFound(new { error = error.Message, userId = id }),
                        409 => Results.Conflict(new { error = error.Message, userId = id }),
                        _ => Results.Problem(detail: error.Message, statusCode: statusCode)
                    };
                }

                return Results.Problem(detail: error.Message, statusCode: 400);
            }

            return Results.NoContent();
        })
        .WithName("DeleteUser")
        .WithSummary("Delete a user")
        .WithDescription("Deletes a user if they have no associated orders")
        .Produces(204)
        .Produces(404)
        .Produces(409);
    }
}
