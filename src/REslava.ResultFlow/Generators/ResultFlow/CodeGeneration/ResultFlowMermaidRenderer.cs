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
        public static string Render(IReadOnlyList<PipelineNode> nodes)
        {
            // Filter out invisible nodes (WithSuccess, WithSuccessAsync, etc.)
            var visible = new List<PipelineNode>();
            foreach (var node in nodes)
            {
                if (node.Kind != NodeKind.Invisible)
                    visible.Add(node);
            }

            if (visible.Count == 0)
                return "flowchart LR";

            var lines = new List<string>();
            var classDefs = new List<string>();
            var declaredClasses = new HashSet<string>();

            for (int i = 0; i < visible.Count; i++)
            {
                var node = visible[i];
                var nodeId = $"N{i}_{node.MethodName}";
                var label = BuildLabel(node);
                bool hasNext = i < visible.Count - 1;
                string nextId = hasNext ? $"N{i + 1}_{visible[i + 1].MethodName}" : string.Empty;

                switch (node.Kind)
                {
                    case NodeKind.Gatekeeper:
                        lines.Add($"    {nodeId}[\"{label}\"]:::gatekeeper");
                        if (hasNext) lines.Add($"    {nodeId} -->|pass| {nextId}");
                        lines.Add($"    {nodeId} -->|\"{FailLabel(node)}\"| F{i}[\"Failure\"]:::failure");
                        TryAddClass(declaredClasses, classDefs, "gatekeeper", "fill:#e3e9fa,color:#3f5c9a");
                        TryAddClass(declaredClasses, classDefs, "failure",    "fill:#f8e3e3,color:#b13e3e");
                        break;

                    case NodeKind.TransformWithRisk:
                        lines.Add($"    {nodeId}[\"{label}\"]:::transform");
                        if (hasNext) lines.Add($"    {nodeId} -->|ok| {nextId}");
                        lines.Add($"    {nodeId} -->|\"{FailLabel(node)}\"| F{i}[\"Failure\"]:::failure");
                        TryAddClass(declaredClasses, classDefs, "transform", "fill:#e3f0e8,color:#2f7a5c");
                        TryAddClass(declaredClasses, classDefs, "failure",   "fill:#f8e3e3,color:#b13e3e");
                        break;

                    case NodeKind.PureTransform:
                        lines.Add($"    {nodeId}[\"{label}\"]:::transform");
                        if (hasNext) lines.Add($"    {nodeId} --> {nextId}");
                        TryAddClass(declaredClasses, classDefs, "transform", "fill:#e3f0e8,color:#2f7a5c");
                        break;

                    case NodeKind.SideEffectSuccess:
                    case NodeKind.SideEffectFailure:
                    case NodeKind.SideEffectBoth:
                        lines.Add($"    {nodeId}[\"{label}\"]:::sideeffect");
                        if (hasNext) lines.Add($"    {nodeId} --> {nextId}");
                        TryAddClass(declaredClasses, classDefs, "sideeffect", "fill:#fff4d9,color:#b8882c");
                        break;

                    case NodeKind.Terminal:
                        lines.Add($"    {nodeId}[\"{label}\"]:::terminal");
                        // No outbound edges — terminal node ends the pipeline
                        TryAddClass(declaredClasses, classDefs, "terminal", "fill:#f2e3f5,color:#8a4f9e");
                        break;

                    default: // Unknown
                        lines.Add($"    {nodeId}[\"{label}\"]:::operation");
                        if (hasNext) lines.Add($"    {nodeId} --> {nextId}");
                        TryAddClass(declaredClasses, classDefs, "operation", "fill:#fef0e3,color:#b86a1c");
                        break;
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("flowchart LR");
            foreach (var line in lines)
                sb.AppendLine(line);
            foreach (var classDef in classDefs)
                sb.AppendLine(classDef);

            return sb.ToString().TrimEnd();
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
    }
}
