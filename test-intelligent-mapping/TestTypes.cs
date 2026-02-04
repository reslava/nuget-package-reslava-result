// Define error types for testing (no inheritance to avoid conflicts)
public record ValidationError(string Message);
public record UserNotFoundError(int UserId, string Message = "User not found");
public record ConflictError(string Message);
public record DatabaseError(string Message);
public record UnauthorizedError(string Message);
public record ForbiddenError(string Message);

// Define success types
public record User(int Id, string Name, string Email);
public record CreateUserRequest(string Name, string Email);
