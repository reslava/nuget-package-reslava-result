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
        public static string Render(IReadOnlyList<PipelineNode> nodes, string? linkMode = null)
        {
            var visible = FilterVisible(nodes);
            if (visible.Count == 0)
                return "flowchart LR";

            // Assign top-level node IDs
            AssignNodeIds(visible, prefix: "N");

            var lines = new List<string>();
            var classDefs = new List<string>();
            var declaredClasses = new HashSet<string>();
            int failureCounter = 0;

            RenderNodes(visible, lines, classDefs, declaredClasses, ref failureCounter, indent: "    ");

            // SUCCESS terminal: connect the last top-level node's happy path to SUCCESS
            string? lastConnectId = GetLastTopLevelConnectId(visible);
            if (lastConnectId != null)
            {
                lines.Add($"    {lastConnectId} -->|ok| SUCCESS");
                lines.Add("    SUCCESS([success]):::success");
                TryAddClass(declaredClasses, classDefs, "success", "fill:#e6f6ea,color:#1c7e4f");
            }

            var sb = new StringBuilder();
            sb.AppendLine("flowchart LR");
            foreach (var line in lines)
                sb.AppendLine(line);
            foreach (var classDef in classDefs)
                sb.AppendLine(classDef);

            // Click directives — emitted when linkMode is "vscode" and source location is available
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

        private static void AssignNodeIds(IReadOnlyList<PipelineNode> nodes, string prefix)
        {
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].NodeId = $"{prefix}{i}_{nodes[i].MethodName}";
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

        // ── Recursive node renderer ───────────────────────────────────────────

        /// <summary>
        /// Renders pipeline nodes into Mermaid lines, handling subgraph expansion recursively.
        /// <paramref name="failureCounter"/> is threaded as a ref so all failure nodes across
        /// nested subgraphs receive globally unique IDs (F0, F1, F2 …).
        /// </summary>
        private static void RenderNodes(
            IReadOnlyList<PipelineNode> nodes,
            List<string> lines,
            List<string> classDefs,
            HashSet<string> declaredClasses,
            ref int failureCounter,
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
                    AssignNodeIds(subVisible, prefix: $"{nodeId}_");

                    lines.Add($"{indent}subgraph {sgId}[\"{node.SubGraphName ?? node.MethodName}\"]");
                    RenderNodes(subVisible, lines, classDefs, declaredClasses, ref failureCounter,
                        indent: indent + "    ");
                    lines.Add($"{indent}end");

                    if (hasNext)
                    {
                        var nextNode = nodes[i + 1];
                        var nextConnectId = (nextNode.SubNodes != null && nextNode.SubNodes.Count > 0)
                            ? $"sg_{nextNode.NodeId}"
                            : nextNode.NodeId!;
                        lines.Add($"{indent}{sgId} -->|ok| {nextConnectId}");
                    }

                    // Fallback failure edge for the subgraph wrapper
                    var fId = $"F{failureCounter++}";
                    lines.Add($"{indent}{sgId} -->|\"{FailLabel(node)}\"| {fId}[\"Failure\"]:::failure");
                    TryAddClass(declaredClasses, classDefs, "failure", "fill:#f8e3e3,color:#b13e3e");
                    TryAddClass(declaredClasses, classDefs, "transform", "fill:#e3f0e8,color:#2f7a5c");
                    continue;
                }

                // ── Leaf node: standard box rendering ────────────────────────

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
                    {
                        var fId = $"F{failureCounter++}";
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::gatekeeper");
                        if (hasNext) lines.Add($"{indent}{nodeId} -->|pass| {resolvedNextId}");
                        lines.Add($"{indent}{nodeId} -->|\"{FailLabel(node)}\"| {fId}[\"Failure\"]:::failure");
                        TryAddClass(declaredClasses, classDefs, "gatekeeper", "fill:#e3e9fa,color:#3f5c9a");
                        TryAddClass(declaredClasses, classDefs, "failure",    "fill:#f8e3e3,color:#b13e3e");
                        break;
                    }
                    case NodeKind.TransformWithRisk:
                    {
                        var fId = $"F{failureCounter++}";
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::transform");
                        if (hasNext) lines.Add($"{indent}{nodeId} -->|ok| {resolvedNextId}");
                        lines.Add($"{indent}{nodeId} -->|\"{FailLabel(node)}\"| {fId}[\"Failure\"]:::failure");
                        TryAddClass(declaredClasses, classDefs, "transform", "fill:#e3f0e8,color:#2f7a5c");
                        TryAddClass(declaredClasses, classDefs, "failure",   "fill:#f8e3e3,color:#b13e3e");
                        break;
                    }
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
                    {
                        var fId = $"F{failureCounter++}";
                        lines.Add($"{indent}{nodeId}{{{{\"{label}\"}}}}:::terminal");
                        lines.Add($"{indent}{nodeId} -->|ok| SUCCESS");
                        lines.Add($"{indent}SUCCESS([success]):::success");
                        TryAddClass(declaredClasses, classDefs, "success", "fill:#e6f6ea,color:#1c7e4f");
                        lines.Add($"{indent}{nodeId} -->|fail| {fId}[\"Failure\"]:::failure");
                        TryAddClass(declaredClasses, classDefs, "terminal", "fill:#f2e3f5,color:#8a4f9e");
                        TryAddClass(declaredClasses, classDefs, "failure", "fill:#f8e3e3,color:#b13e3e");
                        break;
                    }

                    default: // Unknown
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::operation");
                        if (hasNext) lines.Add($"{indent}{nodeId} --> {resolvedNextId}");
                        TryAddClass(declaredClasses, classDefs, "operation", "fill:#fef0e3,color:#b86a1c");
                        break;
                }
            }
        }

        /// <summary>
        /// Builds the Mermaid node label, appending a type-travel secondary line when type info is available.
        /// <list type="bullet">
        ///   <item><description>OutputType known, differs from InputType → <c>MethodName&lt;br/&gt;InputType → OutputType</c></description></item>
        ///   <item><description>OutputType known, same as InputType (or no input) → <c>MethodName&lt;br/&gt;OutputType</c></description></item>
        ///   <item><description>OutputType null → <c>MethodName</c> (current behavior, no regression)</description></item>
        /// </list>
        /// </summary>
        private static string BuildLabel(PipelineNode node)
        {
            var baseName = node.IsAsync ? node.MethodName + " \u26a1" : node.MethodName;

            if (node.OutputType == null)
                return baseName;

            string typeLabel;
            if (node.InputType != null && node.InputType != node.OutputType)
                typeLabel = node.InputType + " \u2192 " + node.OutputType; // e.g. "User → UserDto"
            else
                typeLabel = node.OutputType; // e.g. "User"

            return baseName + "<br/>" + typeLabel;
        }

        /// <summary>
        /// Returns the failure edge label.
        /// <list type="bullet">
        ///   <item><description>Type-read mode: <c>ErrorType</c> is set — use it (HTML-escaped).</description></item>
        ///   <item><description>Body-scan mode: <c>ErrorHint</c> was syntactically extracted from a step
        ///         argument (e.g. <c>new NotFoundError(…)</c> or <c>ValidationError.Field(…)</c>) — use it.</description></item>
        ///   <item><description>Neither: plain <c>"fail"</c>.</description></item>
        /// </list>
        /// </summary>
        private static string FailLabel(PipelineNode node)
        {
            // Type-read mode wins: semantic type from the generic return type
            if (node.ErrorType != null)
            {
                var escaped = node.ErrorType.Replace("<", "&lt;").Replace(">", "&gt;");
                return "fail: " + escaped;
            }

            // Body-scan fallback: syntactically extracted from the step's error argument
            if (node.ErrorHint != null)
                return "fail: " + node.ErrorHint;

            return "fail";
        }

        private static void TryAddClass(HashSet<string> declared, List<string> classDefs, string name, string style)
        {
            if (declared.Add(name))
                classDefs.Add($"    classDef {name} {style}");
        }

        private static string? BuildClickUrl(string? sourceFile, int? sourceLine, string? linkMode)
        {
            if (sourceFile == null || sourceLine == null) return null;
            var path = sourceFile.Replace('\\', '/');
            if (linkMode == "vscode")
                return $"vscode://file/{path}:{sourceLine}";
            return null;
        }
    }
}
