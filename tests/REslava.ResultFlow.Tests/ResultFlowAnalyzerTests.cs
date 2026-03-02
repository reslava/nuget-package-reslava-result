using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using REslava.ResultFlow.Analyzers;
using System.Collections.Immutable;

namespace REslava.ResultFlow.Tests;

[TestClass]
public class ResultFlowAnalyzerTests
{
    // ───────────────────────────────────────────────────────────────────────
    // 1. REF002 emitted for a valid [ResultFlow] fluent chain
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public async Task Analyzer_Should_Emit_REF002_For_Valid_Chain()
    {
        var source = @"
namespace TestNamespace
{
    public class UserService
    {
        [ResultFlow]
        public string RegisterAsync(string cmd) => GetUser(cmd).Bind(Save);
    }
}";
        var diagnostics = await RunAnalyzerAsync(source);

        Assert.IsTrue(diagnostics.Any(d => d.Id == "REF002"), "Should emit REF002 for valid chain");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 2. REF002 NOT emitted when the chain is undetectable (REF001 handles it)
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public async Task Analyzer_Should_Not_Emit_REF002_For_Invalid_Chain()
    {
        var source = @"
namespace TestNamespace
{
    public class UserService
    {
        [ResultFlow]
        public string RegisterAsync(string cmd) { return null; }
    }
}";
        var diagnostics = await RunAnalyzerAsync(source);

        Assert.IsFalse(diagnostics.Any(d => d.Id == "REF002"), "Should not emit REF002 for undetectable chain");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 3. REF002 NOT emitted for methods without [ResultFlow]
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public async Task Analyzer_Should_Not_Emit_REF002_Without_Attribute()
    {
        var source = @"
namespace TestNamespace
{
    public class UserService
    {
        public string RegisterAsync(string cmd) => GetUser(cmd).Bind(Save);
    }
}";
        var diagnostics = await RunAnalyzerAsync(source);

        Assert.IsFalse(diagnostics.Any(d => d.Id == "REF002"), "Should not emit REF002 without [ResultFlow]");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 4. REF002 diagnostic message contains the method name
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public async Task Analyzer_REF002_Message_Should_Contain_Method_Name()
    {
        var source = @"
namespace TestNamespace
{
    public class UserService
    {
        [ResultFlow]
        public string RegisterAsync(string cmd) => GetUser(cmd).Bind(Save);
    }
}";
        var diagnostics = await RunAnalyzerAsync(source);
        var ref002 = diagnostics.FirstOrDefault(d => d.Id == "REF002");

        Assert.IsNotNull(ref002, "REF002 should be emitted");
        Assert.IsTrue(ref002!.GetMessage().Contains("RegisterAsync"), "REF002 message should contain method name");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 5. REF002 is Info severity (not warning, not error)
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public async Task Analyzer_REF002_Should_Be_Info_Severity()
    {
        var source = @"
namespace TestNamespace
{
    public class UserService
    {
        [ResultFlow]
        public string Foo(string x) => Bar(x).Map(ToDto);
    }
}";
        var diagnostics = await RunAnalyzerAsync(source);
        var ref002 = diagnostics.FirstOrDefault(d => d.Id == "REF002");

        Assert.IsNotNull(ref002, "REF002 should be emitted");
        Assert.AreEqual(DiagnosticSeverity.Info, ref002!.Severity, "REF002 should be Info severity");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 6. Code fix targets REF002
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void CodeFix_Should_Target_REF002()
    {
        var fix = new ResultFlowInsertCommentFix();

        Assert.IsTrue(fix.FixableDiagnosticIds.Contains("REF002"),
            "Code fix should target REF002");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 7. Two [ResultFlow] methods → two REF002 diagnostics
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public async Task Analyzer_Should_Emit_One_REF002_Per_Method()
    {
        var source = @"
namespace TestNamespace
{
    public class UserService
    {
        [ResultFlow]
        public string RegisterAsync(string cmd) => GetUser(cmd).Bind(Save);

        [ResultFlow]
        public string GetAsync(int id) => FindUser(id).Map(ToDto);
    }
}";
        var diagnostics = await RunAnalyzerAsync(source);
        var ref002s = diagnostics.Where(d => d.Id == "REF002").ToList();

        Assert.AreEqual(2, ref002s.Count, "Should emit one REF002 per [ResultFlow] method");
    }

    #region Helpers

    private static async Task<IReadOnlyList<Diagnostic>> RunAnalyzerAsync(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new ResultFlowDiagramAnalyzer());
        var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);
        var allDiagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync();

        // Return only REF-prefixed diagnostics (exclude compiler errors)
        return allDiagnostics
            .Where(d => d.Id.StartsWith("REF"))
            .ToList();
    }

    #endregion
}
