using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.Analyzers.Analyzers;
using REslava.Result.Analyzers.Tests.Helpers;

namespace REslava.Result.Analyzers.Tests.Analyzers;

[TestClass]
public class RESL2002_ExhaustiveMatchTests
{
    // ── ErrorsOf<T1,T2> — arity 2 ────────────────────────────────────────────

    [TestMethod]
    public async Task ErrorsOf2_Match_ExactArity_NoReport()
    {
        const string testCode = @"
using REslava.Result;
using REslava.Result.AdvancedPatterns;

class E1 : IError { public string Message => """"; }
class E2 : IError { public string Message => """"; }

class Test
{
    void M()
    {
        var errors = ErrorsOf<E1, E2>.FromT1(new E1());
        var result = errors.Match(e => e.Message, e => e.Message);
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ExhaustiveMatchAnalyzer>(testCode, AnalyzerTestHelper.ErrorsOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ErrorsOf2_Match_Missing1Handler_Reports()
    {
        const string testCode = @"
using REslava.Result;
using REslava.Result.AdvancedPatterns;

class E1 : IError { public string Message => """"; }
class E2 : IError { public string Message => """"; }

class Test
{
    void M()
    {
        var errors = ErrorsOf<E1, E2>.FromT1(new E1());
        var result = errors.{|RESL2002:Match|}(e => e.Message);
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ExhaustiveMatchAnalyzer>(testCode, AnalyzerTestHelper.ErrorsOfStubSource);
        await test.RunAsync();
    }

    // ── ErrorsOf<T1,T2,T3> — arity 3 ─────────────────────────────────────────

    [TestMethod]
    public async Task ErrorsOf3_Match_ExactArity_NoReport()
    {
        const string testCode = @"
using REslava.Result;
using REslava.Result.AdvancedPatterns;

class E1 : IError { public string Message => """"; }
class E2 : IError { public string Message => """"; }
class E3 : IError { public string Message => """"; }

class Test
{
    void M()
    {
        var errors = ErrorsOf<E1, E2, E3>.FromT1(new E1());
        var result = errors.Match(e => e.Message, e => e.Message, e => e.Message);
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ExhaustiveMatchAnalyzer>(testCode, AnalyzerTestHelper.ErrorsOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ErrorsOf3_Match_Missing1Handler_Reports()
    {
        const string testCode = @"
using REslava.Result;
using REslava.Result.AdvancedPatterns;

class E1 : IError { public string Message => """"; }
class E2 : IError { public string Message => """"; }
class E3 : IError { public string Message => """"; }

class Test
{
    void M()
    {
        var errors = ErrorsOf<E1, E2, E3>.FromT1(new E1());
        var result = errors.{|RESL2002:Match|}(e => e.Message, e => e.Message);
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ExhaustiveMatchAnalyzer>(testCode, AnalyzerTestHelper.ErrorsOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ErrorsOf3_Match_Missing2Handlers_Reports()
    {
        const string testCode = @"
using REslava.Result;
using REslava.Result.AdvancedPatterns;

class E1 : IError { public string Message => """"; }
class E2 : IError { public string Message => """"; }
class E3 : IError { public string Message => """"; }

class Test
{
    void M()
    {
        var errors = ErrorsOf<E1, E2, E3>.FromT1(new E1());
        var result = errors.{|RESL2002:Match|}(e => e.Message);
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ExhaustiveMatchAnalyzer>(testCode, AnalyzerTestHelper.ErrorsOfStubSource);
        await test.RunAsync();
    }

    // ── ErrorsOf<T1,T2,T3,T4> — arity 4 ──────────────────────────────────────

    [TestMethod]
    public async Task ErrorsOf4_Match_ExactArity_NoReport()
    {
        const string testCode = @"
using REslava.Result;
using REslava.Result.AdvancedPatterns;

class E1 : IError { public string Message => """"; }
class E2 : IError { public string Message => """"; }
class E3 : IError { public string Message => """"; }
class E4 : IError { public string Message => """"; }

class Test
{
    void M()
    {
        var errors = ErrorsOf<E1, E2, E3, E4>.FromT1(new E1());
        var result = errors.Match(e => e.Message, e => e.Message, e => e.Message, e => e.Message);
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ExhaustiveMatchAnalyzer>(testCode, AnalyzerTestHelper.ErrorsOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ErrorsOf4_Match_Missing1Handler_Reports()
    {
        const string testCode = @"
using REslava.Result;
using REslava.Result.AdvancedPatterns;

class E1 : IError { public string Message => """"; }
class E2 : IError { public string Message => """"; }
class E3 : IError { public string Message => """"; }
class E4 : IError { public string Message => """"; }

class Test
{
    void M()
    {
        var errors = ErrorsOf<E1, E2, E3, E4>.FromT1(new E1());
        var result = errors.{|RESL2002:Match|}(e => e.Message, e => e.Message, e => e.Message);
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ExhaustiveMatchAnalyzer>(testCode, AnalyzerTestHelper.ErrorsOfStubSource);
        await test.RunAsync();
    }

    // ── No false positives ────────────────────────────────────────────────────

    [TestMethod]
    public async Task NonREslavaMatchMethod_NoReport()
    {
        const string testCode = @"
class FakeErrorsOf<T1, T2>
{
    public TResult Match<TResult>(System.Func<T1, TResult> f1) => default!;
    public static FakeErrorsOf<T1, T2> Create() => null!;
}

class Test
{
    void M()
    {
        var fake = FakeErrorsOf<string, int>.Create();
        var result = fake.Match(s => s.Length);
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ExhaustiveMatchAnalyzer>(testCode, AnalyzerTestHelper.ErrorsOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task OneOf2_Match_Missing1Handler_NoReport()
    {
        const string testCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int>.FromT1(""hello"");
        // OneOf is not ErrorsOf — RESL2002 must NOT fire
        var result = oneOf.Match(s => s.Length, i => i);
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ExhaustiveMatchAnalyzer>(testCode, AnalyzerTestHelper.OneOfStubSource, AnalyzerTestHelper.ErrorsOfStubSource);
        await test.RunAsync();
    }
}
