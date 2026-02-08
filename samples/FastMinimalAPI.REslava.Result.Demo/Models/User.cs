namespace FastMinimalAPI.REslava.Result.Demo.Models;

/// <summary>
/// User entity representing application users
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation properties
    public List<Order> Orders { get; set; } = new();
}

/// <summary>
/// DTO for creating a new user
/// </summary>
public record CreateUserRequest(string Email, string Name, string? Role = null);

/// <summary>
/// DTO for updating an existing user
/// </summary>
public record UpdateUserRequest(string? Email = null, string? Name = null, string? Role = null, bool? IsActive = null);

/// <summary>
/// DTO for user response
/// </summary>
public record UserResponse(int Id, string Email, string Name, string Role, bool IsActive, DateTime CreatedAt, int OrderCount);
