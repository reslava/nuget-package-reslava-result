using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace REslava.Result.Observers
{
    /// <summary>
    /// A <see cref="ResultFlowObserver"/> that stores the last <see cref="Capacity"/> pipeline
    /// executions in an in-memory ring buffer. Thread-safe. Zero persistence — cleared on restart.
    /// </summary>
    public sealed class RingBufferObserver : ResultFlowObserver
    {
        private readonly int _capacity;
        private readonly ConcurrentQueue<PipelineTrace> _traces = new ConcurrentQueue<PipelineTrace>();

        // Tracks the in-progress trace per async execution context
        private readonly System.Threading.AsyncLocal<InProgressTrace?> _current
            = new System.Threading.AsyncLocal<InProgressTrace?>();

        public int Capacity => _capacity;

        public RingBufferObserver(int capacity = 50)
        {
            _capacity = capacity < 1 ? 50 : capacity;
        }

        public override void OnPipelineStart(PipelineStartContext ctx)
        {
            _current.Value = new InProgressTrace
            {
                PipelineId = ctx.PipelineId,
                MethodName = ctx.MethodName,
                InputValue = ctx.InputValue,
                StartedAt = ctx.StartedAt,
                StartTimestamp = System.Diagnostics.Stopwatch.GetTimestamp()
            };
        }

        public override void OnNodeExit(NodeExitContext ctx)
        {
            var inProgress = _current.Value;
            if (inProgress == null) return;

            inProgress.Nodes.Add(new NodeTrace
            {
                NodeId = ctx.NodeId,
                StepName = ctx.StepName,
                IsSuccess = ctx.IsSuccess,
                OutputValue = ctx.OutputValue,
                ErrorType = ctx.ErrorType,
                ErrorMessage = ctx.ErrorMessage,
                ElapsedMs = ctx.ElapsedMs,
                NodeIndex = ctx.NodeIndex
            });
        }

        public override void OnPipelineEnd(PipelineEndContext ctx)
        {
            var inProgress = _current.Value;
            if (inProgress == null) return;

            var trace = new PipelineTrace
            {
                PipelineId = ctx.PipelineId,
                MethodName = ctx.MethodName,
                IsSuccess = ctx.IsSuccess,
                ErrorType = ctx.ErrorType,
                InputValue = inProgress.InputValue,
                OutputValue = ctx.OutputValue,
                ElapsedMs = ctx.ElapsedMs,
                StartedAt = inProgress.StartedAt,
                EndedAt = ctx.EndedAt,
                Nodes = inProgress.Nodes.AsReadOnly()
            };

            _traces.Enqueue(trace);
            while (_traces.Count > _capacity)
                _traces.TryDequeue(out _);

            _current.Value = null;
        }

        /// <summary>Returns a snapshot of captured traces, oldest first.</summary>
        public IReadOnlyList<PipelineTrace> GetTraces()
        {
            return new List<PipelineTrace>(_traces).AsReadOnly();
        }

        /// <summary>Clears all captured traces.</summary>
        public void Clear()
        {
            while (_traces.TryDequeue(out _)) { }
        }

        /// <summary>
        /// Serializes all captured traces to a JSON file so the REslava VSIX Debug panel
        /// can load them without a running HTTP server.
        /// Default path: <c>reslava-traces.json</c> next to the executing assembly
        /// (<see cref="System.AppContext.BaseDirectory"/>).
        /// Overwrites any existing file.
        /// </summary>
        /// <param name="path">
        /// Optional absolute or relative path. When <c>null</c>, writes to
        /// <c>{AppContext.BaseDirectory}/reslava-traces.json</c>.
        /// </param>
        public void Save(string? path = null)
        {
            string filePath;
            if (path == null)
            {
                filePath = Path.Combine(System.AppContext.BaseDirectory, "reslava-traces.json");
            }
            else if (path.IndexOf(Path.DirectorySeparatorChar) < 0 && path.IndexOf(Path.AltDirectorySeparatorChar) < 0)
            {
                // Short name (no directory separator) — auto-prefix reslava- so the VSIX picker finds it
                var name = path.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase) ? path : path + ".json";
                if (!name.StartsWith("reslava-", System.StringComparison.OrdinalIgnoreCase))
                    name = "reslava-" + name;
                filePath = Path.Combine(System.AppContext.BaseDirectory, name);
            }
            else
            {
                // Absolute or relative path — use as-is (power-user escape hatch)
                filePath = path;
            }
            var traces = GetTraces();
            var sb = new StringBuilder();
            sb.Append('[');
            for (int t = 0; t < traces.Count; t++)
            {
                if (t > 0) sb.Append(',');
                var tr = traces[t];
                sb.Append('{');
                AppendJsonString(sb, "pipelineId", tr.PipelineId); sb.Append(',');
                AppendJsonString(sb, "methodName", tr.MethodName); sb.Append(',');
                sb.Append($"\"isSuccess\":{(tr.IsSuccess ? "true" : "false")},");
                AppendJsonStringNullable(sb, "errorType", tr.ErrorType); sb.Append(',');
                AppendJsonStringNullable(sb, "inputValue", tr.InputValue); sb.Append(',');
                AppendJsonStringNullable(sb, "outputValue", tr.OutputValue); sb.Append(',');
                sb.Append($"\"elapsedMs\":{tr.ElapsedMs},");
                AppendJsonString(sb, "startedAt", tr.StartedAt.ToString("o")); sb.Append(',');
                AppendJsonString(sb, "endedAt", tr.EndedAt.ToString("o")); sb.Append(',');
                sb.Append("\"nodes\":[");
                for (int n = 0; n < tr.Nodes.Count; n++)
                {
                    if (n > 0) sb.Append(',');
                    var nd = tr.Nodes[n];
                    sb.Append('{');
                    AppendJsonString(sb, "nodeId", nd.NodeId); sb.Append(',');
                    AppendJsonString(sb, "stepName", nd.StepName); sb.Append(',');
                    sb.Append($"\"isSuccess\":{(nd.IsSuccess ? "true" : "false")},");
                    AppendJsonStringNullable(sb, "outputValue", nd.OutputValue); sb.Append(',');
                    AppendJsonStringNullable(sb, "errorType", nd.ErrorType); sb.Append(',');
                    AppendJsonStringNullable(sb, "errorMessage", nd.ErrorMessage); sb.Append(',');
                    sb.Append($"\"elapsedMs\":{nd.ElapsedMs},");
                    sb.Append($"\"nodeIndex\":{nd.NodeIndex}");
                    sb.Append('}');
                }
                sb.Append("]}");
            }
            sb.Append(']');
            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static void AppendJsonString(StringBuilder sb, string key, string value)
        {
            sb.Append('"').Append(key).Append("\":\"").Append(EscapeJson(value)).Append('"');
        }

        private static void AppendJsonStringNullable(StringBuilder sb, string key, string? value)
        {
            if (value == null)
                sb.Append('"').Append(key).Append("\":null");
            else
                AppendJsonString(sb, key, value);
        }

        private static string EscapeJson(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"")
             .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");

        private sealed class InProgressTrace
        {
            internal string PipelineId = string.Empty;
            internal string MethodName = string.Empty;
            internal string? InputValue;
            internal System.DateTimeOffset StartedAt;
            internal long StartTimestamp;
            internal List<NodeTrace> Nodes = new List<NodeTrace>();
        }
    }
}
