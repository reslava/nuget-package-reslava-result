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
            messageFormat: "Access to '.Value' without checking 'IsSuccess' or 'IsFailure'. Use 'Match()', 'GetValueOr()', or check 'IsSuccess' first.",
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

        public static readonly DiagnosticDescriptor RESL1004_AsyncResultNotAwaited = new(
            id: "RESL1004",
            title: "Task<Result<T>> assigned without await",
            messageFormat: "'{0}' returns Task<Result<T>> but is not awaited. The result will be a Task, not the actual Result.",
            category: "REslava.Result.Safety",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In async methods, calls returning Task<Result<T>> must be awaited to get the actual Result. Without await, the variable holds a Task which may be implicitly converted, leading to unexpected behavior at runtime.");

        public static readonly DiagnosticDescriptor RESL2001_UnsafeOneOfAccess = new(
            id: "RESL2001",
            title: "Unsafe OneOf.AsT* access without IsT* check",
            messageFormat: "Access to '.{0}' without checking '.{1}'. Use 'Match()' or check '.{1}' first.",
            category: "REslava.Result.Safety",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Accessing .AsT* on a OneOf without checking the corresponding .IsT* throws InvalidOperationException. Use Match() for safe access.");

        public static readonly DiagnosticDescriptor RESL1006_BothValidateAttributes = new(
            id: "RESL1006",
            title: "[Validate] and [FluentValidate] cannot both be applied to the same type",
            messageFormat: "'{0}' has both [Validate] and [FluentValidate] applied. These attributes generate conflicting '.Validate()' extension methods. Remove one.",
            category: "REslava.Result.Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "[Validate] generates a .Validate() extension using DataAnnotations, while [FluentValidate] generates one using FluentValidation.IValidator<T>. Both cannot coexist on the same type because they produce a compile-time naming conflict.");

        public static readonly DiagnosticDescriptor RESL1009_TryCatchToResultTry = new(
            id: "RESL1009",
            title: "Replace try/catch with Result<T>.Try()",
            messageFormat: "This try/catch pattern can be replaced with 'Result<{0}>.Try(() => ...)'.",
            category: "REslava.Result.Migration",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Inside a method returning Result<T>, a try { return T } catch { return Fail(...) } " +
                         "block is equivalent to Result<T>.Try(() => ...). The factory removes boilerplate " +
                         "and makes the intent explicit.");

        public static readonly DiagnosticDescriptor RESL2002_ExhaustiveMatch = new(
            id: "RESL2002",
            title: "ErrorsOf.Match() is not exhaustive",
            messageFormat: "Match() on ErrorsOf<{0}> provides {1} handler(s) but the union has {2} type(s). Add the missing handler(s) to ensure exhaustive handling.",
            category: "REslava.Result.Safety",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "ErrorsOf<T1..Tn> is a discriminated union — every type must be handled. Providing fewer lambdas than type arguments leaves branches unhandled and causes InvalidOperationException at runtime.");

        public static readonly DiagnosticDescriptor RESL1010_UnhandledFailurePath = new(
            id: "RESL1010",
            title: "Result failure path may be unhandled",
            messageFormat: "'{0}' returns Result<T> but the failure path is not handled. Use Match(), TapOnFailure(), or check IsFailure to ensure errors are not silently lost.",
            category: "REslava.Result.Safety",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A local Result<T> variable has no failure-aware usage (no IsFailure/IsSuccess check, no Match, no TapOnFailure, and the result is not returned or passed through a pipeline). Errors will be silently swallowed.");

        public static readonly DiagnosticDescriptor RESL1005_SuggestDomainError = new(
            id: "RESL1005",
            title: "Consider using a domain-specific error type",
            messageFormat: "Consider using '{0}' instead of 'Error' — it carries HTTP status context and integrates automatically with 'ToIResult()'.",
            category: "REslava.Result.Usage",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "REslava.Result provides domain-specific error types (NotFoundError, ValidationError, ConflictError, UnauthorizedError, ForbiddenError) that set HttpStatusCode tags automatically. Using these instead of the generic Error class improves HTTP mapping accuracy and makes intent explicit.");

        public static readonly DiagnosticDescriptor RESL1021_MultiArgErrorConstructor = new(
            id: "RESL1021",
            title: "IError/IReason implementation has multi-argument public constructor",
            messageFormat: "'{0}' has a public constructor with 2+ required parameters. Use a single-string or parameterless constructor and expose additional parameters through a static factory method.",
            category: "REslava.Result.Design",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "IError/IReason implementations should only expose (), (string), or (string, Exception) constructors. Multi-parameter constructors prevent CallerMemberName capture, which is needed for diagnostic metadata. Add a static factory method instead and mark this constructor [Obsolete].");

        public static readonly DiagnosticDescriptor RESL1030_DomainBoundaryTypedErrorCrossing = new(
            id: "RESL1030",
            title: "Typed error crosses domain boundary without mapping",
            messageFormat: "'{0}' receives Result<T, {1}> directly. Call .MapError() before crossing the [DomainBoundary] to avoid leaking domain-specific error types across layers.",
            category: "REslava.Result.Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A method marked [DomainBoundary] received a Result<T, TError> with a domain-specific typed error. Translate the error surface with .MapError() before passing the result across architectural layers.");
    }
}
