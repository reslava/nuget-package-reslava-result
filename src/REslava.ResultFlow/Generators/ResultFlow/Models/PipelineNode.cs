namespace REslava.ResultFlow.Generators.ResultFlow.Models
{
    internal sealed class PipelineNode
    {
        public string MethodName { get; }
        public NodeKind Kind { get; }

        public PipelineNode(string methodName, NodeKind kind)
        {
            MethodName = methodName;
            Kind = kind;
        }
    }
}
