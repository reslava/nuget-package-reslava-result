using System;
using System.Collections.Generic;
using System.Text;

namespace REslava.Result.Extensions;

/// <summary>
/// Extension methods for binding operations on Result types.
/// </summary>
public static class ResultBindExtensions
{
    /// <summary>
    /// Chains async operations on Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="U">The target value type.</typeparam>
    /// <param name="resultTask">The task containing the result to bind.</param>
    /// <param name="binder">The function that returns a new result.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task containing the bound result.</returns>
    /// <example>
    /// <code>
    /// var result = await GetUserAsync(userId)
    ///     .BindAsync(user => SaveUserAsync(user));
    /// </code>
    /// </example>
    public static async Task<Result<U>> BindAsync<T, U>(
            this Task<Result<T>> resultTask,
            Func<T, Task<Result<U>>> binder,
            CancellationToken cancellationToken = default)
    {
        resultTask = resultTask.EnsureNotNull(nameof(resultTask));
        binder = binder.EnsureNotNull(nameof(binder));
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask;
        return await result.BindAsync(binder, cancellationToken);
    }
}
