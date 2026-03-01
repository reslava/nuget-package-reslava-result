---
hide:
  - navigation
title: Code Safety
description: 6 Roslyn analyzers catch unsafe .Value access, missing await, and conflicting attributes. 3 code-fix providers. Ship safer code.
tagline: Your bugs don't survive the build.
---

# Safety Analyzers

Catch `Result<T>` and `OneOf` mistakes **at compile time** — 6 diagnostics and 3 code fixes.
Install once, protect your whole codebase.

```bash
dotnet add package REslava.Result.Analyzers
```

<div class="grid cards" markdown>

-   :material-alert: __RESL1001 — Unsafe .Value__
    `[Warning + Code Fix]` Detects `.Value` access without an `IsSuccess` guard.
    [](resl1001--unsafe-.value-access-warning--code-fix)

-   :material-trash-can-outline: __RESL1002 — Discarded Result__
    `[Warning]` Warns when a `Result<T>` return value is silently ignored.
    [](resl1002--discarded-resultt-return-value-warning)

-   :material-swap-horizontal: __RESL1003 — Prefer Match__
    `[Info]` Suggests `.Match()` over manual `if`/`else` when both branches use the result.
    [](resl1003--prefer-match-over-if-check-info)

-   :material-timer-off: __RESL1004 — Async Not Awaited__
    `[Warning + Code Fix]` Detects `Task<Result<T>>` assigned without `await`.
    [](resl1004--taskresultt-not-awaited-warning--code-fix)

-   :material-lightbulb-on: __RESL1005 — Suggest Domain Error__
    `[Info]` Suggests domain-specific error types over generic `new Error("...")`.
    [](resl1005--consider-domain-error-info)

-   :material-shield-alert: __RESL1006 — Conflicting Validate Attributes__
    `[Error]` Detects conflicting `[Validate]` and `[FluentValidate]` on the same type.
    [](resl1006--conflicting-validate--fluentvalidate-error)

-   :material-shield-off: __RESL2001 — Unsafe OneOf.AsT*__
    `[Warning + Code Fix]` Detects `.AsT1/.AsT2` access without an `.IsT1/.IsT2` guard.
    [](resl2001--unsafe-oneof.ast-access-warning--code-fix)

</div>