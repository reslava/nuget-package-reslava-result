using System.Collections.Immutable;

namespace REslava.Result;

public partial class Result<TValue> : Result, IResultBase<TValue>
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
        binder = binder.EnsureNotNull(nameof(binder));

        // If already failed, convert to new type with same reasons
        if (IsFailure)
        {
            return new Result<TOut>(default, Reasons) { Context = Context };
        }

        try
        {
            var bindResult = binder(Value!);

            // If original result has success reasons, preserve them
            if (Successes.Count > 0)
            {
                // Enrich any errors produced by the binder with parent context tags
                var bindReasons = bindResult.IsFailure
                    ? ResultContextEnricher.EnrichReasons(bindResult.Reasons, Context)
                    : bindResult.Reasons;

                // Combine original successes with bind result's reasons
                // Order: original successes first (chronological), then new reasons
                var combinedReasons = Successes.ToImmutableList<IReason>()
                    .AddRange(bindReasons);

                // Determine the value based on bind result's success/failure
                var value = bindResult.IsSuccess ? bindResult.Value : default;

                return new Result<TOut>(value, combinedReasons) { Context = Context };
            }

            // Enrich errors produced by the binder, then apply parent-wins context
            if (bindResult.IsFailure)
            {
                var enrichedReasons = ResultContextEnricher.EnrichReasons(bindResult.Reasons, Context);
                return new Result<TOut>(default, enrichedReasons) { Context = Context };
            }

            // Success: parent-wins context
            bindResult.Context = Context;
            return bindResult;
        }
        catch (Exception ex)
        {
            var exceptionError = (IError)ResultContextEnricher.EnrichError(new ExceptionError(ex), Context);

            // Even on exception, preserve original success reasons
            if (Successes.Count > 0)
            {
                // Combine: original successes + exception error
                var combinedReasons = Successes.ToImmutableList<IReason>()
                    .Add(exceptionError);

                return new Result<TOut>(default, combinedReasons) { Context = Context };
            }

            // No successes to preserve
            var failResult = Result<TOut>.Fail(exceptionError);
            failResult.Context = Context;
            return failResult;
        }
    }

    /// <summary>
    /// Asynchronously chains another operation that returns a Result, allowing for sequential operations.
    /// Preserves success reasons from the original result in all cases.
    /// Also known as FlatMap or SelectMany.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="binder">The async function that returns a new Result.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The result of the binder function with accumulated success reasons, or a failed result.</returns>
    public async Task<Result<TOut>> BindAsync<TOut>(Func<TValue, Task<Result<TOut>>> binder, CancellationToken cancellationToken = default)
    {
        binder = binder.EnsureNotNull(nameof(binder));
        cancellationToken.ThrowIfCancellationRequested();

        // If already failed, convert to new type with same reasons
        if (IsFailure)
        {
            return new Result<TOut>(default, Reasons) { Context = Context };
        }

        try
        {
            var bindResult = await binder(Value!);

            // If original result has success reasons, preserve them
            if (Successes.Count > 0)
            {
                // Enrich any errors produced by the binder with parent context tags
                var bindReasons = bindResult.IsFailure
                    ? ResultContextEnricher.EnrichReasons(bindResult.Reasons, Context)
                    : bindResult.Reasons;

                // Combine original successes with bind result's reasons
                // Order: original successes first (chronological), then new reasons
                var combinedReasons = Successes.ToImmutableList<IReason>()
                    .AddRange(bindReasons);

                // Determine the value based on bind result's success/failure
                var value = bindResult.IsSuccess ? bindResult.Value : default;

                return new Result<TOut>(value, combinedReasons) { Context = Context };
            }

            // Enrich errors produced by the binder, then apply parent-wins context
            if (bindResult.IsFailure)
            {
                var enrichedReasons = ResultContextEnricher.EnrichReasons(bindResult.Reasons, Context);
                return new Result<TOut>(default, enrichedReasons) { Context = Context };
            }

            // Success: parent-wins context
            bindResult.Context = Context;
            return bindResult;
        }
        catch (Exception ex)
        {
            var exceptionError = (IError)ResultContextEnricher.EnrichError(new ExceptionError(ex), Context);

            // Even on exception, preserve original success reasons
            if (Successes.Count > 0)
            {
                // Combine: original successes + exception error
                var combinedReasons = Successes.ToImmutableList<IReason>()
                    .Add(exceptionError);

                return new Result<TOut>(default, combinedReasons) { Context = Context };
            }

            // No successes to preserve
            var failResult = Result<TOut>.Fail(exceptionError);
            failResult.Context = Context;
            return failResult;
        }
    }
}