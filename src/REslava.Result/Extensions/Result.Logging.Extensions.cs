using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace REslava.Result.Extensions;

/// <summary>
/// Extension methods for logging Result outcome to <see cref="ILogger"/>.
/// </summary>
public static class ResultLoggingExtensions
{
    /// <summary>
    /// Awaits the result task, logs the outcome to <paramref name="logger"/>, and returns the result
    /// unchanged (Tap-style). Success → <see cref="LogLevel.Debug"/>; failure without exception →
    /// <see cref="LogLevel.Warning"/>; failure wrapping <see cref="ExceptionError"/> → <see cref="LogLevel.Error"/>.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="resultTask">The result task to await.</param>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="operationName">Name of the operation — appears in every log message.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The original result, unchanged.</returns>
    /// <example>
    /// <code>
    /// Result&lt;User&gt; result = await service.GetUser(id)
    ///     .WithLogger(logger, "GetUser");
    /// </code>
    /// </example>
    public static async Task<Result<T>> WithLogger<T>(
        this Task<Result<T>> resultTask,
        ILogger logger,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.WithLogger(logger, operationName);
    }

    /// <summary>
    /// Awaits the result task, logs only on failure, and returns the result unchanged (Tap-style).
    /// Failure without exception → <see cref="LogLevel.Warning"/>;
    /// failure wrapping <see cref="ExceptionError"/> → <see cref="LogLevel.Error"/>.
    /// Success produces no log output.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="resultTask">The result task to await.</param>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="operationName">Name of the operation — appears in every log message.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The original result, unchanged.</returns>
    /// <example>
    /// <code>
    /// Result&lt;Order&gt; result = await service.PlaceOrder(request)
    ///     .LogOnFailure(logger, "PlaceOrder");
    /// </code>
    /// </example>
    public static async Task<Result<T>> LogOnFailure<T>(
        this Task<Result<T>> resultTask,
        ILogger logger,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask.ConfigureAwait(false);
        return result.LogOnFailure(logger, operationName);
    }
}
