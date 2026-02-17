using Microsoft.EntityFrameworkCore;
using REslava.Result;
using REslava.Result.AdvancedPatterns;
using FastMvcAPI.REslava.Result.Demo.Data;
using FastMvcAPI.REslava.Result.Demo.Models;
using System.Text.RegularExpressions;

namespace FastMvcAPI.REslava.Result.Demo.Services;

public class UserService
{
    private readonly DemoDbContext _context;
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public UserService(DemoDbContext context) => _context = context;

    public async Task<Result<List<UserResponse>>> GetAllUsersAsync()
    {
        var users = await _context.Users.Include(u => u.Orders).ToListAsync();
        var response = users.Select(u => new UserResponse(
            u.Id, u.Email, u.Name, u.Role, u.IsActive, u.CreatedAt, u.Orders.Count
        )).ToList();
        return Result<List<UserResponse>>.Ok(response);
    }

    public async Task<OneOf<NotFoundError, UserResponse>> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.Include(u => u.Orders).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return new NotFoundError("User", id);

        return new UserResponse(user.Id, user.Email, user.Name, user.Role, user.IsActive, user.CreatedAt, user.Orders.Count);
    }

    public async Task<OneOf<ValidationError, ConflictError, UserResponse>> CreateUserAsync(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return new ValidationError("Email", "This field is required");
        if (!EmailRegex.IsMatch(request.Email))
            return new ValidationError("Email", "Invalid email format");
        if (string.IsNullOrWhiteSpace(request.Name))
            return new ValidationError("Name", "This field is required");

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
            return new ConflictError("User", "email", request.Email);

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

        return new UserResponse(user.Id, user.Email, user.Name, user.Role, user.IsActive, user.CreatedAt, 0);
    }

    public async Task<OneOf<ValidationError, NotFoundError, ConflictError, UserResponse>> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _context.Users.Include(u => u.Orders).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return new NotFoundError("User", id);

        if (request.Email != null)
        {
            if (!EmailRegex.IsMatch(request.Email))
                return new ValidationError("Email", "Invalid email format");
            if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
                return new ConflictError("User", "email", request.Email);
            user.Email = request.Email;
        }

        if (request.Name != null) user.Name = request.Name;
        if (request.Role != null) user.Role = request.Role;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync();

        return new UserResponse(user.Id, user.Email, user.Name, user.Role, user.IsActive, user.CreatedAt, user.Orders.Count);
    }

    public async Task<Result<bool>> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return Result<bool>.Fail(new NotFoundError("User", id));

        if (await _context.Orders.AnyAsync(o => o.UserId == id))
            return Result<bool>.Fail(new ConflictError("User", "orders", id));

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Result<bool>.Ok(true);
    }
}
