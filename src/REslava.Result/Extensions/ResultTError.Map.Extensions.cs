using System;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Extensions;

/// <summary>
/// <c>Map</c> overloads for <see cref="Result{TValue,TError}"/> typed pipelines.
/// Transforms the success value while leaving the error type unchanged.
/// </summary>
public static class ResultTErrorMapExtensions
{
    /// <summary>
    /// Transforms the success value of a typed result using the given mapper.
    /// If the result is a failure the error is forwarded unchanged.
    /// </summary>
    /// <typeparam name="TIn">The source value type.</typeparam>
    /// <typeparam name="TOut">The target value type.</typeparam>
    /// <typeparam name="TError">The error type. Must implement <see cref="IError"/>.</typeparam>
    /// <param name="result">The result to transform.</param>
    /// <param name="mapper">The function applied to the success value.</param>
    /// <returns>A new result with the mapped value, or the original failure.</returns>
    /// <example>
    /// <code>
    /// Result&lt;Order, ValidationError&gt; result = Validate(req);
    ///
    /// Result&lt;OrderDto, ValidationError&gt; dto = result.Map(order => new OrderDto(order));
    /// </code>
    /// </example>
    public static Result<TOut, TError> Map<TIn, TOut, TError>(
        this Result<TIn, TError> result,
        Func<TIn, TOut> mapper)
        where TError : IError
    {
        if (mapper is null) throw new ArgumentNullException(nameof(mapper));

        if (result.IsFailure)
        {
            var fail = Result<TOut, TError>.Fail(result.Error);
            fail.Context = result.Context;
            return fail;
        }

        // Successful Map: entity updates to TOut name, other fields inherited from parent
        var mappedContext = result.Context is null
            ? new ResultContext { Entity = typeof(TOut).Name }
            : result.Context with { Entity = typeof(TOut).Name };

        var ok = Result<TOut, TError>.Ok(mapper(result.Value));
        ok.Context = mappedContext;
        return ok;
    }
}
