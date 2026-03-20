namespace REslava.ResultFlow.Generators.ResultFlow.Models
{
    internal sealed class PipelineNode
    {
        public string MethodName { get; }
        public NodeKind Kind { get; }
        public bool IsAsync => MethodName.EndsWith("Async");

        /// <summary>
        /// The success type flowing INTO this step (output type of the previous step).
        /// Null when type could not be resolved or this is the first step.
        /// </summary>
        public string? InputType { get; set; }

        /// <summary>
        /// The success type flowing OUT of this step (T in the generic return type of this call).
        /// Null when type could not be resolved.
        /// </summary>
        public string? OutputType { get; set; }

        /// <summary>
        /// The error type of the typed pipeline — <c>TError</c> in <c>Result&lt;T, TError&gt;</c>.
        /// Only set when the method return type carries a second generic argument (type-read mode).
        /// Null for <c>Result&lt;T&gt;</c> pipelines (body-scan mode).
        /// </summary>
        public string? ErrorType { get; set; }

        /// <summary>
        /// Syntactically extracted error type name for body-scan pipelines.
        /// Set when a step argument is a direct error constructor (<c>new NotFoundError(...)</c>)
        /// or a static factory on a type whose name ends with "Error" or "Reason"
        /// (<c>NotFoundError.For(...)</c>, <c>ValidationError.Field(...)</c>).
        /// Used as a fallback by the Mermaid renderer when <see cref="ErrorType"/> is null.
        /// </summary>
        public string? ErrorHint { get; set; }

        /// <summary>
        /// Absolute path of the source file containing this pipeline step's invocation.
        /// Null when source location is unavailable.
        /// Used for emitting Mermaid <c>click</c> directives.
        /// </summary>
        public string? SourceFile { get; set; }

        /// <summary>
        /// 1-indexed source line of this pipeline step's invocation within <see cref="SourceFile"/>.
        /// Null when <see cref="SourceFile"/> is null.
        /// </summary>
        public int? SourceLine { get; set; }

        /// <summary>
        /// Stable node identity assigned by the renderer (e.g. <c>"N0_FindUser"</c>).
        /// Set by <see cref="ResultFlowMermaidRenderer"/> after visibility filtering.
        /// </summary>
        public string? NodeId { get; set; }

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

        public PipelineNode(string methodName, NodeKind kind)
        {
            MethodName = methodName;
            Kind = kind;
        }
    }
}
