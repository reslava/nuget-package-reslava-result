namespace REslava.Result;

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
        ArgumentNullException.ThrowIfNull(selector);

        if (source.IsFailed)
        {
            var failedResult = new Result<T>();
            failedResult.Reasons.AddRange(source.Reasons);
            return failedResult;
        }

        try
        {
            var result = selector(source.Value!);
            
            // If result is successful and source had success reasons, preserve them
            if (result.IsSuccess && source.Successes.Any())
            {
                var newResult = Result<T>.Ok(result.Value!);
                newResult.Reasons.AddRange(source.Successes);
                newResult.Reasons.AddRange(result.Reasons);
                return newResult;
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
        ArgumentNullException.ThrowIfNull(selector);

        if (source.IsFailed)
        {
            var failedResult = new Result<T>();
            failedResult.Reasons.AddRange(source.Reasons);
            return failedResult;
        }

        try
        {
            var result = await selector(source.Value!);
            
            if (result.IsSuccess && source.Successes.Any())
            {
                var newResult = Result<T>.Ok(result.Value!);
                newResult.Reasons.AddRange(source.Successes);
                newResult.Reasons.AddRange(result.Reasons);
                return newResult;
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
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        if (source.IsFailed)
        {
            var failedResult = new Result<T>();
            failedResult.Reasons.AddRange(source.Reasons);
            return failedResult;
        }

        try
        {
            var intermediateResult = selector(source.Value!);

            if (intermediateResult.IsFailed)
            {
                var failedResult = new Result<T>();
                // Preserve any errors from source (shouldn't normally have any, but be safe)
                failedResult.Reasons.AddRange(source.Errors);
                failedResult.Reasons.AddRange(intermediateResult.Reasons);
                return failedResult;
            }

            // Apply final transformation
            var finalValue = resultSelector(source.Value!, intermediateResult.Value!);
            var successResult = Result<T>.Ok(finalValue);

            // Preserve all success reasons from the chain
            successResult.Reasons.AddRange(source.Successes);
            successResult.Reasons.AddRange(intermediateResult.Successes);

            return successResult;
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Asynchronously projects with query syntax support (both selector and resultSelector are async).
    /// </summary>
    public static async Task<Result<T>> SelectMany<S, I, T>(
        this Result<S> source,
        Func<S, Task<Result<I>>> selector,
        Func<S, I, Task<T>> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        if (source.IsFailed)
        {
            var failedResult = new Result<T>();
            failedResult.Reasons.AddRange(source.Reasons);
            return failedResult;
        }

        try
        {
            var intermediateResult = await selector(source.Value!);

            if (intermediateResult.IsFailed)
            {
                var failedResult = new Result<T>();
                failedResult.Reasons.AddRange(source.Errors);
                failedResult.Reasons.AddRange(intermediateResult.Reasons);
                return failedResult;
            }

            var finalValue = await resultSelector(source.Value!, intermediateResult.Value!);
            var successResult = Result<T>.Ok(finalValue);

            successResult.Reasons.AddRange(source.Successes);
            successResult.Reasons.AddRange(intermediateResult.Successes);

            return successResult;
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Mixed async/sync SelectMany: sync selector, async resultSelector.
    /// </summary>
    public static async Task<Result<T>> SelectMany<S, I, T>(
        this Result<S> source,
        Func<S, Result<I>> selector,
        Func<S, I, Task<T>> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        if (source.IsFailed)
        {
            var failedResult = new Result<T>();
            failedResult.Reasons.AddRange(source.Reasons);
            return failedResult;
        }

        try
        {
            var intermediateResult = selector(source.Value!);

            if (intermediateResult.IsFailed)
            {
                var failedResult = new Result<T>();
                failedResult.Reasons.AddRange(source.Errors);
                failedResult.Reasons.AddRange(intermediateResult.Reasons);
                return failedResult;
            }

            var finalValue = await resultSelector(source.Value!, intermediateResult.Value!);
            var successResult = Result<T>.Ok(finalValue);

            successResult.Reasons.AddRange(source.Successes);
            successResult.Reasons.AddRange(intermediateResult.Successes);

            return successResult;
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Mixed async/sync SelectMany: async selector, sync resultSelector.
    /// </summary>
    public static async Task<Result<T>> SelectMany<S, I, T>(
        this Result<S> source,
        Func<S, Task<Result<I>>> selector,
        Func<S, I, T> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        if (source.IsFailed)
        {
            var failedResult = new Result<T>();
            failedResult.Reasons.AddRange(source.Reasons);
            return failedResult;
        }

        try
        {
            var intermediateResult = await selector(source.Value!);

            if (intermediateResult.IsFailed)
            {
                var failedResult = new Result<T>();
                failedResult.Reasons.AddRange(source.Errors);
                failedResult.Reasons.AddRange(intermediateResult.Reasons);
                return failedResult;
            }

            var finalValue = resultSelector(source.Value!, intermediateResult.Value!);
            var successResult = Result<T>.Ok(finalValue);

            successResult.Reasons.AddRange(source.Successes);
            successResult.Reasons.AddRange(intermediateResult.Successes);

            return successResult;
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
        ArgumentNullException.ThrowIfNull(selector);

        if (source.IsFailed)
        {
            var failedResult = new Result<T>();
            failedResult.Reasons.AddRange(source.Reasons);
            return failedResult;
        }

        try
        {
            var value = selector(source.Value!);
            var result = Result<T>.Ok(value);
            
            // Preserve success reasons
            if (source.Successes.Any())
            {
                result.Reasons.AddRange(source.Successes);
            }
            
            return result;
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
        ArgumentNullException.ThrowIfNull(selector);

        if (source.IsFailed)
        {
            var failedResult = new Result<T>();
            failedResult.Reasons.AddRange(source.Reasons);
            return failedResult;
        }

        try
        {
            var value = await selector(source.Value!);
            var result = Result<T>.Ok(value);
            
            if (source.Successes.Any())
            {
                result.Reasons.AddRange(source.Successes);
            }
            
            return result;
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
        ArgumentNullException.ThrowIfNull(predicate);

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
        ArgumentNullException.ThrowIfNull(predicate);

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
        ArgumentNullException.ThrowIfNull(predicate);

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

    #endregion
}