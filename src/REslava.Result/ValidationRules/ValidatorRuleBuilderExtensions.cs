using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace REslava.Result;

/// <summary>
/// Extension methods on <see cref="ValidatorRuleBuilder{T}"/> providing a native DSL
/// for common validation rules (string, numeric, collection, null checks).
/// </summary>
/// <remarks>
/// Rules use <see cref="Expression{TDelegate}"/> selectors so the property name can be
/// inferred automatically for default error messages. The expression is compiled once at
/// build time — acceptable because builders are configured at startup, not in hot loops.
/// </remarks>
/// <example>
/// <code>
/// var validator = new ValidatorRuleBuilder&lt;CreateUserRequest&gt;()
///     .NotEmpty(u => u.Name)
///     .MaxLength(u => u.Name, 100)
///     .EmailAddress(u => u.Email)
///     .Range(u => u.Age, 18, 120)
///     .Build();
/// </code>
/// </example>
public static class ValidatorRuleBuilderExtensions
{
    private static readonly Regex _emailRegex = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string GetMemberName<T, TProp>(Expression<Func<T, TProp>> expr)
    {
        if (expr.Body is MemberExpression member)
            return member.Member.Name;
        if (expr.Body is UnaryExpression { Operand: MemberExpression m })
            return m.Member.Name; // handles boxing: x => (object)x.Age
        return "Value";
    }

    // ── String rules ──────────────────────────────────────────────────────────

    /// <summary>
    /// Validates that the selected string property is not <c>null</c> or empty (<c>""</c>).
    /// </summary>
    public static ValidatorRuleBuilder<T> NotEmpty<T>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, string?>> selector,
        string? errorMessage = null)
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "NotEmpty",
            errorMessage ?? $"'{name}' must not be empty.",
            v => !string.IsNullOrEmpty(v));
    }

    /// <summary>
    /// Validates that the selected string property is not <c>null</c>, empty, or whitespace.
    /// </summary>
    public static ValidatorRuleBuilder<T> NotWhiteSpace<T>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, string?>> selector,
        string? errorMessage = null)
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "NotWhiteSpace",
            errorMessage ?? $"'{name}' must not be empty or whitespace.",
            v => !string.IsNullOrWhiteSpace(v));
    }

    /// <summary>
    /// Validates that the selected string property has at least <paramref name="min"/> characters.
    /// <c>null</c> fails this rule.
    /// </summary>
    public static ValidatorRuleBuilder<T> MinLength<T>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, string?>> selector,
        int min,
        string? errorMessage = null)
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "MinLength",
            errorMessage ?? $"'{name}' must be at least {min} characters.",
            v => v != null && v.Length >= min);
    }

    /// <summary>
    /// Validates that the selected string property does not exceed <paramref name="max"/> characters.
    /// <c>null</c> passes this rule — combine with <see cref="NotEmpty{T}"/> if null should be rejected.
    /// </summary>
    public static ValidatorRuleBuilder<T> MaxLength<T>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, string?>> selector,
        int max,
        string? errorMessage = null)
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "MaxLength",
            errorMessage ?? $"'{name}' must not exceed {max} characters.",
            v => v == null || v.Length <= max);
    }

    /// <summary>
    /// Validates that the selected string property length is between <paramref name="min"/>
    /// and <paramref name="max"/> characters (inclusive). <c>null</c> fails this rule.
    /// </summary>
    public static ValidatorRuleBuilder<T> Length<T>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, string?>> selector,
        int min, int max,
        string? errorMessage = null)
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "Length",
            errorMessage ?? $"'{name}' must be between {min} and {max} characters.",
            v => v != null && v.Length >= min && v.Length <= max);
    }

    /// <summary>
    /// Validates that the selected string property is a valid email address.
    /// <c>null</c> fails this rule.
    /// </summary>
    public static ValidatorRuleBuilder<T> EmailAddress<T>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, string?>> selector,
        string? errorMessage = null)
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "EmailAddress",
            errorMessage ?? $"'{name}' must be a valid email address.",
            v => v != null && _emailRegex.IsMatch(v));
    }

    /// <summary>
    /// Validates that the selected string property matches the given regular expression
    /// <paramref name="pattern"/>. <c>null</c> fails this rule.
    /// </summary>
    public static ValidatorRuleBuilder<T> Matches<T>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, string?>> selector,
        string pattern,
        string? errorMessage = null)
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        var regex = new Regex(pattern);
        return builder.Rule(compiled, "Matches",
            errorMessage ?? $"'{name}' does not match the required pattern.",
            v => v != null && regex.IsMatch(v));
    }

    /// <summary>
    /// Validates that the selected string property starts with <paramref name="prefix"/>.
    /// The comparison is ordinal and case-sensitive by default.
    /// <c>null</c> fails this rule.
    /// </summary>
    public static ValidatorRuleBuilder<T> StartsWith<T>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, string?>> selector,
        string prefix,
        StringComparison comparison = StringComparison.Ordinal,
        string? errorMessage = null)
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "StartsWith",
            errorMessage ?? $"'{name}' must start with '{prefix}'.",
            v => v != null && v.StartsWith(prefix, comparison));
    }

    /// <summary>
    /// Validates that the selected string property ends with <paramref name="suffix"/>.
    /// The comparison is ordinal and case-sensitive by default.
    /// <c>null</c> fails this rule.
    /// </summary>
    public static ValidatorRuleBuilder<T> EndsWith<T>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, string?>> selector,
        string suffix,
        StringComparison comparison = StringComparison.Ordinal,
        string? errorMessage = null)
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "EndsWith",
            errorMessage ?? $"'{name}' must end with '{suffix}'.",
            v => v != null && v.EndsWith(suffix, comparison));
    }

    /// <summary>
    /// Validates that the selected string property contains <paramref name="value"/>.
    /// The comparison is ordinal and case-sensitive by default.
    /// <c>null</c> fails this rule.
    /// </summary>
    public static ValidatorRuleBuilder<T> Contains<T>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, string?>> selector,
        string value,
        StringComparison comparison = StringComparison.Ordinal,
        string? errorMessage = null)
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "Contains",
            errorMessage ?? $"'{name}' must contain '{value}'.",
            v => v != null && v.Contains(value, comparison));
    }

    // ── Numeric rules ─────────────────────────────────────────────────────────

    /// <summary>
    /// Validates that the selected comparable property is strictly greater than <paramref name="min"/>.
    /// Works for <c>int</c>, <c>long</c>, <c>double</c>, <c>decimal</c>, and any <see cref="IComparable{T}"/>.
    /// </summary>
    public static ValidatorRuleBuilder<T> GreaterThan<T, TNum>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, TNum>> selector,
        TNum min,
        string? errorMessage = null)
        where TNum : IComparable<TNum>
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "GreaterThan",
            errorMessage ?? $"'{name}' must be greater than {min}.",
            v => v.CompareTo(min) > 0);
    }

    /// <summary>
    /// Validates that the selected comparable property is strictly less than <paramref name="max"/>.
    /// Works for <c>int</c>, <c>long</c>, <c>double</c>, <c>decimal</c>, and any <see cref="IComparable{T}"/>.
    /// </summary>
    public static ValidatorRuleBuilder<T> LessThan<T, TNum>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, TNum>> selector,
        TNum max,
        string? errorMessage = null)
        where TNum : IComparable<TNum>
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "LessThan",
            errorMessage ?? $"'{name}' must be less than {max}.",
            v => v.CompareTo(max) < 0);
    }

    /// <summary>
    /// Validates that the selected comparable property is between <paramref name="min"/>
    /// and <paramref name="max"/> (inclusive).
    /// Works for <c>int</c>, <c>long</c>, <c>double</c>, <c>decimal</c>, and any <see cref="IComparable{T}"/>.
    /// </summary>
    public static ValidatorRuleBuilder<T> Range<T, TNum>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, TNum>> selector,
        TNum min, TNum max,
        string? errorMessage = null)
        where TNum : IComparable<TNum>
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "Range",
            errorMessage ?? $"'{name}' must be between {min} and {max}.",
            v => v.CompareTo(min) >= 0 && v.CompareTo(max) <= 0);
    }

    /// <summary>
    /// Validates that the selected comparable property is strictly positive (greater than zero).
    /// Works for <c>int</c>, <c>long</c>, <c>double</c>, <c>decimal</c>, and any <see cref="IComparable{T}"/>
    /// whose zero value is <c>default(TNum)</c>.
    /// </summary>
    public static ValidatorRuleBuilder<T> Positive<T, TNum>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, TNum>> selector,
        string? errorMessage = null)
        where TNum : struct, IComparable<TNum>
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        var zero = default(TNum);
        return builder.Rule(compiled, "Positive",
            errorMessage ?? $"'{name}' must be positive.",
            v => v.CompareTo(zero) > 0);
    }

    /// <summary>
    /// Validates that the selected comparable property is zero or positive (non-negative).
    /// Works for <c>int</c>, <c>long</c>, <c>double</c>, <c>decimal</c>, and any <see cref="IComparable{T}"/>
    /// whose zero value is <c>default(TNum)</c>.
    /// </summary>
    public static ValidatorRuleBuilder<T> NonNegative<T, TNum>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, TNum>> selector,
        string? errorMessage = null)
        where TNum : struct, IComparable<TNum>
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        var zero = default(TNum);
        return builder.Rule(compiled, "NonNegative",
            errorMessage ?? $"'{name}' must be zero or positive.",
            v => v.CompareTo(zero) >= 0);
    }

    // ── Collection rules ──────────────────────────────────────────────────────

    /// <summary>
    /// Validates that the selected collection property is not null or empty.
    /// </summary>
    public static ValidatorRuleBuilder<T> NotEmpty<T, TItem>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, IEnumerable<TItem>?>> selector,
        string? errorMessage = null)
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "NotEmpty",
            errorMessage ?? $"'{name}' must not be empty.",
            v => v != null && v.Any());
    }

    /// <summary>
    /// Validates that the selected collection property has at least <paramref name="min"/> items.
    /// <c>null</c> fails this rule.
    /// </summary>
    public static ValidatorRuleBuilder<T> MinCount<T, TItem>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, IEnumerable<TItem>?>> selector,
        int min,
        string? errorMessage = null)
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "MinCount",
            errorMessage ?? $"'{name}' must have at least {min} items.",
            v => v != null && v.Count() >= min);
    }

    /// <summary>
    /// Validates that the selected collection property has at most <paramref name="max"/> items.
    /// <c>null</c> passes this rule — combine with <see cref="NotEmpty{T,TItem}"/> if null should be rejected.
    /// </summary>
    public static ValidatorRuleBuilder<T> MaxCount<T, TItem>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, IEnumerable<TItem>?>> selector,
        int max,
        string? errorMessage = null)
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "MaxCount",
            errorMessage ?? $"'{name}' must not have more than {max} items.",
            v => v == null || v.Count() <= max);
    }

    // ── Reference type rules ──────────────────────────────────────────────────

    /// <summary>
    /// Validates that the selected reference-type property is not null.
    /// </summary>
    public static ValidatorRuleBuilder<T> NotNull<T, TProp>(
        this ValidatorRuleBuilder<T> builder,
        Expression<Func<T, TProp?>> selector,
        string? errorMessage = null)
        where TProp : class
    {
        var name = GetMemberName(selector);
        var compiled = selector.Compile();
        return builder.Rule(compiled, "NotNull",
            errorMessage ?? $"'{name}' must not be null.",
            v => v != null);
    }
}
