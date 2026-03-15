using System.Diagnostics;

namespace REslava.Result.OpenTelemetry;

/// <summary>
/// OpenTelemetry integration extensions for <see cref="Result"/>, <see cref="Result{T}"/>,
/// and <see cref="Result{T,TError}"/> types.
/// </summary>
public static class ResultOpenTelemetryExtensions
{
    // ── Step 26: WithOpenTelemetry ─────────────────────────────────────────────

    /// <summary>
    /// Seeds the result's <see cref="ResultContext"/> with the active OpenTelemetry span:
    /// <list type="bullet">
    ///   <item><description><c>CorrelationId</c> ← <c>Activity.Current.TraceId</c></description></item>
    ///   <item><description><c>OperationName</c> ← <c>Activity.Current.DisplayName</c></description></item>
    /// </list>
    /// No-ops and returns the original result unchanged when no active span is present.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = Result&lt;Order&gt;.Ok(order)
    ///     .WithOpenTelemetry()           // seeds CorrelationId + OperationName from current span
    ///     .WithContext(entityId: orderId.ToString())
    ///     .BindAsync(ValidateStock);
    /// </code>
    /// </example>
    public static Result<T> WithOpenTelemetry<T>(this Result<T> result)
    {
        var (correlationId, operationName) = ReadSpan();
        if (correlationId is null && operationName is null) return result;
        return result.WithContext(correlationId: correlationId, operationName: operationName);
    }

    /// <summary>
    /// Seeds the non-generic result's <see cref="ResultContext"/> with the active OpenTelemetry span.
    /// No-ops when no active span is present.
    /// </summary>
    public static Result WithOpenTelemetry(this Result result)
    {
        var (correlationId, operationName) = ReadSpan();
        if (correlationId is null && operationName is null) return result;
        return result.WithContext(correlationId: correlationId, operationName: operationName);
    }

    /// <summary>
    /// Seeds the typed result's <see cref="ResultContext"/> with the active OpenTelemetry span.
    /// No-ops when no active span is present.
    /// </summary>
    public static Result<T, TError> WithOpenTelemetry<T, TError>(this Result<T, TError> result)
        where TError : IError
    {
        var (correlationId, operationName) = ReadSpan();
        if (correlationId is null && operationName is null) return result;
        return result.WithContext(correlationId: correlationId, operationName: operationName);
    }

    // ── Step 27: WriteErrorTagsToSpan ─────────────────────────────────────────

    /// <summary>
    /// On failure, writes every tag from each error in <see cref="Result{T}.Errors"/>
    /// as a key-value attribute on <c>Activity.Current</c>.
    /// Passes through on success or when no active span is present.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = await ProcessOrderAsync(order)
    ///     .WriteErrorTagsToSpan();   // span receives Entity, EntityId, CorrelationId, etc.
    /// </code>
    /// </example>
    public static Result<T> WriteErrorTagsToSpan<T>(this Result<T> result)
    {
        var activity = Activity.Current;
        if (activity is null || result.IsSuccess) return result;

        foreach (var error in result.Errors)
        {
            foreach (var tag in error.Tags)
                activity.SetTag(tag.Key, tag.Value?.ToString());
        }

        return result;
    }

    /// <summary>
    /// On failure, writes every error tag as a span attribute on <c>Activity.Current</c>.
    /// Passes through on success or when no active span is present.
    /// </summary>
    public static Result WriteErrorTagsToSpan(this Result result)
    {
        var activity = Activity.Current;
        if (activity is null || result.IsSuccess) return result;

        foreach (var error in result.Errors)
        {
            foreach (var tag in error.Tags)
                activity.SetTag(tag.Key, tag.Value?.ToString());
        }

        return result;
    }

    /// <summary>
    /// On failure, writes every tag from the typed error as a span attribute on <c>Activity.Current</c>.
    /// Passes through on success or when no active span is present.
    /// </summary>
    public static Result<T, TError> WriteErrorTagsToSpan<T, TError>(this Result<T, TError> result)
        where TError : IError
    {
        var activity = Activity.Current;
        if (activity is null || result.IsSuccess) return result;

        foreach (var tag in result.Error.Tags)
            activity.SetTag(tag.Key, tag.Value?.ToString());

        return result;
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private static (string? CorrelationId, string? OperationName) ReadSpan()
    {
        var activity = Activity.Current;
        if (activity is null) return (null, null);

        var correlationId = activity.TraceId != default ? activity.TraceId.ToString() : null;
        var operationName = string.IsNullOrEmpty(activity.DisplayName) ? null : activity.DisplayName;

        return (correlationId, operationName);
    }
}
