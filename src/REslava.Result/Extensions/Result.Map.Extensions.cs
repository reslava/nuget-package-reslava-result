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
    /// Asynchronously maps the value inside a Task&lt;Result&lt;T&gt;&gt; to a new value of type U.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="U">The target value type.</typeparam>
    /// <param name="resultTask">The task containing the result to map.</param>
    /// <param name="mapper">The function to transform the value.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task containing the mapped result.</returns>
    /// <example>
    /// <code>
    /// var result = await GetUserAsync(userId)
    ///     .MapAsync(user => user.Name);
    /// </code>
    /// </example>
    public static async Task<Result<U>> MapAsync<T, U>(
        this Task<Result<T>> resultTask,
        Func<T, U> mapper,
        CancellationToken cancellationToken = default)
    {
        resultTask = resultTask.EnsureNotNull(nameof(resultTask));
        mapper = mapper.EnsureNotNull(nameof(mapper));
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask;

        try
        {
            return result.Map(mapper);
        }
        catch (Exception ex)
        {
            return Result<U>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Asynchronously maps the value inside a Task&lt;Result&lt;T&gt;&gt; to a new value.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="U">The target value type.</typeparam>
    /// <param name="resultTask">The task containing the result to map.</param>
    /// <param name="mapper">The async function to transform the value.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task containing the mapped result.</returns>
    /// <example>
    /// <code>
    /// var result = await GetUserAsync(userId)
    ///     .MapAsync(async user => await GetUserProfileAsync(user.Id));
    /// </code>
    /// </example>
    public static async Task<Result<U>> MapAsync<T, U>(
        this Task<Result<T>> resultTask,
        Func<T, Task<U>> mapper,
        CancellationToken cancellationToken = default)
    {
        resultTask = resultTask.EnsureNotNull(nameof(resultTask));
        mapper = mapper.EnsureNotNull(nameof(mapper));
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask;

        try
        {
            return await result.MapAsync(mapper, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<U>.Fail(new ExceptionError(ex));
        }
    }

    /// <summary>
    /// Asynchronously adds a success reason to a Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="resultTask">The task containing the result to add success to.</param>
    /// <param name="message">The success message to add.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task containing the result with the added success reason.</returns>
    /// <example>
    /// <code>
    /// var result = await SaveUserAsync(user)
    ///     .WithSuccessAsync("User saved successfully");
    /// </code>
    /// </example>
    public static async Task<Result<T>> WithSuccessAsync<T>(
        this Task<Result<T>> resultTask,
        string message,
        CancellationToken cancellationToken = default)
    {
        resultTask = resultTask.EnsureNotNull(nameof(resultTask));
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask;
        return result.WithSuccess(message);
    }

    /// <summary>
    /// Asynchronously adds a success reason to a Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="resultTask">The task containing the result to add success to.</param>
    /// <param name="success">The success reason to add.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task containing the result with the added success reason.</returns>
    /// <example>
    /// <code>
    /// var success = new Success("User created");
    /// var result = await CreateUserAsync(user)
    ///     .WithSuccessAsync(success);
    /// </code>
    /// </example>
    public static async Task<Result<T>> WithSuccessAsync<T>(
        this Task<Result<T>> resultTask,
        ISuccess success,
        CancellationToken cancellationToken = default)
    {
        resultTask = resultTask.EnsureNotNull(nameof(resultTask));
        success = success.EnsureNotNull(nameof(success));
        cancellationToken.ThrowIfCancellationRequested();

        var result = await resultTask;
        return result.WithSuccess(success);
    }
}
