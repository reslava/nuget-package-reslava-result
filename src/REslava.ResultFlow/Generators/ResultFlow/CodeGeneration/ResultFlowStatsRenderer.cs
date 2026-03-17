using REslava.ResultFlow.Generators.ResultFlow.Models;
using System.Collections.Generic;
using System.Text;

namespace REslava.ResultFlow.Generators.ResultFlow.CodeGeneration
{
    /// <summary>
    /// Produces the <c>{Method}_Stats</c> markdown table constant from a pipeline model.
    /// Pure string formatting — no Mermaid involved.
    /// Emitted alongside <c>{Method}_LayerView</c>.
    /// </summary>
    internal static class ResultFlowStatsRenderer
    {
        public static string Render(IReadOnlyList<PipelineNode> nodes, string? rootLayer)
        {
            int stepCount = 0;
            int asyncCount = 0;
            var errors = new HashSet<string>();
            var layers = new List<string>();
            int maxDepth = 0;

            Collect(nodes, errors, layers, rootLayer, depth: 0, ref stepCount, ref asyncCount, ref maxDepth);

            var sb = new StringBuilder();
            sb.AppendLine("| Property        | Value                                    |");
            sb.AppendLine("|-----------------|------------------------------------------|");
            sb.AppendLine($"| Steps           | {stepCount,-40} |");
            sb.AppendLine($"| Async steps     | {asyncCount,-40} |");
            sb.AppendLine($"| Possible errors | {(errors.Count > 0 ? string.Join(", ", errors) : "none"),-40} |");
            sb.AppendLine($"| Layers crossed  | {(layers.Count > 0 ? string.Join(" → ", layers) : "—"),-40} |");
            sb.Append($"| Max depth traced | {maxDepth,-40} |");

            return sb.ToString();
        }

        private static void Collect(
            IReadOnlyList<PipelineNode> nodes,
            HashSet<string> errors,
            List<string> layers,
            string? parentLayer,
            int depth,
            ref int stepCount,
            ref int asyncCount,
            ref int maxDepth)
        {
            if (depth > maxDepth)
                maxDepth = depth;

            foreach (var node in nodes)
            {
                if (node.Kind == NodeKind.Invisible)
                    continue;

                stepCount++;
                if (node.IsAsync)
                    asyncCount++;

                // ErrorHint is body-scan fallback
                if (node.ErrorHint != null)
                    errors.Add(node.ErrorHint);

                if (node.SubNodes != null && node.SubNodes.Count > 0)
                {
                    string? subLayer = node.Layer;
                    if (subLayer != null && (layers.Count == 0 || layers[layers.Count - 1] != subLayer))
                        layers.Add(subLayer);

                    Collect(node.SubNodes, errors, layers, subLayer, depth + 1,
                        ref stepCount, ref asyncCount, ref maxDepth);
                }
            }
        }
    }
}
