using System.Collections.Concurrent;
using System.Collections.Generic;

namespace REslava.Result.Observers
{
    /// <summary>
    /// A <see cref="ResultFlowObserver"/> that stores the last <see cref="Capacity"/> pipeline
    /// executions in an in-memory ring buffer. Thread-safe. Zero persistence — cleared on restart.
    /// </summary>
    public sealed class RingBufferObserver : ResultFlowObserver
    {
        private readonly int _capacity;
        private readonly ConcurrentQueue<PipelineTrace> _traces = new ConcurrentQueue<PipelineTrace>();

        // Tracks the in-progress trace per async execution context
        private readonly System.Threading.AsyncLocal<InProgressTrace?> _current
            = new System.Threading.AsyncLocal<InProgressTrace?>();

        public int Capacity => _capacity;

        public RingBufferObserver(int capacity = 50)
        {
            _capacity = capacity < 1 ? 50 : capacity;
        }

        public override void OnPipelineStart(PipelineStartContext ctx)
        {
            _current.Value = new InProgressTrace
            {
                PipelineId = ctx.PipelineId,
                MethodName = ctx.MethodName,
                InputValue = ctx.InputValue,
                StartedAt = ctx.StartedAt,
                StartTimestamp = System.Diagnostics.Stopwatch.GetTimestamp()
            };
        }

        public override void OnNodeExit(NodeExitContext ctx)
        {
            var inProgress = _current.Value;
            if (inProgress == null) return;

            inProgress.Nodes.Add(new NodeTrace
            {
                NodeId = ctx.NodeId,
                StepName = ctx.StepName,
                IsSuccess = ctx.IsSuccess,
                OutputValue = ctx.OutputValue,
                ErrorType = ctx.ErrorType,
                ErrorMessage = ctx.ErrorMessage,
                ElapsedMs = ctx.ElapsedMs,
                NodeIndex = ctx.NodeIndex
            });
        }

        public override void OnPipelineEnd(PipelineEndContext ctx)
        {
            var inProgress = _current.Value;
            if (inProgress == null) return;

            var trace = new PipelineTrace
            {
                PipelineId = ctx.PipelineId,
                MethodName = ctx.MethodName,
                IsSuccess = ctx.IsSuccess,
                ErrorType = ctx.ErrorType,
                InputValue = inProgress.InputValue,
                OutputValue = ctx.OutputValue,
                ElapsedMs = ctx.ElapsedMs,
                StartedAt = inProgress.StartedAt,
                EndedAt = ctx.EndedAt,
                Nodes = inProgress.Nodes.AsReadOnly()
            };

            _traces.Enqueue(trace);
            while (_traces.Count > _capacity)
                _traces.TryDequeue(out _);

            _current.Value = null;
        }

        /// <summary>Returns a snapshot of captured traces, oldest first.</summary>
        public IReadOnlyList<PipelineTrace> GetTraces()
        {
            return new List<PipelineTrace>(_traces).AsReadOnly();
        }

        /// <summary>Clears all captured traces.</summary>
        public void Clear()
        {
            while (_traces.TryDequeue(out _)) { }
        }

        private sealed class InProgressTrace
        {
            internal string PipelineId = string.Empty;
            internal string MethodName = string.Empty;
            internal string? InputValue;
            internal System.DateTimeOffset StartedAt;
            internal long StartTimestamp;
            internal List<NodeTrace> Nodes = new List<NodeTrace>();
        }
    }
}
