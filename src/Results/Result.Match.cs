using System;
using System.Collections.Generic;

namespace REslava.Result;

public partial class Result : IResult
{
    /// <summary>
    /// Matches the result to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure.</param>
    /// <returns>The result of the executed function.</returns>
    public TOut Match<TOut>(Func<TOut> onSuccess, Func<IReadOnlyList<IError>, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess ? onSuccess() : onFailure(Errors);
    }

    /// <summary>
    /// Executes one of two actions based on success or failure.
    /// </summary>
    /// <param name="onSuccess">Action to execute on success.</param>
    /// <param name="onFailure">Action to execute on failure.</param>
    public void Match(Action onSuccess, Action<IReadOnlyList<IError>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (IsSuccess)
            onSuccess();
        else
            onFailure(Errors);
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
    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<IReadOnlyList<IError>, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess ? onSuccess(ValueOrDefault!) : onFailure(Errors);
    }

    /// <summary>
    /// Executes one of two actions based on success or failure.
    /// </summary>
    /// <param name="onSuccess">Action to execute on success.</param>
    /// <param name="onFailure">Action to execute on failure.</param>
    public void Match(Action<TValue> onSuccess, Action<IReadOnlyList<IError>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (IsSuccess)
            onSuccess(ValueOrDefault!);
        else
            onFailure(Errors);
    }
}
