using REslava.Result;

namespace FastMinimalAPI.REslava.Result.Demo.Errors;

/// <summary>
/// Base validation error for invalid input
/// Maps to HTTP 400 Bad Request
/// </summary>
public class ValidationError : Error
{
    public string Field { get; }
    public object? ProvidedValue { get; }

    public ValidationError(string field, string message, object? providedValue = null)
        : base($"Validation failed for '{field}': {message}")
    {
        Field = field;
        ProvidedValue = providedValue;
        
        this.WithTag("ErrorType", "Validation")
            .WithTag("Field", field)
            .WithTag("StatusCode", 400);
        
        if (providedValue != null)
        {
            this.WithTag("ProvidedValue", providedValue.ToString() ?? "null");
        }
    }
}

/// <summary>
/// Email validation specific error
/// </summary>
public class InvalidEmailError : ValidationError
{
    public InvalidEmailError(string email)
        : base("Email", "Invalid email format", email)
    {
    }
}

/// <summary>
/// Price validation error
/// </summary>
public class InvalidPriceError : ValidationError
{
    public InvalidPriceError(decimal price)
        : base("Price", "Price must be greater than 0", price)
    {
    }
}

/// <summary>
/// Stock quantity validation error
/// </summary>
public class InvalidStockError : ValidationError
{
    public InvalidStockError(int quantity)
        : base("StockQuantity", "Stock quantity cannot be negative", quantity)
    {
    }
}

/// <summary>
/// Required field validation error
/// </summary>
public class RequiredFieldError : ValidationError
{
    public RequiredFieldError(string field)
        : base(field, "This field is required", null)
    {
    }
}
