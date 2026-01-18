using System.Collections.Immutable;

namespace REslava.Result;

public partial class Result<TValue> 
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

