using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.Analyzers.Analyzers;
using REslava.Result.Analyzers.Tests.Helpers;

namespace REslava.Result.Analyzers.Tests.Analyzers;

[TestClass]
public class RESL1004_AsyncResultNotAwaitedTests
{
    private static CSharpAnalyzerTest<AsyncResultNotAwaitedAnalyzer, DefaultVerifier> CreateTest(string testCode)
    {
        return AnalyzerTestHelper.CreateAnalyzerTest<AsyncResultNotAwaitedAnalyzer>(testCode);
    }

    [TestMethod]
    public async Task UnawaitedTaskResult_InAsyncMethod_Reports()
    {
        var test = CreateTest(@"
using REslava.Result;
using System.Threading.Tasks;

class C
{
    Task<Result<int>> GetAsync() => Task.FromResult(Result<int>.Ok(1));

    async Task M()
    {
        var result = {|RESL1004:GetAsync()|};
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task UnawaitedTaskResult_Assignment_InAsyncMethod_Reports()
    {
        var test = CreateTest(@"
using REslava.Result;
using System.Threading.Tasks;

class C
{
    Task<Result<int>> GetAsync() => Task.FromResult(Result<int>.Ok(1));

    async Task M()
    {
        object result;
        result = {|RESL1004:GetAsync()|};
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task AwaitedTaskResult_NoReport()
    {
        var test = CreateTest(@"
using REslava.Result;
using System.Threading.Tasks;

class C
{
    Task<Result<int>> GetAsync() => Task.FromResult(Result<int>.Ok(1));

    async Task M()
    {
        var result = await GetAsync();
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task SyncResult_NoReport()
    {
        var test = CreateTest(@"
using REslava.Result;
using System.Threading.Tasks;

class C
{
    Result<int> GetSync() => Result<int>.Ok(1);

    async Task M()
    {
        var result = GetSync();
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ExplicitTaskType_NoReport()
    {
        var test = CreateTest(@"
using REslava.Result;
using System.Threading.Tasks;

class C
{
    Task<Result<int>> GetAsync() => Task.FromResult(Result<int>.Ok(1));

    async Task M()
    {
        Task<Result<int>> task = GetAsync();
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task NonAsyncMethod_NoReport()
    {
        var test = CreateTest(@"
using REslava.Result;
using System.Threading.Tasks;

class C
{
    Task<Result<int>> GetAsync() => Task.FromResult(Result<int>.Ok(1));

    void M()
    {
        var task = GetAsync();
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ReturnedTaskResult_NoReport()
    {
        var test = CreateTest(@"
using REslava.Result;
using System.Threading.Tasks;

class C
{
    Task<Result<int>> GetAsync() => Task.FromResult(Result<int>.Ok(1));

    Task<Result<int>> M()
    {
        return GetAsync();
    }
}");
        await test.RunAsync();
    }
}
