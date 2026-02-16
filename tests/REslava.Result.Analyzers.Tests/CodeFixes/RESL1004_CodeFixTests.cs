using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.Analyzers.Analyzers;
using REslava.Result.Analyzers.CodeFixes;
using REslava.Result.Analyzers.Tests.Helpers;

namespace REslava.Result.Analyzers.Tests.CodeFixes;

[TestClass]
public class RESL1004_CodeFixTests
{
    [TestMethod]
    public async Task AddAwait_ToUnawaitedCall_FixesCorrectly()
    {
        var testCode = @"
using REslava.Result;
using System.Threading.Tasks;

class C
{
    Task<Result<int>> GetAsync() => Task.FromResult(Result<int>.Ok(1));

    async Task M()
    {
        var result = {|RESL1004:GetAsync()|};
    }
}";

        var fixedCode = @"
using REslava.Result;
using System.Threading.Tasks;

class C
{
    Task<Result<int>> GetAsync() => Task.FromResult(Result<int>.Ok(1));

    async Task M()
    {
        var result = await GetAsync();
    }
}";

        var test = AnalyzerTestHelper.CreateCodeFixTest<AsyncResultNotAwaitedAnalyzer, AsyncResultNotAwaitedCodeFixProvider>(
            testCode, fixedCode);
        await test.RunAsync();
    }
}
