using System;
using REslava.Result;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Samples.Console.Examples;

/// <summary>
/// Demonstrates integration between OneOf and Result patterns.
/// Shows how both patterns can work together seamlessly in mixed workflows.
/// </summary>
public static class OneOf_Result_Integration
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== OneOf ↔ Result Integration ===\n");

        // 1. OneOf → Result transformations
        OneOfToResultTransformations();

        // 2. Result → OneOf transformations
        ResultToOneOfTransformations();

        // 3. Mixed pipeline scenarios
        MixedPipelineScenarios();

        System.Console.WriteLine("\n=== OneOf ↔ Result Integration Complete ===");
        await Task.CompletedTask;
    }

    private static void OneOfToResultTransformations()
    {
        System.Console.WriteLine("1. OneOf → Result Transformations:");
        System.Console.WriteLine("-----------------------------------");

        // Example 1: Custom error mapping
        OneOf<string, User> userOneOf = OneOf<string, User>.FromT2(new User("Alice"));
        Result<UserDto> result1 = userOneOf.SelectToResult(
            user => new UserDto(user.Name),
            error => new Error($"User error: {error}")
        );
        System.Console.WriteLine($"Custom mapping: {(result1.IsSuccess ? "Success" : "Failed")} - {result1.Value?.Name}");

        // Example 2: IError direct usage
        OneOf<ApiError, User> apiOneOf = OneOf<ApiError, User>.FromT2(new User("Bob"));
        Result<UserDto> result2 = apiOneOf.SelectToResult(user => new UserDto(user.Name));
        System.Console.WriteLine($"IError direct: {(result2.IsSuccess ? "Success" : "Failed")} - {result2.Value?.Name}");

        // Example 3: Bind operations
        OneOf<string, User> bindOneOf = OneOf<string, User>.FromT2(new User("Charlie"));
        Result<User> result3 = bindOneOf.BindToResult(
            user => ValidateUser(user),
            error => new Error($"Bind error: {error}")
        );
        System.Console.WriteLine($"Bind operation: {(result3.IsSuccess ? "Success" : "Failed")}");

        System.Console.WriteLine();
    }

    private static void ResultToOneOfTransformations()
    {
        System.Console.WriteLine("2. Result → OneOf Transformations:");
        System.Console.WriteLine("-----------------------------------");

        // Example 1: Success case
        Result<User> successResult = Result<User>.Ok(new User("David"));
        OneOf<ValidationError, User> oneOf1 = successResult.ToOneOfCustom(reason => new ValidationError(reason.Message));
        System.Console.WriteLine($"Success conversion: {oneOf1}");

        // Example 2: Error case
        Result<User> errorResult = Result<User>.Fail("User not found");
        OneOf<ValidationError, User> oneOf2 = errorResult.ToOneOfCustom(reason => new ValidationError(reason.Message));
        System.Console.WriteLine($"Error conversion: {oneOf2}");

        System.Console.WriteLine();
    }

    private static void MixedPipelineScenarios()
    {
        System.Console.WriteLine("3. Mixed Pipeline Scenarios:");
        System.Console.WriteLine("--------------------------");

        // Scenario 1: API → Business → Database
        OneOf<ApiError, User> apiResult = GetUserFromApi(1);
        
        // Transform to Result for business logic
        Result<UserDto> businessResult = apiResult.SelectToResult(user => new UserDto(user.Name));
        
        // Transform back to OneOf for database layer
        OneOf<DbError, UserDto> dbResult = businessResult.ToOneOfCustom(reason => new DbError(reason.Message));
        
        System.Console.WriteLine($"Mixed pipeline: {dbResult}");

        // Scenario 2: Filtering
        OneOf<Error, User> filterOneOf = OneOf<Error, User>.FromT2(new User("Eve", true));
        try
        {
            OneOf<Error, User> filtered = filterOneOf.Filter(user => user.IsActive);
            System.Console.WriteLine($"Filtered result: {filtered}");
        }
        catch (InvalidOperationException ex)
        {
            System.Console.WriteLine($"Filter failed: {ex.Message}");
        }

        System.Console.WriteLine();
    }

    // Helper methods

    private static OneOf<ApiError, User> GetUserFromApi(int id)
    {
        return id switch
        {
            1 => new User("Alice"),
            2 => new User("Bob"),
            _ => new ApiError($"User {id} not found", 404)
        };
    }

    private static Result<User> ValidateUser(User user)
    {
        return string.IsNullOrEmpty(user.Name) 
            ? Result<User>.Fail("User name is required")
            : Result<User>.Ok(user);
    }

    // Supporting classes

    public record User(string Name, bool IsActive = true);
    public record UserDto(string Name);
    public record ApiError(string Message, int Code) : IError
    {
        public string Message { get; init; } = Message;
        public System.Collections.Immutable.ImmutableDictionary<string, object> Tags { get; init; } = 
            System.Collections.Immutable.ImmutableDictionary<string, object>.Empty;
    }
    public record ValidationError(string Message);
    public record DbError(string Message);
}
