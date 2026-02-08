using REslava.Result;

namespace FastMinimalAPI.REslava.Result.Demo.Errors;

/// <summary>
/// Conflict error when resource already exists
/// Maps to HTTP 409 Conflict
/// </summary>
public class ConflictError : Error
{
    public string Field { get; }
    public object Value { get; }

    public ConflictError(string field, object value, string message)
        : base(message)
    {
        Field = field;
        Value = value;
        
        this.WithTag("ErrorType", "Conflict")
            .WithTag("Field", field)
            .WithTag("Value", value.ToString() ?? "null")
            .WithTag("StatusCode", 409);
    }
}

/// <summary>
/// Duplicate email error
/// </summary>
public class DuplicateEmailError : ConflictError
{
    public DuplicateEmailError(string email)
        : base("Email", email, $"User with email '{email}' already exists")
    {
    }
}

/// <summary>
/// Insufficient stock error
/// Maps to HTTP 409 Conflict (business rule violation)
/// </summary>
public class InsufficientStockError : Error
{
    public int ProductId { get; }
    public string ProductName { get; }
    public int Requested { get; }
    public int Available { get; }

    public InsufficientStockError(int productId, string productName, int requested, int available)
        : base($"Insufficient stock for product '{productName}'. Requested: {requested}, Available: {available}")
    {
        ProductId = productId;
        ProductName = productName;
        Requested = requested;
        Available = available;
        
        this.WithTag("ErrorType", "InsufficientStock")
            .WithTag("ProductId", productId)
            .WithTag("ProductName", productName)
            .WithTag("Requested", requested)
            .WithTag("Available", available)
            .WithTag("StatusCode", 409);
    }
}

/// <summary>
/// Product unavailable error
/// </summary>
public class ProductUnavailableError : Error
{
    public int ProductId { get; }
    public string ProductName { get; }

    public ProductUnavailableError(int productId, string productName)
        : base($"Product '{productName}' is currently unavailable")
    {
        ProductId = productId;
        ProductName = productName;
        
        this.WithTag("ErrorType", "ProductUnavailable")
            .WithTag("ProductId", productId)
            .WithTag("ProductName", productName)
            .WithTag("StatusCode", 409);
    }
}

/// <summary>
/// Empty order error
/// </summary>
public class EmptyOrderError : ValidationError
{
    public EmptyOrderError()
        : base("Items", "Order must contain at least one item", null)
    {
    }
}

/// <summary>
/// User inactive error
/// </summary>
public class UserInactiveError : Error
{
    public int UserId { get; }

    public UserInactiveError(int userId)
        : base($"User with ID '{userId}' is inactive and cannot place orders")
    {
        UserId = userId;
        
        this.WithTag("ErrorType", "UserInactive")
            .WithTag("UserId", userId)
            .WithTag("StatusCode", 403);
    }
}
