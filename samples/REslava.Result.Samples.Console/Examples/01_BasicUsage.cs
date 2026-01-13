using REslava.Result;

namespace REslava.Result.Samples.Console.Examples;

public static class BasicUsage
{
    public static Task Run()
    {
        System.Console.WriteLine("\n1. Creating Success Results");
        DemoSuccessResults();

        System.Console.WriteLine("\n2. Creating Failed Results");
        DemoFailedResults();

        System.Console.WriteLine("\n3. Checking Result Status");
        DemoCheckingStatus();

        System.Console.WriteLine("\n4. Accessing Values Safely");
        DemoAccessingValues();

        return Task.CompletedTask;
    }

    private static void DemoSuccessResults()
    {
        // Non-generic Result
        var result1 = Result.Ok();
        System.Console.WriteLine($"Result.Ok() → IsSuccess: {result1.IsSuccess}");

        // Generic Result with value
        var result2 = Result<int>.Ok(42);
        System.Console.WriteLine($"Result<int>.Ok(42) → Value: {result2.Value}");

        // Generic Result with complex type
        var UserBasic = new UserBasic { Id = 1, Name = "John Doe", Email = "john@example.com" };
        var result3 = Result<UserBasic>.Ok(UserBasic);
        System.Console.WriteLine($"Result<UserBasic>.Ok(UserBasic) → Name: {result3.Value.Name}");
    }

    private static void DemoFailedResults()
    {
        // Simple error message
        var result1 = Result.Fail("Something went wrong");
        System.Console.WriteLine($"Result.Fail(message) → IsFailed: {result1.IsFailed}");
        System.Console.WriteLine($"  Error: {result1.Errors[0].Message}");

        // Error object with metadata
        var error = new Error("Validation failed")
            .WithTags("Field", "Email")
            .WithTags("Code", "INVALID_FORMAT");

        var result2 = Result<UserBasic>.Fail(error);
        System.Console.WriteLine($"Result.Fail(error) → Error: {result2.Errors[0].Message}");
        System.Console.WriteLine($"  Tags: Field={result2.Errors[0].Tags["Field"]}, Code={result2.Errors[0].Tags["Code"]}");

        // Multiple errors
        var result3 = Result.Fail(new[] { "Error 1", "Error 2", "Error 3" });
        System.Console.WriteLine($"Multiple errors → Count: {result3.Errors.Count}");
        foreach (var err in result3.Errors)
        {
            System.Console.WriteLine($"  - {err.Message}");
        }
    }

    private static void DemoCheckingStatus()
    {
        var success = Result<int>.Ok(100);
        var failure = Result<int>.Fail("Error");

        System.Console.WriteLine($"success.IsSuccess: {success.IsSuccess}");
        System.Console.WriteLine($"success.IsFailed: {success.IsFailed}");
        System.Console.WriteLine($"failure.IsSuccess: {failure.IsSuccess}");
        System.Console.WriteLine($"failure.IsFailed: {failure.IsFailed}");
    }

    private static void DemoAccessingValues()
    {
        var success = Result<int>.Ok(42);
        var failure = Result<int>.Fail("Error");

        // Safe access with Value (throws if failed)
        System.Console.WriteLine($"success.Value: {success.Value}");

        // Safe access with ValueOrDefault (returns default if failed)
        System.Console.WriteLine($"success.ValueOrDefault: {success.ValueOrDefault}");
        System.Console.WriteLine($"failure.ValueOrDefault: {failure.ValueOrDefault}");

        // Conditional access
        if (success.IsSuccess)
        {
            System.Console.WriteLine($"Conditional access: {success.Value}");
        }
    }
}

public class UserBasic
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}
