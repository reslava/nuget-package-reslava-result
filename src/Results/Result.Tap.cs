using System;
using System.Collections.Generic;
using System.Text;

namespace REslava.Result;

public partial class Result
{    
    /// <summary>
    /// Executes a side effect without modifying the result.
    /// </summary>    
    public Result Tap(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (IsSuccess)
        {
            action();
        }
        return this;
    }
    /// <summary>
    /// Executes a side effect asynchronously without modifying the result.
    /// </summary>
    public async Task<Result> TapAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
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
    /// Executes a side effect without modifying the result.
    /// </summary>
    public Result<TValue> Tap<T>(Action<TValue> action)
    {
        if (IsSuccess)
        {
            action(Value!);
        }
        return this;
    }

    /// <summary>
    /// Executes an async side effect without modifying the result.
    /// </summary>
    public async Task<Result<TValue>> TapAsync<T>(Func<TValue, Task> action)
    {
        if (IsSuccess)
        {
            await action(Value!);
        }
        return this;
    }    
}
