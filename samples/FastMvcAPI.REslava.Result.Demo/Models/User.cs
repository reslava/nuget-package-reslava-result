namespace FastMvcAPI.REslava.Result.Demo.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public List<Order> Orders { get; set; } = new();
}

public record CreateUserRequest(string Email, string Name, string? Role = null);
public record UpdateUserRequest(string? Email = null, string? Name = null, string? Role = null, bool? IsActive = null);
public record UserResponse(int Id, string Email, string Name, string Role, bool IsActive, DateTime CreatedAt, int OrderCount);
