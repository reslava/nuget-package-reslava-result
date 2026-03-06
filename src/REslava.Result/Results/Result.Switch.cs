using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Extension methods for void side-effect dispatch on Result types.
/// </summary>
public static class ResultSwitchExtensions
{
    /// <summary>
    /// Executes one of two actions based on success or failure.
    /// </summary>
    /// <param name="result">The result to dispatch.</param>
    /// <param name="onSuccess">Action to execute on success.</param>
    /// <param name="onFailure">Action to execute on failure with errors.</param>
    public static void Switch(
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
    /// <param name="result">The result to dispatch.</param>
    /// <param name="onSuccess">Action to execute on success with value.</param>
    /// <param name="onFailure">Action to execute on failure with errors.</param>
    public static void Switch<T>(
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
    /// Asynchronously executes one of two async actions based on success or failure.
    /// </summary>
    /// <param name="result">The result to dispatch.</param>
    /// <param name="onSuccess">Async action to execute on success.</param>
    /// <param name="onFailure">Async action to execute on failure with errors.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    public static async Task SwitchAsync(
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
    /// <param name="result">The result to dispatch.</param>
    /// <param name="onSuccess">Async action to execute on success with value.</param>
    /// <param name="onFailure">Async action to execute on failure with errors.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    public static async Task SwitchAsync<T>(
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
