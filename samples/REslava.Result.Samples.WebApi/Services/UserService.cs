using Microsoft.AspNetCore.Identity.Data;
using REslava.Result;
using REslava.Result.Samples.WebApi.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace REslava.Result.Samples.WebApi.Services;

public interface IUserService
{
    Task<Result<User>> GetUserByIdAsync(int id);
    Task<Result<User>> CreateUserAsync(CreateUserRequest request);
    Task<Result<User>> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<Result> DeleteUserAsync(int id);
    Task<Result<User>> RegisterUserAsync(RegisterRequest request);
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}


public class UserService : IUserService
{
    private readonly List<User> _users = new(); // In-memory for demo
    private int _nextId = 1;

    public async Task<Result<User>> GetUserByIdAsync(int id)
    {
        await Task.Delay(10); // Simulate async DB call

        var user = _users.FirstOrDefault(u => u.Id == id);

        return Result<User>.OkIf(
            user != null,
            user!,
            new Error("User not found")
                .WithTags("ErrorType", "NotFound")
                .WithTags("StatusCode", 404)
                .WithTags("UserId", id)
        );
    }

    public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
    {
        return await Result<CreateUserRequest>.Ok(request)
            .EnsureNotNull("Request cannot be null")
            .Ensure(r => !string.IsNullOrWhiteSpace(r.Email), "Email is required")
            .Ensure(r => r.Email.Contains("@"), "Invalid email format")
            .Ensure(r => !string.IsNullOrWhiteSpace(r.Name), "Name is required")
            .Ensure(r => r.Age >= 18, "Must be 18 or older")
            .EnsureAsync(async r => !await EmailExistsAsync(r.Email), "Email already exists")
            .MapAsync(async r =>
            {
                await Task.Delay(10); // Simulate DB save
                var user = new User
                {
                    Id = _nextId++,
                    Name = r.Name,
                    Email = r.Email,
                    Age = r.Age,
                    CreatedAt = DateTime.UtcNow
                };
                _users.Add(user);
                return user;
            });
    }

    public async Task<Result<User>> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        return await GetUserByIdAsync(id)
            .EnsureAsync(user => !string.IsNullOrWhiteSpace(request.Name), "Name is required")
            .EnsureAsync(user => request.Age >= 18, "Must be 18 or older")
            .TapAsync(async user =>
            {
                await Task.Delay(10); // Simulate DB update
                user.Name = request.Name;
                user.Age = request.Age;
                user.UpdatedAt = DateTime.UtcNow;
            });
    }

    public async Task<Result> DeleteUserAsync(int id)
    {
        return await GetUserByIdAsync(id)
            .ToResult()
            .TapAsync(async () =>
            {
                await Task.Delay(10); // Simulate DB delete
                _users.RemoveAll(u => u.Id == id);
            });
    }

    public async Task<Result<User>> RegisterUserAsync(RegisterRequest request)
    {
        return await Result<string>.Ok(request.Email)
            .EnsureNotNull("Email is required")
            .Ensure(e => e.Contains("@"), "Invalid email format")
            .EnsureAsync(async e => !await EmailExistsAsync(e), "Email already registered")
            .BindAsync(async email =>
                await Result<string>.Ok(request.Password)
                    .Ensure(
                        (p => p.Length >= 8, new Error("Password must be at least 8 characters")),
                        (p => p.Any(char.IsDigit), new Error("Password must contain at least one digit")),
                        (p => p.Any(char.IsUpper), new Error("Password must contain at least one uppercase letter"))
                    )
                    .Ensure(p => request.AcceptedTerms, "Must accept terms and conditions")
                    .MapAsync(async password =>
                    {
                        await Task.Delay(10); // Simulate password hashing
                        var user = new User
                        {
                            Id = _nextId++,
                            Name = request.Name,
                            Email = email,
                            Age = request.Age,
                            PasswordHash = $"hashed_{password}",
                            CreatedAt = DateTime.UtcNow
                        };
                        _users.Add(user);
                        return user;
                    }));
    }

    private Task<bool> EmailExistsAsync(string email)
    {
        return Task.FromResult(_users.Any(u => u.Email == email));
    }
}
