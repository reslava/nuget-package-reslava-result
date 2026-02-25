using REslava.Result;
using REslava.Result.Extensions;

namespace REslava.Result.Samples.Console.Examples;

/// <summary>
/// Demonstrates the native Validation DSL — 19 fluent rules on ValidatorRuleBuilder&lt;T&gt;.
/// No source generators needed. Pure library, works anywhere (console, API, services).
/// </summary>
public static class ValidationDSL
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== Validation DSL (v1.27.0) ===\n");

        StringRules();
        NumericRules();
        CollectionRules();
        RealWorldValidator();
        MultipleErrors();

        System.Console.WriteLine("=== Validation DSL Complete ===");
        await Task.CompletedTask;
    }

    // ── 1. String rules ──────────────────────────────────────────────────────

    private static void StringRules()
    {
        System.Console.WriteLine("1. String rules:");
        System.Console.WriteLine("----------------");

        var validator = new ValidatorRuleBuilder<UserRegistration>()
            .NotEmpty(u => u.Name)
            .MinLength(u => u.Name, 2)
            .MaxLength(u => u.Name, 50)
            .NotWhiteSpace(u => u.Username)
            .EmailAddress(u => u.Email)
            .Matches(u => u.Phone, @"^\+?[0-9\s\-]{7,15}$")
            .StartsWith(u => u.Username, "user_", errorMessage: "Username must start with 'user_'")
            .Build();

        var valid = new UserRegistration("Alice", "user_alice", "alice@example.com", "+1 555 1234");
        var result1 = validator.Validate(valid);
        System.Console.WriteLine($"  Valid user: {(result1.IsSuccess ? "OK" : "FAIL")}");

        var badEmail = new UserRegistration("Bob", "user_bob", "not-an-email", "+1 555 5678");
        var result2 = validator.Validate(badEmail);
        System.Console.WriteLine($"  Bad email:  {(result2.IsSuccess ? "OK" : "FAIL")}");
        if (result2.IsFailure)
            System.Console.WriteLine($"    -> {result2.Errors[0].Message}");

        var shortName = new UserRegistration("X", "user_x", "x@example.com", "+1 555 9999");
        var result3 = validator.Validate(shortName);
        System.Console.WriteLine($"  Short name: {(result3.IsSuccess ? "OK" : "FAIL")}");
        if (result3.IsFailure)
            System.Console.WriteLine($"    -> {result3.Errors[0].Message}");

        System.Console.WriteLine();
    }

    // ── 2. Numeric rules ─────────────────────────────────────────────────────

    private static void NumericRules()
    {
        System.Console.WriteLine("2. Numeric rules:");
        System.Console.WriteLine("-----------------");

        var validator = new ValidatorRuleBuilder<CreateOrderRequest>()
            .NotEmpty(o => o.CustomerId)
            .Positive<CreateOrderRequest, decimal>(o => o.Amount)   // > 0
            .NonNegative<CreateOrderRequest, int>(o => o.Quantity)  // >= 0
            .Range(o => o.Quantity, 1, 1000)
            .LessThan(o => o.DiscountPercent, 100m)
            .GreaterThan<CreateOrderRequest, decimal>(o => o.DiscountPercent, -1m)
            .Build();

        var valid = new CreateOrderRequest("CUST-1", 99.99m, 3, 10m);
        var r1 = validator.Validate(valid);
        System.Console.WriteLine($"  Valid order:     {(r1.IsSuccess ? "OK" : "FAIL")}");

        var zeroAmount = new CreateOrderRequest("CUST-2", 0m, 1, 0m);
        var r2 = validator.Validate(zeroAmount);
        System.Console.WriteLine($"  Zero amount:     {(r2.IsSuccess ? "OK" : "FAIL")}");
        if (r2.IsFailure)
            System.Console.WriteLine($"    -> {r2.Errors[0].Message}");

        var badQuantity = new CreateOrderRequest("CUST-3", 10m, 5000, 0m);
        var r3 = validator.Validate(badQuantity);
        System.Console.WriteLine($"  Over max qty:    {(r3.IsSuccess ? "OK" : "FAIL")}");
        if (r3.IsFailure)
            System.Console.WriteLine($"    -> {r3.Errors[0].Message}");

        System.Console.WriteLine();
    }

    // ── 3. Collection rules ──────────────────────────────────────────────────

    private static void CollectionRules()
    {
        System.Console.WriteLine("3. Collection rules:");
        System.Console.WriteLine("--------------------");

        var validator = new ValidatorRuleBuilder<BatchRequest>()
            .NotEmpty<BatchRequest, string>(b => b.ItemIds)          // collection overload
            .MinCount<BatchRequest, string>(b => b.ItemIds, 1)
            .MaxCount<BatchRequest, string>(b => b.ItemIds, 100)
            .NotNull(b => b.Metadata)
            .Build();

        var valid = new BatchRequest(["item-1", "item-2", "item-3"], new Dictionary<string, string>());
        var r1 = validator.Validate(valid);
        System.Console.WriteLine($"  Valid batch:     {(r1.IsSuccess ? "OK" : "FAIL")}");

        var empty = new BatchRequest([], new Dictionary<string, string>());
        var r2 = validator.Validate(empty);
        System.Console.WriteLine($"  Empty IDs:       {(r2.IsSuccess ? "OK" : "FAIL")}");
        if (r2.IsFailure)
            System.Console.WriteLine($"    -> {r2.Errors[0].Message}");

        var tooMany = new BatchRequest(Enumerable.Range(1, 150).Select(i => $"item-{i}").ToList(), null!);
        var r3 = validator.Validate(tooMany);
        System.Console.WriteLine($"  Too many items:  {(r3.IsSuccess ? "OK" : "FAIL")}");
        if (r3.IsFailure)
            System.Console.WriteLine($"    -> {r3.Errors[0].Message}");

        System.Console.WriteLine();
    }

    // ── 4. Real-world validator ──────────────────────────────────────────────

    private static void RealWorldValidator()
    {
        System.Console.WriteLine("4. Real-world: CreateOrderRequest validator:");
        System.Console.WriteLine("--------------------------------------------");

        // Build the validator once (typically in DI setup / static readonly)
        var orderValidator = BuildOrderValidator();

        var requests = new[]
        {
            new CreateOrderRequest("CUST-001", 250.00m, 2, 0m),     // valid
            new CreateOrderRequest("", 100m, 1, 0m),                 // empty customer
            new CreateOrderRequest("CUST-002", -5m, 1, 0m),         // negative amount
        };

        foreach (var req in requests)
        {
            var result = orderValidator.Validate(req);

            var tag = result.IsSuccess ? "OK  " : "FAIL";
            var summary = result.IsSuccess
                ? $"Amount={req.Amount:C}"
                : string.Join("; ", result.ValidationErrors.Select(e => e.Message));
            System.Console.WriteLine($"  [{tag}] CustomerId='{req.CustomerId}' -> {summary}");
        }

        System.Console.WriteLine();
    }

    private static ValidatorRuleSet<CreateOrderRequest> BuildOrderValidator()
    {
        return new ValidatorRuleBuilder<CreateOrderRequest>()
            .NotEmpty(o => o.CustomerId)
            .MaxLength(o => o.CustomerId, 50)
            .Positive<CreateOrderRequest, decimal>(o => o.Amount)
            .Range(o => o.Quantity, 1, 1000)
            .NonNegative<CreateOrderRequest, decimal>(o => o.DiscountPercent)
            .LessThan(o => o.DiscountPercent, 100m)
            .Build();
    }

    // ── 5. Multiple errors returned at once ──────────────────────────────────

    private static void MultipleErrors()
    {
        System.Console.WriteLine("5. Multiple errors (all rules run, errors aggregated):");
        System.Console.WriteLine("------------------------------------------------------");

        // DSL uses Ensure() under the hood — all rules run, not short-circuit
        var validator = new ValidatorRuleBuilder<UserRegistration>()
            .NotEmpty(u => u.Name)
            .MinLength(u => u.Name, 2)
            .EmailAddress(u => u.Email)
            .NotWhiteSpace(u => u.Username)
            .Build();

        var bad = new UserRegistration("", "   ", "bad-email", "+1 555 0000");
        var result = validator.Validate(bad);

        System.Console.WriteLine($"  IsSuccess: {result.IsSuccess}");
        System.Console.WriteLine($"  Error count: {result.Errors.Count}");
        foreach (var err in result.ValidationErrors)
        {
            var field = (err as ValidationError)?.FieldName ?? "General";
            System.Console.WriteLine($"    - [{field}] {err.Message}");
        }

        System.Console.WriteLine();
    }

    // ── Supporting records ────────────────────────────────────────────────────

    private record UserRegistration(string Name, string Username, string Email, string Phone);
    private record CreateOrderRequest(string CustomerId, decimal Amount, int Quantity, decimal DiscountPercent);
    private record BatchRequest(IEnumerable<string> ItemIds, Dictionary<string, string>? Metadata);
}
