using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result;

namespace REslava.Result.Tests.ValidationRules;

[TestClass]
public class ValidatorRuleBuilderExtensionsTests
{
    private class TestEntity
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Code { get; set; }
        public int Age { get; set; }
        public long Score { get; set; }
        public double Price { get; set; }
        public decimal Amount { get; set; }
        public List<string>? Tags { get; set; }
        public object? Reference { get; set; }
    }

    private static ValidationResult<TestEntity> Validate(
        ValidatorRuleBuilder<TestEntity> builder, TestEntity entity)
        => builder.Build().Validate(entity);

    // ── NotEmpty (string) ────────────────────────────────────────────────────

    [TestMethod]
    public void NotEmpty_ValidString_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotEmpty(e => e.Name),
            new TestEntity { Name = "Alice" });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void NotEmpty_EmptyString_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotEmpty(e => e.Name),
            new TestEntity { Name = "" });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void NotEmpty_NullString_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotEmpty(e => e.Name),
            new TestEntity { Name = null });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void NotEmpty_WhitespaceString_ShouldPass()
    {
        // NotEmpty only rejects null/"" — whitespace passes (use NotWhiteSpace for that)
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotEmpty(e => e.Name),
            new TestEntity { Name = "   " });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void NotEmpty_DefaultMessage_ContainsFieldName()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotEmpty(e => e.Name),
            new TestEntity { Name = "" });

        StringAssert.Contains(result.ValidationErrors[0].Message, "Name");
    }

    [TestMethod]
    public void NotEmpty_CustomMessage_UsesCustomMessage()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotEmpty(e => e.Name, "Custom error"),
            new TestEntity { Name = "" });

        Assert.AreEqual("Custom error", result.ValidationErrors[0].Message);
    }

    // ── NotWhiteSpace ────────────────────────────────────────────────────────

    [TestMethod]
    public void NotWhiteSpace_ValidString_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotWhiteSpace(e => e.Name),
            new TestEntity { Name = "Alice" });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void NotWhiteSpace_WhitespaceString_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotWhiteSpace(e => e.Name),
            new TestEntity { Name = "   " });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void NotWhiteSpace_EmptyString_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotWhiteSpace(e => e.Name),
            new TestEntity { Name = "" });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void NotWhiteSpace_NullString_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotWhiteSpace(e => e.Name),
            new TestEntity { Name = null });

        Assert.IsFalse(result.IsValid);
    }

    // ── MinLength ────────────────────────────────────────────────────────────

    [TestMethod]
    public void MinLength_AtMinimum_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().MinLength(e => e.Name, 3),
            new TestEntity { Name = "abc" });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void MinLength_AboveMinimum_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().MinLength(e => e.Name, 3),
            new TestEntity { Name = "abcdef" });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void MinLength_BelowMinimum_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().MinLength(e => e.Name, 5),
            new TestEntity { Name = "ab" });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void MinLength_NullString_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().MinLength(e => e.Name, 1),
            new TestEntity { Name = null });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void MinLength_DefaultMessage_ContainsMinAndFieldName()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().MinLength(e => e.Name, 5),
            new TestEntity { Name = "ab" });

        StringAssert.Contains(result.ValidationErrors[0].Message, "Name");
        StringAssert.Contains(result.ValidationErrors[0].Message, "5");
    }

    // ── MaxLength ────────────────────────────────────────────────────────────

    [TestMethod]
    public void MaxLength_AtMaximum_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().MaxLength(e => e.Name, 5),
            new TestEntity { Name = "abcde" });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void MaxLength_ExceedsMaximum_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().MaxLength(e => e.Name, 3),
            new TestEntity { Name = "abcdef" });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void MaxLength_NullString_ShouldPass()
    {
        // null passes MaxLength — use NotEmpty first if null should be rejected
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().MaxLength(e => e.Name, 5),
            new TestEntity { Name = null });

        Assert.IsTrue(result.IsValid);
    }

    // ── Length ───────────────────────────────────────────────────────────────

    [TestMethod]
    public void Length_WithinRange_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Length(e => e.Name, 2, 10),
            new TestEntity { Name = "Alice" });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Length_TooShort_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Length(e => e.Name, 5, 10),
            new TestEntity { Name = "ab" });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Length_TooLong_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Length(e => e.Name, 2, 4),
            new TestEntity { Name = "abcdefg" });

        Assert.IsFalse(result.IsValid);
    }

    // ── EmailAddress ─────────────────────────────────────────────────────────

    [TestMethod]
    public void EmailAddress_ValidEmail_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().EmailAddress(e => e.Email),
            new TestEntity { Email = "user@example.com" });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void EmailAddress_MissingAtSign_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().EmailAddress(e => e.Email),
            new TestEntity { Email = "userexample.com" });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void EmailAddress_NullValue_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().EmailAddress(e => e.Email),
            new TestEntity { Email = null });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void EmailAddress_DefaultMessage_ContainsFieldName()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().EmailAddress(e => e.Email),
            new TestEntity { Email = "bad" });

        StringAssert.Contains(result.ValidationErrors[0].Message, "Email");
    }

    // ── Matches ──────────────────────────────────────────────────────────────

    [TestMethod]
    public void Matches_PatternMatches_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Matches(e => e.Code, @"^\d{4}$"),
            new TestEntity { Code = "1234" });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Matches_PatternDoesNotMatch_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Matches(e => e.Code, @"^\d{4}$"),
            new TestEntity { Code = "abcd" });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Matches_NullValue_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Matches(e => e.Code, @"^\d+$"),
            new TestEntity { Code = null });

        Assert.IsFalse(result.IsValid);
    }

    // ── StartsWith ───────────────────────────────────────────────────────────

    [TestMethod]
    public void StartsWith_MatchingPrefix_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().StartsWith(e => e.Code, "SKU-"),
            new TestEntity { Code = "SKU-1234" });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void StartsWith_NonMatchingPrefix_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().StartsWith(e => e.Code, "SKU-"),
            new TestEntity { Code = "ITEM-1234" });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void StartsWith_NullValue_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().StartsWith(e => e.Code, "SKU-"),
            new TestEntity { Code = null });

        Assert.IsFalse(result.IsValid);
    }

    // ── EndsWith ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void EndsWith_MatchingSuffix_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().EndsWith(e => e.Code, ".pdf"),
            new TestEntity { Code = "document.pdf" });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void EndsWith_NonMatchingSuffix_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().EndsWith(e => e.Code, ".pdf"),
            new TestEntity { Code = "document.docx" });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void EndsWith_NullValue_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().EndsWith(e => e.Code, ".pdf"),
            new TestEntity { Code = null });

        Assert.IsFalse(result.IsValid);
    }

    // ── Contains (string) ────────────────────────────────────────────────────

    [TestMethod]
    public void Contains_ValuePresent_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Contains(e => e.Name, "@"),
            new TestEntity { Name = "user@domain" });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Contains_ValueAbsent_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Contains(e => e.Name, "@"),
            new TestEntity { Name = "userdomain" });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Contains_NullValue_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Contains(e => e.Name, "@"),
            new TestEntity { Name = null });

        Assert.IsFalse(result.IsValid);
    }

    // ── GreaterThan ──────────────────────────────────────────────────────────

    [TestMethod]
    public void GreaterThan_AboveMin_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().GreaterThan(e => e.Age, 17),
            new TestEntity { Age = 18 });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void GreaterThan_EqualToMin_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().GreaterThan(e => e.Age, 18),
            new TestEntity { Age = 18 });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void GreaterThan_BelowMin_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().GreaterThan(e => e.Age, 18),
            new TestEntity { Age = 10 });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void GreaterThan_WorksWithDecimal()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().GreaterThan(e => e.Amount, 0m),
            new TestEntity { Amount = 0.01m });

        Assert.IsTrue(result.IsValid);
    }

    // ── LessThan ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void LessThan_BelowMax_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().LessThan(e => e.Age, 120),
            new TestEntity { Age = 30 });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void LessThan_EqualToMax_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().LessThan(e => e.Age, 120),
            new TestEntity { Age = 120 });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void LessThan_AboveMax_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().LessThan(e => e.Age, 100),
            new TestEntity { Age = 200 });

        Assert.IsFalse(result.IsValid);
    }

    // ── Range ────────────────────────────────────────────────────────────────

    [TestMethod]
    public void Range_WithinRange_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Range(e => e.Age, 18, 120),
            new TestEntity { Age = 25 });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Range_AtLowerBound_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Range(e => e.Age, 18, 120),
            new TestEntity { Age = 18 });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Range_AtUpperBound_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Range(e => e.Age, 18, 120),
            new TestEntity { Age = 120 });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Range_BelowMin_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Range(e => e.Age, 18, 120),
            new TestEntity { Age = 17 });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Range_AboveMax_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Range(e => e.Age, 18, 120),
            new TestEntity { Age = 121 });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Range_WorksWithDouble()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Range(e => e.Price, 0.0, 999.99),
            new TestEntity { Price = 49.99 });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Range_DefaultMessage_ContainsMinMaxAndFieldName()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Range(e => e.Age, 18, 120),
            new TestEntity { Age = 5 });

        StringAssert.Contains(result.ValidationErrors[0].Message, "Age");
        StringAssert.Contains(result.ValidationErrors[0].Message, "18");
        StringAssert.Contains(result.ValidationErrors[0].Message, "120");
    }

    // ── Positive ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void Positive_PositiveInt_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Positive(e => e.Age),
            new TestEntity { Age = 1 });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Positive_ZeroInt_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Positive(e => e.Age),
            new TestEntity { Age = 0 });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Positive_NegativeInt_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Positive(e => e.Age),
            new TestEntity { Age = -5 });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Positive_WorksWithLong()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Positive(e => e.Score),
            new TestEntity { Score = 100L });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Positive_WorksWithDecimal()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().Positive(e => e.Amount),
            new TestEntity { Amount = -1m });

        Assert.IsFalse(result.IsValid);
    }

    // ── NonNegative ──────────────────────────────────────────────────────────

    [TestMethod]
    public void NonNegative_ZeroInt_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NonNegative(e => e.Age),
            new TestEntity { Age = 0 });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void NonNegative_PositiveInt_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NonNegative(e => e.Age),
            new TestEntity { Age = 5 });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void NonNegative_NegativeInt_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NonNegative(e => e.Age),
            new TestEntity { Age = -1 });

        Assert.IsFalse(result.IsValid);
    }

    // ── NotEmpty (collection) ────────────────────────────────────────────────

    [TestMethod]
    public void NotEmptyCollection_NonEmptyList_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotEmpty<TestEntity, string>(e => e.Tags),
            new TestEntity { Tags = new List<string> { "a" } });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void NotEmptyCollection_EmptyList_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotEmpty<TestEntity, string>(e => e.Tags),
            new TestEntity { Tags = new List<string>() });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void NotEmptyCollection_NullList_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotEmpty<TestEntity, string>(e => e.Tags),
            new TestEntity { Tags = null });

        Assert.IsFalse(result.IsValid);
    }

    // ── MinCount ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void MinCount_AtMinimum_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().MinCount<TestEntity, string>(e => e.Tags, 2),
            new TestEntity { Tags = new List<string> { "a", "b" } });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void MinCount_BelowMinimum_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().MinCount<TestEntity, string>(e => e.Tags, 3),
            new TestEntity { Tags = new List<string> { "a" } });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void MinCount_NullList_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().MinCount<TestEntity, string>(e => e.Tags, 1),
            new TestEntity { Tags = null });

        Assert.IsFalse(result.IsValid);
    }

    // ── MaxCount ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void MaxCount_BelowMaximum_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().MaxCount<TestEntity, string>(e => e.Tags, 5),
            new TestEntity { Tags = new List<string> { "a", "b" } });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void MaxCount_ExceedsMaximum_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().MaxCount<TestEntity, string>(e => e.Tags, 2),
            new TestEntity { Tags = new List<string> { "a", "b", "c" } });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void MaxCount_NullList_ShouldPass()
    {
        // null passes MaxCount — combine with NotEmpty if null should be rejected
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().MaxCount<TestEntity, string>(e => e.Tags, 5),
            new TestEntity { Tags = null });

        Assert.IsTrue(result.IsValid);
    }

    // ── NotNull ──────────────────────────────────────────────────────────────

    [TestMethod]
    public void NotNull_NonNullReference_ShouldPass()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotNull(e => e.Reference),
            new TestEntity { Reference = new object() });

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void NotNull_NullReference_ShouldFail()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotNull(e => e.Reference),
            new TestEntity { Reference = null });

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void NotNull_DefaultMessage_ContainsFieldName()
    {
        var result = Validate(
            new ValidatorRuleBuilder<TestEntity>().NotNull(e => e.Reference),
            new TestEntity { Reference = null });

        StringAssert.Contains(result.ValidationErrors[0].Message, "Reference");
    }

    // ── Fluent chaining ──────────────────────────────────────────────────────

    [TestMethod]
    public void FluentChaining_AllRulesPass_ShouldBeValid()
    {
        var entity = new TestEntity
        {
            Name = "Alice",
            Email = "alice@example.com",
            Age = 25,
            Tags = new List<string> { "admin" }
        };

        var result = new ValidatorRuleBuilder<TestEntity>()
            .NotEmpty(e => e.Name)
            .MaxLength(e => e.Name, 100)
            .EmailAddress(e => e.Email)
            .Range(e => e.Age, 18, 120)
            .NotEmpty<TestEntity, string>(e => e.Tags)
            .Build()
            .Validate(entity);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void FluentChaining_FirstRuleFails_ShouldBeInvalid()
    {
        var entity = new TestEntity
        {
            Name = "",            // fails NotEmpty
            Email = "alice@example.com",
            Age = 25
        };

        var result = new ValidatorRuleBuilder<TestEntity>()
            .NotEmpty(e => e.Name)
            .EmailAddress(e => e.Email)
            .Range(e => e.Age, 18, 120)
            .Build()
            .Validate(entity);

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.ValidationErrors[0].Message, "Name");
    }

    [TestMethod]
    public void FluentChaining_ReturnsSameBuilderInstance()
    {
        var builder = new ValidatorRuleBuilder<TestEntity>();
        var returned = builder.NotEmpty(e => e.Name);

        Assert.AreEqual(builder, returned);
    }
}
