using REslava.ResultFlow.Generators.ResultFlow.Models;
using System.Collections.Generic;
using System.Text;

namespace REslava.ResultFlow.Generators.ResultFlow.CodeGeneration
{
    /// <summary>
    /// Renders a <c>{Method}_LayerView</c> Mermaid diagram (<c>flowchart TD</c>) from a pipeline model.
    /// Shows one node per traced method, grouped by architectural layer and class.
    /// Layer subgraph containers are styled by depth index (<c>Layer0_Style</c>, <c>Layer1_Style</c>, …)
    /// so that any layer name works — no name-to-color switch needed.
    /// Returns null when no layer information is available (nothing to group).
    /// </summary>
    internal static class ResultFlowLayerViewRenderer
    {
        public static string? Render(
            IReadOnlyList<PipelineNode> nodes,
            string rootMethodName,
            string rootClassName,
            string? rootLayer,
            string? operationName = null,
            string? linkMode = null,
            bool darkTheme = false)
        {
            // Collect direct subgraph nodes (depth-1 cross-method calls)
            var subMethods = CollectSubMethods(nodes);

            // Only emit when at least one method has a detected layer
            if (!HasAnyLayer(rootLayer, subMethods))
                return null;

            var sb = new StringBuilder();

            // Optional Mermaid front-matter title (requires Mermaid >= 10)
            if (operationName != null)
            {
                sb.AppendLine("---");
                sb.AppendLine($"title: {operationName}");
                sb.AppendLine("---");
            }

            sb.AppendLine(ResultFlowThemes.MermaidInit);
            sb.AppendLine("flowchart TD");
            sb.AppendLine();

            // ── Build layer → class → [(nodeId, methodName, classDefName)] map ──
            string rootNodeId = $"N_{rootMethodName}";
            string rootLayerKey = rootLayer ?? "unknown";

            var layerMap = new Dictionary<string, Dictionary<string, List<(string nodeId, string methodName, string classDefName)>>>();
            AddToLayerMap(layerMap, rootLayerKey, rootClassName, rootNodeId, rootMethodName, "operation");

            foreach (var sub in subMethods)
            {
                string subLayer = sub.Layer ?? "unknown";
                string subClass = sub.ClassName ?? sub.SubGraphName ?? sub.MethodName;
                string subNodeId = $"N_{sub.SubGraphName ?? sub.MethodName}";
                string subMethodName = sub.SubGraphName ?? sub.MethodName;
                AddToLayerMap(layerMap, subLayer, subClass, subNodeId, subMethodName, KindToClassDef(sub.Kind));
            }

            // ── Emit layer subgraphs — depth index drives ID and style ───────────
            var orderedLayers = GetOrderedLayers(layerMap);

            for (int depth = 0; depth < orderedLayers.Count; depth++)
            {
                string layer = orderedLayers[depth];
                string layerId = $"Layer{depth}";
                var classMap = layerMap[layer];

                sb.AppendLine($"  subgraph {layerId}[\"{layer}\"]");
                foreach (var kv in classMap)
                {
                    string classSubgraphId = $"L{depth}_{SanitizeId(kv.Key)}";
                    sb.AppendLine($"    subgraph {classSubgraphId}[\"{kv.Key}\"]");
                    foreach (var (nodeId, methodName, classDefName) in kv.Value)
                        sb.AppendLine($"      {nodeId}[\"{methodName}\"]:::{classDefName}");
                    sb.AppendLine("    end");
                }
                sb.AppendLine("  end");
                sb.AppendLine();
            }

            // ── Emit edges (fan-out: root → each sub-method) ─────────────────────
            bool anyErrorEdges = false;

            foreach (var sub in subMethods)
            {
                string subNodeId = $"N_{sub.SubGraphName ?? sub.MethodName}";
                string edgeLabel = BuildEdgeLabel(sub);
                sb.AppendLine($"  {rootNodeId} -->|\"{edgeLabel}\"| {subNodeId}");

                // Error edges from sub-method
                var errors = CollectErrors(sub);
                if (errors.Count > 0)
                {
                    foreach (var err in errors)
                        sb.AppendLine($"  {subNodeId} -->|\"{err}\"| FAIL");
                    anyErrorEdges = true;
                }
            }

            // SUCCESS: last sub-method → SUCCESS (or root if no subs)
            string successSource = subMethods.Count > 0
                ? $"N_{subMethods[subMethods.Count - 1].SubGraphName ?? subMethods[subMethods.Count - 1].MethodName}"
                : rootNodeId;
            sb.AppendLine($"  {successSource} -->|ok| SUCCESS");

            sb.AppendLine();

            // ── Terminals ─────────────────────────────────────────────────────────
            if (anyErrorEdges)
                sb.AppendLine("  FAIL([fail]):::failure");
            sb.AppendLine("  SUCCESS([success]):::success");

            // ── Click directives (when linkMode set) ──────────────────────────────
            if (!string.IsNullOrEmpty(linkMode) && linkMode != "none")
            {
                foreach (var sub in subMethods)
                {
                    string subNodeId = $"N_{sub.SubGraphName ?? sub.MethodName}";
                    var url = BuildClickUrl(sub.SourceFile, sub.SourceLine, linkMode);
                    if (url != null)
                        sb.AppendLine($"  click {subNodeId} \"{url}\"");
                }
            }

            sb.AppendLine();

            // ── Full theme block (includes all classDefs — operation, bind, failure, success, Layer*_Style) ──
            sb.AppendLine(darkTheme ? ResultFlowThemes.Dark : ResultFlowThemes.Light);

            // ── Apply depth-indexed style to each layer subgraph container ────────
            for (int depth = 0; depth < orderedLayers.Count; depth++)
                sb.AppendLine($"  class Layer{depth} Layer{depth}_Style");

            return sb.ToString().TrimEnd();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static string KindToClassDef(NodeKind kind) => kind switch
        {
            NodeKind.TransformWithRisk  => "bind",
            NodeKind.PureTransform      => "map",
            NodeKind.Gatekeeper         => "gatekeeper",
            NodeKind.SideEffectSuccess
            or NodeKind.SideEffectFailure
            or NodeKind.SideEffectBoth  => "sideeffect",
            NodeKind.Terminal           => "terminal",
            _                           => "operation"
        };

        private static List<PipelineNode> CollectSubMethods(IReadOnlyList<PipelineNode> nodes)
        {
            var result = new List<PipelineNode>();
            foreach (var n in nodes)
                if (n.Kind != NodeKind.Invisible && n.SubNodes != null && n.SubNodes.Count > 0)
                    result.Add(n);
            return result;
        }

        private static bool HasAnyLayer(string? rootLayer, List<PipelineNode> subMethods)
        {
            if (rootLayer != null) return true;
            foreach (var s in subMethods)
                if (s.Layer != null) return true;
            return false;
        }

        private static void AddToLayerMap(
            Dictionary<string, Dictionary<string, List<(string nodeId, string methodName, string classDefName)>>> map,
            string layer, string className, string nodeId, string methodName, string classDefName)
        {
            if (!map.TryGetValue(layer, out var classMap))
                map[layer] = classMap = new Dictionary<string, List<(string, string, string)>>();
            if (!classMap.TryGetValue(className, out var methods))
                classMap[className] = methods = new List<(string, string, string)>();
            methods.Add((nodeId, methodName, classDefName));
        }

        private static List<string> GetOrderedLayers(
            Dictionary<string, Dictionary<string, List<(string, string, string)>>> map)
        {
            var order = new[] { "Presentation", "Application", "Domain", "Infrastructure" };
            var result = new List<string>();
            foreach (var l in order)
                if (map.ContainsKey(l)) result.Add(l);
            foreach (var l in map.Keys)
                if (!result.Contains(l)) result.Add(l);
            return result;
        }

        private static string BuildEdgeLabel(PipelineNode sub)
        {
            string? inputType = sub.InputType;
            var errors = CollectErrors(sub);
            string? errorType = errors.Count > 0 ? string.Join(", ", errors) : null;

            if (inputType != null && errorType != null)
                return $"{inputType} / {errorType}";
            if (inputType != null)
                return inputType;
            if (errorType != null)
                return errorType;
            return "ok";
        }

        private static List<string> CollectErrors(PipelineNode node)
        {
            var result = new HashSet<string>();
            // Syntax-only package: errors come from ErrorType or ErrorHint (single values per node)
            if (node.ErrorType != null)
                result.Add(node.ErrorType);
            else if (node.ErrorHint != null)
                result.Add(node.ErrorHint);
            if (node.SubNodes != null)
                foreach (var sub in node.SubNodes)
                    foreach (var err in CollectErrors(sub))
                        result.Add(err);
            return new List<string>(result);
        }

        private static string SanitizeId(string name) =>
            name.Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace(" ", "_");

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
