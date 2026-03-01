---
hide:
  - navigation
title: Error Types
description: Built-in domain errors, custom CRTP errors, error context, and rich tag system. Everything you need to model failures precisely.
tagline: Name your failures. Own your domain.
---

# Error Types

Built‑in domain errors, custom error hierarchies, and rich contextual tagging.

<div class="grid cards" markdown>

-   :material-alert: __Error Hierarchy__
    Overview of all built-in types: `NotFoundError`, `ValidationError`, `ConflictError`, `UnauthorizedError`, `ForbiddenError`, and more.
    [](error-hierarchy)

-   :material-domain: __Domain Errors__
    Semantic domain-specific errors — map naturally to HTTP status codes.
    [](domain-errors)

-   :material-pencil-plus: __Custom Error Types__
    CRTP base classes for defining your own typed domain errors.
    [](custom-error-types)

-   :material-tag-multiple: __Rich Error Context & Tags__
    Attach structured metadata to errors with fluent chaining and the tag system.
    [](rich-error-context--tags--fluent-chaining)

-   :material-check-circle: __Success & Success Reasons__
    `SuccessReason` — attach messages and metadata to successful results.
    [](success--success-reasons)

-   :material-alert-circle-outline: __Generic Errors__
    `Error`, `ExceptionError`, and all built-in generic types — constructors and use cases.
    [](generic-errors)

</div>