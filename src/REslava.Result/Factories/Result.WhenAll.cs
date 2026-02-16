using System.Collections.Immutable;

namespace REslava.Result;

public partial class Result
{
    /// <summary>
    /// Runs two async Result operations concurrently and returns a typed tuple.
    /// If all succeed, returns Ok with the tuple of values.
    /// If any fail, aggregates all errors from all failed results.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = await Result.WhenAll(GetUser(id), GetAccount(id));
    /// if (result.IsSuccess)
    /// {
    ///     var (user, account) = result.Value;
    /// }
    /// </code>
    /// </example>
    public static async Task<Result<(T1, T2)>> WhenAll<T1, T2>(
        Task<Result<T1>> task1,
        Task<Result<T2>> task2)
    {
        task1 = task1.EnsureNotNull(nameof(task1));
        task2 = task2.EnsureNotNull(nameof(task2));

        try
        {
            await Task.WhenAll(task1, task2);
        }
        catch
        {
            // Individual task exceptions are inspected below
        }

        var result1 = GetSafeResult(task1);
        var result2 = GetSafeResult(task2);

        var errors = CollectErrors(result1, result2);
        if (errors.Count > 0)
            return Result<(T1, T2)>.Fail(errors);

        return Result<(T1, T2)>.Ok((result1.Value!, result2.Value!));
    }

    /// <summary>
    /// Runs three async Result operations concurrently and returns a typed tuple.
    /// </summary>
    public static async Task<Result<(T1, T2, T3)>> WhenAll<T1, T2, T3>(
        Task<Result<T1>> task1,
        Task<Result<T2>> task2,
        Task<Result<T3>> task3)
    {
        task1 = task1.EnsureNotNull(nameof(task1));
        task2 = task2.EnsureNotNull(nameof(task2));
        task3 = task3.EnsureNotNull(nameof(task3));

        try
        {
            await Task.WhenAll(task1, task2, task3);
        }
        catch
        {
            // Individual task exceptions are inspected below
        }

        var result1 = GetSafeResult(task1);
        var result2 = GetSafeResult(task2);
        var result3 = GetSafeResult(task3);

        var errors = CollectErrors(result1, result2, result3);
        if (errors.Count > 0)
            return Result<(T1, T2, T3)>.Fail(errors);

        return Result<(T1, T2, T3)>.Ok((result1.Value!, result2.Value!, result3.Value!));
    }

    /// <summary>
    /// Runs four async Result operations concurrently and returns a typed tuple.
    /// </summary>
    public static async Task<Result<(T1, T2, T3, T4)>> WhenAll<T1, T2, T3, T4>(
        Task<Result<T1>> task1,
        Task<Result<T2>> task2,
        Task<Result<T3>> task3,
        Task<Result<T4>> task4)
    {
        task1 = task1.EnsureNotNull(nameof(task1));
        task2 = task2.EnsureNotNull(nameof(task2));
        task3 = task3.EnsureNotNull(nameof(task3));
        task4 = task4.EnsureNotNull(nameof(task4));

        try
        {
            await Task.WhenAll(task1, task2, task3, task4);
        }
        catch
        {
            // Individual task exceptions are inspected below
        }

        var result1 = GetSafeResult(task1);
        var result2 = GetSafeResult(task2);
        var result3 = GetSafeResult(task3);
        var result4 = GetSafeResult(task4);

        var errors = CollectErrors(result1, result2, result3, result4);
        if (errors.Count > 0)
            return Result<(T1, T2, T3, T4)>.Fail(errors);

        return Result<(T1, T2, T3, T4)>.Ok((result1.Value!, result2.Value!, result3.Value!, result4.Value!));
    }

    /// <summary>
    /// Runs multiple async Result operations concurrently and returns an ImmutableList of values.
    /// If all succeed, returns Ok with the list of values.
    /// If any fail, aggregates all errors from all failed results.
    /// </summary>
    /// <example>
    /// <code>
    /// var tasks = userIds.Select(id => GetUser(id));
    /// var result = await Result.WhenAll(tasks);
    /// </code>
    /// </example>
    public static async Task<Result<ImmutableList<T>>> WhenAll<T>(
        IEnumerable<Task<Result<T>>> tasks)
    {
        tasks = tasks.EnsureNotNull(nameof(tasks));

        var taskList = tasks.ToList();
        if (taskList.Count == 0)
            return Result<ImmutableList<T>>.Ok(ImmutableList<T>.Empty);

        try
        {
            await Task.WhenAll(taskList);
        }
        catch
        {
            // Individual task exceptions are inspected below
        }

        var results = taskList.Select(GetSafeResult).ToList();
        var errors = results.Where(r => r.IsFailed).SelectMany(r => r.Errors).ToList();

        if (errors.Count > 0)
            return Result<ImmutableList<T>>.Fail(errors);

        var values = results.Select(r => r.Value!).ToImmutableList();
        return Result<ImmutableList<T>>.Ok(values);
    }

    /// <summary>
    /// Runs multiple async Result operations concurrently (params variant).
    /// </summary>
    public static Task<Result<ImmutableList<T>>> WhenAll<T>(
        params Task<Result<T>>[] tasks)
    {
        return WhenAll(tasks.AsEnumerable());
    }

    private static Result<T> GetSafeResult<T>(Task<Result<T>> task)
    {
        if (task.IsFaulted)
            return Result<T>.Fail(new ExceptionError(task.Exception!.InnerException ?? task.Exception));
        if (task.IsCanceled)
            return Result<T>.Fail(new ExceptionError(new OperationCanceledException()));
        return task.Result;
    }

    private static List<IError> CollectErrors(params Result[] results)
    {
        return results
            .Where(r => r.IsFailed)
            .SelectMany(r => r.Errors)
            .ToList();
    }
}
