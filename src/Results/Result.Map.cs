using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime;
using System.Text;

namespace REslava.Result;

public partial class Result<TValue> : Result, IResult<TValue>
{
    /// <summary>
    /// Transform the value of a successful result using a specific mapper function.
    /// If the result is failed return a new error with the same reasons.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="mapper">The function to convert the value.</param>
    /// <returns>A new result of transformed value or the original errors</returns>
    public Result<TOut> Map<TOut>(Func<TValue, TOut> mapper)
    {
        if (mapper is null)
        {
            throw new ArgumentNullException(nameof(mapper));
        }

        if (IsFailed)
        {
            var result = new Result<TOut>();
            result.Reasons.AddRange(Reasons);
            return result;
        }

        try
        {
            TOut mappedValue = mapper(Value!);
            var result = Result<TOut>.Ok(mappedValue);
            result.Reasons.AddRange(Successes);  // ← ADD THIS LINE
            return result;
        }
        catch (Exception ex)
        {            
            return Result<TOut>.Fail(new ExceptionError(ex));
        }
    }
    
    /// <summary>
    /// Asynchronously transform the value of a successful result using a specific mapper function.
    /// If the result is failed return a new error with the same reasons.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="mapper">The function to convert the value.</param>
    /// <returns>A new result of transformed value or the original errors</returns>
    public async Task<Result<TOut>> MapAsync<TOut>(Func<TValue, Task<TOut>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        if (IsFailed)
        {
            var failedResult = new Result<TOut>();
            failedResult.Reasons.AddRange(Reasons);
            return failedResult;
        }

        try
        {
            var mappedValue = await mapper(Value!);
            var successResult = Result<TOut>.Ok(mappedValue);
            successResult.Reasons.AddRange(Successes);
            return successResult;
        }
        catch (Exception ex)
        {
            return Result<TOut>.Fail(new ExceptionError(ex));
        }
    }

    
}

public static class ResultTaskExtensions
{
    /// <summary>
    /// Maps the value inside a Task_Result_T to a new value of type U
    /// </summary>
    public static async Task<Result<U>> Map<T, U>(
        this Task<Result<T>> resultTask,
        Func<T, U> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    /// <summary>
    /// Chains async operations on Task_Result_T
    /// </summary>
    public static async Task<Result<U>> BindAsync<T, U>(
        this Task<Result<T>> resultTask,
        Func<T, Task<Result<U>>> binder)
    {
        var result = await resultTask;
        return await result.BindAsync(binder);
    }

    /// <summary>
    /// Validates the value inside Task_Result_T
    /// </summary>
    public static async Task<Result<T>> Ensure<T>(
        this Task<Result<T>> resultTask,
        Func<T, bool> predicate,
        string errorMessage)
    {
        var result = await resultTask;
        return result.Ensure(predicate, errorMessage);
    }
}
