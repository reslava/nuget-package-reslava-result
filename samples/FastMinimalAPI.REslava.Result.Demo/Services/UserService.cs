using Microsoft.EntityFrameworkCore;
using REslava.Result;
using REslava.Result.AdvancedPatterns;
using FastMinimalAPI.REslava.Result.Demo.Data;
using FastMinimalAPI.REslava.Result.Demo.Models;
using FastMinimalAPI.REslava.Result.Demo.Errors;
using System.Text.RegularExpressions;

namespace FastMinimalAPI.REslava.Result.Demo.Services;

/// <summary>
/// User service demonstrating Result pattern with comprehensive error handling
/// </summary>
public class UserService
{
    private readonly DemoDbContext _context;
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public UserService(DemoDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all users - demonstrates simple Result pattern
    /// </summary>
    public async Task<Result<List<UserResponse>>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Include(u => u.Orders)
            .ToListAsync();

        var response = users.Select(u => new UserResponse(
            u.Id, u.Email, u.Name, u.Role, u.IsActive, u.CreatedAt, u.Orders.Count
        )).ToList();

        return Result<List<UserResponse>>.Ok(response);
    }

    /// <summary>
    /// Get user by ID - demonstrates OneOf pattern with NotFoundError
    /// Returns: Success(UserResponse) OR NotFoundError
    /// </summary>
    public async Task<OneOf<UserNotFoundError, UserResponse>> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.Orders)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return new UserNotFoundError(id);

        var response = new UserResponse(
            user.Id, user.Email, user.Name, user.Role, 
            user.IsActive, user.CreatedAt, user.Orders.Count
        );

        return response;
    }

    /// <summary>
    /// Create user - demonstrates OneOf with multiple error types
    /// Returns: Success(UserResponse) OR ValidationError OR DuplicateEmailError
    /// </summary>
    public async Task<OneOf<ValidationError, DuplicateEmailError, UserResponse>> CreateUserAsync(CreateUserRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Email))
            return new RequiredFieldError("Email");

        if (!EmailRegex.IsMatch(request.Email))
            return new InvalidEmailError(request.Email);

        if (string.IsNullOrWhiteSpace(request.Name))
            return new RequiredFieldError("Name");

        // Check duplicate email
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
            return new DuplicateEmailError(request.Email);

        // Create user
        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            Role = request.Role ?? "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var response = new UserResponse(
            user.Id, user.Email, user.Name, user.Role, 
            user.IsActive, user.CreatedAt, 0
        );

        return response;
    }

    /// <summary>
    /// Update user - demonstrates OneOf4 with 4 possible outcomes
    /// Returns: ValidationError OR UserNotFoundError OR DuplicateEmailError OR UserResponse
    /// </summary>
    public async Task<OneOf<ValidationError, UserNotFoundError, DuplicateEmailError, UserResponse>> UpdateUserAsync(
        int id, UpdateUserRequest request)
    {
        // Find user
        var user = await _context.Users
            .Include(u => u.Orders)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return new UserNotFoundError(id);

        // Validate email if provided
        if (request.Email != null)
        {
            if (!EmailRegex.IsMatch(request.Email))
                return new InvalidEmailError(request.Email);

            // Check duplicate email (excluding current user)
            var duplicateEmail = await _context.Users
                .AnyAsync(u => u.Email == request.Email && u.Id != id);

            if (duplicateEmail)
                return new DuplicateEmailError(request.Email);

            user.Email = request.Email;
        }

        // Update fields
        if (request.Name != null)
            user.Name = request.Name;

        if (request.Role != null)
            user.Role = request.Role;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync();

        var response = new UserResponse(
            user.Id, user.Email, user.Name, user.Role, 
            user.IsActive, user.CreatedAt, user.Orders.Count
        );

        return response;
    }

    /// <summary>
    /// Delete user - demonstrates Result with conditional success
    /// </summary>
    public async Task<Result<bool>> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return Result<bool>.Fail(new UserNotFoundError(id));

        // Check if user has orders
        var hasOrders = await _context.Orders.AnyAsync(o => o.UserId == id);
        if (hasOrders)
        {
            return Result<bool>.Fail(new Error("Cannot delete user with existing orders")
                .WithTag("UserId", id)
                .WithTag("StatusCode", 409));
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }
}
