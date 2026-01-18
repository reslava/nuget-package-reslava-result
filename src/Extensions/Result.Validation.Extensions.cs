namespace REslava.Result;

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
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);

        if (result.IsFailed)
        {
            return result;
        }

        return predicate(result.Value!) ? result : Result<T>.Fail(error);
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
        ArgumentNullException.ThrowIfNull(predicate);

        if (result.IsFailed)
        {
            return result;
        }

        return predicate(result.Value!) ? result : Result<T>.Fail(errorMessage);
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
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(validations);
        
        if (validations.Length == 0)
        {
            throw new ArgumentException("At least one validation is required", nameof(validations));
        }

        if (result.IsFailed)
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
            return Result<T>.Fail(errors);
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
        Error error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);

        var result = await resultTask;

        if (result.IsFailed)
        {
            return result;
        }

        return predicate(result.Value!) ? result : Result<T>.Fail(error);
    }

    /// <summary>
    /// Awaits the result then ensures that a condition is met, otherwise returns a failed result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, bool> predicate,
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var result = await resultTask;

        if (result.IsFailed)
        {
            return result;
        }

        return predicate(result.Value!) ? result : Result<T>.Fail(errorMessage);
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
        Error error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);

        if (result.IsFailed)
        {
            return result;
        }

        var isValid = await predicate(result.Value!);
        return isValid ? result : Result<T>.Fail(error);
    }

    /// <summary>
    /// Ensures that an async condition is met, otherwise returns a failed result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Result<T> result,
        Func<T, Task<bool>> predicate,
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        if (result.IsFailed)
        {
            return result;
        }

        var isValid = await predicate(result.Value!);
        return isValid ? result : Result<T>.Fail(errorMessage);
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
        Error error)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);

        var result = await resultTask;

        if (result.IsFailed)
        {
            return result;
        }

        var isValid = await predicate(result.Value!);
        return isValid ? result : Result<T>.Fail(error);
    }

    /// <summary>
    /// Awaits the result then ensures that an async condition is met, otherwise returns a failed result.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task<bool>> predicate,
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var result = await resultTask;

        if (result.IsFailed)
        {
            return result;
        }

        var isValid = await predicate(result.Value!);
        return isValid ? result : Result<T>.Fail(errorMessage);
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
        string errorMessage) where T : class
    {
        return resultTask.EnsureAsync(
            v => v is not null,
            errorMessage ?? "Value can not be null");
    }

    #endregion

    #region Working with Tasks
    /// <summary>
    /// Validates the value inside Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="resultTask">The task containing the result to validate.</param>
    /// <param name="predicate">The validation predicate.</param>
    /// <param name="errorMessage">The error message if validation fails.</param>
    /// <returns>A task containing the validated result.</returns>
    public static async Task<Result<T>> Ensure<T>(
        this Task<Result<T>> resultTask,
        Func<T, bool> predicate,
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(resultTask, nameof(resultTask));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));

        var result = await resultTask;
        return result.Ensure(predicate, errorMessage);
    }
    #endregion
}
