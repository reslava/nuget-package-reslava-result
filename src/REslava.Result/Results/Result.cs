using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Represents a result that can be either successful or failed.
/// </summary>
public partial class Result : IResultResponse
{
    public bool IsSuccess => !IsFailed;
    public bool IsFailed => Reasons.OfType<IError>().Any();
    public ImmutableList<IReason> Reasons { get; private init; }
    public ImmutableList<IError> Errors => Reasons.OfType<IError>().ToImmutableList(); 
    public ImmutableList<ISuccess> Successes => Reasons.OfType<ISuccess>().ToImmutableList();

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
    /// <returns>A task representing the async operation.</returns>
    public async Task MatchAsync(
        Func<Task> onSuccess, 
        Func<ImmutableList<IError>, Task> onFailure)
    {
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
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