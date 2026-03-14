using REslava.ResultFlow.Generators.ResultFlow;

namespace REslava.ResultFlow.Tests;

/// <summary>
/// Tests for the three gap patterns fixed in Block 1 #2:
///   Gap 1 — Lambda-wrapped steps:    .Bind(x => DoThing(x)) → node shows DoThing
///   Gap 2 — Async LINQ recognition:  SelectAsync/WhereAsync/SelectMany registered
///   Gap 3 — Variable-assignment root: chain starting from a local variable is extracted
/// </summary>
[TestClass]
public class ResultFlowChainExtractorGapTests
{
    // ── Gap 1 — Lambda-wrapped steps ─────────────────────────────────────────

    [TestMethod]
    public void Gap1_Bind_LambdaWrapped_NodeShowsInnerMethodName()
    {
        var source = CreateSource("Svc", "Process",
            "GetResult(cmd).Bind(x => DoThing(x)).Map(ToDto)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("DoThing"),
            "Lambda body method 'DoThing' should replace 'Bind' as the node label");
        Assert.IsFalse(output.Contains("\"Bind\""),
            "Outer 'Bind' name should be replaced — only 'DoThing' should appear as a node");
    }

    [TestMethod]
    public void Gap1_Map_LambdaWrapped_NodeShowsInnerMethodName()
    {
        var source = CreateSource("Svc", "Process",
            "GetResult(cmd).Bind(Handle).Map(x => Transform(x))");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Transform"),
            "'Transform' extracted from lambda body");
        Assert.IsFalse(output.Contains("\"Map\""),
            "'Map' label replaced by 'Transform'");
    }

    [TestMethod]
    public void Gap1_Ensure_LambdaWrapped_NodeShowsInnerMethodName()
    {
        var source = CreateSource("Svc", "Process",
            "GetResult(cmd).Ensure(x => Validate(x), \"err\").Map(ToDto)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Validate"),
            "'Validate' extracted from lambda body");
    }

    [TestMethod]
    public void Gap1_MethodReference_NotLambda_KeepsOuterName()
    {
        // .Bind(SaveUser) — method group, NOT a lambda → name stays as "Bind"
        var source = CreateSource("Svc", "Process",
            "GetResult(cmd).Bind(SaveUser).Map(ToDto)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Bind"),
            "Method-group argument should keep 'Bind' as node label");
    }

    [TestMethod]
    public void Gap1_MemberAccessLambda_KeepsOuterName()
    {
        // .Map(x => x.ToString()) — member access lambda, NOT a standalone call → no rename
        var source = CreateSource("Svc", "Process",
            "GetResult(cmd).Bind(Handle).Map(x => x.ToString())");
        var output = RunGenerator(source);

        // Map should keep its outer name because x.ToString() is a member-access call
        Assert.IsTrue(output.Contains("Map"),
            "Member-access lambda should keep 'Map' as node label");
    }

    [TestMethod]
    public void Gap1_Lambda_KindIsPreservedFromOuterMethod()
    {
        // .Bind(x => DoThing(x)) — Kind must remain TransformWithRisk → fail edge present
        var source = CreateSource("Svc", "Process",
            "GetResult(cmd).Bind(x => DoThing(x)).Map(ToDto)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("fail"),
            "Bind kind (TransformWithRisk) must be preserved — fail edge must exist");
        Assert.IsTrue(output.Contains("transform"),
            "Bind kind (TransformWithRisk) gives 'transform' CSS class");
    }

    // ── Gap 2 — Async LINQ recognition ───────────────────────────────────────

    [TestMethod]
    public void Gap2_SelectAsync_RecognizedAsPureTransform()
    {
        var source = CreateSource("Svc", "Process",
            "GetResult(cmd).SelectAsync(ToDto)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("SelectAsync"),
            "SelectAsync should appear as a pipeline node");
        Assert.IsTrue(output.Contains("transform"),
            "SelectAsync maps to PureTransform");
    }

    [TestMethod]
    public void Gap2_WhereAsync_RecognizedAsGatekeeper()
    {
        var source = CreateSource("Svc", "Process",
            "GetResult(cmd).WhereAsync(IsValid)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("WhereAsync"),
            "WhereAsync should appear as a pipeline node");
        Assert.IsTrue(output.Contains("gatekeeper"),
            "WhereAsync maps to Gatekeeper");
        Assert.IsTrue(output.Contains("fail"),
            "Gatekeeper has fail edge");
    }

    [TestMethod]
    public void Gap2_SelectMany_RecognizedAsTransformWithRisk()
    {
        var source = CreateSource("Svc", "Process",
            "GetResult(cmd).SelectMany(Handle)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("SelectMany"),
            "SelectMany should appear as a pipeline node");
        Assert.IsTrue(output.Contains("transform"),
            "SelectMany maps to TransformWithRisk");
        Assert.IsTrue(output.Contains("fail"),
            "TransformWithRisk has fail edge");
    }

    [TestMethod]
    public void Gap2_AsyncLinq_MixedWithRegularPipeline()
    {
        var source = CreateSource("Svc", "Process",
            "GetResult(cmd).Bind(Handle).SelectAsync(ToDto).WhereAsync(IsValid)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Bind"),        "Bind node present");
        Assert.IsTrue(output.Contains("SelectAsync"), "SelectAsync node present");
        Assert.IsTrue(output.Contains("WhereAsync"),  "WhereAsync node present");
    }

    // ── Gap 3 — Variable-assignment chain root ────────────────────────────────

    [TestMethod]
    public void Gap3_ReturnVariable_ChainExtracted()
    {
        var source = @"
namespace TestNamespace
{
    public class Svc
    {
        [ResultFlow]
        public string Process(string cmd)
        {
            var found = GetResult(cmd);
            var result = found.Bind(Handle).Map(ToDto);
            return result;
        }
    }
}";
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Bind"),
            "Bind should be extracted from variable-assigned chain");
        Assert.IsTrue(output.Contains("Map"),
            "Map should be extracted from variable-assigned chain");
        Assert.IsTrue(output.Contains("flowchart"),
            "Diagram should be generated (no REF001)");
    }

    [TestMethod]
    public void Gap3_DirectReturnChain_StillWorks()
    {
        // Regression: direct fluent return (no variable) must continue to work
        var source = @"
namespace TestNamespace
{
    public class Svc
    {
        [ResultFlow]
        public string Process(string cmd)
        {
            return GetResult(cmd).Bind(Handle).Map(ToDto);
        }
    }
}";
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Bind"),     "Bind present in direct chain");
        Assert.IsTrue(output.Contains("Map"),      "Map present in direct chain");
        Assert.IsTrue(output.Contains("flowchart"),"Diagram generated");
    }

    [TestMethod]
    public void Gap3_VariableRoot_BindKindPreserved()
    {
        // The chain steps' kinds must be correct regardless of variable root
        var source = @"
namespace TestNamespace
{
    public class Svc
    {
        [ResultFlow]
        public string Process(string cmd)
        {
            var found = GetResult(cmd);
            var result = found.Bind(Handle).Ensure(IsValid).Map(ToDto);
            return result;
        }
    }
}";
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Bind"),       "Bind node present");
        Assert.IsTrue(output.Contains("Ensure"),     "Ensure node present");
        Assert.IsTrue(output.Contains("Map"),        "Map node present");
        Assert.IsTrue(output.Contains("fail"),       "Fail edges present (Bind + Ensure)");
        Assert.IsTrue(output.Contains("gatekeeper"), "Ensure kind correct");
    }

    // ── Combined ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void Combined_VariableRoot_LambdaStep_AsyncStep()
    {
        var source = @"
namespace TestNamespace
{
    public class Svc
    {
        [ResultFlow]
        public string Process(string cmd)
        {
            var found = GetResult(cmd);
            var result = found.Bind(x => Handle(x)).MapAsync(ToDto);
            return result;
        }
    }
}";
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Handle"),   "Lambda body 'Handle' extracted (Gap 1)");
        Assert.IsTrue(output.Contains("MapAsync"), "Async step present");
        Assert.IsTrue(output.Contains("fail"),     "Fail edge from Bind kind");
        Assert.IsTrue(output.Contains("flowchart"),"Diagram generated (Gap 3)");
    }

    // ── ErrorHint — error type annotation on failure edges ────────────────────

    [TestMethod]
    public void ErrorHint_NewErrorCreation_AnnotatesFailEdge()
    {
        var source = CreateSource("Svc", "Process",
            "GetResult(cmd).Ensure(IsValid, new NotFoundError(\"not found\"))");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("NotFoundError"),
            "Error type from 'new NotFoundError(...)' should appear on the fail edge label");
    }

    [TestMethod]
    public void ErrorHint_StaticFactory_AnnotatesFailEdge()
    {
        var source = CreateSource("Svc", "Process",
            "GetResult(cmd).Ensure(IsValid, ValidationError.Field(\"Name\", \"required\"))");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("ValidationError"),
            "Error type from 'ValidationError.Field(...)' should appear on the fail edge label");
    }

    [TestMethod]
    public void ErrorHint_ConflictErrorDuplicate_AnnotatesFailEdge()
    {
        var source = CreateSource("Svc", "Process",
            "GetResult(cmd).Bind(Save).Ensure(IsUnique, ConflictError.Duplicate(\"User\", \"email\", \"x\"))");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("ConflictError"),
            "Error type from 'ConflictError.Duplicate(...)' should appear on the fail edge label");
    }

    [TestMethod]
    public void ErrorHint_NoErrorArg_FallsBackToPlainFail()
    {
        var source = CreateSource("Svc", "Process",
            "GetResult(cmd).Bind(Handle)");
        var output = RunGenerator(source);

        // The Mermaid string is embedded in a verbatim C# literal, so " becomes "".
        // Plain |"fail"| appears as |""fail""| in the output — just verify "fail" is
        // present but no error-type annotation ("fail: X") was added.
        Assert.IsTrue(output.Contains("fail"),
            "Bind with no error arg should still show a 'fail' edge");
        Assert.IsFalse(output.Contains("fail: "),
            "No error hint should be appended when the arg is a plain method reference");
    }

    #region Helpers

    private static string CreateSource(string className, string methodName, string returnExpression) => $@"
namespace TestNamespace
{{
    public class {className}
    {{
        [ResultFlow]
        public string {methodName}(string cmd) => {returnExpression};
    }}
}}";

    private static string RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(source));

        var references = new System.Collections.Generic.List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ResultFlowGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        var updatedDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var runResult = updatedDriver.GetRunResult();

        var sb = new System.Text.StringBuilder();
        foreach (var tree in runResult.GeneratedTrees)
        {
            using var writer = new System.IO.StringWriter();
            tree.GetText().Write(writer);
            sb.AppendLine(writer.ToString());
        }

        return sb.ToString();
    }

    #endregion
}
