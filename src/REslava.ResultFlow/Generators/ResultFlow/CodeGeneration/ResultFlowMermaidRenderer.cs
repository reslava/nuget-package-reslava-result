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
        public static string Render(
            IReadOnlyList<PipelineNode> nodes,
            string? methodTitle = null,
            string? seedMethodName = null,
            string? linkMode = null,
            bool darkTheme = false)
        {
            var visible = FilterVisible(nodes);
            if (visible.Count == 0)
                return "flowchart LR";

            // Assign top-level node IDs
            AssignNodeIds(visible, prefix: "N");

            var lines = new List<string>();
            var subgraphIds = new List<string>();
            int failureCounter = 0;

            // Entry node: the chain seed call (e.g. FindUser) as an :::operation node with ==> arrow
            if (seedMethodName != null)
            {
                var seedType = visible[0].InputType;
                var seedIsAsync = seedMethodName.EndsWith("Async");
                var rawSeed = (seedIsAsync && seedMethodName.EndsWith("Async"))
                    ? seedMethodName.Substring(0, seedMethodName.Length - 5)
                    : seedMethodName;
                var seedDisplayName = seedIsAsync ? rawSeed + "\u26a1" : rawSeed;
                var seedLabel = seedType != null
                    ? $"{seedDisplayName}<br/>\u2192 {seedType}"
                    : seedDisplayName;
                var firstNode = visible[0];
                var firstConnectId = (firstNode.SubNodes != null && firstNode.SubNodes.Count > 0)
                    ? $"sg_{firstNode.NodeId}"
                    : firstNode.NodeId!;
                lines.Add($"    ENTRY_ROOT[\"{seedLabel}\"]:::operation ==> {firstConnectId}");
            }

            RenderNodes(visible, lines, subgraphIds, ref failureCounter, indent: "    ");

            // SUCCESS terminal: connect the last top-level node's happy path to SUCCESS
            string? lastConnectId = GetLastTopLevelConnectId(visible);
            if (lastConnectId != null)
            {
                lines.Add($"    {lastConnectId} -->|ok| SUCCESS");
                lines.Add("    SUCCESS([success]):::success");
            }

            var sb = new StringBuilder();
            if (methodTitle != null)
            {
                var titleIsAsync = methodTitle.EndsWith("Async");
                var cleanTitle = titleIsAsync ? methodTitle.Substring(0, methodTitle.Length - 5) : methodTitle;
                var titleName = titleIsAsync ? cleanTitle + "\u26a1" : cleanTitle;
                var outputType = visible.Count > 0 ? visible[visible.Count - 1].OutputType : null;
                var typeAnnotation = " \u2192 \u27e8" + (outputType ?? "") + "\u27e9";
                sb.AppendLine("---");
                sb.AppendLine($"title: {titleName}{typeAnnotation}");
                sb.AppendLine("---");
            }
            sb.AppendLine(darkTheme ? ResultFlowThemes.MermaidInitDark : ResultFlowThemes.MermaidInit);
            sb.AppendLine("flowchart LR");
            foreach (var line in lines)
                sb.AppendLine(line);

            // Emit full theme block (classDefs + linkStyle)
            sb.AppendLine(darkTheme ? ResultFlowThemes.Dark : ResultFlowThemes.Light);

            // Assign subgraphStyle to each subgraph by ID (one line per ID)
            foreach (var sgId in subgraphIds)
                sb.AppendLine($"class {sgId} subgraphStyle");

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
            List<string> subgraphIds,
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
                    subgraphIds.Add(sgId);
                    var subVisible = FilterVisible(node.SubNodes);
                    AssignNodeIds(subVisible, prefix: $"{nodeId}_");

                    lines.Add($"{indent}subgraph {sgId}[\"{node.SubGraphName ?? node.MethodName}\"]");

                    // Entry arrow: thick ==> marks where execution enters the expanded method
                    if (subVisible.Count > 0)
                    {
                        var innerIndent = indent + "    ";
                        var entryId = $"ENTRY_{nodeId}";
                        var firstConnectId = (subVisible[0].SubNodes != null && subVisible[0].SubNodes.Count > 0)
                            ? $"sg_{subVisible[0].NodeId}"
                            : subVisible[0].NodeId!;
                        lines.Add($"{innerIndent}{entryId}[ ]:::entry");
                        lines.Add($"{innerIndent}{entryId}[ ] ==> {firstConnectId}");
                    }

                    RenderNodes(subVisible, lines, subgraphIds, ref failureCounter,
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
                        var gatekeeperLabel = node.PredicateText != null
                            ? "<span title='" + node.PredicateText.Replace("\"", "&quot;").Replace("'", "&#39;") + "'>" + label + "</span>"
                            : label;
                        lines.Add($"{indent}{nodeId}[\"{gatekeeperLabel}\"]:::gatekeeper");
                        if (hasNext) lines.Add($"{indent}{nodeId} -->|pass| {resolvedNextId}");
                        lines.Add($"{indent}{nodeId} -->|\"{FailLabel(node)}\"| {fId}[\"Failure\"]:::failure");
                        break;
                    }
                    case NodeKind.TransformWithRisk:
                    {
                        var fId = $"F{failureCounter++}";
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::bind");
                        if (hasNext) lines.Add($"{indent}{nodeId} -->|ok| {resolvedNextId}");
                        lines.Add($"{indent}{nodeId} -->|\"{FailLabel(node)}\"| {fId}[\"Failure\"]:::failure");
                        break;
                    }
                    case NodeKind.PureTransform:
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::map");
                        if (hasNext) lines.Add($"{indent}{nodeId} --> {resolvedNextId}");
                        break;

                    case NodeKind.SideEffectSuccess:
                    case NodeKind.SideEffectFailure:
                    case NodeKind.SideEffectBoth:
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::sideeffect");
                        if (hasNext) lines.Add($"{indent}{nodeId} --> {resolvedNextId}");
                        break;

                    case NodeKind.Terminal:
                    {
                        var fId = $"F{failureCounter++}";
                        lines.Add($"{indent}{nodeId}{{{{\"{label}\"}}}}:::terminal");
                        lines.Add($"{indent}{nodeId} -->|ok| SUCCESS");
                        lines.Add($"{indent}SUCCESS([success]):::success");
                        lines.Add($"{indent}{nodeId} -->|fail| {fId}[\"Failure\"]:::failure");
                        break;
                    }

                    default: // Unknown
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::operation");
                        if (hasNext) lines.Add($"{indent}{nodeId} --> {resolvedNextId}");
                        break;
                }
            }
        }

        /// <summary>
        /// Builds the Mermaid node label, appending a type-travel secondary line when type info is available.
        /// </summary>
        private static string BuildLabel(PipelineNode node)
        {
            var rawName = (node.IsAsync && node.MethodName.EndsWith("Async"))
                ? node.MethodName.Substring(0, node.MethodName.Length - 5)
                : node.MethodName;
            var baseName = node.IsAsync ? rawName + "\u26a1" : rawName;

            if (node.OutputType == null)
                return baseName;

            string typeLabel;
            if (node.InputType != null && node.InputType != node.OutputType)
                typeLabel = node.InputType + " \u2192 " + node.OutputType;
            else
                typeLabel = node.OutputType;

            return baseName + "<br/>" + typeLabel;
        }

        /// <summary>
        /// Returns the failure edge label.
        /// </summary>
        private static string FailLabel(PipelineNode node)
        {
            if (node.ErrorType != null)
            {
                var escaped = node.ErrorType.Replace("<", "&lt;").Replace(">", "&gt;");
                return "fail: " + escaped;
            }
            if (node.ErrorHint != null)
                return "fail: " + node.ErrorHint;
            return "fail";
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
