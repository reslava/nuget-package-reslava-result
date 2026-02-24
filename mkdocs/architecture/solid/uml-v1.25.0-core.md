---
title: REslava.Result — Core Type Hierarchy (v1.25.0)
---

##### Legend
<span style="font-size: 9px;">

- blue: interfaces
- orange: abstract classes
- green: concrete classes
- purple: struct types
</span>


```mermaid
---
  config:
    class:
      hideEmptyMembersBox: true
---
classDiagram
    class IReason:::interface
    class ISuccess:::interface
    class IError:::interface

    class Reason:::abstract
    class Reason_TReason["Reason&lt;TReason&gt;"]:::abstract

    class Success:::concrete
    class Error:::concrete
    class ExceptionError:::concrete
    class ConversionError:::concrete
    class NotFoundError:::concrete
    class ConflictError:::concrete
    class UnauthorizedError:::concrete
    class ForbiddenError:::concrete
    class ValidationError:::concrete {
        +string? FieldName
    }

    IReason <|.. ISuccess : extends
    IReason <|.. IError : extends

    IReason <|.. Reason : implements
    Reason <|-- Reason_TReason : extends

    Reason_TReason <|-- Success : extends~Success~
    Reason_TReason <|-- Error : extends~Error~
    Reason_TReason <|-- ExceptionError : extends~ExceptionError~
    Reason_TReason <|-- ConversionError : extends~ConversionError~
    Reason_TReason <|-- NotFoundError : extends~NotFoundError~
    Reason_TReason <|-- ValidationError : extends~ValidationError~
    Reason_TReason <|-- ConflictError : extends~ConflictError~
    Reason_TReason <|-- UnauthorizedError : extends~UnauthorizedError~
    Reason_TReason <|-- ForbiddenError : extends~ForbiddenError~

    Success ..|> ISuccess : implements
    Error ..|> IError : implements
    ExceptionError ..|> IError : implements
    ConversionError ..|> IError : implements
    NotFoundError ..|> IError : implements
    ValidationError ..|> IError : implements
    ConflictError ..|> IError : implements
    UnauthorizedError ..|> IError : implements
    ForbiddenError ..|> IError : implements

    classDef interface fill:#e1f5ff,stroke:#0288d1,stroke-width:2px,color:black
    classDef abstract fill:#fff3e0,stroke:#f57c00,stroke-width:2px,color:black
    classDef concrete fill:#d4edda,stroke:#388e3c,stroke-width:2px,color:black
```


```mermaid
---
  config:
    class:
      hideEmptyMembersBox: true
---
classDiagram
    class IResultResponse:::interface
    class IResultResponse_TValue["IResultResponse&lt;TValue&gt;"]:::interface
    class Result:::concrete
    class Result_TValue["Result&lt;TValue&gt;"]:::concrete
    class IReason:::interface
    class TValue:::type

    IResultResponse <|-- IResultResponse_TValue : extends

    IResultResponse <|.. Result : implements
    Result <|-- Result_TValue : extends
    Result_TValue ..|> IResultResponse_TValue : implements

    Result o-- IReason : contains reasons
    Result_TValue o-- TValue : contains value

    classDef interface fill:#e1f5ff,stroke:#0288d1,stroke-width:2px,color:black
    classDef concrete fill:#d4edda,stroke:#388e3c,stroke-width:2px,color:black
    classDef type fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px,color:black
```


```mermaid
---
  config:
    class:
      hideEmptyMembersBox: true
---
classDiagram
    class Maybe_T["Maybe&lt;T&gt;"]:::type {
        +bool HasValue
        +T Value
        +static Some(T value)
        +static None()
    }

    class OneOf_T1_T2["OneOf&lt;T1, T2&gt;"]:::type {
        +bool IsT1
        +bool IsT2
        +Match()
    }

    class OneOf_T1_T2_T3["OneOf&lt;T1, T2, T3&gt;"]:::type {
        +bool IsT1
        +bool IsT2
        +bool IsT3
        +Match()
    }

    class OneOf_T1_T2_T3_T4["OneOf&lt;T1, T2, T3, T4&gt;"]:::type {
        +bool IsT1
        +bool IsT2
        +bool IsT3
        +bool IsT4
        +Match()
    }

    class IEquatable:::interface

    Maybe_T ..|> IEquatable : implements
    OneOf_T1_T2 ..|> IEquatable : implements
    OneOf_T1_T2_T3 ..|> IEquatable : implements
    OneOf_T1_T2_T3_T4 ..|> IEquatable : implements

    classDef interface fill:#e1f5ff,stroke:#0288d1,stroke-width:2px,color:black
    classDef type fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px,color:black
```

## Notes

- `Reason<TReason>` uses the **Curiously Recurring Template Pattern (CRTP)** for type-safe reason hierarchies
- All domain errors (`NotFoundError`, `ValidationError`, `ConflictError`, `UnauthorizedError`, `ForbiddenError`) inherit directly from `Reason<TReason>` — not from `Error` — keeping the CRTP chain type-safe
- Each domain error sets default `Tags` at construction time: `ErrorType` + `HttpStatusCode` (e.g. 404, 422, 409, 401, 403)
- `ValidationError` adds an optional `FieldName` property, surfaced by the `[Validate]` source generator (v1.24.0)
- `Maybe<T>`, `OneOf<T1,T2>`, `OneOf<T1,T2,T3>`, and `OneOf<T1,T2,T3,T4>` are **readonly structs** — zero heap allocations
- `Result` and `Result<T>` are **classes** — immutable with `ImmutableList<IReason>` for reasons collection
- `IResultResponse<out TValue>` is **covariant** in TValue for polymorphic result handling
