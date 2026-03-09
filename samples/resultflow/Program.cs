// =============================================================================
// ResultFlow — Pipeline Visualization Sample
//
// ⚠️  CHOOSING THE RIGHT PACKAGE
//
//   This sample uses REslava.Result with REslava.ResultFlow (library-agnostic).
//
//   If your project uses REslava.Result, prefer REslava.Result.Flow instead:
//     • Typed error edges  (-->|UserNotFoundError| FAIL)
//     • Richer type travel (IResultBase anchor)
//     • See samples/result-flow/ for an example
//
//   Use REslava.ResultFlow when targeting FluentResults, ErrorOr, LanguageExt,
//   or any other Result library. See samples/resultflow-fluentresults/ for an
//   example with FluentResults.
//
// REslava.ResultFlow is a SOURCE GENERATOR — library-agnostic.
// It works with REslava.Result, ErrorOr, LanguageExt, or any fluent Result library.
//
// Add [ResultFlow] to any fluent pipeline method.
// The generator walks the chain at compile time and emits a Mermaid diagram
// as a const string — zero runtime overhead, zero manual maintenance.
//
// NEW in v1.38.0:
//   • Type Travel — each node label shows the success type flowing through it
//     e.g.  Bind<br/>User → Order    (type changed)
//           Ensure<br/>Order          (type unchanged)
//   • Async nodes are marked with ⚡ in the label
//
// Sections:
//   1. Guard chain — Ensure × 3
//   2. Risk chain — Bind × 2
//   3. Full pipeline — Ensure + Bind + Tap + Map
//   4. Async pipeline — *Async variants
//   5. Recovery — Or fallback
//   6. Error translation — MapError
//   7. Type Travel — multi-hop type change visible in each node label
// =============================================================================
using Generated.ResultFlow;
using REslava.Result;
using REslava.Result.Extensions;
using REslava.ResultFlow;
using System.Collections.Immutable;

var sep  = new string('─', 60);
var sep2 = new string('═', 60);

Console.WriteLine(sep2);
Console.WriteLine("  ResultFlow — Compile-Time Pipeline Diagrams");
Console.WriteLine("  Library used: REslava.Result");
Console.WriteLine("  ResultFlow itself has NO dependency on any Result library.");
Console.WriteLine(sep2);

void Print(string label, string diagram)
{
    Console.WriteLine();
    Console.WriteLine($"  {label}");
    Console.WriteLine(sep);
    Console.WriteLine(diagram);
}

// Each constant is generated at compile time from the [ResultFlow] attribute.
// Paste into https://mermaid.live to see the visual diagram.
Print("1. Guard chain — Ensure × 3",                  Pipelines_Flows.ValidateOrder);
Print("2. Risk chain — Bind × 2",                     Pipelines_Flows.PlaceOrder);
Print("3. Full pipeline — Ensure+Bind+Tap+Map",       Pipelines_Flows.ProcessCheckout);
Print("4. Async pipeline — *Async variants",           Pipelines_Flows.PlaceOrderAsync);
Print("5. Recovery — Or fallback",                     Pipelines_Flows.WithFallback);
Print("6. Error translation — MapError",               Pipelines_Flows.TranslateErrors);
Print("7. Type Travel — User → Product → Order → string", Pipelines_Flows.FullTypeTravel);

Console.WriteLine();
Console.WriteLine(sep2);
Console.WriteLine("  Runtime verification — pipelines execute normally");
Console.WriteLine(sep2);

// The [ResultFlow] annotation is compile-time only — no runtime effect.
// The pipelines are plain C# methods that run and return Result<T> as usual.
void Run(string label, object result)
{
    var isSuccess = result.GetType().GetProperty("IsSuccess")?.GetValue(result) is true;
    Console.WriteLine($"  {label}: {(isSuccess ? "OK" : "FAIL")}");
}

using var cts = new CancellationTokenSource();
Run("ValidateOrder (valid)             ", Pipelines.ValidateOrder(new Order(1, 42, 99.99m)));
Run("ValidateOrder (bad amount)        ", Pipelines.ValidateOrder(new Order(2, 42, -5m)));
Run("PlaceOrder (success)              ", Pipelines.PlaceOrder(42, 99.99m));
Run("PlaceOrder (user not found)       ", Pipelines.PlaceOrder(999, 50m));
Run("WithFallback (missing user)       ", Pipelines.WithFallback(999));
Run("FullTypeTravel (success)          ", Pipelines.FullTypeTravel(42, 7).GetAwaiter().GetResult());
Run("FullTypeTravel (user not found)   ", Pipelines.FullTypeTravel(999, 7).GetAwaiter().GetResult());
Run("PlaceOrderAsync                   ", Pipelines.PlaceOrderAsync(42, 7, cts.Token).GetAwaiter().GetResult());

Console.WriteLine();

// =============================================================================
// Domain records — same as the lesson series (User, Order, Product)
// =============================================================================
record User(int Id, string Email, string Role);
record Order(int Id, int UserId, decimal Amount);
record Product(int Id, string Name, decimal Price, int Stock);

// =============================================================================
// Pipelines — REslava.Result, annotated with [ResultFlow]
//
// Node colours in the Mermaid output:
//   lavender  = Ensure / EnsureAsync    (Gatekeeper)
//   mint      = Bind / Map / Or         (TransformWithRisk / PureTransform)
//   vanilla   = Tap / TapBoth           (SideEffect)
//   pink      = TapOnFailure / MapError (SideEffectFailure)
//   terminal  = Match                   (Terminal)
//
// NEW — Type Travel labels (v1.38.0):
//   When the semantic model is available, each node label gains a second line:
//     "Bind<br/>User → Order"   ← type changed
//     "Ensure<br/>Order"        ← type unchanged
//   Async nodes are additionally marked with ⚡.
// =============================================================================
static class Pipelines
{
    // ─── 1. Guard chain ─────────────────────────────────────────────────────
    /*
```mermaid
flowchart LR
    N0_Ok["Ok<br/>Order"]:::operation
    N0_Ok --> N1_Ensure
    N1_Ensure["Ensure<br/>Order"]:::gatekeeper
    N1_Ensure -->|pass| N2_Ensure
    N1_Ensure -->|fail| F1["Failure"]:::failure
    N2_Ensure["Ensure<br/>Order"]:::gatekeeper
    N2_Ensure -->|pass| N3_Ensure
    N2_Ensure -->|fail| F2["Failure"]:::failure
    N3_Ensure["Ensure<br/>Order"]:::gatekeeper
    N3_Ensure -->|fail| F3["Failure"]:::failure
    classDef operation fill:#e8f4f0,color:#1c7e6f
    classDef gatekeeper fill:#e3e9fa,color:#3f5c9a
    classDef failure fill:#f8e3e3,color:#b13e3e
```*/
    [ResultFlow]
    public static Result<Order> ValidateOrder(Order order) =>
        Result<Order>.Ok(order)
            .Ensure(o => o.Amount > 0,       "Amount must be positive")
            .Ensure(o => o.Amount < 10_000,  "Amount exceeds limit")
            .Ensure(o => o.UserId > 0,       "Invalid user ID");

    // ─── 2. Risk chain ──────────────────────────────────────────────────────
    /*
```mermaid
flowchart LR
    N0_Bind["Bind<br/>User → Order"]:::transform
    N0_Bind -->|ok| N1_Bind
    N0_Bind -->|fail| F0["Failure"]:::failure
    N1_Bind["Bind<br/>Order"]:::transform
    N1_Bind -->|fail| F1["Failure"]:::failure
    classDef transform fill:#e3f0e8,color:#2f7a5c
    classDef failure fill:#f8e3e3,color:#b13e3e
```*/
    [ResultFlow]
    public static Result<Order> PlaceOrder(int userId, decimal amount) =>
        FindUser(userId)
            .Bind(u  => CheckCredit(u, amount))
            .Bind(SaveOrder);

    // ─── 3. Full pipeline ───────────────────────────────────────────────────
    /*
```mermaid
flowchart LR
    N0_Ensure["Ensure<br/>User"]:::gatekeeper
    N0_Ensure -->|pass| N1_Bind
    N0_Ensure -->|fail| F0["Failure"]:::failure
    N1_Bind["Bind<br/>User → Order"]:::transform
    N1_Bind -->|ok| N2_Tap
    N1_Bind -->|fail| F1["Failure"]:::failure
    N2_Tap["Tap<br/>Order"]:::sideeffect
    N2_Tap --> N3_TapOnFailure
    N3_TapOnFailure["TapOnFailure<br/>Order"]:::sideeffect
    N3_TapOnFailure --> N4_TapBoth
    N4_TapBoth["TapBoth<br/>Order"]:::sideeffect
    N4_TapBoth --> N5_Map
    N5_Map["Map<br/>Order → String"]:::transform
    classDef gatekeeper fill:#e3e9fa,color:#3f5c9a
    classDef failure fill:#f8e3e3,color:#b13e3e
    classDef transform fill:#e3f0e8,color:#2f7a5c
    classDef sideeffect fill:#fff4d9,color:#b8882c
```*/
    [ResultFlow]
    public static Result<string> ProcessCheckout(int userId, decimal amount) =>
        FindUser(userId)
            .Ensure(u => u.Role == "Admin",    "Admins only")
            .Bind(u  => CheckCredit(u, amount))
            .Tap(o   => Log($"Order #{o.Id} ready — ${o.Amount:F2}"))
            .TapOnFailure(e => Log($"Checkout error: {e.Message}"))
            .TapBoth(r => Log($"Pipeline complete ({(r.IsSuccess ? "success" : "failure")})"))
            .Map(o   => $"Confirmed: Order #{o.Id} — ${o.Amount:F2}");

    // ─── 4. Async pipeline ──────────────────────────────────────────────────
    // *Async variants: identical shape to sync — only the types change.
    // Type Travel labels show ⚡ for each async node.
    /*
```mermaid
flowchart LR
    N0_BindAsync["BindAsync ⚡<br/>User → Product"]:::transform
    N0_BindAsync -->|ok| N1_EnsureAsync
    N0_BindAsync -->|fail| F0["Failure"]:::failure
    N1_EnsureAsync["EnsureAsync ⚡<br/>Product"]:::gatekeeper
    N1_EnsureAsync -->|pass| N2_MapAsync
    N1_EnsureAsync -->|fail| F1["Failure"]:::failure
    N2_MapAsync["MapAsync ⚡<br/>Product → Order"]:::transform
    N2_MapAsync --> N3_BindAsync
    N3_BindAsync["BindAsync ⚡<br/>Order"]:::transform
    N3_BindAsync -->|fail| F3["Failure"]:::failure
    classDef transform fill:#e3f0e8,color:#2f7a5c
    classDef failure fill:#f8e3e3,color:#b13e3e
    classDef gatekeeper fill:#e3e9fa,color:#3f5c9a
```*/
    [ResultFlow]
    public static async Task<Result<Order>> PlaceOrderAsync(
        int userId, int productId, CancellationToken ct) =>
        await FindUserAsync(userId, ct)
            .BindAsync(u  => FindProductAsync(productId, ct), ct)
            .EnsureAsync(p => p.Stock > 0, "Product out of stock", ct)
            .MapAsync(p   => new Order(0, userId, p.Price), ct)
            .BindAsync(o  => SaveOrderAsync(o, ct), ct);

    // ─── 5. Recovery — Or fallback ──────────────────────────────────────────
    /*
```mermaid
flowchart LR
    N0_Or["Or<br/>User"]:::transform
    N0_Or -->|ok| N1_Map
    N0_Or -->|fail| F0["Failure"]:::failure
    N1_Map["Map<br/>User → String"]:::transform
    classDef transform fill:#e3f0e8,color:#2f7a5c
    classDef failure fill:#f8e3e3,color:#b13e3e
```*/
    [ResultFlow]
    public static Result<string> WithFallback(int userId) =>
        FindUser(userId)
            .Or(Result<User>.Ok(new User(0, "guest@example.com", "Guest")))
            .Map(u => $"{u.Email} ({u.Role})");

    // ─── 6. Error translation — MapError ────────────────────────────────────
    /*
```mermaid
flowchart LR
    N0_Bind["Bind<br/>User → Order"]:::transform
    N0_Bind -->|ok| N1_Bind
    N0_Bind -->|fail| F0["Failure"]:::failure
    N1_Bind["Bind<br/>Order"]:::transform
    N1_Bind -->|ok| N2_MapError
    N1_Bind -->|fail| F1["Failure"]:::failure
    N2_MapError["MapError<br/>Order"]:::sideeffect
    classDef transform fill:#e3f0e8,color:#2f7a5c
    classDef failure fill:#f8e3e3,color:#b13e3e
    classDef sideeffect fill:#fff4d9,color:#b8882c
```*/
    [ResultFlow]
    public static Result<Order> TranslateErrors(int userId, decimal amount) =>
        FindUser(userId)
            .Bind(u  => CheckCredit(u, amount))
            .Bind(SaveOrder)
            .MapError(errs => errs
                .Select(e => (IError)new ConflictError($"Order failed: {e.Message}"))
                .ToImmutableList());

    // ─── 7. Type Travel — explicit multi-hop type change ───────────────────
    //
    // Every Bind changes the success type:
    //   FindUser       → Result<User>
    //   BindAsync(u)   → Result<Product>   label: "Bind⚡<br/>User → Product"
    //   EnsureAsync(p) → Result<Product>   label: "EnsureAsync⚡<br/>Product"
    //   MapAsync(p)    → Result<Order>     label: "MapAsync⚡<br/>Product → Order"
    //   BindAsync(o)   → Result<Order>     label: "BindAsync⚡<br/>Order"
    //   Map(o)         → Result<string>    label: "Map<br/>Order → string"
    //
    // Paste the generated Mermaid into https://mermaid.live — every node shows
    // the success type and highlights where the type changes.
    /*
```mermaid
flowchart LR
    N0_BindAsync["BindAsync ⚡<br/>User → Product"]:::transform
    N0_BindAsync -->|ok| N1_EnsureAsync
    N0_BindAsync -->|fail| F0["Failure"]:::failure
    N1_EnsureAsync["EnsureAsync ⚡<br/>Product"]:::gatekeeper
    N1_EnsureAsync -->|pass| N2_MapAsync
    N1_EnsureAsync -->|fail| F1["Failure"]:::failure
    N2_MapAsync["MapAsync ⚡<br/>Product → Order"]:::transform
    N2_MapAsync --> N3_BindAsync
    N3_BindAsync["BindAsync ⚡<br/>Order"]:::transform
    N3_BindAsync -->|ok| N4_MapAsync
    N3_BindAsync -->|fail| F3["Failure"]:::failure
    N4_MapAsync["MapAsync ⚡<br/>Order → String"]:::transform
    classDef transform fill:#e3f0e8,color:#2f7a5c
    classDef failure fill:#f8e3e3,color:#b13e3e
    classDef gatekeeper fill:#e3e9fa,color:#3f5c9a
```*/
    [ResultFlow]
    public static async Task<Result<string>> FullTypeTravel(
        int userId, int productId, CancellationToken ct = default) =>
        await FindUserAsync(userId, ct)
            .BindAsync(u  => FindProductAsync(productId, ct), ct)
            .EnsureAsync(p => p.Stock > 0, "Product out of stock", ct)
            .MapAsync(p   => new Order(0, userId, p.Price), ct)
            .BindAsync(o  => SaveOrderAsync(o, ct), ct)
            .MapAsync(o   => $"Order #{o.Id} confirmed — ${o.Amount:F2}", ct);

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static readonly Dictionary<int, User> _users = new()
    {
        [42] = new User(42, "alice@example.com", "Admin"),
        [7]  = new User(7,  "bob@example.com",   "User")
    };
    private static readonly Dictionary<int, Product> _products = new()
    {
        [7]  = new Product(7, "Widget", 29.99m, 100)
    };

    private static Result<User> FindUser(int id) =>
        _users.TryGetValue(id, out var u)
            ? Result<User>.Ok(u)
            : Result<User>.Fail(new NotFoundError($"User {id} not found"));

    private static Result<Order> CheckCredit(User user, decimal amount) =>
        amount <= 500
            ? Result<Order>.Ok(new Order(0, user.Id, amount))
            : Result<Order>.Fail(new ValidationError("amount", "Credit limit exceeded (max 500)"));

    private static Result<Order> SaveOrder(Order order) =>
        Result<Order>.Ok(order with { Id = new Random().Next(1000, 9999) });

    private static async Task<Result<User>> FindUserAsync(int id, CancellationToken ct)
    {
        await Task.Delay(5, ct);
        return FindUser(id);
    }

    private static async Task<Result<Product>> FindProductAsync(int id, CancellationToken ct)
    {
        await Task.Delay(5, ct);
        return _products.TryGetValue(id, out var p)
            ? Result<Product>.Ok(p)
            : Result<Product>.Fail(new NotFoundError($"Product {id} not found"));
    }

    private static async Task<Result<Order>> SaveOrderAsync(Order order, CancellationToken ct)
    {
        await Task.Delay(5, ct);
        return SaveOrder(order);
    }

    private static void Log(string msg) => Console.WriteLine($"    [LOG] {msg}");
}
