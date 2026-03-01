---
hide:
  - navigation
title: Validation
description: Declarative validation rules, DataAnnotations source generator, and FluentValidation bridge — all returning Result<T>.
tagline: Validate early. Return results, not exceptions.
---

# Validation

Three complementary approaches to validation — pick what fits your team.

<div class="grid cards" markdown>

-   :material-play-circle: __Basic Usage__
    First validator, `Validate()`, and collecting errors into `Result<T>`.
    [](basic-usage)

-   :material-format-list-checks: __All Failures Collected__
    Collect every validation error — not just the first — into a single `Result<T>`.
    [](all-failures-collected)

-   :material-pipe: __Pipeline Composition__
    Chain validators and compose validation pipelines.
    [](pipeline-composition)

-   :material-cog: __Custom Validators__
    Build reusable, domain-specific validators.
    [](custom-validators)

-   :material-check-all: __Native Validation DSL__
    Declarative rule DSL — `NotEmpty`, `MaxLength`, `EmailAddress`, `Range`, and more.
    [](native-validation-dsl)

-   :material-tag-check: __Validation Attributes__
    `[Validate]` source generator — DataAnnotations → `Result<T>` automatically.
    [](validation-attributes)

-   ![FluentValidation](https://avatars.githubusercontent.com/u/61082709?s=16&v=4) __FluentValidation Bridge__
    Keep existing FluentValidation validators, get `Result<T>` + SmartEndpoints auto-injection.
    [](../../aspnet/smartendpoints/advanced-usage#1232-fluentvalidation-bridge)

</div>