using System.Collections.Immutable;

namespace REslava.Result.Extensions;

/// <summary>
/// Task extension methods for transforming errors in the failure path of Result types.
/// </summary>
public static class ResultMapErrorTaskExtensions
{
    /// <summary>
    /// Awaits the result task then transforms errors using the provided sync mapper.
    /// If the result is successful, it is returned unchanged.
    /// </summary>
    public static async Task<Result> MapErrorAsync(
        this Task<Result> resultTask,
        Func<ImmutableList<IError>, ImmutableList<IError>> mapper,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.MapError(mapper);
    }

    /// <summary>
    /// Awaits the result task then transforms errors using the provided sync mapper.
    /// If the result is successful, it is returned unchanged.
    /// </summary>
    public static async Task<Result<T>> MapErrorAsync<T>(
        this Task<Result<T>> resultTask,
        Func<ImmutableList<IError>, ImmutableList<IError>> mapper,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.MapError(mapper);
    }

    /// <summary>
    /// Awaits the result task then transforms errors using the provided async mapper.
    /// If the result is successful, it is returned unchanged.
    /// </summary>
    public static async Task<Result> MapErrorAsync(
        this Task<Result> resultTask,
        Func<ImmutableList<IError>, Task<ImmutableList<IError>>> mapper,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return await result.MapErrorAsync(mapper, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the result task then transforms errors using the provided async mapper.
    /// If the result is successful, it is returned unchanged.
    /// </summary>
    public static async Task<Result<T>> MapErrorAsync<T>(
        this Task<Result<T>> resultTask,
        Func<ImmutableList<IError>, Task<ImmutableList<IError>>> mapper,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return await result.MapErrorAsync(mapper, cancellationToken).ConfigureAwait(false);
    }
}
