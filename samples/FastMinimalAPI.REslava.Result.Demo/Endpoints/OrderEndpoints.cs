using FastMinimalAPI.REslava.Result.Demo.Errors;
using FastMinimalAPI.REslava.Result.Demo.Models;
using FastMinimalAPI.REslava.Result.Demo.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastMinimalAPI.REslava.Result.Demo.Endpoints;

/// <summary>
/// Order management endpoints demonstrating complex OneOf4 patterns for multi-step business logic.
/// Shows how to handle multiple error types in a single operation (user validation, stock checking, etc.)
/// </summary>
public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .WithOpenApi();

        // GET /api/orders - Get all orders
        group.MapGet("/", async (OrderService orderService) =>
        {
            var result = await orderService.GetAllOrdersAsync();
            
            return result.Match(
                onSuccess: orders => Results.Ok(new { 
                    success = true, 
                    data = orders,
                    count = orders.Count 
                }),
                onFailure: errors => Results.Problem(
                    detail: string.Join(", ", errors.Select(e => e.Message)),
                    statusCode: 500
                )
            );
        })
        .WithName("GetAllOrders")
        .WithSummary("Get all orders")
        .Produces<object>(200)
        .Produces(500);

        // GET /api/orders/{id} - Get order by ID
        group.MapGet("/{id:int}", async (int id, OrderService orderService) =>
        {
            var result = await orderService.GetOrderByIdAsync(id);
            
            return result.Match(
                case1: notFound => Results.NotFound(new { 
                    success = false, 
                    error = notFound.Message 
                }),
                case2: order => Results.Ok(new { 
                    success = true, 
                    data = order 
                })
            );
        })
        .WithName("GetOrderById")
        .WithSummary("Get a specific order by ID")
        .Produces<object>(200)
        .Produces<object>(404);

        // GET /api/orders/user/{userId} - Get orders by user ID
        group.MapGet("/user/{userId:int}", async (int userId, OrderService orderService) =>
        {
            var result = await orderService.GetOrdersByUserIdAsync(userId);
            
            return result.Match(
                case1: userNotFound => Results.NotFound(new { 
                    success = false, 
                    error = userNotFound.Message,
                    errorType = "UserNotFound" 
                }),
                case2: orders => Results.Ok(new { 
                    success = true, 
                    data = orders,
                    count = orders.Count 
                })
            );
        })
        .WithName("GetOrdersByUser")
        .WithSummary("Get all orders for a specific user")
        .Produces<object>(200)
        .Produces<object>(404);

        // POST /api/orders - Create new order (demonstrates OneOf4 - complex pattern with 4 error types!)
        group.MapPost("/", async ([FromBody] CreateOrderRequest request, OrderService orderService) =>
        {
            var result = await orderService.CreateOrderAsync(request);
            
            // OneOf4 pattern - handling 4 different error types and success!
            // Note: EmptyOrderError is now a ValidationError (both are 400-level errors)
            return result.Match(
                case1: userNotFound => Results.NotFound(new { 
                    success = false, 
                    error = userNotFound.Message,
                    errorType = "UserNotFound",
                    field = "UserId"
                }),
                case2: userInactive => Results.StatusCode(403, new { 
                    success = false, 
                    error = userInactive.Message,
                    errorType = "UserInactive",
                    details = "User account is not active"
                }),
                case3: insufficientStock => Results.StatusCode(409, new { 
                    success = false, 
                    error = insufficientStock.Message,
                    errorType = "InsufficientStock",
                    productId = insufficientStock.Tags.GetValueOrDefault("ProductId"),
                    requested = insufficientStock.Tags.GetValueOrDefault("RequestedQuantity"),
                    available = insufficientStock.Tags.GetValueOrDefault("AvailableStock")
                }),
                case4: validation => Results.BadRequest(new { 
                    success = false, 
                    error = validation.Message,
                    errorType = "ValidationError",
                    field = validation.Tags.GetValueOrDefault("Field"),
                    reason = validation.Tags.GetValueOrDefault("Reason")
                }),
                case5: order => Results.Created($"/api/orders/{order.Id}", new { 
                    success = true, 
                    data = order,
                    message = "Order created successfully"
                })
            );
        })
        .WithName("CreateOrder")
        .WithSummary("Create a new order (demonstrates OneOf4 pattern)")
        .WithDescription("Complex validation: checks user exists, user active, stock available, order not empty (4 error types)")
        .Produces<object>(201)
        .Produces<object>(400)
        .Produces<object>(403)
        .Produces<object>(404)
        .Produces<object>(409);

        // PATCH /api/orders/{id}/status - Update order status
        group.MapPatch("/{id:int}/status", async (
            int id, 
            [FromBody] UpdateOrderStatusRequest request, 
            OrderService orderService) =>
        {
            var result = await orderService.UpdateOrderStatusAsync(id, request.Status);
            
            return result.Match(
                case1: notFound => Results.NotFound(new { 
                    success = false, 
                    error = notFound.Message 
                }),
                case2: validation => Results.BadRequest(new { 
                    success = false, 
                    error = validation.Message,
                    field = validation.Tags.GetValueOrDefault("Field")
                }),
                case3: order => Results.Ok(new { 
                    success = true, 
                    data = order,
                    message = $"Order status updated to {request.Status}"
                })
            );
        })
        .WithName("UpdateOrderStatus")
        .WithSummary("Update an order's status")
        .Produces<object>(200)
        .Produces<object>(400)
        .Produces<object>(404);

        // DELETE /api/orders/{id}/cancel - Cancel order (restores stock)
        group.MapDelete("/{id:int}/cancel", async (int id, OrderService orderService) =>
        {
            var result = await orderService.CancelOrderAsync(id);
            
            return result.Match(
                case1: notFound => Results.NotFound(new { 
                    success = false, 
                    error = notFound.Message 
                }),
                case2: validation => Results.BadRequest(new { 
                    success = false, 
                    error = validation.Message,
                    details = "Order cannot be cancelled in current status"
                }),
                case3: order => Results.Ok(new { 
                    success = true, 
                    data = order,
                    message = "Order cancelled successfully and stock restored"
                })
            );
        })
        .WithName("CancelOrder")
        .WithSummary("Cancel an order and restore stock")
        .WithDescription("Can only cancel orders with status Pending or Processing")
        .Produces<object>(200)
        .Produces<object>(400)
        .Produces<object>(404);

        // GET /api/orders/user/{userId}/statistics - Get order statistics for user
        group.MapGet("/user/{userId:int}/statistics", async (int userId, OrderService orderService) =>
        {
            var result = await orderService.GetUserOrderStatisticsAsync(userId);
            
            return result.Match(
                onSuccess: stats => Results.Ok(new { 
                    success = true, 
                    data = stats 
                }),
                onFailure: errors => Results.Problem(
                    detail: string.Join(", ", errors.Select(e => e.Message)),
                    statusCode: 500
                )
            );
        })
        .WithName("GetUserOrderStatistics")
        .WithSummary("Get order statistics for a user")
        .Produces<object>(200)
        .Produces(500);
    }
}

// Request DTOs
public record UpdateOrderStatusRequest(OrderStatus Status);
