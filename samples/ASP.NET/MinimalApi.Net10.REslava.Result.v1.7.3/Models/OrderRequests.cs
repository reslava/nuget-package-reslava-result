using System.ComponentModel.DataAnnotations;

namespace MinimalApi.Net10.Reference.Models;

public record CreateOrderRequest
{
    [Required, EmailAddress(ErrorMessage = "Please provide a valid email address")]
    public required string CustomerEmail { get; init; }

    [Required, MinLength(5, ErrorMessage = "Shipping address is required")]
    [MaxLength(200, ErrorMessage = "Shipping address cannot exceed 200 characters")]
    public required string ShippingAddress { get; init; }

    [Required, MinLength(1, ErrorMessage = "At least one item is required")]
    public required List<CreateOrderItemRequest> Items { get; init; }
}

public record CreateOrderItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Product ID must be valid")]
    public required int ProductId { get; init; }

    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
    public required int Quantity { get; init; }
}

// Complex validation scenario - Advanced order with business rules
public record CreateAdvancedOrderRequest
{
    [Required, EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", 
        ErrorMessage = "Email format is invalid")]
    public required string CustomerEmail { get; init; }

    [Required, MinLength(10, ErrorMessage = "Shipping address must be at least 10 characters")]
    [MaxLength(500, ErrorMessage = "Shipping address cannot exceed 500 characters")]
    public required string ShippingAddress { get; init; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required")]
    [CustomValidation(typeof(CreateAdvancedOrderRequest), nameof(ValidateItems))]
    public required List<CreateAdvancedOrderItemRequest> Items { get; init; }

    [Range(0.01, 50000.00, ErrorMessage = "Total order value cannot exceed $50,000")]
    public decimal? MaxTotalValue { get; init; }

    [DataType(DataType.Date)]
    [FutureDate(ErrorMessage = "Delivery date must be in the future")]
    public DateTime? RequestedDeliveryDate { get; init; }

    // Custom validation method
    public static ValidationResult? ValidateItems(List<CreateAdvancedOrderItemRequest> items, ValidationContext context)
    {
        if (items == null || items.Count == 0)
            return new ValidationResult("At least one item is required");

        if (items.Count > 50)
            return new ValidationResult("Cannot order more than 50 items at once");

        // Check for duplicate products
        var duplicateProducts = items
            .GroupBy(x => x.ProductId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateProducts.Any())
            return new ValidationResult($"Duplicate product IDs found: {string.Join(", ", duplicateProducts)}");

        return ValidationResult.Success;
    }
}

public record CreateAdvancedOrderItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Product ID must be valid")]
    public required int ProductId { get; init; }

    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
    public required int Quantity { get; init; }

    [StringLength(200, ErrorMessage = "Special instructions cannot exceed 200 characters")]
    public string? SpecialInstructions { get; init; }
}

// Custom validation attribute for future dates
public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime date)
        {
            if (date <= DateTime.UtcNow)
            {
                return new ValidationResult(ErrorMessage ?? "Date must be in the future");
            }
        }
        return ValidationResult.Success;
    }
}
