// ⚠️ OPTIONAL — FluentValidation migration bridge demo
// This file requires:
//   - REslava.Result.FluentValidation project reference (generator-only, no runtime binary)
//   - FluentValidation NuGet package (IValidator<T>, AbstractValidator<T>)
//
// Only for teams MIGRATING from FluentValidation to REslava.Result.
// New projects should use [Validate] + DataAnnotations or the Validation DSL instead.

using FluentValidation;
using REslava.Result;
using REslava.Result.FluentValidation;
using REslava.Result.SourceGenerators.SmartEndpoints;

namespace FastMinimalAPI.REslava.Result.Demo.SmartEndpoints;

/// <summary>
/// Optional FluentValidation bridge demo.
///
/// [FluentValidate] on the request type causes the source generator to emit:
///   - request.Validate(IValidator&lt;T&gt;) → Result&lt;T&gt;
///   - request.ValidateAsync(IValidator&lt;T&gt;, CancellationToken) → Task&lt;Result&lt;T&gt;&gt;
///
/// SmartEndpoints detects [FluentValidate] on the POST body param and auto-injects
/// IValidator&lt;T&gt; as a lambda parameter plus emits the validation guard block.
///
/// Generated POST lambda (conceptual):
///   fluentGroup.MapPost("/", async (
///       CreateFluentProductRequest req,
///       IValidator&lt;CreateFluentProductRequest&gt; reqValidator,  // ← auto-injected
///       ISmartFluentValidationController svc,
///       CancellationToken cancellationToken) =>
///   {
///       var validation = req.Validate(reqValidator);           // ← auto-emitted
///       if (!validation.IsSuccess) return validation.ToIResult();
///       var result = await svc.CreateFluentProduct(req, cancellationToken);
///       return result.ToIResult();
///   });
/// </summary>
[AutoGenerateEndpoints(RoutePrefix = "/api/smart/fluent-validation")]
public class SmartFluentValidationController
{
    private readonly FluentValidationDemoService _service;

    public SmartFluentValidationController(FluentValidationDemoService service) => _service = service;

    /// <summary>
    /// GET endpoint — no FluentValidation involved; just a baseline.
    /// </summary>
    public Task<Result<FluentDemoResponse>> GetFluentDemo()
        => Task.FromResult(Result<FluentDemoResponse>.Ok(
            new FluentDemoResponse("baseline", "FluentValidation bridge demo")));

    /// <summary>
    /// POST endpoint — CreateFluentProductRequest is decorated with [FluentValidate].
    /// IValidator&lt;CreateFluentProductRequest&gt; is auto-injected and the guard auto-emitted.
    /// </summary>
    public async Task<Result<FluentDemoResponse>> CreateFluentProduct(
        CreateFluentProductRequest request,
        CancellationToken cancellationToken = default)
        => await _service.CreateAsync(request, cancellationToken);
}

// ── Request / response types ───────────────────────────────────────────────────

/// <summary>
/// Decorated with [FluentValidate] — validated by an existing FluentValidation validator.
/// Cannot also have [Validate]. (RESL1006 enforces this at compile time.)
/// </summary>
[FluentValidate]
public record CreateFluentProductRequest(
    string ProductName,
    decimal Price,
    string Category);

public record FluentDemoResponse(string Approach, string Message);

// ── FluentValidation validator — your existing validator, unchanged ────────────

/// <summary>
/// Your existing FluentValidation validator — unchanged when adopting REslava.Result.
/// Register in DI: builder.Services.AddScoped&lt;IValidator&lt;CreateFluentProductRequest&gt;, ...&gt;()
/// </summary>
public class CreateFluentProductRequestValidator : AbstractValidator<CreateFluentProductRequest>
{
    public CreateFluentProductRequestValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("ProductName is required")
            .MaximumLength(100).WithMessage("ProductName cannot exceed 100 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required");
    }
}

// ── Service backing the demo endpoint ─────────────────────────────────────────

public class FluentValidationDemoService
{
    public Task<Result<FluentDemoResponse>> CreateAsync(
        CreateFluentProductRequest request,
        CancellationToken cancellationToken = default)
    {
        // If we reach here, FluentValidation guard already passed.
        return Task.FromResult(
            Result<FluentDemoResponse>.Ok(
                new FluentDemoResponse(
                    "FluentValidation bridge ([FluentValidate])",
                    $"Created '{request.ProductName}' at {request.Price:C} [{request.Category}]")));
    }
}
