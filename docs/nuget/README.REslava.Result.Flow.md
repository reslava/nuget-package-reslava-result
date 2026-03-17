# REslava.Result.Flow

**The recommended pipeline visualization package for REslava.Result projects.** Generates Mermaid diagrams with full type travel, typed error edges from method body scanning, and entry-point detection — all via Roslyn semantic analysis.

> Using a different Result library (ErrorOr, LanguageExt, FluentResults)? Use [`REslava.ResultFlow`](https://www.nuget.org/packages/REslava.ResultFlow) — the library-agnostic alternative instead.

## Installation

```bash
dotnet add package REslava.Result.Flow
```

Requires `REslava.Result` in your project. The `[ResultFlow]` attribute is injected automatically — no extra `using` needed.

## What it does

Add `[ResultFlow]` to any `REslava.Result` fluent method and the generator produces:

- **Success type travel** — inferred from `IResultBase<T>` at each step; type-preserving steps show `"MethodName<br/>T"`; type-changing steps show `"MethodName<br/>T → U"`
- **Error surface** — scans method bodies of `Bind`/`Ensure` delegates for error construction, annotates failure edges with specific error types
- **Async step annotation** — ⚡ appended to any step that resolves via `await`

## Quick Start

```csharp
[ResultFlow]
public async Task<Result<UserDto>> RegisterAsync(RegisterCommand cmd) =>
    await CreateUser(cmd)
        .EnsureAsync(IsEmailValid, new InvalidEmailError())
        .BindAsync(SaveUser)
        .TapAsync(SendWelcomeEmail)
        .MapAsync(ToDto);
```

Use the **IDE code action** (REF002 — "Insert diagram as comment") to inject the diagram directly above the method — no build required:

```csharp
/*
```mermaid
flowchart LR
    N0_EnsureAsync["EnsureAsync ⚡<br/>User"]:::gatekeeper
    N0_EnsureAsync -->|pass| N1_BindAsync
    N0_EnsureAsync -->|fail| FAIL
    ...
```*/
[ResultFlow]
public async Task<Result<UserDto>> RegisterAsync(RegisterCommand cmd) => ...
```

The `` ```mermaid `` fence makes the diagram render inline in VS Code, GitHub, Rider, and other Markdown-aware IDEs.

## Generated Diagram Example

`_LayerView` — architecture diagram across layers, generated from `[DomainBoundary]` annotations:

```mermaid
flowchart TD

  subgraph Application["Application"]
    subgraph OrderService["OrderService"]
      N_PlaceOrder["PlaceOrder"]:::layerApp
    end
  end

  subgraph Domain["Domain"]
    subgraph UserService["UserService"]
      N_ValidateUser["ValidateUser"]:::layerDomain
    end
  end

  subgraph Infrastructure["Infrastructure"]
    subgraph PaymentGateway["PaymentGateway"]
      N_ProcessPayment["ProcessPayment"]:::layerInfra
    end
  end

  N_PlaceOrder -->|"Order / ValidationError"| N_ValidateUser
  N_PlaceOrder -->|"Order"| N_ProcessPayment
  N_ValidateUser -->|"ValidationError"| FAIL
  N_ProcessPayment -->|ok| SUCCESS

  FAIL([fail]):::failure
  SUCCESS([success]):::success

  classDef layerApp    fill:#e8f7ee,color:#1e6f43
  classDef layerDomain fill:#fff6e5,color:#a36b00
  classDef layerInfra  fill:#f4e8ff,color:#6a3fa0
  classDef failure     fill:#f8e3e3,color:#b13e3e
  classDef success     fill:#e6f6ea,color:#1c7e4f

  class Application layerApp
  class Domain layerDomain
  class Infrastructure layerInfra
```

Compared to `REslava.ResultFlow`, this package adds:
- Typed failure edges (e.g. `InvalidEmailError` instead of just `fail`)
- Error surface inference via `IError` — no manual annotation required

## Diagnostics

| ID | Severity | Description |
|---|---|---|
| `REF001` | Info | `[ResultFlow]` could not detect a fluent chain — diagram not generated. |
| `REF002` | Info | Fluent chain detected — use the "Insert diagram as comment" code action to embed the Mermaid diagram above the method. |
| `REF003` | Warning | `resultflow.json` could not be parsed — falling back to built-in conventions. |

## Documentation

Full documentation: [reslava.github.io/nuget-package-reslava-result](https://reslava.github.io/nuget-package-reslava-result/resultflow/)

**MIT License** | Works with any .NET project (netstandard2.0)
