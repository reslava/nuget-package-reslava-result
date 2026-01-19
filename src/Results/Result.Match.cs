using System.Collections.Immutable;

namespace REslava.Result;

public partial class Result : IResult
{
    /// <summary>
    /// Matches the result to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure with errors.</param>
    /// <returns>The result of the executed function.</returns>
    public TOut Match<TOut>(
        Func<TOut> onSuccess, 
        Func<ImmutableList<IError>, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
        ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));

        return IsSuccess ? onSuccess() : onFailure(Errors);
    }

    /// <summary>
    /// Executes one of two actions based on success or failure.
    /// </summary>
    /// <param name="onSuccess">Action to execute on success.</param>
    /// <param name="onFailure">Action to execute on failure with errors.</param>
    public void Match(
        Action onSuccess, 
        Action<ImmutableList<IError>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
        ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));

        if (IsSuccess)
        { 
            onSuccess();
        }
        else
        { 
            onFailure(Errors);
        }
    }

    /// <summary>
    /// Asynchronously matches the result to one of two async functions based on success or failure.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="onSuccess">Async function to execute on success.</param>
    /// <param name="onFailure">Async function to execute on failure with errors.</param>
    /// <returns>A task containing the result of the executed function.</returns>
    public async Task<TOut> MatchAsync<TOut>(
        Func<Task<TOut>> onSuccess,
        Func<ImmutableList<IError>, Task<TOut>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
        ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));

        return IsSuccess 
            ? await onSuccess() 
            : await onFailure(Errors);
    }

    /// <summary>
    /// Asynchronously executes one of two async actions based on success or failure.
    /// </summary>
    /// <param name="onSuccess">Async action to execute on success.</param>
    /// <param name="onFailure">Async action to execute on failure with errors.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task MatchAsync(
        Func<Task> onSuccess,
        Func<ImmutableList<IError>, Task> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
        ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));

        if (IsSuccess)
        {
            await onSuccess();
        }
        else
        {
            await onFailure(Errors);
        }
    }
}

public partial class Result<TValue> : Result, IResult<TValue>
{
    /// <summary>
    /// Matches the result to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="onSuccess">Function to execute on success with the value.</param>
    /// <param name="onFailure">Function to execute on failure with errors.</param>
    /// <returns>The result of the executed function.</returns>
    public TOut Match<TOut>(
        Func<TValue, TOut> onSuccess, 
        Func<ImmutableList<IError>, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
        ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));

        // Use Value property (throws if failed) instead of ValueOrDefault with null-forgiving
        return IsSuccess ? onSuccess(Value!) : onFailure(Errors);
    }

    /// <summary>
    /// Executes one of two actions based on success or failure.
    /// </summary>
    /// <param name="onSuccess">Action to execute on success with the value.</param>
    /// <param name="onFailure">Action to execute on failure with errors.</param>
    public void Match(
        Action<TValue> onSuccess, 
        Action<ImmutableList<IError>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
        ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));

        if (IsSuccess)
        { 
            onSuccess(Value!);
        }
        else
        { 
            onFailure(Errors);
        }
    }
    

    /// <summary>
    /// Asynchronously matches the result to one of two async functions based on success or failure.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="onSuccess">Async function to execute on success with the value.</param>
    /// <param name="onFailure">Async function to execute on failure with errors.</param>
    /// <returns>A task containing the result of the executed function.</returns>
    public async Task<TOut> MatchAsync<TOut>(
        Func<TValue, Task<TOut>> onSuccess,
        Func<ImmutableList<IError>, Task<TOut>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
        ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));

        return IsSuccess 
            ? await onSuccess(Value!) 
            : await onFailure(Errors);
    }

    /// <summary>
    /// Asynchronously executes one of two async actions based on success or failure.
    /// </summary>
    /// <param name="onSuccess">Async action to execute on success with the value.</param>
    /// <param name="onFailure">Async action to execute on failure with errors.</param>
    /// <returns>A task representing the async operation.</returns>
    public async Task MatchAsync(
        Func<TValue, Task> onSuccess,
        Func<ImmutableList<IError>, Task> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess, nameof(onSuccess));
        ArgumentNullException.ThrowIfNull(onFailure, nameof(onFailure));

        if (IsSuccess)
        {
            await onSuccess(Value!);
        }
        else
        {
            await onFailure(Errors);
        }
    }    
}
