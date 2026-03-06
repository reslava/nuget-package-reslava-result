using System.Collections.Immutable;

namespace REslava.Result.Extensions;

/// <summary>
/// Task extension methods for providing fallback results on failure.
/// </summary>
public static class ResultOrTaskExtensions
{
    /// <summary>
    /// Awaits the result task then returns it if successful; otherwise returns the provided fallback.
    /// </summary>
    public static async Task<Result> Or(
        this Task<Result> resultTask,
        Result fallback,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.Or(fallback);
    }

    /// <summary>
    /// Awaits the result task then returns it if successful; otherwise invokes the fallback factory.
    /// </summary>
    public static async Task<Result> OrElse(
        this Task<Result> resultTask,
        Func<ImmutableList<IError>, Result> fallbackFactory,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.OrElse(fallbackFactory);
    }

    /// <summary>
    /// Awaits the result task then returns it if successful; otherwise asynchronously invokes the fallback factory.
    /// </summary>
    public static async Task<Result> OrElseAsync(
        this Task<Result> resultTask,
        Func<ImmutableList<IError>, Task<Result>> fallbackFactory,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return await result.OrElseAsync(fallbackFactory, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the result task then returns it if successful; otherwise returns the provided fallback.
    /// </summary>
    public static async Task<Result<T>> Or<T>(
        this Task<Result<T>> resultTask,
        Result<T> fallback,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.Or(fallback);
    }

    /// <summary>
    /// Awaits the result task then returns it if successful; otherwise invokes the fallback factory.
    /// </summary>
    public static async Task<Result<T>> OrElse<T>(
        this Task<Result<T>> resultTask,
        Func<ImmutableList<IError>, Result<T>> fallbackFactory,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.OrElse(fallbackFactory);
    }

    /// <summary>
    /// Awaits the result task then returns it if successful; otherwise asynchronously invokes the fallback factory.
    /// </summary>
    public static async Task<Result<T>> OrElseAsync<T>(
        this Task<Result<T>> resultTask,
        Func<ImmutableList<IError>, Task<Result<T>>> fallbackFactory,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return await result.OrElseAsync(fallbackFactory, cancellationToken).ConfigureAwait(false);
    }
}
