using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.Analyzers.Analyzers;
using REslava.Result.Analyzers.Tests.Helpers;

namespace REslava.Result.Analyzers.Tests.Analyzers;

[TestClass]
public class RESL1030_DomainBoundaryTests
{
    private static Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerTest<DomainBoundaryAnalyzer, DefaultVerifier>
        CreateTest(string testCode)
        => AnalyzerTestHelper.CreateAnalyzerTest<DomainBoundaryAnalyzer>(
            testCode,
            AnalyzerTestHelper.DomainBoundaryStubSource);

    // ── Positive cases ───────────────────────────────────────────────────────

    [TestMethod]
    public async Task PassingTypedResultToBoundaryMethod_ReportsRESL1030()
    {
        var test = CreateTest(@"
using REslava.Result;
class DomainError { }
class C
{
    [DomainBoundary]
    void ApplicationEntry(Result<int, DomainError> result) { }

    void Caller()
    {
        var r = Result<int, DomainError>.Ok(1);
        ApplicationEntry({|RESL1030:r|});
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task PassingTypedResultWithLayerLabelToBoundaryMethod_ReportsRESL1030()
    {
        var test = CreateTest(@"
using REslava.Result;
class DomainError { }
class C
{
    [DomainBoundary(""Application"")]
    void ApplicationEntry(Result<string, DomainError> result) { }

    void Caller()
    {
        var r = Result<string, DomainError>.Ok(""ok"");
        ApplicationEntry({|RESL1030:r|});
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task PassingInlineTypedResultToBoundaryMethod_ReportsRESL1030()
    {
        var test = CreateTest(@"
using REslava.Result;
class DomainError { }
class C
{
    [DomainBoundary]
    void Process(Result<int, DomainError> result) { }

    void Caller() => Process({|RESL1030:Result<int, DomainError>.Ok(42)|});
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task MultipleTypedResultArguments_ReportsBothOccurrences()
    {
        var test = CreateTest(@"
using REslava.Result;
class DomainError { }
class C
{
    [DomainBoundary]
    void Process(Result<int, DomainError> r1, Result<string, DomainError> r2) { }

    void Caller()
    {
        Process({|RESL1030:Result<int, DomainError>.Ok(1)|}, {|RESL1030:Result<string, DomainError>.Ok(""ok"")|});
    }
}");
        await test.RunAsync();
    }

    // ── Negative cases ───────────────────────────────────────────────────────

    [TestMethod]
    public async Task PassingSingleArgResultToNonBoundaryMethod_NoReport()
    {
        var test = CreateTest(@"
using REslava.Result;
class C
{
    void Process(Result<int> result) { }

    void Caller()
    {
        var r = Result<int>.Ok(1);
        Process(r);
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task PassingTypedResultToNonBoundaryMethod_NoReport()
    {
        var test = CreateTest(@"
using REslava.Result;
class DomainError { }
class C
{
    void Process(Result<int, DomainError> result) { }

    void Caller()
    {
        var r = Result<int, DomainError>.Ok(1);
        Process(r);
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task PassingPlainIntToBoundaryMethod_NoReport()
    {
        var test = CreateTest(@"
using REslava.Result;
class C
{
    [DomainBoundary]
    void Process(int value) { }

    void Caller() => Process(42);
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task PassingSingleArgResultToBoundaryMethod_NoReport()
    {
        var test = CreateTest(@"
using REslava.Result;
class C
{
    [DomainBoundary]
    void Process(Result<int> result) { }

    void Caller()
    {
        var r = Result<int>.Ok(1);
        Process(r);
    }
}");
        await test.RunAsync();
    }
}
