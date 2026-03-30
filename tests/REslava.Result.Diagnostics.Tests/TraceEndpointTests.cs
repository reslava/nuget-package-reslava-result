using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using REslava.Result;
using REslava.Result.Diagnostics;
using REslava.Result.Observers;

namespace REslava.Result.Diagnostics.Tests;

[TestClass]
public class TraceEndpointTests
{
    // Each TFM gets its own port range so parallel multi-TFM runs don't collide.
#if NET10_0_OR_GREATER
    private const int PortBase = 19320;
#elif NET9_0_OR_GREATER
    private const int PortBase = 19300;
#else
    private const int PortBase = 19280;
#endif

    // ── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Seeds one completed trace into <paramref name="buffer"/> by firing the
    /// start → end hook pair in the same synchronous context.
    /// </summary>
    private static void AddTrace(RingBufferObserver buffer, string pipelineId = "abc123",
        string methodName = "TestMethod", bool isSuccess = true)
    {
        var now = DateTimeOffset.UtcNow;
        buffer.OnPipelineStart(new PipelineStartContext(
            PipelineId: pipelineId,
            MethodName: methodName,
            InputValue: "input",
            StartedAt: now));

        buffer.OnPipelineEnd(new PipelineEndContext(
            PipelineId: pipelineId,
            MethodName: methodName,
            IsSuccess: isSuccess,
            ErrorType: isSuccess ? null : "NotFoundError",
            OutputValue: isSuccess ? "result" : null,
            ElapsedMs: 15,
            EndedAt: now.AddMilliseconds(15)));
    }

    /// <summary>
    /// Creates, configures, and starts a minimal Kestrel app on <paramref name="port"/>.
    /// Returns an async-disposable handle. Use with <c>await using</c>.
    /// </summary>
    private static async Task<WebApplication> StartTestAppAsync(int port, Action<WebApplication> configure)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            Args = [$"--urls=http://localhost:{port}"]
        });
        builder.Logging.ClearProviders();
        var app = builder.Build();
        configure(app);
        await app.StartAsync();
        return app;
    }

    // ── MapResultFlowTraces(buffer) ─────────────────────────────────────────────

    [TestMethod]
    public async Task MapResultFlowTraces_WithBuffer_EmptyBuffer_Returns200EmptyArray()
    {
        int port = PortBase + 0;
        var buffer = new RingBufferObserver();
        await using var app = await StartTestAppAsync(port, a => a.MapResultFlowTraces(buffer));
        using var client = new HttpClient();

        var response = await client.GetAsync($"http://localhost:{port}/reslava/traces");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/json; charset=utf-8",
            response.Content.Headers.ContentType?.ToString());

        var json = await response.Content.ReadAsStringAsync();
        var array = JsonSerializer.Deserialize<JsonElement[]>(json);
        Assert.IsNotNull(array);
        Assert.AreEqual(0, array.Length);
    }

    [TestMethod]
    public async Task MapResultFlowTraces_WithBuffer_PopulatedBuffer_Returns200WithTraces()
    {
        int port = PortBase + 1;
        var buffer = new RingBufferObserver();
        AddTrace(buffer, pipelineId: "pipe1", methodName: "PlaceOrder");
        await using var app = await StartTestAppAsync(port, a => a.MapResultFlowTraces(buffer));
        using var client = new HttpClient();

        var response = await client.GetAsync($"http://localhost:{port}/reslava/traces");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.AreEqual(JsonValueKind.Array, root.ValueKind);
        Assert.AreEqual(1, root.GetArrayLength());

        var trace = root[0];
        Assert.AreEqual("pipe1", trace.GetProperty("pipelineId").GetString());
        Assert.AreEqual("PlaceOrder", trace.GetProperty("methodName").GetString());
        Assert.IsTrue(trace.GetProperty("isSuccess").GetBoolean());
    }

    [TestMethod]
    public async Task MapResultFlowTraces_WithBuffer_MultipleTraces_AllReturned()
    {
        int port = PortBase + 2;
        var buffer = new RingBufferObserver(capacity: 10);
        AddTrace(buffer, "p1", "Method1");
        AddTrace(buffer, "p2", "Method2");
        AddTrace(buffer, "p3", "Method3");
        await using var app = await StartTestAppAsync(port, a => a.MapResultFlowTraces(buffer));
        using var client = new HttpClient();

        var response = await client.GetAsync($"http://localhost:{port}/reslava/traces");
        var json = await response.Content.ReadAsStringAsync();
        var array = JsonSerializer.Deserialize<JsonElement[]>(json);

        Assert.IsNotNull(array);
        Assert.AreEqual(3, array.Length);
    }

    [TestMethod]
    public async Task MapResultFlowTraces_WithBuffer_SetsAccessControlAllowOriginHeader()
    {
        int port = PortBase + 3;
        var buffer = new RingBufferObserver();
        await using var app = await StartTestAppAsync(port, a => a.MapResultFlowTraces(buffer));
        using var client = new HttpClient();

        var response = await client.GetAsync($"http://localhost:{port}/reslava/traces");

        Assert.IsTrue(response.Headers.Contains("Access-Control-Allow-Origin"),
            "Response must include CORS header");
        Assert.AreEqual("*", response.Headers.GetValues("Access-Control-Allow-Origin").First());
    }

    // ── MapResultFlowTraces() — no-arg overload ─────────────────────────────────

    [TestMethod]
    public async Task MapResultFlowTraces_NoArgument_NoRingBufferRegistered_Returns503()
    {
        int port = PortBase + 4;
        using var scope = PipelineObserver.RegisterScoped(null!);   // ensure no observer

        await using var app = await StartTestAppAsync(port, a => a.MapResultFlowTraces());
        using var client = new HttpClient();

        var response = await client.GetAsync($"http://localhost:{port}/reslava/traces");

        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [TestMethod]
    public async Task MapResultFlowTraces_NoArgument_RingBufferRegistered_Returns200()
    {
        int port = PortBase + 5;
        var buffer = new RingBufferObserver();
        using var scope = PipelineObserver.RegisterScoped(buffer);
        AddTrace(buffer, "px", "SomeMethod");

        await using var app = await StartTestAppAsync(port, a => a.MapResultFlowTraces());
        using var client = new HttpClient();

        var response = await client.GetAsync($"http://localhost:{port}/reslava/traces");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.AreEqual(1, doc.RootElement.GetArrayLength());
    }

    // ── PipelineTraceHost.Start() ───────────────────────────────────────────────

    [TestMethod]
    public async Task PipelineTraceHost_Start_EmptyBuffer_Returns200EmptyArray()
    {
        int port = PortBase + 7;
        var buffer = new RingBufferObserver();
        using var host = PipelineTraceHost.Start(buffer, port: port);
        using var client = new HttpClient();

        var response = await client.GetAsync($"http://localhost:{port}/reslava/traces");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var array = JsonSerializer.Deserialize<JsonElement[]>(json);
        Assert.IsNotNull(array);
        Assert.AreEqual(0, array.Length);
    }

    [TestMethod]
    public async Task PipelineTraceHost_Start_PopulatedBuffer_ReturnTraces()
    {
        int port = PortBase + 8;
        var buffer = new RingBufferObserver();
        AddTrace(buffer, "hosted1", "HostedMethod");
        using var host = PipelineTraceHost.Start(buffer, port: port);
        using var client = new HttpClient();

        var response = await client.GetAsync($"http://localhost:{port}/reslava/traces");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.AreEqual(1, doc.RootElement.GetArrayLength());
        Assert.AreEqual("HostedMethod",
            doc.RootElement[0].GetProperty("methodName").GetString());
    }

    [TestMethod]
    public async Task PipelineTraceHost_Start_SetsAccessControlAllowOriginHeader()
    {
        int port = PortBase + 9;
        var buffer = new RingBufferObserver();
        using var host = PipelineTraceHost.Start(buffer, port: port);
        using var client = new HttpClient();

        var response = await client.GetAsync($"http://localhost:{port}/reslava/traces");

        Assert.IsTrue(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.AreEqual("*", response.Headers.GetValues("Access-Control-Allow-Origin").First());
    }

    [TestMethod]
    public void PipelineTraceHost_Start_NullBuffer_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            PipelineTraceHost.Start(null!));
    }
}
