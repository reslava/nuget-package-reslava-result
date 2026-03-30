using REslava.Result.Observers;

namespace REslava.Result.Tests.Observers;

[TestClass]
[DoNotParallelize]
public class PipelineObserverTests
{
    // ── Register / Unregister ─────────────────────────────────────────────

    [TestMethod]
    public void Register_SetsObserver()
    {
        var observer = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(observer);

        Assert.IsNotNull(ResultPipelineHooks.Observer);
    }

    [TestMethod]
    public void Unregister_ClearsObserver()
    {
        var observer = new RecordingObserver();
        PipelineObserver.Register(observer);
        PipelineObserver.Unregister();

        Assert.IsNull(ResultPipelineHooks.Observer);
    }

    [TestMethod]
    public void Register_ReplacesExistingObserver()
    {
        var first = new RecordingObserver();
        var second = new RecordingObserver();
        PipelineObserver.Register(first);
        PipelineObserver.Register(second);

        Assert.AreSame(second, ResultPipelineHooks.Observer);
        PipelineObserver.Unregister();
    }

    // ── RegisterScoped ───────────────────────────────────────────────────

    [TestMethod]
    public void RegisterScoped_RestoresPreviousObserverOnDispose()
    {
        PipelineObserver.Unregister(); // ensure clean state

        var inner = new RecordingObserver();
        using (PipelineObserver.RegisterScoped(inner))
        {
            Assert.AreSame(inner, ResultPipelineHooks.Observer);
        }

        Assert.IsNull(ResultPipelineHooks.Observer);
    }

    [TestMethod]
    public void RegisterScoped_Nested_RestoresOuterObserver()
    {
        var outer = new RecordingObserver();
        var inner = new RecordingObserver();

        using (PipelineObserver.RegisterScoped(outer))
        {
            using (PipelineObserver.RegisterScoped(inner))
            {
                Assert.AreSame(inner, ResultPipelineHooks.Observer);
            }
            Assert.AreSame(outer, ResultPipelineHooks.Observer);
        }

        Assert.IsNull(ResultPipelineHooks.Observer);
    }

    // ── BeginPipeline / PipelineScope ────────────────────────────────────

    [TestMethod]
    public void BeginPipeline_FiresOnPipelineStart()
    {
        var observer = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(observer);

        using var scope = PipelineObserver.BeginPipeline("pid1", "MyMethod", "input", null);

        Assert.AreEqual(1, observer.StartContexts.Count);
        Assert.AreEqual("pid1", observer.StartContexts[0].PipelineId);
        Assert.AreEqual("MyMethod", observer.StartContexts[0].MethodName);
        Assert.AreEqual("input", observer.StartContexts[0].InputValue);
    }

    [TestMethod]
    public void BeginPipeline_SetsPipelineState()
    {
        using var scope = PipelineObserver.BeginPipeline("pid2", "MyMethod", null,
            new[] { "file.cs:10", "file.cs:12" });

        var state = ResultPipelineHooks.State;
        Assert.IsNotNull(state);
        Assert.AreEqual("pid2", state!.PipelineId);
        Assert.AreEqual("MyMethod", state.MethodName);
        Assert.AreEqual(2, state.NodeIds?.Length);
    }

    [TestMethod]
    public void PipelineScope_Dispose_FiresOnPipelineEnd()
    {
        var observer = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(observer);

        var scope = PipelineObserver.BeginPipeline("pid3", "MyMethod", null, null);
        scope.End(isSuccess: true, outputValue: "42", errorType: null);
        scope.Dispose();

        Assert.AreEqual(1, observer.EndContexts.Count);
        Assert.AreEqual("pid3", observer.EndContexts[0].PipelineId);
        Assert.AreEqual("MyMethod", observer.EndContexts[0].MethodName);
        Assert.IsTrue(observer.EndContexts[0].IsSuccess);
        Assert.AreEqual("42", observer.EndContexts[0].OutputValue);
    }

    [TestMethod]
    public void PipelineScope_Dispose_RestoresPreviousState()
    {
        var outer = PipelineObserver.BeginPipeline("outer", "Outer", null, null);
        var outerState = ResultPipelineHooks.State;

        var inner = PipelineObserver.BeginPipeline("inner", "Inner", null, null);
        Assert.AreEqual("inner", ResultPipelineHooks.State!.PipelineId);

        inner.Dispose();
        Assert.AreSame(outerState, ResultPipelineHooks.State);

        outer.Dispose();
        Assert.IsNull(ResultPipelineHooks.State);
    }

    [TestMethod]
    public void PipelineScope_Dispose_WhenEndNotCalled_ReportsException()
    {
        var observer = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(observer);

        var scope = PipelineObserver.BeginPipeline("pid4", "MyMethod", null, null);
        // Deliberately skip scope.End() — simulates exception path
        scope.Dispose();

        Assert.AreEqual(1, observer.EndContexts.Count);
        Assert.IsFalse(observer.EndContexts[0].IsSuccess);
        Assert.AreEqual("Exception", observer.EndContexts[0].ErrorType);
    }

    // ── AsyncLocal isolation ─────────────────────────────────────────────

    [TestMethod]
    public async Task AsyncLocal_TwoConcurrentPipelines_DoNotShareState()
    {
        var barrier = new System.Threading.SemaphoreSlim(0, 2);

        string? task1Id = null;
        string? task2Id = null;

        var t1 = Task.Run(async () =>
        {
            using var scope = PipelineObserver.BeginPipeline("pipeline-A", "MethodA", null, null);
            await barrier.WaitAsync(); // wait for task2 to start
            task1Id = ResultPipelineHooks.State?.PipelineId;
        });

        var t2 = Task.Run(async () =>
        {
            using var scope = PipelineObserver.BeginPipeline("pipeline-B", "MethodB", null, null);
            barrier.Release(2); // release both
            await Task.Yield();
            task2Id = ResultPipelineHooks.State?.PipelineId;
        });

        await Task.WhenAll(t1, t2);

        Assert.AreEqual("pipeline-A", task1Id);
        Assert.AreEqual("pipeline-B", task2Id);
    }

    [TestMethod]
    public void AsyncLocal_NoLeakAfterScopeDispose()
    {
        using (PipelineObserver.BeginPipeline("leaking?", "Method", null, null)) { }

        // State in THIS async context should be restored to null
        Assert.IsNull(ResultPipelineHooks.State);
    }

    // ── NodeId / ConsumeIndex ────────────────────────────────────────────

    [TestMethod]
    public void PipelineState_ConsumeIndex_IncrementsEachCall()
    {
        using var scope = PipelineObserver.BeginPipeline("pid", "M", null,
            new[] { "file.cs:1", "file.cs:2", "file.cs:3" });

        var state = ResultPipelineHooks.State!;
        Assert.AreEqual(0, state.ConsumeIndex());
        Assert.AreEqual(1, state.ConsumeIndex());
        Assert.AreEqual(2, state.ConsumeIndex());
    }

    [TestMethod]
    public void PipelineState_CurrentNodeId_ReturnsNodeIdsEntry()
    {
        using var scope = PipelineObserver.BeginPipeline("pid", "M", null,
            new[] { "file.cs:10", "file.cs:20" });

        var state = ResultPipelineHooks.State!;
        Assert.AreEqual("file.cs:10", state.CurrentNodeId());
        state.ConsumeIndex();
        Assert.AreEqual("file.cs:20", state.CurrentNodeId());
    }

    [TestMethod]
    public void PipelineState_CurrentNodeId_FallsBackWhenBeyondNodeIds()
    {
        using var scope = PipelineObserver.BeginPipeline("pid99", "M", null,
            new[] { "file.cs:1" });

        var state = ResultPipelineHooks.State!;
        state.ConsumeIndex(); // consume the only entry
        var fallback = state.CurrentNodeId();

        StringAssert.StartsWith(fallback, "pid99:");
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private sealed class RecordingObserver : ResultFlowObserver
    {
        public List<PipelineStartContext> StartContexts { get; } = new();
        public List<PipelineEndContext> EndContexts { get; } = new();
        public List<NodeExitContext> NodeExits { get; } = new();

        public override void OnPipelineStart(PipelineStartContext ctx) => StartContexts.Add(ctx);
        public override void OnPipelineEnd(PipelineEndContext ctx) => EndContexts.Add(ctx);
        public override void OnNodeExit(NodeExitContext ctx) => NodeExits.Add(ctx);
    }
}
