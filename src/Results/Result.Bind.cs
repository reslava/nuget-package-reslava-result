using System;

namespace REslava.Result;

public partial class Result<TValue> : Result, IResult<TValue>
{
    /// <summary>
    /// Chains another operation that returns a Result, allowing for sequential operations.
    /// Preserves success reasons from the original result in all cases.
    /// Also known as FlatMap or SelectMany.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="binder">The function that returns a new Result.</param>
    /// <returns>The result of the binder function with accumulated success reasons, or a failed result.</returns>
    public Result<TOut> Bind<TOut>(Func<TValue, Result<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        // If already failed, convert to new type with same errors
        if (IsFailed)
        {
            var failedResult = new Result<TOut>();
            failedResult.Reasons.AddRange(Reasons);
            return failedResult;
        }

        try
        {
            var bindResult = binder(Value!);

            // Always preserve original success reasons if they exist
            if (Successes.Any())
            {
                var resultWithSuccesses = bindResult.IsSuccess
                    ? Result<TOut>.Ok(bindResult.Value!)
                    : new Result<TOut>();

                // Add original success reasons first (chronological order)
                resultWithSuccesses.Reasons.AddRange(Successes);
                // Then add new reasons (both successes and errors)
                resultWithSuccesses.Reasons.AddRange(bindResult.Reasons);

                return resultWithSuccesses;
            }

            // No original successes to preserve
            return bindResult;
        }
        catch (Exception ex)
        {            
            var errorResult = Result<TOut>.Fail(new ExceptionError(ex));
            
            // Even on exception, preserve original success reasons
            if (Successes.Any())
            {
                errorResult.Reasons.InsertRange(0, Successes);
            }
            
            return errorResult;
        }
    }

    /// <summary>
    /// Asynchronously chains another operation that returns a Result, allowing for sequential operations.
    /// Preserves success reasons from the original result in all cases.
    /// Also known as FlatMap or SelectMany.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="binder">The async function that returns a new Result.</param>
    /// <returns>The result of the binder function with accumulated success reasons, or a failed result.</returns>
    public async Task<Result<TOut>> BindAsync<TOut>(Func<TValue, Task<Result<TOut>>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);

        // If already failed, convert to new type with same errors
        if (IsFailed)
        {
            var failedResult = new Result<TOut>();
            failedResult.Reasons.AddRange(Reasons);
            return failedResult;
        }

        try
        {
            var bindResult = await binder(Value!);

            // Always preserve original success reasons if they exist
            if (Successes.Any())
            {
                var resultWithSuccesses = bindResult.IsSuccess
                    ? Result<TOut>.Ok(bindResult.Value!)
                    : new Result<TOut>();

                // Add original success reasons first (chronological order)
                resultWithSuccesses.Reasons.AddRange(Successes);
                // Then add new reasons (both successes and errors)
                resultWithSuccesses.Reasons.AddRange(bindResult.Reasons);

                return resultWithSuccesses;
            }

            // No original successes to preserve
            return bindResult;
        }
        catch (Exception ex)
        {
            var errorResult = Result<TOut>.Fail(new ExceptionError(ex));
            
            // Even on exception, preserve original success reasons
            if (Successes.Any())
            {
                errorResult.Reasons.InsertRange(0, Successes);
            }
            
            return errorResult;
        }
    }







    
}
