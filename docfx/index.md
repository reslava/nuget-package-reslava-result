---
_layout: landing
---

<div class="container-xl py-5">

<!-- ── Hero ─────────────────────────────────────────────────────────────── -->
<div class="pb-4 mb-5 border-bottom">
  <h1 class="display-5 fw-bold">
    REslava.Result
    <span class="text-muted fw-normal fs-4 ms-2">API Reference</span>
  </h1>
  <p class="lead text-muted mb-4">
    Functional error handling &amp; zero-boilerplate Minimal APIs for .NET.<br>
    Type-safe <code>Result&lt;T&gt;</code>, discriminated unions, source generators.
  </p>

  <!-- Quick links -->
  <div class="d-flex flex-wrap gap-2">
    <a href="https://reslava.github.io/nuget-package-reslava-result/reference/"
       class="btn btn-primary">
      ← Back to Docs
    </a>
    <a href="https://github.com/reslava/nuget-package-reslava-result"
       class="btn btn-outline-secondary">
      GitHub
    </a>
    <a href="https://www.nuget.org/packages/REslava.Result/"
       class="btn btn-outline-secondary">
      NuGet: REslava.Result
    </a>
    <a href="https://www.nuget.org/packages/REslava.Result.Flow/"
       class="btn btn-outline-secondary">
      NuGet: Result.Flow
    </a>
    <a href="https://www.nuget.org/packages/REslava.Result.AspNetCore/"
       class="btn btn-outline-secondary">
      NuGet: AspNetCore
    </a>
    <a href="https://www.nuget.org/packages/REslava.Result.Http/"
       class="btn btn-outline-secondary">
      NuGet: Http
    </a>
    <a href="https://www.nuget.org/packages/REslava.Result.Analyzers/"
       class="btn btn-outline-secondary">
      NuGet: Analyzers
    </a>        
    <a href="https://www.nuget.org/packages/REslava.ResultFlow/"
       class="btn btn-outline-secondary">
      NuGet: ResultFlow
    </a>
    <a href="https://www.nuget.org/packages/REslava.Result.FluentValidation/"
       class="btn btn-outline-secondary">
      NuGet: FluentValidation
    </a>
  </div>
</div>

<!-- ── Namespaces ────────────────────────────────────────────────────────── -->
<h2 class="mb-1">Namespaces</h2>
<p class="text-muted mb-4">Browse the full type inventory for each namespace.</p>

<div class="row row-cols-1 row-cols-md-2 g-4 mb-5">

  <div class="col">
    <div class="card h-100 border-start border-4 border-primary shadow-sm">
      <div class="card-body">
        <h5 class="card-title">
          <a href="REslava.Result.html" class="text-decoration-none stretched-link">REslava.Result</a>
        </h5>
        <p class="card-text text-muted small">
          Core library. <code>Result&lt;T&gt;</code>, <code>Result&lt;TValue,TError&gt;</code>,
          <code>Maybe&lt;T&gt;</code>, and the full error hierarchy —
          <code>Error</code>, <code>ValidationError</code>, <code>ExceptionError</code>,
          <code>ConversionError</code>, <code>Reason&lt;T&gt;</code>.
        </p>
      </div>
    </div>
  </div>

  <div class="col">
    <div class="card h-100 border-start border-4 border-primary shadow-sm">
      <div class="card-body">
        <h5 class="card-title">
          <a href="REslava.Result.AdvancedPatterns.html" class="text-decoration-none stretched-link">REslava.Result.AdvancedPatterns</a>
        </h5>
        <p class="card-text text-muted small">
          Discriminated unions and typed error pipelines.
          <code>OneOf&lt;T1..T8&gt;</code> (sealed class) ·
          <code>ErrorsOf&lt;T1..T8&gt;</code> (typed error union, implements <code>IError</code>) ·
          <code>OneOfBase&lt;T1..T8&gt;</code> (shared dispatch) ·
          <code>IOneOf&lt;T1..T8&gt;</code> · <code>Maybe&lt;T&gt;</code>.
        </p>
      </div>
    </div>
  </div>

  <div class="col">
    <div class="card h-100 border-start border-4 border-primary shadow-sm">
      <div class="card-body">
        <h5 class="card-title">
          <a href="REslava.Result.Extensions.html" class="text-decoration-none stretched-link">REslava.Result.Extensions</a>
        </h5>
        <p class="card-text text-muted small">
          Functional extension methods for <code>Result&lt;T&gt;</code> and <code>Result&lt;TValue,TError&gt;</code>:
          <code>Map</code>, <code>Bind</code> ×7, <code>Tap</code>, <code>Ensure</code> ×7,
          <code>MapError</code>, <code>WhenAll</code>, <code>Retry</code>, <code>Timeout</code>,
          <code>ToIResult</code>, <code>ToActionResult</code> and async variants.
        </p>
      </div>
    </div>
  </div>

  <div class="col">
    <div class="card h-100 border-start border-4 border-primary shadow-sm">
      <div class="card-body">
        <h5 class="card-title">
          <a href="REslava.Result.Serialization.html" class="text-decoration-none stretched-link">REslava.Result.Serialization</a>
        </h5>
        <p class="card-text text-muted small">
          <code>System.Text.Json</code> converters for <code>Result&lt;T&gt;</code>,
          <code>Maybe&lt;T&gt;</code>, and error types. Enables seamless serialization
          in web APIs and distributed systems.
        </p>
      </div>
    </div>
  </div>

  <div class="col">
    <div class="card h-100 border-start border-4 border-primary shadow-sm">
      <div class="card-body">
        <h5 class="card-title">
          <a href="REslava.Result.Http.html" class="text-decoration-none stretched-link">REslava.Result.Http</a>
        </h5>
        <p class="card-text text-muted small">
          <code>HttpClient</code> extensions that return <code>Result&lt;T&gt;</code>
          instead of throwing. Maps HTTP 4xx/5xx to typed domain errors
          (<code>NotFoundError</code>, <code>UnauthorizedError</code>, …) and
          wraps network failures in <code>ExceptionError</code>.
        </p>
      </div>
    </div>
  </div>

</div>

<!-- ── Core Types at a Glance ────────────────────────────────────────────── -->
<h2 class="mb-1">Core Types at a Glance</h2>
<p class="text-muted mb-4">The most commonly used types across the library.</p>

<div class="row row-cols-1 row-cols-md-3 g-3 mb-5">

  <div class="col">
    <div class="card h-100 bg-body-secondary border-0">
      <div class="card-body">
        <h6 class="card-title fw-semibold"><a href="REslava.Result.Result-1.html" class="text-decoration-none">Result&lt;T&gt;</a></h6>
        <p class="card-text small text-muted">
          Wraps a success value or a list of errors. The primary return type for all
          fallible operations. Use <code>.IsSuccess</code>, <code>.Value</code>, <code>.Errors</code>.
        </p>
      </div>
    </div>
  </div>

  <div class="col">
    <div class="card h-100 bg-body-secondary border-0">
      <div class="card-body">
        <h6 class="card-title fw-semibold"><a href="REslava.Result.AdvancedPatterns.Maybe-1.html" class="text-decoration-none">Maybe&lt;T&gt;</a></h6>
        <p class="card-text small text-muted">
          Null-safe optional value. Eliminates <code>null</code> reference checks with
          a type-safe <code>Some</code> / <code>None</code> distinction.
        </p>
      </div>
    </div>
  </div>

  <div class="col">
    <div class="card h-100 bg-body-secondary border-0">
      <div class="card-body">
        <h6 class="card-title fw-semibold"><a href="REslava.Result.AdvancedPatterns.OneOf-2.html" class="text-decoration-none">OneOf&lt;T1..T8&gt;</a></h6>
        <p class="card-text small text-muted">
          Discriminated union (sealed class). Represents exactly one of 2–8 types.
          Ideal for service methods that return different success shapes.
        </p>
      </div>
    </div>
  </div>

  <div class="col">
    <div class="card h-100 bg-body-secondary border-0">
      <div class="card-body">
        <h6 class="card-title fw-semibold"><a href="REslava.Result.AdvancedPatterns.ErrorsOf-2.html" class="text-decoration-none">ErrorsOf&lt;T1..T8&gt;</a></h6>
        <p class="card-text small text-muted">
          Typed error union (<code>where Ti : IError</code>). Implements <code>IError</code> — usable as
          <code>TError</code> in <code>Result&lt;TValue,TError&gt;</code>.
          Exhaustive <code>Match</code> enforced at compile time.
        </p>
      </div>
    </div>
  </div>

  <div class="col">
    <div class="card h-100 bg-body-secondary border-0">
      <div class="card-body">      
        <h6 class="card-title fw-semibold"><a href="REslava.Result.Result-2.html" class="text-decoration-none">Result&lt;TValue, TError&gt;</a></h6>
        <p class="card-text small text-muted">
          Typed result with a concrete compile-time error type.
          Each <code>Bind</code> step grows the error union by one slot.
          Use <code>.IsSuccess</code>, <code>.Value</code>, <code>.Error</code>.
        </p>
      </div>
    </div>
  </div>

  <div class="col">
    <div class="card h-100 bg-body-secondary border-0">
      <div class="card-body">
        <h6 class="card-title fw-semibold"><a href="REslava.Result.Error.html" class="text-decoration-none">Error hierarchy</a></h6>
        <p class="card-text small text-muted">
          <code>Error</code> · <code>ValidationError</code> (field + message) ·
          <code>ExceptionError</code> · <code>ConversionError</code>.
          All implement <code>IError</code> with tag-based HTTP status mapping.
        </p>
      </div>
    </div>
  </div>

  <div class="col">
    <div class="card h-100 bg-body-secondary border-0">
      <div class="card-body">
        <h6 class="card-title fw-semibold"><a href="REslava.Result.Reason-1.html" class="text-decoration-none">Reason&lt;TReason&gt;</a></h6>
        <p class="card-text small text-muted">
          Base class for all reasons (successes and errors). Carries a <code>Message</code>,
          optional <code>Tags</code> dictionary, and CRTP fluent builder pattern.
        </p>
      </div>
    </div>
  </div>

  <div class="col">
    <div class="card h-100 bg-body-secondary border-0">
      <div class="card-body">
        <h6 class="card-title fw-semibold"><a href="REslava.Result.Extensions.html" class="text-decoration-none">Extension methods</a></h6>
        <p class="card-text small text-muted">
          Functional pipeline: <code>Map → Bind → Tap → Ensure → ToIResult</code>.
          Async variants for all operations. Combinators: <code>WhenAll</code>, <code>Retry</code>, <code>Timeout</code>.
        </p>
      </div>
    </div>
  </div>

</div>

<!-- ── Footer note ───────────────────────────────────────────────────────── -->
<p class="text-muted small border-top pt-4">
  Use the namespace menu at the top to browse all types.
  Source generators and analyzer rules are documented in the
  <a href="https://reslava.github.io/nuget-package-reslava-result/">main docs</a>.
</p>

</div>
