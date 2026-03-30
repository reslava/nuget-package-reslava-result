using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using REslava.Result.Observers;

namespace REslava.Result.Diagnostics
{
    internal static class TraceSerializer
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
        };

        internal static async Task WriteAsync(HttpResponse response, IReadOnlyList<PipelineTrace> traces)
        {
            response.ContentType = "application/json; charset=utf-8";
            response.StatusCode = 200;
            await JsonSerializer.SerializeAsync(response.Body, traces, _options);
        }
    }
}
