namespace MinimalApi.Net10.REslavaResult.Models;

/// <summary>
/// Product model for OneOf4 demonstration
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
