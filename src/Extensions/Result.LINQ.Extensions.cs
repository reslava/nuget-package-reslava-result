using System.Collections.Immutable;

namespace REslava.Result;

/// <summary>
/// LINQ query syntax support for Result types.
/// Enables functional composition using C# query expressions.
/// </summary>
public static class ResultLINQExtensions
{
    #region SelectMany - Two Parameter (Bind equivalent)

    /// <summary>
    /// Projects each element of a Result into a new Result and flattens the resulting sequences.
    /// This is equivalent to Bind and enables LINQ query syntax.
    /// </summary>
    /// <example>
    /// <code>
    /// // Method syntax
    /// var result = Result&lt;int&gt;.Ok(5)
    ///     .SelectMany(x => Result&lt;string&gt;.Ok(x.ToString()));
    /// 
    /// // Query syntax (single from clause uses this overload)
    /// var result = from x in Result&lt;int&gt;.Ok(5)
    ///              select x * 2;
    /// </code>
    /// </example>
    public static Result<T> SelectMany<S, T>(
        this Result<S> source,
        Func<S, Result<T>> selector)
    {
        selector = selector.EnsureNotNull(nameof(selector));

        // If source failed, propagate failure to new type
        if (source.IsFailed)
        {
            return new Result<T>(default, source.Reasons);
        }

        try
        {
            var result = selector(source.Value!);

            // Preserve success reasons from source if any
            if (result.IsSuccess && source.Successes.Count > 0)
            {
                var combinedReasons = source.Successes.ToImmutableList<IReason>()
                    .AddRange(result.Reasons);

                return new Result<T>(result.Value, combinedReasons);
            }

            return result;
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Asynchronously projects each element of a Result into a new Result.
    /// </summary>
    public static async Task<Result<T>> SelectManyAsync<S, T>(
        this Result<S> source,
        Func<S, Task<Result<T>>> selector)
    {
        selector = selector.EnsureNotNull(nameof(selector));

        if (source.IsFailed)
        {
            return new Result<T>(default, source.Reasons);
        }

        try
        {
            var result = await selector(source.Value!);

            if (result.IsSuccess && source.Successes.Count > 0)
            {
                var combinedReasons = source.Successes.ToImmutableList<IReason>()
                    .AddRange(result.Reasons);

                return new Result<T>(result.Value, combinedReasons);
            }

            return result;
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new ExceptionError(ex));
        }
    }

    #endregion

    #region SelectMany - Three Parameter (LINQ query syntax support)

    /// <summary>
    /// Projects each element of a Result into a new Result, invokes a result selector function on each element,
    /// and flattens the resulting sequences. This enables LINQ query syntax with multiple from clauses.
    /// </summary>
    /// <remarks>
    /// This is the overload that C# query syntax uses when you have multiple 'from' clauses.
    /// The resultSelector takes both the original value (S) and intermediate value (I) and produces
    /// a final value (T) - NOT a Result&lt;T&gt;. This is by design for LINQ compatibility.
    /// </remarks>
    /// <example>
    /// <code>
    /// // LINQ query syntax (uses this overload)
    /// var result = from x in Result&lt;int&gt;.Ok(5)
    ///              from y in Result&lt;int&gt;.Ok(x * 2)
    ///              select x + y;  // resultSelector: (x, y) => x + y
    /// 
    /// // Equivalent fluent syntax
    /// var result = Result&lt;int&gt;.Ok(5)
    ///     .SelectMany(
    ///         x => Result&lt;int&gt;.Ok(x * 2),
    ///         (x, y) => x + y
    ///     );
    /// </code>
    /// </example>
    public static Result<T> SelectMany<S, I, T>(
        this Result<S> source,
        Func<S, Result<I>> selector,
        Func<S, I, T> resultSelector)
    {
        selector = selector.EnsureNotNull(nameof(selector));
        resultSelector = resultSelector.EnsureNotNull(nameof(resultSelector));

        if (source.IsFailed)
        {
            return new Result<T>(default, source.Reasons);
        }

        try
        {
            var intermediateResult = selector(source.Value!);

            if (intermediateResult.IsFailed)
            {
                // Combine errors from both source and intermediate
                var combinedReasons = source.Errors.ToImmutableList<IReason>()
                    .AddRange(intermediateResult.Reasons);

                return new Result<T>(default, combinedReasons);
            }

            // Apply final transformation
            var finalValue = resultSelector(source.Value!, intermediateResult.Value!);

            // Preserve all success reasons from the chain
            var allSuccesses = source.Successes.ToImmutableList<IReason>()
                .AddRange(intermediateResult.Successes);

            return new Result<T>(finalValue, allSuccesses);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Asynchronously projects with query syntax support (both selector and resultSelector are async).
    /// </summary>
    public static async Task<Result<T>> SelectManyAsync<S, I, T>(
        this Result<S> source,
        Func<S, Task<Result<I>>> selector,
        Func<S, I, Task<T>> resultSelector)
    {
        selector = selector.EnsureNotNull(nameof(selector));
        resultSelector = resultSelector.EnsureNotNull(nameof(resultSelector));

        if (source.IsFailed)
        {
            return new Result<T>(default, source.Reasons);
        }

        try
        {
            var intermediateResult = await selector(source.Value!);

            if (intermediateResult.IsFailed)
            {
                var combinedReasons = source.Errors.ToImmutableList<IReason>()
                    .AddRange(intermediateResult.Reasons);

                return new Result<T>(default, combinedReasons);
            }

            var finalValue = await resultSelector(source.Value!, intermediateResult.Value!);

            var allSuccesses = source.Successes.ToImmutableList<IReason>()
                .AddRange(intermediateResult.Successes);

            return new Result<T>(finalValue, allSuccesses);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Mixed async/sync SelectMany: sync selector, async resultSelector.
    /// </summary>
    public static async Task<Result<T>> SelectManyAsync<S, I, T>(
        this Result<S> source,
        Func<S, Result<I>> selector,
        Func<S, I, Task<T>> resultSelector)
    {
        selector = selector.EnsureNotNull(nameof(selector));
        resultSelector = resultSelector.EnsureNotNull(nameof(resultSelector));

        if (source.IsFailed)
        {
            return new Result<T>(default, source.Reasons);
        }

        try
        {
            var intermediateResult = selector(source.Value!);

            if (intermediateResult.IsFailed)
            {
                var combinedReasons = source.Errors.ToImmutableList<IReason>()
                    .AddRange(intermediateResult.Reasons);

                return new Result<T>(default, combinedReasons);
            }

            var finalValue = await resultSelector(source.Value!, intermediateResult.Value!);

            var allSuccesses = source.Successes.ToImmutableList<IReason>()
                .AddRange(intermediateResult.Successes);

            return new Result<T>(finalValue, allSuccesses);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Mixed async/sync SelectMany: async selector, sync resultSelector.
    /// </summary>
    public static async Task<Result<T>> SelectManyAsync<S, I, T>(
        this Result<S> source,
        Func<S, Task<Result<I>>> selector,
        Func<S, I, T> resultSelector)
    {
        selector = selector.EnsureNotNull(nameof(selector));
        resultSelector = resultSelector.EnsureNotNull(nameof(resultSelector));

        if (source.IsFailed)
        {
            return new Result<T>(default, source.Reasons);
        }

        try
        {
            var intermediateResult = await selector(source.Value!);

            if (intermediateResult.IsFailed)
            {
                var combinedReasons = source.Errors.ToImmutableList<IReason>()
                    .AddRange(intermediateResult.Reasons);

                return new Result<T>(default, combinedReasons);
            }

            var finalValue = resultSelector(source.Value!, intermediateResult.Value!);

            var allSuccesses = source.Successes.ToImmutableList<IReason>()
                .AddRange(intermediateResult.Successes);

            return new Result<T>(finalValue, allSuccesses);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new ExceptionError(ex));
        }
    }

    #endregion

    #region Select (Map equivalent)

    /// <summary>
    /// Projects each element of a Result into a new form. Equivalent to Map.
    /// Enables LINQ query syntax 'select' clause.
    /// </summary>
    /// <example>
    /// <code>
    /// // Query syntax
    /// var result = from x in Result&lt;int&gt;.Ok(5)
    ///              select x * 2;
    /// 
    /// // Method syntax
    /// var result = Result&lt;int&gt;.Ok(5).Select(x => x * 2);
    /// </code>
    /// </example>
    public static Result<T> Select<S, T>(
        this Result<S> source,
        Func<S, T> selector)
    {
        selector = selector.EnsureNotNull(nameof(selector));

        if (source.IsFailed)
        {
            return new Result<T>(default, source.Reasons);
        }

        try
        {
            var value = selector(source.Value!);

            // Preserve success reasons if any
            if (source.Successes.Count > 0)
            {
                return new Result<T>(value, source.Reasons);
            }

            return Result<T>.Ok(value);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Asynchronously projects each element of a Result into a new form.
    /// </summary>
    public static async Task<Result<T>> SelectAsync<S, T>(
        this Result<S> source,
        Func<S, Task<T>> selector)
    {
        selector = selector.EnsureNotNull(nameof(selector));

        if (source.IsFailed)
        {
            return new Result<T>(default, source.Reasons);
        }

        try
        {
            var value = await selector(source.Value!);

            if (source.Successes.Count > 0)
            {
                return new Result<T>(value, source.Reasons);
            }

            return Result<T>.Ok(value);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new ExceptionError(ex));
        }
    }

    #endregion

    #region Where (Filter)

    /// <summary>
    /// Filters a Result based on a predicate.
    /// If the predicate returns false, converts success to failure.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = from x in Result&lt;int&gt;.Ok(5)
    ///              where x > 0
    ///              select x * 2;
    /// </code>
    /// </example>
    public static Result<S> Where<S>(
        this Result<S> source,
        Func<S, bool> predicate)
    {
        predicate = predicate.EnsureNotNull(nameof(predicate));

        if (source.IsFailed)
        {
            return source;
        }

        try
        {
            return predicate(source.Value!)
                ? source
                : Result<S>.Fail("Predicate not satisfied");
        }
        catch (Exception ex)
        {
            return Result<S>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Filters a Result based on a predicate with custom error message.
    /// </summary>
    public static Result<S> Where<S>(
        this Result<S> source,
        Func<S, bool> predicate,
        string errorMessage)
    {
        predicate = predicate.EnsureNotNull(nameof(predicate));
        errorMessage = errorMessage.EnsureNotNullOrWhiteSpace(nameof(errorMessage));

        if (source.IsFailed)
        {
            return source;
        }

        try
        {
            return predicate(source.Value!)
                ? source
                : Result<S>.Fail(errorMessage);
        }
        catch (Exception ex)
        {
            return Result<S>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Asynchronously filters a Result based on a predicate.
    /// </summary>
    public static async Task<Result<S>> WhereAsync<S>(
        this Result<S> source,
        Func<S, Task<bool>> predicate)
    {
        predicate = predicate.EnsureNotNull(nameof(predicate));

        if (source.IsFailed)
        {
            return source;
        }

        try
        {
            var satisfiesPredicate = await predicate(source.Value!);
            return satisfiesPredicate
                ? source
                : Result<S>.Fail("Predicate not satisfied");
        }
        catch (Exception ex)
        {
            return Result<S>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Asynchronously filters a Result based on a predicate with custom error message.
    /// </summary>
    public static async Task<Result<S>> WhereAsync<S>(
        this Result<S> source,
        Func<S, Task<bool>> predicate,
        string errorMessage)
    {
        predicate = predicate.EnsureNotNull(nameof(predicate));
        errorMessage = errorMessage.EnsureNotNullOrWhiteSpace(nameof(errorMessage));

        if (source.IsFailed)
        {
            return source;
        }

        try
        {
            var satisfiesPredicate = await predicate(source.Value!);
            return satisfiesPredicate
                ? source
                : Result<S>.Fail(errorMessage);
        }
        catch (Exception ex)
        {
            return Result<S>.Fail(new ExceptionError(ex));
        }
    }       

    #endregion

    #region Task<Result<T>> Extensions for LINQ

    /// <summary>
    /// Awaits the result then filters based on a predicate.
    /// </summary>
    public static async Task<Result<S>> WhereAsync<S>(
        this Task<Result<S>> resultTask,
        Func<S, bool> predicate,
        string errorMessage)
    {
        var result = await resultTask;
        return result.Where(predicate, errorMessage);
    }

    /// <summary>
    /// Awaits the result then filters based on an async predicate.
    /// </summary>
    public static async Task<Result<S>> WhereAsync<S>(
        this Task<Result<S>> resultTask,
        Func<S, Task<bool>> predicate,
        string errorMessage)
    {
        var result = await resultTask;
        return await result.WhereAsync(predicate, errorMessage);
    }

    /// <summary>
    /// Awaits the result then projects to a new value.
    /// </summary>
    public static async Task<Result<T>> SelectAsync<S, T>(
        this Task<Result<S>> resultTask,
        Func<S, T> selector)
    {
        var result = await resultTask;
        return result.Select(selector);
    }

    /// <summary>
    /// Awaits the result then projects to a new value asynchronously.
    /// </summary>
    public static async Task<Result<T>> SelectAsync<S, T>(
        this Task<Result<S>> resultTask,
        Func<S, Task<T>> selector)
    {
        var result = await resultTask;
        return await result.SelectAsync(selector);
    }

    /// <summary>
    /// Awaits the result then projects each element into a new Result.
    /// </summary>
    public static async Task<Result<T>> SelectManyAsync<S, T>(
        this Task<Result<S>> resultTask,
        Func<S, Result<T>> selector)
    {
        var result = await resultTask;
        return result.SelectMany(selector);
    }

    /// <summary>
    /// Awaits the result then asynchronously projects each element into a new Result.
    /// </summary>
    public static async Task<Result<T>> SelectManyAsync<S, T>(
        this Task<Result<S>> resultTask,
        Func<S, Task<Result<T>>> selector)
    {
        var result = await resultTask;
        return await result.SelectManyAsync(selector);
    }

    /// <summary>
    /// Awaits the result then projects with query syntax support (sync selector, sync resultSelector).
    /// </summary>
    public static async Task<Result<T>> SelectManyAsync<S, I, T>(
        this Task<Result<S>> resultTask,
        Func<S, Result<I>> selector,
        Func<S, I, T> resultSelector)
    {
        var result = await resultTask;
        return result.SelectMany(selector, resultSelector);
    }

    /// <summary>
    /// Awaits the result then projects with query syntax support (async selector, sync resultSelector).
    /// </summary>
    public static async Task<Result<T>> SelectManyAsync<S, I, T>(
        this Task<Result<S>> resultTask,
        Func<S, Task<Result<I>>> selector,
        Func<S, I, T> resultSelector)
    {
        var result = await resultTask;
        return await result.SelectManyAsync(selector, resultSelector);
    }

    /// <summary>
    /// Awaits the result then projects with query syntax support (sync selector, async resultSelector).
    /// </summary>
    public static async Task<Result<T>> SelectManyAsync<S, I, T>(
        this Task<Result<S>> resultTask,
        Func<S, Result<I>> selector,
        Func<S, I, Task<T>> resultSelector)
    {
        var result = await resultTask;
        return await result.SelectManyAsync(selector, resultSelector);
    }

    /// <summary>
    /// Awaits the result then projects with query syntax support (both selector and resultSelector are async).
    /// </summary>
    public static async Task<Result<T>> SelectManyAsync<S, I, T>(
        this Task<Result<S>> resultTask,
        Func<S, Task<Result<I>>> selector,
        Func<S, I, Task<T>> resultSelector)
    {
        var result = await resultTask;
        return await result.SelectManyAsync(selector, resultSelector);
    }

    #endregion
}
