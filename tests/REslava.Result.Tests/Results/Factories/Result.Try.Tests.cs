namespace REslava.Result.Tests;

[TestClass]
public sealed class ResultTryTests
{
    #region Try (Sync) Tests

    [TestMethod]
    public void Try_SuccessfulOperation_ReturnsOkResult()
    {
        // Arrange
        var executed = false;

        // Act
        var result = Result.Try(() => executed = true);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(executed);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Try_ThrowsException_ReturnsFailedResultWithExceptionError()
    {
        // Act
        var result = Result.Try(() => throw new InvalidOperationException("Test exception"));

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
        Assert.AreEqual("Test exception", result.Errors[0].Message);
    }

    [TestMethod]
    public void Try_WithCustomErrorHandler_UsesCustomError()
    {
        // Arrange
        var customErrorMessage = "Custom error occurred";

        // Act
        var result = Result.Try(
            () => throw new InvalidOperationException("Original exception"),
            ex => new Error(customErrorMessage)
                .WithTags("ExceptionType", ex.GetType().Name)
        );

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(customErrorMessage, result.Errors[0].Message);
        Assert.IsTrue(result.Errors[0].Tags.ContainsKey("ExceptionType"));
        Assert.AreEqual("InvalidOperationException", result.Errors[0].Tags["ExceptionType"]);
    }

    [TestMethod]
    public void Try_NullOperation_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Result.Try(null!)
        );
    }

    [TestMethod]
    public void Try_ExceptionWithInnerException_CapturesInnerException()
    {
        // Arrange
        var innerException = new ArgumentException("Inner error");
        var outerException = new InvalidOperationException("Outer error", innerException);

        // Act
        var result = Result.Try(() => throw outerException);

        // Assert
        Assert.IsTrue(result.IsFailed);
        var exceptionError = result.Errors[0] as ExceptionError;
        Assert.IsNotNull(exceptionError);
        Assert.IsTrue(exceptionError!.Tags.ContainsKey("InnerException"));
        Assert.AreEqual("Inner error", exceptionError.Tags["InnerException"]);
    }

    #endregion

    #region TryAsync Tests

    [TestMethod]
    public async Task TryAsync_SuccessfulOperation_ReturnsOkResult()
    {
        // Arrange
        var executed = false;

        // Act
        var result = await Result.TryAsync(async () =>
        {
            await Task.Delay(10);
            executed = true;
        });

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(executed);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public async Task TryAsync_ThrowsException_ReturnsFailedResultWithExceptionError()
    {
        // Act
        var result = await Result.TryAsync(async () =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Async test exception");
        });

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.IsInstanceOfType<ExceptionError>(result.Errors[0]);
        Assert.AreEqual("Async test exception", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task TryAsync_WithCustomErrorHandler_UsesCustomError()
    {
        // Arrange
        var customErrorMessage = "Custom async error";

        // Act
        var result = await Result.TryAsync(
            async () =>
            {
                await Task.Delay(10);
                throw new InvalidOperationException("Original async exception");
            },
            ex => new Error(customErrorMessage)
                .WithTags("ExceptionType", ex.GetType().Name)
                .WithTags("IsAsync", true)
        );

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(customErrorMessage, result.Errors[0].Message);
        Assert.IsTrue(result.Errors[0].Tags.ContainsKey("IsAsync"));
        Assert.IsTrue((bool)result.Errors[0].Tags["IsAsync"]);
    }

    [TestMethod]
    public async Task TryAsync_NullOperation_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await Result.TryAsync(null!)
        );
    }

    [TestMethod]
    public async Task TryAsync_TaskCanceled_CapturesOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await Result.TryAsync(async () =>
        {
            await Task.Delay(1000, cts.Token);
        });

        // Assert
        Assert.IsTrue(result.IsFailed);
        var exceptionError = result.Errors[0] as ExceptionError;
        Assert.IsNotNull(exceptionError);
        Assert.IsInstanceOfType<TaskCanceledException>(exceptionError!.Exception);
    }

    #endregion

    #region Real-World Scenario Tests

    [TestMethod]
    public void Try_FileOperation_HandlesIOException()
    {
        // Arrange
        var invalidPath = "Z:\\nonexistent\\path\\file.txt";

        // Act
        var result = Result.Try(
            () => File.WriteAllText(invalidPath, "content"),
            ex => new Error("File operation failed")
                .WithTags("Path", invalidPath)
                .WithTags("Operation", "Write")
        );

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("File operation failed", result.Errors[0].Message);
    }

    [TestMethod]
    public async Task TryAsync_HttpRequest_HandlesNetworkError()
    {
        // Arrange
        var invalidUrl = "https://this-domain-definitely-does-not-exist-12345.com";

        // Act
        var result = await Result.TryAsync(
            async () =>
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(2);
                await client.GetAsync(invalidUrl);
            },
            ex => new Error("HTTP request failed")
                .WithTags("Url", invalidUrl)
                .WithTags("ErrorType", ex.GetType().Name)
        );

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual("HTTP request failed", result.Errors[0].Message);
    }

    [TestMethod]
    public void Try_ChainedWithMatch_WorksCorrectly()
    {
        // Arrange
        var matchExecuted = false;
        string? matchResult = null;

        // Act
        Result.Try(() => throw new Exception("Test"))
            .Match(
                onSuccess: () => matchResult = "success",
                onFailure: errors => { matchExecuted = true; matchResult = "failed"; }
            );

        // Assert
        Assert.IsTrue(matchExecuted);
        Assert.AreEqual("failed", matchResult);
    }

    #endregion
}
