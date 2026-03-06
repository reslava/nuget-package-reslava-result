using System.Collections.Immutable;

namespace REslava.Result.Extensions;

/// <summary>
/// Task extension methods for void side-effect dispatch on Result types.
/// </summary>
public static class ResultSwitchTaskExtensions
{
    /// <summary>
    /// Awaits the result task then executes one of two sync actions based on success or failure.
    /// </summary>
    public static async Task Switch(
        this Task<Result> resultTask,
        Action onSuccess,
        Action<ImmutableList<IError>> onFailure,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask;
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
    /// Awaits the result task then executes one of two sync actions based on success or failure.
    /// </summary>
    public static async Task Switch<T>(
        this Task<Result<T>> resultTask,
        Action<T> onSuccess,
        Action<ImmutableList<IError>> onFailure,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask;
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
    /// Awaits the result task then executes one of two async actions based on success or failure.
    /// </summary>
    public static async Task SwitchAsync(
        this Task<Result> resultTask,
        Func<Task> onSuccess,
        Func<ImmutableList<IError>, Task> onFailure,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask;
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
    /// Awaits the result task then executes one of two async actions based on success or failure.
    /// </summary>
    public static async Task SwitchAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task> onSuccess,
        Func<ImmutableList<IError>, Task> onFailure,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask;
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
