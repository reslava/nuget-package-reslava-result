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
            string? methodTitle = null,
            string? seedMethodName = null,
            string? operationName = null,
            string? correlationId = null,
            string? linkMode = null,
            bool darkTheme = false,
            string? entrySourceFile = null,
            int? entrySourceLine = null,
            string? pipelineId = null,
            bool typeLabels = false)
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
            // Use pipelineId as scope key so node IDs match those emitted in _PipelineRegistry.g.cs.
            AssignNodeIds(visible, scopeKey: pipelineId ?? "pipeline");

            var lines = new List<string>();
            var subgraphIds = new List<string>();
            bool anyErrorEdges = false;
            var collectedErrors = new List<string>(); // tracks error names for FAIL label

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

            RenderNodes(visible, lines, subgraphIds, ref anyErrorEdges, collectedErrors, indent: "    ", typeLabels: typeLabels);

            // SUCCESS terminal: connect the last top-level node's happy path to SUCCESS
            string? lastConnectId = GetLastTopLevelConnectId(visible);
            if (lastConnectId != null)
            {
                lines.Add($"    {lastConnectId} -->|ok| SUCCESS");
                lines.Add("    SUCCESS([success]):::success");
            }

            if (anyErrorEdges)
            {
                var failLabel = BuildFailLabel(collectedErrors);
                lines.Add($"    FAIL({failLabel}):::failure");
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
            if (pipelineId != null)
                sb.AppendLine($"%% pipelineId: {pipelineId}");
            foreach (var line in lines)
                sb.AppendLine(line);

            // Emit full theme block (classDefs + linkStyle)
            sb.AppendLine(darkTheme ? ResultFlowThemes.Dark : ResultFlowThemes.Light);

            // Assign subgraphStyle to each subgraph by ID (one line per ID)
            foreach (var sgId in subgraphIds)
                sb.AppendLine($"class {sgId} subgraphStyle");

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
                // ENTRY_ROOT click — navigates to the [ResultFlow] method declaration
                var entryUrl = BuildClickUrl(entrySourceFile, entrySourceLine, linkMode);
                if (entryUrl != null && seedMethodName != null)
                    sb.AppendLine($"    click ENTRY_ROOT \"{entryUrl}\" \"Go to {seedMethodName}\"");

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
        /// Assigns stable <see cref="PipelineNode.NodeId"/> values using FNV-1a hashing.
        /// Top-level nodes use <paramref name="scopeKey"/> = pipelineId (matches registry _Info nodeIds).
        /// Sub-graph nodes use their parent's already-assigned NodeId as the scope key so IDs are
        /// globally unique within the diagram and traceable back to their parent pipeline.
        /// </summary>
        private static void AssignNodeIds(IReadOnlyList<PipelineNode> nodes, string scopeKey)
        {
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].NodeId = ShortHash.Compute(scopeKey, nodes[i].MethodName, i.ToString());
        }

        // ── Recursive node renderer ───────────────────────────────────────────

        /// <summary>
        /// Renders a list of pipeline nodes into Mermaid lines.
        /// Handles subgraph expansion for <see cref="PipelineNode.SubNodes"/> recursively.
        /// </summary>
        private static void RenderNodes(
            IReadOnlyList<PipelineNode> nodes,
            List<string> lines,
            List<string> subgraphIds,
            ref bool anyErrorEdges,
            List<string> collectedErrors,
            string indent,
            bool typeLabels = false)
        {
            // Returns the labelled success edge text, using OutputType when typeLabels is on.
            string SuccessLabel(PipelineNode n, string fallback) =>
                typeLabels && n.OutputType != null ? $"\"{n.OutputType}\"" : fallback;
            // Returns a --> or -->|type| edge line depending on typeLabels and OutputType availability.
            string TypeEdge(string from, string to, PipelineNode n, string ind) =>
                typeLabels && n.OutputType != null
                    ? $"{ind}{from} -->|\"{n.OutputType}\"| {to}"
                    : $"{ind}{from} --> {to}";

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

                    // Sub-node scope: parent nodeId ensures globally unique hashes within the diagram
                    AssignNodeIds(subVisible, scopeKey: nodeId);

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

                    RenderNodes(subVisible, lines, subgraphIds, ref anyErrorEdges, collectedErrors,
                        indent: indent + "    ", typeLabels: typeLabels);
                    lines.Add($"{indent}end");

                    // Connect: previous step → subgraph, subgraph → next step
                    // Edge FROM this node to next uses the subgraph ID
                    if (hasNext)
                    {
                        var nextNode = nodes[i + 1];
                        var nextConnectId = (nextNode.SubNodes != null && nextNode.SubNodes.Count > 0)
                            ? $"sg_{nextNode.NodeId}"
                            : nextNode.NodeId!;
                        lines.Add($"{indent}{sgId} -->|{SuccessLabel(node, "ok")}| {nextConnectId}");
                    }

                    // Error edges from the Bind wrapper itself (if any) still go to outer FAIL
                    EmitErrorEdges(lines, sgId, node, ref anyErrorEdges, collectedErrors,
                        fallbackEdge: $"{indent}{sgId} -->|fail| FAIL", indent: indent);
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
                    {
                        var gatekeeperLabel = node.PredicateText != null
                            ? "<span title='" + node.PredicateText.Replace("\"", "\u201c").Replace("'", "\u2019") + "'>ℹ️" + label + "</span>"
                            : label;
                        lines.Add($"{indent}{nodeId}[\"{gatekeeperLabel}\"]:::gatekeeper");
                        if (hasNext) lines.Add($"{indent}{nodeId} -->|{SuccessLabel(node, "pass")}| {resolvedNextId}");
                        EmitErrorEdges(lines, nodeId, node, ref anyErrorEdges, collectedErrors,
                            fallbackEdge: $"{indent}{nodeId} -->|fail| FAIL", indent: indent);
                        break;
                    }

                    case NodeKind.TransformWithRisk:
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::bind");
                        if (hasNext) lines.Add($"{indent}{nodeId} -->|{SuccessLabel(node, "ok")}| {resolvedNextId}");
                        EmitErrorEdges(lines, nodeId, node, ref anyErrorEdges, collectedErrors,
                            fallbackEdge: $"{indent}{nodeId} -->|fail| FAIL", indent: indent);
                        break;

                    case NodeKind.PureTransform:
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::map");
                        if (hasNext) lines.Add(TypeEdge(nodeId, resolvedNextId, node, indent));
                        break;

                    case NodeKind.SideEffectSuccess:
                    case NodeKind.SideEffectFailure:
                    case NodeKind.SideEffectBoth:
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::sideeffect");
                        if (hasNext) lines.Add(TypeEdge(nodeId, resolvedNextId, node, indent));
                        break;

                    case NodeKind.Terminal:
                        lines.Add($"{indent}{nodeId}{{{{\"{label}\"}}}}:::terminal");
                        lines.Add($"{indent}{nodeId} -->|ok| SUCCESS");
                        lines.Add($"{indent}SUCCESS([success]):::success");
                        if (node.MatchBranchLabels != null && node.MatchBranchLabels.Count > 0)
                        {
                            foreach (var branch in node.MatchBranchLabels)
                            {
                                lines.Add($"{indent}{nodeId} -->|{branch}| FAIL");
                                anyErrorEdges = true;
                                collectedErrors.Add(branch);
                            }
                        }
                        else
                        {
                            lines.Add($"{indent}{nodeId} -->|fail| FAIL");
                            anyErrorEdges = true;
                        }
                        break;

                    default: // Unknown — entry point or unrecognised operation
                        lines.Add($"{indent}{nodeId}[\"{label}\"]:::operation");
                        if (hasNext) lines.Add(TypeEdge(nodeId, resolvedNextId, node, indent));
                        EmitErrorEdges(lines, nodeId, node, ref anyErrorEdges, collectedErrors, fallbackEdge: null, indent: indent);
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
            var rawName = (node.IsAsync && node.MethodName.EndsWith("Async"))
                ? node.MethodName.Substring(0, node.MethodName.Length - 5)
                : node.MethodName;
            var baseName = node.IsAsync ? rawName + "\u26a1" : rawName;

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
            List<string> collectedErrors,
            string? fallbackEdge,
            string indent = "    ")
        {
            if (node.PossibleErrors.Count > 0)
            {
                foreach (var error in node.PossibleErrors)
                {
                    lines.Add($"{indent}{nodeId} -->|{error}| FAIL");
                    anyErrorEdges = true;
                    collectedErrors.Add(error);
                }
            }
            else if (fallbackEdge != null)
            {
                // Gatekeeper / TransformWithRisk: always show a failure path even when no specific error found
                lines.Add(fallbackEdge);
                anyErrorEdges = true;
            }
        }

        /// <summary>
        /// Builds the FAIL node label from collected error names:
        /// 0 errors → <c>[fail]</c>
        /// 1–3 errors → <c>["fail\nErr1\nErr2"]</c> (inline, "Error" suffix stripped)
        /// 4+ errors → <c>["&lt;span title='Err1, Err2, ...'&gt;ℹ️fail&lt;/span&gt;"]</c> (tooltip)
        /// </summary>
        private static string BuildFailLabel(List<string> errors)
        {
            var distinct = new List<string>();
            var seen = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);
            foreach (var e in errors)
                if (seen.Add(e)) distinct.Add(e);

            if (distinct.Count == 0)
                return "[fail]";

            var stripped = new List<string>(distinct.Count);
            foreach (var e in distinct)
                stripped.Add(StripErrorSuffix(e));

            if (stripped.Count <= 3)
                return "[\"fail\\n" + string.Join("\\n", stripped) + "\"]";

            var tooltip = string.Join(", ", stripped);
            return "[\"<span title='" + tooltip + "'>ℹ️fail</span>\"]";
        }

        private static string StripErrorSuffix(string name)
            => name.EndsWith("Error") && name.Length > 5
                ? name.Substring(0, name.Length - 5)
                : name;

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
