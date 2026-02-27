using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;

namespace REslava.Result.Tests.Results;

[TestClass]
public sealed class ResultLoggingTests
{
    // Minimal logger that captures log calls for assertions
    private sealed class CapturingLogger : ILogger
    {
        public List<(LogLevel Level, string Message)> Logs { get; } = new();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
            => Logs.Add((logLevel, formatter(state, exception)));

        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }

    #region WithLogger — success

    [TestMethod]
    public void WithLogger_SuccessResult_LogsDebug()
    {
        var logger = new CapturingLogger();
        var result = new Result<int>(42, new Success("ok"));

        result.WithLogger(logger, "GetItem");

        Assert.HasCount(1, logger.Logs);
        Assert.AreEqual(LogLevel.Debug, logger.Logs[0].Level);
        StringAssert.Contains(logger.Logs[0].Message, "GetItem");
        StringAssert.Contains(logger.Logs[0].Message, "succeeded");
    }

    [TestMethod]
    public void WithLogger_SuccessResult_ReturnsOriginalResult()
    {
        var logger = new CapturingLogger();
        var result = new Result<int>(42, new Success("ok"));

        var returned = result.WithLogger(logger, "GetItem");

        Assert.IsTrue(ReferenceEquals(result, returned));
    }

    #endregion

    #region WithLogger — failure

    [TestMethod]
    public void WithLogger_FailureWithPlainError_LogsWarning()
    {
        var logger = new CapturingLogger();
        var result = new Result<int>(default, ImmutableList.Create<IReason>(new Error("not found")));

        result.WithLogger(logger, "GetItem");

        Assert.HasCount(1, logger.Logs);
        Assert.AreEqual(LogLevel.Warning, logger.Logs[0].Level);
        StringAssert.Contains(logger.Logs[0].Message, "GetItem");
        StringAssert.Contains(logger.Logs[0].Message, "failed");
        StringAssert.Contains(logger.Logs[0].Message, "not found");
    }

    [TestMethod]
    public void WithLogger_FailureWithExceptionError_LogsError()
    {
        var logger = new CapturingLogger();
        var result = new Result<int>(default, ImmutableList.Create<IReason>(
            new ExceptionError(new InvalidOperationException("boom"))));

        result.WithLogger(logger, "GetItem");

        Assert.HasCount(1, logger.Logs);
        Assert.AreEqual(LogLevel.Error, logger.Logs[0].Level);
    }

    [TestMethod]
    public void WithLogger_FailureWithMultipleErrors_LogsErrorCount()
    {
        var logger = new CapturingLogger();
        var result = new Result<int>(default, ImmutableList.Create<IReason>(
            new Error("first"), new Error("second"), new Error("third")));

        result.WithLogger(logger, "GetItem");

        Assert.HasCount(1, logger.Logs);
        StringAssert.Contains(logger.Logs[0].Message, "3");
    }

    [TestMethod]
    public void WithLogger_FailureWithSingleError_NoCountInMessage()
    {
        var logger = new CapturingLogger();
        var result = new Result<int>(default, ImmutableList.Create<IReason>(new NotFoundError("Item", 1)));

        result.WithLogger(logger, "GetItem");

        Assert.HasCount(1, logger.Logs);
        StringAssert.Contains(logger.Logs[0].Message, "NotFoundError");
    }

    [TestMethod]
    public void WithLogger_FailureResult_ReturnsOriginalResult()
    {
        var logger = new CapturingLogger();
        var result = new Result<int>(default, ImmutableList.Create<IReason>(new Error("fail")));

        var returned = result.WithLogger(logger, "GetItem");

        Assert.IsTrue(ReferenceEquals(result, returned));
    }

    [TestMethod]
    public void WithLogger_OperationNameAppearsInMessage()
    {
        var logger = new CapturingLogger();
        var result = new Result<int>(42, new Success("ok"));

        result.WithLogger(logger, "MyCustomOperation");

        StringAssert.Contains(logger.Logs[0].Message, "MyCustomOperation");
    }

    #endregion

    #region LogOnFailure

    [TestMethod]
    public void LogOnFailure_SuccessResult_NoLogEmitted()
    {
        var logger = new CapturingLogger();
        var result = new Result<int>(42, new Success("ok"));

        result.LogOnFailure(logger, "GetItem");

        Assert.HasCount(0, logger.Logs);
    }

    [TestMethod]
    public void LogOnFailure_FailureWithPlainError_LogsWarning()
    {
        var logger = new CapturingLogger();
        var result = new Result<int>(default, ImmutableList.Create<IReason>(new Error("db error")));

        result.LogOnFailure(logger, "SaveItem");

        Assert.HasCount(1, logger.Logs);
        Assert.AreEqual(LogLevel.Warning, logger.Logs[0].Level);
        StringAssert.Contains(logger.Logs[0].Message, "SaveItem");
    }

    [TestMethod]
    public void LogOnFailure_FailureWithExceptionError_LogsError()
    {
        var logger = new CapturingLogger();
        var result = new Result<int>(default, ImmutableList.Create<IReason>(
            new ExceptionError(new TimeoutException("timed out"))));

        result.LogOnFailure(logger, "SaveItem");

        Assert.HasCount(1, logger.Logs);
        Assert.AreEqual(LogLevel.Error, logger.Logs[0].Level);
    }

    [TestMethod]
    public void LogOnFailure_ReturnsOriginalResult()
    {
        var logger = new CapturingLogger();
        var result = new Result<int>(default, ImmutableList.Create<IReason>(new Error("fail")));

        var returned = result.LogOnFailure(logger, "SaveItem");

        Assert.IsTrue(ReferenceEquals(result, returned));
    }

    #endregion
}
