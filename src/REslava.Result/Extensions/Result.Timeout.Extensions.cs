namespace REslava.Result.Extensions;

/// <summary>
/// Extension methods for enforcing timeouts on async Result operations.
/// </summary>
public static class ResultTimeoutExtensions
{
    /// <summary>
    /// Enforces a timeout on an async Result operation.
    /// If the operation does not complete within the specified timeout,
    /// returns a failed Result with a timeout error.
    /// </summary>
    /// <remarks>
    /// The underlying task is NOT cancelled when a timeout occurs.
    /// Pass a CancellationToken to the underlying operation for cooperative cancellation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await GetSlowData().Timeout(TimeSpan.FromSeconds(5));
    /// </code>
    /// </example>
    public static async Task<Result<T>> Timeout<T>(
        this Task<Result<T>> resultTask,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        resultTask = resultTask.EnsureNotNull(nameof(resultTask));
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            var delayTask = Task.Delay(timeout, cts.Token);
            var completedTask = await Task.WhenAny(resultTask, delayTask);

            if (completedTask == resultTask)
            {
                await cts.CancelAsync();

                if (resultTask.IsFaulted)
                    return Result<T>.Fail(
                        new ExceptionError(resultTask.Exception!.InnerException ?? resultTask.Exception));

                return await resultTask;
            }
            else
            {
                return Result<T>.Fail(
                    new Error($"Operation timed out after {FormatTimeout(timeout)}")
                        .WithTags(("TimeoutTag", true), ("Timeout", timeout)));
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Result<T>.Fail(
                new ExceptionError(new OperationCanceledException(cancellationToken)));
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Enforces a timeout on a non-generic async Result operation.
    /// </summary>
    public static async Task<Result> Timeout(
        this Task<Result> resultTask,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        resultTask = resultTask.EnsureNotNull(nameof(resultTask));
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            var delayTask = Task.Delay(timeout, cts.Token);
            var completedTask = await Task.WhenAny(resultTask, delayTask);

            if (completedTask == resultTask)
            {
                await cts.CancelAsync();

                if (resultTask.IsFaulted)
                    return Result.Fail(
                        new ExceptionError(resultTask.Exception!.InnerException ?? resultTask.Exception));

                return await resultTask;
            }
            else
            {
                return Result.Fail(
                    new Error($"Operation timed out after {FormatTimeout(timeout)}")
                        .WithTags(("TimeoutTag", true), ("Timeout", timeout)));
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Result.Fail(
                new ExceptionError(new OperationCanceledException(cancellationToken)));
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionError(ex));
        }
    }

    private static string FormatTimeout(TimeSpan timeout)
    {
        if (timeout.TotalSeconds < 1)
            return $"{timeout.TotalMilliseconds}ms";
        return $"{timeout.TotalSeconds}s";
    }
}
