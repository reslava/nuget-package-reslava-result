namespace REslava.Result.Observers
{
    internal static class ResultPipelineHooks
    {
        /// <summary>The registered observer. Null when no observer is active — zero overhead.</summary>
        internal static ResultFlowObserver? Observer;

        private static readonly System.Threading.AsyncLocal<PipelineState?> _state
            = new System.Threading.AsyncLocal<PipelineState?>();

        internal static PipelineState? State
        {
            get => _state.Value;
            set => _state.Value = value;
        }
    }

    internal sealed class PipelineState
    {
        internal string PipelineId = string.Empty;
        internal string MethodName = string.Empty;
        internal string[]? NodeIds;
        internal int NodeIndex;
        internal long StartTimestamp;
        internal System.DateTimeOffset StartedAt;
        /// <summary>Saved outer state for nested pipeline restore.</summary>
        internal PipelineState? Previous;

        internal string CurrentNodeId()
        {
            if (NodeIds != null && NodeIndex < NodeIds.Length)
                return NodeIds[NodeIndex];
            return $"{PipelineId}:{NodeIndex}";
        }

        internal int ConsumeIndex()
        {
            var idx = NodeIndex;
            NodeIndex++;
            return idx;
        }
    }
}
