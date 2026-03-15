using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Extension methods for providing fallback results on failure.
/// </summary>
public static class ResultOrExtensions
{
    /// <summary>
    /// Returns the result if successful; otherwise returns the provided fallback.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <param name="fallback">The fallback result to use on failure.</param>
    /// <returns>The original result if successful; otherwise <paramref name="fallback"/>.</returns>
    public static Result Or(
        this Result result,
        Result fallback)
    {
        result = result.EnsureNotNull(nameof(result));
        if (result.IsSuccess) return result;
        fallback.Context = result.Context;
        return fallback;
    }

    /// <summary>
    /// Returns the result if successful; otherwise invokes the fallback factory with the current errors.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <param name="fallbackFactory">Function that receives the current errors and produces a fallback result.</param>
    /// <returns>The original result if successful; otherwise the result of <paramref name="fallbackFactory"/>.</returns>
    public static Result OrElse(
        this Result result,
        Func<ImmutableList<IError>, Result> fallbackFactory)
    {
        result = result.EnsureNotNull(nameof(result));
        fallbackFactory = fallbackFactory.EnsureNotNull(nameof(fallbackFactory));
        if (result.IsSuccess) return result;
        var fallback = fallbackFactory(result.Errors);
        fallback.Context = result.Context;
        return fallback;
    }

    /// <summary>
    /// Returns the result if successful; otherwise asynchronously invokes the fallback factory with the current errors.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <param name="fallbackFactory">Async function that receives the current errors and produces a fallback result.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The original result if successful; otherwise the result of <paramref name="fallbackFactory"/>.</returns>
    public static async Task<Result> OrElseAsync(
        this Result result,
        Func<ImmutableList<IError>, Task<Result>> fallbackFactory,
        CancellationToken cancellationToken = default)
    {
        result = result.EnsureNotNull(nameof(result));
        fallbackFactory = fallbackFactory.EnsureNotNull(nameof(fallbackFactory));
        cancellationToken.ThrowIfCancellationRequested();
        if (result.IsSuccess) return result;
        var fallback = await fallbackFactory(result.Errors).ConfigureAwait(false);
        fallback.Context = result.Context;
        return fallback;
    }

    /// <summary>
    /// Returns the result if successful; otherwise returns the provided fallback.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <param name="fallback">The fallback result to use on failure.</param>
    /// <returns>The original result if successful; otherwise <paramref name="fallback"/>.</returns>
    public static Result<T> Or<T>(
        this Result<T> result,
        Result<T> fallback)
    {
        result = result.EnsureNotNull(nameof(result));
        if (result.IsSuccess) return result;
        fallback.Context = result.Context;
        return fallback;
    }

    /// <summary>
    /// Returns the result if successful; otherwise invokes the fallback factory with the current errors.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <param name="fallbackFactory">Function that receives the current errors and produces a fallback result.</param>
    /// <returns>The original result if successful; otherwise the result of <paramref name="fallbackFactory"/>.</returns>
    public static Result<T> OrElse<T>(
        this Result<T> result,
        Func<ImmutableList<IError>, Result<T>> fallbackFactory)
    {
        result = result.EnsureNotNull(nameof(result));
        fallbackFactory = fallbackFactory.EnsureNotNull(nameof(fallbackFactory));
        if (result.IsSuccess) return result;
        var fallback = fallbackFactory(result.Errors);
        fallback.Context = result.Context;
        return fallback;
    }

    /// <summary>
    /// Returns the result if successful; otherwise asynchronously invokes the fallback factory with the current errors.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <param name="fallbackFactory">Async function that receives the current errors and produces a fallback result.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The original result if successful; otherwise the result of <paramref name="fallbackFactory"/>.</returns>
    public static async Task<Result<T>> OrElseAsync<T>(
        this Result<T> result,
        Func<ImmutableList<IError>, Task<Result<T>>> fallbackFactory,
        CancellationToken cancellationToken = default)
    {
        result = result.EnsureNotNull(nameof(result));
        fallbackFactory = fallbackFactory.EnsureNotNull(nameof(fallbackFactory));
        cancellationToken.ThrowIfCancellationRequested();
        if (result.IsSuccess) return result;
        var fallback = await fallbackFactory(result.Errors).ConfigureAwait(false);
        fallback.Context = result.Context;
        return fallback;
    }
}
