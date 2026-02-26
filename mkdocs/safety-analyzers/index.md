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

!!! warning "-   :material-alert: __RESL1001 — Unsafe .Value__"
        `[Warning + Code Fix]` Detects `.Value` access without an `IsSuccess` guard.
        [](analyzers/#resl1001-unsafe-value-access)


!!! warning "-   :material-trash-can-outline: __RESL1002 — Discarded Result__"
        `[Warning]` Warns when a `Result<T>` return value is silently ignored.
        [](analyzers/#resl1002-discarded-result-return-value)


!!! warning "-   :material-swap-horizontal: __RESL1003 — Prefer Match__"
        `[Info]` Suggests `.Match()` over manual `if`/`else` when both branches use the result.
        [](analyzers/#resl1003-prefer-match-over-if-check)


!!! warning "-   :material-timer-off: __RESL1004 — Async Not Awaited__"
        `[Warning + Code Fix]` Detects `Task<Result<T>>` assigned without `await`.
        [](analyzers/#resl1004-taskresultt-not-awaited)


!!! warning "-   :material-lightbulb-on: __RESL1005 — Suggest Domain Error__"
        `[Info]` Suggests domain-specific error types over generic `new Error("...")`.
        [](analyzers/#resl1005-suggest-domain-error)


!!! warning "-   :material-shield-off: __RESL2001 — Unsafe OneOf.AsT*__"
        `[Warning + Code Fix]` Detects `.AsT1/.AsT2` access without an `.IsT1/.IsT2` guard.
        [](analyzers/#resl2001-unsafe-oneofast-access)


</div>