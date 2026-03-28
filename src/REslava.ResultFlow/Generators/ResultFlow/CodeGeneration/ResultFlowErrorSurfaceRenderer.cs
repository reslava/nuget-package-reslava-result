using REslava.ResultFlow.Generators.ResultFlow.Models;
using System.Collections.Generic;
using System.Text;

namespace REslava.ResultFlow.Generators.ResultFlow.CodeGeneration
{
    /// <summary>
    /// Renders a <c>{Method}_ErrorSurface</c> Mermaid diagram — fail-edges-only filtered view.
    /// Shows every error type the pipeline can produce and where it originates.
    /// Returns null when the pipeline has no error edges.
    /// </summary>
    internal static class ResultFlowErrorSurfaceRenderer
    {
        public static string? Render(IReadOnlyList<PipelineNode> nodes, bool darkTheme = false, string? pipelineId = null)
        {
            var failEdges = new List<(string nodeLabel, string errorLabel)>();
            Collect(nodes, failEdges);

            if (failEdges.Count == 0)
                return null;

            var sb = new StringBuilder();
            sb.AppendLine(ResultFlowThemes.MermaidInit);
            sb.AppendLine("flowchart LR");
            if (pipelineId != null)
                sb.AppendLine($"%% pipelineId: {pipelineId}");

            for (int i = 0; i < failEdges.Count; i++)
            {
                var (nodeLabel, errorLabel) = failEdges[i];
                sb.AppendLine($"  N{i}_{SanitizeId(nodeLabel)}[\"{nodeLabel}\"] -->|\"{errorLabel}\"| FAIL");
            }

            sb.AppendLine();
            sb.AppendLine("  FAIL([fail]):::failure");
            sb.AppendLine();
            sb.AppendLine(darkTheme
                ? "  classDef failure fill:#3a1f1f,color:#f2b8b8"
                : "  classDef failure fill:#f8e3e3,color:#b13e3e");
            sb.Append(darkTheme
                ? "  linkStyle default stroke:#666,stroke-width:1.5px"
                : "  linkStyle default stroke:#888,stroke-width:1.5px");

            return sb.ToString();
        }

        private static void Collect(
            IReadOnlyList<PipelineNode> nodes,
            List<(string, string)> failEdges)
        {
            foreach (var node in nodes)
            {
                if (node.Kind == NodeKind.Invisible)
                    continue;

                string label = node.SubGraphName ?? node.MethodName;

                // ErrorHint is body-scan fallback in ResultFlow package
                if (node.ErrorHint != null)
                {
                    failEdges.Add((label, node.ErrorHint));
                }
                else if (node.Kind == NodeKind.Gatekeeper || node.Kind == NodeKind.TransformWithRisk)
                {
                    failEdges.Add((label, "fail"));
                }

                if (node.SubNodes != null && node.SubNodes.Count > 0)
                    Collect(node.SubNodes, failEdges);
            }
        }

        private static string SanitizeId(string name) =>
            name.Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace(" ", "_");
    }
}
