using System;
using REslava.Result;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Samples.Console.Examples;

/// <summary>
/// Demonstrates conversions between Result&lt;T&gt; and OneOf&lt;T1, T2&gt; patterns.
/// Shows how to migrate from Result to OneOf and integrate both patterns.
/// </summary>
public static class Result_OneOf_Conversions
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== Result ↔ OneOf Conversions ===\n");

        // 1. Result → OneOf Migration
        ResultToOneOfMigration();

        // 2. OneOf → Result Integration
        OneOfToResultIntegration();

        // 3. Real-world scenario
        RealWorldScenario();

        System.Console.WriteLine("\n=== Result ↔ OneOf Conversions Complete ===");
        await Task.CompletedTask;
    }

    private static void ResultToOneOfMigration()
    {
        System.Console.WriteLine("1. Result → OneOf Migration:");
        System.Console.WriteLine("---------------------------");

        // Legacy Result code
        Result<User> legacyResult = GetUserFromLegacySystem(1);

        // Convert to modern OneOf pattern
        OneOf<ApiError, User> modernResult = legacyResult.ToOneOf(reason => new ApiError(reason.Message, 500));

        System.Console.WriteLine($"Legacy Result: {(legacyResult.IsSuccess ? "Success" : "Failed")}");
        System.Console.WriteLine($"Modern OneOf: {modernResult}");
        System.Console.WriteLine();
    }

    private static void OneOfToResultIntegration()
    {
        System.Console.WriteLine("2. OneOf → Result Integration:");
        System.Console.WriteLine("----------------------------");

        // New OneOf validation
        OneOf<ValidationError, User> validationResult = ValidateUser(new User("alice@example.com"));

        // Convert to Result for existing infrastructure
        Result<User> resultForDatabase = validationResult.ToResult(error => new Error($"{error.Field}: {error.Message}"));

        System.Console.WriteLine($"Validation Result: {validationResult}");
        System.Console.WriteLine($"Database Result: {(resultForDatabase.IsSuccess ? "Success" : "Failed")}");
        
        if (resultForDatabase.IsSuccess)
        {
            System.Console.WriteLine($"User ready for database: {resultForDatabase.Value.Email}");
        }
        else
        {
            System.Console.WriteLine($"Database error: {resultForDatabase.Errors.First().Message}");
        }
        System.Console.WriteLine();
    }

    private static void RealWorldScenario()
    {
        System.Console.WriteLine("3. Real-world Scenario:");
        System.Console.WriteLine("----------------------");

        // API layer uses OneOf
        OneOf<ApiError, User> apiResult = GetUserFromApi(1);

        // Convert to Result for business logic
        Result<User> businessResult = apiResult.ToResult(error => new Error($"API Error: {error.Message} (Code: {error.Code})"));

        // Business logic uses Result
        Result<User> processedResult = ProcessUser(businessResult);

        // Convert back to OneOf for response
        OneOf<BusinessError, User> responseResult = processedResult.ToOneOf(reason => new BusinessError(reason.Message));

        System.Console.WriteLine($"API Result: {apiResult}");
        System.Console.WriteLine($"Business Result: {(processedResult.IsSuccess ? "Success" : "Failed")}");
        System.Console.WriteLine($"Response Result: {responseResult}");
        System.Console.WriteLine();
    }

    // Helper methods

    private static Result<User> GetUserFromLegacySystem(int id)
    {
        return id switch
        {
            1 => Result<User>.Ok(new User("alice@example.com")),
            2 => Result<User>.Ok(new User("bob@example.com")),
            _ => Result<User>.Fail($"User with ID {id} not found")
        };
    }

    private static OneOf<ValidationError, User> ValidateUser(User user)
    {
        if (string.IsNullOrEmpty(user.Email))
            return new ValidationError("Email is required", "Email");

        if (!user.Email.Contains("@"))
            return new ValidationError("Invalid email format", "Email");

        return user;
    }

    private static OneOf<ApiError, User> GetUserFromApi(int id)
    {
        return id switch
        {
            1 => new User("alice@example.com"),
            _ => new ApiError($"User {id} not found", 404)
        };
    }

    private static Result<User> ProcessUser(Result<User> userResult)
    {
        if (userResult.IsFailed)
            return userResult;

        var user = userResult.Value;
        
        // Simulate processing
        if (user.Email.StartsWith("test"))
            return Result<User>.Fail("Test users cannot be processed");

        return Result<User>.Ok(user with { Email = user.Email.ToUpper() });
    }

    // Supporting classes

    public record User(string Email);
    public record ApiError(string Message, int Code);
    public record ValidationError(string Message, string Field);
    public record BusinessError(string Message);
}
