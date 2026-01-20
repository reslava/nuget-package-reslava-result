using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Extension methods for match operations on Result types.
/// </summary>
public static class ResultMatchExtensions
{
    /// <summary>
    /// Matches result to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure with errors.</param>
    /// <returns>The result of the executed function.</returns>
    public static TOut Match<TOut>(
        Func<TOut> onSuccess, 
        Func<ImmutableList<IError>, TOut> onFailure)
    {
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        return IsSuccess ? onSuccess() : onFailure(Errors);
    }

    /// <summary>
    /// Executes one of two actions based on success or failure.
    /// </summary>
    /// <param name="onSuccess">Action to execute on success with value.</param>
    /// <param name="onFailure">Action to execute on failure with errors.</param>
    public void Match(
        Action<T> onSuccess, 
        Action<ImmutableList<IError>> onFailure)
    {
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        if (IsSuccess)
        {
            onSuccess(Value!);
        }
        else
        {
            onFailure(Errors);
        }
    }

    /// <summary>
    /// Asynchronously matches result to one of two async functions based on success or failure.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="onSuccess">Async function to execute on success with value.</param>
    /// <param name="onFailure">Async function to execute on failure with errors.</param>
    /// <returns>A task containing the result of the executed function.</returns>
    public async Task<TOut> MatchAsync<TOut>(
        Func<T, Task<TOut>> onSuccess,
        Func<ImmutableList<IError>, Task<TOut>> onFailure)
    {
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        return IsSuccess 
            ? await onSuccess(Value!) 
            : await onFailure(Errors);
    }

    /// <summary>
    /// Asynchronously executes one of two async actions based on success or failure.
    /// </summary>
    /// <param name="onSuccess">Async action to execute on success with value.</param>
    /// <param name="onFailure">Async action to execute on failure with errors.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task MatchAsync(
        Func<T, Task> onSuccess, 
        Func<ImmutableList<IError>, Task> onFailure)
    {
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        if (IsSuccess)
        {
            await onSuccess(Value!);
        }
        else
        {
            await onFailure(Errors);
        }
    }
}
