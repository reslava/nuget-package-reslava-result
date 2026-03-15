using REslava.Result.Flow.Generators.ResultFlow.Models;
using System.Collections.Generic;
using System.Text;

namespace REslava.Result.Flow.Generators.ResultFlow.CodeGeneration
{
    /// <summary>
    /// Renders an ordered list of <see cref="PipelineNode"/> into a Mermaid flowchart string.
    /// Features vs the library-agnostic renderer:
    /// - Inline type label: "MethodName&lt;br/&gt;InputType → OutputType" (when types are known)
    /// - Typed error edges: "N0 --&gt;|DatabaseError| FAIL" (from IError body scanning)
    /// - Shared FAIL([fail]) terminal node when any error edges exist
    /// </summary>
    internal static class ResultFlowMermaidRenderer
    {
        public static string Render(
            IReadOnlyList<PipelineNode> nodes,
            string? operationName = null,
            string? correlationId = null)
        {
            // Filter invisible nodes
            var visible = new List<PipelineNode>();
            foreach (var node in nodes)
            {
                if (node.Kind != NodeKind.Invisible)
                    visible.Add(node);
            }

            if (visible.Count == 0)
                return "flowchart LR";

            // Assign stable NodeIds before rendering (used for diagram nodes and runtime correlation).
            for (int i = 0; i < visible.Count; i++)
                visible[i].NodeId = $"N{i}_{visible[i].MethodName}";

            var lines = new List<string>();
            var classDefs = new List<string>();
            var declaredClasses = new HashSet<string>();
            bool anyErrorEdges = false;

            for (int i = 0; i < visible.Count; i++)
            {
                var node = visible[i];
                var nodeId = node.NodeId!;
                var label = BuildLabel(node);
                bool hasNext = i < visible.Count - 1;
                string nextId = hasNext ? visible[i + 1].NodeId! : string.Empty;

                switch (node.Kind)
                {
                    case NodeKind.Gatekeeper:
                        lines.Add($"    {nodeId}[\"{label}\"]:::gatekeeper");
                        if (hasNext) lines.Add($"    {nodeId} -->|pass| {nextId}");
                        EmitErrorEdges(lines, nodeId, node, ref anyErrorEdges,
                            fallbackEdge: $"    {nodeId} -->|fail| FAIL");
                        TryAddClass(declaredClasses, classDefs, "gatekeeper", "fill:#e3e9fa,color:#3f5c9a");
                        break;

                    case NodeKind.TransformWithRisk:
                        lines.Add($"    {nodeId}[\"{label}\"]:::transform");
                        if (hasNext) lines.Add($"    {nodeId} -->|ok| {nextId}");
                        EmitErrorEdges(lines, nodeId, node, ref anyErrorEdges,
                            fallbackEdge: $"    {nodeId} -->|fail| FAIL");
                        TryAddClass(declaredClasses, classDefs, "transform", "fill:#e3f0e8,color:#2f7a5c");
                        break;

                    case NodeKind.PureTransform:
                        lines.Add($"    {nodeId}[\"{label}\"]:::transform");
                        if (hasNext) lines.Add($"    {nodeId} --> {nextId}");
                        TryAddClass(declaredClasses, classDefs, "transform", "fill:#e3f0e8,color:#2f7a5c");
                        break;

                    case NodeKind.SideEffectSuccess:
                    case NodeKind.SideEffectFailure:
                    case NodeKind.SideEffectBoth:
                        lines.Add($"    {nodeId}[\"{label}\"]:::sideeffect");
                        if (hasNext) lines.Add($"    {nodeId} --> {nextId}");
                        TryAddClass(declaredClasses, classDefs, "sideeffect", "fill:#fff4d9,color:#b8882c");
                        break;

                    case NodeKind.Terminal:
                        lines.Add($"    {nodeId}[\"{label}\"]:::terminal");
                        TryAddClass(declaredClasses, classDefs, "terminal", "fill:#f2e3f5,color:#8a4f9e");
                        break;

                    default: // Unknown — entry point or unrecognised operation
                        lines.Add($"    {nodeId}[\"{label}\"]:::operation");
                        if (hasNext) lines.Add($"    {nodeId} --> {nextId}");
                        EmitErrorEdges(lines, nodeId, node, ref anyErrorEdges, fallbackEdge: null);
                        TryAddClass(declaredClasses, classDefs, "operation", "fill:#fef0e3,color:#b86a1c");
                        break;
                }
            }

            if (anyErrorEdges)
            {
                lines.Add("    FAIL([fail])");
                TryAddClass(declaredClasses, classDefs, "failure", "fill:#f8e3e3,color:#b13e3e");
                lines.Add("    FAIL:::failure");
            }

            var sb = new StringBuilder();
            sb.AppendLine("flowchart LR");
            foreach (var line in lines)
                sb.AppendLine(line);
            foreach (var classDef in classDefs)
                sb.AppendLine(classDef);

            // Correlation table: maps diagram NodeId → pipeline step name.
            // At runtime, set error.Metadata = error.Metadata with { NodeId = "<id>", PipelineStep = "<name>" }
            // to link a runtime error back to its diagram node.
            sb.AppendLine("%% --- Node correlation (ReasonMetadata.NodeId / PipelineStep) ---");
            foreach (var n in visible)
                sb.AppendLine($"%%   {n.NodeId} → {n.MethodName}");

            // Context hints: static OperationName/CorrelationId literals found in .WithContext() call.
            // Entity is shown inline on each node label. Runtime-only fields are noted as <runtime>.
            if (operationName != null || correlationId != null)
            {
                sb.AppendLine("%% --- Context (ResultContext fields from .WithContext() call) ---");
                sb.AppendLine("%%   Entity: auto-seeded per Result<T> type (see node labels)");
                if (operationName != null)
                    sb.AppendLine($"%%   OperationName: \"{operationName}\"");
                if (correlationId != null)
                    sb.AppendLine($"%%   CorrelationId: \"{correlationId}\"");
            }

            return sb.ToString().TrimEnd();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string BuildLabel(PipelineNode node)
        {
            var baseName = node.IsAsync ? node.MethodName + " \u26a1" : node.MethodName;

            if (node.OutputType == null)
                return baseName;

            var typePart = (node.InputType != null && node.InputType != node.OutputType)
                ? $"{node.InputType} \u2192 {node.OutputType}"  // → arrow
                : node.OutputType;

            return $"{baseName}<br/>{typePart}";
        }

        private static void EmitErrorEdges(
            List<string> lines,
            string nodeId,
            PipelineNode node,
            ref bool anyErrorEdges,
            string? fallbackEdge)
        {
            if (node.PossibleErrors.Count > 0)
            {
                foreach (var error in node.PossibleErrors)
                {
                    lines.Add($"    {nodeId} -->|{error}| FAIL");
                    anyErrorEdges = true;
                }
            }
            else if (fallbackEdge != null)
            {
                // Gatekeeper / TransformWithRisk: always show a failure path even when no specific error found
                lines.Add(fallbackEdge);
                anyErrorEdges = true;
            }
        }

        private static void TryAddClass(HashSet<string> declared, List<string> classDefs, string name, string style)
        {
            if (declared.Add(name))
                classDefs.Add($"    classDef {name} {style}");
        }
    }
}
