using REslava.Result.Flow.Generators.ResultFlow.Models;
using System.Collections.Generic;
using System.Text;

namespace REslava.Result.Flow.Generators.ResultFlow.CodeGeneration
{
    /// <summary>
    /// Renders a <c>{Method}_LayerView</c> Mermaid diagram (<c>flowchart TD</c>) from a pipeline model.
    /// Shows one node per traced method, grouped by architectural layer and class.
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
            string? linkMode = null)
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

            sb.AppendLine("flowchart TD");
            sb.AppendLine();

            // ── Build layer → class → [(nodeId, methodName)] map ─────────────
            string rootNodeId = $"N_{rootMethodName}";
            string rootLayerKey = rootLayer ?? "unknown";

            var layerMap = new Dictionary<string, Dictionary<string, List<(string nodeId, string methodName)>>>();
            AddToLayerMap(layerMap, rootLayerKey, rootClassName, rootNodeId, rootMethodName);

            foreach (var sub in subMethods)
            {
                string subLayer = sub.Layer ?? "unknown";
                string subClass = sub.ClassName ?? sub.SubGraphName ?? sub.MethodName;
                string subNodeId = $"N_{sub.SubGraphName ?? sub.MethodName}";
                string subMethodName = sub.SubGraphName ?? sub.MethodName;
                AddToLayerMap(layerMap, subLayer, subClass, subNodeId, subMethodName);
            }

            // ── Emit subgraphs (Presentation → Application → Domain → Infrastructure → unknown) ──
            var orderedLayers = GetOrderedLayers(layerMap);
            var emittedClassDefs = new HashSet<string>();

            foreach (var layer in orderedLayers)
            {
                var classMap = layerMap[layer];
                string classDefName = GetLayerClassDef(layer);

                sb.AppendLine($"  subgraph {SanitizeId(layer)}[\"{layer}\"]");
                foreach (var kv in classMap)
                {
                    sb.AppendLine($"    subgraph {SanitizeId(kv.Key)}[\"{kv.Key}\"]");
                    foreach (var (nodeId, methodName) in kv.Value)
                        sb.AppendLine($"      {nodeId}[\"{methodName}\"]:::{classDefName}");
                    sb.AppendLine("    end");
                }
                sb.AppendLine("  end");
                sb.AppendLine();

                emittedClassDefs.Add(classDefName);
            }

            // ── Emit edges (fan-out: root → each sub-method) ─────────────────
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

            // ── Terminals ─────────────────────────────────────────────────────
            if (anyErrorEdges)
                sb.AppendLine("  FAIL([fail]):::failure");
            sb.AppendLine("  SUCCESS([success]):::success");

            // ── Click directives (when linkMode set) ──────────────────────────
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

            // ── classDef styles ───────────────────────────────────────────────
            foreach (var layer in orderedLayers)
            {
                var classDefName = GetLayerClassDef(layer);
                if (emittedClassDefs.Remove(classDefName))
                    sb.AppendLine($"  classDef {classDefName} {GetLayerStyle(layer)}");
            }
            if (anyErrorEdges)
                sb.AppendLine("  classDef failure fill:#f8e3e3,color:#b13e3e");
            sb.AppendLine("  classDef success fill:#e6f6ea,color:#1c7e4f");

            // Apply layer colors to outer subgraph containers via 'class' directive
            sb.AppendLine();
            foreach (var layer in orderedLayers)
                sb.AppendLine($"  class {SanitizeId(layer)} {GetLayerClassDef(layer)}");

            return sb.ToString().TrimEnd();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

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
            Dictionary<string, Dictionary<string, List<(string, string)>>> map,
            string layer, string className, string nodeId, string methodName)
        {
            if (!map.TryGetValue(layer, out var classMap))
                map[layer] = classMap = new Dictionary<string, List<(string, string)>>();
            if (!classMap.TryGetValue(className, out var methods))
                classMap[className] = methods = new List<(string, string)>();
            methods.Add((nodeId, methodName));
        }

        private static List<string> GetOrderedLayers(
            Dictionary<string, Dictionary<string, List<(string, string)>>> map)
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
            foreach (var err in node.PossibleErrors)
                result.Add(err);
            if (node.SubNodes != null)
                foreach (var sub in node.SubNodes)
                    foreach (var err in CollectErrors(sub))
                        result.Add(err);
            return new List<string>(result);
        }

        private static string GetLayerClassDef(string layer) => layer switch
        {
            "Presentation"  => "layerPres",
            "Application"   => "layerApp",
            "Domain"        => "layerDomain",
            "Infrastructure" => "layerInfra",
            _               => "layerUnknown"
        };

        private static string GetLayerStyle(string layer) => layer switch
        {
            "Presentation"  => "fill:#eef4ff,color:#2b4c7e",
            "Application"   => "fill:#e8f7ee,color:#1e6f43",
            "Domain"        => "fill:#fff6e5,color:#a36b00",
            "Infrastructure" => "fill:#f4e8ff,color:#6a3fa0",
            _               => "fill:#f5f5f5,color:#555555"
        };

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
