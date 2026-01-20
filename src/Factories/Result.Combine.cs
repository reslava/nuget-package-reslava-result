using System.Collections.Immutable;

namespace REslava.Result;

public partial class Result 
{
    #region Merge - Combines all reasons (success or failure)
    
    /// <summary>
    /// Merges multiple results, preserving all reasons.
    /// Returns failed if ANY result is failed, success only if ALL succeeded.
    /// </summary>
    /// <example>
    /// <code>
    /// var results = new[] { 
    ///     Result.Ok().WithSuccess("Step 1"),
    ///     Result.Ok().WithSuccess("Step 2"),
    ///     Result.Fail("Error in step 3")
    /// };
    /// var merged = Result.Merge(results);
    /// // merged.IsFailed == true
    /// // merged.Errors contains "Error in step 3"
    /// // merged.Successes contains "Step 1", "Step 2"
    /// </code>
    /// </example>
    public static Result Merge(IEnumerable<Result> results)
    {
        results = results.EnsureNotNull(nameof(results));
        
        var resultsList = results.ToList();
        if (resultsList.Count == 0)
        {
            return Result.Ok();
        }

        // ✅ Collect all reasons immutably
        var allReasons = resultsList
            .SelectMany(r => r.Reasons)
            .ToImmutableList();

        // ✅ Create new result with combined reasons (immutable)
        return new Result(allReasons);
    }

    /// <summary>
    /// Merges multiple results with params syntax.
    /// </summary>
    public static Result Merge(params Result[] results)
    {
        return Merge(results.AsEnumerable());
    }

    #endregion

    #region Combine - Requires all to succeed

    /// <summary>
    /// Combines results - ALL must succeed for combined result to succeed.
    /// If any fail, returns failed result with all errors.
    /// Only preserves success reasons if ALL succeeded.
    /// </summary>
    /// <example>
    /// <code>
    /// var validation = Result.Combine(
    ///     ValidateEmail(email),
    ///     ValidateAge(age),
    ///     ValidateName(name)
    /// );
    /// </code>
    /// </example>
    public static Result Combine(IEnumerable<Result> results)
    {
        results = results.EnsureNotNull(nameof(results));
        
        var resultsList = results.ToList();
        if (resultsList.Count == 0)
        {
            return Result.Ok();
        }

        var failures = resultsList.Where(r => r.IsFailed).ToList();
        
        if (failures.Count > 0)
        {
            // ✅ Return failed result with all errors (immutable)
            var allErrors = failures.SelectMany(f => f.Errors);
            return Result.Fail(allErrors);
        }

        // ✅ All succeeded - collect and preserve all success reasons (immutable)
        var allSuccesses = resultsList
            .SelectMany(r => r.Successes)
            .ToImmutableList<IReason>();

        if (allSuccesses.Count > 0)
        {
            return new Result(allSuccesses);
        }

        return Result.Ok();
    }

    /// <summary>
    /// Combines results with params syntax.
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        return Combine(results.AsEnumerable());
    }

    #endregion

    #region CombineParallel - For async parallel operations

    /// <summary>
    /// Combines results from parallel async operations.
    /// Waits for all tasks even if some fail.
    /// </summary>
    public static async Task<Result> CombineParallelAsync(
        IEnumerable<Task<Result>> resultTasks)
    {
        resultTasks = resultTasks.EnsureNotNull(nameof(resultTasks));
        
        var tasks = resultTasks.ToList();
        if (tasks.Count == 0)
        {
            return Result.Ok();
        }

        // Wait for all tasks to complete
        var results = await Task.WhenAll(tasks);
        
        return Combine(results);
    }

    #endregion
}

public partial class Result<TValue>
{
    #region Combine with values

    /// <summary>
    /// Combines multiple Result&lt;T&gt; - ALL must succeed.
    /// Returns Result&lt;IEnumerable&lt;T&gt;&gt; with all values if all succeeded.
    /// </summary>
    /// <example>
    /// <code>
    /// var users = Result&lt;User&gt;.Combine(
    ///     GetUser(id1),
    ///     GetUser(id2),
    ///     GetUser(id3)
    /// );
    /// // users: Result&lt;IEnumerable&lt;User&gt;&gt;
    /// </code>
    /// </example>
    public static Result<IEnumerable<TValue>> Combine(
        IEnumerable<Result<TValue>> results)
    {
        results = results.EnsureNotNull(nameof(results));
        
        var resultsList = results.ToList();
        if (resultsList.Count == 0)
        {
            return Result<IEnumerable<TValue>>.Ok(Enumerable.Empty<TValue>());
        }

        var failures = resultsList.Where(r => r.IsFailed).ToList();
        
        if (failures.Count > 0)
        {
            // ✅ Return failed result with all errors (immutable)
            var allErrors = failures.SelectMany(f => f.Errors);
            return Result<IEnumerable<TValue>>.Fail(allErrors);
        }

        // ✅ All succeeded - collect values and successes (immutable)
        var values = resultsList.Select(r => r.Value);
        
        var allSuccesses = resultsList
            .SelectMany(r => r.Successes)
            .ToImmutableList<IReason>();

        // Create result with values and all success reasons
        if (allSuccesses.Count > 0)
        {
            return new Result<IEnumerable<TValue>>(values, allSuccesses);
        }

        return Result<IEnumerable<TValue>>.Ok(values);
    }

    /// <summary>
    /// Combines results with params syntax.
    /// </summary>
    public static Result<IEnumerable<TValue>> Combine(
        params Result<TValue>[] results)
    {
        return Combine(results.AsEnumerable());
    }

    /// <summary>
    /// Combines results from parallel async operations.
    /// </summary>
    public static async Task<Result<IEnumerable<TValue>>> CombineParallelAsync(
        IEnumerable<Task<Result<TValue>>> resultTasks)
    {
        resultTasks = resultTasks.EnsureNotNull(nameof(resultTasks));
        
        var tasks = resultTasks.ToList();
        if (tasks.Count == 0)
        {
            return Result<IEnumerable<TValue>>.Ok(Enumerable.Empty<TValue>());
        }

        var results = await Task.WhenAll(tasks);
        return Combine(results);
    }

    #endregion
}