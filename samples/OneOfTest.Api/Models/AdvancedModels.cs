namespace OneOfTest.Api.Models;

/// <summary>
/// Additional model types for comprehensive OneOf testing
/// </summary>
public class UserUpdated
{
    public User Value { get; set; } = new User();
    public string Message { get; set; } = "User updated successfully";
}

public class UserDeleted
{
    public int DeletedUserId { get; set; }
    public string Message { get; set; } = "User deleted successfully";
}

public class UnauthorizedAccess
{
    public string Reason { get; set; } = "Unauthorized access";
    public string RequiredPermission { get; set; } = "admin";
}

public class ServerError
{
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
