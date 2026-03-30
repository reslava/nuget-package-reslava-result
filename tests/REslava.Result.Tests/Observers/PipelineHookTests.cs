using REslava.Result.Extensions;
using REslava.Result.Observers;
using REslava.Result.Reasons;

namespace REslava.Result.Tests.Observers;

[TestClass]
[DoNotParallelize]
public class PipelineHookTests
{
    // ── Bind ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public void Bind_Success_FiresOnNodeExitWithCorrectValues()
    {
        var obs = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        Result<int>.Ok(42).Bind(x => Result<string>.Ok(x.ToString()));

        Assert.AreEqual(1, obs.NodeExits.Count);
        var exit = obs.NodeExits[0];
        Assert.AreEqual("Bind", exit.StepName);
        Assert.IsTrue(exit.IsSuccess);
        Assert.AreEqual("42", exit.OutputValue);
        Assert.IsNull(exit.ErrorType);
        Assert.IsTrue(exit.ElapsedMs >= 0);
    }

    [TestMethod]
    public void Bind_ReturnsFailure_FiresOnNodeExitWithErrorType()
    {
        var obs = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        Result<int>.Ok(1).Bind(_ => Result<string>.Fail("oops"));

        Assert.AreEqual(1, obs.NodeExits.Count);
        var exit = obs.NodeExits[0];
        Assert.AreEqual("Bind", exit.StepName);
        Assert.IsFalse(exit.IsSuccess);
        Assert.IsNotNull(exit.ErrorType);
        Assert.IsNull(exit.OutputValue);
    }

    [TestMethod]
    public void Bind_AlreadyFailed_DoesNotFireHook()
    {
        var obs = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        Result<int>.Fail("already failed").Bind(x => Result<string>.Ok(x.ToString()));

        Assert.AreEqual(0, obs.NodeExits.Count);
    }

    [TestMethod]
    public void Bind_NoObserver_DoesNotThrow()
    {
        PipelineObserver.Unregister();

        // No exception should be thrown
        var result = Result<int>.Ok(5).Bind(x => Result<string>.Ok(x.ToString()));
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public void Bind_Sequential_EachFiresOnce()
    {
        var obs = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        Result<int>.Ok(1)
            .Bind(x => Result<int>.Ok(x + 1))
            .Bind(x => Result<int>.Ok(x + 1))
            .Bind(x => Result<int>.Ok(x + 1));

        Assert.AreEqual(3, obs.NodeExits.Count);
        Assert.IsTrue(obs.NodeExits.All(e => e.StepName == "Bind" && e.IsSuccess));
    }

    [TestMethod]
    public void Bind_ShortCircuit_DownstreamHooksSkipped()
    {
        var obs = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        Result<int>.Ok(1)
            .Bind(_ => Result<int>.Fail("fail here"))  // fires, returns failure
            .Bind(x => Result<int>.Ok(x + 1))           // skipped — short-circuit
            .Bind(x => Result<int>.Ok(x + 1));           // skipped — short-circuit

        Assert.AreEqual(1, obs.NodeExits.Count);
        Assert.IsFalse(obs.NodeExits[0].IsSuccess);
    }

    // ── Map ──────────────────────────────────────────────────────────────────

    [TestMethod]
    public void Map_Success_FiresOnNodeExitWithOutputValue()
    {
        var obs = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        Result<int>.Ok(7).Map(x => x * 2);

        Assert.AreEqual(1, obs.NodeExits.Count);
        var exit = obs.NodeExits[0];
        Assert.AreEqual("Map", exit.StepName);
        Assert.IsTrue(exit.IsSuccess);
        Assert.AreEqual("14", exit.OutputValue);
    }

    [TestMethod]
    public void Map_AlreadyFailed_DoesNotFireHook()
    {
        var obs = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        Result<int>.Fail("bad").Map(x => x * 2);

        Assert.AreEqual(0, obs.NodeExits.Count);
    }

    [TestMethod]
    public void Map_MapperThrows_FiresHookWithErrorType()
    {
        var obs = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        try
        {
            Result<int>.Ok(1).Map<string>(_ => throw new InvalidOperationException("boom"));
        }
        catch { /* swallow — we're testing the hook, not the exception */ }

        Assert.AreEqual(1, obs.NodeExits.Count);
        var exit = obs.NodeExits[0];
        Assert.AreEqual("Map", exit.StepName);
        Assert.IsFalse(exit.IsSuccess);
        Assert.AreEqual("ExceptionError", exit.ErrorType);
    }

    // ── Ensure ───────────────────────────────────────────────────────────────

    [TestMethod]
    public void Ensure_PredicateTrue_FiresHookSuccess()
    {
        var obs = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        Result<int>.Ok(10).Ensure(x => x > 5, "must be > 5");

        Assert.AreEqual(1, obs.NodeExits.Count);
        var exit = obs.NodeExits[0];
        Assert.AreEqual("Ensure", exit.StepName);
        Assert.IsTrue(exit.IsSuccess);
    }

    [TestMethod]
    public void Ensure_PredicateFalse_FiresHookFailure()
    {
        var obs = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        Result<int>.Ok(3).Ensure(x => x > 5, "must be > 5");

        Assert.AreEqual(1, obs.NodeExits.Count);
        var exit = obs.NodeExits[0];
        Assert.AreEqual("Ensure", exit.StepName);
        Assert.IsFalse(exit.IsSuccess);
        Assert.IsNotNull(exit.ErrorType);
        Assert.IsNotNull(exit.ErrorMessage);
    }

    [TestMethod]
    public void Ensure_AlreadyFailed_DoesNotFireHook()
    {
        var obs = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        Result<int>.Fail("nope").Ensure(x => x > 5, "must be > 5");

        Assert.AreEqual(0, obs.NodeExits.Count);
    }

    // ── Tap ──────────────────────────────────────────────────────────────────

    [TestMethod]
    public void Tap_SuccessResult_FiresHook()
    {
        var obs = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        Result<int>.Ok(99).Tap(x => { var __ = x; });

        Assert.AreEqual(1, obs.NodeExits.Count);
        var exit = obs.NodeExits[0];
        Assert.AreEqual("Tap", exit.StepName);
        Assert.IsTrue(exit.IsSuccess);
    }

    [TestMethod]
    public void Tap_AlreadyFailed_FiresHookWithFailureInfo()
    {
        var obs = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        Result<int>.Fail("err").Tap(x => { var __ = x; });

        Assert.AreEqual(1, obs.NodeExits.Count);
        Assert.IsFalse(obs.NodeExits[0].IsSuccess);
    }

    // ── NodeId / auto-tier ───────────────────────────────────────────────────

    [TestMethod]
    public void Hook_AutoTier_NodeIdContainsFileAndLine()
    {
        var obs = new RecordingObserver();
        using var _ = PipelineObserver.RegisterScoped(obs);

        // No PipelineState set — auto-tier uses [CallerFilePath]:[CallerLineNumber]
        Result<int>.Ok(1).Bind(x => Result<string>.Ok("ok"));

        Assert.AreEqual(1, obs.NodeExits.Count);
        var nodeId = obs.NodeExits[0].NodeId;
        // NodeId should be "FileName.cs:NNN" format (auto-tier: [CallerFilePath]:[CallerLineNumber])
        Assert.IsFalse(string.IsNullOrEmpty(nodeId));
        Assert.IsTrue(nodeId.Contains(':'), $"NodeId '{nodeId}' should be 'file:line' format");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    [TestCleanup]
    public void Cleanup() => PipelineObserver.Unregister();

    private sealed class RecordingObserver : ResultFlowObserver
    {
        public List<NodeExitContext> NodeExits { get; } = new();
        public override void OnNodeExit(NodeExitContext ctx) => NodeExits.Add(ctx);
    }
}
