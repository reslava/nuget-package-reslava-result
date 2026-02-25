using System.ComponentModel.DataAnnotations;
using REslava.Result;
using REslava.Result.SourceGenerators;
using REslava.Result.SourceGenerators.SmartEndpoints;
using FastMinimalAPI.REslava.Result.Demo.Services;

namespace FastMinimalAPI.REslava.Result.Demo.SmartEndpoints;

/// <summary>
/// Dedicated controller showcasing three validation approaches side-by-side.
///
/// Generated endpoints:
///   GET    /api/smart/validation/demo          — no validation (baseline)
///   POST   /api/smart/validation/with-validate  — [Validate] on body → auto-guard
///   POST   /api/smart/validation/with-dsl       — Validation DSL in service
/// </summary>
[AutoGenerateEndpoints(RoutePrefix = "/api/smart/validation")]
public class SmartValidationController
{
    private readonly ValidationDemoService _service;

    public SmartValidationController(ValidationDemoService service) => _service = service;

    /// <summary>
    /// Baseline — no validation, returns a plain response.
    /// </summary>
    public Task<Result<ValidationDemoResponse>> GetDemo()
        => Task.FromResult(Result<ValidationDemoResponse>.Ok(
            new ValidationDemoResponse("baseline", "No validation on this endpoint")));

    /// <summary>
    /// DataAnnotations + [Validate] approach:
    /// CreateAnnotatedRequest is decorated with [Validate] — the source generator
    /// emits a .Validate() extension and auto-injects the guard in the lambda.
    ///
    /// Generated POST lambda (conceptual):
    ///   var validation = request.Validate();
    ///   if (!validation.IsSuccess) return validation.ToIResult();  // 422
    ///   var result = await svc.CreateWithAnnotationsAsync(request, ct);
    ///   return result.ToIResult();
    /// </summary>
    public async Task<Result<ValidationDemoResponse>> CreateWithValidate(
        CreateAnnotatedRequest request,
        CancellationToken cancellationToken = default)
        => await _service.CreateWithAnnotationsAsync(request, cancellationToken);

    /// <summary>
    /// Validation DSL approach:
    /// The service runs 19-rule DSL validation internally, returns Result&lt;T&gt;.
    /// No [Validate] attribute needed — fully code-based.
    /// </summary>
    public async Task<Result<ValidationDemoResponse>> CreateWithDsl(
        CreateDslRequest request,
        CancellationToken cancellationToken = default)
        => await _service.CreateWithDslAsync(request, cancellationToken);
}

// ── Request types ──────────────────────────────────────────────────────────────

/// <summary>
/// DataAnnotations-backed request — [Validate] enables .Validate() extension generation
/// and SmartEndpoints auto-injection.
/// </summary>
[Validate]
public record CreateAnnotatedRequest(
    [property: Required(ErrorMessage = "ProductName is required")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "ProductName must be 2–100 characters")]
    string ProductName,

    [property: Range(0.01, 99_999.99, ErrorMessage = "Price must be between 0.01 and 99,999.99")]
    decimal Price,

    [property: Required(ErrorMessage = "Category is required")]
    string Category);

/// <summary>
/// Plain request type — validated entirely via Validation DSL inside the service.
/// No attributes needed on the record itself.
/// </summary>
public record CreateDslRequest(
    string ProductName,
    decimal Price,
    string Category,
    int StockQuantity);

/// <summary>
/// Response DTO for validation demo endpoints.
/// </summary>
public record ValidationDemoResponse(string Approach, string Message);
