using Microsoft.AspNetCore.Mvc;
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
            return Results.Ok(orders);
        })
        .WithName("GetAllOrders")
        .WithSummary("Get all orders")
        .WithDescription("Retrieves a list of all orders");

        // GET /api/orders/{id}
        orderGroup.MapGet("/{id:int}", (int id, OrderService orderService) =>
        {
            var order = orderService.GetOrderById(id);
            return order is null ? Results.NotFound() : Results.Ok(order);
        })
        .WithName("GetOrderById")
        .WithSummary("Get order by ID")
        .WithDescription("Retrieves a specific order by its ID")
        .Produces(200)
        .Produces(404);

        // POST /api/orders - Simple order creation
        orderGroup.MapPost("/", (CreateOrderRequest request, OrderService orderService) =>
        {
            // Basic validation
            var errors = new Dictionary<string, string[]>();
            
            if (string.IsNullOrWhiteSpace(request.CustomerEmail) || !request.CustomerEmail.Contains('@'))
                errors["CustomerEmail"] = new[] { "Valid email address is required" };
            
            if (string.IsNullOrWhiteSpace(request.ShippingAddress) || request.ShippingAddress.Length < 5)
                errors["ShippingAddress"] = new[] { "Shipping address must be at least 5 characters" };
            
            if (request.Items == null || !request.Items.Any())
                errors["Items"] = new[] { "At least one item is required" };
            
            if (errors.Any())
                return Results.ValidationProblem(errors);

            var order = orderService.CreateOrder(request);
            return order is null 
                ? Results.BadRequest("One or more products are invalid or out of stock")
                : Results.Created($"/api/orders/{order.Id}", order);
        })
        .WithName("CreateOrder")
        .WithSummary("Create a new order")
        .WithDescription("Creates a new order with the provided items")
        .Accepts<CreateOrderRequest>("application/json")
        .Produces(201)
        .Produces<ValidationProblemDetails>(400)
        .Produces<string>(400);

        // POST /api/orders/advanced - Complex order with advanced validation
        orderGroup.MapPost("/advanced", (CreateAdvancedOrderRequest request, OrderService orderService) =>
        {
            // Basic validation
            var errors = new Dictionary<string, string[]>();
            
            if (string.IsNullOrWhiteSpace(request.CustomerEmail) || !request.CustomerEmail.Contains('@'))
                errors["CustomerEmail"] = new[] { "Valid email address is required" };
            
            if (string.IsNullOrWhiteSpace(request.ShippingAddress) || request.ShippingAddress.Length < 10)
                errors["ShippingAddress"] = new[] { "Shipping address must be at least 10 characters" };
            
            if (request.Items == null || !request.Items.Any())
                errors["Items"] = new[] { "At least one item is required" };
            
            if (request.Items?.Count > 50)
                errors["Items"] = new[] { "Cannot order more than 50 items at once" };
            
            if (errors.Any())
                return Results.ValidationProblem(errors);

            var order = orderService.CreateAdvancedOrder(request);
            return order is null 
                ? Results.BadRequest("Order validation failed or products unavailable")
                : Results.Created($"/api/orders/{order.Id}", order);
        })
        .WithName("CreateAdvancedOrder")
        .WithSummary("Create an advanced order with enhanced validation")
        .WithDescription("Creates a new order with advanced business rules and validation")
        .Accepts<CreateAdvancedOrderRequest>("application/json")
        .Produces(201)
        .Produces<ValidationProblemDetails>(400)
        .Produces<string>(400);

        // PATCH /api/orders/{id}/status
        orderGroup.MapPatch("/{id:int}/status", (int id, OrderStatus status, OrderService orderService) =>
        {
            var order = orderService.UpdateOrderStatus(id, status);
            return order is null ? Results.NotFound() : Results.Ok(order);
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
            var deleted = orderService.DeleteOrder(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteOrder")
        .WithSummary("Delete an order")
        .WithDescription("Deletes an order by its ID")
        .Produces(204)
        .Produces(404);
    }
}
