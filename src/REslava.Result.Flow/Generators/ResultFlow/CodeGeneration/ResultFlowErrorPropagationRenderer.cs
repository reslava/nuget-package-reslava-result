using REslava.Result.Flow.Generators.ResultFlow.Models;
using System.Collections.Generic;
using System.Text;

namespace REslava.Result.Flow.Generators.ResultFlow.CodeGeneration
{
    /// <summary>
    /// Renders a <c>{Method}_ErrorPropagation</c> Mermaid diagram — per-layer error grouping.
    /// Shows which error types can escape from each architectural layer.
    /// Returns null when no layer information is available or no layers have errors.
    /// Only emitted by <c>REslava.Result.Flow</c> (requires semantic <c>PossibleErrors</c>).
    /// </summary>
    internal static class ResultFlowErrorPropagationRenderer
    {
        private static readonly string[] LayerOrder =
            { "Presentation", "Application", "Domain", "Infrastructure", "unknown" };

        public static string? Render(IReadOnlyList<PipelineNode> nodes, string? rootLayer)
        {
            // Guard: only emit when HasAnyLayer (same as _LayerView)
            if (!HasAnyLayer(rootLayer, nodes))
                return null;

            // Collect errors per layer
            var layerErrors = new Dictionary<string, HashSet<string>>();
            Collect(nodes, rootLayer, layerErrors);

            if (layerErrors.Count == 0)
                return null;

            var sb = new StringBuilder();
            sb.AppendLine("flowchart TD");
            sb.AppendLine();

            // Emit subgraphs in canonical layer order
            int counter = 0;
            // nodeId → already emitted (for edge phase)
            var emittedNodes = new List<string>();

            foreach (var layer in LayerOrder)
            {
                if (!layerErrors.TryGetValue(layer, out var errors) || errors.Count == 0)
                    continue;

                string layerClassDef = GetLayerClassDef(layer);
                sb.AppendLine($"  subgraph {SanitizeId(layer)}[\"{layer}\"]");

                foreach (var error in errors)
                {
                    string nodeId = $"E{counter++}";
                    sb.AppendLine($"    {nodeId}[\"{error}\"]:::failure");
                    emittedNodes.Add(nodeId);
                }

                sb.AppendLine("  end");
                sb.AppendLine();
            }

            // Emit edges: all error nodes → FAIL
            foreach (var nodeId in emittedNodes)
                sb.AppendLine($"  {nodeId} --> FAIL([fail]):::failure");

            sb.AppendLine();

            // classDef declarations
            var usedLayers = new HashSet<string>();
            foreach (var layer in layerErrors.Keys)
                usedLayers.Add(layer);

            foreach (var layer in LayerOrder)
            {
                if (!usedLayers.Contains(layer))
                    continue;
                var (fill, color) = GetLayerStyle(layer);
                sb.AppendLine($"  classDef {GetLayerClassDef(layer)} fill:{fill},color:{color}");
            }

            sb.AppendLine("  classDef failure fill:#f8e3e3,color:#b13e3e");
            sb.AppendLine();

            // Apply layer colors to subgraph containers via 'class' directive
            foreach (var layer in LayerOrder)
            {
                if (!usedLayers.Contains(layer))
                    continue;
                sb.AppendLine($"  class {SanitizeId(layer)} {GetLayerClassDef(layer)}");
            }

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

                // Determine this node's layer: use node.Layer when set (sub-method), else contextLayer
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

        private static string GetLayerClassDef(string layer) => layer switch
        {
            "Presentation"  => "layerPres",
            "Application"   => "layerApp",
            "Domain"        => "layerDomain",
            "Infrastructure"=> "layerInfra",
            _               => "layerUnknown"
        };

        private static (string fill, string color) GetLayerStyle(string layer) => layer switch
        {
            "Presentation"  => ("#eef4ff", "#2b4c7e"),
            "Application"   => ("#e8f7ee", "#1e6f43"),
            "Domain"        => ("#fff6e5", "#a36b00"),
            "Infrastructure"=> ("#f4e8ff", "#6a3fa0"),
            _               => ("#f5f5f5", "#555555")
        };

        private static string SanitizeId(string name) =>
            name.Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace(" ", "_");
    }
}
