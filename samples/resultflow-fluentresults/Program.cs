// =============================================================================
// REslava.ResultFlow + FluentResults — Library-Agnostic Pipeline Visualization
//
// This sample demonstrates that REslava.ResultFlow works with any Result library.
// FluentResults is used here — no dependency on REslava.Result whatsoever.
//
// REslava.ResultFlow recognises Bind and Map from its built-in convention
// dictionary — zero configuration required. Add [ResultFlow] to any fluent
// pipeline method and the generator emits a Mermaid diagram as a const string.
//
// Type Travel (v1.38.0):
//   Each node label shows the FluentResults<T> success type at that step.
//   e.g.  Bind<br/>User → Product    (type changed)
//         Bind<br/>Order              (type unchanged)
//
// Sections:
//   1. Risk chain   — Bind × 2 with type travel (User → Product → Order)
//   2. Full chain   — Bind × 2 + Map            (User → Product → Order → string)
//   3. Guard chain  — inline validation via Bind (same type throughout)
// =============================================================================
using FluentResults;
using Generated.ResultFlow;
using REslava.ResultFlow;

var sep  = new string('─', 60);
var sep2 = new string('═', 60);

Console.WriteLine(sep2);
Console.WriteLine("  REslava.ResultFlow + FluentResults");
Console.WriteLine("  Library-agnostic — no REslava.Result dependency");
Console.WriteLine(sep2);

void Print(string label, string diagram)
{
    Console.WriteLine();
    Console.WriteLine($"  {label}");
    Console.WriteLine(sep);
    Console.WriteLine(diagram);
}

Print("1. Risk chain — Bind × 2, type travel",         Pipelines_Flows.PlaceOrder);
Print("2. Full chain — Bind × 2 + Map",                Pipelines_Flows.ProcessCheckout);
Print("3. Guard chain — validation via Bind",           Pipelines_Flows.ValidateAndPlace);

Console.WriteLine();
Console.WriteLine(sep2);
Console.WriteLine("  Runtime verification");
Console.WriteLine(sep2);

void Run(string label, object result)
{
    var ok = result.GetType().GetProperty("IsSuccess")?.GetValue(result) is true;
    Console.WriteLine($"  {label}: {(ok ? "OK" : "FAIL")}");
}

Run("PlaceOrder (success)              ", Pipelines.PlaceOrder(42, 7));
Run("PlaceOrder (user not found)       ", Pipelines.PlaceOrder(999, 7));
Run("PlaceOrder (product not found)    ", Pipelines.PlaceOrder(42, 99));
Run("ProcessCheckout (success)         ", Pipelines.ProcessCheckout(42, 7));
Run("ProcessCheckout (user not found)  ", Pipelines.ProcessCheckout(999, 7));
Run("ValidateAndPlace (success)        ", Pipelines.ValidateAndPlace(42, 7, 200m));
Run("ValidateAndPlace (out of stock)   ", Pipelines.ValidateAndPlace(42, 8, 200m));
Run("ValidateAndPlace (price too high) ", Pipelines.ValidateAndPlace(42, 7, 10m));

Console.WriteLine();

// =============================================================================
// Domain records
// =============================================================================
record User(int Id, string Email);
record Product(int Id, string Name, decimal Price, int Stock);
record Order(int Id, int UserId, decimal Amount);

// =============================================================================
// Pipelines — FluentResults, annotated with [ResultFlow]
//
// Built-in convention:
//   Bind → TransformWithRisk (mint green)  — can fail
//   Map  → PureTransform     (mint green)  — always succeeds
//
// The generator uses method name matching — no library-specific knowledge.
// Bind and Map are pre-configured; custom methods can be added via resultflow.json.
// =============================================================================
static class Pipelines
{
    // ─── 1. Risk chain ──────────────────────────────────────────────────────
    //
    // Type Travel:
    //   FindUser(userId)       → Result<User>
    //   .Bind(FindProduct)     → Result<Product>   label: "Bind<br/>User → Product"
    //   .Bind(BuildOrder)      → Result<Order>     label: "Bind<br/>Product → Order"
    /*
```mermaid
flowchart LR
    N0_Bind["Bind<br/>User → Product"]:::transform
    N0_Bind -->|ok| N1_Bind
    N0_Bind -->|fail| F0["Failure"]:::failure
    N1_Bind["Bind<br/>Product → Order"]:::transform
    N1_Bind -->|fail| F1["Failure"]:::failure
    classDef transform fill:#e3f0e8,color:#2f7a5c
    classDef failure fill:#f8e3e3,color:#b13e3e
```*/
    [ResultFlow]
    public static Result<Order> PlaceOrder(int userId, int productId) =>
        FindUser(userId)
            .Bind(_ => FindProduct(productId))
            .Bind(p  => BuildOrder(userId, p));

    // ─── 2. Full chain ──────────────────────────────────────────────────────
    //
    // Type Travel:
    //   FindUser(userId)       → Result<User>
    //   .Bind(FindProduct)     → Result<Product>   label: "Bind<br/>User → Product"
    //   .Bind(BuildOrder)      → Result<Order>     label: "Bind<br/>Product → Order"
    //   .Map(FormatOrder)      → Result<string>    label: "Map<br/>Order → String"
    /*
```mermaid
flowchart LR
    N0_Bind["Bind<br/>User → Product"]:::transform
    N0_Bind -->|ok| N1_Bind
    N0_Bind -->|fail| F0["Failure"]:::failure
    N1_Bind["Bind<br/>Product → Order"]:::transform
    N1_Bind -->|ok| N2_Map
    N1_Bind -->|fail| F1["Failure"]:::failure
    N2_Map["Map<br/>Order → String"]:::transform
    classDef transform fill:#e3f0e8,color:#2f7a5c
    classDef failure fill:#f8e3e3,color:#b13e3e
```*/
    [ResultFlow]
    public static Result<string> ProcessCheckout(int userId, int productId) =>
        FindUser(userId)
            .Bind(_ => FindProduct(productId))
            .Bind(p  => BuildOrder(userId, p))
            .Map(o   => $"Order #{o.Id} confirmed — {o.Amount:C}");

    // ─── 3. Guard chain — inline validation via Bind ─────────────────────────
    //
    // FluentResults has no Ensure method; guards are expressed as Bind with
    // an inline condition. The diagram shows each guard as a Bind node
    // (TransformWithRisk) — honest: a failed guard produces a failure result.
    //
    // Type Travel: Result<Product> throughout — no type change shown.
    /*
```mermaid
flowchart LR
    N0_Bind["Bind<br/>User → Product"]:::transform
    N0_Bind -->|ok| N1_Bind
    N0_Bind -->|fail| F0["Failure"]:::failure
    N1_Bind["Bind<br/>Product"]:::transform
    N1_Bind -->|ok| N2_Bind
    N1_Bind -->|fail| F1["Failure"]:::failure
    N2_Bind["Bind<br/>Product → Order"]:::transform
    N2_Bind -->|fail| F2["Failure"]:::failure
    classDef transform fill:#e3f0e8,color:#2f7a5c
    classDef failure fill:#f8e3e3,color:#b13e3e
```*/
    [ResultFlow]
    public static Result<Order> ValidateAndPlace(int userId, int productId, decimal maxAmount) =>
        FindUser(userId)
            .Bind(_ => FindProduct(productId))
            .Bind(p  => p.Stock > 0
                ? Result.Ok(p)
                : Result.Fail<Product>($"'{p.Name}' is out of stock"))
            .Bind(p  => p.Price <= maxAmount
                ? BuildOrder(userId, p)
                : Result.Fail<Order>($"Price {p.Price:C} exceeds limit {maxAmount:C}"));

    // ─── Data + helpers ──────────────────────────────────────────────────────

    private static readonly Dictionary<int, User> _users = new()
    {
        [42] = new User(42, "alice@example.com"),
        [7]  = new User(7,  "bob@example.com")
    };

    private static readonly Dictionary<int, Product> _products = new()
    {
        [7] = new Product(7, "Widget", 29.99m, 100),
        [8] = new Product(8, "Gadget", 49.99m, 0)    // out of stock
    };

    private static Result<User> FindUser(int id) =>
        _users.TryGetValue(id, out var u)
            ? Result.Ok(u)
            : Result.Fail<User>($"User {id} not found");

    private static Result<Product> FindProduct(int id) =>
        _products.TryGetValue(id, out var p)
            ? Result.Ok(p)
            : Result.Fail<Product>($"Product {id} not found");

    private static Result<Order> BuildOrder(int userId, Product p) =>
        Result.Ok(new Order(new Random().Next(1000, 9999), userId, p.Price));
}
