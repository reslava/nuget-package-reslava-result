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

        public PipelineNode(string methodName, NodeKind kind)
        {
            MethodName = methodName;
            Kind = kind;
        }
    }
}
