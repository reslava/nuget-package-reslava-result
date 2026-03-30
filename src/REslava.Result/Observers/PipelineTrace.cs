using System.Collections.Generic;

namespace REslava.Result.Observers
{
    /// <summary>A snapshot of one complete pipeline execution captured by <see cref="RingBufferObserver"/>.</summary>
    public sealed class PipelineTrace
    {
        public string PipelineId { get; internal set; } = string.Empty;
        public string MethodName { get; internal set; } = string.Empty;
        public bool IsSuccess { get; internal set; }
        public string? ErrorType { get; internal set; }
        public string? InputValue { get; internal set; }
        public string? OutputValue { get; internal set; }
        public long ElapsedMs { get; internal set; }
        public System.DateTimeOffset StartedAt { get; internal set; }
        public System.DateTimeOffset EndedAt { get; internal set; }
        public IReadOnlyList<NodeTrace> Nodes { get; internal set; } = System.Array.Empty<NodeTrace>();
    }

    /// <summary>A snapshot of one node within a <see cref="PipelineTrace"/>.</summary>
    public sealed class NodeTrace
    {
        public string NodeId { get; internal set; } = string.Empty;
        public string StepName { get; internal set; } = string.Empty;
        public bool IsSuccess { get; internal set; }
        public string? InputValue { get; internal set; }
        public string? OutputValue { get; internal set; }
        public string? ErrorType { get; internal set; }
        public string? ErrorMessage { get; internal set; }
        public long ElapsedMs { get; internal set; }
        public int NodeIndex { get; internal set; }
    }
}
