---
title: Architectural Flow Catalog
description: Live pipeline and architecture diagrams auto-generated from the REslava.Result.Flow.Demo project — see exactly what ResultFlow produces for real application code.
---

# 🗺️ Architectural Flow Catalog

> **See ResultFlow in action on real code.** Every diagram on this page is auto-generated from the [`REslava.Result.Flow.Demo`](https://github.com/reslava/nuget-package-reslava-result/tree/main/samples/REslava.Result.Flow.Demo) project — the same output you get when you annotate your own methods with `[ResultFlow]`.

Each method shows its full set of generated views: pipeline flow, architecture layer view, stats, error surface, and error propagation — all derived automatically from the source code with zero manual work.

!!! info
    This page is regenerated automatically on every release. Do not edit manually.

---

## MatchDemo

### ConfirmOrder

#### Pipeline

*Success path, typed error edges, async steps*

```mermaid
---
title: ConfirmOrder → ⟨⟩
---
flowchart LR
    ENTRY_ROOT["BuildOrder<br/>→ Order"]:::operation ==> N0_Match
    N0_Match{{"Match"}}:::terminal
    N0_Match -->|ok| SUCCESS
    SUCCESS([success]):::success
    N0_Match -->|fail| FAIL
    FAIL([fail])
    FAIL:::failure
    classDef operation fill:#fef0e3,color:#b86a1c
    classDef success fill:#e6f6ea,color:#1c7e4f
    classDef terminal fill:#f2e3f5,color:#8a4f9e
    classDef failure fill:#f8e3e3,color:#b13e3e
%% --- Node correlation (ReasonMetadata.NodeId / PipelineStep) ---
%%   N0_Match → Match
```

---

## OrderService

### PlaceOrderCross

#### Pipeline

*Success path, typed error edges, async steps*

```mermaid
---
title: PlaceOrderCross → ⟨Order⟩
---
flowchart LR
    ENTRY_ROOT["FindUser<br/>→ User"]:::operation ==> sg_N0_ValidateUser
    subgraph sg_N0_ValidateUser["ValidateUser"]
        ENTRY_N0_ValidateUser[ ]:::entry
        ENTRY_N0_ValidateUser[ ] ==> N0_ValidateUser_0_Ok
        N0_ValidateUser_0_Ok["Ok<br/>User"]:::operation
        N0_ValidateUser_0_Ok --> N0_ValidateUser_1_Bind
        N0_ValidateUser_1_Bind["Bind<br/>User"]:::bind
        N0_ValidateUser_1_Bind -->|ok| N0_ValidateUser_2_Bind
        N0_ValidateUser_1_Bind -->|UserInactiveError| FAIL
        N0_ValidateUser_2_Bind["Bind<br/>User"]:::bind
        N0_ValidateUser_2_Bind -->|UnauthorizedRoleError| FAIL
    end
    sg_N0_ValidateUser -->|ok| N1_FindProduct
    sg_N0_ValidateUser -->|fail| FAIL
    N1_FindProduct["FindProduct<br/>User → Product"]:::bind
    N1_FindProduct -->|ok| N2_Map
    N1_FindProduct -->|fail| FAIL
    N2_Map["Map<br/>Product → Order"]:::map
    N2_Map -->|ok| SUCCESS
    SUCCESS([success]):::success
    FAIL([fail])
    FAIL:::failure
    classDef operation fill:#fef0e3,color:#b86a1c
    classDef entry fill:none,stroke:none
    classDef bind fill:#e3f0e8,color:#2f7a5c,stroke:#1a5c3c,stroke-width:3px
    classDef transform fill:#e3f0e8,color:#2f7a5c
    classDef map fill:#e3f0e8,color:#2f7a5c
    classDef success fill:#e6f6ea,color:#1c7e4f
    classDef failure fill:#f8e3e3,color:#b13e3e
%% --- Node correlation (ReasonMetadata.NodeId / PipelineStep) ---
%%   N0_ValidateUser → ValidateUser
%%   N1_FindProduct → FindProduct
%%   N2_Map → Map
```

#### Layer View

*Architecture layers — Domain / Application / Infrastructure boundaries*

```mermaid
flowchart TD

  subgraph Application["Application"]
    subgraph OrderService["OrderService"]
      N_PlaceOrderCross["PlaceOrderCross"]:::layerApp
    end
  end

  subgraph Domain["Domain"]
    subgraph UserService["UserService"]
      N_ValidateUser["ValidateUser"]:::layerDomain
    end
  end

  N_PlaceOrderCross -->|"User / UserInactiveError, UnauthorizedRoleError"| N_ValidateUser
  N_ValidateUser -->|"UserInactiveError"| FAIL
  N_ValidateUser -->|"UnauthorizedRoleError"| FAIL
  N_ValidateUser -->|ok| SUCCESS

  FAIL([fail]):::failure
  SUCCESS([success]):::success

  classDef layerApp fill:#e8f7ee,color:#1e6f43
  classDef layerDomain fill:#fff6e5,color:#a36b00
  classDef failure fill:#f8e3e3,color:#b13e3e
  classDef success fill:#e6f6ea,color:#1c7e4f

  class Application layerApp
  class Domain layerDomain
```

#### Stats

*Node count, error count, depth, async steps*

| Property        | Value                                    |
|-----------------|------------------------------------------|
| Steps           | 6                                        |
| Async steps     | 0                                        |
| Possible errors | UserInactiveError, UnauthorizedRoleError |
| Layers crossed  | Domain                                   |
| Max depth traced | 1                                        |

#### Error Surface

*All possible errors grouped by the step that produces them*

```mermaid
flowchart LR
  N0_ValidateUser["ValidateUser"] -->|"fail"| FAIL
  N1_Bind["Bind"] -->|"UserInactiveError"| FAIL
  N2_Bind["Bind"] -->|"UnauthorizedRoleError"| FAIL
  N3_FindProduct["FindProduct"] -->|"fail"| FAIL

  FAIL([fail]):::failure

  classDef failure fill:#f8e3e3,color:#b13e3e
```

#### Error Propagation

*Error types grouped by the architectural layer they originate from*

```mermaid
flowchart TD

  subgraph Domain["Domain"]
    E0["UserInactiveError"]:::failure
    E1["UnauthorizedRoleError"]:::failure
  end

  E0 --> FAIL([fail]):::failure
  E1 --> FAIL([fail]):::failure

  classDef layerDomain fill:#fff6e5,color:#a36b00
  classDef failure fill:#f8e3e3,color:#b13e3e

  class Domain layerDomain
```

---

## Pipelines

### ValidateOrder

#### Pipeline

*Success path, typed error edges, async steps*

```mermaid
---
title: ValidateOrder → ⟨Order⟩
---
flowchart LR
    N0_Ok["Ok<br/>Order"]:::operation
    N0_Ok --> N1_Ensure
    N1_Ensure["<span title='o.Amount > 0'>Ensure<br/>Order</span>"]:::gatekeeper
    N1_Ensure -->|pass| N2_Ensure
    N1_Ensure -->|fail| FAIL
    N2_Ensure["<span title='o.Amount < 5_000'>Ensure<br/>Order</span>"]:::gatekeeper
    N2_Ensure -->|pass| N3_Ensure
    N2_Ensure -->|fail| FAIL
    N3_Ensure["<span title='GetUserRole(o.UserId) == &quot;Admin&quot;'>Ensure<br/>Order</span>"]:::gatekeeper
    N3_Ensure -->|fail| FAIL
    N3_Ensure -->|ok| SUCCESS
    SUCCESS([success]):::success
    FAIL([fail])
    FAIL:::failure
    classDef operation fill:#fef0e3,color:#b86a1c
    classDef gatekeeper fill:#e3e9fa,color:#3f5c9a
    classDef success fill:#e6f6ea,color:#1c7e4f
    classDef failure fill:#f8e3e3,color:#b13e3e
%% --- Node correlation (ReasonMetadata.NodeId / PipelineStep) ---
%%   N0_Ok → Ok
%%   N1_Ensure → Ensure
%%   N2_Ensure → Ensure
%%   N3_Ensure → Ensure
```

---

### PlaceOrder

#### Pipeline

*Success path, typed error edges, async steps*

```mermaid
---
title: PlaceOrder → ⟨Order⟩
---
flowchart LR
    ENTRY_ROOT["FindUser<br/>→ User"]:::operation ==> N0_FindProduct
    N0_FindProduct["FindProduct<br/>User → Product"]:::bind
    N0_FindProduct -->|ok| N1_BuildOrder
    N0_FindProduct -->|fail| FAIL
    N1_BuildOrder["BuildOrder<br/>Product → Order"]:::bind
    N1_BuildOrder -->|fail| FAIL
    N1_BuildOrder -->|ok| SUCCESS
    SUCCESS([success]):::success
    FAIL([fail])
    FAIL:::failure
    classDef operation fill:#fef0e3,color:#b86a1c
    classDef bind fill:#e3f0e8,color:#2f7a5c,stroke:#1a5c3c,stroke-width:3px
    classDef success fill:#e6f6ea,color:#1c7e4f
    classDef failure fill:#f8e3e3,color:#b13e3e
%% --- Node correlation (ReasonMetadata.NodeId / PipelineStep) ---
%%   N0_FindProduct → FindProduct
%%   N1_BuildOrder → BuildOrder
```

---

### ProcessCheckout

#### Pipeline

*Success path, typed error edges, async steps*

```mermaid
---
title: ProcessCheckout → ⟨String⟩
---
flowchart LR
    ENTRY_ROOT["FindUser<br/>→ User"]:::operation ==> N0_FindProduct
    N0_FindProduct["FindProduct<br/>User → Product"]:::bind
    N0_FindProduct -->|ok| N1_BuildOrder
    N0_FindProduct -->|fail| FAIL
    N1_BuildOrder["BuildOrder<br/>Product → Order"]:::bind
    N1_BuildOrder -->|ok| N2_Map
    N1_BuildOrder -->|fail| FAIL
    N2_Map["Map<br/>Order → String"]:::map
    N2_Map -->|ok| SUCCESS
    SUCCESS([success]):::success
    FAIL([fail])
    FAIL:::failure
    classDef operation fill:#fef0e3,color:#b86a1c
    classDef bind fill:#e3f0e8,color:#2f7a5c,stroke:#1a5c3c,stroke-width:3px
    classDef map fill:#e3f0e8,color:#2f7a5c
    classDef success fill:#e6f6ea,color:#1c7e4f
    classDef failure fill:#f8e3e3,color:#b13e3e
%% --- Node correlation (ReasonMetadata.NodeId / PipelineStep) ---
%%   N0_FindProduct → FindProduct
%%   N1_BuildOrder → BuildOrder
%%   N2_Map → Map
```

---

### PlaceOrderAsync

#### Pipeline

*Success path, typed error edges, async steps*

```mermaid
---
title: PlaceOrder⚡ → ⟨Order⟩
---
flowchart LR
    ENTRY_ROOT["FindUser⚡<br/>→ User"]:::operation ==> N0_FindProductAsync
    N0_FindProductAsync["FindProduct⚡<br/>User → Product"]:::bind
    N0_FindProductAsync -->|ok| N1_EnsureAsync
    N0_FindProductAsync -->|fail| FAIL
    N1_EnsureAsync["<span title='p.Stock > 0'>Ensure⚡<br/>Product</span>"]:::gatekeeper
    N1_EnsureAsync -->|pass| N2_MapAsync
    N1_EnsureAsync -->|fail| FAIL
    N2_MapAsync["Map⚡<br/>Product → Order"]:::map
    N2_MapAsync --> N3_SaveOrderAsync
    N3_SaveOrderAsync["SaveOrder⚡<br/>Order"]:::bind
    N3_SaveOrderAsync -->|fail| FAIL
    N3_SaveOrderAsync -->|ok| SUCCESS
    SUCCESS([success]):::success
    FAIL([fail])
    FAIL:::failure
    classDef operation fill:#fef0e3,color:#b86a1c
    classDef bind fill:#e3f0e8,color:#2f7a5c,stroke:#1a5c3c,stroke-width:3px
    classDef gatekeeper fill:#e3e9fa,color:#3f5c9a
    classDef map fill:#e3f0e8,color:#2f7a5c
    classDef success fill:#e6f6ea,color:#1c7e4f
    classDef failure fill:#f8e3e3,color:#b13e3e
%% --- Node correlation (ReasonMetadata.NodeId / PipelineStep) ---
%%   N0_FindProductAsync → FindProductAsync
%%   N1_EnsureAsync → EnsureAsync
%%   N2_MapAsync → MapAsync
%%   N3_SaveOrderAsync → SaveOrderAsync
```

---

### AdminCheckout

#### Pipeline

*Success path, typed error edges, async steps*

```mermaid
---
title: AdminCheckout → ⟨String⟩
---
flowchart LR
    ENTRY_ROOT["FindUser⚡<br/>→ User"]:::operation ==> N0_EnsureAsync
    N0_EnsureAsync["<span title='u.Role == &quot;Admin&quot;'>Ensure⚡<br/>User</span>"]:::gatekeeper
    N0_EnsureAsync -->|pass| N1_FindProductAsync
    N0_EnsureAsync -->|fail| FAIL
    N1_FindProductAsync["FindProduct⚡<br/>User → Product"]:::bind
    N1_FindProductAsync -->|ok| N2_EnsureAsync
    N1_FindProductAsync -->|fail| FAIL
    N2_EnsureAsync["<span title='p.Stock > 0'>Ensure⚡<br/>Product</span>"]:::gatekeeper
    N2_EnsureAsync -->|pass| N3_MapAsync
    N2_EnsureAsync -->|fail| FAIL
    N3_MapAsync["Map⚡<br/>Product → Order"]:::map
    N3_MapAsync --> N4_SaveOrderAsync
    N4_SaveOrderAsync["SaveOrder⚡<br/>Order"]:::bind
    N4_SaveOrderAsync -->|ok| N5_Log
    N4_SaveOrderAsync -->|fail| FAIL
    N5_Log["Log<br/>Order"]:::sideeffect
    N5_Log --> N6_Log
    N6_Log["Log<br/>Order"]:::sideeffect
    N6_Log --> N7_MapAsync
    N7_MapAsync["Map⚡<br/>Order → String"]:::map
    N7_MapAsync -->|ok| SUCCESS
    SUCCESS([success]):::success
    FAIL([fail])
    FAIL:::failure
    classDef operation fill:#fef0e3,color:#b86a1c
    classDef gatekeeper fill:#e3e9fa,color:#3f5c9a
    classDef bind fill:#e3f0e8,color:#2f7a5c,stroke:#1a5c3c,stroke-width:3px
    classDef map fill:#e3f0e8,color:#2f7a5c
    classDef sideeffect fill:#fff4d9,color:#b8882c
    classDef success fill:#e6f6ea,color:#1c7e4f
    classDef failure fill:#f8e3e3,color:#b13e3e
%% --- Node correlation (ReasonMetadata.NodeId / PipelineStep) ---
%%   N0_EnsureAsync → EnsureAsync
%%   N1_FindProductAsync → FindProductAsync
%%   N2_EnsureAsync → EnsureAsync
%%   N3_MapAsync → MapAsync
%%   N4_SaveOrderAsync → SaveOrderAsync
%%   N5_Log → Log
%%   N6_Log → Log
%%   N7_MapAsync → MapAsync
```

---
