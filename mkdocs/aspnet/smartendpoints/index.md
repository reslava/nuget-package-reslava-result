---
hide:
  - navigation
title: SmartEndpoints
description: Zero-boilerplate Minimal API generation — routes, HTTP verbs, OpenAPI, auth, filters, caching, FluentValidation, and CancellationToken threading.
tagline: Write the logic. We generate the API.
---

# SmartEndpoints

Auto-generate Minimal API endpoints. Routes, HTTP verbs, DI, validation, and `IResult` mapping — zero wiring code.

<div class="grid cards" markdown>

-   :material-rocket-launch: __Setup & Routing__
    `[SmartEndpoint]` basics, route inference, HTTP method conventions, and service registration.
    [](setup)

-   :material-shield-lock: __Features__
    Authorization, endpoint filters, output caching, rate limiting, CancellationToken, and FluentValidation bridge.
    [](features)

-   :material-cog: __Advanced Usage__
    Complex scenarios, custom configuration, and advanced integration patterns.
    [](advanced-usage)

-   :material-swap-horizontal: __OneOf to IResult__
    Convert `OneOf<T1,...>` to HTTP responses — status codes from error types.
    [](oneof-to-iresult)

</div>