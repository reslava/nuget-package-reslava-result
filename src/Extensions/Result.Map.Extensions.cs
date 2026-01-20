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
    /// <example>
    /// <code>
    /// var result = await GetUserAsync(userId)
    ///     .Map(user => user.Name);
    /// </code>
    /// </example>
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
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="U">The target value type.</typeparam>
    /// <param name="resultTask">The task containing the result to map.</param>
    /// <param name="mapper">The async function to transform the value.</param>
    /// <returns>A task containing the mapped result.</returns>
    /// <example>
    /// <code>
    /// var result = await GetUserAsync(userId)
    ///     .MapAsync(async user => await GetUserProfileAsync(user.Id));
    /// </code>
    /// </example>
    public static async Task<Result<U>> MapAsync<T, U>(
        this Task<Result<T>> resultTask,
        Func<T, Task<U>> mapper)
    {
        ArgumentNullException.ThrowIfNull(resultTask, nameof(resultTask));
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));

        var result = await resultTask;
        return await result.MapAsync(mapper);
    }

    /// <summary>
    /// Adds a success reason to a Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="resultTask">The task containing the result to add success to.</param>
    /// <param name="message">The success message to add.</param>
    /// <returns>A task containing the result with the added success reason.</returns>
    /// <example>
    /// <code>
    /// var result = await SaveUserAsync(user)
    ///     .WithSuccess("User saved successfully");
    /// </code>
    /// </example>
    public static async Task<Result<T>> WithSuccess<T>(
        this Task<Result<T>> resultTask,
        string message)
    {
        ArgumentNullException.ThrowIfNull(resultTask, nameof(resultTask));
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));

        var result = await resultTask;
        return result.WithSuccess(message);
    }

    /// <summary>
    /// Adds a success reason to a Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="resultTask">The task containing the result to add success to.</param>
    /// <param name="success">The success reason to add.</param>
    /// <returns>A task containing the result with the added success reason.</returns>
    /// <example>
    /// <code>
    /// var success = new Success("User created");
    /// var result = await CreateUserAsync(user)
    ///     .WithSuccess(success);
    /// </code>
    /// </example>
    public static async Task<Result<T>> WithSuccess<T>(
        this Task<Result<T>> resultTask,
        ISuccess success)
    {
        ArgumentNullException.ThrowIfNull(resultTask, nameof(resultTask));
        ArgumentNullException.ThrowIfNull(success, nameof(success));

        var result = await resultTask;
        return result.WithSuccess(success);
    }
}
