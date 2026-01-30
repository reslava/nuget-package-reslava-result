using Microsoft.AspNetCore.Mvc;
using REslava.Result;
using Generated.ResultExtensions;
using MinimalApi.Net10.Reference.Models;
using MinimalApi.Net10.Reference.Services;

namespace MinimalApi.Net10.Reference.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var orderGroup = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .WithOpenApi();

        // GET /api/orders
        orderGroup.MapGet("/", (OrderService orderService) =>
        {
            var orders = orderService.GetAllOrders();
            var result = Result<IEnumerable<Order>>.Ok(orders);
            return result.ToIResult(); // 🎯 GENERATED: Result<T> to IResult
        })
        .WithName("GetAllOrders")
        .WithSummary("Get all orders")
        .WithDescription("Retrieves a list of all orders");

        // GET /api/orders/{id}
        orderGroup.MapGet("/{id:int}", (int id, OrderService orderService) =>
        {
            if (id <= 0)
                return Result<Order>.Fail("Invalid order ID").ToIResult(); // 🎯 GENERATED: Error to 400
                
            var order = orderService.GetOrderById(id);
            if (order is null)
                return Result<Order>.Fail("Order not found").ToIResult(); // 🎯 GENERATED: Error to 404
                
            return Result<Order>.Ok(order).ToIResult(); // 🎯 GENERATED: Success to 200
        })
        .WithName("GetOrderById")
        .WithSummary("Get order by ID")
        .WithDescription("Retrieves a specific order by its ID")
        .Produces(200)
        .Produces(404);

        // POST /api/orders - Simple order creation
        orderGroup.MapPost("/", (CreateOrderRequest request, OrderService orderService) =>
        {
            // Basic validation - using Result pattern instead of ValidationProblem
            if (string.IsNullOrWhiteSpace(request.CustomerEmail) || !request.CustomerEmail.Contains('@'))
                return Result<Order>.Fail("Valid email address is required").ToIResult(); // 🎯 GENERATED: Validation error to 400
            
            if (string.IsNullOrWhiteSpace(request.ShippingAddress) || request.ShippingAddress.Length < 5)
                return Result<Order>.Fail("Shipping address must be at least 5 characters").ToIResult(); // 🎯 GENERATED: Validation error to 400
            
            if (request.Items == null || !request.Items.Any())
                return Result<Order>.Fail("At least one item is required").ToIResult(); // 🎯 GENERATED: Validation error to 400

            var order = orderService.CreateOrder(request);
            if (order is null)
                return Result<Order>.Fail("One or more products are invalid or out of stock").ToIResult(); // 🎯 GENERATED: Error to 400
                
            var result = Result<Order>.Ok(order);
            return result.ToPostResult(); // 🎯 GENERATED: Success to 201
        })
        .WithName("CreateOrder")
        .WithSummary("Create a new order")
        .WithDescription("Creates a new order with the provided items")
        .Accepts<CreateOrderRequest>("application/json")
        .Produces(201)
        .Produces(400);

        // POST /api/orders/advanced - Complex order with advanced validation
        orderGroup.MapPost("/advanced", (CreateAdvancedOrderRequest request, OrderService orderService) =>
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(request.CustomerEmail) || !request.CustomerEmail.Contains('@'))
                return Result<Order>.Fail("Valid email address is required").ToIResult(); // 🎯 GENERATED: Validation error to 400
            
            if (string.IsNullOrWhiteSpace(request.ShippingAddress) || request.ShippingAddress.Length < 10)
                return Result<Order>.Fail("Shipping address must be at least 10 characters").ToIResult(); // 🎯 GENERATED: Validation error to 400
            
            if (request.Items == null || !request.Items.Any())
                return Result<Order>.Fail("At least one item is required").ToIResult(); // 🎯 GENERATED: Validation error to 400
            
            if (request.Items?.Count > 50)
                return Result<Order>.Fail("Cannot order more than 50 items at once").ToIResult(); // 🎯 GENERATED: Validation error to 400

            var order = orderService.CreateAdvancedOrder(request);
            if (order is null)
                return Result<Order>.Fail("Order validation failed or products unavailable").ToIResult(); // 🎯 GENERATED: Error to 400
                
            var result = Result<Order>.Ok(order);
            return result.ToPostResult(); // 🎯 GENERATED: Success to 201
        })
        .WithName("CreateAdvancedOrder")
        .WithSummary("Create an advanced order with enhanced validation")
        .WithDescription("Creates a new order with advanced business rules and validation")
        .Accepts<CreateAdvancedOrderRequest>("application/json")
        .Produces(201)
        .Produces(400);

        // PATCH /api/orders/{id}/status
        orderGroup.MapPatch("/{id:int}/status", (int id, OrderStatus status, OrderService orderService) =>
        {
            if (id <= 0)
                return Result<Order>.Fail("Invalid order ID").ToIResult(); // 🎯 GENERATED: Error to 400
                
            var order = orderService.UpdateOrderStatus(id, status);
            if (order is null)
                return Result<Order>.Fail("Order not found").ToIResult(); // 🎯 GENERATED: Error to 404
                
            var result = Result<Order>.Ok(order);
            return result.ToPatchResult(); // 🎯 GENERATED: Success to 200
        })
        .WithName("UpdateOrderStatus")
        .WithSummary("Update order status")
        .WithDescription("Updates the status of an existing order")
        .Accepts<OrderStatus>("application/json")
        .Produces(200)
        .Produces(404);

        // DELETE /api/orders/{id}
        orderGroup.MapDelete("/{id:int}", (int id, OrderService orderService) =>
        {
            if (id <= 0)
                return Result<object>.Fail("Invalid order ID").ToIResult(); // 🎯 GENERATED: Error to 400
                
            var deleted = orderService.DeleteOrder(id);
            if (!deleted)
                return Result<object>.Fail("Order not found").ToIResult(); // 🎯 GENERATED: Error to 404
                
            var result = Result<object>.Ok(new { Message = $"Order {id} deleted successfully" });
            return result.ToDeleteResult(); // 🎯 GENERATED: Success to 200
        })
        .WithName("DeleteOrder")
        .WithSummary("Delete an order")
        .WithDescription("Deletes an order by its ID")
        .Produces(200)
        .Produces(404);
    }
}
