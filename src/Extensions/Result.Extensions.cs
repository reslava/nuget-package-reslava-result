namespace REslava.Result;

public static class ResultExtensions
{
    #region Tap - Non-Generic Result
    
    /// <summary>
    /// Executes a side effect without modifying the result.
    /// </summary>
    public static void Tap(this Result result, Action action)
    {
        if (result.IsSuccess)
        {
            action();
        }
    }

    /// <summary>
    /// Executes a side effect asynchronously without modifying the result.
    /// </summary>
    public static async Task TapAsync(this Result result, Func<Task> action)
    {
        if (result.IsSuccess)
        {
            await action();
        }
    }
    public static Result<T> TapOnFailure<T>(this Result<T> result, Action<IError> action)
    {
        if (result.IsFailed)
        {
            action(result.Errors[0]);
        }
        return result;
    }

    public static async Task<Result<T>> TapOnFailureAsync<T>(this Result<T> result, Func<IError, Task> action)
    {
        if (result.IsFailed)
        {
            await action(result.Errors[0]);
        }
        return result;
    }
    

    /// <summary>
    /// Awaits the result then executes a side effect without modifying it.
    /// </summary>
    public static async Task TapAsync(this Task<Result> resultTask, Action action)
    {
        var result = await resultTask;
        if (result.IsSuccess)
        {
            action();
        }
    }

    /// <summary>
    /// Awaits the result then executes an async side effect without modifying it.
    /// </summary>
    public static async Task TapAsync(this Task<Result> resultTask, Func<Task> action)
    {
        var result = await resultTask;
        if (result.IsSuccess)
        {
            await action();
        }
    }    

    #endregion

    #region Tap - Generic Result<T>

    /// <summary>
    /// Executes a side effect without modifying the result.
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
        {
            action(result.Value!);
        }
        return result;
    }

    /// <summary>
    /// Executes an async side effect without modifying the result.
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(this Result<T> result, Func<T, Task> action)
    {
        if (result.IsSuccess)
        {
            await action(result.Value!);
        }
        return result;
    }

    /// <summary>
    /// Awaits the result then executes a side effect without modifying it.
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Action<T> action)
    {
        var result = await resultTask;
        if (result.IsSuccess)
        {
            action(result.Value!);
        }
        return result;
    }

    /// <summary>
    /// Awaits the result then executes an async side effect without modifying it.
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Func<T, Task> action)
    {
        var result = await resultTask;
        if (result.IsSuccess)
        {
            await action(result.Value!);
        }
        return result;
    }

    #endregion
}
