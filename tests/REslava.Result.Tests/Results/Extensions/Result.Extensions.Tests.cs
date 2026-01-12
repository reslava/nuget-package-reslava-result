namespace REslava.Result.Tests;

/// <summary>
/// Comprehensive tests for Result extension methods (Tap and TapAsync)
/// </summary>
[TestClass]
public sealed class ResultExtensionsCompleteTests
{
    #region Tap - Non-Generic Result - Sync

    [TestMethod]
    public void Tap_Result_OnSuccess_ExecutesAction()
    {
        // Arrange
        var result = Result.Ok();
        var executed = false;

        // Act
        result.Tap(() => executed = true);

        // Assert
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public void Tap_Result_OnFailure_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result.Fail("Error");
        var executed = false;

        // Act
        result.Tap(() => executed = true);

        // Assert
        Assert.IsFalse(executed);
    }

    [TestMethod]
    public void Tap_Result_WithSideEffects_ModifiesExternalState()
    {
        // Arrange
        var result = Result.Ok();
        var log = new List<string>();

        // Act
        result.Tap(() => log.Add("Action executed"));

        // Assert
        Assert.HasCount(1, log);
        Assert.AreEqual("Action executed", log[0]);
    }

    #endregion

    #region TapAsync - Non-Generic Result - Async Action

    [TestMethod]
    public async Task TapAsync_Result_AsyncAction_OnSuccess_ExecutesAction()
    {
        // Arrange
        var result = Result.Ok();
        var executed = false;

        // Act
        await result.TapAsync(async () =>
        {
            await Task.Delay(10);
            executed = true;
        });

        // Assert
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public async Task TapAsync_Result_AsyncAction_OnFailure_DoesNotExecute()
    {
        // Arrange
        var result = Result.Fail("Error");
        var executed = false;

        // Act
        await result.TapAsync(async () =>
        {
            await Task.Delay(10);
            executed = true;
        });

        // Assert
        Assert.IsFalse(executed);
    }

    [TestMethod]
    public async Task TapAsync_Result_AsyncAction_WithSideEffects_Works()
    {
        // Arrange
        var result = Result.Ok();
        var log = new List<string>();

        // Act
        await result.TapAsync(async () =>
        {
            await Task.Delay(10);
            log.Add("Async action executed");
        });

        // Assert
        Assert.HasCount(1, log);
        Assert.AreEqual("Async action executed", log[0]);
    }

    #endregion

    #region TapAsync - Task<Result> - Sync Action

    [TestMethod]
    public async Task TapAsync_TaskResult_SyncAction_OnSuccess_ExecutesAction()
    {
        // Arrange
        var executed = false;

        // Act
        await Task.FromResult(Result.Ok())
            .TapAsync(() => executed = true);

        // Assert
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public async Task TapAsync_TaskResult_SyncAction_OnFailure_DoesNotExecute()
    {
        // Arrange
        var executed = false;

        // Act
        await Task.FromResult(Result.Fail("Error"))
            .TapAsync(() => executed = true);

        // Assert
        Assert.IsFalse(executed);
    }

    // TOFIX
    // [TestMethod]
    // public async Task TapAsync_TaskResult_SyncAction_WithChaining_Works()
    // {
    //     // Arrange
    //     var executionOrder = new List<int>();

    //     // Act
    //     await GetResultAsync()
    //         .TapAsync(() => executionOrder.Add(1))
    //         .TapAsync(() => executionOrder.Add(2));

    //     // Assert
    //     Assert.HasCount(2, executionOrder);
    //     Assert.AreEqual(1, executionOrder[0]);
    //     Assert.AreEqual(2, executionOrder[1]);
    // }

    #endregion

    #region TapAsync - Task<Result> - Async Action

    [TestMethod]
    public async Task TapAsync_TaskResult_AsyncAction_OnSuccess_ExecutesAction()
    {
        // Arrange
        var executed = false;

        // Act
        await Task.FromResult(Result.Ok())
            .TapAsync(async () =>
            {
                await Task.Delay(10);
                executed = true;
            });

        // Assert
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public async Task TapAsync_TaskResult_AsyncAction_OnFailure_DoesNotExecute()
    {
        // Arrange
        var executed = false;

        // Act
        await Task.FromResult(Result.Fail("Error"))
            .TapAsync(async () =>
            {
                await Task.Delay(10);
                executed = true;
            });

        // Assert
        Assert.IsFalse(executed);
    }

    // TOFIX
    // [TestMethod]
    // public async Task TapAsync_TaskResult_AsyncAction_WithChaining_Works()
    // {
    //     // Arrange
    //     var executionOrder = new List<int>();

    //     // Act
    //     await GetResultAsync()
    //         .TapAsync(async () =>
    //         {
    //             await Task.Delay(10);
    //             executionOrder.Add(1);
    //         })
    //         .TapAsync(async () =>
    //         {
    //             await Task.Delay(10);
    //             executionOrder.Add(2);
    //         });

    //     // Assert
    //     Assert.HasCount(2, executionOrder);
    //     Assert.AreEqual(1, executionOrder[0]);
    //     Assert.AreEqual(2, executionOrder[1]);
    // }

    #endregion

    #region Tap - Generic Result<T> - Sync

    [TestMethod]
    public void Tap_Generic_OnSuccess_ExecutesAction()
    {
        // Arrange
        var result = Result<int>.Ok(42);
        var capturedValue = 0;

        // Act
        var returned = result.Tap(value => capturedValue = value);

        // Assert
        Assert.AreEqual(42, capturedValue);
        Assert.AreSame(result, returned);
    }

    [TestMethod]
    public void Tap_Generic_OnSuccess_ReturnsSameInstance()
    {
        // Arrange
        var result = Result<string>.Ok("test");

        // Act
        var returned = result.Tap(value => { });

        // Assert
        Assert.AreSame(result, returned);
        Assert.IsTrue(returned.IsSuccess);
        Assert.AreEqual("test", returned.Value);
    }

    [TestMethod]
    public void Tap_Generic_OnFailure_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result<int>.Fail("Error occurred");
        var actionExecuted = false;

        // Act
        var returned = result.Tap(value => actionExecuted = true);

        // Assert
        Assert.IsFalse(actionExecuted);
        Assert.AreSame(result, returned);
    }

    [TestMethod]
    public void Tap_Generic_OnFailure_PreservesErrors()
    {
        // Arrange
        var result = Result<string>.Fail("Original error");

        // Act
        var returned = result.Tap(value => { });

        // Assert
        Assert.IsTrue(returned.IsFailed);
        Assert.HasCount(1, returned.Errors);
        Assert.AreEqual("Original error", returned.Errors[0].Message);
    }

    [TestMethod]
    public void Tap_Generic_MultipleCallsExecuteInOrder()
    {
        // Arrange
        var result = Result<int>.Ok(5);
        var executionOrder = new List<int>();

        // Act
        result
            .Tap(v => executionOrder.Add(1))
            .Tap(v => executionOrder.Add(2))
            .Tap(v => executionOrder.Add(3));

        // Assert
        Assert.AreEqual(3, executionOrder.Count);
        Assert.AreEqual(1, executionOrder[0]);
        Assert.AreEqual(2, executionOrder[1]);
        Assert.AreEqual(3, executionOrder[2]);
    }

    [TestMethod]
    public void Tap_Generic_WithComplexType_Works()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };
        var result = Result<Person>.Ok(person);
        var loggedName = "";

        // Act
        result.Tap(p => loggedName = p.Name);

        // Assert
        Assert.AreEqual("John", loggedName);
    }

    #endregion

    #region TapAsync - Generic Result<T> - Async Action

    [TestMethod]
    public async Task TapAsync_Generic_AsyncAction_OnSuccess_ExecutesAction()
    {
        // Arrange
        var result = Result<int>.Ok(42);
        var capturedValue = 0;

        // Act
        var returned = await result.TapAsync(async value =>
        {
            await Task.Delay(10);
            capturedValue = value;
        });

        // Assert
        Assert.AreEqual(42, capturedValue);
        Assert.AreSame(result, returned);
    }

    [TestMethod]
    public async Task TapAsync_Generic_AsyncAction_OnFailure_DoesNotExecute()
    {
        // Arrange
        var result = Result<int>.Fail("Error");
        var executed = false;

        // Act
        var returned = await result.TapAsync(async value =>
        {
            await Task.Delay(10);
            executed = true;
        });

        // Assert
        Assert.IsFalse(executed);
        Assert.AreSame(result, returned);
    }

    [TestMethod]
    public async Task TapAsync_Generic_AsyncAction_WithChaining_Works()
    {
        // Arrange
        var result = Result<int>.Ok(10);
        var log = new List<string>();

        // Act
        await result
            .TapAsync(async v =>
            {
                await Task.Delay(10);
                log.Add($"Step 1: {v}");
            })
            .TapAsync(async v =>
            {
                await Task.Delay(10);
                log.Add($"Step 2: {v}");
            });

        // Assert
        Assert.HasCount(2, log);
        Assert.AreEqual("Step 1: 10", log[0]);
        Assert.AreEqual("Step 2: 10", log[1]);
    }

    #endregion

    #region TapAsync - Task<Result<T>> - Sync Action

    [TestMethod]
    public async Task TapAsync_TaskGeneric_SyncAction_OnSuccess_ExecutesAction()
    {
        // Arrange
        var capturedValue = 0;

        // Act
        await Task.FromResult(Result<int>.Ok(42))
            .TapAsync(value => capturedValue = value);

        // Assert
        Assert.AreEqual(42, capturedValue);
    }

    [TestMethod]
    public async Task TapAsync_TaskGeneric_SyncAction_OnFailure_DoesNotExecute()
    {
        // Arrange
        var executed = false;

        // Act
        await Task.FromResult(Result<int>.Fail("Error"))
            .TapAsync(value => executed = true);

        // Assert
        Assert.IsFalse(executed);
    }

    [TestMethod]
    public async Task TapAsync_TaskGeneric_SyncAction_WithChaining_Works()
    {
        // Arrange
        var log = new List<string>();

        // Act
        await GetUserAsync(123)
            .TapAsync(user => log.Add($"Got user: {user.Name}"))
            .TapAsync(user => log.Add($"Age: {user.Age}"));

        // Assert
        Assert.HasCount(2, log);
        Assert.Contains("Got user:", log[0]);
        Assert.Contains("Age:", log[1]);
    }

    #endregion

    #region TapAsync - Task<Result<T>> - Async Action

    [TestMethod]
    public async Task TapAsync_TaskGeneric_AsyncAction_OnSuccess_ExecutesAction()
    {
        // Arrange
        var capturedValue = 0;

        // Act
        await Task.FromResult(Result<int>.Ok(42))
            .TapAsync(async value =>
            {
                await Task.Delay(10);
                capturedValue = value;
            });

        // Assert
        Assert.AreEqual(42, capturedValue);
    }

    [TestMethod]
    public async Task TapAsync_TaskGeneric_AsyncAction_OnFailure_DoesNotExecute()
    {
        // Arrange
        var executed = false;

        // Act
        await Task.FromResult(Result<int>.Fail("Error"))
            .TapAsync(async value =>
            {
                await Task.Delay(10);
                executed = true;
            });

        // Assert
        Assert.IsFalse(executed);
    }

    [TestMethod]
    public async Task TapAsync_TaskGeneric_AsyncAction_WithChaining_Works()
    {
        // Arrange
        var log = new List<string>();

        // Act
        await GetUserAsync(123)
            .TapAsync(async user =>
            {
                await Task.Delay(10);
                log.Add($"Processing: {user.Name}");
            })
            .TapAsync(async user =>
            {
                await Task.Delay(10);
                log.Add("Complete");
            });

        // Assert
        Assert.HasCount(2, log);
        Assert.Contains("Processing:", log[0]);
        Assert.AreEqual("Complete", log[1]);
    }

    #endregion

    #region Integration Tests - Chaining with Map and Bind

    [TestMethod]
    public void Tap_ChainedWithMap_Works()
    {
        // Arrange
        var result = Result<int>.Ok(10);
        var logged = new List<string>();

        // Act
        var final = result
            .Tap(v => logged.Add($"Original: {v}"))
            .Map(v => v * 2)
            .Tap(v => logged.Add($"Doubled: {v}"))
            .Map(v => v.ToString());

        // Assert
        Assert.IsTrue(final.IsSuccess);
        Assert.AreEqual("20", final.Value);
        Assert.HasCount(2, logged);
        Assert.AreEqual("Original: 10", logged[0]);
        Assert.AreEqual("Doubled: 20", logged[1]);
    }

    [TestMethod]
    public void Tap_ChainedWithBind_Works()
    {
        // Arrange
        var result = Result<int>.Ok(5);
        var operations = new List<string>();

        // Act
        var final = result
            .Tap(v => operations.Add("Before bind"))
            .Bind(v => Result<string>.Ok(v.ToString()))
            .Tap(v => operations.Add($"After bind: {v}"));

        // Assert
        Assert.IsTrue(final.IsSuccess);
        Assert.AreEqual("5", final.Value);
        Assert.HasCount(2, operations);
    }

    [TestMethod]
    public void Tap_InWorkflow_StopsExecutingAfterFailure()
    {
        // Arrange
        var result = Result<int>.Ok(10);
        var executionLog = new List<string>();

        // Act
        var final = result
            .Tap(v => executionLog.Add("Step 1"))
            .Bind(v => Result<int>.Fail("Failed here"))
            .Tap(v => executionLog.Add("Step 2")) // Should not execute
            .Map(v => v * 2);

        // Assert
        Assert.IsTrue(final.IsFailed);
        Assert.HasCount(1, executionLog);
        Assert.AreEqual("Step 1", executionLog[0]);
    }

    [TestMethod]
    public async Task TapAsync_ChainedWithMapAndBind_Works()
    {
        // Arrange
        var log = new List<string>();

        // Act
        var result = await GetUserAsync(123)
            .TapAsync(user => log.Add($"Fetched: {user.Name}"))
            .TapAsync(async user => await LogToServiceAsync($"Processing {user.Name}"))
            .TapAsync(user => log.Add("Validation next"));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(2, log);
    }

    #endregion

    #region Real-World Scenarios

    [TestMethod]
    public async Task TapAsync_ForLogging_RealWorldScenario()
    {
        // Arrange
        var logger = new TestLogger();

        // Act
        var result = await GetUserAsync(42)
            .TapAsync(async u => await logger.LogAsync($"Processing user: {u.Name}"))
            .TapAsync(u => logger.Log($"Age: {u.Age}"))
            .TapAsync(async u => await logger.LogAsync("Processing complete"));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(3, logger.Logs);
        Assert.Contains("Processing user:", logger.Logs[0]);
        Assert.Contains("Age:", logger.Logs[1]);
        Assert.AreEqual("Processing complete", logger.Logs[2]);
    }

    [TestMethod]
    public async Task TapAsync_ForAuditing_RealWorldScenario()
    {
        // Arrange
        var auditTrail = new List<AuditEntry>();
        var userId = "user-123";

        // Act
        var result = await CreateOrderAsync(userId)
            .TapAsync(order => auditTrail.Add(new AuditEntry
            {
                Action = "OrderCreated",
                OrderId = order.Id,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            }))
            .TapAsync(async order => await ValidateOrderAsync(order))
            .TapAsync(order => auditTrail.Add(new AuditEntry
            {
                Action = "OrderValidated",
                OrderId = order.Id,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            }));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(2, auditTrail);
        Assert.AreEqual("OrderCreated", auditTrail[0].Action);
        Assert.AreEqual("OrderValidated", auditTrail[1].Action);
    }

    [TestMethod]
    public async Task TapAsync_MixingSyncAndAsync_Works()
    {
        // Arrange
        var log = new List<string>();

        // Act
        var result = await GetUserAsync(123)
            .TapAsync(u => log.Add($"Sync: {u.Name}"))                    // Sync action
            .TapAsync(async u => await Task.Run(() => log.Add("Async"))) // Async action
            .TapAsync(u => log.Add("Sync again"));                        // Sync action

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(3, log);
        Assert.Contains("Sync:", log[0]);
        Assert.AreEqual("Async", log[1]);
        Assert.AreEqual("Sync again", log[2]);
    }

    [TestMethod]
    public void Tap_WithSideEffectsThatThrow_DoesNotCatchException()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            result.Tap(v => throw new InvalidOperationException("Side effect failed"))
        );
    }

    [TestMethod]
    public async Task TapAsync_WithSideEffectsThatThrow_DoesNotCatchException()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await result.TapAsync(async v =>
            {
                await Task.Delay(10);
                throw new InvalidOperationException("Async side effect failed");
            })
        );
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public async Task TapAsync_WithNullValue_Works()
    {
        // Arrange
        var result = Result<string?>.Ok(null);
        string? capturedValue = "not null";

        // Act
        await result.TapAsync(async v =>
        {
            await Task.Delay(10);
            capturedValue = v;
        });

        // Assert
        Assert.IsNull(capturedValue);
    }

    [TestMethod]
    public async Task TapAsync_EmptyChain_DoesNothing()
    {
        // Arrange & Act
        var result = await GetUserAsync(123);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task TapAsync_LongChain_ExecutesInOrder()
    {
        // Arrange
        var executionOrder = new List<int>();

        // Act
        await GetUserAsync(123)
            .TapAsync(u => executionOrder.Add(1))
            .TapAsync(async u => { await Task.Delay(5); executionOrder.Add(2); })
            .TapAsync(u => executionOrder.Add(3))
            .TapAsync(async u => { await Task.Delay(5); executionOrder.Add(4); })
            .TapAsync(u => executionOrder.Add(5));

        // Assert
        Assert.AreEqual(5, executionOrder.Count);
        for (int i = 0; i < 5; i++)
        {
            Assert.AreEqual(i + 1, executionOrder[i]);
        }
    }

    #endregion

    #region Helper Methods and Classes

    private static Task<Result> GetResultAsync()
    {
        return Task.FromResult(Result.Ok());
    }

    private static Task<Result<Person>> GetUserAsync(int id)
    {
        return Task.FromResult(Result<Person>.Ok(new Person
        {
            Id = id,
            Name = $"User{id}",
            Age = 30
        }));
    }

    private static Task<Result<Order>> CreateOrderAsync(string userId)
    {
        return Task.FromResult(Result<Order>.Ok(new Order
        {
            Id = "ORD-001",
            UserId = userId,
            Total = 99.99m
        }));
    }

    private static Task ValidateOrderAsync(Order order)
    {
        return Task.Delay(10);
    }

    private static Task LogToServiceAsync(string message)
    {
        return Task.Delay(10);
    }

    private class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    private class Order
    {
        public string Id { get; set; } = "";
        public string UserId { get; set; } = "";
        public decimal Total { get; set; }
    }

    private class AuditEntry
    {
        public string Action { get; set; } = "";
        public string OrderId { get; set; } = "";
        public string UserId { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    private class TestLogger
    {
        public List<string> Logs { get; } = new();
        public void Log(string message) => Logs.Add(message);
        public Task LogAsync(string message)
        {
            Logs.Add(message);
            return Task.Delay(10);
        }
    }

    #endregion
}