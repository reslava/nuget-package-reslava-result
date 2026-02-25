using REslava.Result;
using FastMinimalAPI.REslava.Result.Demo.SmartEndpoints;

namespace FastMinimalAPI.REslava.Result.Demo.Services;

/// <summary>
/// Service backing the SmartValidationController demo.
/// Shows two validation approaches: DataAnnotations (handled by [Validate] guard before service call)
/// and the native Validation DSL (handled inside the service method).
/// </summary>
public class ValidationDemoService
{
    // ── DataAnnotations approach ─────────────────────────────────────────────

    /// <summary>
    /// Called after the SmartEndpoints-generated validation guard fires.
    /// By the time this method runs, DataAnnotations have already been checked
    /// (the guard returns 422 if any annotation fails). Pure business logic here.
    /// </summary>
    public Task<Result<ValidationDemoResponse>> CreateWithAnnotationsAsync(
        CreateAnnotatedRequest request,
        CancellationToken cancellationToken = default)
    {
        // If we reach here, DataAnnotations already passed.
        // Add business-rule checks that DataAnnotations can't express:
        if (request.Price > 10_000m && request.Category == "Budget")
            return Task.FromResult(
                Result<ValidationDemoResponse>.Fail(
                    "A 'Budget' product cannot have a price above 10,000"));

        return Task.FromResult(
            Result<ValidationDemoResponse>.Ok(
                new ValidationDemoResponse(
                    "DataAnnotations + [Validate]",
                    $"Created '{request.ProductName}' at {request.Price:C} [{request.Category}]")));
    }

    // ── Validation DSL approach ──────────────────────────────────────────────

    // Built once — rule compilation happens at startup, not per-request
    private static readonly ValidatorRuleSet<CreateDslRequest> _dslValidator =
        new ValidatorRuleBuilder<CreateDslRequest>()
            .NotEmpty(r => r.ProductName)
            .MinLength(r => r.ProductName, 2)
            .MaxLength(r => r.ProductName, 100)
            .Positive<CreateDslRequest, decimal>(r => r.Price)
            .LessThan(r => r.Price, 100_000m)
            .NotEmpty(r => r.Category)
            .NonNegative<CreateDslRequest, int>(r => r.StockQuantity)
            .Range(r => r.StockQuantity, 0, 10_000)
            .Build();

    /// <summary>
    /// Validation runs inside the service using the native Validation DSL.
    /// No [Validate] attribute needed on CreateDslRequest — the DSL is code-first.
    /// </summary>
    public Task<Result<ValidationDemoResponse>> CreateWithDslAsync(
        CreateDslRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = _dslValidator.Validate(request);
        if (validation.IsFailure)
            return Task.FromResult(validation.Map(_ => (ValidationDemoResponse)null!));

        return Task.FromResult(
            Result<ValidationDemoResponse>.Ok(
                new ValidationDemoResponse(
                    "Validation DSL",
                    $"Created '{request.ProductName}' at {request.Price:C} [{request.Category}] (stock: {request.StockQuantity})")));
    }
}
