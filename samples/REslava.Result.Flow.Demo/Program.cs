// =============================================================================
// REslava.Result.Flow — Pipeline Visualization with Type Travel + Typed Errors
//
// REslava.Result.Flow is the REslava.Result-NATIVE source generator.
// Unlike the library-agnostic REslava.ResultFlow, it uses IResultBase and IError
// as Roslyn anchors to produce richer diagrams:
//
//   ✦ Type Travel   — every node label shows the success type flowing through it
//                     "Bind<br/>User → Product"  (type changed)
//                     "Ensure<br/>Product"        (type unchanged)
//
//   ✦ Typed Errors  — failure edges are labelled with the actual IError subtype
//                     "-->|UserNotFoundError| FAIL"   (vs generic "-->|fail| FAIL")
//
// Attribute namespace: REslava.Result.Flow
// Generated accessor:  Generated.ResultFlow.{ClassName}_Flows.{MethodName}
//
// Sections:
//   1. Single-type guard chain — Ensure × 3 with typed errors
//   2. Risk chain — Bind × 2, type stays Order
//   3. Multi-hop type travel — User → Product → Order → string
//   4. Async pipeline — ⚡ markers + type travel + typed errors
//   5. Full pipeline — all node kinds, type travel end-to-end
//   6. Cross-method tracing — [ResultFlow(MaxDepth = 2)] expands Bind into subgraph
//   7. Sidecar constant — writes PlaceOrderCross.ResultFlow.md to disk
//   8. Clickable nodes — ResultFlowLinkMode = vscode
//   9. Domain boundary diagrams — [DomainBoundary] triggers _LayerView, _Stats,
//      _ErrorSurface, _ErrorPropagation alongside the existing _Diagram constant
//  10. Match multi-branch fan-out — hexagon shape + typed N-branch FAIL edges
//  11. InventoryService.CheckStock — cross-method Infrastructure → Domain layer view
//  12. InventoryService.ReserveStock — cross-method with Ensure gate before subgraph
//  13. Dark theme — [ResultFlow(Theme = ResultFlowTheme.Dark)] on WarehouseService + InventoryService.ReserveStock
// =============================================================================
using Generated.ResultFlow;
using REslava.Result;
using REslava.Result.AdvancedPatterns;
using REslava.Result.Extensions;
using REslava.Result.Flow;

var sep  = new string('─', 60);
var sep2 = new string('═', 60);

Console.WriteLine(sep2);
Console.WriteLine("  REslava.Result.Flow — Feature Demo");
Console.WriteLine(sep2);

void Print(string label, string diagram)
{
    Console.WriteLine();
    Console.WriteLine($"  {label}");
    Console.WriteLine(sep);
    Console.WriteLine(diagram);
}

Print("1. Guard chain — Ensure × 3 + typed errors",                Pipelines_Flows.ValidateOrder);
Print("2. Risk chain — Bind × 2, typed errors",                    Pipelines_Flows.PlaceOrder);
Print("3. Type travel — User → Product → Order → string",          Pipelines_Flows.ProcessCheckout);
Print("4. Async pipeline — ⚡ labels + typed errors",              Pipelines_Flows.PlaceOrderAsync);
Print("5. Full pipeline — all node kinds",                          Pipelines_Flows.AdminCheckout);
Print("6. Cross-method tracing — MaxDepth = 2, subgraph expanded", OrderService_Flows.PlaceOrderCross);

// ── 7. Sidecar constant ───────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("  7. Sidecar constant — written to PlaceOrderCross.ResultFlow.md");
Console.WriteLine(sep);
File.WriteAllText("PlaceOrderCross.ResultFlow.md", OrderService_Flows.PlaceOrderCross_Sidecar);
Console.WriteLine("     ↳ Wrote PlaceOrderCross.ResultFlow.md");
Console.WriteLine("       Open in VS Code (Ctrl+Shift+V) to preview the diagram.");

// ── 8. Clickable nodes ────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("  8. Clickable nodes — ResultFlowLinkMode = vscode");
Console.WriteLine(sep);
Console.WriteLine("     Add to .csproj:");
Console.WriteLine("       <ResultFlowLinkMode>vscode</ResultFlowLinkMode>");
Console.WriteLine("     Each node becomes:");
Console.WriteLine("       click N0_FindUser \"vscode://file/{path}:{line}\" \"Go to FindUser\"");
Console.WriteLine("     Click in VS Code Mermaid preview → jumps directly to that source line.");

// ── 9. Domain boundary diagrams ───────────────────────────────────────────────
//
// [DomainBoundary("Application")] on OrderService class  → rootLayer = "Application"
// [DomainBoundary("Domain")]      on UserService class   → ValidateUser subgraph Layer = "Domain"
//
// Because PlaceOrderCross has MaxDepth = 2, the generator traces into UserService.ValidateUser
// and detects two distinct layers → triggers all four additional constants:
//
//   _LayerView         — flowchart TD, Layer → Class → Method subgraphs, colored containers
//   _Stats             — markdown table: steps, async steps, errors, layers, depth
//   _ErrorSurface      — fail-edges-only filtered view of the full cross-method chain
//   _ErrorPropagation  — flowchart TD, errors grouped by the layer they originate from
//
Console.WriteLine();
Console.WriteLine("  9. Domain boundary diagrams — [DomainBoundary] + layer-aware constants");
Console.WriteLine(sep2);
Print("9a. _LayerView        — architecture (Application → Domain)", OrderService_Flows.PlaceOrderCross_LayerView);
Print("9b. _Stats            — pipeline statistics",                 OrderService_Flows.PlaceOrderCross_Stats);
Print("9c. _ErrorSurface     — fail-edges only",                     OrderService_Flows.PlaceOrderCross_ErrorSurface);
Print("9d. _ErrorPropagation — errors grouped by layer",             OrderService_Flows.PlaceOrderCross_ErrorPropagation);

// ── 10. Match multi-branch fan-out ────────────────────────────────────────────
//
// ConfirmOrder ends with .Match(3 explicitly-typed lambdas).
// REslava.Result.Flow extracts each lambda's explicit parameter type annotation
// and emits one typed -->|TypeName| FAIL edge per fail branch:
//
//   N0_BuildOrder["BuildOrder<br/>..."]:::operation
//   N1_Match{{"Match"}}:::terminal
//   N1_Match -->|ok| SUCCESS
//   N1_Match -->|UserNotFoundError| FAIL
//   N1_Match -->|ProductNotFoundError| FAIL
//
Print("10. Match — hexagon + typed N-branch fan-out (v1.46.0)", MatchDemo_Flows.ConfirmOrder);

// ── 11. InventoryService — cross-method Infrastructure → Domain ───────────────
Console.WriteLine();
Console.WriteLine("  11. InventoryService — cross-method Infrastructure → Domain layer view");
Console.WriteLine(sep2);
Print("11a. CheckStock — pipeline (Infrastructure → Domain)",            InventoryService_Flows.CheckStock);
Print("11b. CheckStock _LayerView  — architecture (Infrastructure → Domain)", InventoryService_Flows.CheckStock_LayerView);
Print("11c. CheckStock _ErrorSurface  — fail-edges only",                InventoryService_Flows.CheckStock_ErrorSurface);
Print("11d. CheckStock _ErrorPropagation — errors by layer",             InventoryService_Flows.CheckStock_ErrorPropagation);

// ── 12. InventoryService.ReserveStock — Ensure gate before subgraph ────────────
Console.WriteLine();
Console.WriteLine("  12. ReserveStock — Ensure gate + cross-method subgraph");
Console.WriteLine(sep2);
Print("12a. ReserveStock — pipeline (Ensure → subgraph → Map)", InventoryService_Flows.ReserveStock);
Print("12b. ReserveStock _LayerView  — architecture layers",     InventoryService_Flows.ReserveStock_LayerView);

// ── 13. Dark theme ─────────────────────────────────────────────────────────────
//
// [ResultFlow(Theme = ResultFlowTheme.Dark)] emits a Mermaid diagram with the dark
// classDef palette (matched to the MkDocs slate colour scheme) instead of the default
// pastel light palette.  The diagram structure is identical — only node fill/text
// colours and arrow stroke colour change.
//
// FulfillmentService [DomainBoundary("Application")] calls WarehouseService [DomainBoundary("Domain")].
// Because both classes have [DomainBoundary], the generator emits dark-themed
// _LayerView, _ErrorSurface and _ErrorPropagation constants alongside the pipeline diagram.
Console.WriteLine();
Console.WriteLine("  13. Dark theme — dark classDef palette + dark linkStyle");
Console.WriteLine(sep2);
Print("13a. WarehouseService.ReserveStock — dark (flat pipeline)",                  WarehouseService_Flows.ReserveStock);
Print("13b. FulfillmentService.FulfillOrder — dark cross-method pipeline",           FulfillmentService_Flows.FulfillOrder);
Print("13c. FulfillOrder _LayerView  — dark Application → Domain architecture",     FulfillmentService_Flows.FulfillOrder_LayerView);
Print("13d. FulfillOrder _ErrorSurface  — dark fail-edges only",                    FulfillmentService_Flows.FulfillOrder_ErrorSurface);
Print("13e. FulfillOrder _ErrorPropagation — dark errors grouped by layer",         FulfillmentService_Flows.FulfillOrder_ErrorPropagation);

Console.WriteLine();
Console.WriteLine(sep2);
Console.WriteLine("  Runtime verification");
Console.WriteLine(sep2);

void Run(string label, object result)
{
    var ok = result.GetType().GetProperty("IsSuccess")?.GetValue(result) is true;
    Console.WriteLine($"  {label}: {(ok ? "✓ OK" : "✗ FAIL")}");
}

using var cts = new CancellationTokenSource();
Run("ValidateOrder (valid)                    ", Pipelines.ValidateOrder(new Order(1, 42, 199.99m)));
Run("ValidateOrder (amount ≤ 0)               ", Pipelines.ValidateOrder(new Order(2, 42, 0m)));
Run("ValidateOrder (wrong role)               ", Pipelines.ValidateOrder(new Order(3, 7, 50m)));
Run("PlaceOrder (success)                     ", Pipelines.PlaceOrder(42, 7));
Run("PlaceOrder (product not found)           ", Pipelines.PlaceOrder(42, 99));
Run("ProcessCheckout (success)                ", Pipelines.ProcessCheckout(42, 7));
Run("ProcessCheckout (user not found)         ", Pipelines.ProcessCheckout(999, 7));
Run("PlaceOrderAsync (success)                ", Pipelines.PlaceOrderAsync(42, 7, cts.Token).GetAwaiter().GetResult());
Run("PlaceOrderAsync (out of stock)           ", Pipelines.PlaceOrderAsync(42, 8, cts.Token).GetAwaiter().GetResult());
Run("AdminCheckout (success)                  ", Pipelines.AdminCheckout(42, 7, cts.Token).GetAwaiter().GetResult());
Run("AdminCheckout (unauthorized role)        ", Pipelines.AdminCheckout(7,  7, cts.Token).GetAwaiter().GetResult());
Console.WriteLine();
Console.WriteLine("  Cross-method tracing:");
Run("PlaceOrderCross (success)                ", OrderService.PlaceOrderCross(42, 7));
Run("PlaceOrderCross (user not found)         ", OrderService.PlaceOrderCross(999, 7));
Run("PlaceOrderCross (user inactive)          ", OrderService.PlaceOrderCross(8, 7));
Run("PlaceOrderCross (unauthorized role)      ", OrderService.PlaceOrderCross(7, 7));
Run("PlaceOrderCross (product not found)      ", OrderService.PlaceOrderCross(42, 99));

Console.WriteLine();
Console.WriteLine("  Match multi-branch:");
Console.WriteLine($"  ConfirmOrder (success)                  : {MatchDemo.ConfirmOrder(42, 7)}");
Console.WriteLine($"  ConfirmOrder (user not found)           : {MatchDemo.ConfirmOrder(999, 7)}");
Console.WriteLine($"  ConfirmOrder (product not found)        : {MatchDemo.ConfirmOrder(42, 99)}");

Console.WriteLine();
Console.WriteLine("  InventoryService:");
Run("CheckStock (success — 10 units)          ", InventoryService.CheckStock(7, 10));
Run("CheckStock (product not found)           ", InventoryService.CheckStock(99, 10));
Run("CheckStock (insufficient stock)          ", InventoryService.CheckStock(8, 5));
Run("ReserveStock (success)                   ", InventoryService.ReserveStock(7, 10));
Run("ReserveStock (out of stock gate)         ", InventoryService.ReserveStock(8, 5));

Console.WriteLine();
Console.WriteLine("  FulfillmentService (dark theme):");
Run("FulfillOrder (success)                   ", FulfillmentService.FulfillOrder(7, 10));
Run("FulfillOrder (product not found)         ", FulfillmentService.FulfillOrder(99, 10));
Run("FulfillOrder (insufficient stock)        ", FulfillmentService.FulfillOrder(8, 5));

Console.WriteLine();

// =============================================================================
// Domain
// =============================================================================
record User(int Id, string Email, string Role, bool IsActive = true);
record Order(int Id, int UserId, decimal Amount);
record Product(int Id, string Name, decimal Price, int Stock);
record StockReservation(int ProductId, int Quantity, decimal UnitPrice);

// =============================================================================
// Custom domain errors (5) — extend built-in REslava.Result error types.
//
// Because these implement IError, REslava.Result.Flow's body scanner finds them
// inside lambda / method bodies and emits their class name on failure edges:
//   "-->|UserNotFoundError| FAIL"   instead of   "-->|fail| FAIL"
// =============================================================================
sealed class UserNotFoundError(int id)
    : NotFoundError($"User {id} not found") { }

sealed class ProductNotFoundError(int id)
    : NotFoundError($"Product {id} not found") { }

sealed class OutOfStockError(string productName)
    : ValidationError($"stock: '{productName}' is out of stock") { }

sealed class CreditLimitError(decimal amount, decimal limit)
    : ValidationError($"amount: Amount {amount:C} exceeds credit limit {limit:C}") { }

sealed class UnauthorizedRoleError(string role)
    : ForbiddenError($"Role '{role}' is not authorized for this operation") { }

sealed class UserInactiveError(int id)
    : ForbiddenError($"User {id} account is inactive") { }

sealed class InsufficientStockError(string name, int available, int requested)
    : ValidationError($"stock: '{name}' has {available} available, {requested} requested") { }

// =============================================================================
// Pipelines — [ResultFlow] from REslava.Result.Flow
// =============================================================================
static class Pipelines
{
    // ─── 1. Guard chain — Ensure × 3 ────────────────────────────────────────
    //
    // Ensure uses string messages here because ValidationError and its subclasses
    // implement IError directly (not via Error) — the Ensure(pred, Error) overload
    // requires the concrete Error type.
    //
    // Typed error edges appear on Bind nodes where IError subclasses are created
    // inside lambda bodies (FindUser, FindProduct, BuildOrder).
    //
    // Type Travel:  every Ensure keeps Result<Order> → label shows just "Order"
    [ResultFlow]
    public static Result<Order> ValidateOrder(Order order) =>
        Result<Order>.Ok(order)
            .Ensure(o => o.Amount > 0,
                "Amount must be positive")
            .Ensure(o => o.Amount < 5_000,
                $"Amount exceeds credit limit 5,000")
            .Ensure(o => GetUserRole(o.UserId) == "Admin",
                "Admin role required");

    // ─── 2. Risk chain — Bind × 2 with typed errors ─────────────────────────
    //
    // Typed error edges:
    //   UserNotFoundError      (from FindUser body)
    //   ProductNotFoundError   (from FindProduct body)
    //
    // Type Travel:
    //   FindUser(userId)            → Result<User>     (entry node)
    //   .Bind(FindProduct)          → Result<Product>  label: "Bind<br/>User → Product"
    //   .Bind(u,p → BuildOrder)     → Result<Order>    label: "Bind<br/>Product → Order"
    [ResultFlow]
    public static Result<Order> PlaceOrder(int userId, int productId) =>
        FindUser(userId)
            .Bind(_ => FindProduct(productId))
            .Bind(p  => BuildOrder(userId, p));

    // ─── 3. Multi-hop type travel — User → Product → Order → string ─────────
    //
    // Every Bind changes the success type — visible in each node label:
    //   FindUser(userId)      → Result<User>     (entry)
    //   .Bind(FindProduct)    → Result<Product>  "Bind<br/>User → Product"
    //   .Bind(BuildOrder)     → Result<Order>    "Bind<br/>Product → Order"
    //   .Map(FormatOrder)     → Result<string>   "Map<br/>Order → string"
    //
    // Typed errors appear on the two Bind edges:
    //   ProductNotFoundError, then no additional error on Map (pure transform)
    [ResultFlow]
    public static Result<string> ProcessCheckout(int userId, int productId) =>
        FindUser(userId)
            .Bind(_ => FindProduct(productId))
            .Bind(p  => BuildOrder(userId, p))
            .Map(o   => $"Order #{o.Id} confirmed — {o.Amount:C}");

    // ─── 4. Async pipeline — ⚡ labels + typed errors ─────────────────────
    //
    // Async nodes are marked with ⚡ in the label.
    // Typed error edges still resolve because the scanner follows method groups
    // and async lambdas into their bodies.
    //
    // Type Travel:
    //   FindUserAsync   → Result<User>
    //   BindAsync       → Result<Product>   "BindAsync⚡<br/>User → Product"
    //   EnsureAsync     → Result<Product>   "EnsureAsync⚡<br/>Product"
    //   MapAsync        → Result<Order>     "MapAsync⚡<br/>Product → Order"
    //   BindAsync       → Result<Order>     "BindAsync⚡<br/>Order"
    [ResultFlow]
    public static async Task<Result<Order>> PlaceOrderAsync(
        int userId, int productId, CancellationToken ct) =>
        await FindUserAsync(userId, ct)
            .BindAsync(_  => FindProductAsync(productId, ct), ct)
            .EnsureAsync(p => p.Stock > 0,
                $"'{_products.GetValueOrDefault(productId)?.Name ?? "unknown"}' is out of stock", ct)
            .MapAsync(p   => new Order(0, userId, p.Price), ct)
            .BindAsync(o  => SaveOrderAsync(o, ct), ct);

    // ─── 5. Full pipeline — all node kinds, type travel end-to-end ──────────
    //
    // Shows every node kind in one pipeline:
    //   FindUserAsync  (entry)
    //   EnsureAsync    Gatekeeper    — UnauthorizedRoleError
    //   BindAsync      TransformWithRisk — ProductNotFoundError  User → Product
    //   EnsureAsync    Gatekeeper    — OutOfStockError
    //   MapAsync       PureTransform                             Product → Order
    //   BindAsync      TransformWithRisk                         Order → Order
    //   TapAsync       SideEffectSuccess
    //   TapOnFailure   SideEffectFailure
    //   MapAsync       PureTransform                             Order → string
    [ResultFlow]
    public static async Task<Result<string>> AdminCheckout(
        int userId, int productId, CancellationToken ct) =>
        await FindUserAsync(userId, ct)
            .EnsureAsync(u => u.Role == "Admin",
                $"Role '{FindUserRole(userId)}' is not authorized", ct)
            .BindAsync(_  => FindProductAsync(productId, ct), ct)
            .EnsureAsync(p => p.Stock > 0,
                $"'{_products.GetValueOrDefault(productId)?.Name ?? "unknown"}' is out of stock", ct)
            .MapAsync(p   => new Order(0, userId, p.Price), ct)
            .BindAsync(o  => SaveOrderAsync(o, ct), ct)
            .TapAsync(o   => Log($"Order #{o.Id} placed — {o.Amount:C}"), ct)
            .TapOnFailureAsync(e => Log($"Checkout failed: {e.Message}"), ct)
            .MapAsync(o   => $"Confirmed: Order #{o.Id} — {o.Amount:C}", ct);

    // ─── Data + helpers ──────────────────────────────────────────────────────

    private static readonly Dictionary<int, User> _users = new()
    {
        [42] = new User(42, "alice@example.com", "Admin", IsActive: true),
        [7]  = new User(7,  "bob@example.com",   "User",  IsActive: true),
        [8]  = new User(8,  "carol@example.com", "User",  IsActive: false)  // inactive
    };

    private static readonly Dictionary<int, Product> _products = new()
    {
        [7] = new Product(7, "Widget",  29.99m, 100),
        [8] = new Product(8, "Gadget",  49.99m, 0)    // out of stock
    };

    private static string GetUserRole(int userId) =>
        _users.TryGetValue(userId, out var u) ? u.Role : "Unknown";

    private static string FindUserRole(int userId) =>
        _users.TryGetValue(userId, out var u) ? u.Role : "Unknown";

    private static Result<User> FindUser(int id) =>
        _users.TryGetValue(id, out var u)
            ? Result<User>.Ok(u)
            : Result<User>.Fail(new UserNotFoundError(id));

    private static Result<Product> FindProduct(int id) =>
        _products.TryGetValue(id, out var p)
            ? Result<Product>.Ok(p)
            : Result<Product>.Fail(new ProductNotFoundError(id));

    private static Result<Order> BuildOrder(int userId, Product p) =>
        p.Stock > 0
            ? Result<Order>.Ok(new Order(0, userId, p.Price))
            : Result<Order>.Fail(new OutOfStockError(p.Name));

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
        return FindProduct(id);
    }

    private static async Task<Result<Order>> SaveOrderAsync(Order order, CancellationToken ct)
    {
        await Task.Delay(5, ct);
        return SaveOrder(order);
    }

    private static void Log(string msg) => Console.WriteLine($"    [LOG] {msg}");
}

// =============================================================================
// Section 6 — Cross-method pipeline tracing (cross-class)
//
// [ResultFlow(MaxDepth = 2)] on OrderService.PlaceOrderCross causes the generator
// to follow the lambda body `.Bind(u => UserService.ValidateUser(u))` into
// UserService.ValidateUser and expand its chain as a Mermaid subgraph:
//
//   N0_FindUser -->|ok| sg_N1_ValidateUser
//
//   subgraph sg_N1_ValidateUser["ValidateUser"]
//       N1_ValidateUser_0_Bind["Bind"]:::transform
//       N1_ValidateUser_0_Bind -->|ok| N1_ValidateUser_1_Bind
//       N1_ValidateUser_0_Bind -->|"UserInactiveError"| FAIL
//       N1_ValidateUser_1_Bind["Bind"]:::transform
//       N1_ValidateUser_1_Bind -->|"UnauthorizedRoleError"| FAIL
//   end
//
//   sg_N1_ValidateUser -->|ok| N2_Bind
//
// MaxDepth controls recursion depth. Default is 2. Set MaxDepth = 0 to disable.
// =============================================================================

// [DomainBoundary("Domain")] on the class tags all methods as Domain layer.
// The generator reads this via class-level annotation (priority: method > class > namespace heuristic).
[DomainBoundary("Domain")]
static class UserService
{
    // Bind is used (instead of Ensure + string) so the source generator's error scanner
    // detects UserInactiveError / UnauthorizedRoleError in the lambda body and emits them
    // on the _ErrorPropagation diagram under the Domain layer subgraph.
    public static Result<User> ValidateUser(User u) =>
        Result<User>.Ok(u)
            .Bind(x => x.IsActive
                ? Result<User>.Ok(x)
                : Result<User>.Fail(new UserInactiveError(x.Id)))
            .Bind(x => x.Role == "Admin"
                ? Result<User>.Ok(x)
                : Result<User>.Fail(new UnauthorizedRoleError(x.Role)));
}

// [DomainBoundary("Application")] on the class tags PlaceOrderCross (and any other methods) as Application layer.
[DomainBoundary("Application")]
static class OrderService
{
    private static readonly Dictionary<int, User> _users = new()
    {
        [42] = new User(42, "alice@example.com", "Admin", IsActive: true),
        [7]  = new User(7,  "bob@example.com",   "User",  IsActive: true),
        [8]  = new User(8,  "carol@example.com", "User",  IsActive: false)   // inactive
    };

    private static readonly Dictionary<int, Product> _products = new()
    {
        [7] = new Product(7, "Widget", 29.99m, 100),
    };

    private static Result<User> FindUser(int id) =>
        _users.TryGetValue(id, out var u)
            ? Result<User>.Ok(u)
            : Result<User>.Fail(new UserNotFoundError(id));

    private static Result<Product> FindProduct(int id) =>
        _products.TryGetValue(id, out var p)
            ? Result<Product>.Ok(p)
            : Result<Product>.Fail(new ProductNotFoundError(id));

    // ── Cross-method tracing entry point ─────────────────────────────────────

    //
    // [ResultFlow(MaxDepth = 2)] follows the lambda into UserService.ValidateUser
    // and stitches its Ensure chain as a Mermaid subgraph connected with -->|ok|.
    // Qualified calls (x => SomeClass.Method(x)) are now supported.
    //
    // Sidecar constant: OrderService_Flows.PlaceOrderCross_Sidecar
    [ResultFlow(MaxDepth = 2)]
    public static Result<Order> PlaceOrderCross(int userId, int productId) =>
        FindUser(userId)
            .Bind(u => UserService.ValidateUser(u))
            .Bind(_ => FindProduct(productId))
            .Map(p  => new Order(0, userId, p.Price));

}

// =============================================================================
// Section 10 — Match multi-branch fan-out (v1.46.0)
//
// Demonstrates the typed N-branch Match rendering:
//   - Match renders as a Mermaid hexagon {{"Match"}}:::terminal
//   - One -->|TypeName| FAIL edge per explicitly-typed fail-branch lambda
//   - Type names extracted from lambda parameter annotations (not body scanning)
//
// Diagram produced by ConfirmOrder:
//
//   N0_BuildOrder["BuildOrder<br/>..."]:::operation
//   N1_Match{{"Match"}}:::terminal
//   N1_Match -->|ok| SUCCESS([success]):::success
//   N1_Match -->|UserNotFoundError| FAIL([fail]):::failure
//   N1_Match -->|ProductNotFoundError| FAIL
//
// For plain Result<T> Match with 2 args, a generic -->|fail| FAIL is emitted instead.
// =============================================================================
static class MatchDemo
{
    private static Result<User> LookupUser(int id) =>
        _users.TryGetValue(id, out var u)
            ? Result<User>.Ok(u)
            : Result<User>.Fail(new UserNotFoundError(id));

    private static Result<Product> LookupProduct(int id) =>
        _products.TryGetValue(id, out var p)
            ? Result<Product>.Ok(p)
            : Result<Product>.Fail(new ProductNotFoundError(id));

    private static Result<Order> BuildOrder(int userId, int productId) =>
        LookupUser(userId)
            .Bind(_ => LookupProduct(productId))
            .Map(p   => new Order(0, userId, p.Price));

    // [ResultFlow] method — Match is the terminal node (hexagon, v1.46.0).
    // Renders as {{"Match"}}:::terminal with explicit -->|ok| SUCCESS and -->|fail| FAIL.
    // For typed N-branch fan-out (-->|UserNotFoundError| FAIL etc.), the generator
    // reads explicit lambda parameter type annotations — available when a multi-arg
    // Match overload for Result<T, ErrorsOf<T1..Tn>> is used.
    [ResultFlow]
    public static string ConfirmOrder(int userId, int productId) =>
        BuildOrder(userId, productId).Match(
            onSuccess: (Order o)               => $"✓ Order #{o.Id} — {o.Amount:C}",
            onFailure: errors                  => $"✗ {errors[0].Message}");

    private static readonly Dictionary<int, User> _users = new()
    {
        [42] = new User(42, "alice@example.com", "Admin"),
        [7]  = new User(7,  "bob@example.com",   "User"),
    };

    private static readonly Dictionary<int, Product> _products = new()
    {
        [7] = new Product(7, "Widget", 29.99m, 100),
    };
}

// =============================================================================
// Section 11 & 12 — InventoryService + WarehouseService
//
// Two distinct layers introduce a second cross-method scenario independent of
// the OrderService / UserService pairing:
//
//   WarehouseService   [DomainBoundary("Domain")]        — pure domain logic
//   InventoryService   [DomainBoundary("Infrastructure")] — data access + orchestration
//
// CheckStock (§11):
//   FindProduct → WarehouseService.ReserveStock (subgraph) → Map
//   _LayerView shows Infrastructure root calling into Domain subgraph
//
// ReserveStock (§12):
//   FindProduct → Ensure (gate, no subgraph) → WarehouseService.ReserveStock (subgraph) → Map
//   Same layers but an extra Gatekeeper before the subgraph — different visual shape
// =============================================================================

[DomainBoundary("Domain")]
static class WarehouseService
{
    // Pure domain check: returns the product with updated stock count, or fails.
    // Uses .Bind() (fluent API) so the generator can trace into this method when
    // InventoryService.CheckStock/ReserveStock call it with MaxDepth = 2.
    // [ResultFlow(Theme = ResultFlowTheme.Dark)] — showcases dark classDef palette.
    [ResultFlow(Theme = ResultFlowTheme.Dark)]
    public static Result<Product> ReserveStock(Product p, int quantity) =>
        Result<Product>.Ok(p)
            .Bind(x => x.Stock >= quantity
                ? Result<Product>.Ok(x with { Stock = x.Stock - quantity })
                : Result<Product>.Fail(new InsufficientStockError(x.Name, x.Stock, quantity)));
}

[DomainBoundary("Infrastructure")]
static class InventoryService
{
    private static readonly Dictionary<int, Product> _products = new()
    {
        [7]  = new Product(7,  "Widget", 29.99m, 100),
        [8]  = new Product(8,  "Gadget", 49.99m, 0),    // out of stock
        [10] = new Product(10, "Gizmo",  19.99m, 3),
    };

    private static Result<Product> FindProduct(int id) =>
        _products.TryGetValue(id, out var p)
            ? Result<Product>.Ok(p)
            : Result<Product>.Fail(new ProductNotFoundError(id));

    // ── §11: CheckStock — FindProduct → subgraph → Map ───────────────────────
    //
    // Simplest cross-method shape: entry Bind, one subgraph expansion (Domain),
    // one pure Map. Generates _LayerView (Infrastructure → Domain), _ErrorSurface,
    // _ErrorPropagation in addition to the plain pipeline diagram.
    [ResultFlow(MaxDepth = 2)]
    public static Result<StockReservation> CheckStock(int productId, int quantity) =>
        FindProduct(productId)
            .Bind(p => WarehouseService.ReserveStock(p, quantity))
            .Map(p  => new StockReservation(p.Id, quantity, p.Price));

    // ── §12: ReserveStock — FindProduct → Ensure gate → subgraph → Map ───────
    //
    // Adds a Gatekeeper node before the cross-method subgraph expansion, producing
    // a visually distinct pipeline: the Domain subgraph is preceded by a blue gate.
    [ResultFlow(MaxDepth = 2)]
    public static Result<StockReservation> ReserveStock(int productId, int quantity) =>
        FindProduct(productId)
            .Ensure(p => p.Stock > 0,
                $"Product {productId} is out of stock")
            .Bind(p => WarehouseService.ReserveStock(p, quantity))
            .Map(p  => new StockReservation(p.Id, quantity, p.Price));

}

// =============================================================================
// Section 13 — FulfillmentService (dark theme + cross-method + DomainBoundary)
//
// [DomainBoundary("Application")] on FulfillmentService + [DomainBoundary("Domain")]
// on WarehouseService → two distinct layers → generates dark-themed _LayerView,
// _ErrorSurface, _ErrorPropagation constants alongside the dark pipeline diagram.
//
// Layer0 (Application) and Layer1 (Domain) use alternating depth-indexed colors
// from ResultFlowThemes.Dark (Layer0_Style / Layer1_Style).
// =============================================================================

[DomainBoundary("Application")]
static class FulfillmentService
{
    private static readonly Dictionary<int, Product> _catalog = new()
    {
        [7]  = new Product(7,  "Widget", 29.99m, 100),
        [8]  = new Product(8,  "Gadget", 49.99m, 0),
    };

    private static Result<Product> FindProduct(int id) =>
        _catalog.TryGetValue(id, out var p)
            ? Result<Product>.Ok(p)
            : Result<Product>.Fail(new ProductNotFoundError(id));

    // ── §13: FulfillOrder — Application → Domain cross-method, dark theme ────
    //
    // Calls WarehouseService.ReserveStock (Domain layer) via Bind.
    // [ResultFlow(MaxDepth = 2, Theme = Dark)] traces into WarehouseService and
    // emits dark-palette constants including dark _LayerView and _ErrorPropagation.
    //
    // CodeLens UX (REslava.Result Extensions — VS Code Marketplace):
    //   ▶ Open diagram preview    ← always visible above this method; click to open rendered diagram
    /*
```mermaid
---
title: FulfillOrder → ⟨StockReservation⟩
---
%%{init: {'theme': 'base', 'flowchart': {'scale': 1}}}%%
flowchart LR
    ENTRY_ROOT["FindProduct<br/>→ Product"]:::operation ==> sg_N0_ReserveStock
    subgraph sg_N0_ReserveStock["ReserveStock"]
        ENTRY_N0_ReserveStock[ ]:::entry
        ENTRY_N0_ReserveStock[ ] ==> N0_ReserveStock_0_Ok
        N0_ReserveStock_0_Ok["Ok<br/>Product"]:::operation
        N0_ReserveStock_0_Ok --> N0_ReserveStock_1_Bind
        N0_ReserveStock_1_Bind["Bind<br/>Product"]:::bind
        N0_ReserveStock_1_Bind -->|InsufficientStockError| FAIL
    end
    sg_N0_ReserveStock -->|ok| N1_Map
    sg_N0_ReserveStock -->|fail| FAIL
    N1_Map["Map<br/>Product → StockReservation"]:::map
    N1_Map -->|ok| SUCCESS
    SUCCESS([success]):::success
    FAIL([fail])
    FAIL:::failure
classDef entry      fill:none,stroke:none
classDef operation  fill:#faf0e3,color:#b45f2b
classDef bind       fill:#e3f0e8,color:#2f7a5c,stroke:#1a5c3c,stroke-width:3px
classDef map        fill:#e3f0e8,color:#2f7a5c
classDef transform  fill:#e3f0e8,color:#2f7a5c
classDef gatekeeper fill:#e3e9fa,color:#3f5c9a
classDef sideeffect fill:#fff4d9,color:#b8882c
classDef terminal   fill:#f2e3f5,color:#8a4f9e
classDef success    fill:#e8f4f0,color:#1c7e6f
classDef failure    fill:#f8e3e3,color:#b13e3e
classDef note       fill:#f5f5f5,color:#555555,stroke:#cccccc
classDef subgraphStyle fill:#ffffde,stroke:#aa3,stroke-width:1px
linkStyle default stroke:#888,stroke-width:1.5px
classDef Layer0_Style fill:#eff4ff,color:#2b4c7e,stroke:#c0d0f0,stroke-width:1px
classDef Layer1_Style fill:#f0f8f0,color:#1e6f43,stroke:#b8d8c0,stroke-width:1px
classDef Layer2_Style fill:#eff4ff,color:#2b4c7e,stroke:#c0d0f0,stroke-width:1px
classDef Layer3_Style fill:#f0f8f0,color:#1e6f43,stroke:#b8d8c0,stroke-width:1px
classDef Layer4_Style fill:#eff4ff,color:#2b4c7e,stroke:#c0d0f0,stroke-width:1px
class sg_N0_ReserveStock subgraphStyle
%% --- Node correlation (ReasonMetadata.NodeId / PipelineStep) ---
%%   N0_ReserveStock → ReserveStock
%%   N1_Map → Map
```*/
    [ResultFlow(MaxDepth = 2, Theme = ResultFlowTheme.Dark)]
    public static Result<StockReservation> FulfillOrder(int productId, int quantity) =>
        FindProduct(productId)
            .Bind(p => WarehouseService.ReserveStock(p, quantity))
            .Map(p  => new StockReservation(p.Id, quantity, p.Price));
}
