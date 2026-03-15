using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Represents a result that can be either successful or failed.
/// </summary>
public partial class Result : IResultBase
{
    // Lazy-cached filtered collections — safe because Result is immutable
    private ImmutableList<IError>? _errors;
    private ImmutableList<ISuccess>? _successes;

    /// <summary>
    /// Ambient context for this result. Null by default on non-generic Result (no T to infer
    /// Entity from). Set explicitly via <see cref="WithContext"/>.
    /// </summary>
    public ResultContext? Context { get; internal set; }

    /// <summary>
    /// Returns a copy of this result with additional context values merged in.
    /// Only non-null arguments overwrite existing context fields.
    /// </summary>
    public Result WithContext(
        string? entity = null,
        string? entityId = null,
        string? correlationId = null,
        string? operationName = null,
        string? tenantId = null)
    {
        var current = Context ?? ResultContext.Empty;
        var merged = current with
        {
            Entity        = entity        ?? current.Entity,
            EntityId      = entityId      ?? current.EntityId,
            CorrelationId = correlationId ?? current.CorrelationId,
            OperationName = operationName ?? current.OperationName,
            TenantId      = tenantId      ?? current.TenantId,
        };
        return new Result(Reasons) { Context = merged };
    }

    public bool IsSuccess => !IsFailure;
    public bool IsFailure => Errors.Count > 0;
    public ImmutableList<IReason> Reasons { get; private init; }
    public ImmutableList<IError> Errors => _errors ??= Reasons.OfType<IError>().ToImmutableList();
    public ImmutableList<ISuccess> Successes => _successes ??= Reasons.OfType<ISuccess>().ToImmutableList();

    // Protected constructors - only factory methods create Results
    /// <summary>
    /// Initializes a new instance of the Result class with no reasons.
    /// This constructor is protected and used internally for creating successful results.
    /// </summary>
    protected Result()
    {
        Reasons = [];
    }

    /// <summary>
    /// Initializes a new instance of the Result class with multiple reasons.
    /// This constructor is internal and used by factory methods.
    /// </summary>
    /// <param name="reasons">The reasons associated with this result.</param>
    internal Result(ImmutableList<IReason> reasons)
    {
        Reasons = reasons ?? [];
    }

    /// <summary>
    /// Initializes a new instance of the Result class with a single reason.
    /// This constructor is protected and used internally.
    /// </summary>
    /// <param name="reason">The reason associated with this result.</param>
    protected Result(IReason reason)
    {
        reason = reason.EnsureNotNull(nameof(reason));
        Reasons = ImmutableList.Create(reason);
    }

    /// <summary>
    /// Returns a string representation of the Result.
    /// </summary>
    public override string ToString()
    {
        var reasons = string.Join(", ", Reasons.Select(r => r.Message));
        return $"Result: IsSuccess='{IsSuccess}', Reasons=[{reasons}]";
    }

    /// <summary>
    /// Matches result to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure with errors.</param>
    /// <returns>The result of the executed function.</returns>
    public TOut Match<TOut>(
        Func<TOut> onSuccess, 
        Func<ImmutableList<IError>, TOut> onFailure)
    {
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        if (IsSuccess)
        {
            return onSuccess();
        }
        else
        {
            return onFailure(Errors);
        }
    }

    /// <summary>
    /// Asynchronously executes one of two async actions based on success or failure.
    /// </summary>
    /// <param name="onSuccess">Async action to execute on success.</param>
    /// <param name="onFailure">Async action to execute on failure with errors.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task MatchAsync(
        Func<Task> onSuccess,
        Func<ImmutableList<IError>, Task> onFailure,
        CancellationToken cancellationToken = default)
    {
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        cancellationToken.ThrowIfCancellationRequested();
        if (IsSuccess)
        {
            await onSuccess();
        }
        else
        {
            await onFailure(Errors);
        }
    }
}