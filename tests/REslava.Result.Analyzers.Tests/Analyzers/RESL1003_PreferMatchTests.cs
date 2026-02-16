using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.Analyzers.Analyzers;
using REslava.Result.Analyzers.Tests.Helpers;

namespace REslava.Result.Analyzers.Tests.Analyzers;

[TestClass]
public class RESL1003_PreferMatchTests
{
    [TestMethod]
    public async Task IfIsSuccess_WithValueAndErrors_Reports()
    {
        const string testCode = @"
using REslava.Result;

class Test
{
    void M()
    {
        var result = Result<int>.Ok(42);
        {|RESL1003:if|} (result.IsSuccess)
        {
            var x = result.Value;
        }
        else
        {
            var e = result.Errors;
        }
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<PreferMatchOverIfCheckAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task IfIsFailed_WithErrorsAndValue_Reports()
    {
        const string testCode = @"
using REslava.Result;

class Test
{
    void M()
    {
        var result = Result<int>.Ok(42);
        {|RESL1003:if|} (result.IsFailed)
        {
            var e = result.Errors;
        }
        else
        {
            var x = result.Value;
        }
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<PreferMatchOverIfCheckAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task IfNotIsSuccess_WithErrorsAndValue_Reports()
    {
        const string testCode = @"
using REslava.Result;

class Test
{
    void M()
    {
        var result = Result<int>.Ok(42);
        {|RESL1003:if|} (!result.IsSuccess)
        {
            var e = result.Errors;
        }
        else
        {
            var x = result.Value;
        }
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<PreferMatchOverIfCheckAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task IfNotIsFailed_WithValueAndErrors_Reports()
    {
        const string testCode = @"
using REslava.Result;

class Test
{
    void M()
    {
        var result = Result<int>.Ok(42);
        {|RESL1003:if|} (!result.IsFailed)
        {
            var x = result.Value;
        }
        else
        {
            var e = result.Errors;
        }
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<PreferMatchOverIfCheckAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task IfIsSuccess_OnlyValue_NoElse_NoReport()
    {
        const string testCode = @"
using REslava.Result;

class Test
{
    void M()
    {
        var result = Result<int>.Ok(42);
        if (result.IsSuccess)
        {
            var x = result.Value;
        }
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<PreferMatchOverIfCheckAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task IfIsSuccess_OnlyErrors_InElse_NoReport()
    {
        const string testCode = @"
using REslava.Result;

class Test
{
    void M()
    {
        var result = Result<int>.Ok(42);
        if (result.IsSuccess)
        {
            System.Console.WriteLine(""ok"");
        }
        else
        {
            var e = result.Errors;
        }
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<PreferMatchOverIfCheckAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task DifferentVariables_NoReport()
    {
        const string testCode = @"
using REslava.Result;

class Test
{
    void M()
    {
        var r1 = Result<int>.Ok(42);
        var r2 = Result<int>.Ok(99);
        if (r1.IsSuccess)
        {
            var x = r1.Value;
        }
        else
        {
            var e = r2.Errors;
        }
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<PreferMatchOverIfCheckAnalyzer>(testCode);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task NonResultType_NoReport()
    {
        const string testCode = @"
class Wrapper
{
    public bool IsSuccess => true;
    public int Value => 42;
    public string Errors => ""none"";
}

class Test
{
    void M()
    {
        var w = new Wrapper();
        if (w.IsSuccess)
        {
            var x = w.Value;
        }
        else
        {
            var e = w.Errors;
        }
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<PreferMatchOverIfCheckAnalyzer>(testCode);
        await test.RunAsync();
    }
}
