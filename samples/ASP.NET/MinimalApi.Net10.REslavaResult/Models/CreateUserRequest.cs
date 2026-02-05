namespace MinimalApi.Net10.REslavaResult.Models;

/// <summary>
/// Request model for creating a user
/// </summary>
public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
