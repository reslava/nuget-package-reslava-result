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
        public bool IsSuccess => !IsFailed;
        public bool IsFailed => false;
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
