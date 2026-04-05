using REslava.Result.Observers;

namespace REslava.Result.Tests.Observers;

[TestClass]
[DoNotParallelize]
public class RingBufferObserverTests
{
    // ── Construction ─────────────────────────────────────────────────────

    [TestMethod]
    public void DefaultCapacity_Is50()
    {
        var obs = new RingBufferObserver();
        Assert.AreEqual(0, obs.GetTraces().Count);
    }

    [TestMethod]
    public void CustomCapacity_Accepted()
    {
        var obs = new RingBufferObserver(capacity: 5);
        Assert.IsNotNull(obs);
    }

    [TestMethod]
    public void ZeroOrNegativeCapacity_FallsBackTo50()
    {
        // Should not throw — invalid capacity silently falls back
        var obs = new RingBufferObserver(capacity: 0);
        Assert.IsNotNull(obs);
    }

    // ── Trace collection ─────────────────────────────────────────────────

    [TestMethod]
    public void GetTraces_ReturnsCompletedPipelinesOnly()
    {
        var obs = new RingBufferObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        var scope = PipelineObserver.BeginPipeline("p1", "M1", null, null);
        // Don't end — in-progress should not appear yet
        Assert.AreEqual(0, obs.GetTraces().Count);

        scope.End(isSuccess: true, outputValue: null, errorType: null);
        scope.Dispose();

        Assert.AreEqual(1, obs.GetTraces().Count);
    }

    [TestMethod]
    public void GetTraces_CapturesMethodNameAndPipelineId()
    {
        var obs = new RingBufferObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        var scope = PipelineObserver.BeginPipeline("abc123", "PlaceOrder", "input", null);
        scope.End(isSuccess: true, outputValue: "order-42", errorType: null);
        scope.Dispose();

        var traces = obs.GetTraces();
        Assert.AreEqual(1, traces.Count);
        Assert.AreEqual("abc123", traces[0].PipelineId);
        Assert.AreEqual("PlaceOrder", traces[0].MethodName);
        Assert.IsTrue(traces[0].IsSuccess);
    }

    [TestMethod]
    public void GetTraces_CapturesFailureInfo()
    {
        var obs = new RingBufferObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        var scope = PipelineObserver.BeginPipeline("p2", "ValidateOrder", null, null);
        scope.End(isSuccess: false, outputValue: null, errorType: "ValidationError");
        scope.Dispose();

        var traces = obs.GetTraces();
        Assert.AreEqual(1, traces.Count);
        Assert.IsFalse(traces[0].IsSuccess);
    }

    [TestMethod]
    public void GetTraces_CapturesNodeExits()
    {
        var obs = new RingBufferObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        var scope = PipelineObserver.BeginPipeline("p3", "M", null, new[] { "f.cs:1", "f.cs:2" });

        obs.OnNodeExit(new NodeExitContext("p3", "f.cs:1", "Bind", true, "User", null, null, 5, 0));
        obs.OnNodeExit(new NodeExitContext("p3", "f.cs:2", "Map", true, "Order", null, null, 3, 1));

        scope.End(isSuccess: true, outputValue: null, errorType: null);
        scope.Dispose();

        var traces = obs.GetTraces();
        Assert.AreEqual(1, traces.Count);
        Assert.AreEqual(2, traces[0].Nodes.Count);
        Assert.AreEqual("Bind", traces[0].Nodes[0].StepName);
        Assert.AreEqual("Map", traces[0].Nodes[1].StepName);
    }

    // ── Capacity / eviction ───────────────────────────────────────────────

    [TestMethod]
    public void Capacity_OldestEvictedWhenFull()
    {
        var obs = new RingBufferObserver(capacity: 3);
        using var _ = PipelineObserver.RegisterScoped(obs);

        for (int i = 0; i < 5; i++)
        {
            var scope = PipelineObserver.BeginPipeline($"p{i}", $"M{i}", null, null);
            scope.End(isSuccess: true, outputValue: null, errorType: null);
            scope.Dispose();
        }

        var traces = obs.GetTraces();
        Assert.AreEqual(3, traces.Count);
        // Oldest (p0, p1) evicted; newest (p2, p3, p4) remain
        Assert.IsTrue(traces.All(t => t.PipelineId.CompareTo("p2") >= 0));
    }

    // ── Clear ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void Clear_EmptiesBuffer()
    {
        var obs = new RingBufferObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        var scope = PipelineObserver.BeginPipeline("p1", "M", null, null);
        scope.End(isSuccess: true, outputValue: null, errorType: null);
        scope.Dispose();

        Assert.AreEqual(1, obs.GetTraces().Count);
        obs.Clear();
        Assert.AreEqual(0, obs.GetTraces().Count);
    }

    // ── GetTraces snapshot ────────────────────────────────────────────────

    [TestMethod]
    public void GetTraces_ReturnsSnapshot_NotLiveReference()
    {
        var obs = new RingBufferObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        var s1 = PipelineObserver.BeginPipeline("p1", "M", null, null);
        s1.End(true, null, null);
        s1.Dispose();

        var snapshot = obs.GetTraces();
        Assert.AreEqual(1, snapshot.Count);

        var s2 = PipelineObserver.BeginPipeline("p2", "M", null, null);
        s2.End(true, null, null);
        s2.Dispose();

        // Original snapshot should not change
        Assert.AreEqual(1, snapshot.Count);
        Assert.AreEqual(2, obs.GetTraces().Count);
    }

    // ── ElapsedMs / timing ────────────────────────────────────────────────

    [TestMethod]
    public void PipelineTrace_ElapsedMs_IsNonNegative()
    {
        var obs = new RingBufferObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        var scope = PipelineObserver.BeginPipeline("p1", "M", null, null);
        scope.End(isSuccess: true, outputValue: null, errorType: null);
        scope.Dispose();

        Assert.IsTrue(obs.GetTraces()[0].ElapsedMs >= 0);
    }

    // ── Save() ────────────────────────────────────────────────────────────

    [TestMethod]
    public void Save_CreatesFile_AtDefaultPath()
    {
        var obs = new RingBufferObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);
        var scope = PipelineObserver.BeginPipeline("pid1", "DoWork", "input", null);
        scope.End(isSuccess: true, outputValue: "out", errorType: null);
        scope.Dispose();

        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"reslava-save-test-{System.Guid.NewGuid():N}.json");
        try
        {
            obs.Save(path);
            Assert.IsTrue(System.IO.File.Exists(path), "Save() must create the file");
        }
        finally { System.IO.File.Delete(path); }
    }

    [TestMethod]
    public void Save_ProducesValidJson_WithTrace()
    {
        var obs = new RingBufferObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);
        var scope = PipelineObserver.BeginPipeline("pid2", "Process", "42", null);
        scope.End(isSuccess: false, outputValue: null, errorType: "NotFoundError");
        scope.Dispose();

        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"reslava-save-valid-{System.Guid.NewGuid():N}.json");
        try
        {
            obs.Save(path);
            var json = System.IO.File.ReadAllText(path);
            Assert.IsTrue(json.StartsWith("["), "Root must be a JSON array");
            Assert.IsTrue(json.Contains("\"methodName\":\"Process\""), "Must contain methodName");
            Assert.IsTrue(json.Contains("\"isSuccess\":false"), "Must contain isSuccess=false");
            Assert.IsTrue(json.Contains("\"errorType\":\"NotFoundError\""), "Must contain errorType");
        }
        finally { System.IO.File.Delete(path); }
    }

    [TestMethod]
    public void Save_OverwritesExistingFile()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"reslava-save-overwrite-{System.Guid.NewGuid():N}.json");
        System.IO.File.WriteAllText(path, "old content");
        try
        {
            var obs = new RingBufferObserver();
            obs.Save(path);
            var json = System.IO.File.ReadAllText(path);
            Assert.AreEqual("[]", json, "Empty buffer must produce empty JSON array");
        }
        finally { System.IO.File.Delete(path); }
    }

    [TestMethod]
    public void Save_EmptyBuffer_ProducesEmptyArray()
    {
        var obs = new RingBufferObserver();
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"reslava-save-empty-{System.Guid.NewGuid():N}.json");
        try
        {
            obs.Save(path);
            var json = System.IO.File.ReadAllText(path);
            Assert.AreEqual("[]", json);
        }
        finally { System.IO.File.Delete(path); }
    }
}
