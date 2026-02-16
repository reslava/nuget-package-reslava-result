using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.Analyzers.Analyzers;
using REslava.Result.Analyzers.Tests.Helpers;

namespace REslava.Result.Analyzers.Tests.Analyzers;

[TestClass]
public class RESL2001_UnsafeOneOfAccessTests
{
    [TestMethod]
    public async Task DirectAsT1Access_WithoutGuard_Reports()
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

        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnsafeOneOfAccessAnalyzer>(testCode, AnalyzerTestHelper.OneOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task DirectAsT2Access_WithoutGuard_Reports()
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

        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnsafeOneOfAccessAnalyzer>(testCode, AnalyzerTestHelper.OneOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task AsT1Access_InsideIsT1Guard_NoReport()
    {
        const string testCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int>.FromT1(""hello"");
        if (oneOf.IsT1)
        {
            var x = oneOf.AsT1;
        }
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnsafeOneOfAccessAnalyzer>(testCode, AnalyzerTestHelper.OneOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task AsT2Access_InsideIsT2Guard_NoReport()
    {
        const string testCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int>.FromT1(""hello"");
        if (oneOf.IsT2)
        {
            var x = oneOf.AsT2;
        }
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnsafeOneOfAccessAnalyzer>(testCode, AnalyzerTestHelper.OneOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task AsT1Access_AfterEarlyReturn_NoReport()
    {
        const string testCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int>.FromT1(""hello"");
        if (!oneOf.IsT1) return;
        var x = oneOf.AsT1;
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnsafeOneOfAccessAnalyzer>(testCode, AnalyzerTestHelper.OneOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task AsT1Access_WrongGuard_Reports()
    {
        const string testCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int>.FromT1(""hello"");
        if (oneOf.IsT2)
        {
            var x = oneOf.{|RESL2001:AsT1|};
        }
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnsafeOneOfAccessAnalyzer>(testCode, AnalyzerTestHelper.OneOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task OneOf3_AsT3Access_WithoutGuard_Reports()
    {
        const string testCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int, bool>.FromT1(""hello"");
        var x = oneOf.{|RESL2001:AsT3|};
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnsafeOneOfAccessAnalyzer>(testCode, AnalyzerTestHelper.OneOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task OneOf3_AsT3Access_InsideIsT3Guard_NoReport()
    {
        const string testCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int, bool>.FromT1(""hello"");
        if (oneOf.IsT3)
        {
            var x = oneOf.AsT3;
        }
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnsafeOneOfAccessAnalyzer>(testCode, AnalyzerTestHelper.OneOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task OneOf4_AsT4Access_WithoutGuard_Reports()
    {
        const string testCode = @"
using REslava.Result.AdvancedPatterns;

class Test
{
    void M()
    {
        var oneOf = OneOf<string, int, bool, double>.FromT1(""hello"");
        var x = oneOf.{|RESL2001:AsT4|};
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnsafeOneOfAccessAnalyzer>(testCode, AnalyzerTestHelper.OneOfStubSource);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task NonOneOfType_NoReport()
    {
        const string testCode = @"
class FakeOneOf
{
    public string AsT1 => ""hello"";
    public bool IsT1 => true;
}

class Test
{
    void M()
    {
        var fake = new FakeOneOf();
        var x = fake.AsT1;
    }
}";

        var test = AnalyzerTestHelper.CreateAnalyzerTest<UnsafeOneOfAccessAnalyzer>(testCode, AnalyzerTestHelper.OneOfStubSource);
        await test.RunAsync();
    }
}
