---
hide:
  - navigation
title: ASP.NET
description: Auto-generate Minimal API + MVC endpoints. SmartEndpoints handles routing, DI, validation, and IResult mapping. Zero wiring code.
tagline: Write the logic. We generate the API.
---

# ASP.NET Integration

Plug REslava.Result into your web stack – Minimal API, MVC, and everything in between.

<div class="grid cards" markdown>

-   :material-lightbulb-on: __SmartEndpoints__
    Zero‑boilerplate Minimal API generation — routes, HTTP verbs, OpenAPI, auth, filters, caching.
    [](smartendpoints/)
    {: .is-featured }

-   :material-swap-horizontal: __OneOf to IResult__
    Convert `OneOf<T1,...>` to HTTP responses in one call — status codes from error types.
    [](smartendpoints/oneof-to-iresult)

-   :material-code-json: __Minimal API__
    `Result.ToIResult()`, HTTP method variants (`ToPostResult`, `ToPutResult`, `ToDeleteResult`).
    [](resulttoiresult-extensions)

-   :material-view-dashboard: __MVC — Result__
    `Result.ToActionResult()` – convention‑based MVC controllers.
    [](resulttoactionresult-extensions-mvc-support--v1.21.0)

-   :material-view-dashboard-variant: __MVC — OneOf__
    `OneOf.ToActionResult()` – multi-case MVC responses.
    [](oneoftoactionresult-extensions-mvc-oneof-support--v1.22.0)

-   :material-map-marker-path: __Smart HTTP Mapping__
    Automatic HTTP status code selection from error types.
    [](smart-http-mapping)

-   :material-alert-box: __Problem Details__
    RFC 7807 compliant error responses via `[MapToProblemDetails]`.
    [](problem-details-integration)

-   :material-cloud-download: __HTTP Client__
    Wrap `HttpClient` calls so every response and network failure becomes a typed `Result<T>`.
    [](http-client--reslava.result.http)

</div>