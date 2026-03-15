namespace REslava.Result.Extensions;

/// <summary>
/// Extension methods for validation operations on Result types.
/// </summary>
public static class ResultValidationExtensions
{
    #region Ensure - Sync with Error

    /// <summary>
    /// Ensures that a condition is met, otherwise returns a failed result.
    /// </summary>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
    {
        predicate = predicate.EnsureNotNull(nameof(predicate));
        error = error.EnsureNotNull(nameof(error));

        if (result.IsFailure)
        {
            return result;
        }

        try
        {
            if (predicate(result.Value!)) return result;
            var enrichedError = ResultContextEnricher.EnrichError(error, result.Context);
            var fail = Result<T>.Fail(enrichedError);
            fail.Context = result.Context;
            return fail;
        }
        catch (Exception ex)
        {
            var enrichedError = ResultContextEnricher.EnrichError(new ExceptionError(ex), result.Context);
            var fail = Result<T>.Fail(enrichedError);
            fail.Context = result.Context;
            return fail;
        }
    }

    #endregion

    #region Ensure - Sync with String

    /// <summary>
    /// Ensures that a condition is met, otherwise returns a failed result.
    /// </summary>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        string errorMessage)
    {
        predicate = predicate.EnsureNotNull(nameof(predicate));

        if (result.IsFailure)
        {
            return result;
        }

        try
        {
            if (predicate(result.Value!)) return result;
            var rawError = new Error(errorMessage);
            var enrichedError = ResultContextEnricher.EnrichError(rawError, result.Context);
            var fail = Result<T>.Fail(enrichedError);
            fail.Context = result.Context;
            return fail;
        }
        catch (Exception ex)
        {
            var enrichedError = ResultContextEnricher.EnrichError(new ExceptionError(ex), result.Context);
            var fail = Result<T>.Fail(enrichedError);
            fail.Context = result.Context;
            return fail;
        }
    }

    #endregion

    #region Ensure - Multiple Validations

    /// <summary>
    /// Ensures that multiple conditions are met, otherwise returns a failed result.
    /// </summary>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        params (Func<T, bool> predicate, Error error)[] validations)
    {
        result = result.EnsureNotNull(nameof(result));
        validations = validations.EnsureNotNull(nameof(validations));

        if (validations.Length == 0)
        {
            throw new ArgumentException("At least one validation is required", nameof(validations));
        }

        if (result.IsFailure)
        {
            return result;
        }

        var errors = new List<IError>();
        foreach (var (predicate, error) in validations)
        {
            if (!predicate(result.Value!))
            {
                errors.Add(error);
            }
        }

        if (errors.Any())
        {
            var enrichedErrors = errors.Select(e => ResultContextEnricher.EnrichError(e, result.Context));
            var fail = Result<T>.Fail(enrichedErrors);
            fail.Context = result.Context;
            return fail;
        }

        return result;
    }

    #endregion

    #region EnsureAsync - Task<Result<T>> + Sync Predicate + Error

    /// <summary>
    /// Awaits the result then ensures that a condition is met, otherwise returns a failed result.
    /// <example>
    /// <code>
    /// var result = await GetUserAsync(userId)
    ///     .EnsureAsync(
    ///         user => user.IsActive,
    ///         new Error("User is not active"));
    /// </code>
    /// </example>
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, bool> predicate,
        Error error,
        CancellationToken cancellationToken = default)
    {
        predicate = predicate.EnsureNotNull(nameof(predicate));
        error = error.EnsureNotNull(nameof(error));
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask;

        if (result.IsFailure)
        {
            return result;
        }

        try
        {
            if (predicate(result.Value!)) return result;
            var enrichedError = ResultContextEnricher.EnrichError(error, result.Context);
            var fail = Result<T>.Fail(enrichedError);
            fail.Context = result.Context;
            return fail;
        }
        catch (Exception ex)
        {
            var enrichedError = ResultContextEnricher.EnrichError(new ExceptionError(ex), result.Context);
            var fail = Result<T>.Fail(enrichedError);
            fail.Context = result.Context;
            return fail;
        }
    }

    /// <summary>
    /// Awaits the result then ensures that a condition is met, otherwise returns a failed result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, bool> predicate,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        predicate = predicate.EnsureNotNull(nameof(predicate));
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask;

        if (result.IsFailure)
        {
            return result;
        }

        try
        {
            if (predicate(result.Value!)) return result;
            var rawError = new Error(errorMessage);
            var enrichedError = ResultContextEnricher.EnrichError(rawError, result.Context);
            var fail = Result<T>.Fail(enrichedError);
            fail.Context = result.Context;
            return fail;
        }
        catch (Exception ex)
        {
            var enrichedError = ResultContextEnricher.EnrichError(new ExceptionError(ex), result.Context);
            var fail = Result<T>.Fail(enrichedError);
            fail.Context = result.Context;
            return fail;
        }
    }

    #endregion

    #region EnsureAsync - Result<T> + Async Predicate + Error

    /// <summary>
    /// Ensures that an async condition is met, otherwise returns a failed result.
    /// <example>
    /// <code>
    /// var user = new User { Id = 123, Email = "test@example.com" };
    /// var result = Result&lt;User&gt;.Ok(user);
    ///
    /// var validated = await result
    ///     .EnsureAsync(
    ///         async u => await _db.UserExistsAsync(u.Id),
    ///         new Error("User not found in database"));
    /// </code>
    /// </example>
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Result<T> result,
        Func<T, Task<bool>> predicate,
        Error error,
        CancellationToken cancellationToken = default)
    {
        predicate = predicate.EnsureNotNull(nameof(predicate));
        error = error.EnsureNotNull(nameof(error));
        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsFailure)
        {
            return result;
        }

        var isValid = await predicate(result.Value!);
        if (isValid) return result;
        var enrichedError = ResultContextEnricher.EnrichError(error, result.Context);
        var fail = Result<T>.Fail(enrichedError);
        fail.Context = result.Context;
        return fail;
    }

    /// <summary>
    /// Ensures that an async condition is met, otherwise returns a failed result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Result<T> result,
        Func<T, Task<bool>> predicate,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        predicate = predicate.EnsureNotNull(nameof(predicate));
        cancellationToken.ThrowIfCancellationRequested();

        if (result.IsFailure)
        {
            return result;
        }

        var isValid = await predicate(result.Value!);
        if (isValid) return result;
        var rawError = new Error(errorMessage);
        var enrichedError = ResultContextEnricher.EnrichError(rawError, result.Context);
        var fail = Result<T>.Fail(enrichedError);
        fail.Context = result.Context;
        return fail;
    }

    #endregion

    #region EnsureAsync - Task<Result<T>> + Async Predicate + Error

    /// <summary>
    /// Awaits the result then ensures that an async condition is met, otherwise returns a failed result.
    /// <example>
    /// <code>
    /// var result = await GetUserAsync(userId)
    ///     .EnsureAsync(
    ///         async user => await _userService.IsUserActiveAsync(user.Id),
    ///         new Error("User is not active"));
    /// </code>
    /// </example>
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task<bool>> predicate,
        Error error,
        CancellationToken cancellationToken = default)
    {
        predicate = predicate.EnsureNotNull(nameof(predicate));
        error = error.EnsureNotNull(nameof(error));
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask;

        if (result.IsFailure)
        {
            return result;
        }

        var isValid = await predicate(result.Value!);
        if (isValid) return result;
        var enrichedError = ResultContextEnricher.EnrichError(error, result.Context);
        var fail = Result<T>.Fail(enrichedError);
        fail.Context = result.Context;
        return fail;
    }

    /// <summary>
    /// Awaits the result then ensures that an async condition is met, otherwise returns a failed result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task<bool>> predicate,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        predicate = predicate.EnsureNotNull(nameof(predicate));
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask;

        if (result.IsFailure)
        {
            return result;
        }

        var isValid = await predicate(result.Value!);
        if (isValid) return result;
        var rawError = new Error(errorMessage);
        var enrichedError = ResultContextEnricher.EnrichError(rawError, result.Context);
        var fail = Result<T>.Fail(enrichedError);
        fail.Context = result.Context;
        return fail;
    }

    #endregion

    #region Common Validations

    /// <summary>
    /// Ensures the value is not null.
    /// </summary>
    public static Result<T> EnsureNotNull<T>(
        this Result<T> result,
        string errorMessage) where T : class
    {
        return result.Ensure(
            v => v is not null,
            errorMessage ?? "Value can not be null");
    }

    /// <summary>
    /// Awaits the result then ensures the value is not null.
    /// </summary>
    public static Task<Result<T>> EnsureNotNullAsync<T>(
        this Task<Result<T>> resultTask,
        string errorMessage,
        CancellationToken cancellationToken = default) where T : class
    {
        return resultTask.EnsureAsync(
            v => v is not null,
            errorMessage ?? "Value can not be null",
            cancellationToken);
    }

    #endregion
}
