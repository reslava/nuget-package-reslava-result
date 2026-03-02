using REslava.ResultFlow.Generators.ResultFlow.Models;
using System.Collections.Generic;
using System.Text;

namespace REslava.ResultFlow.Generators.ResultFlow.CodeGeneration
{
    internal static class ResultFlowMermaidRenderer
    {
        /// <summary>
        /// Converts an ordered list of <see cref="PipelineNode"/> into a Mermaid <c>flowchart LR</c> string.
        /// Invisible nodes are filtered out before rendering.
        /// </summary>
        public static string Render(IReadOnlyList<PipelineNode> nodes)
        {
            // Filter out invisible nodes (WithSuccess, WithSuccessAsync, etc.)
            var visible = new List<PipelineNode>();
            foreach (var node in nodes)
            {
                if (node.Kind != NodeKind.Invisible)
                    visible.Add(node);
            }

            if (visible.Count == 0)
                return "flowchart LR";

            var lines = new List<string>();
            var classDefs = new List<string>();
            var declaredClasses = new HashSet<string>();

            for (int i = 0; i < visible.Count; i++)
            {
                var node = visible[i];
                var nodeId = $"N{i}_{node.MethodName}";
                bool hasNext = i < visible.Count - 1;
                string nextId = hasNext ? $"N{i + 1}_{visible[i + 1].MethodName}" : string.Empty;

                switch (node.Kind)
                {
                    case NodeKind.Gatekeeper:
                        lines.Add($"    {nodeId}[\"{node.MethodName}\"]:::gatekeeper");
                        if (hasNext) lines.Add($"    {nodeId} -->|pass| {nextId}");
                        lines.Add($"    {nodeId} -->|fail| F{i}[\"Failure\"]:::failure");
                        TryAddClass(declaredClasses, classDefs, "gatekeeper", "fill:#e3e9fa,color:#3f5c9a");
                        TryAddClass(declaredClasses, classDefs, "failure",    "fill:#f8e3e3,color:#b13e3e");
                        break;

                    case NodeKind.TransformWithRisk:
                        lines.Add($"    {nodeId}[\"{node.MethodName}\"]:::transform");
                        if (hasNext) lines.Add($"    {nodeId} -->|ok| {nextId}");
                        lines.Add($"    {nodeId} -->|fail| F{i}[\"Failure\"]:::failure");
                        TryAddClass(declaredClasses, classDefs, "transform", "fill:#e3f0e8,color:#2f7a5c");
                        TryAddClass(declaredClasses, classDefs, "failure",   "fill:#f8e3e3,color:#b13e3e");
                        break;

                    case NodeKind.PureTransform:
                        lines.Add($"    {nodeId}[\"{node.MethodName}\"]:::transform");
                        if (hasNext) lines.Add($"    {nodeId} --> {nextId}");
                        TryAddClass(declaredClasses, classDefs, "transform", "fill:#e3f0e8,color:#2f7a5c");
                        break;

                    case NodeKind.SideEffectSuccess:
                    case NodeKind.SideEffectFailure:
                    case NodeKind.SideEffectBoth:
                        lines.Add($"    {nodeId}[\"{node.MethodName}\"]:::sideeffect");
                        if (hasNext) lines.Add($"    {nodeId} --> {nextId}");
                        TryAddClass(declaredClasses, classDefs, "sideeffect", "fill:#fff4d9,color:#b8882c");
                        break;

                    case NodeKind.Terminal:
                        lines.Add($"    {nodeId}[\"{node.MethodName}\"]:::terminal");
                        // No outbound edges — terminal node ends the pipeline
                        TryAddClass(declaredClasses, classDefs, "terminal", "fill:#f2e3f5,color:#8a4f9e");
                        break;

                    default: // Unknown
                        lines.Add($"    {nodeId}[\"{node.MethodName}\"]:::operation");
                        if (hasNext) lines.Add($"    {nodeId} --> {nextId}");
                        TryAddClass(declaredClasses, classDefs, "operation", "fill:#e8f4f0,color:#1c7e6f");
                        break;
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("flowchart LR");
            foreach (var line in lines)
                sb.AppendLine(line);
            foreach (var classDef in classDefs)
                sb.AppendLine(classDef);

            return sb.ToString().TrimEnd();
        }

        private static void TryAddClass(HashSet<string> declared, List<string> classDefs, string name, string style)
        {
            if (declared.Add(name))
                classDefs.Add($"    classDef {name} {style}");
        }
    }
}
