using REslava.Result.Flow.Generators.ResultFlow.Models;
using System.Collections.Generic;
using System.Text;

namespace REslava.Result.Flow.Generators.ResultFlow.CodeGeneration
{
    /// <summary>
    /// Renders a <c>{Method}_ErrorPropagation</c> Mermaid diagram — per-layer error grouping.
    /// Shows which error types can escape from each architectural layer.
    /// Layer subgraph containers are styled by depth index (<c>Layer0_Style</c>, <c>Layer1_Style</c>, …).
    /// Returns null when no layer information is available or no layers have errors.
    /// Only emitted by <c>REslava.Result.Flow</c> (requires semantic <c>PossibleErrors</c>).
    /// </summary>
    internal static class ResultFlowErrorPropagationRenderer
    {
        private static readonly string[] CanonicalOrder =
            { "Presentation", "Application", "Domain", "Infrastructure" };

        public static string? Render(IReadOnlyList<PipelineNode> nodes, string? rootLayer, bool darkTheme = false, string? pipelineId = null)
        {
            // Guard: only emit when HasAnyLayer (same as _LayerView)
            if (!HasAnyLayer(rootLayer, nodes))
                return null;

            // Collect errors per layer
            var layerErrors = new Dictionary<string, HashSet<string>>();
            Collect(nodes, rootLayer, layerErrors);

            if (layerErrors.Count == 0)
                return null;

            // Build ordered list of layers that actually have errors (canonical order + custom)
            var orderedLayers = new List<string>();
            foreach (var l in CanonicalOrder)
                if (layerErrors.ContainsKey(l)) orderedLayers.Add(l);
            foreach (var l in layerErrors.Keys)
                if (!orderedLayers.Contains(l)) orderedLayers.Add(l);

            var sb = new StringBuilder();
            sb.AppendLine(ResultFlowThemes.MermaidInit);
            sb.AppendLine("flowchart TD");
            if (pipelineId != null)
                sb.AppendLine($"%% pipelineId: {pipelineId}");
            sb.AppendLine();

            // ── Emit layer subgraphs — depth index drives ID and style ───────────
            int counter = 0;
            var emittedNodes = new List<string>();

            for (int depth = 0; depth < orderedLayers.Count; depth++)
            {
                string layer = orderedLayers[depth];
                if (!layerErrors.TryGetValue(layer, out var errors) || errors.Count == 0)
                    continue;

                sb.AppendLine($"  subgraph Layer{depth}[\"{layer}\"]");

                foreach (var error in errors)
                {
                    string nodeId = $"E{counter++}";
                    sb.AppendLine($"    {nodeId}[\"{error}\"]:::failure");
                    emittedNodes.Add(nodeId);
                }

                sb.AppendLine("  end");
                sb.AppendLine();
            }

            // ── Emit edges: all error nodes → FAIL ───────────────────────────────
            foreach (var nodeId in emittedNodes)
                sb.AppendLine($"  {nodeId} --> FAIL([fail]):::failure");

            sb.AppendLine();

            // ── Full theme block (includes failure classDef, Layer*_Style, linkStyle) ──
            sb.AppendLine(darkTheme ? ResultFlowThemes.Dark : ResultFlowThemes.Light);

            // ── Apply depth-indexed style to each layer subgraph container ────────
            for (int depth = 0; depth < orderedLayers.Count; depth++)
                sb.AppendLine($"  class Layer{depth} Layer{depth}_Style");

            return sb.ToString().TrimEnd();
        }

        private static void Collect(
            IReadOnlyList<PipelineNode> nodes,
            string? contextLayer,
            Dictionary<string, HashSet<string>> layerErrors)
        {
            foreach (var node in nodes)
            {
                if (node.Kind == NodeKind.Invisible)
                    continue;

                string layer = node.Layer ?? contextLayer ?? "unknown";

                if (node.PossibleErrors != null)
                {
                    foreach (var err in node.PossibleErrors)
                    {
                        if (!layerErrors.ContainsKey(layer))
                            layerErrors[layer] = new HashSet<string>();
                        layerErrors[layer].Add(err);
                    }
                }

                if (node.SubNodes != null && node.SubNodes.Count > 0)
                    Collect(node.SubNodes, node.Layer ?? contextLayer, layerErrors);
            }
        }

        private static bool HasAnyLayer(string? rootLayer, IReadOnlyList<PipelineNode> nodes)
        {
            if (rootLayer != null)
                return true;
            foreach (var node in nodes)
            {
                if (node.Layer != null)
                    return true;
                if (node.SubNodes != null && HasAnyLayer(null, node.SubNodes))
                    return true;
            }
            return false;
        }

        private static string SanitizeId(string name) =>
            name.Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace(" ", "_");
    }
}
