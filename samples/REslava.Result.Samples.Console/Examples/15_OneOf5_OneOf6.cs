using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Samples.Console.Examples;

/// <summary>
/// Demonstrates OneOf&lt;T1..T5&gt; and OneOf&lt;T1..T6&gt; — exhaustive discriminated unions
/// for multi-outcome workflows, plus chain extensions (ToFiveWay, ToSixWay, ToFourWay, etc.).
/// </summary>
public static class AdvancedPatterns_OneOf5_OneOf6
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== OneOf5 / OneOf6 Discriminated Unions (v1.27.0) ===\n");

        FiveWayMatch();
        SixWayMatch();
        SwitchDispatch();
        ChainExtensionsUpward();
        ChainExtensionsDownward();
        RealWorldCheckoutPipeline();

        System.Console.WriteLine("=== OneOf5 / OneOf6 Complete ===");
        await Task.CompletedTask;
    }

    // ── 1. Exhaustive 5-way Match ─────────────────────────────────────────────

    private static void FiveWayMatch()
    {
        System.Console.WriteLine("1. OneOf<T1..T5> — exhaustive Match (all 5 cases required at compile time):");
        System.Console.WriteLine("--------------------------------------------------------------------------");

        var outcomes = new OneOf<PaymentSuccess, InsufficientFunds, CardDeclined, FraudAlert, NetworkError>[]
        {
            new PaymentSuccess("TXN-001", 99.99m),
            new InsufficientFunds(50.00m, 99.99m),
            new CardDeclined("Card expired"),
            new FraudAlert("Suspicious activity"),
            new NetworkError("Gateway timeout"),
        };

        foreach (var outcome in outcomes)
        {
            var message = outcome.Match(
                case1: ok    => $"  Charged {ok.Amount:C} (TXN: {ok.TransactionId})",
                case2: ins   => $"  Insufficient funds — balance {ins.Balance:C}, needed {ins.Required:C}",
                case3: dec   => $"  Card declined: {dec.Reason}",
                case4: fraud => $"  Fraud detected: {fraud.Description}",
                case5: net   => $"  Network error: {net.Message}"
            );
            System.Console.WriteLine(message);
        }

        System.Console.WriteLine();
    }

    // ── 2. Six-way Match ─────────────────────────────────────────────────────

    private static void SixWayMatch()
    {
        System.Console.WriteLine("2. OneOf<T1..T6> — 6 typed outcomes:");
        System.Console.WriteLine("------------------------------------");

        OneOf<PaymentSuccess, InsufficientFunds, CardDeclined, FraudAlert, NetworkError, RateLimitError>
            rateLimited = new RateLimitError(30);

        var message = rateLimited.Match(
            case1: ok    => $"  OK: {ok.TransactionId}",
            case2: ins   => $"  Insufficient: {ins.Balance:C}",
            case3: dec   => $"  Declined: {dec.Reason}",
            case4: fraud => $"  Fraud: {fraud.Description}",
            case5: net   => $"  Network: {net.Message}",
            case6: rl    => $"  Rate limited — retry after {rl.RetryAfterSeconds}s"
        );
        System.Console.WriteLine(message);

        // Implicit conversion (no FromT* needed)
        OneOf<PaymentSuccess, InsufficientFunds, CardDeclined, FraudAlert, NetworkError, RateLimitError>
            success = new PaymentSuccess("TXN-999", 5.00m);
        System.Console.WriteLine($"  IsT1 (PaymentSuccess): {success.IsT1}");
        System.Console.WriteLine($"  IsT6 (RateLimitError): {success.IsT6}");

        System.Console.WriteLine();
    }

    // ── 3. Switch — action-only dispatch ─────────────────────────────────────

    private static void SwitchDispatch()
    {
        System.Console.WriteLine("3. Switch — void action dispatch:");
        System.Console.WriteLine("---------------------------------");

        OneOf<PaymentSuccess, InsufficientFunds, CardDeclined, FraudAlert, NetworkError>
            outcome = new CardDeclined("3D-Secure challenge failed");

        outcome.Switch(
            case1: ok    => System.Console.WriteLine($"  [LOG] Payment ok: {ok.TransactionId}"),
            case2: ins   => System.Console.WriteLine($"  [LOG] Insufficient funds: {ins.Balance:C}"),
            case3: dec   => System.Console.WriteLine($"  [ALERT] Card declined: {dec.Reason}"),
            case4: fraud => System.Console.WriteLine($"  [SECURITY] Fraud: {fraud.Description}"),
            case5: net   => System.Console.WriteLine($"  [WARN] Network error: {net.Message}")
        );

        System.Console.WriteLine();
    }

    // ── 4. Chain extensions — upward (2 → 3 → 4 → 5 → 6) ───────────────────

    private static void ChainExtensionsUpward()
    {
        System.Console.WriteLine("4. Chain extensions — up-conversions (ToThreeWay → ToFiveWay → ToSixWay):");
        System.Console.WriteLine("------------------------------------------------------------------------");

        // Start with a simple 2-way outcome
        OneOf<PaymentError, PaymentSuccess> twoWay = new PaymentSuccess("TXN-100", 25.00m);
        System.Console.WriteLine($"  2-way IsT2 (success): {twoWay.IsT2}");

        // Upgrade to 3-way by anchoring a new T3 type
        OneOf<PaymentError, PaymentSuccess, CardDeclined> threeWay =
            twoWay.ToThreeWay<PaymentError, PaymentSuccess, CardDeclined>(new CardDeclined("default"));
        System.Console.WriteLine($"  3-way IsT2 (success): {threeWay.IsT2}");

        // Upgrade to 4-way
        OneOf<PaymentError, PaymentSuccess, CardDeclined, FraudAlert> fourWay =
            threeWay.ToFourWay<PaymentError, PaymentSuccess, CardDeclined, FraudAlert>(new FraudAlert("default"));
        System.Console.WriteLine($"  4-way IsT2 (success): {fourWay.IsT2}");

        // Upgrade to 5-way
        OneOf<PaymentError, PaymentSuccess, CardDeclined, FraudAlert, NetworkError> fiveWay =
            fourWay.ToFiveWay<PaymentError, PaymentSuccess, CardDeclined, FraudAlert, NetworkError>(new NetworkError("default"));
        System.Console.WriteLine($"  5-way IsT2 (success): {fiveWay.IsT2}");

        // Upgrade to 6-way
        OneOf<PaymentError, PaymentSuccess, CardDeclined, FraudAlert, NetworkError, RateLimitError> sixWay =
            fiveWay.ToSixWay<PaymentError, PaymentSuccess, CardDeclined, FraudAlert, NetworkError, RateLimitError>(new RateLimitError(0));
        System.Console.WriteLine($"  6-way IsT2 (success): {sixWay.IsT2}  (value preserved through all promotions)");

        System.Console.WriteLine();
    }

    // ── 5. Chain extensions — downward (5 → 4, mapping T5 case) ─────────────

    private static void ChainExtensionsDownward()
    {
        System.Console.WriteLine("5. Chain extensions — down-conversions:");
        System.Console.WriteLine("---------------------------------------");

        // 5-way with T5 (NetworkError) set
        OneOf<PaymentSuccess, InsufficientFunds, CardDeclined, FraudAlert, NetworkError>
            fiveWay = new NetworkError("DNS failure");

        // Downgrade to 4-way by mapping NetworkError → FraudAlert (or drop it)
        // Option A: Map T5 to T4
        OneOf<PaymentSuccess, InsufficientFunds, CardDeclined, FraudAlert> fourWay =
            fiveWay.ToFourWay<PaymentSuccess, InsufficientFunds, CardDeclined, FraudAlert, NetworkError>(
                t5ToT4: net => new FraudAlert($"Network fault treated as alert: {net.Message}")
            );
        System.Console.WriteLine($"  ToFourWay (T5→T4): IsT4 = {fourWay.IsT4}");

        // Option B: Filter (returns null when T5)
        OneOf<PaymentSuccess, InsufficientFunds, CardDeclined, FraudAlert>? filtered =
            fiveWay.ToFourWay<PaymentSuccess, InsufficientFunds, CardDeclined, FraudAlert, NetworkError>();
        System.Console.WriteLine($"  ToFourWay filter (T5 case): result is null = {filtered == null}");

        // 3-way with T1 set → downgrade to 2-way
        OneOf<PaymentError, PaymentSuccess, CardDeclined> threeWay =
            new PaymentSuccess("TXN-200", 10m);
        OneOf<PaymentError, PaymentSuccess>? twoWay =
            threeWay.ToTwoWay<PaymentError, PaymentSuccess, CardDeclined>();
        System.Console.WriteLine($"  ToTwoWay filter (T1 case): result IsT2 = {twoWay?.IsT2}");

        System.Console.WriteLine();
    }

    // ── 6. Real-world: checkout pipeline with 5 outcomes ─────────────────────

    private static void RealWorldCheckoutPipeline()
    {
        System.Console.WriteLine("6. Real-world: checkout pipeline returning 5 typed outcomes:");
        System.Console.WriteLine("------------------------------------------------------------");

        var scenarios = new (string Label, decimal Amount, string CustomerId)[]
        {
            ("Valid purchase",  49.99m, "CUST-A"),
            ("Insufficient",    999m,  "CUST-B"),
            ("Fraud flag",      1m,    "CUST-FRAUD"),
            ("Network fail",    25m,   "CUST-NET"),
            ("Card declined",   75m,   "CUST-DEC"),
        };

        foreach (var (label, amount, customerId) in scenarios)
        {
            var outcome = RunCheckout(customerId, amount);
            var http = outcome.Match(
                case1: _     => 200,
                case2: _     => 402,
                case3: _     => 402,
                case4: _     => 403,
                case5: _     => 503
            );

            outcome.Switch(
                case1: ok    => System.Console.WriteLine($"  [{http}] {label}: charged {ok.Amount:C}"),
                case2: ins   => System.Console.WriteLine($"  [{http}] {label}: balance {ins.Balance:C} < {ins.Required:C}"),
                case3: dec   => System.Console.WriteLine($"  [{http}] {label}: {dec.Reason}"),
                case4: fraud => System.Console.WriteLine($"  [{http}] {label}: {fraud.Description}"),
                case5: net   => System.Console.WriteLine($"  [{http}] {label}: {net.Message}")
            );
        }

        System.Console.WriteLine();
    }

    private static OneOf<PaymentSuccess, InsufficientFunds, CardDeclined, FraudAlert, NetworkError>
        RunCheckout(string customerId, decimal amount)
    {
        // Simulate deterministic outcomes based on test inputs
        return customerId switch
        {
            "CUST-FRAUD" => new FraudAlert("High-risk transaction pattern"),
            "CUST-NET"   => new NetworkError("Payment gateway unreachable"),
            "CUST-DEC"   => new CardDeclined("Card reported lost"),
            "CUST-B"     => new InsufficientFunds(Balance: 10m, Required: amount),
            _            => new PaymentSuccess($"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}", amount),
        };
    }

    // ── Domain types ──────────────────────────────────────────────────────────

    private record PaymentSuccess(string TransactionId, decimal Amount);
    private record InsufficientFunds(decimal Balance, decimal Required);
    private record CardDeclined(string Reason);
    private record FraudAlert(string Description);
    private record NetworkError(string Message);
    private record RateLimitError(int RetryAfterSeconds);
    private record PaymentError(string Message);
}
