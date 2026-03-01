---
hide:
  - navigation
title: Advanced Types
description: Maybe<T> for null safety and OneOf discriminated unions for exhaustive multi-case matching.
tagline: Null dies here. Exceptions retire.
---

# Advanced Types

Two powerful companions to `Result<T>` for modelling optional values and multi-outcome returns.

<div class="grid cards" markdown>

-   :material-null: __Maybe&lt;T&gt; — Operations__
    Safe null handling with optionals — no null reference exceptions.
    [](operations)

-   :material-help-circle: __When to Use Maybe&lt;T&gt;__
    Guidance on choosing between `Maybe<T>`, `Result<T>`, and nullable.
    [](when-to-use-maybet)

-   :simple-oneplus: __OneOf Unions__
    Discriminated unions for 2–6 typed outcomes with exhaustive matching.
    [](oneof-unions)

-   :material-swap-horizontal: __OneOf Arities & Conversions__
    Arity 2–6, chain conversions, and mapping to `Result<T>` or `IResult`.
    [](arities-2-3-4-5-6)

-   :material-arrow-decision: __Convert to `Result<T>` or `IResult`__
    `ToResult()`, `ToIResult()` — bridge OneOf to the Result pipeline or HTTP responses.
    [](convert-to-resultt-or-iresult)

-   :material-help-rhombus: __When to Use `OneOf<...>` vs `Result<T>`__
    Decision guide — when each type fits best in your domain model.
    [](when-to-use-oneof...-vs-resultt)

</div>