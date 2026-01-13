namespace REslava.Result;

public partial class Result
{
    #region OkIf - Returns Ok if condition is true

    /// <summary>
    /// Returns Ok if condition is true, otherwise Fail with message.
    /// </summary>
    /// <example>
    /// var result = Result.OkIf(age >= 18, "Must be 18 or older");
    /// </example>
    public static Result OkIf(bool condition, string errorMessage)
    {
        return condition 
            ? Result.Ok() 
            : Result.Fail(errorMessage);
    }

    /// <summary>
    /// Returns Ok if condition is true, otherwise Fail with error.
    /// </summary>
    public static Result OkIf(bool condition, IError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return condition 
            ? Result.Ok() 
            : Result.Fail(error);
    }

    /// <summary>
    /// Returns Ok with success message if condition is true, otherwise Fail.
    /// </summary>
    public static Result OkIf(
        bool condition, 
        string errorMessage, 
        string successMessage)
    {
        return condition 
            ? Result.Ok().WithSuccess(successMessage)
            : Result.Fail(errorMessage);
    }

    /// <summary>
    /// Evaluates condition lazily - useful for expensive checks.
    /// </summary>
    public static Result OkIf(Func<bool> predicate, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        
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
    public static async Task<Result> OkIfAsync(
        Func<Task<bool>> predicate, 
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        
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
    /// var result = Result.FailIf(age &lt; 18, "Must be 18 or older");
    /// </example>
    public static Result FailIf(bool condition, string errorMessage)
    {
        return condition 
            ? Result.Fail(errorMessage)
            : Result.Ok();
    }

    /// <summary>
    /// Returns Fail if condition is true, otherwise Ok with error.
    /// </summary>
    public static Result FailIf(bool condition, IError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return condition 
            ? Result.Fail(error)
            : Result.Ok();
    }

    /// <summary>
    /// Returns Fail if condition is true, otherwise Ok with success message.
    /// </summary>
    public static Result FailIf(
        bool condition, 
        string errorMessage, 
        string successMessage)
    {
        return condition 
            ? Result.Fail(errorMessage)
            : Result.Ok().WithSuccess(successMessage);
    }

    /// <summary>
    /// Evaluates condition lazily.
    /// </summary>
    public static Result FailIf(Func<bool> predicate, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        
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
    public static async Task<Result> FailIfAsync(
        Func<Task<bool>> predicate, 
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        
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
    /// var result = Result&lt;User&gt;.OkIf(
    ///     user != null, 
    ///     user!, 
    ///     "User not found"
    /// );
    /// </example>
    public static Result<TValue> OkIf(
        bool condition, 
        TValue value, 
        string errorMessage)
    {
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
        ArgumentNullException.ThrowIfNull(error);
        return condition 
            ? Result<TValue>.Ok(value)
            : Result<TValue>.Fail(error);
    }

    /// <summary>
    /// Evaluates value lazily - only creates value if condition is true.
    /// </summary>
    public static Result<TValue> OkIf(
        bool condition, 
        Func<TValue> valueFactory, 
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(valueFactory);
        
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
        ArgumentNullException.ThrowIfNull(valueFactory);
        
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
        return condition 
            ? Result<TValue>.Fail(errorMessage)
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
        ArgumentNullException.ThrowIfNull(valueFactory);
        
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

    #endregion
}