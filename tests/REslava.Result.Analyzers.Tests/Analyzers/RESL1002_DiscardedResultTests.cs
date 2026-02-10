using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.Analyzers.Analyzers;
using REslava.Result.Analyzers.Tests.Helpers;

namespace REslava.Result.Analyzers.Tests.Analyzers;

[TestClass]
public class RESL1002_DiscardedResultTests
{
    private static CSharpAnalyzerTest<DiscardedResultAnalyzer, DefaultVerifier> CreateTest(string testCode)
    {
        var test = new CSharpAnalyzerTest<DiscardedResultAnalyzer, DefaultVerifier>
        {
            TestCode = testCode,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };
        test.TestState.Sources.Add(("ResultStub.cs", AnalyzerTestHelper.ResultStubSource));
        return test;
    }

    [TestMethod]
    public async Task DiscardedResult_Reports()
    {
        var test = CreateTest(@"
using REslava.Result;

class C
{
    Result<int> Save() => Result<int>.Ok(1);

    void M()
    {
        {|RESL1002:Save()|};
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task AssignedResult_NoReport()
    {
        var test = CreateTest(@"
using REslava.Result;

class C
{
    Result<int> Save() => Result<int>.Ok(1);

    void M()
    {
        var result = Save();
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ReturnedResult_NoReport()
    {
        var test = CreateTest(@"
using REslava.Result;

class C
{
    Result<int> Save() => Result<int>.Ok(1);

    Result<int> M()
    {
        return Save();
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task PassedAsArgument_NoReport()
    {
        var test = CreateTest(@"
using REslava.Result;

class C
{
    Result<int> Save() => Result<int>.Ok(1);
    void Process(Result<int> r) { }

    void M()
    {
        Process(Save());
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task DiscardedAsyncResult_Reports()
    {
        var test = CreateTest(@"
using REslava.Result;
using System.Threading.Tasks;

class C
{
    Task<Result<int>> SaveAsync() => Task.FromResult(Result<int>.Ok(1));

    async void M()
    {
        {|RESL1002:await SaveAsync()|};
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task AssignedAsyncResult_NoReport()
    {
        var test = CreateTest(@"
using REslava.Result;
using System.Threading.Tasks;

class C
{
    Task<Result<int>> SaveAsync() => Task.FromResult(Result<int>.Ok(1));

    async void M()
    {
        var result = await SaveAsync();
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task VoidMethod_NoReport()
    {
        var test = CreateTest(@"
class C
{
    void DoWork() { }

    void M()
    {
        DoWork();
    }
}");
        await test.RunAsync();
    }

    [TestMethod]
    public async Task NonResultReturn_NoReport()
    {
        var test = CreateTest(@"
class C
{
    int Calculate() => 42;

    void M()
    {
        Calculate();
    }
}");
        await test.RunAsync();
    }
}
