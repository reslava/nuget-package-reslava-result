using System;
using System.Collections.Generic;
using System.Text;

namespace REslava.Result.Extensions;

/// <summary>
/// Extension methods for working with Task&lt;Result&lt;T&gt;&gt;.
/// </summary>
public static class ResultTaskExtensions
{
    /// <summary>
    /// Maps the value inside a Task&lt;Result&lt;T&gt;&gt; to a new value of type U.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="U">The target value type.</typeparam>
    /// <param name="resultTask">The task containing the result to map.</param>
    /// <param name="mapper">The function to transform the value.</param>
    /// <returns>A task containing the mapped result.</returns>
    public static async Task<Result<U>> Map<T, U>(
        this Task<Result<T>> resultTask,
        Func<T, U> mapper)
    {
        ArgumentNullException.ThrowIfNull(resultTask, nameof(resultTask));
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));

        var result = await resultTask;
        return result.Map(mapper);
    }

    /// <summary>
    /// Asynchronously maps the value inside a Task&lt;Result&lt;T&gt;&gt; to a new value.
    /// </summary>
    public static async Task<Result<U>> MapAsync<T, U>(
        this Task<Result<T>> resultTask,
        Func<T, Task<U>> mapper)
    {
        ArgumentNullException.ThrowIfNull(resultTask, nameof(resultTask));
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));

        var result = await resultTask;
        return await result.MapAsync(mapper);
    }
}
