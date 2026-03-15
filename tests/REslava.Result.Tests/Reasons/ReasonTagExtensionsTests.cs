using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace REslava.Result.Tests.Reasons;

[TestClass]
public sealed class ReasonTagExtensionsTests
{
    private static readonly TagKey<string> NameKey   = new("Name");
    private static readonly TagKey<int>    ScoreKey  = new("Score");
    private static readonly TagKey<string> MissingKey = new("Missing");

    // -------------------------------------------------------------------------
    // TryGet<T> — success cases
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TryGet_StringKey_ReturnsValue()
    {
        var error = new Error("test").WithTag(NameKey, "Alice");

        var found = error.TryGet(NameKey, out var value);

        Assert.IsTrue(found);
        Assert.AreEqual("Alice", value);
    }

    [TestMethod]
    public void TryGet_IntKey_ReturnsValue()
    {
        var error = new Error("test").WithTag(ScoreKey, 42);

        var found = error.TryGet(ScoreKey, out var value);

        Assert.IsTrue(found);
        Assert.AreEqual(42, value);
    }

    [TestMethod]
    public void TryGet_MissingKey_ReturnsFalse()
    {
        var error = new Error("test");

        var found = error.TryGet(MissingKey, out var value);

        Assert.IsFalse(found);
        Assert.IsNull(value);
    }

    [TestMethod]
    public void TryGet_WrongType_ReturnsFalse()
    {
        // Tag stored as int, retrieved as string — type mismatch
        var error = new Error("test").WithTag(ScoreKey, 42);
        var stringKey = new TagKey<string>("Score");

        var found = error.TryGet(stringKey, out var value);

        Assert.IsFalse(found);
        Assert.IsNull(value);
    }

    // -------------------------------------------------------------------------
    // Has<T>
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Has_PresentKey_ReturnsTrue()
    {
        var error = new Error("test").WithTag(NameKey, "Alice");

        Assert.IsTrue(error.Has(NameKey));
    }

    [TestMethod]
    public void Has_MissingKey_ReturnsFalse()
    {
        var error = new Error("test");

        Assert.IsFalse(error.Has(MissingKey));
    }

    [TestMethod]
    public void Has_WrongType_ReturnsFalse()
    {
        var error = new Error("test").WithTag(ScoreKey, 42);
        var stringKey = new TagKey<string>("Score");

        Assert.IsFalse(error.Has(stringKey));
    }

    // -------------------------------------------------------------------------
    // Typed WithTag round-trip
    // -------------------------------------------------------------------------

    [TestMethod]
    public void WithTag_TypedKey_RoundTrips()
    {
        var key   = new TagKey<int>("Retries");
        var error = new Error("test").WithTag(key, 3);

        var found = error.TryGet(key, out var value);

        Assert.IsTrue(found);
        Assert.AreEqual(3, value);
    }

    [TestMethod]
    public void WithTag_TypedKey_DoesNotMutateOriginal()
    {
        var key      = new TagKey<string>("Region");
        var original = new Error("test");
        var enriched = original.WithTag(key, "EU");

        Assert.IsFalse(original.Has(key));
        Assert.IsTrue(enriched.Has(key));
    }

    // -------------------------------------------------------------------------
    // DomainTags constants — round-trip
    // -------------------------------------------------------------------------

    [TestMethod]
    public void WithTag_DomainTags_Entity_RoundTrips()
    {
        var error = new Error("test").WithTag(DomainTags.Entity, "Order");

        var found = error.TryGet(DomainTags.Entity, out var entity);

        Assert.IsTrue(found);
        Assert.AreEqual("Order", entity);
    }

    [TestMethod]
    public void WithTag_SystemTags_HttpStatus_RoundTrips()
    {
        var error = new Error("test").WithTag(SystemTags.HttpStatus, 503);

        var found = error.TryGet(SystemTags.HttpStatus, out var status);

        Assert.IsTrue(found);
        Assert.AreEqual(503, status);
    }
}
