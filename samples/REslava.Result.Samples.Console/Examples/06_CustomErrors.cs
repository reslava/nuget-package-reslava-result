using REslava.Result;
using System.Collections.Immutable;

namespace REslava.Result.Samples.Console;

/// <summary>
/// Demonstrates creating and using custom error types.
/// </summary>
public static class CustomErrorsSamples
{
    public static void Run()
    {
        System.Console.WriteLine("=== Custom Errors Samples ===\n");

        SimpleCustomErrors();
        ValidationErrors();
        DomainErrors();
        AdvancedCustomErrors();
        ErrorHierarchy();
        ErrorWithMetadata();
        TypeSpecificHandling();
        CustomErrorInPipeline();

        System.Console.WriteLine("\n=== Custom Errors Complete ===\n");
    }

    #region Simple Custom Errors

    private static void SimpleCustomErrors()
    {
        System.Console.WriteLine("--- Simple Custom Errors ---");

        // Simple inheritance from Error
        var notFoundError = new NotFoundError("User", "user-123");
        var result1 = Result<User>.Fail(notFoundError);

        System.Console.WriteLine($"NotFoundError: {result1.IsFailed}");
        System.Console.WriteLine($"  Message: {result1.Errors[0].Message}");
        System.Console.WriteLine($"  Entity: {result1.Errors[0].Tags["EntityType"]}");
        System.Console.WriteLine($"  ID: {result1.Errors[0].Tags["EntityId"]}");
        System.Console.WriteLine($"  Status: {result1.Errors[0].Tags["StatusCode"]}");

        // ValidationError
        var validationError = new ValidationError("Email", "Invalid format");
        var result2 = Result<string>.Fail(validationError);

        System.Console.WriteLine($"\nValidationError: {result2.IsFailed}");
        System.Console.WriteLine($"  Message: {result2.Errors[0].Message}");
        System.Console.WriteLine($"  Field: {result2.Errors[0].Tags["Field"]}");
        System.Console.WriteLine($"  Type: {result2.Errors[0].Tags["ErrorType"]}");

        // AuthorizationError
        var authError = new AuthorizationError("user-456", "admin:write");
        var result3 = Result.Fail(authError);

        System.Console.WriteLine($"\nAuthorizationError: {result3.IsFailed}");
        System.Console.WriteLine($"  Message: {result3.Errors[0].Message}");
        System.Console.WriteLine($"  User: {result3.Errors[0].Tags["UserId"]}");
        System.Console.WriteLine($"  Permission: {result3.Errors[0].Tags["RequiredPermission"]}");

        System.Console.WriteLine();
    }

    #endregion

    #region Validation Errors

    private static void ValidationErrors()
    {
        System.Console.WriteLine("--- Validation Errors ---");

        // Multiple field validation errors
        var errors = new[]
        {
            new ValidationError("Email", "Required field"),
            new ValidationError("Password", "Too weak"),
            new ValidationError("Age", "Below minimum")
        };

        var result = Result<UserRegistration>.Fail(errors);

        System.Console.WriteLine($"Validation failed: {result.Errors.Count} errors");
        foreach (var error in result.Errors)
        {
            System.Console.WriteLine($"  ✗ {error.Tags["Field"]}: {error.Message}");
        }

        // Using validation error with additional context
        var emailError = new ValidationError("Email", "Invalid format")
            .WithTag("Pattern", @"^[^@]+@[^@]+\.[^@]+$")
            .WithTag("ProvidedValue", "invalid-email")
            .WithTag("Suggestion", "Use format: user@domain.com");

        var result2 = Result<string>.Fail(emailError);

        System.Console.WriteLine($"\nDetailed validation error:");
        System.Console.WriteLine($"  Message: {result2.Errors[0].Message}");
        System.Console.WriteLine($"  Pattern: {result2.Errors[0].Tags["Pattern"]}");
        System.Console.WriteLine($"  Suggestion: {result2.Errors[0].Tags["Suggestion"]}");

        System.Console.WriteLine();
    }

    #endregion

    #region Domain Errors

    private static void DomainErrors()
    {
        System.Console.WriteLine("--- Domain Errors ---");

        // Business rule violations
        var insufficientFunds = new BusinessRuleError(
            "InsufficientFunds",
            "Account balance too low for this transaction"
        ).WithTag("AccountId", "ACC-123")
         .WithTag("RequiredAmount", 1000m)
         .WithTag("AvailableBalance", 500m);

        var result1 = Result<Transaction>.Fail(insufficientFunds);

        System.Console.WriteLine($"Business rule error:");
        System.Console.WriteLine($"  Rule: {result1.Errors[0].Tags["RuleCode"]}");
        System.Console.WriteLine($"  Message: {result1.Errors[0].Message}");
        System.Console.WriteLine($"  Required: ${result1.Errors[0].Tags["RequiredAmount"]}");
        System.Console.WriteLine($"  Available: ${result1.Errors[0].Tags["AvailableBalance"]}");

        // Domain-specific errors
        var inventoryError = new InventoryError(
            "OUT_OF_STOCK",
            "Product not available"
        ).WithProductId("PROD-789")
         .WithRequestedQuantity(10)
         .WithAvailableQuantity(3);

        var result2 = Result<Order>.Fail(inventoryError);

        System.Console.WriteLine($"\nInventory error:");
        System.Console.WriteLine($"  Code: {result2.Errors[0].Tags["ErrorCode"]}");
        System.Console.WriteLine($"  Product: {result2.Errors[0].Tags["ProductId"]}");
        System.Console.WriteLine($"  Requested: {result2.Errors[0].Tags["RequestedQuantity"]}");
        System.Console.WriteLine($"  Available: {result2.Errors[0].Tags["AvailableQuantity"]}");

        System.Console.WriteLine();
    }

    #endregion

    #region Advanced Custom Errors (CRTP)

    private static void AdvancedCustomErrors()
    {
        System.Console.WriteLine("--- Advanced Custom Errors (CRTP) ---");

        // DatabaseError with custom fluent methods
        var dbError = new DatabaseError("Connection timeout")
            .WithQuery("SELECT * FROM Users WHERE Id = @id")
            .WithRetryCount(3)
            .WithConnectionString("Server=localhost;Database=MyDB")
            .WithTag("Timeout", 30)
            .WithTag("LastAttempt", DateTime.UtcNow);

        var result1 = Result<User>.Fail(dbError);

        System.Console.WriteLine($"DatabaseError with custom methods:");
        System.Console.WriteLine($"  Message: {result1.Errors[0].Message}");
        System.Console.WriteLine($"  Query: {result1.Errors[0].Tags["Query"]}");
        System.Console.WriteLine($"  Retries: {result1.Errors[0].Tags["RetryCount"]}");
        System.Console.WriteLine($"  Connection: {result1.Errors[0].Tags["ConnectionString"]}");

        // ApiError with custom fluent methods
        var apiError = new ApiError("Rate limit exceeded")
            .WithEndpoint("/api/users")
            .WithHttpMethod("GET")
            .WithStatusCode(429)
            .WithRetryAfter(60);

        var result2 = Result<ApiResponse>.Fail(apiError);

        System.Console.WriteLine($"\nApiError with custom methods:");
        System.Console.WriteLine($"  Endpoint: {result2.Errors[0].Tags["Endpoint"]}");
        System.Console.WriteLine($"  Method: {result2.Errors[0].Tags["HttpMethod"]}");
        System.Console.WriteLine($"  Status: {result2.Errors[0].Tags["StatusCode"]}");
        System.Console.WriteLine($"  Retry after: {result2.Errors[0].Tags["RetryAfter"]}s");

        System.Console.WriteLine();
    }

    #endregion

    #region Error Hierarchy

    private static void ErrorHierarchy()
    {
        System.Console.WriteLine("--- Error Hierarchy ---");

        // Base payment error
        var paymentError = new PaymentError("Payment processing failed")
            .WithTransactionId("TXN-12345")
            .WithAmount(99.99m)
            .WithCurrency("USD");

        // Specific payment errors
        var cardDeclined = new CardDeclinedError("Insufficient funds")
            .WithCardLastFour("1234")
            .WithCardType("Visa")
            .WithTag("DeclineCode", "51");

        var fraudDetected = new FraudDetectedError("Suspicious activity")
            .WithRiskScore(0.95)
            .WithFraudReason("Unusual location");

        System.Console.WriteLine("Payment error hierarchy:");
        System.Console.WriteLine($"  Base: {paymentError.Message}");
        System.Console.WriteLine($"  Card declined: {cardDeclined.Message}");
        System.Console.WriteLine($"    Last 4: {cardDeclined.Tags["CardLastFour"]}");
        System.Console.WriteLine($"    Decline code: {cardDeclined.Tags["DeclineCode"]}");
        System.Console.WriteLine($"  Fraud detected: {fraudDetected.Message}");
        System.Console.WriteLine($"    Risk score: {fraudDetected.Tags["RiskScore"]}");
        System.Console.WriteLine($"    Reason: {fraudDetected.Tags["FraudReason"]}");

        // Type checking
        IError error = cardDeclined;
        if (error is CardDeclinedError declined)
        {
            System.Console.WriteLine($"\nDetected CardDeclinedError: {declined.Tags["CardType"]}");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Error with Metadata

    private static void ErrorWithMetadata()
    {
        System.Console.WriteLine("--- Error with Metadata ---");

        // Rich error metadata for debugging
        var error = new DetailedError("Operation failed")
            .WithCorrelationId(Guid.NewGuid().ToString())
            .WithTimestamp(DateTime.UtcNow)
            .WithSource("UserService")
            .WithOperation("CreateUser")
            .WithUserId("user-789")
            .WithEnvironment("Production")
            .WithTag("Version", "1.2.3")
            .WithTag("ServerName", "web-server-01")
            .WithTag("RequestId", "req-abc-123");

        var result = Result.Fail(error);

        System.Console.WriteLine("Error with rich metadata:");
        System.Console.WriteLine($"  Correlation: {result.Errors[0].Tags["CorrelationId"]}");
        System.Console.WriteLine($"  Source: {result.Errors[0].Tags["Source"]}");
        System.Console.WriteLine($"  Operation: {result.Errors[0].Tags["Operation"]}");
        System.Console.WriteLine($"  Environment: {result.Errors[0].Tags["Environment"]}");
        System.Console.WriteLine($"  Server: {result.Errors[0].Tags["ServerName"]}");

        // Error suitable for logging
        System.Console.WriteLine($"\nFormatted for logging:");
        System.Console.WriteLine($"  [{result.Errors[0].Tags["Timestamp"]}] " +
            $"[{result.Errors[0].Tags["Environment"]}] " +
            $"[{result.Errors[0].Tags["Source"]}] " +
            $"{result.Errors[0].Message} " +
            $"(CorrelationId: {result.Errors[0].Tags["CorrelationId"]})");

        System.Console.WriteLine();
    }

    #endregion

    #region Type-Specific Handling

    private static void TypeSpecificHandling()
    {
        System.Console.WriteLine("--- Type-Specific Handling ---");

        var errors = new IError[]
        {
            new NotFoundError("User", "user-123"),
            new ValidationError("Email", "Invalid"),
            new AuthorizationError("user-456", "admin:write"),
            new DatabaseError("Connection failed").WithRetryCount(3)
        };

        var result = Result.Fail(errors);

        System.Console.WriteLine("Handling different error types:");
        foreach (var error in result.Errors)
        {
            switch (error)
            {
                case NotFoundError notFound:
                    System.Console.WriteLine($"  🔍 Not Found: {notFound.Tags["EntityType"]} " +
                        $"#{notFound.Tags["EntityId"]} (HTTP {notFound.Tags["StatusCode"]})");
                    break;

                case ValidationError validation:
                    System.Console.WriteLine($"  ✗ Validation: {validation.Tags["Field"]} - " +
                        $"{validation.Message}");
                    break;

                case AuthorizationError auth:
                    System.Console.WriteLine($"  🔒 Authorization: User {auth.Tags["UserId"]} " +
                        $"lacks {auth.Tags["RequiredPermission"]}");
                    break;

                case DatabaseError db:
                    System.Console.WriteLine($"  💾 Database: {db.Message} " +
                        $"(Retried {db.Tags["RetryCount"]} times)");
                    break;

                default:
                    System.Console.WriteLine($"  ⚠️ Unknown: {error.Message}");
                    break;
            }
        }

        // Pattern matching for recovery
        var recoveryResult = result.Match(
            onSuccess: () => "Success",
            onFailure: errors =>
            {
                var firstError = errors[0];
                return firstError switch
                {
                    NotFoundError => "Resource not found - redirect to 404",
                    ValidationError => "Invalid input - show validation errors",
                    AuthorizationError => "Access denied - redirect to login",
                    DatabaseError => "Service unavailable - retry later",
                    _ => "Unknown error - contact support"
                };
            }
        );

        System.Console.WriteLine($"\nRecovery strategy: {recoveryResult}");

        System.Console.WriteLine();
    }

    #endregion

    #region Custom Error in Pipeline

    private static void CustomErrorInPipeline()
    {
        System.Console.WriteLine("--- Custom Error in Pipeline ---");

        var email = "test@example.com";
        var age = 15;

        var result = Result<string>.Ok(email)
            .Ensure(
                e => !string.IsNullOrWhiteSpace(e),
                new ValidationError("Email", "Required field")
            )
            .Ensure(
                e => e.Contains("@"),
                new ValidationError("Email", "Invalid format")
                    .WithTag("Pattern", @"^[^@]+@[^@]+$")
            )
            .Bind(e => Result<int>.Ok(age)
                .Ensure(
                    a => a >= 18,
                    new BusinessRuleError("MinimumAge", "User must be 18 or older")
                        .WithTag("MinimumAge", 18)
                        .WithTag("ProvidedAge", age)
                )
                .Map(a => new UserRegistration
                {
                    Email = e,
                    Age = a
                }));

        System.Console.WriteLine($"Pipeline with custom errors: {result.IsFailed}");
        if (result.IsFailed)
        {
            foreach (var error in result.Errors)
            {
                if (error is ValidationError validation)
                {
                    System.Console.WriteLine($"  Validation: {validation.Tags["Field"]} - {error.Message}");
                }
                else if (error is BusinessRuleError business)
                {
                    System.Console.WriteLine($"  Business Rule: {business.Tags["RuleCode"]} - {error.Message}");
                    System.Console.WriteLine($"    Min: {business.Tags["MinimumAge"]}, " +
                        $"Provided: {business.Tags["ProvidedAge"]}");
                }
            }
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Custom Error Classes

    // Simple approach - inherit from Error
    private class NotFoundError : Error
    {
        public NotFoundError(string entityType, string entityId)
            : base($"{entityType} with id '{entityId}' not found")
        {
            WithTag("EntityType", entityType);
            WithTag("EntityId", entityId);
            WithTag("StatusCode", 404);
            WithTag("ErrorType", "NotFound");
        }
    }

    private class ValidationError : Error
    {
        public ValidationError(string field, string message)
            : base($"{field}: {message}")
        {
            WithTag("Field", field);
            WithTag("ErrorType", "Validation");
            WithTag("Severity", "Warning");
        }
    }

    private class AuthorizationError : Error
    {
        public AuthorizationError(string userId, string permission)
            : base($"User '{userId}' lacks permission '{permission}'")
        {
            WithTag("UserId", userId);
            WithTag("RequiredPermission", permission);
            WithTag("StatusCode", 403);
            WithTag("ErrorType", "Authorization");
        }
    }

    private class BusinessRuleError : Error
    {
        public BusinessRuleError(string ruleCode, string message)
            : base(message)
        {
            WithTag("RuleCode", ruleCode);
            WithTag("ErrorType", "BusinessRule");
            WithTag("Severity", "Error");
        }
    }

    // Advanced approach - CRTP for custom fluent methods
    private class DatabaseError : Reason<DatabaseError>, IError
    {
        public DatabaseError(string message) : base(message) { }
        public DatabaseError(string message, ImmutableDictionary<string, object> tags) : base(message, tags) { }

        public DatabaseError WithQuery(string query)
        {
            return WithTag("Query", query);
        }

        public DatabaseError WithRetryCount(int count)
        {
            return WithTag("RetryCount", count);
        }

        public DatabaseError WithConnectionString(string connectionString)
        {
            return WithTag("ConnectionString", connectionString);
        }

        protected override DatabaseError CreateNew(string message, ImmutableDictionary<string, object> tags)
        {
            return new DatabaseError(message, tags);
        }
    }

    private class ApiError : Reason<ApiError>, IError
    {
        public ApiError(string message) : base(message) { }
        public ApiError(string message, ImmutableDictionary<string, object> tags) : base(message, tags) { }

        public ApiError WithEndpoint(string endpoint)
        {
            return WithTag("Endpoint", endpoint);
        }

        public ApiError WithHttpMethod(string method)
        {
            return WithTag("HttpMethod", method);
        }

        public ApiError WithStatusCode(int statusCode)
        {
            return WithTag("StatusCode", statusCode);
        }

        public ApiError WithRetryAfter(int seconds)
        {
            return WithTag("RetryAfter", seconds);
        }

        protected override ApiError CreateNew(string message, ImmutableDictionary<string, object> tags)
        {
            return new ApiError(message, tags);
        }
    }

    private class InventoryError : Reason<InventoryError>, IError
    {
        public InventoryError(string message) : base (message) { }
        public InventoryError(string message, ImmutableDictionary<string, object> tags) : base(message, tags) { }
        public InventoryError(string errorCode, string message) : base(message)
        {
            WithTag("ErrorCode", errorCode);
        }

        public InventoryError WithProductId(string productId)
        {
            return WithTag("ProductId", productId);
        }

        public InventoryError WithRequestedQuantity(int quantity)
        {
            return WithTag("RequestedQuantity", quantity);
        }

        public InventoryError WithAvailableQuantity(int quantity)
        {
            return WithTag("AvailableQuantity", quantity);
        }

        protected override InventoryError CreateNew(string message, ImmutableDictionary<string, object> tags)
        {
            return new InventoryError(message, tags);
        }
    }

    private class PaymentError : Reason<PaymentError>, IError
    {
        public PaymentError(string message) : base(message) { }
        public PaymentError(string message, ImmutableDictionary<string, object> tags) : base(message, tags) { }

        public PaymentError WithTransactionId(string txnId)
        {
            return WithTag("TransactionId", txnId);
        }

        public PaymentError WithAmount(decimal amount)
        {
            return WithTag("Amount", amount);
        }

        public PaymentError WithCurrency(string currency)
        {
            return WithTag("Currency", currency);
        }

        protected override PaymentError CreateNew(string message, ImmutableDictionary<string, object> tags)
        {
            return new PaymentError(message, tags);
        }
    }

    private class CardDeclinedError : PaymentError
    {
        public CardDeclinedError(string message) : base(message) { }

        public CardDeclinedError WithCardLastFour(string lastFour)
        {
            return (CardDeclinedError)WithTag("CardLastFour", lastFour);
        }

        public CardDeclinedError WithCardType(string cardType)
        {
            return (CardDeclinedError)WithTag("CardType", cardType);
        }
    }

    private class FraudDetectedError : PaymentError
    {
        public FraudDetectedError(string message) : base(message) { }

        public FraudDetectedError WithRiskScore(double score)
        {
            return (FraudDetectedError)WithTag("RiskScore", score);
        }

        public FraudDetectedError WithFraudReason(string reason)
        {
            return (FraudDetectedError)WithTag("FraudReason", reason);
        }
    }

    private class DetailedError : Reason<DetailedError>, IError
    {
        public DetailedError(string message) : base(message) { }
        public DetailedError(string message, ImmutableDictionary<string, object> tags) : base(message, tags) { }

        public DetailedError WithCorrelationId(string id)
        {
            return WithTag("CorrelationId", id);
        }

        public DetailedError WithTimestamp(DateTime timestamp)
        {
            return WithTag("Timestamp", timestamp);
        }

        public DetailedError WithSource(string source)
        {
            return WithTag("Source", source);
        }

        public DetailedError WithOperation(string operation)
        {
            return WithTag("Operation", operation);
        }

        public DetailedError WithUserId(string userId)
        {
            return WithTag("UserId", userId);
        }

        public DetailedError WithEnvironment(string environment)
        {
            return WithTag("Environment", environment);
        }

        protected override DetailedError CreateNew(string message, ImmutableDictionary<string, object> tags)
        {
            return new DetailedError(message, tags);
        }
    }

    #endregion

    #region Helper Classes

    private class User
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }

    private class UserRegistration
    {
        public string Email { get; set; } = "";
        public int Age { get; set; }
    }

    private class Transaction
    {
        public string Id { get; set; } = "";
        public decimal Amount { get; set; }
    }

    private class Order
    {
        public string Id { get; set; } = "";
        public List<string> Items { get; set; } = new();
    }

    private class ApiResponse
    {
        public int StatusCode { get; set; }
        public string Data { get; set; } = "";
    }

    #endregion
}
