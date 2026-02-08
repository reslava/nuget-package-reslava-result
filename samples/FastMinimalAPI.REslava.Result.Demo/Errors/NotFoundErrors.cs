using REslava.Result;

namespace FastMinimalAPI.REslava.Result.Demo.Errors;

/// <summary>
/// Base error for resources not found
/// Maps to HTTP 404 Not Found
/// </summary>
public class NotFoundError : Error
{
    public string ResourceType { get; }
    public object ResourceId { get; }

    public NotFoundError(string resourceType, object resourceId)
        : base($"{resourceType} with ID '{resourceId}' not found")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
        
        this.WithTag("ErrorType", "NotFound")
            .WithTag("ResourceType", resourceType)
            .WithTag("ResourceId", resourceId.ToString() ?? "null")
            .WithTag("StatusCode", 404);
    }
}

/// <summary>
/// User not found error
/// </summary>
public class UserNotFoundError : NotFoundError
{
    public UserNotFoundError(int userId)
        : base("User", userId)
    {
    }
}

/// <summary>
/// Product not found error
/// </summary>
public class ProductNotFoundError : NotFoundError
{
    public ProductNotFoundError(int productId)
        : base("Product", productId)
    {
    }
}

/// <summary>
/// Order not found error
/// </summary>
public class OrderNotFoundError : NotFoundError
{
    public OrderNotFoundError(int orderId)
        : base("Order", orderId)
    {
    }
}
