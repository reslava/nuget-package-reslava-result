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
        /// Matches <c>ReasonMetadata.NodeId</c> for runtime error correlation.
        /// Set by <c>ResultFlowMermaidRenderer</c> after visibility filtering.
        /// </summary>
        public string? NodeId { get; set; }

        /// <summary>
        /// Absolute path of the source file containing this pipeline step's invocation.
        /// Null when source location is unavailable (e.g. generated code or syntax-only extraction).
        /// Used for emitting Mermaid <c>click</c> directives.
        /// </summary>
        public string? SourceFile { get; set; }

        /// <summary>
        /// 1-indexed source line of this pipeline step's invocation within <see cref="SourceFile"/>.
        /// Null when <see cref="SourceFile"/> is null.
        /// </summary>
        public int? SourceLine { get; set; }

        /// <summary>
        /// When non-null, this Bind/BindAsync node expands into a sub-pipeline traced from the
        /// called method. The renderer emits a Mermaid <c>subgraph</c> block instead of a box.
        /// </summary>
        public IReadOnlyList<PipelineNode>? SubNodes { get; set; }

        /// <summary>
        /// Title of the Mermaid <c>subgraph</c> block — the called method's name.
        /// Populated whenever <see cref="SubNodes"/> is non-null.
        /// </summary>
        public string? SubGraphName { get; set; }

        /// <summary>
        /// Architectural layer of the method this node calls into.
        /// Populated only for subgraph nodes (<see cref="SubNodes"/> != null).
        /// Sourced from <c>[DomainBoundary("Layer")]</c> annotation (takes precedence)
        /// or namespace heuristics (fallback). Null when undetermined.
        /// </summary>
        public string? Layer { get; set; }

        /// <summary>
        /// Name of the class that contains the method this node calls into.
        /// Populated only for subgraph nodes (<see cref="SubNodes"/> != null).
        /// Used by <c>ResultFlowLayerViewRenderer</c> to group nodes by class within a layer.
        /// </summary>
        public string? ClassName { get; set; }

        /// <summary>
        /// For Gatekeeper nodes (Ensure, Filter): the predicate lambda body as a source text string,
        /// e.g. <c>"p.Stock &gt; 0"</c> from <c>.Ensure(p =&gt; p.Stock &gt; 0, ...)</c>.
        /// Null for all other node kinds or when the predicate is not a simple lambda expression.
        /// Rendered as a Mermaid <c>&lt;span title='...'&gt;</c> tooltip — visible on hover in VS Code.
        /// </summary>
        public string? PredicateText { get; set; }

        /// <summary>
        /// Typed error-branch labels for Match/MatchAsync, extracted from lambda parameter types
        /// via the semantic model. Count &gt; 0 means typed N-branch fan-out; null or empty means
        /// generic 2-branch (ok/fail). Null for all other NodeKinds.
        /// </summary>
        public IReadOnlyList<string>? MatchBranchLabels { get; set; }

        public PipelineNode(string methodName, NodeKind kind)
        {
            MethodName = methodName;
            Kind = kind;
        }
    }
}
