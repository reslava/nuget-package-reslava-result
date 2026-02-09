# REslava.Result — Source Generator Pipeline (v1.12.1)

##### Legend
<span style="font-size: 9px;">

- blue: Roslyn interfaces
- orange: orchestrators
- green: generators (entry points)
- purple: generated output
</span>

## Generator Architecture

```mermaid
---
  config:
    class:
      hideEmptyMembersBox: true
---
classDiagram
    class IIncrementalGenerator:::interface
    class IGeneratorOrchestrator:::interface
    class IAttributeGenerator:::interface
    class ICodeGenerator:::interface

    class ResultToIResultRefactoredGenerator:::generator
    class OneOf2ToIResultGenerator:::generator
    class OneOf3ToIResultGenerator:::generator
    class OneOf4ToIResultGenerator:::generator
    class SmartEndpointsGenerator:::generator

    class ResultToIResultOrchestrator:::orchestrator
    class OneOf2ToIResultOrchestrator:::orchestrator
    class OneOf3ToIResultOrchestrator:::orchestrator
    class OneOf4ToIResultOrchestrator:::orchestrator
    class SmartEndpointsOrchestrator:::orchestrator

    IIncrementalGenerator <|.. ResultToIResultRefactoredGenerator : implements
    IIncrementalGenerator <|.. OneOf2ToIResultGenerator : implements
    IIncrementalGenerator <|.. OneOf3ToIResultGenerator : implements
    IIncrementalGenerator <|.. OneOf4ToIResultGenerator : implements
    IIncrementalGenerator <|.. SmartEndpointsGenerator : implements

    ResultToIResultRefactoredGenerator --> ResultToIResultOrchestrator : delegates to
    OneOf2ToIResultGenerator --> OneOf2ToIResultOrchestrator : delegates to
    OneOf3ToIResultGenerator --> OneOf3ToIResultOrchestrator : delegates to
    OneOf4ToIResultGenerator --> OneOf4ToIResultOrchestrator : delegates to
    SmartEndpointsGenerator --> SmartEndpointsOrchestrator : delegates to

    IGeneratorOrchestrator <|.. ResultToIResultOrchestrator : implements
    IGeneratorOrchestrator <|.. OneOf2ToIResultOrchestrator : implements
    IGeneratorOrchestrator <|.. OneOf3ToIResultOrchestrator : implements
    IGeneratorOrchestrator <|.. OneOf4ToIResultOrchestrator : implements
    IGeneratorOrchestrator <|.. SmartEndpointsOrchestrator : implements

    classDef interface fill:#e1f5ff,stroke:#0288d1,stroke-width:2px,color:black
    classDef orchestrator fill:#fff3e0,stroke:#f57c00,stroke-width:2px,color:black
    classDef generator fill:#d4edda,stroke:#388e3c,stroke-width:2px,color:black
```

## Two-Phase Generation Pipeline

```mermaid
flowchart LR
    subgraph Phase1["Phase 1: RegisterPostInitializationOutput"]
        AG[IAttributeGenerator] --> ATTR["Generated Attributes<br/>(available in same compilation)"]
    end

    subgraph Phase2["Phase 2: RegisterSourceOutput"]
        CG[ICodeGenerator] --> EXT["Generated Extensions<br/>(sees classes using attributes)"]
    end

    Phase1 --> Phase2

    style Phase1 fill:#e1f5ff,stroke:#0288d1,stroke-width:2px
    style Phase2 fill:#d4edda,stroke:#388e3c,stroke-width:2px
```

## Generated Output per Generator

```mermaid
flowchart TB
    subgraph ResultToIResult
        R_ATTR["[GenerateResultToIResult]<br/>attribute"] --> R_EXT["ResultExtensions.ToIResult()<br/>namespace: Generated.ResultExtensions"]
    end

    subgraph OneOf2ToIResult
        O2_ATTR["[GenerateOneOf2ToIResult]<br/>attribute"] --> O2_EXT["OneOf2Extensions.ToIResult()<br/>namespace: Generated.OneOfExtensions"]
    end

    subgraph OneOf3ToIResult
        O3_ATTR["[GenerateOneOf3ToIResult]<br/>attribute"] --> O3_EXT["OneOf3Extensions.ToIResult()<br/>namespace: Generated.OneOfExtensions"]
    end

    subgraph OneOf4ToIResult
        O4_ATTR["[GenerateOneOf4ToIResult]<br/>attribute"] --> O4_EXT["OneOf4Extensions.ToIResult()<br/>namespace: Generated.OneOfExtensions"]
    end

    subgraph SmartEndpoints
        SE_ATTR["[AutoGenerateEndpoints]<br/>attribute"] --> SE_EXT["SmartEndpointExtensions<br/>.MapSmartEndpoints()<br/>namespace: Generated.SmartEndpoints"]
    end

    R_EXT --> IRESULT["ASP.NET Core IResult"]
    O2_EXT --> IRESULT
    O3_EXT --> IRESULT
    O4_EXT --> IRESULT
    SE_EXT --> IRESULT

    style ResultToIResult fill:#e8f5e9,stroke:#388e3c
    style OneOf2ToIResult fill:#e8f5e9,stroke:#388e3c
    style OneOf3ToIResult fill:#e8f5e9,stroke:#388e3c
    style OneOf4ToIResult fill:#e8f5e9,stroke:#388e3c
    style SmartEndpoints fill:#fff3e0,stroke:#f57c00
    style IRESULT fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
```

## Notes

- Each generator follows the same **delegation pattern**: `Generator → Orchestrator → AttributeGenerator + CodeGenerator`
- The two-phase approach ensures generated attributes are available to user code in the same compilation cycle
- SmartEndpoints additionally generates `using Generated.ResultExtensions;` and `using Generated.OneOfExtensions;` in its output, so it depends on the other generators' output
- All orchestrators implement `IGeneratorOrchestrator` following the **Open/Closed Principle** — new generators can be added without modifying existing ones
