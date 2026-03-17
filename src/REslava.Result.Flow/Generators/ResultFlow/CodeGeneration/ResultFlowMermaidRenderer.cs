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
            string? correlationId = null,
            string? linkMode = null)
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
            AssignNodeIds(visible, prefix: "N");

            var lines = new List<string>();
            var classDefs = new List<string>();
            var declaredClasses = new HashSet<string>();
            bool anyErrorEdges = false;

            RenderNodes(visible, lines, classDefs, declaredClasses, ref anyErrorEdges, indent: "    ");

            // SUCCESS terminal: connect the last top-level node's happy path to SUCCESS
            string? lastConnectId = GetLastTopLevelConnectId(visible);
            if (lastConnectId != null)
            {
                lines.Add($"    {lastConnectId} -->|ok| SUCCESS");
                lines.Add("    SUCCESS([success]):::success");
                TryAddClass(declaredClasses, classDefs, "success", "fill:#e6f6ea,color:#1c7e4f");
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

            // Click directives — emitted when linkMode is "vscode" or "github" and source location is available
            if (!string.IsNullOrEmpty(linkMode) && linkMode != "none")
            {
                foreach (var n in visible)
                {
                    var url = BuildClickUrl(n.SourceFile, n.SourceLine, linkMode);
                    if (url != null)
                        sb.AppendLine($"    click {n.NodeId} \"{url}\" \"Go to {n.MethodName}\"");
                }
            }

            return sb.ToString().TrimEnd();
        }

        // ── Node ID assignment ────────────────────────────────────────────────

        /// <summary>
        /// Assigns stable <see cref="PipelineNode.NodeId"/> values to all visible nodes
        /// (top-level only — sub-graph nodes are assigned separately during rendering
        /// with a depth-qualified prefix so IDs are globally unique).
        /// </summary>
        private static void AssignNodeIds(IReadOnlyList<PipelineNode> nodes, string prefix)
        {
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].NodeId = $"{prefix}{i}_{nodes[i].MethodName}";
        }

        // ── Recursive node renderer ───────────────────────────────────────────

        /// <summary>
        /// Renders a list of pipeline nodes into Mermaid lines.
        /// Handles subgraph expansion for <see cref="PipelineNode.SubNodes"/> recursively.
        /// </summary>
        private static void RenderNodes(
            IReadOnlyList<PipelineNode> nodes,
            List<string> lines,
            List<string> classDefs,
            HashSet<string> declaredClasses,
            ref bool anyErrorEdges,
            string indent)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var nodeId = node.NodeId!;
                var label = BuildLabel(node);
                bool hasNext = i < nodes.Count - 1;

                // ── Expanded Bind node: emit subgraph instead of a box ──────
                if (node.SubNodes != null && node.SubNodes.Count > 0)
                {
                    var sgId = $"sg_{nodeId}";
                    var subVisible = FilterVisible(node.SubNodes);

                    // Assign sub-node IDs with depth-qualified prefix so they are globally unique
                    AssignNodeIds(subVisible, prefix: $"{nodeId}_");

                    lines.Add($"{indent}subgraph {sgId}[\"{node.SubGraphName ?? node.MethodName}\"]");
                    RenderNodes(subVisible, lines, classDefs, declaredClasses, ref anyErrorEdges,
                        indent: indent + "    ");
                    lines.Add($"{indent}end");

                    // Connect: previous step → subgraph, subgraph → next step
                    // Edge FROM this node to next uses the subgraph ID
                    if (hasNext)
                    {
                        var nextNode = nodes[i + 1];
                        var nextConnectId = (nextNode.SubNodes != null && nextNode.SubNodes.Count > 0)
                            ? $"sg_{nextNode.NodeId}"
                            : nextNode.NodeId!;
                        lines.Add($"{indent}{sgId} -->|ok| {nextConnectId}");
                    }

                    // Error edges from the Bind wrapper itself (if any) still go to outer FAIL
                    EmitErrorEdges(lines, sgId, node, ref anyErrorEdges,
                        fallbackEdge: $"{indent}{sgId} -->|fail| FAIL", indent: indent);
                    TryAddClass(declaredClasses, classDefs, "transform", "fill:#e3f0e8,color:#2f7a5c");
                    continue;
                }

                // ── Leaf node: standard box rendering ────────────────────────

                // Resolve the connect-to ID: if next node is expanded, connect to its subgraph
                string resolvedNextId = string.Empty;
                if (hasNext)
                {
                    var nextNode = nodes[i + 1];
                    resolvedNextId = (nextNode.SubNodes != null && nextNode.SubNodes.Count > 0)
                        ? $"sg_{nextNode.NodeId}"
                        : nextNode.NodeId!;
                }

                switch (node.Kind)
                {
                    case NodeKind.Gatekeeper:
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::gatekeeper");
                        if (hasNext) lines.Add($"{indent}{nodeId} -->|pass| {resolvedNextId}");
                        EmitErrorEdges(lines, nodeId, node, ref anyErrorEdges,
                            fallbackEdge: $"{indent}{nodeId} -->|fail| FAIL", indent: indent);
                        TryAddClass(declaredClasses, classDefs, "gatekeeper", "fill:#e3e9fa,color:#3f5c9a");
                        break;

                    case NodeKind.TransformWithRisk:
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::transform");
                        if (hasNext) lines.Add($"{indent}{nodeId} -->|ok| {resolvedNextId}");
                        EmitErrorEdges(lines, nodeId, node, ref anyErrorEdges,
                            fallbackEdge: $"{indent}{nodeId} -->|fail| FAIL", indent: indent);
                        TryAddClass(declaredClasses, classDefs, "transform", "fill:#e3f0e8,color:#2f7a5c");
                        break;

                    case NodeKind.PureTransform:
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::transform");
                        if (hasNext) lines.Add($"{indent}{nodeId} --> {resolvedNextId}");
                        TryAddClass(declaredClasses, classDefs, "transform", "fill:#e3f0e8,color:#2f7a5c");
                        break;

                    case NodeKind.SideEffectSuccess:
                    case NodeKind.SideEffectFailure:
                    case NodeKind.SideEffectBoth:
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::sideeffect");
                        if (hasNext) lines.Add($"{indent}{nodeId} --> {resolvedNextId}");
                        TryAddClass(declaredClasses, classDefs, "sideeffect", "fill:#fff4d9,color:#b8882c");
                        break;

                    case NodeKind.Terminal:
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::terminal");
                        TryAddClass(declaredClasses, classDefs, "terminal", "fill:#f2e3f5,color:#8a4f9e");
                        break;

                    default: // Unknown — entry point or unrecognised operation
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::operation");
                        if (hasNext) lines.Add($"{indent}{nodeId} --> {resolvedNextId}");
                        EmitErrorEdges(lines, nodeId, node, ref anyErrorEdges, fallbackEdge: null, indent: indent);
                        TryAddClass(declaredClasses, classDefs, "operation", "fill:#fef0e3,color:#b86a1c");
                        break;
                }
            }
        }

        /// <summary>
        /// Returns the connect-ID of the last top-level visible node (the node or subgraph ID
        /// that should receive the <c>--&gt;|ok| SUCCESS</c> edge).
        /// Returns null for empty pipelines.
        /// </summary>
        private static string? GetLastTopLevelConnectId(IReadOnlyList<PipelineNode> visible)
        {
            if (visible.Count == 0) return null;
            var last = visible[visible.Count - 1];
            // Terminal nodes (Match) are already pipeline endpoints — no outbound SUCCESS edge
            if (last.Kind == NodeKind.Terminal) return null;
            return (last.SubNodes != null && last.SubNodes.Count > 0)
                ? $"sg_{last.NodeId}"
                : last.NodeId;
        }

        private static List<PipelineNode> FilterVisible(IReadOnlyList<PipelineNode> nodes)
        {
            var result = new List<PipelineNode>(nodes.Count);
            foreach (var n in nodes)
                if (n.Kind != NodeKind.Invisible) result.Add(n);
            return result;
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
            string? fallbackEdge,
            string indent = "    ")
        {
            if (node.PossibleErrors.Count > 0)
            {
                foreach (var error in node.PossibleErrors)
                {
                    lines.Add($"{indent}{nodeId} -->|{error}| FAIL");
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

        /// <summary>
        /// Builds the URL for a Mermaid <c>click</c> directive based on <paramref name="linkMode"/>.
        /// Returns null when source location is unavailable or the mode is unrecognised.
        /// </summary>
        private static string? BuildClickUrl(string? sourceFile, int? sourceLine, string? linkMode)
        {
            if (sourceFile == null || sourceLine == null) return null;
            var path = sourceFile.Replace('\\', '/');

            if (linkMode == "vscode")
                return $"vscode://file/{path}:{sourceLine}";

            // "github" mode: requires RepositoryUrl + branch from MSBuild — not yet implemented.
            return null;
        }
    }
}
