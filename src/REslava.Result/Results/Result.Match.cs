using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using REslava.Result.Observers;

namespace REslava.Result;

/// <summary>
/// Extension methods for match operations on Result types.
/// </summary>
public static class ResultMatchExtensions
{
    /// <summary>
    /// Matches result to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure with errors.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    /// <returns>The result of the executed function.</returns>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<ImmutableList<IError>, TOut> onFailure,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        TOut value = result.IsSuccess ? onSuccess() : onFailure(result.Errors);

        if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "Match",
                IsSuccess: result.IsSuccess,
                OutputValue: result.IsSuccess ? value?.ToString() : null,
                ErrorType: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].GetType().Name : null,
                ErrorMessage: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].Message : null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
        }

        return value;
    }

    /// <summary>
    /// Matches result to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Function to execute on success with value.</param>
    /// <param name="onFailure">Function to execute on failure with errors.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    /// <returns>The result of the executed function.</returns>
    public static TOut Match<T, TOut>(
        this Result<T> result,
        Func<T, TOut> onSuccess,
        Func<ImmutableList<IError>, TOut> onFailure,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        TOut value = result.IsSuccess ? onSuccess(result.Value!) : onFailure(result.Errors);

        if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "Match",
                IsSuccess: result.IsSuccess,
                OutputValue: result.IsSuccess ? value?.ToString() : null,
                ErrorType: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].GetType().Name : null,
                ErrorMessage: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].Message : null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
        }

        return value;
    }

    /// <summary>
    /// Executes one of two actions based on success or failure.
    /// </summary>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Action to execute on success.</param>
    /// <param name="onFailure">Action to execute on failure with errors.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    public static void Match(
        this Result result,
        Action onSuccess,
        Action<ImmutableList<IError>> onFailure,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        if (result.IsSuccess)
            onSuccess();
        else
            onFailure(result.Errors);

        if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "Match",
                IsSuccess: result.IsSuccess, OutputValue: null,
                ErrorType: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].GetType().Name : null,
                ErrorMessage: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].Message : null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
        }
    }

    /// <summary>
    /// Executes one of two actions based on success or failure.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Action to execute on success with value.</param>
    /// <param name="onFailure">Action to execute on failure with errors.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    public static void Match<T>(
        this Result<T> result,
        Action<T> onSuccess,
        Action<ImmutableList<IError>> onFailure,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        if (result.IsSuccess)
            onSuccess(result.Value!);
        else
            onFailure(result.Errors);

        if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "Match",
                IsSuccess: result.IsSuccess, OutputValue: null,
                ErrorType: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].GetType().Name : null,
                ErrorMessage: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].Message : null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
        }
    }

    /// <summary>
    /// Asynchronously matches result to one of two async functions based on success or failure.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Async function to execute on success.</param>
    /// <param name="onFailure">Async function to execute on failure with errors.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    /// <returns>A task containing the result of the executed function.</returns>
    public static async Task<TOut> MatchAsync<TOut>(
        this Result result,
        Func<Task<TOut>> onSuccess,
        Func<ImmutableList<IError>, Task<TOut>> onFailure,
        CancellationToken cancellationToken = default,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        cancellationToken.ThrowIfCancellationRequested();

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        TOut value = result.IsSuccess ? await onSuccess() : await onFailure(result.Errors);

        if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "MatchAsync",
                IsSuccess: result.IsSuccess,
                OutputValue: result.IsSuccess ? value?.ToString() : null,
                ErrorType: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].GetType().Name : null,
                ErrorMessage: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].Message : null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
        }

        return value;
    }

    /// <summary>
    /// Asynchronously matches result to one of two async functions based on success or failure.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Async function to execute on success with value.</param>
    /// <param name="onFailure">Async function to execute on failure with errors.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    /// <returns>A task containing the result of the executed function.</returns>
    public static async Task<TOut> MatchAsync<T, TOut>(
        this Result<T> result,
        Func<T, Task<TOut>> onSuccess,
        Func<ImmutableList<IError>, Task<TOut>> onFailure,
        CancellationToken cancellationToken = default,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        cancellationToken.ThrowIfCancellationRequested();

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        TOut value = result.IsSuccess ? await onSuccess(result.Value!) : await onFailure(result.Errors);

        if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "MatchAsync",
                IsSuccess: result.IsSuccess,
                OutputValue: result.IsSuccess ? value?.ToString() : null,
                ErrorType: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].GetType().Name : null,
                ErrorMessage: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].Message : null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
        }

        return value;
    }

    /// <summary>
    /// Asynchronously executes one of two async actions based on success or failure.
    /// </summary>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Async action to execute on success.</param>
    /// <param name="onFailure">Async action to execute on failure with errors.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    /// <returns>A task representing the async operation.</returns>
    public static async Task MatchAsync(
        this Result result,
        Func<Task> onSuccess,
        Func<ImmutableList<IError>, Task> onFailure,
        CancellationToken cancellationToken = default,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        cancellationToken.ThrowIfCancellationRequested();

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        if (result.IsSuccess)
            await onSuccess();
        else
            await onFailure(result.Errors);

        if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "MatchAsync",
                IsSuccess: result.IsSuccess, OutputValue: null,
                ErrorType: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].GetType().Name : null,
                ErrorMessage: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].Message : null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
        }
    }

    /// <summary>
    /// Asynchronously executes one of two async actions based on success or failure.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Async action to execute on success with value.</param>
    /// <param name="onFailure">Async action to execute on failure with errors.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    /// <returns>A task representing the async operation.</returns>
    public static async Task MatchAsync<T>(
        this Result<T> result,
        Func<T, Task> onSuccess,
        Func<ImmutableList<IError>, Task> onFailure,
        CancellationToken cancellationToken = default,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        result = result.EnsureNotNull(nameof(result));
        onSuccess = onSuccess.EnsureNotNull(nameof(onSuccess));
        onFailure = onFailure.EnsureNotNull(nameof(onFailure));
        cancellationToken.ThrowIfCancellationRequested();

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        if (result.IsSuccess)
            await onSuccess(result.Value!);
        else
            await onFailure(result.Errors);

        if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "MatchAsync",
                IsSuccess: result.IsSuccess, OutputValue: null,
                ErrorType: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].GetType().Name : null,
                ErrorMessage: result.IsFailure && result.Errors.Count > 0 ? result.Errors[0].Message : null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
        }
    }
}
