using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.Analyzers.Tests.Helpers;

namespace REslava.Result.Analyzers.Tests.Analyzers;

[TestClass]
public class RESL1001_UnsafeValueAccessTests
{
    [TestMethod]
    public async Task DirectValueAccess_WithoutGuard_Reports()
    {
        var test = AnalyzerTestHelper.CreateTest(@"
using REslava.Result;

class C
{
    void M()
    {
        var result = Result<int>.Ok(42);
        var x = result.{|RESL1001:Value|};
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ValueAccess_InsideIsSuccessGuard_NoReport()
    {
        var test = AnalyzerTestHelper.CreateTest(@"
using REslava.Result;

class C
{
    void M()
    {
        var result = Result<int>.Ok(42);
        if (result.IsSuccess)
        {
            var x = result.Value;
        }
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ValueAccess_InsideNotIsFailedGuard_NoReport()
    {
        var test = AnalyzerTestHelper.CreateTest(@"
using REslava.Result;

class C
{
    void M()
    {
        var result = Result<int>.Ok(42);
        if (!result.IsFailed)
        {
            var x = result.Value;
        }
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ValueAccess_InElseBranchOfIsFailed_NoReport()
    {
        var test = AnalyzerTestHelper.CreateTest(@"
using REslava.Result;

class C
{
    void M()
    {
        var result = Result<int>.Ok(42);
        if (result.IsFailed)
        {
            return;
        }
        else
        {
            var x = result.Value;
        }
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ValueAccess_AfterEarlyReturnOnIsFailed_NoReport()
    {
        var test = AnalyzerTestHelper.CreateTest(@"
using REslava.Result;

class C
{
    void M()
    {
        var result = Result<int>.Ok(42);
        if (result.IsFailed) return;
        var x = result.Value;
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ValueAccess_AfterEarlyThrowOnNotIsSuccess_NoReport()
    {
        var test = AnalyzerTestHelper.CreateTest(@"
using REslava.Result;
using System;

class C
{
    void M()
    {
        var result = Result<int>.Ok(42);
        if (!result.IsSuccess) throw new Exception();
        var x = result.Value;
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ValueAccess_InWrongBranch_Reports()
    {
        var test = AnalyzerTestHelper.CreateTest(@"
using REslava.Result;

class C
{
    void M()
    {
        var result = Result<int>.Ok(42);
        if (result.IsFailed)
        {
            var x = result.{|RESL1001:Value|};
        }
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ValueAccess_OnDifferentVariable_Reports()
    {
        var test = AnalyzerTestHelper.CreateTest(@"
using REslava.Result;

class C
{
    void M()
    {
        var result1 = Result<int>.Ok(42);
        var result2 = Result<int>.Ok(99);
        if (result1.IsSuccess)
        {
            var x = result2.{|RESL1001:Value|};
        }
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task UsingMatch_NoReport()
    {
        var test = AnalyzerTestHelper.CreateTest(@"
using REslava.Result;

class C
{
    void M()
    {
        var result = Result<int>.Ok(42);
        var x = result.Match(v => v, e => 0);
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task NonResultValue_NoReport()
    {
        var test = AnalyzerTestHelper.CreateTest(@"
class C
{
    void M()
    {
        var kvp = new System.Collections.Generic.KeyValuePair<string, int>(""key"", 42);
        var x = kvp.Value;
    }
}");
        await test.RunAsync();
    }
}
