---
title: Dark Theme Flow Catalog
description: Dark-themed pipeline and architecture diagrams from the REslava.Result.Flow.Demo project — showcasing [ResultFlow(Theme = ResultFlowTheme.Dark)] output.
force-dark-mode: true
---

# 🌙 Dark Theme Flow Catalog

> **Same diagrams, dark palette.** Annotate any `[ResultFlow]` method with `Theme = ResultFlowTheme.Dark` to emit the full diagram set using a dark colour scheme — optimised for dark-mode editors, MkDocs slate, and presentation slides.

```csharp
[ResultFlow(MaxDepth = 2, Theme = ResultFlowTheme.Dark)]
public static Result<StockReservation> FulfillOrder(int productId, int quantity) => ...
```

Each method with `Theme = Dark` generates the same set of constants as the light theme — pipeline, layer view, error surface, and error propagation — all using the dark `classDef` palette.

!!! info
    This page is regenerated automatically on every release. Do not edit manually.

---

## FulfillmentService

### FulfillOrder

#### Pipeline

*Cross-method pipeline — Application → Domain, dark palette*

```mermaid
---
title: FulfillOrder → ⟨StockReservation⟩
---
%%{init: {'theme': 'base', 'flowchart': {'scale': 1}, 'themeVariables': {'primaryTextColor': '#fff', 'titleColor': '#fff', 'edgeLabelBackground': '#2a2a2a'}}}%%
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
classDef operation  fill:#3a2b1f,color:#f2c2a0
classDef bind       fill:#1f3a2d,color:#9fe0c0,stroke:#2d5a4a,stroke-width:3px
classDef map        fill:#1f3a2d,color:#9fe0c0
classDef transform  fill:#1f3a2d,color:#9fe0c0
classDef gatekeeper fill:#1f263a,color:#b8c8f2
classDef sideeffect fill:#3a331f,color:#f2e0a0
classDef terminal   fill:#33203a,color:#d6b0f2
classDef success    fill:#1f3a36,color:#9fe0d6
classDef failure    fill:#3a1f1f,color:#f2b8b8
classDef note       fill:#2a2a2a,color:#aaaaaa,stroke:#555555
classDef subgraphStyle fill:#252520,stroke:#665,stroke-width:1px
linkStyle default stroke:#666,stroke-width:1.5px
class sg_N0_ReserveStock subgraphStyle
```

#### Layer View

*Domain boundary — Application (FulfillmentService) → Domain (WarehouseService)*

```mermaid
%%{init: {'theme': 'base', 'flowchart': {'scale': 1}}}%%
flowchart TD

  subgraph Layer0["Application"]
    subgraph L0_FulfillmentService["FulfillmentService"]
      N_FulfillOrder["FulfillOrder"]:::operation
    end
  end

  subgraph Layer1["Domain"]
    subgraph L1_WarehouseService["WarehouseService"]
      N_ReserveStock["ReserveStock"]:::bind
    end
  end

  N_FulfillOrder -->|"Product / InsufficientStockError"| N_ReserveStock
  N_ReserveStock -->|"InsufficientStockError"| FAIL
  N_ReserveStock -->|ok| SUCCESS

  FAIL([fail]):::failure
  SUCCESS([success]):::success

classDef entry      fill:none,stroke:none
classDef operation  fill:#3a2b1f,color:#f2c2a0
classDef bind       fill:#1f3a2d,color:#9fe0c0,stroke:#2d5a4a,stroke-width:3px
classDef map        fill:#1f3a2d,color:#9fe0c0
classDef success    fill:#1f3a36,color:#9fe0d6
classDef failure    fill:#3a1f1f,color:#f2b8b8
linkStyle default stroke:#666,stroke-width:1.5px
classDef Layer0_Style fill:#1a2535,color:#8aa8d0,stroke:#2a3a55,stroke-width:1px
classDef Layer1_Style fill:#1a2a20,color:#70b890,stroke:#2a3a30,stroke-width:1px
  class Layer0 Layer0_Style
  class Layer1 Layer1_Style
```

#### Error Surface

*Fail-edges only — dark palette*

```mermaid
%%{init: {'theme': 'base', 'flowchart': {'scale': 1}}}%%
flowchart LR
  N0_ReserveStock["ReserveStock"] -->|"fail"| FAIL
  N1_Bind["Bind"] -->|"InsufficientStockError"| FAIL

  FAIL([fail]):::failure

  classDef failure fill:#3a1f1f,color:#f2b8b8
  linkStyle default stroke:#666,stroke-width:1.5px
```

#### Error Propagation

*Error types grouped by the layer they originate from — dark palette*

```mermaid
%%{init: {'theme': 'base', 'flowchart': {'scale': 1}}}%%
flowchart TD

  subgraph Layer0["Domain"]
    E0["InsufficientStockError"]:::failure
  end

  E0 --> FAIL([fail]):::failure

classDef failure    fill:#3a1f1f,color:#f2b8b8
linkStyle default stroke:#666,stroke-width:1.5px
classDef Layer0_Style fill:#1a2535,color:#8aa8d0,stroke:#2a3a55,stroke-width:1px
  class Layer0 Layer0_Style
```

---
