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
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure with errors.</param>
    /// <returns>The result of the executed function.</returns>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess, 
        Func<ImmutableList<IError>, TOut> onFailure)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        return result.IsSuccess ? onSuccess() : onFailure(result.Errors);
    }

    /// <summary>
    /// Matches result to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Function to execute on success with value.</param>
    /// <param name="onFailure">Function to execute on failure with errors.</param>
    /// <returns>The result of the executed function.</returns>
    public static TOut Match<T, TOut>(
        this Result<T> result,
        Func<T, TOut> onSuccess, 
        Func<ImmutableList<IError>, TOut> onFailure)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        return result.IsSuccess ? onSuccess(result.Value!) : onFailure(result.Errors);
    }

    /// <summary>
    /// Executes one of two actions based on success or failure.
    /// </summary>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Action to execute on success.</param>
    /// <param name="onFailure">Action to execute on failure with errors.</param>
    public static void Match(
        this Result result,
        Action onSuccess, 
        Action<ImmutableList<IError>> onFailure)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        if (result.IsSuccess)
        {
            onSuccess();
        }
        else
        {
            onFailure(result.Errors);
        }
    }

    /// <summary>
    /// Executes one of two actions based on success or failure.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Action to execute on success with value.</param>
    /// <param name="onFailure">Action to execute on failure with errors.</param>
    public static void Match<T>(
        this Result<T> result,
        Action<T> onSuccess, 
        Action<ImmutableList<IError>> onFailure)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        if (result.IsSuccess)
        {
            onSuccess(result.Value!);
        }
        else
        {
            onFailure(result.Errors);
        }
    }

    /// <summary>
    /// Asynchronously matches result to one of two async functions based on success or failure.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Async function to execute on success.</param>
    /// <param name="onFailure">Async function to execute on failure with errors.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task containing the result of the executed function.</returns>
    public static async Task<TOut> MatchAsync<TOut>(
        this Result result,
        Func<Task<TOut>> onSuccess,
        Func<ImmutableList<IError>, Task<TOut>> onFailure,
        CancellationToken cancellationToken = default)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        cancellationToken.ThrowIfCancellationRequested();
        return result.IsSuccess
            ? await onSuccess()
            : await onFailure(result.Errors);
    }

    /// <summary>
    /// Asynchronously matches result to one of two async functions based on success or failure.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Async function to execute on success with value.</param>
    /// <param name="onFailure">Async function to execute on failure with errors.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task containing the result of the executed function.</returns>
    public static async Task<TOut> MatchAsync<T, TOut>(
        this Result<T> result,
        Func<T, Task<TOut>> onSuccess,
        Func<ImmutableList<IError>, Task<TOut>> onFailure,
        CancellationToken cancellationToken = default)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        cancellationToken.ThrowIfCancellationRequested();
        return result.IsSuccess
            ? await onSuccess(result.Value!)
            : await onFailure(result.Errors);
    }

    /// <summary>
    /// Asynchronously executes one of two async actions based on success or failure.
    /// </summary>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Async action to execute on success.</param>
    /// <param name="onFailure">Async action to execute on failure with errors.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    public static async Task MatchAsync(
        this Result result,
        Func<Task> onSuccess,
        Func<ImmutableList<IError>, Task> onFailure,
        CancellationToken cancellationToken = default)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        cancellationToken.ThrowIfCancellationRequested();
        if (result.IsSuccess)
        {
            await onSuccess();
        }
        else
        {
            await onFailure(result.Errors);
        }
    }

    /// <summary>
    /// Asynchronously executes one of two async actions based on success or failure.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Async action to execute on success with value.</param>
    /// <param name="onFailure">Async action to execute on failure with errors.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    public static async Task MatchAsync<T>(
        this Result<T> result,
        Func<T, Task> onSuccess,
        Func<ImmutableList<IError>, Task> onFailure,
        CancellationToken cancellationToken = default)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        cancellationToken.ThrowIfCancellationRequested();
        if (result.IsSuccess)
        {
            await onSuccess(result.Value!);
        }
        else
        {
            await onFailure(result.Errors);
        }
    }
}
