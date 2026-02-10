using Microsoft.CodeAnalysis.CSharp.Testing;
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

        public static Result<TValue> Ok(TValue value) => new Result<TValue> { _value = value };
        public static Result<TValue> Fail(string error) => new Result<TValue>();

        public TResult Match<TResult>(System.Func<TValue, TResult> onSuccess, System.Func<object, TResult> onFailure)
            => onSuccess(_value);
        public TValue GetValueOr(TValue defaultValue) => IsSuccess ? _value : defaultValue;
    }
}
";

    public static CSharpAnalyzerTest<UnsafeValueAccessAnalyzer, DefaultVerifier> CreateTest(string testCode)
    {
        var test = new CSharpAnalyzerTest<UnsafeValueAccessAnalyzer, DefaultVerifier>
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        // Add the stub as an additional source file in the test compilation
        test.TestState.Sources.Add(("ResultStub.cs", ResultStubSource));

        return test;
    }
}
