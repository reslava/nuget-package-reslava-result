using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace REslava.Result;

public static class ResultExtensions
{
    #region Tap - Non-Generic Result    

    /// <summary>
    /// Awaits the result then executes a side effect without modifying it.
    /// </summary>
    public static async Task<Result> TapAsync(this Task<Result> resultTask, Action action)
    {
        var result = await resultTask;
        if (result.IsSuccess)
        {
            action();
        }
        return result;
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

    // Non-generic Result
    public static Result TapOnFailure(this Result result, Action<IError> action)
    {
        ValidationExtensions.EnsureNotNull(action, nameof(action));
        ValidationExtensions.EnsureNotNullOrEmpty(result.Errors, nameof(result.Errors));
        if (result.IsFailed)
            action(result.Errors[0]);
        return result;            
    }
    public static async Task<Result> TapOnFailureAsync(this Result result, Func<IError, Task> action)
    {
        ValidationExtensions.EnsureNotNull(action, nameof(action));
        ValidationExtensions.EnsureNotNullOrEmpty(result.Errors, nameof(result.Errors));
        if (result.IsFailed) 
            await action(result.Errors[0]);
        return result;
    }

    // Task<Result<T>> extensions
    public static async Task<Result<T>> TapOnFailureAsync<T>(
        this Task<Result<T>> resultTask,
        Action<IError> action)
    {
        var result = await resultTask;
        if (result.IsFailed)
            action(result.Errors[0]);
        return result;
    }

    // All errors variants
    public static Result<T> TapOnFailure<T>(
        this Result<T> result,
        Action<ImmutableList<IError>> action)
    {        
        if (result.IsFailed)
            action(result.Errors);
        return result;
    }


    #endregion
}
