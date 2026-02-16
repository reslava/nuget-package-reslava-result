using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.Analyzers.Analyzers;
using REslava.Result.Analyzers.CodeFixes;
using REslava.Result.Analyzers.Tests.Helpers;

namespace REslava.Result.Analyzers.Tests.CodeFixes;

[TestClass]
public class RESL2001_CodeFixTests
{
    [TestMethod]
    public async Task OneOf2_AsT1_ReplacedWithMatch()
    {
        const string testCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int>.FromT1(""hello"");
        var x = oneOf.{|RESL2001:AsT1|};
    }
}";

        const string fixedCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int>.FromT1(""hello"");
        var x = oneOf.Match(t1 => t1, t2 => throw new System.NotImplementedException());
    }
}";

        var test = AnalyzerTestHelper.CreateCodeFixTest<UnsafeOneOfAccessAnalyzer, UnsafeOneOfAccessCodeFixProvider>(
            testCode, fixedCode, codeFixIndex: 0, AnalyzerTestHelper.OneOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task OneOf2_AsT2_ReplacedWithMatch()
    {
        const string testCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int>.FromT1(""hello"");
        var x = oneOf.{|RESL2001:AsT2|};
    }
}";

        const string fixedCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int>.FromT1(""hello"");
        var x = oneOf.Match(t1 => throw new System.NotImplementedException(), t2 => t2);
    }
}";

        var test = AnalyzerTestHelper.CreateCodeFixTest<UnsafeOneOfAccessAnalyzer, UnsafeOneOfAccessCodeFixProvider>(
            testCode, fixedCode, codeFixIndex: 0, AnalyzerTestHelper.OneOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task OneOf3_AsT2_ReplacedWithMatch()
    {
        const string testCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int, bool>.FromT1(""hello"");
        var x = oneOf.{|RESL2001:AsT2|};
    }
}";

        const string fixedCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int, bool>.FromT1(""hello"");
        var x = oneOf.Match(t1 => throw new System.NotImplementedException(), t2 => t2, t3 => throw new System.NotImplementedException());
    }
}";

        var test = AnalyzerTestHelper.CreateCodeFixTest<UnsafeOneOfAccessAnalyzer, UnsafeOneOfAccessCodeFixProvider>(
            testCode, fixedCode, codeFixIndex: 0, AnalyzerTestHelper.OneOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task OneOf4_AsT3_ReplacedWithMatch()
    {
        const string testCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int, bool, double>.FromT1(""hello"");
        var x = oneOf.{|RESL2001:AsT3|};
    }
}";

        const string fixedCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int, bool, double>.FromT1(""hello"");
        var x = oneOf.Match(t1 => throw new System.NotImplementedException(), t2 => throw new System.NotImplementedException(), t3 => t3, t4 => throw new System.NotImplementedException());
    }
}";

        var test = AnalyzerTestHelper.CreateCodeFixTest<UnsafeOneOfAccessAnalyzer, UnsafeOneOfAccessCodeFixProvider>(
            testCode, fixedCode, codeFixIndex: 0, AnalyzerTestHelper.OneOfStubSource);
        await test.RunAsync();
    }
}
