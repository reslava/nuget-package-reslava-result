using REslava.ResultFlow.Generators.ResultFlow;

namespace REslava.ResultFlow.Tests;

[TestClass]
public class ResultFlowGeneratorTests
{
    // ───────────────────────────────────────────────────────────────────────
    // 1. Single Ensure → node + Failure edge
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void ResultFlow_Ensure_Should_Generate_Node_And_Failure_Edge()
    {
        var source = CreateSource("UserService", "RegisterAsync", "GetUser(cmd).Ensure(IsValid)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Ensure"),       "Should contain Ensure node");
        Assert.IsTrue(output.Contains("Failure"),      "Should contain Failure node");
        Assert.IsTrue(output.Contains("fail"),         "Should contain fail edge");
        Assert.IsTrue(output.Contains("gatekeeper"),   "Should apply gatekeeper class");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 1b. Ensure with lambda predicate → tooltip span in node label
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void ResultFlow_Ensure_LambdaPredicate_Should_Generate_Tooltip()
    {
        var source = CreateSource("UserService", "RegisterAsync",
            "GetUser(cmd).Ensure(x => x.IsActive)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("span title="),  "Ensure lambda predicate should emit <span title=...> tooltip");
        Assert.IsTrue(output.Contains("x.IsActive"),   "Tooltip should contain predicate body text");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 1c. Ensure with method-group predicate → no tooltip (not a lambda)
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void ResultFlow_Ensure_MethodGroupPredicate_Should_Not_Generate_Tooltip()
    {
        var source = CreateSource("UserService", "RegisterAsync",
            "GetUser(cmd).Ensure(IsValid)");
        var output = RunGenerator(source);

        Assert.IsFalse(PipelineOnly(output).Contains("span title="), "Method-group predicate should not emit tooltip");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 2. Bind + Map → TransformWithRisk has fail edge, PureTransform does not
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void ResultFlow_Bind_And_Map_Should_Have_Correct_Edges()
    {
        var source = CreateSource("UserService", "RegisterAsync",
            "await GetUser(cmd).BindAsync(SaveUser).MapAsync(ToDto)");
        var output = RunGenerator(source);

        // Node IDs are hashes; "Async" suffix stripped in labels → method appears as "Bind⚡" / "Map⚡"
        Assert.IsTrue(output.Contains("Bind\u26a1"),  "Should contain Bind node (rendered as Bind⚡)");
        Assert.IsTrue(output.Contains("Map\u26a1"),   "Should contain Map node (rendered as Map⚡)");
        Assert.IsTrue(output.Contains("fail"),         "Bind should have fail edge");

        // MapAsync (PureTransform) should not have its own fail edge
        Assert.IsFalse(output.Contains("F1["), "MapAsync should not have a fail edge");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 3. Tap → sideeffect class, no fail edge
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void ResultFlow_Tap_Should_Be_SideEffect_Without_Fail_Edge()
    {
        var source = CreateSource("UserService", "RegisterAsync",
            "GetUser(cmd).Tap(SendEmail)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Tap"),          "Should contain Tap node");
        Assert.IsTrue(output.Contains("sideeffect"),   "Should apply sideeffect class");
        Assert.IsFalse(output.Contains("|fail|"),      "Tap should not have fail edge");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 4. Match → terminal class, emits hexagon + ok/fail edges
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void ResultFlow_Match_Should_Be_Terminal_With_Ok_And_Fail_Edges()
    {
        var source = CreateSource("UserService", "GetAsync",
            "GetUser(id).Match(ok => ok, err => null)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Match"),    "Should contain Match node");
        Assert.IsTrue(output.Contains("terminal"), "Should apply terminal class");

        // Terminal node emits |ok| SUCCESS and |fail| edges
        Assert.IsTrue(output.Contains("-->|ok| SUCCESS"), "Match should emit -->|ok| SUCCESS edge");
        Assert.IsTrue(output.Contains("-->|fail|"),        "Match should emit a fail edge");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 5. WithSuccess → invisible, must NOT appear in diagram
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void ResultFlow_WithSuccess_Should_Not_Appear_In_Diagram()
    {
        var source = CreateSource("UserService", "RegisterAsync",
            "GetUser(cmd).WithSuccess(\"ok\").Bind(SaveUser)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Bind"),        "Bind should be in diagram");
        Assert.IsFalse(output.Contains("WithSuccess"), "WithSuccess should be invisible");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 6. Complete pipeline with all node kinds
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void ResultFlow_Complete_Pipeline_Should_Contain_All_Node_Types()
    {
        var source = CreateSource("UserService", "RegisterAsync",
            "await GetUser(cmd).EnsureAsync(IsValid).BindAsync(SaveUser).TapAsync(SendEmail).MapAsync(ToDto).Match(ok => ok, err => null)");
        var output = RunGenerator(source);

        // "Async" suffix stripped in labels → method appears as "Ensure⚡" / "Bind⚡" / etc.
        Assert.IsTrue(output.Contains("Ensure\u26a1"),  "Should have Ensure node (rendered as Ensure⚡)");
        Assert.IsTrue(output.Contains("Bind\u26a1"),    "Should have Bind node (rendered as Bind⚡)");
        Assert.IsTrue(output.Contains("Tap\u26a1"),     "Should have Tap node (rendered as Tap⚡)");
        Assert.IsTrue(output.Contains("Map\u26a1"),     "Should have Map node (rendered as Map⚡)");
        Assert.IsTrue(output.Contains("Match"),        "Should have Match terminal");
        Assert.IsTrue(output.Contains("gatekeeper"),   "Should have gatekeeper class");
        Assert.IsTrue(output.Contains("bind"),         "Should have bind class (TransformWithRisk)");
        Assert.IsTrue(output.Contains("map"),          "Should have map class (PureTransform)");
        Assert.IsTrue(output.Contains("sideeffect"),   "Should have sideeffect class");
        Assert.IsTrue(output.Contains("terminal"),     "Should have terminal class");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 7. Two [ResultFlow] methods in same class → both consts in same _Flows class
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void ResultFlow_Two_Methods_Same_Class_Should_Be_In_Same_Flows_Class()
    {
        var source = @"
namespace TestNamespace
{
    public class UserService
    {
        [ResultFlow]
        public string RegisterAsync(string cmd)
            => GetUser(cmd).Ensure(IsValid).Bind(SaveUser);

        [ResultFlow]
        public string GetAsync(int id)
            => FindUser(id).Map(ToDto);
    }
}";
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("UserService_Flows"), "Should generate UserService_Flows class");
        Assert.IsTrue(output.Contains("RegisterAsync"),     "Should contain RegisterAsync const");
        Assert.IsTrue(output.Contains("GetAsync"),          "Should contain GetAsync const");

        var firstOccurrence = output.IndexOf("UserService_Flows", System.StringComparison.Ordinal);
        Assert.IsTrue(firstOccurrence >= 0, "UserService_Flows class should exist");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 8. Method without [ResultFlow] → no diagram generated
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void ResultFlow_Method_Without_Attribute_Should_Not_Generate_Diagram()
    {
        var source = @"
namespace TestNamespace
{
    public class UserService
    {
        public string RegisterAsync(string cmd)
            => GetUser(cmd).Ensure(IsValid).Bind(SaveUser);
    }
}";
        var output = RunGenerator(source);

        Assert.IsFalse(output.Contains("UserService_Flows"), "Should not generate flows class without [ResultFlow]");
        Assert.IsFalse(output.Contains("flowchart"),         "Should not generate Mermaid diagram");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 9. Empty / non-fluent method body → REF001 diagnostic, no diagram
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void ResultFlow_Empty_Method_Should_Emit_REF001_And_No_Diagram()
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
        var (output, diagnostics) = RunGeneratorFull(source);

        Assert.IsFalse(output.Contains("flowchart"), "Should not generate diagram for empty chain");
        Assert.IsTrue(diagnostics.Any(d => d.Id == "REF001"), "Should emit REF001 diagnostic");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 10. [ResultFlow] attribute is generated in output
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void ResultFlow_Attribute_Should_Be_Generated()
    {
        var output = RunGenerator("namespace T { }");

        Assert.IsTrue(output.Contains("ResultFlowAttribute"), "Should generate ResultFlowAttribute class");
        Assert.IsTrue(output.Contains("AttributeTargets.Method"), "Should target methods");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 11. Generated namespace is Generated.ResultFlow
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void ResultFlow_Generated_Namespace_Should_Be_Correct()
    {
        var source = CreateSource("UserService", "RegisterAsync", "GetUser(cmd).Bind(SaveUser)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Generated.ResultFlow"), "Should use Generated.ResultFlow namespace");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 12. Const name matches method name exactly
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void ResultFlow_Const_Name_Should_Match_Method_Name()
    {
        var source = CreateSource("UserService", "RegisterAsync", "GetUser(cmd).Bind(SaveUser)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("public const string RegisterAsync"),
            "Const name should match the decorated method name");
    }

    // ── Convention dictionary — ErrorOr / LanguageExt additions ────────────────
    // ───────────────────────────────────────────────────────────────────────
    // 13. Then (ErrorOr) → TransformWithRisk — has fail edge
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void Convention_Then_Should_Be_TransformWithRisk()
    {
        var source = CreateSource("Svc", "Process", "GetResult(cmd).Then(Handle)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Then"),      "Should render Then node");
        Assert.IsTrue(output.Contains("bind"),      "Then should use bind class (TransformWithRisk)");
        Assert.IsTrue(output.Contains("fail"),      "Then should have a fail edge (TransformWithRisk)");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 14. Switch (ErrorOr/OneOf) → Terminal — no outbound edges
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void Convention_Switch_Should_Be_Terminal()
    {
        var source = CreateSource("Svc", "Process", "GetResult(cmd).Bind(Handle).Switch(x => x)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Switch"),   "Should render Switch node");
        Assert.IsTrue(output.Contains("terminal"), "Switch should use terminal class");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 15. Filter (LanguageExt) → Gatekeeper — has fail edge
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void Convention_Filter_Should_Be_Gatekeeper()
    {
        var source = CreateSource("Svc", "Process", "GetResult(cmd).Filter(IsValid)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Filter"),     "Should render Filter node");
        Assert.IsTrue(output.Contains("gatekeeper"), "Filter should use gatekeeper class");
        Assert.IsTrue(output.Contains("fail"),       "Filter should have a fail edge (Gatekeeper)");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 16. Do (LanguageExt) → SideEffectSuccess — sideeffect class
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void Convention_Do_Should_Be_SideEffectSuccess()
    {
        var source = CreateSource("Svc", "Process", "GetResult(cmd).Bind(Handle).Do(Log)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("sideeffect"), "Do should use sideeffect class");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 17. DoLeft (LanguageExt) → SideEffectFailure — sideeffect class
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void Convention_DoLeft_Should_Be_SideEffectFailure()
    {
        var source = CreateSource("Svc", "Process", "GetResult(cmd).Bind(Handle).DoLeft(LogError)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("sideeffect"), "DoLeft should use sideeffect class");
    }

    // ── resultflow.json config ───────────────────────────────────────────────
    // ───────────────────────────────────────────────────────────────────────
    // 18. Custom method classified via resultflow.json
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void Config_CustomMethod_Should_Be_Classified_By_Config()
    {
        var config = @"{""mappings"":[{""bind"":[""Chain""]}]}";
        var source = CreateSource("Svc", "Process", "GetResult(cmd).Chain(Handle)");
        var (output, _) = RunGeneratorWithConfig(source, config);

        Assert.IsTrue(output.Contains("Chain"),     "Should render Chain node");
        Assert.IsTrue(output.Contains("bind"),      "Chain should use bind class (bind → TransformWithRisk)");
        Assert.IsTrue(output.Contains("fail"),      "Chain should have a fail edge (TransformWithRisk)");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 19. Config overrides built-in classification
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void Config_Should_Override_Builtin_Classification()
    {
        // Reclassify Bind as PureTransform (map) → no fail edge
        var config = @"{""mappings"":[{""map"":[""Bind""]}]}";
        var source = CreateSource("Svc", "Process", "GetResult(cmd).Bind(Handle)");
        var (output, _) = RunGeneratorWithConfig(source, config);

        Assert.IsTrue(output.Contains("map"),         "Bind reclassified as PureTransform uses map class");
        Assert.IsFalse(output.Contains("|fail|"),     "Bind reclassified as map should have no fail edge");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 20. Invalid resultflow.json → REF003 diagnostic + fallback to convention
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void Config_InvalidJson_Should_Emit_REF003_And_Fallback_To_Convention()
    {
        var config = "{ this is not valid json }";
        var source = CreateSource("Svc", "Process", "GetResult(cmd).Bind(Handle)");
        var (output, diagnostics) = RunGeneratorWithConfig(source, config);

        Assert.IsTrue(diagnostics.Any(d => d.Id == "REF003"), "Should emit REF003 for malformed JSON");
        Assert.IsTrue(output.Contains("Bind"),     "Should still generate diagram (fallback to convention)");
        Assert.IsTrue(output.Contains("bind"),      "Bind should still use bind class (fallback to convention)");
    }

    // ── RF-1: Async step annotation (⚡ suffix) ──────────────────────────────
    // ───────────────────────────────────────────────────────────────────────
    // 21. BindAsync label contains ⚡
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void AsyncAnnotation_BindAsync_Label_Should_Contain_Lightning()
    {
        var source = CreateSource("Svc", "Process", "GetResult(cmd).BindAsync(Handle)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Bind\u26a1"), "BindAsync label should strip suffix and contain ⚡");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 22. EnsureAsync label contains ⚡
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void AsyncAnnotation_EnsureAsync_Label_Should_Contain_Lightning()
    {
        var source = CreateSource("Svc", "Process", "GetResult(cmd).EnsureAsync(IsValid)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Ensure\u26a1"), "EnsureAsync label should strip suffix and contain ⚡");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 23. MapAsync label contains ⚡
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void AsyncAnnotation_MapAsync_Label_Should_Contain_Lightning()
    {
        var source = CreateSource("Svc", "Process", "GetResult(cmd).Bind(Handle).MapAsync(ToDto)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Map\u26a1"), "MapAsync label should strip suffix and contain ⚡");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 24. TapAsync label contains ⚡
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void AsyncAnnotation_TapAsync_Label_Should_Contain_Lightning()
    {
        var source = CreateSource("Svc", "Process", "GetResult(cmd).Bind(Handle).TapAsync(Log)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Tap\u26a1"), "TapAsync label should strip suffix and contain ⚡");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 25. Sync Bind label does NOT contain ⚡
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void AsyncAnnotation_SyncBind_Label_Should_Not_Contain_Lightning()
    {
        var source = CreateSource("Svc", "Process", "GetResult(cmd).Bind(Handle)");
        var output = RunGenerator(source);

        Assert.IsFalse(output.Contains("Bind\u26a1"), "Sync Bind label should not contain ⚡");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 26. Mixed pipeline — sync and async nodes each marked correctly
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void AsyncAnnotation_Mixed_Pipeline_Should_Mark_Async_And_Leave_Sync_Clean()
    {
        var source = CreateSource("Svc", "Process",
            "await GetResult(cmd).Bind(Handle).BindAsync(SaveAsync)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("\"Bind\""),             "Sync Bind should have no ⚡");
        Assert.IsTrue(output.Contains("\"Bind\u26a1\""), "BindAsync should have ⚡ with no Async suffix");
    }

    // ── RF-2: Success Type Travel ─────────────────────────────────────────────

    // ───────────────────────────────────────────────────────────────────────
    // 27. Map with type change → type transition label
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void TypeTravel_Map_ShowsTypeTransition()
    {
        var source = CreateTypedSource("UserService", "Register",
            "CreateUser().Map(ToDto)",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static UserDto ToDto(User u) => new UserDto();");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Map"),    "Map node present");
        Assert.IsTrue(output.Contains("<br/>"),  "Type label separator present");
        Assert.IsTrue(output.Contains("User"),   "Input type User present");
        Assert.IsTrue(output.Contains("UserDto"), "Output type UserDto present");
        Assert.IsTrue(output.Contains("\u2192"), "Transition arrow present");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 28. Bind same type → type-only label (no arrow)
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void TypeTravel_Bind_SameType_ShowsTypeOnly()
    {
        var source = CreateTypedSource("UserService", "Register",
            "CreateUser().Bind(SaveUser)",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static Result<User> SaveUser(User u) => Result<User>.Ok(u);");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Bind"),  "Bind node present");
        Assert.IsTrue(output.Contains("<br/>"), "Type label present");
        Assert.IsTrue(output.Contains("User"),  "User type present");
        // Bind node itself should have no → (same-type); ENTRY_ROOT may contain → from seed label
        // Node IDs are now hashes; find the Bind node line via :::bind classDef
        var bindLine = System.Array.Find(output.Split('\n'), l => l.Contains(":::bind") && l.Contains("\"Bind"));
        Assert.IsNotNull(bindLine, "Bind node line should exist");
        Assert.IsFalse(bindLine!.Contains("\u2192"), "Bind node label should have no transition arrow for same-type Bind");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 29. Ensure (type-preserving) → type-only label
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void TypeTravel_Ensure_ShowsTypeOnly()
    {
        var source = CreateTypedSource("UserService", "Register",
            "CreateUser().Ensure(IsValid, \"invalid\")",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static bool IsValid(User u) => true;");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Ensure"), "Ensure node present");
        Assert.IsTrue(output.Contains("<br/>"),  "Type label present");
        Assert.IsTrue(output.Contains("User"),   "User type present");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 30. Tap (type-preserving side-effect) → type-only label
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void TypeTravel_Tap_ShowsTypeOnly()
    {
        var source = CreateTypedSource("UserService", "Register",
            "CreateUser().Tap(SendEmail)",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static void SendEmail(User u) { }");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Tap"),   "Tap node present");
        Assert.IsTrue(output.Contains("<br/>"), "Type label present");
        Assert.IsTrue(output.Contains("User"),  "User type present");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 31. Non-generic return type → falls back to method name only (no <br/>)
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void TypeTravel_NonGenericReturn_FallsBackToMethodName()
    {
        // Uses CreateSource (string-returning method) — Bind doesn't exist on string,
        // so GetSymbolInfo returns null → OutputType = null → no type label
        var source = CreateSource("Svc", "Process", "GetResult(cmd).Bind(Handle)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Bind"),   "Bind node still present");
        Assert.IsFalse(PipelineOnly(output).Contains("<br/>"), "No type label when type cannot be resolved");
    }

    // ───────────────────────────────────────────────────────────────────────
    // 32. Mixed pipeline → all nodes have correct type labels
    // ───────────────────────────────────────────────────────────────────────
    [TestMethod]
    public void TypeTravel_Mixed_Pipeline_CorrectLabelsOnAllNodes()
    {
        var source = CreateTypedSource("UserService", "Register",
            "CreateUser().Ensure(IsValid, \"x\").Bind(SaveUser).Map(ToDto)",
            extraMethods: @"
        static Result<User> CreateUser() => Result<User>.Ok(new User());
        static bool IsValid(User u) => true;
        static Result<User> SaveUser(User u) => Result<User>.Ok(u);
        static UserDto ToDto(User u) => new UserDto();");

        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("Ensure"), "Ensure node present");
        Assert.IsTrue(output.Contains("Bind"),   "Bind node present");
        Assert.IsTrue(output.Contains("Map"),    "Map node present");
        Assert.IsTrue(output.Contains("User"),   "User type present");
        Assert.IsTrue(output.Contains("UserDto"), "UserDto type present");
        Assert.IsTrue(output.Contains("\u2192"), "Transition arrow present for Map");
    }

    // ── Title frontmatter + entry node (v1.47.1) ─────────────────────────────

    [TestMethod]
    public void Title_EmitsMethodNameAsFrontmatterTitle()
    {
        var source = CreateSource("UserService", "RegisterAsync", "GetUser(cmd).Bind(SaveUser)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("title: Register\u26a1"), "Frontmatter title should strip Async suffix and add ⚡");
        Assert.IsTrue(output.Contains("---"), "Frontmatter delimiters should be present");
    }

    [TestMethod]
    public void EntryNode_EmitsSeedMethodAndType()
    {
        var source = CreateSource("UserService", "RegisterAsync", "GetUser(cmd).Bind(SaveUser)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("ENTRY_ROOT"), "Entry node should be emitted");
        Assert.IsTrue(output.Contains("GetUser"), "Entry node should use seed method name");
        Assert.IsTrue(output.Contains("==>"), "Entry node should use thick arrow");
    }

    [TestMethod]
    public void EntryNode_AsyncSeed_EmitsLightningMarker()
    {
        var source = CreateSource("UserService", "RegisterAsync", "GetUserAsync(cmd).Bind(SaveUser)");
        var output = RunGenerator(source);

        Assert.IsTrue(output.Contains("ENTRY_ROOT"), "Entry node should be emitted");
        Assert.IsTrue(output.Contains("\u26a1"), "Async seed should have ⚡ marker");
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

    private static string CreateTypedSource(string className, string methodName, string returnExpression, string extraMethods = "") => $@"
using System;

namespace TestNS
{{
    public class User {{ }}
    public class UserDto {{ }}

    public class Result<T>
    {{
        public static Result<T> Ok(T value) => new Result<T>();
        public static Result<T> Fail(string msg) => new Result<T>();
        public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> f) => new Result<TOut>();
        public Result<T> Ensure(Func<T, bool> predicate, string errorMessage) => new Result<T>();
        public Result<TOut> Map<TOut>(Func<T, TOut> mapper) => new Result<TOut>();
        public Result<T> Tap(Action<T> action) => this;
    }}

    public class {className}
    {{
        [ResultFlow]
        public Result<UserDto> {methodName}() => {returnExpression};
        {extraMethods}
    }}
}}";

    private static string RunGenerator(string source)
    {
        var (output, _) = RunGeneratorFull(source);
        return output;
    }

    // Returns only the pipeline constants portion — strips the Legend constant so that
    // Legend-specific content (Guard tooltip, note <br/>) doesn't trip pipeline-only assertions.
    private static string PipelineOnly(string output)
    {
        var idx = output.IndexOf("const string Legend", StringComparison.Ordinal);
        return idx >= 0 ? output[..idx] : output;
    }

    private static (string output, System.Collections.Generic.IReadOnlyList<Diagnostic> diagnostics)
        RunGeneratorFull(string source)
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

        // Collect generated source
        var sb = new System.Text.StringBuilder();
        foreach (var tree in runResult.GeneratedTrees)
        {
            using var writer = new System.IO.StringWriter();
            tree.GetText().Write(writer);
            sb.AppendLine(writer.ToString());
        }

        // Collect generator-reported diagnostics (REF001 etc.)
        var genDiagnostics = new System.Collections.Generic.List<Diagnostic>();
        foreach (var result in runResult.Results)
            genDiagnostics.AddRange(result.Diagnostics);

        return (sb.ToString(), genDiagnostics);
    }

    private static (string output, System.Collections.Generic.IReadOnlyList<Diagnostic> diagnostics)
        RunGeneratorWithConfig(string source, string configJson)
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

        var additionalText = new TestAdditionalText("resultflow.json", configJson);

        var generator = new ResultFlowGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.AddAdditionalTexts(
            System.Collections.Immutable.ImmutableArray.Create<AdditionalText>(additionalText));

        var updatedDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        var runResult = updatedDriver.GetRunResult();

        var sb = new System.Text.StringBuilder();
        foreach (var tree in runResult.GeneratedTrees)
        {
            using var writer = new System.IO.StringWriter();
            tree.GetText().Write(writer);
            sb.AppendLine(writer.ToString());
        }

        var genDiagnostics = new System.Collections.Generic.List<Diagnostic>();
        foreach (var result in runResult.Results)
            genDiagnostics.AddRange(result.Diagnostics);

        return (sb.ToString(), genDiagnostics);
    }

    private sealed class TestAdditionalText : AdditionalText
    {
        private readonly SourceText _text;

        public TestAdditionalText(string path, string content)
        {
            Path = path;
            _text = SourceText.From(content);
        }

        public override string Path { get; }

        public override SourceText? GetText(CancellationToken cancellationToken = default) => _text;
    }

    #endregion
}
