using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using REslava.Result.Observers;

namespace REslava.Result.Extensions;

/// <summary>
/// Extension methods for tap operations on Result types.
/// </summary>
public static class ResultExtensions
{
    #region Tap - Non-Generic Result

    /// <summary>
    /// Awaits the result then executes a side effect without modifying it.
    /// </summary>
    public static async Task<Result> TapAsync(this Task<Result> resultTask, Action action, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask;
        if (result.IsSuccess)
        {
            action();
        }
        return result;
    }

    /// <summary>
    /// Awaits the result then executes an async side effect without modifying it.
    /// </summary>
    public static async Task TapAsync(this Task<Result> resultTask, Func<Task> action, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask;
        if (result.IsSuccess)
        {
            await action();
        }
    }

    #endregion

    #region Tap - Generic Result<T>

    /// <summary>
    /// Awaits the result then executes a side effect without modifying it.
    /// </summary>
    /// <param name="resultTask">The task returning the result.</param>
    /// <param name="action">The action to execute with the value if the result is successful.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Action<T> action,
        CancellationToken cancellationToken = default,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        var result = await resultTask;

        if (result.IsSuccess)
        {
            action(result.Value!);
        }

        if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "TapAsync",
                IsSuccess: result.IsSuccess, OutputValue: null,
                ErrorType: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].GetType().Name : null,
                ErrorMessage: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].Message : null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
        }

        return result;
    }

    /// <summary>
    /// Awaits the result then executes an async side effect without modifying it.
    /// </summary>
    /// <param name="resultTask">The task returning the result.</param>
    /// <param name="action">The async action to execute with the value if the result is successful.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task> action,
        CancellationToken cancellationToken = default,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        var result = await resultTask;

        if (result.IsSuccess)
        {
            await action(result.Value!);
        }

        if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "TapAsync",
                IsSuccess: result.IsSuccess, OutputValue: null,
                ErrorType: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].GetType().Name : null,
                ErrorMessage: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].Message : null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
        }

        return result;
    }

    /// <summary>
    /// Executes a side effect when the result is failed, without modifying the result.
    /// </summary>
    /// <param name="result">The result to inspect.</param>
    /// <param name="action">The action to execute with the first error if the result is failed.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    public static Result<T> TapOnFailure<T>(
        this Result<T> result,
        Action<IError> action,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        if (result.IsFailure)
        {
            action(result.Errors[0]);
        }

        if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "TapOnFailure",
                IsSuccess: result.IsSuccess, OutputValue: null,
                ErrorType: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].GetType().Name : null,
                ErrorMessage: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].Message : null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
        }

        return result;
    }

    /// <summary>
    /// Asynchronously executes a side effect when the result is failed, without modifying the result.
    /// </summary>
    /// <param name="result">The result to inspect.</param>
    /// <param name="action">The async action to execute with the first error if the result is failed.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    public static async Task<Result<T>> TapOnFailureAsync<T>(
        this Result<T> result,
        Func<IError, Task> action,
        CancellationToken cancellationToken = default,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        if (result.IsFailure)
        {
            await action(result.Errors[0]);
        }

        if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "TapOnFailureAsync",
                IsSuccess: result.IsSuccess, OutputValue: null,
                ErrorType: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].GetType().Name : null,
                ErrorMessage: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].Message : null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
        }

        return result;
    }

    // Non-generic Result
    public static Result TapOnFailure(this Result result, Action<IError> action)
    {
        ValidationExtensions.EnsureNotNull(action, nameof(action));
        ValidationExtensions.EnsureNotNullOrEmpty(result.Errors, nameof(result.Errors));
        if (result.IsFailure)
            action(result.Errors[0]);
        return result;
    }

    public static async Task<Result> TapOnFailureAsync(this Result result, Func<IError, Task> action, CancellationToken cancellationToken = default)
    {
        ValidationExtensions.EnsureNotNull(action, nameof(action));
        ValidationExtensions.EnsureNotNullOrEmpty(result.Errors, nameof(result.Errors));
        cancellationToken.ThrowIfCancellationRequested();
        if (result.IsFailure)
            await action(result.Errors[0]);
        return result;
    }

    // Task<Result<T>> extensions
    public static async Task<Result<T>> TapOnFailureAsync<T>(
        this Task<Result<T>> resultTask,
        Action<IError> action,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await resultTask;
        if (result.IsFailure)
            action(result.Errors[0]);
        return result;
    }

    // All errors variants
    public static Result<T> TapOnFailure<T>(
        this Result<T> result,
        Action<ImmutableList<IError>> action)
    {
        if (result.IsFailure)
            action(result.Errors);
        return result;
    }

    public static Result<T> TapBoth<T>(
        this Result<T> result,
        Action<Result<T>> action)
    {
        action(result);
        return result;
    }

    #endregion
}
