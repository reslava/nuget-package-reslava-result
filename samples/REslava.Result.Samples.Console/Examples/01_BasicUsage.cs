using REslava.Result;

namespace REslava.Result.Samples.Console;

/// <summary>
/// Demonstrates basic usage of the Result pattern.
/// </summary>
public static class BasicUsageSamples
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== Basic Usage Samples ===\n");

        SuccessResults();
        FailureResults();
        ImplicitConversions();
        PatternMatching();
        WorkingWithValues();
        SafeValueAccess();
        SuccessReasons();
        ErrorMetadata();

        System.Console.WriteLine("\n=== Basic Usage Complete ===\n");
    }

    #region Success Results

    private static void SuccessResults()
    {
        System.Console.WriteLine("--- Success Results ---");

        // Simple success
        var result1 = Result.Ok();
        System.Console.WriteLine($"Simple success: {result1.IsSuccess}");

        // Success with message
        var result2 = Result.Ok("Operation completed successfully");
        System.Console.WriteLine($"Success with message: {result2.Successes[0].Message}");

        // Success with value
        var result3 = Result<int>.Ok(42);
        System.Console.WriteLine($"Success with value: {result3.Value}");

        // Success with value and message
        var result4 = Result<string>.Ok("Hello World", "Data retrieved");
        System.Console.WriteLine($"Value: {result4.Value}, Message: {result4.Successes[0].Message}");

        System.Console.WriteLine();
    }

    #endregion

    #region Failure Results

    private static void FailureResults()
    {
        System.Console.WriteLine("--- Failure Results ---");

        // Simple failure
        var result1 = Result.Fail("Something went wrong");
        System.Console.WriteLine($"Is failed: {result1.IsFailed}");
        System.Console.WriteLine($"Error: {result1.Errors[0].Message}");

        // Failure with custom error
        var error = new Error("Validation failed")
            .WithTag("Field", "Email")
            .WithTag("Code", 422);

        var result2 = Result<string>.Fail(error);
        System.Console.WriteLine($"Error with tags: {result2.Errors[0].Message}");
        System.Console.WriteLine($"Field: {result2.Errors[0].Tags["Field"]}");
        System.Console.WriteLine($"Code: {result2.Errors[0].Tags["Code"]}");

        // Multiple errors
        var errors = new[]
        {
            new Error("Email is required"),
            new Error("Password is too short"),
            new Error("Username is taken")
        };

        var result3 = Result<User>.Fail(errors);
        System.Console.WriteLine($"Multiple errors count: {result3.Errors.Count}");
        foreach (var err in result3.Errors)
        {
            System.Console.WriteLine($"  - {err.Message}");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Implicit Conversions

    private static void ImplicitConversions()
    {
        System.Console.WriteLine("--- Implicit Conversions ---");

        // Value to Result
        Result<int> result1 = 42;
        System.Console.WriteLine($"From value: {result1.Value}");

        // Error to Result
        Result<string> result2 = new Error("Not found");
        System.Console.WriteLine($"From error: {result2.Errors[0].Message}");

        // Error array to Result
        Result<User> result3 = new[]
        {
            new Error("Email invalid"),
            new Error("Age below minimum")
        };
        System.Console.WriteLine($"From error array: {result3.Errors.Count} errors");

        System.Console.WriteLine();
    }

    #endregion

    #region Pattern Matching

    private static void PatternMatching()
    {
        System.Console.WriteLine("--- Pattern Matching ---");

        var successResult = Result<int>.Ok(100);
        var failureResult = Result<int>.Fail("Error occurred");

        // Match with return value
        var message1 = successResult.Match(
            onSuccess: value => $"Success: {value}",
            onFailure: errors => $"Failed: {errors[0].Message}"
        );
        System.Console.WriteLine(message1);

        var message2 = failureResult.Match(
            onSuccess: value => $"Success: {value}",
            onFailure: errors => $"Failed: {errors[0].Message}"
        );
        System.Console.WriteLine(message2);

        // Match with actions (no return value)
        successResult.Match(
            onSuccess: value => System.Console.WriteLine($"Processing value: {value}"),
            onFailure: errors => System.Console.WriteLine($"Error: {errors[0].Message}")
        );

        failureResult.Match(
            onSuccess: value => System.Console.WriteLine($"Processing value: {value}"),
            onFailure: errors => System.Console.WriteLine($"Error: {errors[0].Message}")
        );

        System.Console.WriteLine();
    }

    #endregion

    #region Working with Values

    private static void WorkingWithValues()
    {
        System.Console.WriteLine("--- Working with Values ---");

        var result = Result<int>.Ok(50);

        // Safe access - check first
        if (result.IsSuccess)
        {
            System.Console.WriteLine($"Value: {result.Value}");
        }

        // Try pattern
        if (result.TryGetValue(out var value))
        {
            System.Console.WriteLine($"TryGetValue: {value}");
        }

        // Access will throw on failed result
        var failedResult = Result<string>.Fail("Error");
        try
        {
            var _ = failedResult.Value; // This will throw
        }
        catch (InvalidOperationException ex)
        {
            System.Console.WriteLine($"Expected exception: {ex.Message.Substring(0, 50)}...");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Safe Value Access

    private static void SafeValueAccess()
    {
        System.Console.WriteLine("--- Safe Value Access ---");

        var successResult = Result<int>.Ok(42);
        var failedResult = Result<int>.Fail("Error");

        // GetValueOr with default
        var value1 = successResult.GetValueOr(0);
        var value2 = failedResult.GetValueOr(0);
        System.Console.WriteLine($"GetValueOr: {value1}, {value2}");

        // GetValueOr with factory
        var value3 = failedResult.GetValueOr(() =>
        {
            System.Console.WriteLine("  Computing default value...");
            return 99;
        });
        System.Console.WriteLine($"GetValueOr(factory): {value3}");

        // GetValueOr with error handler
        var value4 = failedResult.GetValueOr(errors =>
        {
            System.Console.WriteLine($"  Error: {errors[0].Message}");
            return -1;
        });
        System.Console.WriteLine($"GetValueOr(errorHandler): {value4}");

        System.Console.WriteLine();
    }

    #endregion

    #region Success Reasons

    private static void SuccessReasons()
    {
        System.Console.WriteLine("--- Success Reasons ---");

        var result = Result<string>.Ok("data", "Retrieved from cache")
            .WithSuccess("Validation passed")
            .WithSuccess("Security check completed");

        System.Console.WriteLine($"Success count: {result.Successes.Count}");
        foreach (var successL in result.Successes)
        {
            System.Console.WriteLine($"  ✓ {successL.Message}");
        }

        // Success with metadata
        var success = new Success("User created")
            .WithTag("UserId", "user-123")
            .WithTag("Timestamp", DateTime.UtcNow)
            .WithTag("Source", "API");

        var userResult = Result<User>.Ok(new User { Id = 123, Name = "Alice" })
            .WithSuccess(success);

        System.Console.WriteLine($"\nSuccess with metadata:");
        System.Console.WriteLine($"  Message: {userResult.Successes[0].Message}");
        System.Console.WriteLine($"  UserId: {userResult.Successes[0].Tags["UserId"]}");
        System.Console.WriteLine($"  Source: {userResult.Successes[0].Tags["Source"]}");

        System.Console.WriteLine();
    }

    #endregion

    #region Error Metadata

    private static void ErrorMetadata()
    {
        System.Console.WriteLine("--- Error Metadata ---");

        // Error with tags
        var error = new Error("Database connection failed")
            .WithTag("Server", "localhost")
            .WithTag("Port", 5432)
            .WithTag("Database", "ProductionDB")
            .WithTag("RetryCount", 3)
            .WithTag("Timestamp", DateTime.UtcNow);

        var result = Result<string>.Fail(error);

        System.Console.WriteLine("Error details:");
        System.Console.WriteLine($"  Message: {result.Errors[0].Message}");
        System.Console.WriteLine($"  Server: {result.Errors[0].Tags["Server"]}");
        System.Console.WriteLine($"  Port: {result.Errors[0].Tags["Port"]}");
        System.Console.WriteLine($"  RetryCount: {result.Errors[0].Tags["RetryCount"]}");

        // ExceptionError
        try
        {
            throw new InvalidOperationException("Something broke");
        }
        catch (Exception ex)
        {
            var exceptionError = new ExceptionError(ex)
                .WithTag("Component", "DataAccess")
                .WithTag("Operation", "SaveUser");

            var exResult = Result.Fail(exceptionError);

            System.Console.WriteLine($"\nException error:");
            System.Console.WriteLine($"  Message: {exResult.Errors[0].Message}");
            System.Console.WriteLine($"  Type: {exResult.Errors[0].Tags["ExceptionType"]}");
            System.Console.WriteLine($"  Component: {exResult.Errors[0].Tags["Component"]}");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Helper Classes

    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public int Age { get; set; }
    }

    #endregion
}
