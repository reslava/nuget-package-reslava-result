using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// Extension methods for transforming errors in the failure path of Result types.
/// </summary>
public static class ResultMapErrorExtensions
{
    /// <summary>
    /// Transforms the errors of a failed result using the provided mapper.
    /// If the result is successful, it is returned unchanged.
    /// The result state (IsSuccess/IsFailure) never changes.
    /// </summary>
    /// <param name="result">The result to map errors on.</param>
    /// <param name="mapper">Function that transforms the current error list.</param>
    /// <returns>The original result if successful; otherwise a new failed result with mapped errors.</returns>
    public static Result MapError(
        this Result result,
        Func<ImmutableList<IError>, ImmutableList<IError>> mapper)
    {
        result = result.EnsureNotNull(nameof(result));
        mapper = mapper.EnsureNotNull(nameof(mapper));
        if (result.IsSuccess) return result;
        var fail = Result.Fail(mapper(result.Errors));
        fail.Context = result.Context;
        return fail;
    }

    /// <summary>
    /// Transforms the errors of a failed result using the provided mapper.
    /// If the result is successful, it is returned unchanged.
    /// The result state (IsSuccess/IsFailure) never changes.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to map errors on.</param>
    /// <param name="mapper">Function that transforms the current error list.</param>
    /// <returns>The original result if successful; otherwise a new failed result with mapped errors.</returns>
    public static Result<T> MapError<T>(
        this Result<T> result,
        Func<ImmutableList<IError>, ImmutableList<IError>> mapper)
    {
        result = result.EnsureNotNull(nameof(result));
        mapper = mapper.EnsureNotNull(nameof(mapper));
        if (result.IsSuccess) return result;
        var fail = Result<T>.Fail(mapper(result.Errors));
        fail.Context = result.Context;
        return fail;
    }

    /// <summary>
    /// Asynchronously transforms the errors of a failed result using the provided async mapper.
    /// If the result is successful, it is returned unchanged.
    /// </summary>
    /// <param name="result">The result to map errors on.</param>
    /// <param name="mapper">Async function that transforms the current error list.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The original result if successful; otherwise a new failed result with mapped errors.</returns>
    public static async Task<Result> MapErrorAsync(
        this Result result,
        Func<ImmutableList<IError>, Task<ImmutableList<IError>>> mapper,
        CancellationToken cancellationToken = default)
    {
        result = result.EnsureNotNull(nameof(result));
        mapper = mapper.EnsureNotNull(nameof(mapper));
        cancellationToken.ThrowIfCancellationRequested();
        if (result.IsSuccess) return result;
        var fail = Result.Fail(await mapper(result.Errors).ConfigureAwait(false));
        fail.Context = result.Context;
        return fail;
    }

    /// <summary>
    /// Asynchronously transforms the errors of a failed result using the provided async mapper.
    /// If the result is successful, it is returned unchanged.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to map errors on.</param>
    /// <param name="mapper">Async function that transforms the current error list.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The original result if successful; otherwise a new failed result with mapped errors.</returns>
    public static async Task<Result<T>> MapErrorAsync<T>(
        this Result<T> result,
        Func<ImmutableList<IError>, Task<ImmutableList<IError>>> mapper,
        CancellationToken cancellationToken = default)
    {
        result = result.EnsureNotNull(nameof(result));
        mapper = mapper.EnsureNotNull(nameof(mapper));
        cancellationToken.ThrowIfCancellationRequested();
        if (result.IsSuccess) return result;
        var fail = Result<T>.Fail(await mapper(result.Errors).ConfigureAwait(false));
        fail.Context = result.Context;
        return fail;
    }
}
