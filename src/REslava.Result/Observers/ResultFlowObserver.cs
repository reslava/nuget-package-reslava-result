namespace REslava.Result.Observers
{
    /// <summary>
    /// Base class for receiving pipeline execution events.
    /// Override only the methods you need — all have no-op default implementations.
    /// Register via <see cref="PipelineObserver.Register"/> or <c>PipelineObserver.AddPipelineObserver&lt;T&gt;()</c>.
    /// </summary>
    public abstract class ResultFlowObserver
    {
        /// <summary>Called once before the first node executes.</summary>
        public virtual void OnPipelineStart(PipelineStartContext ctx) { }

        /// <summary>Called once after the pipeline completes (success or failure).</summary>
        public virtual void OnPipelineEnd(PipelineEndContext ctx) { }

        /// <summary>Called immediately before a node executes, with the incoming value.</summary>
        public virtual void OnNodeEnter(NodeEnterContext ctx) { }

        /// <summary>Called immediately after a node completes, with the outgoing result.</summary>
        public virtual void OnNodeExit(NodeExitContext ctx) { }
    }
}
