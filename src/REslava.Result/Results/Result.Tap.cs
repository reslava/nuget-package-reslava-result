using System.Runtime.CompilerServices;
using REslava.Result.Observers;

namespace REslava.Result;

public partial class Result
{
    /// <summary>
    /// Executes a side effect without modifying the result.
    /// Useful for logging, telemetry, or other side effects that shouldn't affect the result flow.
    /// </summary>
    /// <param name="action">The action to execute if the result is successful.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    /// <returns>The original result unchanged.</returns>
    /// <example>
    /// <code>
    /// var result = Result.Ok()
    ///     .Tap(() => Console.WriteLine("Operation completed successfully"));
    /// </code>
    /// </example>
    public Result Tap(
        Action action,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        action = action.EnsureNotNull(nameof(action));

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        if (IsSuccess)
        {
            action();
        }

        if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "Tap",
                IsSuccess: IsSuccess, OutputValue: null,
                ErrorType: null, ErrorMessage: null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
        }

        return this;
    }

    /// <summary>
    /// Executes a side effect asynchronously without modifying the result.
    /// Useful for async logging, telemetry, or other side effects that shouldn't affect the result flow.
    /// </summary>
    /// <param name="action">The async action to execute if the result is successful.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    /// <returns>A task containing the original result unchanged.</returns>
    /// <example>
    /// <code>
    /// var result = await Result.Ok()
    ///     .TapAsync(async () => await logger.LogAsync("Operation completed"));
    /// </code>
    /// </example>
    public async Task<Result> TapAsync(
        Func<Task> action,
        CancellationToken cancellationToken = default,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        action = action.EnsureNotNull(nameof(action));
        cancellationToken.ThrowIfCancellationRequested();

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        if (IsSuccess)
        {
            await action();
        }

        if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "TapAsync",
                IsSuccess: IsSuccess, OutputValue: null,
                ErrorType: null, ErrorMessage: null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
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
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    /// <returns>The original result unchanged.</returns>
    /// <example>
    /// <code>
    /// var result = Result&lt;User&gt;.Ok(user)
    ///     .Tap(u => Console.WriteLine($"User created: {u.Name}"));
    /// </code>
    /// </example>
    public Result<TValue> Tap(
        Action<TValue> action,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        action = action.EnsureNotNull(nameof(action));

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        if (IsSuccess)
        {
            try
            {
                action(Value!);

                if (obs != null)
                {
                    var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                        * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
                    obs.OnNodeExit(new NodeExitContext(
                        PipelineId: pipelineId, NodeId: nodeId, StepName: "Tap",
                        IsSuccess: true, OutputValue: null,
                        ErrorType: null, ErrorMessage: null,
                        ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
                }
            }
            catch (Exception ex)
            {
                if (obs != null)
                {
                    var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                        * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
                    obs.OnNodeExit(new NodeExitContext(
                        PipelineId: pipelineId, NodeId: nodeId, StepName: "Tap",
                        IsSuccess: false, OutputValue: null,
                        ErrorType: ex.GetType().Name, ErrorMessage: ex.Message,
                        ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
                }
                return new Result<TValue>(default(TValue), new ExceptionError(ex)) { Context = Context };
            }
        }
        else if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "Tap",
                IsSuccess: false, OutputValue: null,
                ErrorType: Errors.Count > 0 ? Errors[0].GetType().Name : null,
                ErrorMessage: Errors.Count > 0 ? Errors[0].Message : null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
        }

        return this;
    }

    /// <summary>
    /// Executes an async side effect with the value without modifying the result.
    /// Useful for async logging the value, telemetry, or other side effects that shouldn't affect the result flow.
    /// </summary>
    /// <param name="action">The async action to execute with the value if the result is successful.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    /// <returns>A task containing the original result unchanged.</returns>
    /// <example>
    /// <code>
    /// var result = await Result&lt;User&gt;.Ok(user)
    ///     .TapAsync(async u => await logger.LogAsync($"User created: {u.Name}"));
    /// </code>
    /// </example>
    public async Task<Result<TValue>> TapAsync(
        Func<TValue, Task> action,
        CancellationToken cancellationToken = default,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        action = action.EnsureNotNull(nameof(action));
        cancellationToken.ThrowIfCancellationRequested();

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        if (IsSuccess)
        {
            try
            {
                await action(Value!);

                if (obs != null)
                {
                    var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                        * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
                    obs.OnNodeExit(new NodeExitContext(
                        PipelineId: pipelineId, NodeId: nodeId, StepName: "TapAsync",
                        IsSuccess: true, OutputValue: null,
                        ErrorType: null, ErrorMessage: null,
                        ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
                }
            }
            catch (Exception ex)
            {
                if (obs != null)
                {
                    var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                        * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
                    obs.OnNodeExit(new NodeExitContext(
                        PipelineId: pipelineId, NodeId: nodeId, StepName: "TapAsync",
                        IsSuccess: false, OutputValue: null,
                        ErrorType: ex.GetType().Name, ErrorMessage: ex.Message,
                        ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
                }
                return new Result<TValue>(default(TValue), new ExceptionError(ex)) { Context = Context };
            }
        }
        else if (obs != null)
        {
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            obs.OnNodeExit(new NodeExitContext(
                PipelineId: pipelineId, NodeId: nodeId, StepName: "TapAsync",
                IsSuccess: false, OutputValue: null,
                ErrorType: Errors.Count > 0 ? Errors[0].GetType().Name : null,
                ErrorMessage: Errors.Count > 0 ? Errors[0].Message : null,
                ElapsedMs: elapsedMs, NodeIndex: nodeIndex));
        }

        return this;
    }
}
