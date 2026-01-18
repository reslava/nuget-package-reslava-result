using System.Collections.Immutable;

namespace REslava.Result;

public partial class Result<TValue> : Result, IResult<TValue>
{
    /// <summary>
    /// Transform the value of a successful result using a specific mapper function.
    /// If the result is failed, returns a new failed result with the same reasons.
    /// If successful, preserves all success reasons from the original result.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="mapper">The function to convert the value.</param>
    /// <returns>A new result with the transformed value or the original errors.</returns>
    public Result<TOut> Map<TOut>(Func<TValue, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));

        // If already failed, propagate all reasons to new type
        if (IsFailed)
        {
            return new Result<TOut>(default, Reasons);
        }

        try
        {
            // Transform the value
            TOut mappedValue = mapper(Value!);
            
            // Create new result preserving all success reasons
            // If there are success reasons, include them
            if (Successes.Count > 0)
            {
                return new Result<TOut>(mappedValue, Reasons);
            }
            
            // No success reasons to preserve
            return Result<TOut>.Ok(mappedValue);
        }
        catch (Exception ex)
        {
            // On exception, preserve any success reasons from original result
            var exceptionError = new ExceptionError(ex);
            
            if (Successes.Count > 0)
            {
                // Combine original successes with the new error
                var combinedReasons = Successes.ToImmutableList<IReason>().Add(exceptionError);
                return new Result<TOut>(default, combinedReasons);
            }
            
            return Result<TOut>.Fail(exceptionError);
        }
    }
    
    /// <summary>
    /// Asynchronously transforms the value of a successful result using a mapper function.
    /// If the result is failed, returns a new failed result with the same reasons.
    /// If successful, preserves all success reasons from the original result.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="mapper">The async function to convert the value.</param>
    /// <returns>A task containing a new result with the transformed value or the original errors.</returns>
    public async Task<Result<TOut>> MapAsync<TOut>(Func<TValue, Task<TOut>> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));

        // If already failed, propagate all reasons to new type
        if (IsFailed)
        {
            return new Result<TOut>(default, Reasons);
        }

        try
        {
            // Transform the value asynchronously
            var mappedValue = await mapper(Value!);
            
            // Create new result preserving all success reasons
            if (Successes.Count > 0)
            {
                return new Result<TOut>(mappedValue, Reasons);
            }
            
            return Result<TOut>.Ok(mappedValue);
        }
        catch (Exception ex)
        {
            // On exception, preserve any success reasons from original result
            var exceptionError = new ExceptionError(ex);
            
            if (Successes.Count > 0)
            {
                var combinedReasons = Successes.ToImmutableList<IReason>().Add(exceptionError);
                return new Result<TOut>(default, combinedReasons);
            }
            
            return Result<TOut>.Fail(exceptionError);
        }
    }
}

/// <summary>
/// Extension methods for working with Task&lt;Result&lt;T&gt;&gt;.
/// </summary>
public static class ResultTaskExtensions
{
    /// <summary>
    /// Maps the value inside a Task&lt;Result&lt;T&gt;&gt; to a new value of type U.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="U">The target value type.</typeparam>
    /// <param name="resultTask">The task containing the result to map.</param>
    /// <param name="mapper">The function to transform the value.</param>
    /// <returns>A task containing the mapped result.</returns>
    public static async Task<Result<U>> Map<T, U>(
        this Task<Result<T>> resultTask,
        Func<T, U> mapper)
    {
        ArgumentNullException.ThrowIfNull(resultTask, nameof(resultTask));
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
        
        var result = await resultTask;
        return result.Map(mapper);
    }

    /// <summary>
    /// Asynchronously maps the value inside a Task&lt;Result&lt;T&gt;&gt; to a new value.
    /// </summary>
    public static async Task<Result<U>> MapAsync<T, U>(
        this Task<Result<T>> resultTask,
        Func<T, Task<U>> mapper)
    {
        ArgumentNullException.ThrowIfNull(resultTask, nameof(resultTask));
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
        
        var result = await resultTask;
        return await result.MapAsync(mapper);
    }

    /// <summary>
    /// Chains async operations on Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="U">The target value type.</typeparam>
    /// <param name="resultTask">The task containing the result to bind.</param>
    /// <param name="binder">The function that returns a new result.</param>
    /// <returns>A task containing the bound result.</returns>
    public static async Task<Result<U>> BindAsync<T, U>(
        this Task<Result<T>> resultTask,
        Func<T, Task<Result<U>>> binder)
    {
        ArgumentNullException.ThrowIfNull(resultTask, nameof(resultTask));
        ArgumentNullException.ThrowIfNull(binder, nameof(binder));
        
        var result = await resultTask;
        return await result.BindAsync(binder);
    }

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
}