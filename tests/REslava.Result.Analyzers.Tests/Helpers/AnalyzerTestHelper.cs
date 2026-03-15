using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using REslava.Result.Analyzers.Analyzers;

namespace REslava.Result.Analyzers.Tests.Helpers;

internal static class AnalyzerTestHelper
{
    /// <summary>
    /// Minimal stub of Result&lt;T&gt; so the analyzer can resolve the type
    /// without depending on the actual REslava.Result assembly (avoids TFM mismatch).
    /// </summary>
    internal const string ResultStubSource = @"
namespace REslava.Result
{
    public class Result<TValue>
    {
        private TValue _value;
        public bool IsSuccess => !IsFailure;
        public bool IsFailure => false;
        public TValue Value => _value;
        public System.Collections.Generic.List<object> Errors => new System.Collections.Generic.List<object>();

        public static Result<TValue> Ok(TValue value) => new Result<TValue> { _value = value };
        public static Result<TValue> Fail(string error) => new Result<TValue>();

        public TResult Match<TResult>(System.Func<TValue, TResult> onSuccess, System.Func<object, TResult> onFailure)
            => onSuccess(_value);
        public TValue GetValueOr(TValue defaultValue) => IsSuccess ? _value : defaultValue;
    }
}
";

    /// <summary>
    /// Minimal stubs for OneOf types so RESL2001 can resolve them.
    /// </summary>
    internal const string OneOfStubSource = @"
namespace REslava.Result.AdvancedPatterns
{
    public readonly struct OneOf<T1, T2>
    {
        public bool IsT1 => false;
        public bool IsT2 => false;
        public T1 AsT1 => throw new System.InvalidOperationException();
        public T2 AsT2 => throw new System.InvalidOperationException();
        public static OneOf<T1, T2> FromT1(T1 value) => default;
        public static OneOf<T1, T2> FromT2(T2 value) => default;
        public TResult Match<TResult>(System.Func<T1, TResult> case1, System.Func<T2, TResult> case2) => default;
    }

    public readonly struct OneOf<T1, T2, T3>
    {
        public bool IsT1 => false;
        public bool IsT2 => false;
        public bool IsT3 => false;
        public T1 AsT1 => throw new System.InvalidOperationException();
        public T2 AsT2 => throw new System.InvalidOperationException();
        public T3 AsT3 => throw new System.InvalidOperationException();
        public static OneOf<T1, T2, T3> FromT1(T1 value) => default;
        public TResult Match<TResult>(System.Func<T1, TResult> c1, System.Func<T2, TResult> c2, System.Func<T3, TResult> c3) => default;
    }

    public readonly struct OneOf<T1, T2, T3, T4>
    {
        public bool IsT1 => false;
        public bool IsT2 => false;
        public bool IsT3 => false;
        public bool IsT4 => false;
        public T1 AsT1 => throw new System.InvalidOperationException();
        public T2 AsT2 => throw new System.InvalidOperationException();
        public T3 AsT3 => throw new System.InvalidOperationException();
        public T4 AsT4 => throw new System.InvalidOperationException();
        public static OneOf<T1, T2, T3, T4> FromT1(T1 value) => default;
        public TResult Match<TResult>(System.Func<T1, TResult> c1, System.Func<T2, TResult> c2, System.Func<T3, TResult> c3, System.Func<T4, TResult> c4) => default;
    }
}
";

    /// <summary>
    /// Minimal stubs for ErrorsOf types (arities 2–4) so RESL2002 can resolve them.
    /// Each arity exposes under-count Match overloads so test code with missing handlers still compiles.
    /// </summary>
    internal const string ErrorsOfStubSource = @"
namespace REslava.Result
{
    public interface IError { string Message { get; } }
}
namespace REslava.Result.AdvancedPatterns
{
    public sealed class ErrorsOf<T1, T2>
        where T1 : REslava.Result.IError
        where T2 : REslava.Result.IError
    {
        public static ErrorsOf<T1, T2> FromT1(T1 value) => null!;
        public static ErrorsOf<T1, T2> FromT2(T2 value) => null!;
        public TResult Match<TResult>(System.Func<T1, TResult> f1) => default!;
        public TResult Match<TResult>(System.Func<T1, TResult> f1, System.Func<T2, TResult> f2) => default!;
    }
    public sealed class ErrorsOf<T1, T2, T3>
        where T1 : REslava.Result.IError
        where T2 : REslava.Result.IError
        where T3 : REslava.Result.IError
    {
        public static ErrorsOf<T1, T2, T3> FromT1(T1 value) => null!;
        public TResult Match<TResult>(System.Func<T1, TResult> f1) => default!;
        public TResult Match<TResult>(System.Func<T1, TResult> f1, System.Func<T2, TResult> f2) => default!;
        public TResult Match<TResult>(System.Func<T1, TResult> f1, System.Func<T2, TResult> f2, System.Func<T3, TResult> f3) => default!;
    }
    public sealed class ErrorsOf<T1, T2, T3, T4>
        where T1 : REslava.Result.IError
        where T2 : REslava.Result.IError
        where T3 : REslava.Result.IError
        where T4 : REslava.Result.IError
    {
        public static ErrorsOf<T1, T2, T3, T4> FromT1(T1 value) => null!;
        public TResult Match<TResult>(System.Func<T1, TResult> f1) => default!;
        public TResult Match<TResult>(System.Func<T1, TResult> f1, System.Func<T2, TResult> f2) => default!;
        public TResult Match<TResult>(System.Func<T1, TResult> f1, System.Func<T2, TResult> f2, System.Func<T3, TResult> f3) => default!;
        public TResult Match<TResult>(System.Func<T1, TResult> f1, System.Func<T2, TResult> f2, System.Func<T3, TResult> f3, System.Func<T4, TResult> f4) => default!;
    }
}
";

    /// <summary>
    /// Extension methods stub for Result&lt;T&gt; so RESL1010 tests can reference
    /// TapOnFailure, Bind, Map, Ensure, GetValueOr, etc. without depending on the real assembly.
    /// Must be added alongside <see cref="ResultStubSource"/> in tests that need it.
    /// </summary>
    internal const string ResultFluentStubSource = @"
namespace REslava.Result
{
    public static class ResultFluentExtensions
    {
        public static Result<TValue> TapOnFailure<TValue>(
            this Result<TValue> result,
            System.Action<object> action) => result;

        public static Result<TOut> Bind<TValue, TOut>(
            this Result<TValue> result,
            System.Func<TValue, Result<TOut>> binder) => new Result<TOut>();

        public static Result<TOut> Map<TValue, TOut>(
            this Result<TValue> result,
            System.Func<TValue, TOut> mapper) => new Result<TOut>();

        public static Result<TValue> Ensure<TValue>(
            this Result<TValue> result,
            System.Func<TValue, bool> predicate,
            string errorMessage) => result;
    }
}
";

    /// <summary>
    /// Minimal stubs for Error and domain-specific error types so RESL1005 can resolve them.
    /// </summary>
    internal const string ErrorStubSource = @"
namespace REslava.Result
{
    public interface IError { }
    public class Error : IError
    {
        public Error(string message) { }
        public Error(string message, string propertyName) { }
    }
    public class NotFoundError : IError
    {
        public NotFoundError(string message) { }
    }
    public class ValidationError : IError
    {
        public ValidationError(string message) { }
        public ValidationError(string message, string fieldName) { }
    }
    public class ConflictError : IError
    {
        public ConflictError(string message) { }
    }
    public class UnauthorizedError : IError
    {
        public UnauthorizedError(string message) { }
    }
    public class ForbiddenError : IError
    {
        public ForbiddenError(string message) { }
    }
}
";

    /// <summary>
    /// Minimal stubs for <c>Result&lt;T, TError&gt;</c> (2-arg) and <c>[DomainBoundary]</c>
    /// so RESL1030 tests can resolve the types without depending on the real assembly.
    /// Must be added alongside <see cref="ResultStubSource"/> in RESL1030 tests.
    /// </summary>
    internal const string DomainBoundaryStubSource = @"
namespace REslava.Result
{
    public class Result<TValue, TError>
    {
        public bool IsSuccess => true;
        public bool IsFailure => false;
        public TValue Value => default!;
        public TError Error => default!;

        public static Result<TValue, TError> Ok(TValue value) => new Result<TValue, TError>();
        public static Result<TValue, TError> Fail(TError error) => new Result<TValue, TError>();

        public Result<TValue, TError2> MapError<TError2>(System.Func<TError, TError2> mapper)
            => new Result<TValue, TError2>();
    }

    [System.AttributeUsage(System.AttributeTargets.Method | System.AttributeTargets.Constructor, Inherited = false)]
    public sealed class DomainBoundaryAttribute : System.Attribute
    {
        public string? Layer { get; }
        public DomainBoundaryAttribute() { }
        public DomainBoundaryAttribute(string layer) { Layer = layer; }
    }
}
";

    /// <summary>
    /// Creates a test for UnsafeValueAccessAnalyzer (backward compat for existing tests).
    /// </summary>
    public static CSharpAnalyzerTest<UnsafeValueAccessAnalyzer, DefaultVerifier> CreateTest(string testCode)
    {
        var test = new CSharpAnalyzerTest<UnsafeValueAccessAnalyzer, DefaultVerifier>
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        test.TestState.Sources.Add(("ResultStub.cs", ResultStubSource));

        return test;
    }

    /// <summary>
    /// Creates a generic analyzer test with optional additional stub sources.
    /// </summary>
    public static CSharpAnalyzerTest<TAnalyzer, DefaultVerifier> CreateAnalyzerTest<TAnalyzer>(
        string testCode,
        params string[] additionalStubs) where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        test.TestState.Sources.Add(("ResultStub.cs", ResultStubSource));

        for (int i = 0; i < additionalStubs.Length; i++)
        {
            test.TestState.Sources.Add(($"Stub{i}.cs", additionalStubs[i]));
        }

        return test;
    }

    /// <summary>
    /// Creates a code fix test with optional additional stub sources.
    /// </summary>
    public static CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier> CreateCodeFixTest<TAnalyzer, TCodeFix>(
        string testCode,
        string fixedCode,
        int codeFixIndex = 0,
        params string[] additionalStubs)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
        {
            TestCode = testCode,
            FixedCode = fixedCode,
            CodeActionIndex = codeFixIndex,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        test.TestState.Sources.Add(("ResultStub.cs", ResultStubSource));
        test.FixedState.Sources.Add(("ResultStub.cs", ResultStubSource));

        for (int i = 0; i < additionalStubs.Length; i++)
        {
            test.TestState.Sources.Add(($"Stub{i}.cs", additionalStubs[i]));
            test.FixedState.Sources.Add(($"Stub{i}.cs", additionalStubs[i]));
        }

        return test;
    }
}
