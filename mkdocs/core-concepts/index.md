---
hide:
  - navigation
title: Fundamentals
description: Learn Result<T> for success/failure, OneOf for multi-case returns, and Maybe<T> for null safety. Async, LINQ, and domain errors included.
tagline: Null dies here. Exceptions retire.
---

# Core Concepts

The functional programming foundation of REslava.Result. Learn each piece step by step.

<div class="grid cards" markdown>

-   :material-checkbox-marked-circle: __Result Pattern__
    `Result<T>`, factory methods, functional composition, async, LINQ, and advanced patterns.
    [](result/)

-   :material-alert: __Error Types__
    Built‑in domain errors (`NotFoundError`, `ValidationError`, `ConflictError`, etc.), custom CRTP errors, and rich tag context.
    [](error-types/)

-   :material-check-all: __Validation__
    Declarative rules, `[Validate]` source generator, and FluentValidation bridge.
    [](validation/)

-   :material-null: __Advanced Types__
    `Maybe<T>` for null safety and `OneOf` discriminated unions for multi-outcome returns.
    [](advanced-types/)

</div>