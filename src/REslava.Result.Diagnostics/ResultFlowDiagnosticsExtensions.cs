using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using REslava.Result.Observers;

namespace REslava.Result.Diagnostics
{
    /// <summary>
    /// Extension methods for exposing pipeline trace data via an HTTP endpoint.
    /// </summary>
    public static class ResultFlowDiagnosticsExtensions
    {
        /// <summary>
        /// Maps <c>GET /reslava/traces</c> to serve the contents of <paramref name="buffer"/>
        /// as a JSON array of <see cref="PipelineTrace"/> objects.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <param name="buffer">The <see cref="RingBufferObserver"/> whose traces to expose.</param>
        /// <returns>The <see cref="IEndpointRouteBuilder"/> for chaining.</returns>
        public static IEndpointRouteBuilder MapResultFlowTraces(
            this IEndpointRouteBuilder endpoints, RingBufferObserver buffer)
        {
            endpoints.MapGet("/reslava/traces", async ctx =>
            {
                ctx.Response.Headers["Access-Control-Allow-Origin"] = "*";
                await TraceSerializer.WriteAsync(ctx.Response, buffer.GetTraces());
            });

            return endpoints;
        }

        /// <summary>
        /// Maps <c>GET /reslava/traces</c> using the currently registered
        /// <see cref="PipelineObserver"/> observer if it is a <see cref="RingBufferObserver"/>.
        /// Returns HTTP 503 if no <see cref="RingBufferObserver"/> is registered.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
        /// <returns>The <see cref="IEndpointRouteBuilder"/> for chaining.</returns>
        public static IEndpointRouteBuilder MapResultFlowTraces(
            this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/reslava/traces", async ctx =>
            {
                ctx.Response.Headers["Access-Control-Allow-Origin"] = "*";

                if (PipelineObserver.Current is RingBufferObserver buffer)
                {
                    await TraceSerializer.WriteAsync(ctx.Response, buffer.GetTraces());
                }
                else
                {
                    ctx.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    ctx.Response.ContentType = "application/json; charset=utf-8";
                    await ctx.Response.WriteAsync(
                        "{\"error\":\"No RingBufferObserver registered. Call PipelineObserver.Register(new RingBufferObserver()) first.\"}");
                }
            });

            return endpoints;
        }
    }
}
