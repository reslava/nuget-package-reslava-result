namespace REslava.Result.Tests;

/// <summary>
/// Comprehensive tests for Result extension methods (Tap)
/// </summary>
[TestClass]
public sealed class ResultExtensionsTests
{
    #region Tap Tests - Success Cases

    [TestMethod]
    public void Tap_OnSuccess_ExecutesAction()
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
    public void Tap_OnSuccess_ReturnsSameInstance()
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
    public void Tap_OnSuccess_CanModifyExternalState()
    {
        // Arrange
        var result = Result<int>.Ok(10);
        var sideEffectList = new List<string>();

        // Act
        result.Tap(value => sideEffectList.Add($"Processing: {value}"));

        // Assert
        Assert.HasCount(1, sideEffectList);
        Assert.AreEqual("Processing: 10", sideEffectList[0]);
    }

    [TestMethod]
    public void Tap_OnSuccess_MultipleCallsExecuteInOrder()
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

    #endregion

    #region Tap Tests - Failure Cases

    [TestMethod]
    public void Tap_OnFailure_DoesNotExecuteAction()
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
    public void Tap_OnFailure_PreservesErrors()
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
    public void Tap_OnFailure_MultipleCallsDoNotExecute()
    {
        // Arrange
        var result = Result<int>.Fail("Error");
        var executionCount = 0;

        // Act
        result
            .Tap(v => executionCount++)
            .Tap(v => executionCount++)
            .Tap(v => executionCount++);

        // Assert
        Assert.AreEqual(0, executionCount);
    }

    #endregion

    #region Tap Tests - Chaining

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

    #endregion

    #region Tap Tests - Complex Scenarios

    [TestMethod]
    public void Tap_WithComplexType_Works()
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

    [TestMethod]
    public void Tap_ForLogging_RealWorldScenario()
    {
        // Arrange
        var logger = new TestLogger();

        // Act
        var result = Result<int>.Ok(42)
            .Tap(v => logger.Log($"Processing value: {v}"))
            .Map(v => v * 2)
            .Tap(v => logger.Log($"Doubled to: {v}"))
            .Bind(v => v > 50
                ? Result<string>.Ok($"Large: {v}")
                : Result<string>.Fail("Too small"))
            .Tap(v => logger.Log($"Final result: {v}"));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(3, logger.Logs);
        Assert.AreEqual("Processing value: 42", logger.Logs[0]);
        Assert.AreEqual("Doubled to: 84", logger.Logs[1]);
        Assert.AreEqual("Final result: Large: 84", logger.Logs[2]);
    }

    [TestMethod]
    public void Tap_ForAuditing_RealWorldScenario()
    {
        // Arrange
        var auditTrail = new List<AuditEntry>();
        var userId = "user-123";

        // Act
        var result = Result<Order>.Ok(new Order { Id = "ORD-001", Total = 99.99m })
            .Tap(order => auditTrail.Add(new AuditEntry
            {
                Action = "OrderCreated",
                OrderId = order.Id,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            }))
            .Bind(order => ValidateOrder(order))
            .Tap(order => auditTrail.Add(new AuditEntry
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
    public void Tap_WithSideEffectsThatThrow_DoesNotCatchException()
    {
        // Arrange
        var result = Result<int>.Ok(10);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            result.Tap(v => throw new InvalidOperationException("Side effect failed"))
        );
    }

    #endregion

    #region Helper Classes

    private class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    private class Order
    {
        public string Id { get; set; } = "";
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
    }

    private Result<Order> ValidateOrder(Order order)
    {
        return order.Total > 0
            ? Result<Order>.Ok(order)
            : Result<Order>.Fail("Invalid order total");
    }

    #endregion
}
