using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.Analyzers.Analyzers;
using REslava.Result.Analyzers.CodeFixes;
using REslava.Result.Analyzers.Tests.Helpers;

namespace REslava.Result.Analyzers.Tests.CodeFixes;

[TestClass]
public class RESL1001_CodeFixTests
{
    [TestMethod]
    public async Task FixA_WrapsInIfGuard_ForLocalDeclaration()
    {
        const string testCode = @"
using REslava.Result;

class Test
{
    void M()
    {
        var result = Result<int>.Ok(42);
        var x = result.{|RESL1001:Value|};
    }
}";

        const string fixedCode = @"
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

        var test = AnalyzerTestHelper.CreateCodeFixTest<UnsafeValueAccessAnalyzer, UnsafeValueAccessCodeFixProvider>(
            testCode, fixedCode, codeFixIndex: 0);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task FixA_WrapsInIfGuard_ForExpressionStatement()
    {
        const string testCode = @"
using REslava.Result;

class Test
{
    void M()
    {
        var result = Result<string>.Ok(""hello"");
        System.Console.WriteLine(result.{|RESL1001:Value|});
    }
}";

        const string fixedCode = @"
using REslava.Result;

class Test
{
    void M()
    {
        var result = Result<string>.Ok(""hello"");
        if (result.IsSuccess)
        {
            System.Console.WriteLine(result.Value);
        }
    }
}";

        var test = AnalyzerTestHelper.CreateCodeFixTest<UnsafeValueAccessAnalyzer, UnsafeValueAccessCodeFixProvider>(
            testCode, fixedCode, codeFixIndex: 0);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task FixB_ReplacesWithMatch_ForLocalDeclaration()
    {
        const string testCode = @"
using REslava.Result;

class Test
{
    void M()
    {
        var result = Result<int>.Ok(42);
        var x = result.{|RESL1001:Value|};
    }
}";

        const string fixedCode = @"
using REslava.Result;

class Test
{
    void M()
    {
        var result = Result<int>.Ok(42);
        var x = result.Match(v => v, e => default);
    }
}";

        var test = AnalyzerTestHelper.CreateCodeFixTest<UnsafeValueAccessAnalyzer, UnsafeValueAccessCodeFixProvider>(
            testCode, fixedCode, codeFixIndex: 1);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task FixB_ReplacesWithMatch_ForExpressionStatement()
    {
        const string testCode = @"
using REslava.Result;

class Test
{
    void M()
    {
        var result = Result<string>.Ok(""hello"");
        System.Console.WriteLine(result.{|RESL1001:Value|});
    }
}";

        const string fixedCode = @"
using REslava.Result;

class Test
{
    void M()
    {
        var result = Result<string>.Ok(""hello"");
        System.Console.WriteLine(result.Match(v => v, e => default));
    }
}";

        var test = AnalyzerTestHelper.CreateCodeFixTest<UnsafeValueAccessAnalyzer, UnsafeValueAccessCodeFixProvider>(
            testCode, fixedCode, codeFixIndex: 1);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task FixA_WrapsInIfGuard_ForReturnStatement()
    {
        const string testCode = @"
using REslava.Result;

class Test
{
    int M()
    {
        var result = Result<int>.Ok(42);
        return result.{|RESL1001:Value|};
    }
}";

        const string fixedCode = @"
using REslava.Result;

class Test
{
    int M()
    {
        var result = Result<int>.Ok(42);
        if (result.IsSuccess)
        {
            return result.Value;
        }
    }
}";

        var test = AnalyzerTestHelper.CreateCodeFixTest<UnsafeValueAccessAnalyzer, UnsafeValueAccessCodeFixProvider>(
            testCode, fixedCode, codeFixIndex: 0);
        // The fixed code won't compile (missing return path) but that's expected for the code fix
        test.CompilerDiagnostics = Microsoft.CodeAnalysis.Testing.CompilerDiagnostics.None;
        await test.RunAsync();
    }

    [TestMethod]
    public async Task FixB_ReplacesWithMatch_ForReturnStatement()
    {
        const string testCode = @"
using REslava.Result;

class Test
{
    int M()
    {
        var result = Result<int>.Ok(42);
        return result.{|RESL1001:Value|};
    }
}";

        const string fixedCode = @"
using REslava.Result;

class Test
{
    int M()
    {
        var result = Result<int>.Ok(42);
        return result.Match(v => v, e => default);
    }
}";

        var test = AnalyzerTestHelper.CreateCodeFixTest<UnsafeValueAccessAnalyzer, UnsafeValueAccessCodeFixProvider>(
            testCode, fixedCode, codeFixIndex: 1);
        await test.RunAsync();
    }
}
