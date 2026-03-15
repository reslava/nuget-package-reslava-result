using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;
using REslava.Result.Serialization;

namespace REslava.Result.Tests.Reasons;

[TestClass]
public class ReasonMetadataTests
{
    // -------------------------------------------------------------------------
    // ReasonMetadata record
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ReasonMetadata_Empty_IsNotNull()
        => Assert.IsNotNull(ReasonMetadata.Empty);

    [TestMethod]
    public void ReasonMetadata_Empty_AllPropertiesAreNull()
    {
        var empty = ReasonMetadata.Empty;
        Assert.IsNull(empty.CallerMember);
        Assert.IsNull(empty.CallerFile);
        Assert.IsNull(empty.CallerLine);
    }

    [TestMethod]
    public void ReasonMetadata_ValueEquality_SameValues_AreEqual()
    {
        var a = new ReasonMetadata { CallerMember = "Foo", CallerFile = "Bar.cs", CallerLine = 42 };
        var b = new ReasonMetadata { CallerMember = "Foo", CallerFile = "Bar.cs", CallerLine = 42 };
        Assert.AreEqual(a, b);
    }

    [TestMethod]
    public void ReasonMetadata_ValueEquality_DifferentValues_AreNotEqual()
    {
        var a = new ReasonMetadata { CallerMember = "Foo" };
        var b = new ReasonMetadata { CallerMember = "Bar" };
        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void ReasonMetadata_WithExpression_ProducesUpdatedCopy()
    {
        var original = new ReasonMetadata { CallerMember = "Original", CallerLine = 10 };
        var updated  = original with { CallerMember = "Updated" };

        Assert.AreEqual("Updated", updated.CallerMember);
        Assert.AreEqual(10, updated.CallerLine);
        Assert.AreEqual("Original", original.CallerMember); // original unchanged
    }

    // -------------------------------------------------------------------------
    // IReasonMetadata interface — built-in types
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Error_Implements_IReasonMetadata()
        => Assert.IsInstanceOfType<IReasonMetadata>(new Error("msg"));

    [TestMethod]
    public void ValidationError_Implements_IReasonMetadata()
        => Assert.IsInstanceOfType<IReasonMetadata>(new ValidationError("msg"));

    [TestMethod]
    public void NotFoundError_Implements_IReasonMetadata()
        => Assert.IsInstanceOfType<IReasonMetadata>(new NotFoundError("Entity"));

    [TestMethod]
    public void ConflictError_Implements_IReasonMetadata()
        => Assert.IsInstanceOfType<IReasonMetadata>(new ConflictError("msg"));

    [TestMethod]
    public void ForbiddenError_Implements_IReasonMetadata()
        => Assert.IsInstanceOfType<IReasonMetadata>(new ForbiddenError());

    [TestMethod]
    public void UnauthorizedError_Implements_IReasonMetadata()
        => Assert.IsInstanceOfType<IReasonMetadata>(new UnauthorizedError());

    [TestMethod]
    public void ExceptionError_Implements_IReasonMetadata()
        => Assert.IsInstanceOfType<IReasonMetadata>(new ExceptionError(new Exception("boom")));

    [TestMethod]
    public void ConversionError_Implements_IReasonMetadata()
        => Assert.IsInstanceOfType<IReasonMetadata>(new ConversionError("msg"));

    [TestMethod]
    public void Success_Implements_IReasonMetadata()
        => Assert.IsInstanceOfType<IReasonMetadata>(new Success("ok"));

    // -------------------------------------------------------------------------
    // IReasonMetadata — default Metadata is Empty
    // -------------------------------------------------------------------------

    [TestMethod]
    public void NewError_DefaultMetadata_CapturesCallerMember()
    {
        var error = new Error("msg");
        Assert.AreEqual(nameof(NewError_DefaultMetadata_CapturesCallerMember), error.Metadata.CallerMember);
    }

    // -------------------------------------------------------------------------
    // Pattern matching from IReason-typed reference
    // -------------------------------------------------------------------------

    [TestMethod]
    public void IReason_PatternMatch_IReasonMetadata_Succeeds_ForBuiltInError()
    {
        IReason reason = new Error("msg");
        Assert.IsTrue(reason is IReasonMetadata);
    }

    [TestMethod]
    public void IReason_PatternMatch_IReasonMetadata_Fails_ForExternalStub()
    {
        IReason reason = new StubReason();
        Assert.IsFalse(reason is IReasonMetadata);
    }

    // -------------------------------------------------------------------------
    // TryGetMetadata extension method
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TryGetMetadata_OnBuiltInError_ReturnsMetadata()
    {
        IReason reason = new Error("msg");
        var metadata = reason.TryGetMetadata();
        Assert.IsNotNull(metadata);
    }

    [TestMethod]
    public void TryGetMetadata_OnExternalStub_ReturnsNull()
    {
        IReason reason = new StubReason();
        Assert.IsNull(reason.TryGetMetadata());
    }

    // -------------------------------------------------------------------------
    // HasCallerInfo extension method
    // -------------------------------------------------------------------------

    [TestMethod]
    public void HasCallerInfo_WhenMetadataIsEmpty_ReturnsFalse()
    {
        IReason reason = new Error("msg").WithMetadata(ReasonMetadata.Empty);
        Assert.IsFalse(reason.HasCallerInfo());
    }

    [TestMethod]
    public void HasCallerInfo_WhenMetadataHasCallerMember_ReturnsTrue()
    {
        var error = new Error("msg")
            .WithMetadata(new ReasonMetadata { CallerMember = "MyMethod" });

        Assert.IsTrue(((IReason)error).HasCallerInfo());
    }

    [TestMethod]
    public void HasCallerInfo_OnExternalStub_ReturnsFalse()
    {
        IReason reason = new StubReason();
        Assert.IsFalse(reason.HasCallerInfo());
    }

    // -------------------------------------------------------------------------
    // WithMetadata — preserves through CRTP chain
    // -------------------------------------------------------------------------

    [TestMethod]
    public void WithMetadata_AttachesMetadata()
    {
        var meta  = new ReasonMetadata { CallerMember = "CreateOrder", CallerLine = 99 };
        var error = new Error("msg").WithMetadata(meta);

        Assert.AreEqual("CreateOrder", error.Metadata.CallerMember);
        Assert.AreEqual(99, error.Metadata.CallerLine);
    }

    [TestMethod]
    public void WithMessage_PreservesMetadata()
    {
        var meta  = new ReasonMetadata { CallerMember = "CreateOrder" };
        var error = new Error("original").WithMetadata(meta).WithMessage("updated");

        Assert.AreEqual("CreateOrder", error.Metadata.CallerMember);
    }

    [TestMethod]
    public void WithTag_PreservesMetadata()
    {
        var meta  = new ReasonMetadata { CallerMember = "CreateOrder" };
        var error = new Error("msg").WithMetadata(meta).WithTag("Code", 42);

        Assert.AreEqual("CreateOrder", error.Metadata.CallerMember);
    }

    [TestMethod]
    public void WithMetadata_Null_UsesEmpty()
    {
        var error = new Error("msg")
            .WithMetadata(new ReasonMetadata { CallerMember = "X" })
            .WithMetadata(null!);

        Assert.AreEqual(ReasonMetadata.Empty, error.Metadata);
    }

    // -------------------------------------------------------------------------
    // Factory caller capture (items 19)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void NotFoundError_Constructor_CapturesCallerMember()
    {
        var error = new NotFoundError("User", 42);
        Assert.AreEqual(nameof(NotFoundError_Constructor_CapturesCallerMember), error.Metadata.CallerMember);
    }

    [TestMethod]
    public void ConflictError_Duplicate_T_CapturesCallerMember()
    {
        var error = ConflictError.Duplicate<TestEntity>("email", "test@example.com");
        Assert.AreEqual(nameof(ConflictError_Duplicate_T_CapturesCallerMember), error.Metadata.CallerMember);
    }

    [TestMethod]
    public void ValidationError_Field_CapturesCallerMember()
    {
        var error = ValidationError.Field("Email", "Invalid format");
        Assert.AreEqual(nameof(ValidationError_Field_CapturesCallerMember), error.Metadata.CallerMember);
    }

    [TestMethod]
    public void ForbiddenError_For_CapturesCallerMember()
    {
        var error = ForbiddenError.For("Delete", "Order");
        Assert.AreEqual(nameof(ForbiddenError_For_CapturesCallerMember), error.Metadata.CallerMember);
    }

    // -------------------------------------------------------------------------
    // Auto-tags (item 20)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void NotFoundError_Constructor_AutoTags_EntityNameAndId()
    {
        var error = new NotFoundError("User", 42);
        Assert.AreEqual("User", error.Tags[DomainTags.Entity.Name]);
        Assert.AreEqual("42", error.Tags[DomainTags.EntityId.Name]);
    }

    [TestMethod]
    public void ConflictError_Duplicate_T_AutoTags_EntityFieldValue()
    {
        var error = ConflictError.Duplicate<TestEntity>("email", "test@example.com");
        Assert.AreEqual("TestEntity", error.Tags[DomainTags.Entity.Name]);
        Assert.AreEqual("email", error.Tags[DomainTags.Field.Name]);
        Assert.AreEqual("test@example.com", error.Tags[DomainTags.Value.Name]);
    }

    // -------------------------------------------------------------------------
    // JSON roundtrip (item 21)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void JsonRoundTrip_PreservesMetadataCallerMember()
    {
        var options = new JsonSerializerOptions().AddREslavaResultConverters();
        var original = ValidationError.Field("Email", "Invalid format");
        var result   = Result<string>.Fail(original);

        var json     = JsonSerializer.Serialize(result, options);
        var restored = JsonSerializer.Deserialize<Result<string>>(json, options)!;

        var restoredMeta = restored.Errors[0].TryGetMetadata();
        Assert.IsNotNull(restoredMeta);
        Assert.AreEqual(nameof(JsonRoundTrip_PreservesMetadataCallerMember), restoredMeta!.CallerMember);
    }

    // -------------------------------------------------------------------------
    // Stub helper
    // -------------------------------------------------------------------------

    private sealed class StubReason : IReason
    {
        public string Message { get; init; } = "stub";
        public ImmutableDictionary<string, object> Tags { get; init; }
            = ImmutableDictionary<string, object>.Empty;
    }

    private sealed class TestEntity { }
}
