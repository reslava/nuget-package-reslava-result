using System.Collections.Generic;

namespace REslava.Result.Flow.Generators.ResultFlow.Models
{
    internal sealed class PipelineNode
    {
        public string MethodName { get; }
        public NodeKind Kind { get; }

        /// <summary>True when the method name ends with "Async".</summary>
        public bool IsAsync => MethodName.EndsWith("Async");

        /// <summary>
        /// The success type T flowing INTO this step (output type of the previous step).
        /// Null when type could not be resolved or this is the first step.
        /// </summary>
        public string? InputType { get; set; }

        /// <summary>
        /// The success type T flowing OUT of this step (T in Result&lt;T&gt; returned by this call).
        /// Null when type could not be resolved.
        /// </summary>
        public string? OutputType { get; set; }

        /// <summary>
        /// Error types that may be produced by this step, collected from method body scanning.
        /// Empty when the step method is in a referenced assembly (no body available) or produces no IError.
        /// Best-effort: errors created but not returned may appear; errors in helper methods are not followed.
        /// </summary>
        public IReadOnlyCollection<string> PossibleErrors { get; set; } = new HashSet<string>();

        /// <summary>
        /// Stable node identity assigned by the renderer (e.g. <c>"N0_FindUser"</c>).
        /// Matches <see cref="REslava.Result.ReasonMetadata.NodeId"/> for runtime error correlation.
        /// Set by <see cref="ResultFlowMermaidRenderer"/> after visibility filtering.
        /// </summary>
        public string? NodeId { get; set; }

        public PipelineNode(string methodName, NodeKind kind)
        {
            MethodName = methodName;
            Kind = kind;
        }
    }
}
