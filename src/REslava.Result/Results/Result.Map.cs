using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using REslava.Result.Observers;

namespace REslava.Result;

public partial class Result<TValue>
{
    /// <summary>
    /// Transform the value of a successful result using a specific mapper function.
    /// If the result is failed, returns a new failed result with the same reasons.
    /// If successful, preserves all success reasons from the original result.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="mapper">The function to convert the value.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    /// <returns>A new result with the transformed value or the original errors.</returns>
    public Result<TOut> Map<TOut>(
        Func<TValue, TOut> mapper,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        mapper = mapper.EnsureNotNull(nameof(mapper));

        // If already failed, propagate all reasons to new type — entity unchanged (no mapping occurred)
        if (IsFailure)
        {
            return new Result<TOut>(default, Reasons) { Context = Context };
        }

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        // Successful Map: entity updates to TOut name, other fields inherited from parent
        var mappedContext = Context is null ? new ResultContext { Entity = typeof(TOut).Name }
                                           : Context with { Entity = typeof(TOut).Name };
        try
        {
            // Transform the value
            TOut mappedValue = mapper(Value!);

            if (obs != null)
            {
                var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                    * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
                obs.OnNodeExit(new NodeExitContext(
                    PipelineId: pipelineId,
                    NodeId: nodeId,
                    StepName: "Map",
                    IsSuccess: true,
                    OutputValue: mappedValue?.ToString(),
                    ErrorType: null,
                    ErrorMessage: null,
                    ElapsedMs: elapsedMs,
                    NodeIndex: nodeIndex));
            }

            // Create new result preserving all success reasons
            if (Successes.Count > 0)
            {
                return new Result<TOut>(mappedValue, Reasons) { Context = mappedContext };
            }

            // No success reasons to preserve
            var ok = Result<TOut>.Ok(mappedValue);
            ok.Context = mappedContext;
            return ok;
        }
        catch (Exception ex)
        {
            // On exception, preserve any success reasons from original result
            var exceptionError = new ExceptionError(ex);

            if (obs != null)
            {
                var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                    * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
                obs.OnNodeExit(new NodeExitContext(
                    PipelineId: pipelineId,
                    NodeId: nodeId,
                    StepName: "Map",
                    IsSuccess: false,
                    OutputValue: null,
                    ErrorType: exceptionError.GetType().Name,
                    ErrorMessage: exceptionError.Message,
                    ElapsedMs: elapsedMs,
                    NodeIndex: nodeIndex));
            }

            if (Successes.Count > 0)
            {
                var combinedReasons = Successes.ToImmutableList<IReason>().Add(exceptionError);
                return new Result<TOut>(default, combinedReasons) { Context = Context };
            }

            var fail = Result<TOut>.Fail(exceptionError);
            fail.Context = Context;
            return fail;
        }
    }

    /// <summary>
    /// Asynchronously transforms the value of a successful result using a mapper function.
    /// If the result is failed, returns a new failed result with the same reasons.
    /// If successful, preserves all success reasons from the original result.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="mapper">The async function to convert the value.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <param name="_callerFile">Infrastructure — do not use.</param>
    /// <param name="_callerLine">Infrastructure — do not use.</param>
    /// <returns>A task containing a new result with the transformed value or the original errors.</returns>
    public async Task<Result<TOut>> MapAsync<TOut>(
        Func<TValue, Task<TOut>> mapper,
        CancellationToken cancellationToken = default,
        [CallerFilePath] string _callerFile = "",
        [CallerLineNumber] int _callerLine = 0)
    {
        mapper = mapper.EnsureNotNull(nameof(mapper));
        cancellationToken.ThrowIfCancellationRequested();

        // If already failed, propagate all reasons to new type — entity unchanged
        if (IsFailure)
        {
            return new Result<TOut>(default, Reasons) { Context = Context };
        }

        var obs = ResultPipelineHooks.Observer;
        var state = ResultPipelineHooks.State;
        var nodeIndex = state?.ConsumeIndex() ?? 0;
        var nodeId = state != null ? state.CurrentNodeId() : $"{System.IO.Path.GetFileName(_callerFile)}:{_callerLine}";
        var pipelineId = state?.PipelineId ?? _callerFile;
        var ts = obs != null ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;

        var mappedContext = Context is null ? new ResultContext { Entity = typeof(TOut).Name }
                                           : Context with { Entity = typeof(TOut).Name };
        try
        {
            // Transform the value asynchronously
            var mappedValue = await mapper(Value!);

            if (obs != null)
            {
                var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                    * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
                obs.OnNodeExit(new NodeExitContext(
                    PipelineId: pipelineId,
                    NodeId: nodeId,
                    StepName: "MapAsync",
                    IsSuccess: true,
                    OutputValue: mappedValue?.ToString(),
                    ErrorType: null,
                    ErrorMessage: null,
                    ElapsedMs: elapsedMs,
                    NodeIndex: nodeIndex));
            }

            // Create new result preserving all success reasons
            if (Successes.Count > 0)
            {
                return new Result<TOut>(mappedValue, Reasons) { Context = mappedContext };
            }

            var ok = Result<TOut>.Ok(mappedValue);
            ok.Context = mappedContext;
            return ok;
        }
        catch (Exception ex)
        {
            var exceptionError = new ExceptionError(ex);

            if (obs != null)
            {
                var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - ts)
                    * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
                obs.OnNodeExit(new NodeExitContext(
                    PipelineId: pipelineId,
                    NodeId: nodeId,
                    StepName: "MapAsync",
                    IsSuccess: false,
                    OutputValue: null,
                    ErrorType: exceptionError.GetType().Name,
                    ErrorMessage: exceptionError.Message,
                    ElapsedMs: elapsedMs,
                    NodeIndex: nodeIndex));
            }

            if (Successes.Count > 0)
            {
                var combinedReasons = Successes.ToImmutableList<IReason>().Add(exceptionError);
                return new Result<TOut>(default, combinedReasons) { Context = Context };
            }

            var fail = Result<TOut>.Fail(exceptionError);
            fail.Context = Context;
            return fail;
        }
    }
}
