using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using REslava.Result;
using REslava.Result.Extensions;

namespace REslava.Result.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class ResultBenchmarks
{
    private const int Iterations = 1000;
    private readonly User _testUser = new("test@example.com", 25);

    [Benchmark(Baseline = true)]
    public int ExceptionChaining()
    {
        var total = 0;
        for (int i = 0; i < Iterations; i++)
        {
            try
            {
                var result = ProcessWithExceptions(_testUser);
                total += result.Age;
            }
            catch
            {
                // Handle exception
            }
        }
        return total;
    }

    [Benchmark]
    public int REslavaResultChaining()
    {
        var total = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var result = ProcessWithREslavaResult(_testUser);
            if (result.IsSuccess)
                total += result.Value.Age;
        }
        return total;
    }

    // Exception-based approach
    private User ProcessWithExceptions(User user)
    {
        if (user.Age < 18)
            throw new ValidationException("Must be 18+");
        
        if (!IsValidEmail(user.Email))
            throw new ValidationException("Invalid email");

        var processedEmail = ProcessEmail(user.Email);
        var validatedUser = ValidateUser(user);
        return TransformUser(validatedUser, processedEmail);
    }

    // REslava.Result approach
    private Result<User> ProcessWithREslavaResult(User user)
    {
        return Result<User>.Ok(user)
            .Ensure(u => u.Age >= 18, "Must be 18+")
            .Ensure(u => IsValidEmail(u.Email), "Invalid email")
            .Map(u => new { User = u, ProcessedEmail = ProcessEmail(u.Email) })
            .Bind(data => Result<User>.Ok(ValidateUser(data.User))
                .Map(validated => TransformUser(validated, data.ProcessedEmail)));
    }

    // Helper methods
    private static bool IsValidEmail(string email) => email.Contains("@");
    
    private static string ProcessEmail(string email) => email.ToLowerInvariant();
    
    private static User ValidateUser(User user) => user with { Email = user.Email.ToLowerInvariant() };
    
    private static User TransformUser(User user, string processedEmail) => 
        user with { Email = processedEmail, Age = user.Age + 1 };
}

// Supporting types
public record User(string Email, int Age);
public class ValidationException(string message) : Exception(message);

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<ResultBenchmarks>();
        Console.WriteLine(summary);
    }
}
