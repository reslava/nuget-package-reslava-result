using Microsoft.CodeAnalysis;

namespace REslava.Result.Analyzers
{
    /// <summary>
    /// Centralized diagnostic descriptors for all REslava.Result analyzers.
    /// </summary>
    internal static class Descriptors
    {
        public static readonly DiagnosticDescriptor RESL1001_UnsafeValueAccess = new(
            id: "RESL1001",
            title: "Unsafe Result<T>.Value access without IsSuccess check",
            messageFormat: "Access to '.Value' without checking 'IsSuccess' or 'IsFailed'. Use 'Match()', 'GetValueOr()', or check 'IsSuccess' first.",
            category: "REslava.Result.Safety",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Accessing .Value on a failed Result<T> throws InvalidOperationException. Always check IsSuccess before accessing .Value, or use Match()/GetValueOr() for safe access.");

        public static readonly DiagnosticDescriptor RESL1002_DiscardedResult = new(
            id: "RESL1002",
            title: "Result<T> return value is discarded",
            messageFormat: "Return value of type 'Result<T>' is discarded. Errors will be silently lost.",
            category: "REslava.Result.Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Ignoring a Result<T> return value means errors are silently swallowed. Assign the result to a variable and check IsSuccess, or use Match().");
        public static readonly DiagnosticDescriptor RESL1003_PreferMatch = new(
            id: "RESL1003",
            title: "Prefer Match() over if-check with Value/Errors access",
            messageFormat: "Consider using 'Match()' instead of checking 'IsSuccess' and accessing both '.Value' and '.Errors'.",
            category: "REslava.Result.Style",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "When both .Value and .Errors are accessed in complementary if/else branches, Match() is a safer and more concise alternative.");

        public static readonly DiagnosticDescriptor RESL2001_UnsafeOneOfAccess = new(
            id: "RESL2001",
            title: "Unsafe OneOf.AsT* access without IsT* check",
            messageFormat: "Access to '.{0}' without checking '.{1}'. Use 'Match()' or check '.{1}' first.",
            category: "REslava.Result.Safety",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Accessing .AsT* on a OneOf without checking the corresponding .IsT* throws InvalidOperationException. Use Match() for safe access.");
    }
}
