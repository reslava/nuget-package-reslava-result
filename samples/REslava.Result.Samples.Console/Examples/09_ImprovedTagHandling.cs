using REslava.Result;

namespace REslava.Result.Samples.Console;

/// <summary>
/// Demonstrates improved tag handling patterns.
/// </summary>
public static class ImprovedTagHandlingSamples
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== Improved Tag Handling Samples ===\n");

        SafeTagAccess();
        ImprovedErrorCreation();
        TypeSafeTagAccess();
        TagFormatting();
        AdvancedTagPatterns();

        System.Console.WriteLine("\n=== Improved Tag Handling Complete ===");
    }

    private static void SafeTagAccess()
    {
        System.Console.WriteLine("--- Safe Tag Access ---");

        // Create error with tags
        var error = new Error("Validation failed")
            .WithTag("Field", "Email")
            .WithTag("Value", "invalid-email")
            .WithTag("Severity", "Error");

        // Safe access methods
        var field = error.GetTagString("Field", "Unknown");
        var severity = error.GetTagString("Severity", "Info");
        var missingTag = error.GetTagString("MissingTag", "Default");

        System.Console.WriteLine($"Field: {field}");
        System.Console.WriteLine($"Severity: {severity}");
        System.Console.WriteLine($"Missing tag: {missingTag}");

        // Check if tag exists
        System.Console.WriteLine($"Has 'Field' tag: {error.HasTag("Field")}");
        System.Console.WriteLine($"Has 'MissingTag' tag: {error.HasTag("MissingTag")}");

        // Get all tags
        System.Console.WriteLine($"All tags: {error.FormatTags()}");

        System.Console.WriteLine();
    }

    private static void ImprovedErrorCreation()
    {
        System.Console.WriteLine("--- Improved Error Creation ---");

        // Using improved ValidationError
        var validationError = ImprovedValidationError.Create("Email", "invalid-email", "Invalid format");
        var result1 = Result<string>.Fail(validationError);

        System.Console.WriteLine($"ValidationError:");
        System.Console.WriteLine($"  Message: {result1.Errors[0].Message}");
        System.Console.WriteLine($"  Field: {result1.Errors[0].GetTagString("Field")}");
        System.Console.WriteLine($"  Value: {result1.Errors[0].GetTagString("Value")}");
        System.Console.WriteLine($"  ErrorType: {result1.Errors[0].GetTagString("ErrorType")}");

        // Using improved NotFoundError
        var notFoundError = ImprovedNotFoundError.User("user-123");
        var result2 = Result<string>.Fail(notFoundError);

        System.Console.WriteLine($"\nNotFoundError:");
        System.Console.WriteLine($"  Message: {result2.Errors[0].Message}");
        System.Console.WriteLine($"  EntityType: {result2.Errors[0].GetTagString("EntityType")}");
        System.Console.WriteLine($"  EntityId: {result2.Errors[0].GetTagString("EntityId")}");
        System.Console.WriteLine($"  StatusCode: {result2.Errors[0].GetTagInt("StatusCode")}");

        System.Console.WriteLine();
    }

    private static void TypeSafeTagAccess()
    {
        System.Console.WriteLine("--- Type-Safe Tag Access ---");

        var error = new Error("Processing failed")
            .WithTag("RetryCount", 3)
            .WithTag("Timeout", 5000)
            .WithTag("IsRetryable", true)
            .WithTag("Component", "Database");

        // Type-safe access
        var retryCount = error.GetTagInt("RetryCount");
        var timeout = error.GetTagInt("Timeout");
        var isRetryable = error.GetTagBool("IsRetryable");
        var component = error.GetTagString("Component");

        System.Console.WriteLine($"Retry count: {retryCount}");
        System.Console.WriteLine($"Timeout: {timeout}ms");
        System.Console.WriteLine($"Is retryable: {isRetryable}");
        System.Console.WriteLine($"Component: {component}");

        // Required tag (throws if missing)
        try
        {
            var requiredComponent = error.RequireTag<string>("Component");
            System.Console.WriteLine($"Required component: {requiredComponent}");
        }
        catch (KeyNotFoundException ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
        }

        System.Console.WriteLine();
    }

    private static void TagFormatting()
    {
        System.Console.WriteLine("--- Tag Formatting ---");

        var error = new Error("Complex error")
            .WithTag("Field", "Email")
            .WithTag("Value", "test@example.com")
            .WithTag("Severity", "Warning")
            .WithTag("Timestamp", DateTime.UtcNow);

        // Different formatting options
        System.Console.WriteLine($"Default format: {error.FormatTags()}");
        System.Console.WriteLine($"Custom separator: {error.FormatTags(" | ")}");

        // Get all keys
        System.Console.WriteLine($"Tag keys: {string.Join(", ", error.GetTagKeys())}");

        // Iterate through all tags
        System.Console.WriteLine("All tags:");
        foreach (var tag in error.GetTags())
        {
            System.Console.WriteLine($"  {tag.Key}: {tag.Value}");
        }

        System.Console.WriteLine();
    }

    private static void AdvancedTagPatterns()
    {
        System.Console.WriteLine("--- Advanced Tag Patterns ---");

        // Create error with rich metadata
        var error = new Error("API call failed")
            .WithTag("Endpoint", "/api/users")
            .WithTag("Method", "GET")
            .WithTag("StatusCode", 500)
            .WithTag("ResponseTime", 1250)
            .WithTag("RetryCount", 2)
            .WithTag("IsRetryable", true)
            .WithTag("CorrelationId", Guid.NewGuid())
            .WithTag("Timestamp", DateTime.UtcNow);

        // Build error context
        var context = new
        {
            Endpoint = error.GetTagString("Endpoint"),
            Method = error.GetTagString("Method"),
            StatusCode = error.GetTagInt("StatusCode"),
            ResponseTime = error.GetTagInt("ResponseTime"),
            RetryCount = error.GetTagInt("RetryCount"),
            IsRetryable = error.GetTagBool("IsRetryable"),
            CorrelationId = error.GetTag<Guid>("CorrelationId"),
            Timestamp = error.GetTag<DateTime>("Timestamp")
        };

        System.Console.WriteLine("Error context:");
        System.Console.WriteLine($"  {context.Method} {context.Endpoint} -> {context.StatusCode}");
        System.Console.WriteLine($"  Response time: {context.ResponseTime}ms");
        System.Console.WriteLine($"  Retry count: {context.RetryCount}");
        System.Console.WriteLine($"  Is retryable: {context.IsRetryable}");
        System.Console.WriteLine($"  Correlation ID: {context.CorrelationId}");
        System.Console.WriteLine($"  Timestamp: {context.Timestamp:yyyy-MM-dd HH:mm:ss}");

        // Conditional logic based on tags
        if (error.GetTagBool("IsRetryable") && error.GetTagInt("RetryCount") < 3)
        {
            System.Console.WriteLine("Action: Retry the request");
        }
        else
        {
            System.Console.WriteLine("Action: Log and notify");
        }

        System.Console.WriteLine();
    }
}
