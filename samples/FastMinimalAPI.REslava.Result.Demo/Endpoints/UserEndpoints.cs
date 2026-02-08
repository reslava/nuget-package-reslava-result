using FastMinimalAPI.REslava.Result.Demo.Models;
using FastMinimalAPI.REslava.Result.Demo.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastMinimalAPI.REslava.Result.Demo.Endpoints;

/// <summary>
/// User endpoints demonstrating REslava.Result patterns
/// All methods automatically convert to IResult with proper HTTP status codes
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
        // Returns: OneOf<UserNotFoundError, UserResponse>
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
        // Returns: OneOf<ValidationError, DuplicateEmailError, UserResponse>
        // HTTP: 400 Bad Request | 409 Conflict | 201 Created
        group.MapPost("/", async ([FromBody] CreateUserRequest request, UserService service) =>
        {
            var result = await service.CreateUserAsync(request);
            
            return result.Match(
                case1: validationError => Results.BadRequest(new
                {
                    error = validationError.Message,
                    field = validationError.Tags.ContainsKey("Field") ? validationError.Tags["Field"] : null,
                    statusCode = 400
                }),
                case2: duplicateError => Results.Conflict(new
                {
                    error = duplicateError.Message,
                    email = duplicateError.Tags.ContainsKey("Email") ? duplicateError.Tags["Email"] : null,
                    statusCode = 409
                }),
                case3: user => Results.Created($"/api/users/{user.Id}", user)
            );
        })
        .WithName("CreateUser")
        .WithSummary("Create a new user")
        .WithDescription("Creates a new user with the provided information")
        .Produces<UserResponse>(201)
        .Produces(400)
        .Produces(409);

        // PUT /api/users/{id} - Update user
        // Returns: OneOf<ValidationError, UserNotFoundError, DuplicateEmailError, UserResponse> (OneOf4!)
        // HTTP: 400 Bad Request | 404 Not Found | 409 Conflict | 200 OK
        group.MapPut("/{id:int}", async (int id, [FromBody] UpdateUserRequest request, UserService service) =>
        {
            var result = await service.UpdateUserAsync(id, request);
            
            return result.Match(
                case1: validationError => Results.BadRequest(new
                {
                    error = validationError.Message,
                    field = validationError.Tags.ContainsKey("Field") ? validationError.Tags["Field"] : null,
                    statusCode = 400
                }),
                case2: notFoundError => Results.NotFound(new
                {
                    error = notFoundError.Message,
                    userId = id,
                    statusCode = 404
                }),
                case3: duplicateError => Results.Conflict(new
                {
                    error = duplicateError.Message,
                    email = duplicateError.Tags.ContainsKey("Email") ? duplicateError.Tags["Email"] : null,
                    statusCode = 409
                }),
                case4: user => Results.Ok(user)
            );
        })
        .WithName("UpdateUser")
        .WithSummary("Update an existing user")
        .WithDescription("Updates user information. Demonstrates OneOf4 pattern with 4 possible outcomes.")
        .Produces<UserResponse>(200)
        .Produces(400)
        .Produces(404)
        .Produces(409);

        // DELETE /api/users/{id} - Delete user
        // Returns: Result<bool>
        // HTTP: 409 Conflict (has orders) | 404 Not Found | 204 No Content
        group.MapDelete("/{id:int}", async (int id, UserService service) =>
        {
            var result = await service.DeleteUserAsync(id);
            
            if (result.IsFailed)
            {
                var error = result.Errors.First();
                var statusCode = error.Tags.ContainsKey("StatusCode") 
                    ? Convert.ToInt32(error.Tags["StatusCode"]) 
                    : 400;
                
                return statusCode == 404 
                    ? Results.NotFound(new { error = error.Message, userId = id })
                    : Results.Conflict(new { error = error.Message, userId = id });
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
