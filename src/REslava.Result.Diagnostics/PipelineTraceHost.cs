using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using REslava.Result.Observers;

namespace REslava.Result.Diagnostics
{
    /// <summary>
    /// Hosts a minimal HTTP server that exposes <see cref="RingBufferObserver"/> trace data
    /// for consumption by the REslava.Result Extensions VSIX Live panel.
    /// </summary>
    /// <remarks>
    /// For ASP.NET Core apps use <c>app.MapResultFlowTraces(buffer)</c> instead.
    /// Use this class for console apps, worker services, or any host without a <c>WebApplication</c>.
    /// </remarks>
    public static class PipelineTraceHost
    {
        /// <summary>
        /// Starts a minimal Kestrel HTTP server on <c>http://localhost:{port}/reslava/traces</c>.
        /// Dispose the returned handle to stop the server.
        /// </summary>
        /// <param name="buffer">The <see cref="RingBufferObserver"/> to expose.</param>
        /// <param name="port">Port to listen on (default: 5297).</param>
        /// <returns>A disposable handle that stops the server when disposed.</returns>
        public static IDisposable Start(RingBufferObserver buffer, int port = 5297)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
            {
                // --urls sets the listen address before Kestrel initialises
                Args = [$"--urls=http://localhost:{port}"]
            });

            builder.Logging.ClearProviders(); // suppress Kestrel startup noise

            var app = builder.Build();

            app.MapGet("/reslava/traces", async ctx =>
            {
                ctx.Response.Headers["Access-Control-Allow-Origin"] = "*";
                await TraceSerializer.WriteAsync(ctx.Response, buffer.GetTraces());
            });

            app.StartAsync().GetAwaiter().GetResult();
            return new HostHandle(app);
        }

        private sealed class HostHandle : IDisposable
        {
            private readonly WebApplication _app;
            private bool _disposed;

            internal HostHandle(WebApplication app) => _app = app;

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _app.StopAsync().GetAwaiter().GetResult();
                _app.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
        }
    }
}
