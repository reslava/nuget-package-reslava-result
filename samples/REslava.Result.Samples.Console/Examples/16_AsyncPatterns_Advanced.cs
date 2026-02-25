using REslava.Result;
using REslava.Result.Extensions;

namespace REslava.Result.Samples.Console.Examples;

/// <summary>
/// Demonstrates advanced async patterns:
/// WhenAll, Retry, Timeout, TapOnFailure, OkIf/FailIf, Try/TryAsync,
/// and a combined pipeline showing them working together.
/// </summary>
public static class AsyncPatterns_Advanced
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== Advanced Async Patterns (v1.18.0–v1.20.0) ===\n");

        await WhenAllDemo();
        await RetryDemo();
        await TimeoutDemo();
        TapOnFailureDemo();
        OkIfFailIfDemo();
        TryDemo();
        await CombinedPipelineDemo();

        System.Console.WriteLine("=== Advanced Async Patterns Complete ===");
    }

    // ── 1. Result.WhenAll — concurrent operations, aggregated errors ─────────

    private static async Task WhenAllDemo()
    {
        System.Console.WriteLine("1. Result.WhenAll — run 3 ops concurrently, aggregate errors:");
        System.Console.WriteLine("-------------------------------------------------------------");

        // All succeed
        var all = await Result.WhenAll(
            FetchUser(1),
            FetchProduct("P-001"),
            FetchInventory("P-001")
        );

        if (all.IsSuccess)
        {
            var (user, product, inventory) = all.Value;
            System.Console.WriteLine($"  All ok: user={user.Name}, product={product.Name}, stock={inventory.Stock}");
        }

        // One fails → errors from ALL failed ops are aggregated
        var mixed = await Result.WhenAll(
            FetchUser(99),           // fails — user not found
            FetchProduct("P-001"),   // succeeds
            FetchInventory("P-BAD")  // fails — inventory not found
        );

        System.Console.WriteLine($"  Mixed: IsSuccess={mixed.IsSuccess}, error count={mixed.Errors.Count}");
        foreach (var err in mixed.Errors)
            System.Console.WriteLine($"    - {err.Message}");

        System.Console.WriteLine();
    }

    // ── 2. Result.Retry — transient failure recovery ─────────────────────────

    private static async Task RetryDemo()
    {
        System.Console.WriteLine("2. Result.Retry — retry with configurable delay and backoff:");
        System.Console.WriteLine("------------------------------------------------------------");

        var attempt = 0;

        // Simulates a flaky external call that succeeds on attempt 3
        var result = await Result.Retry(
            operation: async () =>
            {
                attempt++;
                System.Console.WriteLine($"  Attempt {attempt}...");
                await Task.Delay(1);
                if (attempt < 3)
                    return Result<string>.Fail($"Transient failure #{attempt}");
                return Result<string>.Ok("Connected!");
            },
            maxRetries: 3,
            delay: TimeSpan.FromMilliseconds(10),
            backoffFactor: 1.5);

        System.Console.WriteLine($"  Final: {(result.IsSuccess ? result.Value : result.Errors[0].Message)}");

        // Exhausts all retries → returns accumulated errors
        var alwaysFails = await Result.Retry(
            () => Task.FromResult(Result<string>.Fail("Service unavailable")),
            maxRetries: 2,
            delay: TimeSpan.FromMilliseconds(5));

        System.Console.WriteLine($"  Exhausted: IsSuccess={alwaysFails.IsSuccess}, errors={alwaysFails.Errors.Count}");

        System.Console.WriteLine();
    }

    // ── 3. .Timeout() — enforce time limit ───────────────────────────────────

    private static async Task TimeoutDemo()
    {
        System.Console.WriteLine("3. .Timeout() — enforce time limit on async operation:");
        System.Console.WriteLine("-------------------------------------------------------");

        // Fast operation — completes within timeout
        var fast = await FetchUser(1).Timeout(TimeSpan.FromSeconds(2));
        System.Console.WriteLine($"  Fast op:  IsSuccess={fast.IsSuccess}");

        // Slow operation — exceeds timeout
        var slow = await SlowOperation().Timeout(TimeSpan.FromMilliseconds(50));
        System.Console.WriteLine($"  Slow op:  IsSuccess={slow.IsSuccess}");
        if (slow.IsFailure)
            System.Console.WriteLine($"    -> {slow.Errors[0].Message}");

        System.Console.WriteLine();
    }

    // ── 4. TapOnFailure — side-effect on failure, result unchanged ───────────

    private static void TapOnFailureDemo()
    {
        System.Console.WriteLine("4. TapOnFailure — side-effect only on failure:");
        System.Console.WriteLine("----------------------------------------------");

        // Failure case — tap fires, error is logged
        var fail = Result<string>.Fail("Database connection lost")
            .TapOnFailure(err => System.Console.WriteLine($"  [LOG] Error captured: {err.Message}"));

        System.Console.WriteLine($"  Result unchanged: IsFailure={fail.IsFailure}");

        // Success case — tap does NOT fire
        var ok = Result<string>.Ok("data")
            .TapOnFailure((IError err) => System.Console.WriteLine("  [LOG] This should NOT print"));

        System.Console.WriteLine($"  Success untouched: IsSuccess={ok.IsSuccess}");

        // TapBoth — fires regardless of outcome
        Result<int>.Ok(42)
            .TapBoth(r => System.Console.WriteLine($"  [TRACE] Outcome: {(r.IsSuccess ? $"ok({r.Value})" : "fail")}"));

        System.Console.WriteLine();
    }

    // ── 5. Result.OkIf / FailIf — boolean condition → Result factory ─────────

    private static void OkIfFailIfDemo()
    {
        System.Console.WriteLine("5. Result.OkIf / FailIf — condition-based factories:");
        System.Console.WriteLine("----------------------------------------------------");

        int userAge = 20;
        int minAge = 18;

        // OkIf — condition must be TRUE for success
        var ageCheck = Result.OkIf(userAge >= minAge, $"Must be at least {minAge}");
        System.Console.WriteLine($"  OkIf(20 >= 18): {(ageCheck.IsSuccess ? "OK" : "FAIL")}");

        var underAge = Result.OkIf(15 >= minAge, $"Must be at least {minAge}");
        System.Console.WriteLine($"  OkIf(15 >= 18): {(underAge.IsSuccess ? "OK" : underAge.Errors[0].Message)}");

        // FailIf — condition must be FALSE for success
        bool isAccountLocked = false;
        var lockCheck = Result.FailIf(isAccountLocked, "Account is locked");
        System.Console.WriteLine($"  FailIf(locked=false): {(lockCheck.IsSuccess ? "OK" : "FAIL")}");

        var lockedResult = Result.FailIf(true, "Account is locked");
        System.Console.WriteLine($"  FailIf(locked=true):  {lockedResult.Errors[0].Message}");

        // Generic OkIf — includes the value
        var product = new ProductDto("Laptop", 999.99m);
        var priceCheck = Result<ProductDto>.OkIf(
            product.Price < 10_000m,
            product,
            "Price exceeds maximum allowed");
        System.Console.WriteLine($"  OkIf price ok: {(priceCheck.IsSuccess ? priceCheck.Value!.Name : priceCheck.Errors[0].Message)}");

        System.Console.WriteLine();
    }

    // ── 6. Result.Try / TryAsync — exception-safe wrapping ───────────────────

    private static void TryDemo()
    {
        System.Console.WriteLine("6. Result<T>.Try / TryAsync — wrap exceptions as typed errors:");
        System.Console.WriteLine("--------------------------------------------------------------");

        // Try — wraps any exception as ExceptionError
        var parseOk = Result<int>.Try(() => int.Parse("42"));
        System.Console.WriteLine($"  Try parse '42':    {(parseOk.IsSuccess ? parseOk.Value : "fail")}");

        var parseFail = Result<int>.Try(() => int.Parse("not-a-number"));
        System.Console.WriteLine($"  Try parse 'NaN':   IsFailure={parseFail.IsFailure}");
        if (parseFail.IsFailure)
            System.Console.WriteLine($"    -> {parseFail.Errors[0].Message}");

        // Try with custom error mapper
        var customErr = Result<string>.Try(
            () => throw new InvalidOperationException("File system error"),
            ex => new Error($"IO failure: {ex.Message}"));
        System.Console.WriteLine($"  Try custom mapper: {customErr.Errors[0].Message}");

        System.Console.WriteLine();
    }

    // ── 7. Combined pipeline ──────────────────────────────────────────────────

    private static async Task CombinedPipelineDemo()
    {
        System.Console.WriteLine("7. Combined: Retry + Timeout + TapOnFailure in one pipeline:");
        System.Console.WriteLine("------------------------------------------------------------");

        var attempt = 0;

        // 3 retries, each with a 100ms timeout, log every failure
        var result = await Result.Retry(
            operation: async () =>
            {
                attempt++;
                var opResult = await SlowOrFastOperation(attempt).Timeout(TimeSpan.FromMilliseconds(150));
                return opResult
                    .TapOnFailure(err => System.Console.WriteLine($"  [WARN] Attempt {attempt} failed: {err.Message}"));
            },
            maxRetries: 3,
            delay: TimeSpan.FromMilliseconds(5));

        System.Console.WriteLine($"  Pipeline result: {(result.IsSuccess ? result.Value : $"FAILED after {attempt} attempts")}");
        System.Console.WriteLine();
    }

    // ── Simulated async operations ────────────────────────────────────────────

    private static Task<Result<UserDto>> FetchUser(int id) =>
        Task.FromResult(id == 1
            ? Result<UserDto>.Ok(new UserDto(1, "Alice"))
            : Result<UserDto>.Fail($"User {id} not found"));

    private static Task<Result<ProductDto>> FetchProduct(string sku) =>
        Task.FromResult(sku == "P-BAD"
            ? Result<ProductDto>.Fail($"Product '{sku}' not found")
            : Result<ProductDto>.Ok(new ProductDto(sku, 29.99m)));

    private static Task<Result<InventoryDto>> FetchInventory(string sku) =>
        Task.FromResult(sku == "P-BAD"
            ? Result<InventoryDto>.Fail($"Inventory for '{sku}' not found")
            : Result<InventoryDto>.Ok(new InventoryDto(sku, 42)));

    private static async Task<Result<string>> SlowOperation()
    {
        await Task.Delay(500);
        return Result<string>.Ok("slow result");
    }

    private static async Task<Result<string>> SlowOrFastOperation(int attempt)
    {
        // First two attempts are slow (will timeout), third is fast
        if (attempt <= 2)
            await Task.Delay(200);
        else
            await Task.Delay(1);
        return Result<string>.Ok($"Done on attempt {attempt}");
    }

    // ── Domain types ──────────────────────────────────────────────────────────

    private record UserDto(int Id, string Name);
    private record ProductDto(string Name, decimal Price);
    private record InventoryDto(string Sku, int Stock);
}
