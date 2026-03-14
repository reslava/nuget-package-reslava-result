using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using REslava.Result.Analyzers.Analyzers;
using REslava.Result.Analyzers.Tests.Helpers;

namespace REslava.Result.Analyzers.Tests.Analyzers;

[TestClass]
public class RESL1021_ErrorConstructorOverloadTests
{
    /// <summary>Minimal IError + IReason stubs — no other types needed.</summary>
    private const string Stubs = @"
namespace REslava.Result
{
    public interface IError  { string Message { get; } }
    public interface IReason { string Message { get; } }
}";

    // ── Valid — no diagnostic ─────────────────────────────────────────────────

    [TestMethod]
    public async Task SingleStringCtor_IError_NoReport()
    {
        const string code = @"
using REslava.Result;
class MyError : IError
{
    public MyError(string message) { }
    public string Message { get; }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ErrorConstructorOverloadAnalyzer>(code, Stubs);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ParameterlessCtor_NoReport()
    {
        const string code = @"
using REslava.Result;
class MyError : IError
{
    public MyError() { }
    public string Message { get; }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ErrorConstructorOverloadAnalyzer>(code, Stubs);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task StringAndExceptionCtor_NoReport()
    {
        const string code = @"
using System;
using REslava.Result;
class MyError : IError
{
    public MyError(string message, Exception ex) { }
    public string Message { get; }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ErrorConstructorOverloadAnalyzer>(code, Stubs);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ObsoleteTwoArgCtor_NoReport()
    {
        const string code = @"
using System;
using REslava.Result;
class MyError : IError
{
    [Obsolete]
    public MyError(string entity, string field) { }
    public string Message { get; }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ErrorConstructorOverloadAnalyzer>(code, Stubs);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task PrivateTwoArgCtor_NoReport()
    {
        const string code = @"
using REslava.Result;
class MyError : IError
{
    private MyError(string entity, string field) { }
    public string Message { get; }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ErrorConstructorOverloadAnalyzer>(code, Stubs);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ClassNotImplementingInterface_NoReport()
    {
        const string code = @"
class Helper
{
    public Helper(string a, string b) { }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ErrorConstructorOverloadAnalyzer>(code, Stubs);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task OptionalSecondParam_NoReport()
    {
        const string code = @"
using REslava.Result;
class MyError : IError
{
    public MyError(string message, string? callerMember = null) { }
    public string Message { get; }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ErrorConstructorOverloadAnalyzer>(code, Stubs);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task IReason_SingleStringCtor_NoReport()
    {
        const string code = @"
using REslava.Result;
class MyReason : IReason
{
    public MyReason(string message) { }
    public string Message { get; }
}";
        var test = AnalyzerTestHelper.CreateAnalyzerTest<ErrorConstructorOverloadAnalyzer>(code, Stubs);
        await test.RunAsync();
    }

    // ── Invalid — diagnostic expected ─────────────────────────────────────────

    [TestMethod]
    public async Task TwoStringCtor_IError_Reports()
    {
        const string code = @"
using REslava.Result;
class MyError : IError
{
    public {|#0:MyError|}(string entity, string field) { }
    public string Message { get; }
}";
        var expected = new DiagnosticResult("RESL1021", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("MyError");

        var test = AnalyzerTestHelper.CreateAnalyzerTest<ErrorConstructorOverloadAnalyzer>(code, Stubs);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task StringObjectCtor_Reports()
    {
        const string code = @"
using REslava.Result;
class MyError : IError
{
    public {|#0:MyError|}(string entity, object value) { }
    public string Message { get; }
}";
        var expected = new DiagnosticResult("RESL1021", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("MyError");

        var test = AnalyzerTestHelper.CreateAnalyzerTest<ErrorConstructorOverloadAnalyzer>(code, Stubs);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task ThreeArgCtor_Reports()
    {
        const string code = @"
using REslava.Result;
class MyError : IError
{
    public {|#0:MyError|}(string entity, string field, int code) { }
    public string Message { get; }
}";
        var expected = new DiagnosticResult("RESL1021", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("MyError");

        var test = AnalyzerTestHelper.CreateAnalyzerTest<ErrorConstructorOverloadAnalyzer>(code, Stubs);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [TestMethod]
    public async Task IReason_TwoStringCtor_Reports()
    {
        const string code = @"
using REslava.Result;
class MyReason : IReason
{
    public {|#0:MyReason|}(string entity, string field) { }
    public string Message { get; }
}";
        var expected = new DiagnosticResult("RESL1021", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning)
            .WithLocation(0)
            .WithArguments("MyReason");

        var test = AnalyzerTestHelper.CreateAnalyzerTest<ErrorConstructorOverloadAnalyzer>(code, Stubs);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }
}
