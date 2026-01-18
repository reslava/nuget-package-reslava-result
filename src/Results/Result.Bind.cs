using System.Collections.Immutable;

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
        ArgumentNullException.ThrowIfNull(binder, nameof(binder));

        // If already failed, convert to new type with same reasons
        if (IsFailed)
        {
            return new Result<TOut>(default, Reasons);
        }

        try
        {
            var bindResult = binder(Value!);

            // If original result has success reasons, preserve them
            if (Successes.Count > 0)
            {
                // Combine original successes with bind result's reasons
                // Order: original successes first (chronological), then new reasons
                var combinedReasons = Successes.ToImmutableList<IReason>()
                    .AddRange(bindResult.Reasons);

                // Determine the value based on bind result's success/failure
                var value = bindResult.IsSuccess ? bindResult.Value : default;
                
                return new Result<TOut>(value, combinedReasons);
            }

            // No original successes to preserve - return bind result as-is
            return bindResult;
        }
        catch (Exception ex)
        {            
            var exceptionError = new ExceptionError(ex);
            
            // Even on exception, preserve original success reasons
            if (Successes.Count > 0)
            {
                // Combine: original successes + exception error
                var combinedReasons = Successes.ToImmutableList<IReason>()
                    .Add(exceptionError);
                
                return new Result<TOut>(default, combinedReasons);
            }
            
            // No successes to preserve
            return Result<TOut>.Fail(exceptionError);
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
        ArgumentNullException.ThrowIfNull(binder, nameof(binder));

        // If already failed, convert to new type with same reasons
        if (IsFailed)
        {
            return new Result<TOut>(default, Reasons);
        }

        try
        {
            var bindResult = await binder(Value!);

            // If original result has success reasons, preserve them
            if (Successes.Count > 0)
            {
                // Combine original successes with bind result's reasons
                // Order: original successes first (chronological), then new reasons
                var combinedReasons = Successes.ToImmutableList<IReason>()
                    .AddRange(bindResult.Reasons);

                // Determine the value based on bind result's success/failure
                var value = bindResult.IsSuccess ? bindResult.Value : default;                
                
                return new Result<TOut>(value, combinedReasons);
            }

            // No original successes to preserve - return bind result as-is
            return bindResult;
        }
        catch (Exception ex)
        {
            var exceptionError = new ExceptionError(ex);
            
            // Even on exception, preserve original success reasons
            if (Successes.Count > 0)
            {
                // Combine: original successes + exception error
                var combinedReasons = Successes.ToImmutableList<IReason>()
                    .Add(exceptionError);
                
                return new Result<TOut>(default, combinedReasons);
            }
            
            // No successes to preserve
            return Result<TOut>.Fail(exceptionError);
        }
    }
}