using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.AdvancedPatterns;
using REslava.Result.Extensions;

namespace REslava.Result.Tests.Results;

/// <summary>
/// Tests for <see cref="ResultContext"/> — auto-seeding, fluent merging,
/// pipeline propagation (parent-wins), Map entity update, error enrichment
/// (non-overwriting), and non-generic Result context behaviour.
/// </summary>
[TestClass]
public sealed class ResultContextTests
{
    // ── Private domain types ─────────────────────────────────────────────────
    private sealed class Order { }
    private sealed class OrderDto { }
    private sealed class E1 : Error { public E1() : base("E1") { } }
    private sealed class E2 : Error { public E2() : base("E2") { } }

    // =========================================================================
    // Step 28 — Ok<T>() / Fail<T>() seed Entity = typeof(T).Name
    // =========================================================================

    #region Auto-seeding

    [TestMethod]
    public void Ok_SeedsEntityFromTypeName_NonNullableOtherFields()
    {
        var r = Result<Order>.Ok(new Order());

        Assert.AreEqual("Order", r.Context?.Entity);
        Assert.IsNull(r.Context?.EntityId);
        Assert.IsNull(r.Context?.CorrelationId);
        Assert.IsNull(r.Context?.OperationName);
        Assert.IsNull(r.Context?.TenantId);
    }

    [TestMethod]
    public void Fail_SeedsEntityFromTypeName()
    {
        var r = Result<Order>.Fail("error");

        Assert.AreEqual("Order", r.Context?.Entity);
    }

    [TestMethod]
    public void Ok_TypedPipeline_SeedsEntityFromTypeName()
    {
        var r = Result<Order, E1>.Ok(new Order());

        Assert.AreEqual("Order", r.Context?.Entity);
        Assert.IsNull(r.Context?.EntityId);
    }

    [TestMethod]
    public void Fail_TypedPipeline_SeedsEntityFromTypeName()
    {
        var r = Result<Order, E1>.Fail(new E1());

        Assert.AreEqual("Order", r.Context?.Entity);
    }

    #endregion

    // =========================================================================
    // Step 29 — WithContext merges into existing Context
    // =========================================================================

    #region WithContext merging

    [TestMethod]
    public void WithContext_MergesRuntimeValues_PreservesAutoSeededEntity()
    {
        var r = Result<Order>.Ok(new Order())
            .WithContext(entityId: "order-42", correlationId: "trace-abc");

        Assert.AreEqual("Order", r.Context?.Entity);          // preserved
        Assert.AreEqual("order-42", r.Context?.EntityId);
        Assert.AreEqual("trace-abc", r.Context?.CorrelationId);
    }

    [TestMethod]
    public void WithContext_CalledTwice_MergesIncrementally()
    {
        var r = Result<Order>.Ok(new Order())
            .WithContext(entityId: "first", correlationId: "trace-1")
            .WithContext(entityId: "second");   // overrides only EntityId

        Assert.AreEqual("second", r.Context?.EntityId);
        Assert.AreEqual("trace-1", r.Context?.CorrelationId);  // preserved from first call
        Assert.AreEqual("Order", r.Context?.Entity);
    }

    [TestMethod]
    public void WithContext_AllFields_CanSetAllAtOnce()
    {
        var r = Result<Order>.Ok(new Order())
            .WithContext(
                entityId:      "e-1",
                correlationId: "corr-1",
                operationName: "ProcessOrder",
                tenantId:      "tenant-1");

        Assert.AreEqual("Order", r.Context?.Entity);
        Assert.AreEqual("e-1", r.Context?.EntityId);
        Assert.AreEqual("corr-1", r.Context?.CorrelationId);
        Assert.AreEqual("ProcessOrder", r.Context?.OperationName);
        Assert.AreEqual("tenant-1", r.Context?.TenantId);
    }

    #endregion

    // =========================================================================
    // Step 30 — Bind propagates parent context, ignores child context
    // =========================================================================

    #region Bind context propagation

    [TestMethod]
    public void Bind_OnSuccess_ParentContextWins_IgnoresChildContext()
    {
        var parent = Result<Order>.Ok(new Order())
            .WithContext(entityId: "parent-42", correlationId: "trace-parent");

        var child = parent.Bind(o => Result<OrderDto>.Ok(new OrderDto())
            .WithContext(entityId: "child-999"));  // child context is ignored

        Assert.IsTrue(child.IsSuccess);
        Assert.AreEqual("parent-42", child.Context?.EntityId);       // parent wins
        Assert.AreEqual("trace-parent", child.Context?.CorrelationId);  // parent wins
    }

    [TestMethod]
    public void Bind_OnBinderFailure_PropagatesParentContext()
    {
        var parent = Result<Order>.Ok(new Order())
            .WithContext(entityId: "parent-42", correlationId: "trace-parent");

        var child = parent.Bind(_ => Result<OrderDto>.Fail("step failed"));

        Assert.IsTrue(child.IsFailure);
        Assert.AreEqual("parent-42", child.Context?.EntityId);
        Assert.AreEqual("trace-parent", child.Context?.CorrelationId);
    }

    [TestMethod]
    public void Bind_WhenAlreadyFailed_PassesThroughContext()
    {
        var alreadyFailed = Result<Order>.Fail("initial error")
            .WithContext(entityId: "fail-ctx");

        // Note: Result<T>.WithContext on a failed result still updates context
        var child = alreadyFailed.Bind(_ => Result<OrderDto>.Ok(new OrderDto()));

        Assert.IsTrue(child.IsFailure);
        Assert.AreEqual("fail-ctx", child.Context?.EntityId);
    }

    #endregion

    // =========================================================================
    // Step 31 — Map updates Entity to typeof(TOut).Name, preserves other fields
    // =========================================================================

    #region Map entity update

    [TestMethod]
    public void Map_OnSuccess_UpdatesEntityToOutputType_PreservesOtherFields()
    {
        var r = Result<Order>.Ok(new Order())
            .WithContext(entityId: "42", correlationId: "trace-1", operationName: "PlaceOrder")
            .Map(_ => new OrderDto());

        Assert.AreEqual("OrderDto", r.Context?.Entity);             // updated
        Assert.AreEqual("42", r.Context?.EntityId);                 // preserved
        Assert.AreEqual("trace-1", r.Context?.CorrelationId);       // preserved
        Assert.AreEqual("PlaceOrder", r.Context?.OperationName);    // preserved
    }

    [TestMethod]
    public void Map_OnFailure_KeepsParentEntityUnchanged()
    {
        var r = Result<Order>.Fail("error")
            .Map(_ => new OrderDto());

        Assert.IsTrue(r.IsFailure);
        Assert.AreEqual("Order", r.Context?.Entity);  // not updated — mapping did not occur
    }

    #endregion

    // =========================================================================
    // Step 32 — Error auto-enrichment: non-overwriting, Context fills missing tags
    // =========================================================================

    #region Error auto-enrichment

    [TestMethod]
    public void Ensure_WhenPredicateFails_EnrichesErrorWithAllContextTags()
    {
        var result = Result<Order>.Ok(new Order())
            .WithContext(entityId: "order-42", correlationId: "trace-abc", tenantId: "t-1")
            .Ensure(_ => false, new Error("guard failed"));

        Assert.IsTrue(result.IsFailure);
        var error = result.Errors[0];
        Assert.AreEqual("Order", error.Tags["Entity"]);
        Assert.AreEqual("order-42", error.Tags["EntityId"]);
        Assert.AreEqual("trace-abc", error.Tags["CorrelationId"]);
        Assert.AreEqual("t-1", error.Tags["TenantId"]);
    }

    [TestMethod]
    public void Ensure_WhenErrorHasEntityTagAlreadySet_DoesNotOverwrite_FillsMissingTags()
    {
        // Error with Entity pre-set by a "factory" — simulates a typed factory setting it
        var factoryError = new Error("guard failed")
            .WithTag(DomainTags.Entity, "FactoryEntity");

        var result = Result<Order>.Ok(new Order())
            .WithContext(entityId: "order-42", correlationId: "trace-abc")
            .Ensure(_ => false, factoryError);

        Assert.IsTrue(result.IsFailure);
        var error = result.Errors[0];

        // Factory-set Entity is preserved — context must not overwrite
        Assert.AreEqual("FactoryEntity", error.Tags["Entity"]);
        // Missing EntityId is filled from context
        Assert.AreEqual("order-42", error.Tags["EntityId"]);
        // Missing CorrelationId is filled from context
        Assert.AreEqual("trace-abc", error.Tags["CorrelationId"]);
    }

    [TestMethod]
    public void Bind_WhenBinderReturnsFail_EnrichesBinderErrorWithParentContextTags()
    {
        var result = Result<Order>.Ok(new Order())
            .WithContext(entityId: "order-42", correlationId: "trace-abc")
            .Bind(_ => Result<OrderDto>.Fail(new Error("step failed")));

        Assert.IsTrue(result.IsFailure);
        var error = result.Errors[0];
        Assert.AreEqual("Order", error.Tags["Entity"]);
        Assert.AreEqual("order-42", error.Tags["EntityId"]);
        Assert.AreEqual("trace-abc", error.Tags["CorrelationId"]);
    }

    [TestMethod]
    public void Ensure_NoContext_ErrorTagsUnchanged()
    {
        // Without WithContext, no tags are injected — no crash
        var result = Result<Order>.Ok(new Order())
            .Ensure(_ => false, new Error("guard failed"));

        Assert.IsTrue(result.IsFailure);
        var error = result.Errors[0];
        // Entity IS present because Ok() auto-seeds Context.Entity = "Order"
        Assert.AreEqual("Order", error.Tags["Entity"]);
        Assert.IsFalse(error.Tags.ContainsKey("EntityId"));
        Assert.IsFalse(error.Tags.ContainsKey("CorrelationId"));
    }

    #endregion

    // =========================================================================
    // Step 33 — Result<T,TError> — same propagation rules as Result<T>
    // =========================================================================

    #region Typed pipeline context propagation

    [TestMethod]
    public void TypedPipeline_Bind_PropagatesParentContext_ParentEntityWins()
    {
        var parent = Result<Order, E1>.Ok(new Order())
            .WithContext(entityId: "parent-42", correlationId: "trace-parent");

        // Typed Bind 1→2: Result<Order,E1> + Result<string,E2> → Result<string,ErrorsOf<E1,E2>>
        var child = parent.Bind(o => Result<string, E2>.Ok("ok"));

        Assert.IsTrue(child.IsSuccess);
        Assert.AreEqual("parent-42", child.Context?.EntityId);
        Assert.AreEqual("trace-parent", child.Context?.CorrelationId);
        Assert.AreEqual("Order", child.Context?.Entity);  // parent entity wins, not "String"
    }

    [TestMethod]
    public void TypedPipeline_Bind_OnFailure_PropagatesParentContext()
    {
        var parent = Result<Order, E1>.Ok(new Order())
            .WithContext(entityId: "parent-42", correlationId: "trace-parent");

        var child = parent.Bind(_ => Result<string, E2>.Fail(new E2()));

        Assert.IsTrue(child.IsFailure);
        Assert.AreEqual("parent-42", child.Context?.EntityId);
        Assert.AreEqual("trace-parent", child.Context?.CorrelationId);
    }

    [TestMethod]
    public void TypedPipeline_Map_UpdatesEntityToOutputType_PreservesOtherFields()
    {
        var r = Result<Order, E1>.Ok(new Order())
            .WithContext(entityId: "42", correlationId: "trace-1")
            .Map(_ => new OrderDto());

        Assert.IsTrue(r.IsSuccess);
        Assert.AreEqual("OrderDto", r.Context?.Entity);   // updated
        Assert.AreEqual("42", r.Context?.EntityId);        // preserved
        Assert.AreEqual("trace-1", r.Context?.CorrelationId);  // preserved
    }

    [TestMethod]
    public void TypedPipeline_Ensure_WhenPredicateFails_PropagatesParentContext()
    {
        var result = Result<Order, E1>.Ok(new Order())
            .WithContext(entityId: "42", correlationId: "trace-1")
            .Ensure(_ => false, new E2());

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual("42", result.Context?.EntityId);
        Assert.AreEqual("trace-1", result.Context?.CorrelationId);
    }

    #endregion

    // =========================================================================
    // Step 34 — Non-generic Result: Context null by default, set via WithContext
    // =========================================================================

    #region Non-generic Result

    [TestMethod]
    public void Result_NonGeneric_Ok_ContextIsNullByDefault()
    {
        var r = Result.Ok();
        Assert.IsNull(r.Context);
    }

    [TestMethod]
    public void Result_NonGeneric_Fail_ContextIsNullByDefault()
    {
        var r = Result.Fail("error");
        Assert.IsNull(r.Context);
    }

    [TestMethod]
    public void Result_NonGeneric_WithContext_SetsAllFields()
    {
        var r = Result.Ok()
            .WithContext(
                entity:        "Order",
                entityId:      "42",
                correlationId: "trace-1",
                operationName: "ProcessOrder",
                tenantId:      "t-1");

        Assert.AreEqual("Order", r.Context?.Entity);
        Assert.AreEqual("42", r.Context?.EntityId);
        Assert.AreEqual("trace-1", r.Context?.CorrelationId);
        Assert.AreEqual("ProcessOrder", r.Context?.OperationName);
        Assert.AreEqual("t-1", r.Context?.TenantId);
    }

    [TestMethod]
    public void Result_NonGeneric_WithContext_IsPassthrough_DoesNotChangeIsSuccess()
    {
        var ok = Result.Ok().WithContext(entity: "Order");
        Assert.IsTrue(ok.IsSuccess);

        var fail = Result.Fail("err").WithContext(entity: "Order");
        Assert.IsTrue(fail.IsFailure);
    }

    [TestMethod]
    public void Result_NonGeneric_WithContext_CalledTwice_MergesIncrementally()
    {
        var r = Result.Ok()
            .WithContext(entity: "Order", entityId: "1")
            .WithContext(entityId: "2");  // overrides only EntityId

        Assert.AreEqual("Order", r.Context?.Entity);  // preserved
        Assert.AreEqual("2", r.Context?.EntityId);    // overridden
    }

    #endregion
}
