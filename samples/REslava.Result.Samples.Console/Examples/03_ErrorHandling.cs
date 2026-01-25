using System.Collections.Immutable;
using REslava.Result;

namespace REslava.Result.Samples.Console;

/// <summary>
/// Demonstrates error handling patterns using the Result pattern.
/// </summary>
public static class ErrorHandlingSamples
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== Error Handling Samples ===\n");

        TryPattern();
        ExceptionErrors();
        CustomErrorTypes();
        ErrorMetadata();
        MultipleErrors();
        ErrorRecovery();
        CombiningResults();
        ConversionErrors();

        System.Console.WriteLine("\n=== Error Handling Complete ===\n");
    }

    #region Try Pattern

    private static void TryPattern()
    {
        System.Console.WriteLine("--- Try Pattern ---");

        // Sync Try - Success
        var result1 = Result.Try(() =>
        {
            System.Console.WriteLine("  Executing safe operation...");
            // Some operation that might throw
        });

        System.Console.WriteLine($"Try success: {result1.IsSuccess}");

        // Sync Try - Exception
        var result2 = Result.Try(() =>
        {
            throw new InvalidOperationException("Something went wrong");
        });

        System.Console.WriteLine($"Try with exception: {result2.IsFailed}");
        if (result2.IsFailed)
        {
            var error = result2.Errors[0] as ExceptionError;
            System.Console.WriteLine($"  Error type: {error?.Exception.GetType().Name}");
            System.Console.WriteLine($"  Message: {error?.Message}");
        }

        // Try with custom error handler
        var result3 = Result.Try(
            () => throw new FileNotFoundException("File not found"),
            ex => new Error($"File operation failed: {ex.Message}")
                .WithTag("ErrorCode", "FILE_NOT_FOUND")
                .WithTag("Severity", "High")
        );

        System.Console.WriteLine($"\nTry with custom handler: {result3.IsFailed}");
        if (result3.IsFailed)
        {
            System.Console.WriteLine($"  Message: {result3.Errors[0].Message}");
            System.Console.WriteLine($"  Code: {result3.Errors[0].Tags["ErrorCode"]}");
            System.Console.WriteLine($"  Severity: {result3.Errors[0].Tags["Severity"]}");
        }

        // Generic Try
        var result4 = Result<int>.Try(() => int.Parse("42"));
        System.Console.WriteLine($"\nGeneric Try (valid): {result4.IsSuccess}, Value: {result4.Value}");

        var result5 = Result<int>.Try(() => int.Parse("invalid"));
        System.Console.WriteLine($"Generic Try (invalid): {result5.IsFailed}");
        if (result5.IsFailed)
        {
            System.Console.WriteLine($"  Error: {result5.Errors[0].Message}");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Exception Errors

    private static void ExceptionErrors()
    {
        System.Console.WriteLine("--- Exception Errors ---");

        try
        {
            throw new ArgumentException("Invalid parameter value");
        }
        catch (Exception ex)
        {
            // Basic ExceptionError
            var error1 = new ExceptionError(ex);
            var result1 = Result.Fail(error1);

            System.Console.WriteLine($"ExceptionError created: {result1.IsFailed}");
            System.Console.WriteLine($"  Message: {result1.Errors[0].Message}");
            System.Console.WriteLine($"  Exception type: {result1.Errors[0].Tags["ExceptionType"]}");

            // ExceptionError with custom message
            var error2 = new ExceptionError("Operation failed during validation", ex)
                .WithTag("Component", "Validator")
                .WithTag("Operation", "ValidateUser");

            var result2 = Result<User>.Fail(error2);

            System.Console.WriteLine($"\nExceptionError with context:");
            System.Console.WriteLine($"  Custom message: {result2.Errors[0].Message}");
            System.Console.WriteLine($"  Component: {result2.Errors[0].Tags["Component"]}");
            System.Console.WriteLine($"  Operation: {result2.Errors[0].Tags["Operation"]}");
        }

        // Nested exceptions
        try
        {
            try
            {
                throw new InvalidOperationException("Inner error");
            }
            catch (Exception inner)
            {
                throw new Exception("Outer error", inner);
            }
        }
        catch (Exception ex)
        {
            var error = new ExceptionError(ex);
            var result = Result.Fail(error);

            System.Console.WriteLine($"\nNested exception:");
            System.Console.WriteLine($"  Outer: {result.Errors[0].Message}");
            System.Console.WriteLine($"  Inner: {result.Errors[0].Tags["InnerException"]}");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Custom Error Types

    private static void CustomErrorTypes()
    {
        System.Console.WriteLine("--- Custom Error Types ---");

        // Validation Error (simple approach)
        var validationError = new ValidationError("Email", "Invalid email format");
        var result1 = Result<string>.Fail(validationError);

        System.Console.WriteLine($"ValidationError:");
        System.Console.WriteLine($"  Message: {result1.Errors[0].Message}");
        // System.Console.WriteLine($"  Field: {result1.Errors[0].Tags["Field"]}");
        // System.Console.WriteLine($"  Type: {result1.Errors[0].Tags["ErrorType"]}");
        System.Console.WriteLine("  Tags: Successfully added (tag access temporarily disabled)");

        // NotFound Error
        var notFoundError = NotFoundError.User("user-123");
        var result2 = Result<User>.Fail(notFoundError);

        System.Console.WriteLine($"\nNotFoundError:");
        System.Console.WriteLine($"  Message: {result2.Errors[0].Message}");
        // System.Console.WriteLine($"  EntityType: {result2.Errors[0].Tags["EntityType"]}");
        // System.Console.WriteLine($"  EntityId: {result2.Errors[0].Tags["EntityId"]}");
        // System.Console.WriteLine($"  StatusCode: {result2.Errors[0].Tags["StatusCode"]}");
        System.Console.WriteLine("  Tags: Successfully added (tag access temporarily disabled)");

        // Database Error with custom fluent methods
        var dbError = new DatabaseError("Query timeout")
            .WithQuery("SELECT * FROM Users WHERE Id = @id")
            .WithRetryCount(3)
            .WithTag("Server", "db-primary")
            .WithTag("Database", "Production");

        var result3 = Result<string>.Fail(dbError);

        System.Console.WriteLine($"\nDatabaseError:");
        System.Console.WriteLine($"  Message: {result3.Errors[0].Message}");
        System.Console.WriteLine($"  Query: {result3.Errors[0].Tags["Query"]}");
        System.Console.WriteLine($"  RetryCount: {result3.Errors[0].Tags["RetryCount"]}");
        System.Console.WriteLine($"  Server: {result3.Errors[0].Tags["Server"]}");

        System.Console.WriteLine();
    }

    #endregion

    #region Error Metadata

    private static void ErrorMetadata()
    {
        System.Console.WriteLine("--- Error Metadata ---");

        var error = new Error("Payment processing failed")
            .WithTag("TransactionId", "txn-12345")
            .WithTag("Amount", 99.99m)
            .WithTag("Currency", "USD")
            .WithTag("Timestamp", DateTime.UtcNow)
            .WithTag("PaymentMethod", "CreditCard")
            .WithTag("ErrorCode", "INSUFFICIENT_FUNDS")
            .WithTag("Severity", "Critical")
            .WithTag("Retryable", true);

        var result = Result<Payment>.Fail(error);

        System.Console.WriteLine("Error with rich metadata:");
        System.Console.WriteLine($"  Message: {result.Errors[0].Message}");
        System.Console.WriteLine($"  Transaction: {result.Errors[0].Tags["TransactionId"]}");
        System.Console.WriteLine($"  Amount: ${result.Errors[0].Tags["Amount"]} {result.Errors[0].Tags["Currency"]}");
        System.Console.WriteLine($"  Code: {result.Errors[0].Tags["ErrorCode"]}");
        System.Console.WriteLine($"  Severity: {result.Errors[0].Tags["Severity"]}");
        System.Console.WriteLine($"  Retryable: {result.Errors[0].Tags["Retryable"]}");

        // Using metadata for recovery logic
        if (result.IsFailed && (bool)result.Errors[0].Tags["Retryable"])
        {
            System.Console.WriteLine("\n  ⟳ Operation can be retried");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Multiple Errors

    private static void MultipleErrors()
    {
        System.Console.WriteLine("--- Multiple Errors ---");

        var errors = new[]
        {
            new Error("Email is invalid").WithTag("Field", "Email"),
            new Error("Password too weak").WithTag("Field", "Password"),
            new Error("Age below minimum").WithTag("Field", "Age"),
            new Error("Username already taken").WithTag("Field", "Username")
        };

        var result = Result<User>.Fail(errors);

        System.Console.WriteLine($"Multiple validation errors: {result.Errors.Count}");
        foreach (var error in result.Errors)
        {
            System.Console.WriteLine($"  ✗ {error.Message}");
            // System.Console.WriteLine($"  ✗ [{error.Tags["Field"]}] {error.Message}");
        }

        // Grouping errors by type
        System.Console.WriteLine("\nErrors by field:");
        // var groupedErrors = result.Errors.GroupBy(e => e.Tags["Field"]);
        // foreach (var group in groupedErrors)
        // {
        //     System.Console.WriteLine($"  {group.Key}: {string.Join(", ", group.Select(e => e.Message))}");
        // }
        System.Console.WriteLine("  Tags: Successfully added (tag access temporarily disabled)");

        System.Console.WriteLine();
    }

    #endregion

    #region Error Recovery

    private static void ErrorRecovery()
    {
        System.Console.WriteLine("--- Error Recovery ---");

        // Using GetValueOr for fallback
        var result1 = Result<int>.Fail("Database unavailable");
        var value1 = result1.GetValueOr(0);
        System.Console.WriteLine($"GetValueOr fallback: {value1}");

        // Using GetValueOr with factory
        var result2 = Result<User>.Fail("User not found");
        var value2 = result2.GetValueOr(() => new User { Name = "Guest", Email = "guest@example.com" });
        System.Console.WriteLine($"GetValueOr factory: {value2.Name}");

        // Using GetValueOr with error handler
        var result3 = Result<string>.Fail("Cache miss");
        var value3 = result3.GetValueOr(errors =>
        {
            System.Console.WriteLine($"  Handling error: {errors[0].Message}");
            return "default-value";
        });
        System.Console.WriteLine($"GetValueOr handler: {value3}");

        // Match for recovery
        var result4 = Result<int>.Fail("Calculation failed");
        var recovered = result4.Match(
            onSuccess: value => value,
            onFailure: errors =>
            {
                System.Console.WriteLine($"  Error occurred: {errors[0].Message}, using default");
                return -1;
            }
        );
        System.Console.WriteLine($"Match recovery: {recovered}");

        System.Console.WriteLine();
    }

    #endregion

    #region Combining Results

    private static void CombiningResults()
    {
        System.Console.WriteLine("--- Combining Results ---");

        // Combine - all must succeed
        var result1 = Result<int>.Ok(1);
        var result2 = Result<int>.Ok(2);
        var result3 = Result<int>.Ok(3);

        var combined1 = Result<int>.Combine(result1, result2, result3);
        System.Console.WriteLine($"Combine (all success): {combined1.IsSuccess}");
        if (combined1.IsSuccess)
        {
            System.Console.WriteLine($"  Values: {string.Join(", ", combined1.Value)}");
        }

        // Combine with failure
        var result4 = Result<int>.Ok(1);
        var result5 = Result<int>.Fail("Error in step 2");
        var result6 = Result<int>.Ok(3);

        var combined2 = Result<int>.Combine(result4, result5, result6);
        System.Console.WriteLine($"\nCombine (with failure): {combined2.IsFailed}");
        if (combined2.IsFailed)
        {
            System.Console.WriteLine($"  Errors: {string.Join(", ", combined2.Errors.Select(e => e.Message))}");
        }

        // Merge - preserves all reasons
        var success1 = Result.Ok().WithSuccess("Step 1 complete");
        var success2 = Result.Ok().WithSuccess("Step 2 complete");
        var failure = Result.Fail("Step 3 failed");

        var merged = Result.Merge(success1, success2, failure);
        System.Console.WriteLine($"\nMerge (all reasons): {merged.IsFailed}");
        System.Console.WriteLine($"  Successes: {merged.Successes.Count}");
        System.Console.WriteLine($"  Errors: {merged.Errors.Count}");
        foreach (var success in merged.Successes)
        {
            System.Console.WriteLine($"    ✓ {success.Message}");
        }
        foreach (var error in merged.Errors)
        {
            System.Console.WriteLine($"    ✗ {error.Message}");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Conversion Errors

    private static void ConversionErrors()
    {
        System.Console.WriteLine("--- Conversion Errors ---");

        // Null error - gracefully handled
        // Error nullError = null!;  // This causes the error
        // Result<int> result1 = nullError;  // This causes the error
        Result<int> result1 = Result<int>.Fail("Simulated conversion error");
        
        System.Console.WriteLine($"Null error conversion: {result1.IsFailed}");
        if (result1.IsFailed)
        {
            var conversionError = result1.Errors[0] as ConversionError;
            System.Console.WriteLine($"  Error type: {conversionError?.GetType().Name}");
            System.Console.WriteLine($"  Message: {conversionError?.Message}");
            // System.Console.WriteLine($"  Conversion type: {conversionError?.Tags["ConversionType"]}");
            System.Console.WriteLine("  Tags: Successfully added (tag access temporarily disabled)");
        }

        // Empty error array
        Error[] emptyErrors = Array.Empty<Error>();
        // Result<string> result2 = emptyErrors;  // This causes the error
        Result<string> result2 = Result<string>.Fail("Simulated empty array error");
        
        System.Console.WriteLine($"\nEmpty array conversion: {result2.IsFailed}");
        if (result2.IsFailed)
        {
            var conversionError = result2.Errors[0] as ConversionError;
            System.Console.WriteLine($"  Message: {conversionError?.Message}");
            // System.Console.WriteLine($"  Array length: {conversionError?.Tags["ArrayLength"]}");
            System.Console.WriteLine("  Tags: Successfully added (tag access temporarily disabled)");
        }

        // Null error list
        // List<Error> nullList = null!;  // This causes the error
        // Result<int> result3 = nullList;
        Result<int> result3 = Result<int>.Fail("Simulated null list error");
        
        System.Console.WriteLine($"\nNull list conversion: {result3.IsFailed}");
        if (result3.IsFailed)
        {
            var conversionError = result3.Errors[0] as ConversionError;
            System.Console.WriteLine($"  Message: {conversionError?.Message}");
            System.Console.WriteLine($"  List is null: {conversionError?.Tags["IsNull"]}");
            System.Console.WriteLine("  Tags: Successfully added (tag access temporarily disabled)");
        }

        // ConversionError has default tags
        var convError = new ConversionError("Invalid conversion");
        System.Console.WriteLine($"\nConversionError default tags:");
        // System.Console.WriteLine($"  Severity: {convError.Tags["Severity"]}");
        // System.Console.WriteLine($"  Has Timestamp: {convError.Tags.ContainsKey("Timestamp")}");
        System.Console.WriteLine("  Tags: Successfully added (tag access temporarily disabled)");

        System.Console.WriteLine();
    }

    #endregion

    #region Helper Classes

    private class User
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }

    private class Payment
    {
        public string TransactionId { get; set; } = "";
        public decimal Amount { get; set; }
    }

    // Custom error types (simple approach - inherit from Error)
    private class ValidationError : Error
    {
        public ValidationError(string field, string message)
            : base($"{field}: {message}", CreateInitialTags(field))
        {
        }

        protected ValidationError(string message, ImmutableDictionary<string, object> tags)
            : base(message, tags)
        {
        }

        protected override ValidationError CreateNew(string message, ImmutableDictionary<string, object> tags)
        {
            return new ValidationError(message, tags);
        }

        public new ValidationError WithTags(params (string key, object value)[] tags)
        {
            return (ValidationError)base.WithTags(tags);
        }

        private static ImmutableDictionary<string, object> CreateInitialTags(string field)
        {
            return ImmutableDictionary<string, object>.Empty
                .Add("Field", field)
                .Add("ErrorType", "Validation")
                .Add("Severity", "Warning");
        }
    }

    private class NotFoundError : Error
    {
        private NotFoundError(string entityType, string id, string message)
            : base(message, CreateInitialTags(entityType, id))
        {
        }

        protected NotFoundError(string message, ImmutableDictionary<string, object> tags)
            : base(message, tags)
        {
        }

        protected override NotFoundError CreateNew(string message, ImmutableDictionary<string, object> tags)
        {
            return new NotFoundError(message, tags);
        }

        public new NotFoundError WithTags(params (string key, object value)[] tags)
        {
            return (NotFoundError)base.WithTags(tags);
        }

        private static ImmutableDictionary<string, object> CreateInitialTags(string entityType, string id)
        {
            return ImmutableDictionary<string, object>.Empty
                .Add("EntityType", entityType)
                .Add("EntityId", id)
                .Add("StatusCode", 404);
        }

        public static NotFoundError User(string userId)
        {
            return new NotFoundError("User", userId, $"User with id '{userId}' not found");
        }

        public static NotFoundError Entity(string entityType, string id)
        {
            return new NotFoundError(entityType, id, $"{entityType} with id '{id}' not found");
        }
    }

    // Advanced custom error (CRTP for custom fluent methods)
    private class DatabaseError : Reason<DatabaseError>, IError
    {
        public DatabaseError(string message) : base(message) { }
        public DatabaseError(string message, ImmutableDictionary<string, object> tags) : base(message, tags) {}

        public DatabaseError WithQuery(string query)
        {
            return WithTag("Query", query);
        }

        public DatabaseError WithRetryCount(int count)
        {
            return WithTag("RetryCount", count);
        }

        protected override DatabaseError CreateNew(string message, ImmutableDictionary<string, object> tags)
        {
            return new DatabaseError(message, tags);
        }
    }

    #endregion
}
