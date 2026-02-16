namespace REslava.Result;

public partial class Result
{
    /// <summary>
    /// Retries an async operation that returns Result&lt;T&gt; up to maxRetries times.
    /// Uses configurable delay with optional exponential backoff.
    /// </summary>
    /// <param name="operation">The async operation to retry.</param>
    /// <param name="maxRetries">Maximum number of retries after the first attempt. Default is 3.</param>
    /// <param name="delay">Delay between retries. Default is 1 second.</param>
    /// <param name="backoffFactor">Multiplier applied to delay after each retry. Default is 1.0 (constant delay). Use 2.0 for exponential backoff.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The first successful result, or a failed result with all accumulated errors.</returns>
    /// <example>
    /// <code>
    /// // Simple retry
    /// var result = await Result.Retry(() => CallExternalApi(), maxRetries: 3);
    ///
    /// // Exponential backoff
    /// var result = await Result.Retry(
    ///     () => CallExternalApi(),
    ///     maxRetries: 3,
    ///     delay: TimeSpan.FromSeconds(1),
    ///     backoffFactor: 2.0);
    /// </code>
    /// </example>
    public static async Task<Result<T>> Retry<T>(
        Func<Task<Result<T>>> operation,
        int maxRetries = 3,
        TimeSpan? delay = null,
        double backoffFactor = 1.0,
        CancellationToken cancellationToken = default)
    {
        operation = operation.EnsureNotNull(nameof(operation));
        if (maxRetries < 0)
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "maxRetries must be >= 0");
        if (backoffFactor < 1.0)
            throw new ArgumentOutOfRangeException(nameof(backoffFactor), "backoffFactor must be >= 1.0");

        var currentDelay = delay ?? TimeSpan.FromSeconds(1);
        var allErrors = new List<IError>();
        var totalAttempts = maxRetries + 1;

        for (var attempt = 1; attempt <= totalAttempts; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                allErrors.Add(new ExceptionError(new OperationCanceledException(cancellationToken)));
                return Result<T>.Fail(allErrors);
            }

            try
            {
                var result = await operation();

                if (result.IsSuccess)
                    return result;

                allErrors.Add(new Error($"Retry attempt {attempt} of {totalAttempts} failed"));
                foreach (var error in result.Errors)
                    allErrors.Add(error);
            }
            catch (OperationCanceledException oce)
            {
                allErrors.Add(new Error($"Retry attempt {attempt} of {totalAttempts} cancelled"));
                allErrors.Add(new ExceptionError(oce));
                return Result<T>.Fail(allErrors);
            }
            catch (Exception ex)
            {
                allErrors.Add(new Error($"Retry attempt {attempt} of {totalAttempts} failed"));
                allErrors.Add(new ExceptionError(ex));
            }

            // Wait before next retry (skip delay after last attempt)
            if (attempt < totalAttempts)
            {
                try
                {
                    await Task.Delay(currentDelay, cancellationToken);
                }
                catch (OperationCanceledException oce)
                {
                    allErrors.Add(new ExceptionError(oce));
                    return Result<T>.Fail(allErrors);
                }

                currentDelay = TimeSpan.FromMilliseconds(currentDelay.TotalMilliseconds * backoffFactor);
            }
        }

        return Result<T>.Fail(allErrors);
    }

    /// <summary>
    /// Retries an async non-generic operation up to maxRetries times.
    /// </summary>
    public static async Task<Result> Retry(
        Func<Task<Result>> operation,
        int maxRetries = 3,
        TimeSpan? delay = null,
        double backoffFactor = 1.0,
        CancellationToken cancellationToken = default)
    {
        operation = operation.EnsureNotNull(nameof(operation));
        if (maxRetries < 0)
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "maxRetries must be >= 0");
        if (backoffFactor < 1.0)
            throw new ArgumentOutOfRangeException(nameof(backoffFactor), "backoffFactor must be >= 1.0");

        var currentDelay = delay ?? TimeSpan.FromSeconds(1);
        var allErrors = new List<IError>();
        var totalAttempts = maxRetries + 1;

        for (var attempt = 1; attempt <= totalAttempts; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                allErrors.Add(new ExceptionError(new OperationCanceledException(cancellationToken)));
                return Fail(allErrors);
            }

            try
            {
                var result = await operation();

                if (result.IsSuccess)
                    return result;

                allErrors.Add(new Error($"Retry attempt {attempt} of {totalAttempts} failed"));
                foreach (var error in result.Errors)
                    allErrors.Add(error);
            }
            catch (OperationCanceledException oce)
            {
                allErrors.Add(new Error($"Retry attempt {attempt} of {totalAttempts} cancelled"));
                allErrors.Add(new ExceptionError(oce));
                return Fail(allErrors);
            }
            catch (Exception ex)
            {
                allErrors.Add(new Error($"Retry attempt {attempt} of {totalAttempts} failed"));
                allErrors.Add(new ExceptionError(ex));
            }

            if (attempt < totalAttempts)
            {
                try
                {
                    await Task.Delay(currentDelay, cancellationToken);
                }
                catch (OperationCanceledException oce)
                {
                    allErrors.Add(new ExceptionError(oce));
                    return Fail(allErrors);
                }

                currentDelay = TimeSpan.FromMilliseconds(currentDelay.TotalMilliseconds * backoffFactor);
            }
        }

        return Fail(allErrors);
    }
}
