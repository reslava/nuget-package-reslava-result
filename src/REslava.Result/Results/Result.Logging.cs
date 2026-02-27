using System.Linq;
using Microsoft.Extensions.Logging;

namespace REslava.Result;

public partial class Result<TValue>
{
    /// <summary>
    /// Logs the result outcome to <paramref name="logger"/> and returns the result unchanged (Tap-style).
    /// Success is logged at <see cref="LogLevel.Debug"/>; failure without an exception at
    /// <see cref="LogLevel.Warning"/>; failure wrapping an <see cref="ExceptionError"/> at
    /// <see cref="LogLevel.Error"/>.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="operationName">Name of the operation — appears in every log message.</param>
    /// <returns>The original result, unchanged.</returns>
    /// <example>
    /// <code>
    /// Result&lt;User&gt; result = await service.GetUser(id)
    ///     .WithLogger(logger, "GetUser");
    /// </code>
    /// </example>
    public Result<TValue> WithLogger(ILogger logger, string operationName)
    {
        if (IsSuccess)
        {
            logger.Log(LogLevel.Debug, "{OperationName} succeeded", operationName);
            return this;
        }

        LogFailure(logger, operationName);
        return this;
    }

    /// <summary>
    /// Logs the result outcome only on failure and returns the result unchanged (Tap-style).
    /// Failure without an exception is logged at <see cref="LogLevel.Warning"/>;
    /// failure wrapping an <see cref="ExceptionError"/> at <see cref="LogLevel.Error"/>.
    /// On success, nothing is logged.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="operationName">Name of the operation — appears in every log message.</param>
    /// <returns>The original result, unchanged.</returns>
    /// <example>
    /// <code>
    /// Result&lt;Order&gt; result = await service.PlaceOrder(request)
    ///     .LogOnFailure(logger, "PlaceOrder");
    /// </code>
    /// </example>
    public Result<TValue> LogOnFailure(ILogger logger, string operationName)
    {
        if (IsFailure)
            LogFailure(logger, operationName);

        return this;
    }

    private void LogFailure(ILogger logger, string operationName)
    {
        var level = Errors.OfType<ExceptionError>().Any() ? LogLevel.Error : LogLevel.Warning;
        var first = Errors[0];

        if (Errors.Count > 1)
            logger.Log(level,
                "{OperationName} failed with {ErrorCount} errors: {ErrorType} — {ErrorMessage}",
                operationName, Errors.Count, first.GetType().Name, first.Message);
        else
            logger.Log(level,
                "{OperationName} failed: {ErrorType} — {ErrorMessage}",
                operationName, first.GetType().Name, first.Message);
    }
}
