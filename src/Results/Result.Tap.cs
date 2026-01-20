using System;
using System.Collections.Generic;
using System.Text;

namespace REslava.Result;

public partial class Result
{    
    /// <summary>
    /// Executes a side effect without modifying the result.
    /// Useful for logging, telemetry, or other side effects that shouldn't affect the result flow.
    /// </summary>
    /// <param name="action">The action to execute if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    /// <example>
    /// <code>
    /// var result = Result.Ok()
    ///     .Tap(() => Console.WriteLine("Operation completed successfully"));
    /// </code>
    /// </example>
    public Result Tap(Action action)
    {
        action = action.EnsureNotNull(nameof(action));
        if (IsSuccess)
        {
            action();
        }
        return this;
    }
    /// <summary>
    /// Executes a side effect asynchronously without modifying the result.
    /// Useful for async logging, telemetry, or other side effects that shouldn't affect the result flow.
    /// </summary>
    /// <param name="action">The async action to execute if the result is successful.</param>
    /// <returns>A task containing the original result unchanged.</returns>
    /// <example>
    /// <code>
    /// var result = await Result.Ok()
    ///     .TapAsync(async () => await logger.LogAsync("Operation completed"));
    /// </code>
    /// </example>
    public async Task<Result> TapAsync(Func<Task> action)
    {
        action = action.EnsureNotNull(nameof(action));
        if (IsSuccess)
        {
            await action();
        }
        return this;
    } 
}
public partial class Result<TValue>
{    
    /// <summary>
    /// Executes a side effect with the value without modifying the result.
    /// Useful for logging the value, telemetry, or other side effects that shouldn't affect the result flow.
    /// </summary>
    /// <param name="action">The action to execute with the value if the result is successful.</param>
    /// <returns>The original result unchanged.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;User&gt;.Ok(user)
    ///     .Tap(u => Console.WriteLine($"User created: {u.Name}"));
    /// </code>
    /// </example>
    public Result<TValue> Tap(Action<TValue> action)
    {
        if (IsSuccess)
        {
            action(Value!);
        }
        return this;
    }

    /// <summary>
    /// Executes an async side effect with the value without modifying the result.
    /// Useful for async logging the value, telemetry, or other side effects that shouldn't affect the result flow.
    /// </summary>
    /// <param name="action">The async action to execute with the value if the result is successful.</param>
    /// <returns>A task containing the original result unchanged.</returns>
    /// <example>
    /// <code>
    /// var result = await Result&lt;User&gt;.Ok(user)
    ///     .TapAsync(async u => await logger.LogAsync($"User created: {u.Name}"));
    /// </code>
    /// </example>
    public async Task<Result<TValue>> TapAsync(Func<TValue, Task> action)
    {
        if (IsSuccess)
        {
            await action(Value!);
        }
        return this;
    }    
}
