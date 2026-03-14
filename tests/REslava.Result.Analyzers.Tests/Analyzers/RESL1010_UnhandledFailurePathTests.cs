using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.Analyzers.Analyzers;
using REslava.Result.Analyzers.Tests.Helpers;

namespace REslava.Result.Analyzers.Tests.Analyzers;

[TestClass]
public class RESL1010_UnhandledFailurePathTests
{
    // ── No diagnostic — failure is handled ───────────────────────────────────

    [TestMethod]
    public async Task Match_BothBranches_NoReport()
    {
        const string testCode = @"
using REslava.Result;
class Test
{
    void M()
    {
        var result = Result<string>.Ok(""hello"");
        var x = result.Match(v => v, e => ""err"");
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnhandledFailurePathAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task IsFailureCheck_NoReport()
    {
        const string testCode = @"
using REslava.Result;
class Test
{
    void M()
    {
        var result = Result<string>.Ok(""hello"");
        if (result.IsFailure)
            System.Console.Error.WriteLine(""failed"");
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnhandledFailurePathAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task IsSuccessCheck_NoReport()
    {
        // Phase 2: IsSuccess counts as "handled" even without else.
        // The IsSuccess-without-else false-negative is a known Phase 3 improvement.
        const string testCode = @"
using REslava.Result;
class Test
{
    void M()
    {
        var result = Result<string>.Ok(""hello"");
        if (result.IsSuccess)
            System.Console.WriteLine(result.Value);
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnhandledFailurePathAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task IsSuccessWithElse_NoReport()
    {
        const string testCode = @"
using REslava.Result;
class Test
{
    void M()
    {
        var result = Result<string>.Ok(""hello"");
        if (result.IsSuccess)
            System.Console.WriteLine(result.Value);
        else
            System.Console.Error.WriteLine(""failure"");
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnhandledFailurePathAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ReturnResult_NoReport()
    {
        const string testCode = @"
using REslava.Result;
class Test
{
    Result<string> M()
    {
        var result = Result<string>.Ok(""hello"");
        return result;
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnhandledFailurePathAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task TapOnFailure_NoReport()
    {
        const string testCode = @"
using REslava.Result;
class Test
{
    void M()
    {
        var result = Result<string>.Ok(""hello"");
        result.TapOnFailure(e => System.Console.Error.WriteLine(e));
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnhandledFailurePathAnalyzer>(
            testCode, AnalyzerTestHelper.ResultFluentStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task PipelineChaining_Bind_NoReport()
    {
        const string testCode = @"
using REslava.Result;
class Test
{
    void M()
    {
        var result = Result<string>.Ok(""hello"");
        var piped = result.Bind(v => Result<int>.Ok(v.Length));
        if (piped.IsSuccess) System.Console.WriteLine(piped.Value);
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnhandledFailurePathAnalyzer>(
            testCode, AnalyzerTestHelper.ResultFluentStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task GetValueOr_NoReport()
    {
        const string testCode = @"
using REslava.Result;
class Test
{
    void M()
    {
        var result = Result<string>.Ok(""hello"");
        var value = result.GetValueOr(""default"");
        System.Console.WriteLine(value);
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnhandledFailurePathAnalyzer>(testCode);
        await test.RunAsync();
    }

    // ── Diagnostic expected ───────────────────────────────────────────────────

    [TestMethod]
    public async Task NoHandling_OnlyUnrelatedCode_Reports()
    {
        const string testCode = @"
using REslava.Result;
class Test
{
    void M()
    {
        var {|RESL1010:result|} = Result<string>.Ok(""hello"");
        System.Console.WriteLine(""unrelated"");
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnhandledFailurePathAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task OnlyValueAccessed_Reports()
    {
        // .Value access alone does not handle the failure path
        // (RESL1001 covers the unsafe access; RESL1010 fires here independently)
        const string testCode = @"
using REslava.Result;
class Test
{
    void M()
    {
        var {|RESL1010:result|} = Result<string>.Ok(""hello"");
        System.Console.WriteLine(result.Value);
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnhandledFailurePathAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task OnlyToStringAccessed_Reports()
    {
        const string testCode = @"
using REslava.Result;
class Test
{
    void M()
    {
        var {|RESL1010:result|} = Result<string>.Ok(""hello"");
        System.Console.WriteLine(result.ToString());
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnhandledFailurePathAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task MultipleResults_EachEvaluatedIndependently()
    {
        // r1 is handled (IsFailure), r2 is not → only r2 gets RESL1010
        const string testCode = @"
using REslava.Result;
class Test
{
    void M()
    {
        var r1 = Result<string>.Ok(""a"");
        var {|RESL1010:r2|} = Result<int>.Ok(1);
        if (r1.IsFailure) System.Console.Error.WriteLine(""r1 failed"");
        System.Console.WriteLine(r2.Value);
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnhandledFailurePathAnalyzer>(testCode);
        await test.RunAsync();
    }

    // ── No false positive on non-Result types ─────────────────────────────────

    [TestMethod]
    public async Task NonResultType_NoReport()
    {
        const string testCode = @"
class Test
{
    void M()
    {
        var x = ""hello"";
        System.Console.WriteLine(x);
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnhandledFailurePathAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task NonGenericResult_NoReport()
    {
        // Non-generic Result (void result) is excluded from RESL1010 scope
        const string testCode = @"
namespace REslava.Result
{
    public class Result
    {
        public bool IsSuccess => true;
        public bool IsFailure => false;
        public static Result Ok() => new Result();
        public static Result Fail(string msg) => new Result();
    }
}
class Test
{
    void M()
    {
        var result = REslava.Result.Result.Ok();
        System.Console.WriteLine(""done"");
    }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnhandledFailurePathAnalyzer>(testCode);
        await test.RunAsync();
    }
}
