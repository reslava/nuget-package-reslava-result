using REslava.Result.Observers;

namespace REslava.Result
{
    /// <summary>
    /// Registers a <see cref="ResultFlowObserver"/> to receive pipeline execution events
    /// from <c>Bind</c>, <c>Map</c>, <c>Ensure</c>, <c>Tap</c>, and <c>Match</c> calls.
    /// </summary>
    public static class PipelineObserver
    {
        /// <summary>
        /// Registers the observer globally. Replaces any previously registered observer.
        /// Call in <c>Program.cs</c> before the app starts processing requests.
        /// </summary>
        public static void Register(ResultFlowObserver observer)
        {
            ResultPipelineHooks.Observer = observer;
        }

        /// <summary>Removes the currently registered observer.</summary>
        public static void Unregister()
        {
            ResultPipelineHooks.Observer = null;
        }

        /// <summary>
        /// Registers the observer for the duration of the returned scope.
        /// Restores the previous observer on dispose. Useful in tests.
        /// </summary>
        public static System.IDisposable RegisterScoped(ResultFlowObserver observer)
        {
            var previous = ResultPipelineHooks.Observer;
            ResultPipelineHooks.Observer = observer;
            return new ScopedRegistration(() => ResultPipelineHooks.Observer = previous);
        }

        /// <summary>
        /// Begins a traced pipeline execution. Fires <see cref="ResultFlowObserver.OnPipelineStart"/>,
        /// sets the <see cref="Observers.PipelineState"/> for the current async context (saving any outer
        /// state for nested pipelines), and returns a scope that fires
        /// <see cref="ResultFlowObserver.OnPipelineEnd"/> on dispose.
        /// Call from a <c>_Traced</c> generated wrapper inside a try/finally.
        /// </summary>
        /// <param name="pipelineId">Stable identifier for this pipeline (e.g. FNV-1a hash).</param>
        /// <param name="methodName">The user's method name (e.g. "PlaceOrder").</param>
        /// <param name="inputValue">Optional string representation of the input for display.</param>
        /// <param name="nodeIds">Ordered node identifiers matching the pipeline steps.</param>
        public static PipelineScope BeginPipeline(
            string pipelineId,
            string methodName,
            string? inputValue,
            string[]? nodeIds)
        {
            var obs = ResultPipelineHooks.Observer;
            var previous = ResultPipelineHooks.State;

            var startedAt = System.DateTimeOffset.UtcNow;
            var startTs = System.Diagnostics.Stopwatch.GetTimestamp();

            var state = new Observers.PipelineState
            {
                PipelineId = pipelineId,
                MethodName = methodName,
                NodeIds = nodeIds,
                NodeIndex = 0,
                StartTimestamp = startTs,
                StartedAt = startedAt,
                Previous = previous
            };
            ResultPipelineHooks.State = state;

            obs?.OnPipelineStart(new Observers.PipelineStartContext(
                PipelineId: pipelineId,
                MethodName: methodName,
                InputValue: inputValue,
                StartedAt: startedAt));

            return new PipelineScope(state, previous, obs);
        }

        private sealed class ScopedRegistration : System.IDisposable
        {
            private readonly System.Action _restore;
            internal ScopedRegistration(System.Action restore) => _restore = restore;
            public void Dispose() => _restore();
        }
    }

    /// <summary>
    /// Represents an active pipeline execution scope. Call <see cref="End"/> with the final
    /// result before the scope is disposed, then dispose in a <c>finally</c> block.
    /// </summary>
    public sealed class PipelineScope : System.IDisposable
    {
        private readonly Observers.PipelineState _state;
        private readonly Observers.PipelineState? _previous;
        private readonly Observers.ResultFlowObserver? _obs;
        private bool _ended;
        private bool _isSuccess;
        private string? _outputValue;
        private string? _errorType;

        internal PipelineScope(
            Observers.PipelineState state,
            Observers.PipelineState? previous,
            Observers.ResultFlowObserver? obs)
        {
            _state = state;
            _previous = previous;
            _obs = obs;
        }

        /// <summary>
        /// Records the outcome of the pipeline. Call before the <c>finally</c> block disposes the scope.
        /// </summary>
        public void End(bool isSuccess, string? outputValue, string? errorType)
        {
            _isSuccess = isSuccess;
            _outputValue = outputValue;
            _errorType = errorType;
            _ended = true;
        }

        /// <summary>
        /// Fires <see cref="Observers.ResultFlowObserver.OnPipelineEnd"/> and restores the outer
        /// <see cref="Observers.PipelineState"/>. Safe to call multiple times — only fires once.
        /// </summary>
        public void Dispose()
        {
            // Always restore state (even if End was never called — exception path)
            ResultPipelineHooks.State = _previous;

            if (_obs == null) return;

            var endedAt = System.DateTimeOffset.UtcNow;
            var elapsedMs = (long)((System.Diagnostics.Stopwatch.GetTimestamp() - _state.StartTimestamp)
                * 1000.0 / System.Diagnostics.Stopwatch.Frequency);

            _obs.OnPipelineEnd(new Observers.PipelineEndContext(
                PipelineId: _state.PipelineId,
                MethodName: _state.MethodName,
                IsSuccess: _isSuccess,
                ErrorType: _ended ? _errorType : "Exception",
                OutputValue: _isSuccess ? _outputValue : null,
                ElapsedMs: elapsedMs,
                EndedAt: endedAt));
        }
    }
}

#if NET8_0_OR_GREATER
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>DI convenience extension — net8+ only.</summary>
    public static class PipelineObserverServiceCollectionExtensions
    {
        public static IServiceCollection AddPipelineObserver<T>(
            this IServiceCollection services)
            where T : REslava.Result.Observers.ResultFlowObserver, new()
        {
            var instance = new T();
            REslava.Result.PipelineObserver.Register(instance);
            return services.AddSingleton<REslava.Result.Observers.ResultFlowObserver>(instance);
        }
    }
}
#endif
