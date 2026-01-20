namespace REslava.Result;

public partial class Result
{
    #region OkIf - Returns Ok if condition is true

    /// <summary>
    /// Returns Ok if condition is true, otherwise Fail with message.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="errorMessage">The error message if condition is false.</param>
    /// <returns>Ok if condition is true, otherwise a failed result.</returns>
    /// <example>
    /// <code>
    /// var result = Result.OkIf(age >= 18, "Must be 18 or older");
    /// </code>
    /// </example>
    public static Result OkIf(bool condition, string errorMessage)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        
        return condition 
            ? Result.Ok() 
            : Result.Fail(errorMessage);
    }

    /// <summary>
    /// Returns Ok if condition is true, otherwise Fail with error.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="error">The error to return if condition is false.</param>
    /// <returns>Ok if condition is true, otherwise a failed result.</returns>
    /// <example>
    /// <code>
    /// var error = new ValidationError("User must be active");
    /// var result = Result.OkIf(user.IsActive, error);
    /// </code>
    /// </example>
    public static Result OkIf(bool condition, IError error)
    {
        ArgumentNullException.ThrowIfNull(error, nameof(error));
        
        return condition 
            ? Result.Ok() 
            : Result.Fail(error);
    }

    /// <summary>
    /// Returns Ok with success message if condition is true, otherwise Fail.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="errorMessage">The error message if condition is false.</param>
    /// <param name="successMessage">The success message if condition is true.</param>
    /// <returns>Ok with success message if condition is true, otherwise a failed result.</returns>
    /// <example>
    /// <code>
    /// var result = Result.OkIf(
    ///     age >= 18, 
    ///     "Must be 18 or older",
    ///     "Age validation passed"
    /// );
    /// </code>
    /// </example>
    public static Result OkIf(
        bool condition, 
        string errorMessage, 
        string successMessage)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        ArgumentException.ThrowIfNullOrEmpty(successMessage, nameof(successMessage));
        
        return condition 
            ? Result.Ok(successMessage)
            : Result.Fail(errorMessage);
    }

    /// <summary>
    /// Evaluates condition lazily - useful for expensive checks.
    /// </summary>
    /// <param name="predicate">Function that returns the condition to evaluate.</param>
    /// <param name="errorMessage">The error message if condition is false.</param>
    /// <returns>Ok if condition is true, otherwise a failed result.</returns>
    /// <example>
    /// <code>
    /// var result = Result.OkIf(
    ///     () => _database.IsUserActive(userId),
    ///     "User is not active"
    /// );
    /// </code>
    /// </example>
    public static Result OkIf(Func<bool> predicate, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        
        try
        {
            return predicate() 
                ? Result.Ok() 
                : Result.Fail(errorMessage);
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Async version - evaluates condition asynchronously.
    /// </summary>
    /// <param name="predicate">Async function that returns the condition to evaluate.</param>
    /// <param name="errorMessage">The error message if condition is false.</param>
    /// <returns>A task containing Ok if condition is true, otherwise a failed result.</returns>
    /// <example>
    /// <code>
    /// var result = await Result.OkIfAsync(
    ///     () => _api.CheckUserExistsAsync(userId),
    ///     "User does not exist"
    /// );
    /// </code>
    /// </example>
    public static async Task<Result> OkIfAsync(
        Func<Task<bool>> predicate, 
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        
        try
        {
            var condition = await predicate();
            return condition 
                ? Result.Ok() 
                : Result.Fail(errorMessage);
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionError(ex));
        }
    }

    #endregion

    #region FailIf - Returns Fail if condition is true

    /// <summary>
    /// Returns Fail if condition is true, otherwise Ok.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = Result.FailIf(age &lt; 18, "Must be 18 or older");
    /// </code>
    /// </example>
    public static Result FailIf(bool condition, string errorMessage)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        
        return condition 
            ? Result.Fail(errorMessage)
            : Result.Ok();
    }

    /// <summary>
    /// Returns Fail if condition is true, otherwise Ok with error.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="error">The error to return if condition is true.</param>
    /// <returns>Fail if condition is true, otherwise Ok.</returns>
    /// <example>
    /// <code>
    /// var error = new ValidationError("Account is suspended");
    /// var result = Result.FailIf(user.IsSuspended, error);
    /// </code>
    /// </example>
    public static Result FailIf(bool condition, IError error)
    {
        ArgumentNullException.ThrowIfNull(error, nameof(error));
        
        return condition 
            ? Result.Fail(error)
            : Result.Ok();
    }

    /// <summary>
    /// Returns Fail if condition is true, otherwise Ok with success message.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="errorMessage">The error message if condition is true.</param>
    /// <param name="successMessage">The success message if condition is false.</param>
    /// <returns>Fail if condition is true, otherwise Ok with success message.</returns>
    /// <example>
    /// <code>
    /// var result = Result.FailIf(
    ///     age &lt; 18,
    ///     "Must be 18 or older",
    ///     "Age validation passed"
    /// );
    /// </code>
    /// </example>
    public static Result FailIf(
        bool condition, 
        string errorMessage, 
        string successMessage)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        ArgumentException.ThrowIfNullOrEmpty(successMessage, nameof(successMessage));
        
        return condition 
            ? Result.Fail(errorMessage)
            : Result.Ok(successMessage);
    }

    /// <summary>
    /// Evaluates condition lazily.
    /// </summary>
    /// <param name="predicate">Function that returns the condition to evaluate.</param>
    /// <param name="errorMessage">The error message if condition is true.</param>
    /// <returns>Fail if condition is true, otherwise Ok.</returns>
    /// <example>
    /// <code>
    /// var result = Result.FailIf(
    ///     () => _database.IsAccountSuspended(userId),
    ///     "Account is suspended"
    /// );
    /// </code>
    /// </example>
    public static Result FailIf(Func<bool> predicate, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        
        try
        {
            return predicate() 
                ? Result.Fail(errorMessage)
                : Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Async version.
    /// </summary>
    /// <param name="predicate">Async function that returns the condition to evaluate.</param>
    /// <param name="errorMessage">The error message if condition is true.</param>
    /// <returns>A task containing Fail if condition is true, otherwise Ok.</returns>
    /// <example>
    /// <code>
    /// var result = await Result.FailIfAsync(
    ///     () => _api.IsAccountSuspendedAsync(userId),
    ///     "Account is suspended"
    /// );
    /// </code>
    /// </example>
    public static async Task<Result> FailIfAsync(
        Func<Task<bool>> predicate, 
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        
        try
        {
            var condition = await predicate();
            return condition 
                ? Result.Fail(errorMessage)
                : Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionError(ex));
        }
    }

    #endregion
}

public partial class Result<TValue>
{
    #region OkIf with value

    /// <summary>
    /// Returns Ok with value if condition is true, otherwise Fail.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = Result&lt;User&gt;.OkIf(
    ///     user != null, 
    ///     user!, 
    ///     "User not found"
    /// );
    /// </code>
    /// </example>
    public static Result<TValue> OkIf(
        bool condition, 
        TValue value, 
        string errorMessage)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        
        return condition 
            ? Result<TValue>.Ok(value)
            : Result<TValue>.Fail(errorMessage);
    }

    /// <summary>
    /// Returns Ok with value if condition is true, otherwise Fail with error.
    /// </summary>
    public static Result<TValue> OkIf(
        bool condition, 
        TValue value, 
        IError error)
    {
        ArgumentNullException.ThrowIfNull(error, nameof(error));
        
        return condition 
            ? Result<TValue>.Ok(value)
            : Result<TValue>.Fail(error);
    }

    /// <summary>
    /// Evaluates value lazily - only creates value if condition is true.
    /// Useful when value creation is expensive.
    /// </summary>
    public static Result<TValue> OkIf(
        bool condition, 
        Func<TValue> valueFactory, 
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(valueFactory, nameof(valueFactory));
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        
        if (!condition)
        {
            return Result<TValue>.Fail(errorMessage);
        }

        try
        {
            return Result<TValue>.Ok(valueFactory());
        }
        catch (Exception ex)
        {
            return Result<TValue>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Async version - creates value asynchronously if condition is true.
    /// </summary>
    public static async Task<Result<TValue>> OkIfAsync(
        bool condition,
        Func<Task<TValue>> valueFactory,
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(valueFactory, nameof(valueFactory));
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        
        if (!condition)
        {
            return Result<TValue>.Fail(errorMessage);
        }

        try
        {
            var value = await valueFactory();
            return Result<TValue>.Ok(value);
        }
        catch (Exception ex)
        {
            return Result<TValue>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Evaluates both condition and value lazily.
    /// </summary>
    public static Result<TValue> OkIf(
        Func<bool> predicate,
        Func<TValue> valueFactory,
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentNullException.ThrowIfNull(valueFactory, nameof(valueFactory));
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));

        try
        {
            if (!predicate())
            {
                return Result<TValue>.Fail(errorMessage);
            }

            return Result<TValue>.Ok(valueFactory());
        }
        catch (Exception ex)
        {
            return Result<TValue>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Async version - evaluates both condition and value asynchronously.
    /// </summary>
    public static async Task<Result<TValue>> OkIfAsync(
        Func<Task<bool>> predicate,
        Func<Task<TValue>> valueFactory,
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentNullException.ThrowIfNull(valueFactory, nameof(valueFactory));
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));

        try
        {
            var condition = await predicate();
            if (!condition)
            {
                return Result<TValue>.Fail(errorMessage);
            }

            var value = await valueFactory();
            return Result<TValue>.Ok(value);
        }
        catch (Exception ex)
        {
            return Result<TValue>.Fail(new ExceptionError(ex));
        }
    }

    #endregion

    #region FailIf with value

    /// <summary>
    /// Returns Fail if condition is true, otherwise Ok with value.
    /// </summary>
    public static Result<TValue> FailIf(
        bool condition, 
        string errorMessage, 
        TValue value)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        
        return condition 
            ? Result<TValue>.Fail(errorMessage)
            : Result<TValue>.Ok(value);
    }

    /// <summary>
    /// Returns Fail if condition is true, otherwise Ok with error.
    /// </summary>
    public static Result<TValue> FailIf(
        bool condition,
        IError error,
        TValue value)
    {
        ArgumentNullException.ThrowIfNull(error, nameof(error));

        return condition
            ? Result<TValue>.Fail(error)
            : Result<TValue>.Ok(value);
    }

    /// <summary>
    /// Returns Fail if condition is true, otherwise Ok with lazy value.
    /// </summary>
    public static Result<TValue> FailIf(
        bool condition, 
        string errorMessage,
        Func<TValue> valueFactory)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        ArgumentNullException.ThrowIfNull(valueFactory, nameof(valueFactory));
        
        if (condition)
        {
            return Result<TValue>.Fail(errorMessage);
        }

        try
        {
            return Result<TValue>.Ok(valueFactory());
        }
        catch (Exception ex)
        {
            return Result<TValue>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Evaluates both condition and value lazily.
    /// </summary>
    public static Result<TValue> FailIf(
        Func<bool> predicate,
        string errorMessage,
        Func<TValue> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        ArgumentNullException.ThrowIfNull(valueFactory, nameof(valueFactory));

        try
        {
            if (predicate())
            {
                return Result<TValue>.Fail(errorMessage);
            }

            return Result<TValue>.Ok(valueFactory());
        }
        catch (Exception ex)
        {
            return Result<TValue>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Async version - evaluates both condition and value asynchronously.
    /// </summary>
    public static async Task<Result<TValue>> FailIfAsync(
        Func<Task<bool>> predicate,
        string errorMessage,
        Func<Task<TValue>> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentException.ThrowIfNullOrEmpty(errorMessage, nameof(errorMessage));
        ArgumentNullException.ThrowIfNull(valueFactory, nameof(valueFactory));

        try
        {
            var condition = await predicate();
            if (condition)
            {
                return Result<TValue>.Fail(errorMessage);
            }

            var value = await valueFactory();
            return Result<TValue>.Ok(value);
        }
        catch (Exception ex)
        {
            return Result<TValue>.Fail(new ExceptionError(ex));
        }
    }

    #endregion
}