using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace REslava.Result.Tests.Reasons;

[TestClass]
public sealed class TagKeyTests
{
    // -------------------------------------------------------------------------
    // TagKey record equality
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TagKey_SameName_AreEqual()
    {
        var a = new TagKey<string>("Entity");
        var b = new TagKey<string>("Entity");

        Assert.AreEqual(a, b);
    }

    [TestMethod]
    public void TagKey_DifferentName_AreNotEqual()
    {
        var a = new TagKey<string>("Entity");
        var b = new TagKey<string>("Field");

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void TagKey_DifferentType_AreNotEqual()
    {
        TagKey a = new TagKey<string>("HttpStatusCode");
        TagKey b = new TagKey<int>("HttpStatusCode");

        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void TagKey_Name_RoundTrips()
    {
        var key = new TagKey<int>("Score");

        Assert.AreEqual("Score", key.Name);
    }

    // -------------------------------------------------------------------------
    // DomainTags constants
    // -------------------------------------------------------------------------

    [TestMethod]
    public void DomainTags_Entity_HasCorrectName()
        => Assert.AreEqual("Entity", DomainTags.Entity.Name);

    [TestMethod]
    public void DomainTags_EntityId_HasCorrectName()
        => Assert.AreEqual("EntityId", DomainTags.EntityId.Name);

    [TestMethod]
    public void DomainTags_Field_HasCorrectName()
        => Assert.AreEqual("Field", DomainTags.Field.Name);

    [TestMethod]
    public void DomainTags_Value_HasCorrectName()
        => Assert.AreEqual("Value", DomainTags.Value.Name);

    [TestMethod]
    public void DomainTags_Operation_HasCorrectName()
        => Assert.AreEqual("Operation", DomainTags.Operation.Name);

    // -------------------------------------------------------------------------
    // SystemTags constants
    // -------------------------------------------------------------------------

    [TestMethod]
    public void SystemTags_HttpStatus_HasCorrectName()
        => Assert.AreEqual("HttpStatusCode", SystemTags.HttpStatus.Name);

    [TestMethod]
    public void SystemTags_ErrorCode_HasCorrectName()
        => Assert.AreEqual("ErrorCode", SystemTags.ErrorCode.Name);

    [TestMethod]
    public void SystemTags_RetryAfter_HasCorrectName()
        => Assert.AreEqual("RetryAfter", SystemTags.RetryAfter.Name);

    [TestMethod]
    public void SystemTags_Service_HasCorrectName()
        => Assert.AreEqual("Service", SystemTags.Service.Name);

    // -------------------------------------------------------------------------
    // TagKey<T> identity — same constant reference
    // -------------------------------------------------------------------------

    [TestMethod]
    public void DomainTags_Entity_IsSameReference()
    {
        var a = DomainTags.Entity;
        var b = DomainTags.Entity;

        Assert.AreSame(a, b);
    }
}
