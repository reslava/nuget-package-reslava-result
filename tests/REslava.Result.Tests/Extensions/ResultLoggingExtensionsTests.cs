using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Extensions;

namespace REslava.Result.Tests.Extensions;

[TestClass]
public sealed class ResultLoggingExtensionsTests
{
    private sealed class CapturingLogger : ILogger
    {
        public List<(LogLevel Level, string Message)> Logs { get; } = new();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
            => Logs.Add((logLevel, formatter(state, exception)));

        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }

    [TestMethod]
    public async Task WithLogger_TaskSuccessResult_LogsDebug()
    {
        var logger = new CapturingLogger();
        var task = Task.FromResult(new Result<int>(42, new Success("ok")));

        var result = await task.WithLogger(logger, "GetItem");

        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(1, logger.Logs);
        Assert.AreEqual(LogLevel.Debug, logger.Logs[0].Level);
    }

    [TestMethod]
    public async Task WithLogger_TaskFailureResult_LogsWarning()
    {
        var logger = new CapturingLogger();
        var task = Task.FromResult(
            new Result<int>(default, ImmutableList.Create<IReason>(new Error("db timeout"))));

        var result = await task.WithLogger(logger, "GetItem");

        Assert.IsTrue(result.IsFailure);
        Assert.HasCount(1, logger.Logs);
        Assert.AreEqual(LogLevel.Warning, logger.Logs[0].Level);
        StringAssert.Contains(logger.Logs[0].Message, "GetItem");
    }

    [TestMethod]
    public async Task WithLogger_CancellationAlreadyRequested_Throws()
    {
        var logger = new CapturingLogger();
        var task = Task.FromResult(new Result<int>(42, new Success("ok")));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var thrown = false;
        try { await task.WithLogger(logger, "GetItem", cts.Token); }
        catch (OperationCanceledException) { thrown = true; }

        Assert.IsTrue(thrown);
        Assert.HasCount(0, logger.Logs);
    }

    [TestMethod]
    public async Task LogOnFailure_TaskSuccessResult_NoLogEmitted()
    {
        var logger = new CapturingLogger();
        var task = Task.FromResult(new Result<int>(42, new Success("ok")));

        await task.LogOnFailure(logger, "GetItem");

        Assert.HasCount(0, logger.Logs);
    }

    [TestMethod]
    public async Task LogOnFailure_TaskFailureResult_LogsWarning()
    {
        var logger = new CapturingLogger();
        var task = Task.FromResult(
            new Result<int>(default, ImmutableList.Create<IReason>(new Error("not found"))));

        var result = await task.LogOnFailure(logger, "GetItem");

        Assert.IsTrue(result.IsFailure);
        Assert.HasCount(1, logger.Logs);
        Assert.AreEqual(LogLevel.Warning, logger.Logs[0].Level);
    }

    [TestMethod]
    public async Task LogOnFailure_CancellationAlreadyRequested_Throws()
    {
        var logger = new CapturingLogger();
        var task = Task.FromResult(new Result<int>(42, new Success("ok")));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var thrown = false;
        try { await task.LogOnFailure(logger, "GetItem", cts.Token); }
        catch (OperationCanceledException) { thrown = true; }

        Assert.IsTrue(thrown);
    }
}
