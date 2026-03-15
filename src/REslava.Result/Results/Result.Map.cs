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
        mapper = mapper.EnsureNotNull(nameof(mapper));

        // If already failed, propagate all reasons to new type — entity unchanged (no mapping occurred)
        if (IsFailure)
        {
            return new Result<TOut>(default, Reasons) { Context = Context };
        }

        // Successful Map: entity updates to TOut name, other fields inherited from parent
        var mappedContext = Context is null ? new ResultContext { Entity = typeof(TOut).Name }
                                           : Context with { Entity = typeof(TOut).Name };
        try
        {
            // Transform the value
            TOut mappedValue = mapper(Value!);

            // Create new result preserving all success reasons
            if (Successes.Count > 0)
            {
                return new Result<TOut>(mappedValue, Reasons) { Context = mappedContext };
            }

            // No success reasons to preserve
            var ok = Result<TOut>.Ok(mappedValue);
            ok.Context = mappedContext;
            return ok;
        }
        catch (Exception ex)
        {
            // On exception, preserve any success reasons from original result
            var exceptionError = new ExceptionError(ex);

            if (Successes.Count > 0)
            {
                var combinedReasons = Successes.ToImmutableList<IReason>().Add(exceptionError);
                return new Result<TOut>(default, combinedReasons) { Context = Context };
            }

            var fail = Result<TOut>.Fail(exceptionError);
            fail.Context = Context;
            return fail;
        }
    }
    
    /// <summary>
    /// Asynchronously transforms the value of a successful result using a mapper function.
    /// If the result is failed, returns a new failed result with the same reasons.
    /// If successful, preserves all success reasons from the original result.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="mapper">The async function to convert the value.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task containing a new result with the transformed value or the original errors.</returns>
    public async Task<Result<TOut>> MapAsync<TOut>(Func<TValue, Task<TOut>> mapper, CancellationToken cancellationToken = default)
    {
        mapper = mapper.EnsureNotNull(nameof(mapper));
        cancellationToken.ThrowIfCancellationRequested();

        // If already failed, propagate all reasons to new type — entity unchanged
        if (IsFailure)
        {
            return new Result<TOut>(default, Reasons) { Context = Context };
        }

        var mappedContext = Context is null ? new ResultContext { Entity = typeof(TOut).Name }
                                           : Context with { Entity = typeof(TOut).Name };
        try
        {
            // Transform the value asynchronously
            var mappedValue = await mapper(Value!);

            // Create new result preserving all success reasons
            if (Successes.Count > 0)
            {
                return new Result<TOut>(mappedValue, Reasons) { Context = mappedContext };
            }

            var ok = Result<TOut>.Ok(mappedValue);
            ok.Context = mappedContext;
            return ok;
        }
        catch (Exception ex)
        {
            var exceptionError = new ExceptionError(ex);

            if (Successes.Count > 0)
            {
                var combinedReasons = Successes.ToImmutableList<IReason>().Add(exceptionError);
                return new Result<TOut>(default, combinedReasons) { Context = Context };
            }

            var fail = Result<TOut>.Fail(exceptionError);
            fail.Context = Context;
            return fail;
        }
    }
}

