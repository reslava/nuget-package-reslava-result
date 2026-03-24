# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) guideline.

## [1.49.0] - 2026-03-24

### ✨ Added

#### VSIX v1.1.0 — WebviewPanel Renderer

- **`vscode.WebviewPanel`** — replaces sidecar `.md` + `markdown.showPreviewToSide`; diagram renders in a dedicated side panel with bundled Mermaid v10.9.5 (fully offline, no CDN, no external dependencies)
- **One-panel-per-method** — each `[ResultFlow]` method gets its own panel; rapid clicks reveal the existing panel instead of opening duplicates (`reveal(existing.viewColumn)`)
- **Node-click navigation** — clicking any diagram node (when `ResultFlowLinkMode=vscode`) navigates VS Code to that exact line in source; Windows `C:/` paths parsed right-to-left to handle drive letters correctly

#### VSIX v1.1.0 — Toolbar

- **Source panel** — collapsible; shows raw Mermaid DSL with Copy button
- **Legend panel** — collapsible; node-kind colour swatches + hover/click interaction hints
- **SVG export** — save dialog → writes raw SVG to disk
- **PNG export** — SVG cloned with explicit pixel dimensions → base64 data URL → canvas 2× scale → save dialog; CSP includes `img-src data:`

#### VSIX v1.1.0 — Theme & UX

- **Theme-aware background** — detects light/dark via `themeVariables` presence in `%%{init}%%`; page background follows `ResultFlowDefaultTheme`
- **Internal command hidden from palette** — `reslava._previewMethod` removed from `contributes.commands`; no longer appears in `Ctrl+Shift+P`

### 🔧 Fixed

- **`REslava.Result.Flow` build props** — new `build/REslava.Result.Flow.props` and `buildTransitive/REslava.Result.Flow.props` expose `ResultFlowLinkMode` + `ResultFlowDefaultTheme` as `CompilerVisibleProperty`; brings parity with `REslava.ResultFlow` which already had these
- **MermaidRenderer — Gatekeeper tooltip quote escaping** — `"` replaced with `\u201c` (`"`) and `'` with `\u2019` (`'`) in predicate tooltip text; prevents Mermaid parse errors when predicates contain quotes or apostrophes; both `REslava.Result.Flow` and `REslava.ResultFlow`

### Stats

- Tests: 4,688 passing (floor: >4,500)
- Features: 218 across 15 categories

---

## [1.48.0] - 2026-03-22

### ✨ Added

#### CodeLens — Diagram Preview (REslava.Result Extensions VSIX)

- **`src/REslava.Result.Flow.VSix/`** — new VS Code extension project (`reslava-result-extensions`); activation `onLanguage:csharp`; command `reslava._previewMethod`; TypeScript + `tsc` build
- **`ResultFlowCodeLensProvider`** — emits `▶ Open diagram preview` CodeLens above every `[ResultFlow]`-annotated method; always visible, no cursor required
- **4-step fallback chain** (`diagramResolver.ts`): (1) scan workspace for `*_Flows.g.cs` → extract Mermaid constant; (2) auto-run "Insert diagram as comment" Roslyn code action via `vscode.executeCodeActionProvider` + `applyEdit`; (3) scan existing `` /* ```mermaid...``` */ `` comment in source; (4) information message "Diagram not ready yet — try again in a moment"
- **`openMarkdownPreview`** — writes temp `.md` to `%TEMP%/REslava.ResultFlow/{Class}_{Method}.md`; opens via `markdown.showPreviewToSide`; file watcher on temp dir for Option A bridge

#### `ResultFlowDefaultTheme` MSBuild Property

- **`build_property.ResultFlowDefaultTheme`** — both generator packages read the value via `AnalyzerConfigOptionsProvider.GlobalOptions`; accepted values: `Light` (default) / `Dark` (case-insensitive); method-level `[ResultFlow(Theme = ...)]` always overrides
- **Priority chain**: method attribute → `<ResultFlowDefaultTheme>` in `Directory.Build.props` → built-in `Light` default

#### NuGet README PNG Images

- **`mermaid-to-svg.sh`** — extended to emit `.png` alongside `.svg` (`mmdc --backgroundColor transparent`); 27 PNGs generated; `svg.sh` done message updated
- **NuGet READMEs updated** — `REslava.Result` and `REslava.Result.Flow` now reference `https://raw.githubusercontent.com/.../main/images/X.png`; local SVG `<None Pack>` entries removed from both `.csproj` files

### 🧪 Tests

- **`ResultFlowDefaultThemeTests`** (×2) — 6 tests per package verifying `build_property.ResultFlowDefaultTheme = Dark` picked up; method attribute wins; case-insensitive; `TestAnalyzerConfigOptionsProvider` + `WithUpdatedAnalyzerConfigOptions` pattern; both `REslava.Result.Flow.Tests` and `REslava.ResultFlow.Tests`

### Stats
- Tests: 4,688 passing (floor: >4,500)
- Features: 200 across 15 categories

---

## [1.47.5] - 2026-03-22

### 🔧 Fixed

- **NuGet README images** — NuGet.org blocks external `raw.githubusercontent.com` URLs; `REslava.Result` and `REslava.Result.Flow` now pack the showcase SVGs locally inside the `.nupkg` (`images\` folder) and reference them with relative paths — diagrams render correctly on NuGet.org

---

## [1.47.4] - 2026-03-21

### ✨ Added

#### Dark Theme — `[ResultFlow(Theme = ResultFlowTheme.Dark)]`

- **`ResultFlowTheme` enum** — `Light = 0` (default), `Dark = 1`; `Theme` named property on `[ResultFlow]`; both packages
- **Dark `classDef` palette** — full node colour set for `operation`, `bind`, `map`, `gatekeeper`, `sideeffect`, `terminal`, `success`, `failure`, `note`, `subgraphStyle`; matched to MkDocs slate colour scheme; both packages
- **`MermaidInitDark`** — new init constant with `themeVariables`: `primaryTextColor: #fff` (front-matter title), `titleColor: #fff` (subgraph labels), `edgeLabelBackground: #2a2a2a` (edge label pills); emitted instead of `MermaidInit` when `darkTheme = true`; both packages
- **Dark aux diagrams** — `_LayerView`, `_ErrorSurface`, `_ErrorPropagation` all accept `darkTheme` and emit the dark palette; depth-indexed layer classDefs (`Layer0_Style`..`Layer4_Style`) centralized in `ResultFlowThemes.cs`; both packages
- **Layer color centralization** — `Layer{n}_Style` classDefs live entirely in `ResultFlowThemes.Light` / `ResultFlowThemes.Dark`; renderers emit only `class Layer{depth} Layer{depth}_Style` assignment lines — zero hardcoded colour strings outside the themes file; both packages
- **Depth-indexed layer subgraphs** — `LayerViewRenderer` and `ErrorPropagationRenderer` rewritten to use `Layer{depth}["ActualName"]` IDs; inner nodes keep their `NodeKind` classDef (`:::bind`, `:::gatekeeper`, etc.); 2-colour alternating palette (even = blue-lavender, odd = mint)
- **`FulfillmentService` demo** — new `[DomainBoundary("Application")]` class with `[ResultFlow(MaxDepth = 2, Theme = Dark)]` calling `WarehouseService` (Domain); generates dark `FulfillOrder`, `FulfillOrder_LayerView`, `FulfillOrder_ErrorSurface`, `FulfillOrder_ErrorPropagation`
- **MkDocs dark catalog** — `mkdocs/demo/flow-catalog-dark.md` with `force-dark-mode: true` frontmatter; `mkdocs/demo/index.md` updated with Light / Dark catalog buttons

### Stats
- Tests: 4,648 passing (floor: >4,500)
- Features: 197 across 15 categories

---

## [1.47.3] - 2026-03-20

### ✨ Added

#### SVG Single Source of Truth — All Diagram Showcases Automated

- **`scripts/svg.sh`** — new local orchestrator: builds the Demo project, exports `.mmd` files via `generate_flow_catalog.py --export-mmd`, converts to SVG via `mermaid-to-svg.sh`; SVGs committed as static assets (mmdc requires Puppeteer — not suitable for CI)
- **`generate_flow_catalog.py --export-mmd DIR`** — new export mode: writes `{ClassName}_{ConstantName}.mmd` files for Pipeline/LayerView/ErrorSurface/ErrorPropagation/Cross constants; `Legend.mmd` exported once (no class prefix); Stats/Sidecar skipped
- **Auto width detection** — `mermaid-to-svg.sh` detects `flowchart TD` → `SVGO_WIDTH=450`, else `SVGO_WIDTH=900`; `images/svgo.config.js` reads `SVGO_WIDTH` env var instead of hardcoded `'900'` — one config file, no duplication
- **All showcase diagrams replaced with SVGs** — GitHub `README.md`, NuGet READMEs (`REslava.Result`, `REslava.Result.Flow`, `REslava.ResultFlow`), MkDocs `index.md` and `resultflow/diagrams/index.md` — all non-code Mermaid showcase blocks replaced with `![alt](images/Name.svg)` image links
- **Demo project extended** — `InventoryService` (`[DomainBoundary("Infrastructure")]`, `[ResultFlow(MaxDepth = 2)]`) + `WarehouseService` (`[DomainBoundary("Domain")]`); provides variety for LayerView / ErrorSurface / ErrorPropagation diagram types beyond the existing `OrderService` scenario
- **Legend Guard tooltip** — `Legend` constant Guard node now uses `<span title='hover shows condition'>Guard</span>` label, matching real Gatekeeper nodes and making the tooltip feature self-documenting in the legend; note text updated: `⚡ = async | Guard: condition shown on hover`; both packages

### Stats
- Tests: 4,648 passing (floor: >4,500)
- Features: 197 across 15 categories

---

## [1.47.2] - 2026-03-20

### ✨ Added

#### REslava.Result.Flow + REslava.ResultFlow — Diagram Clarity & Readability

- **Async suffix strip** — node labels and diagram titles no longer repeat `Async`; the `⚡` marker already signals async, so `FindProductAsync ⚡` → `FindProduct⚡`; both packages
- **Bind vs Map visual distinction** — `Bind`/`Or`/`OrElse`/`MapError` nodes emit `:::bind` (thick dark-green border `stroke:#1a5c3c,stroke-width:3px`); `Map`/`MapAsync` nodes emit `:::map` (plain green); previously both used `:::transform`; subgraph borders still use `:::transform`; both packages
- **Title type annotation** — diagram frontmatter title now includes the output type: `PlaceOrder⚡ → ⟨Order⟩`; non-generic pipelines render `→ ⟨⟩`; `Result<T,TError>` shows only `T` (value side); both packages
- **`Legend` constant** — one `Legend` Mermaid mini-diagram per `*_Flows` class (not per method); shows all 9 node types with their shapes and colors in a horizontal row; access via `OrderService_Flows.Legend`; both packages
- **Gatekeeper predicate tooltip** — `Ensure`/`Filter` node labels wrap in `<span title='predicate'>` when the first argument is a lambda expression (e.g. `p => p.Stock > 0`); tooltip visible on hover in VS Code Markdown Preview / Mermaid.js; silently dropped on GitHub (node still renders); both packages

### Stats
- Tests: 4,648 passing (floor: >4,500)
- Features: 197 across 15 categories

---

## [1.47.1] - 2026-03-18

### ✨ Added

#### REslava.Result.Flow + REslava.ResultFlow — Diagram Title & Root Entry Node

- **Mermaid frontmatter title** — every generated `[ResultFlow]` diagram now opens with `---\ntitle: MethodName\n---`, rendering the annotated method name as a native heading above the diagram in all Mermaid-capable viewers
- **Root entry node (`ENTRY_ROOT`)** — the chain seed call (e.g. `FindUser`) is rendered as a labelled amber `:::operation` node with a thick `==>` arrow to the first pipeline step, making the pipeline entry point explicit in every diagram; async seed calls receive the `⚡` suffix
- Both `REslava.Result.Flow` (semantic) and `REslava.ResultFlow` (syntax-only) updated in parity

#### Code Action — Insert / Refresh diagram comment

- **Title + entry node** — the "Insert / Refresh `[ResultFlow]` diagram" code action now passes `methodTitle`, `seedMethodName`, `operationName`, and `correlationId` to the renderer — previously called with no arguments, so title and entry node were absent from inserted comments
- **Refresh parity** (`REslava.ResultFlow`) — code action now replaces an existing diagram comment in-place instead of inserting a duplicate; "Refresh" title shown when block already exists (matches `REslava.Result.Flow` behaviour)
- **CRLF normalisation** — comment content is normalised to `\n` before insertion, preventing a spurious blank line in the correlation block on second refresh on Windows

#### `generate_flow_catalog.py`

- **Compact verbatim string format** — catalog script now correctly parses constants in the new `@"content";` single-declaration format (previously only matched the multi-line `@"\n...\n";` format, silently producing an empty catalog)

### Stats
- Tests: 4,638 passing (floor: >4,500)
- Features: 197 across 15 categories

---

## [1.47.0] - 2026-03-18

### ✨ Added

#### Architectural Flow Catalog — `scripts/generate_flow_catalog.py`

- **`generate_flow_catalog.py`** — post-build Python script that scans `obj/Generated/**/*_Flows.g.cs` for all `[ResultFlow]` and `[DomainBoundary]` generated diagram constants and publishes them as a live MkDocs catalog page; no generator changes required
- **`mkdocs/reference/flow-catalog/index.md`** — auto-generated MkDocs page grouping every Mermaid diagram by class → method → view type (Pipeline, Layer View, Stats, Error Surface, Error Propagation); replaces hand-maintained gallery with a code-driven equivalent always in sync with the source
- **`mkdocs/demo/`** — new Demo Project MkDocs section (`index.md` + `flow-catalog.md`) presenting the `REslava.Result.Flow.Demo` as a live feature showcase; wired into nav and home page grid
- **Catalog auto-rebuild on publish** — `mkdocs-docfx.yml` builds the demo project and runs `generate_flow_catalog.py` before every MkDocs deploy; catalog is always up to date with the latest generator output
- **`_Sidecar` constants skipped** — internal correlation data excluded from the public catalog; all visual diagram types included

### Stats
- Tests: 4,638 passing (floor: >4,500)
- Features: 197 across 15 categories

---

## [1.46.3] - 2026-03-18

### ✨ Added

#### REslava.Result.Flow + REslava.ResultFlow — Subgraph Entry Arrow

- **Subgraph entry arrow** — cross-method `subgraph` blocks now open with an invisible `ENTRY_N[ ]:::entry ==>` thick arrow pointing to the first inner node, making the execution entry point immediately visible in expanded pipelines
- `classDef entry fill:none,stroke:none` is emitted once per diagram when at least one subgraph is present
- Flat (non-subgraph) pipelines are unchanged — the root amber `:::operation` node continues to serve as the visual entry indicator

### Stats
- Tests: 4,638 passing (floor: >4,500)
- Features: 197 across 15 categories

---

## [1.46.2] - 2026-03-18

### 🔧 Changed (non-breaking)

#### REslava.Result + REslava.Result.Flow NuGet READMEs

- **Markdown image syntax** — replaced HTML `<p><img /></p>` blocks (stripped by NuGet's HTML sanitizer) with standard Markdown `![alt](url)` syntax; SVG diagrams now render correctly on NuGet.org

### Stats
- Tests: 4,634 passing (floor: >4,500)
- Features: 197 across 15 categories

---

## [1.46.1] - 2026-03-18

### 🔧 Changed (non-breaking)

#### REslava.Result + REslava.Result.Flow NuGet READMEs

- **SVG diagrams replace Mermaid blocks** — NuGet.org does not render Mermaid; replaced with hosted SVG images via `raw.githubusercontent.com`. Images are clickable links to the GitHub repository
- `REslava.Result` README shows: pipeline flowchart + architecture layer view
- `REslava.Result.Flow` README shows: cross-method pipeline tracing + Match multi-branch fan-out

### Stats
- Tests: 4,634 passing (floor: >4,500)
- Features: 197 across 15 categories

---

## [1.46.0] - 2026-03-18

### ✨ Added

#### REslava.Result.Flow

- **`PipelineNode.MatchBranchLabels`** — new `IReadOnlyList<string>?` property; populated when a `Match`/`MatchAsync` invocation has explicitly-typed fail-branch lambdas (e.g. `(ValidationError v) => ...`); `Count > 0` means typed N-branch fan-out; `Count == 0` / `null` means generic 2-branch

#### REslava.Result.Flow + REslava.ResultFlow

- **Match hexagon shape** — `Match`/`MatchAsync` now renders as a Mermaid decision hexagon `{{"Match"}}:::terminal` (previously a plain rectangle)
- **Match `-->|ok| SUCCESS` + `-->|fail| FAIL` edges** — Terminal node emits both exits explicitly; `SUCCESS([success]):::success` and `FAIL([fail]):::failure` nodes are emitted by the Terminal case directly (not by the shared post-pipeline logic)

### 🔧 Changed (non-breaking)

#### REslava.Result.Flow + REslava.ResultFlow

- **Gap 1 Terminal guard** — the lambda body method-name heuristic (Gap 1) no longer overwrites the `Match` node label; previously, `Match(o => o.Id.ToString(), ...)` would rename the node to `"ToString"`

### ✨ Added — Typed N-branch (REslava.Result.Flow only)

- When `Match` has `argumentCount > 2`, `ResultFlowChainExtractor` reads each fail-branch lambda's explicit parameter type annotation from `ParenthesizedLambdaExpressionSyntax`; semantic model (`GetTypeInfo`) is tried first, raw identifier text is the fallback; result stored in `PipelineNode.MatchBranchLabels`
- `ResultFlowMermaidRenderer` emits N distinct `-->|TypeName| FAIL` edges when `MatchBranchLabels.Count > 0`; falls back to generic `-->|fail| FAIL` for plain `Result<T>` or when type info is unavailable

### Stats
- Tests: 4,634 passing (floor: >4,500)
- Features: 197 across 15 categories

---

## [1.45.0] - 2026-03-18

### ✨ Added

#### REslava.Result

- **`[DomainBoundary]` on classes** — `AttributeUsage` extended to include `AttributeTargets.Class`; annotating a class applies the layer to all its methods (method-level annotation takes priority; class-level is the fallback before namespace heuristics)

#### REslava.Result.Flow

- **`LayerDetector`** — new internal class that resolves the architectural layer of any `IMethodSymbol`; priority chain: `[DomainBoundary]` method attribute → `[DomainBoundary]` class attribute → namespace heuristics (`*.Domain.*` → Domain, `*.Application.*` / `*.UseCases.*` → Application, `*.Infrastructure.*` / `*.Repositories.*` → Infrastructure, `*.Controllers.*` / `*.Presentation.*` → Presentation)
- **`PipelineNode.Layer`** — each pipeline node now carries its detected architectural layer string (null when no layer is detected)
- **`_LayerView` constant** — `flowchart TD` diagram; groups pipeline nodes into `subgraph` blocks by Layer → Class → Method; outer layer subgraphs are color-coded via Mermaid `class` directive (Application = green, Domain = amber, Infrastructure = purple, Presentation = blue); class-level subgraphs intentionally neutral (grey) to create a clear three-tier visual hierarchy; generated only when at least one layer is detected
- **`_Stats` constant** — Markdown table summarising the pipeline: step count, async step count, all possible error types, layers crossed (ordered), and max depth traced; generated alongside `_LayerView`
- **`_ErrorSurface` constant** — `flowchart LR` diagram; renders only failure edges from the pipeline (success path stripped); lets reviewers see the complete error surface at a glance; generated alongside `_LayerView`
- **`_ErrorPropagation` constant** — `flowchart TD` diagram; one `subgraph` per detected architectural layer, one node per distinct error type originating from that layer, all edges converging on a shared `FAIL` terminal; layer subgraphs color-coded with the same palette as `_LayerView`; generated only when `_LayerView` is generated AND at least one layer has errors

### 🔧 Changed (non-breaking)

- **`_LayerView` subgraph coloring** — outer layer subgraphs now receive a `class {SubgraphId} {classDef}` Mermaid directive so the container background is colored (previously only the method nodes were colored)
- **README hero** — replaced single `flowchart LR` diagram with two contrasting diagrams: `_LayerView` (architecture, `flowchart TD`) + `_Diagram` (pipeline, `flowchart LR`)
- **MkDocs** — new `mkdocs/resultflow/diagrams/index.md` public gallery showcasing all diagram types; new grid card for Domain Boundary Diagrams in `mkdocs/resultflow/index.md`

### Stats
- Tests: ~4,680 passing (floor: >4,500)
- Features: 192 across 15 categories

---

## [1.44.1] - 2026-03-16

### 🔧 Changed (non-breaking)

- **Library branding** — tagline updated to "Visual Result pipelines for .NET"; slogan "Don't try to understand the pipeline—watch the flow."; showcase Mermaid diagram (`RegisterUserAsync` pipeline in real generator format) added above the fold in README
- **NuGet `<Description>` fields** — all 8 packages updated with feature-accurate descriptions covering typed errors, railway-oriented pipelines, pipeline visualization, Roslyn analyzers, and framework integrations
- **NuGet README opening sections** — all 8 `docs/nuget/README.*.md` rewritten with updated tagline, feature summaries, and package hierarchy; `REslava.Result.AspNetCore` README fixed (was still titled `REslava.Result.SourceGenerators`)
- **`REslava.Result.Flow` primary positioning** — README section 3 and `mkdocs/resultflow/index.md` restructured: `REslava.Result.Flow` (semantic, library-native) is now clearly the primary package; `REslava.ResultFlow` (syntax-only, library-agnostic) presented as secondary alternative
- **MkDocs updates** — `mkdocs/index.md` mirrored to match new README hero; `advanced/index.md` and `advanced/typed-pipelines/index.md` added `description` + `tagline` frontmatter; `testing/index.md` test count updated to 4,500+; `netstandard2.0` added to TFM list
- **`organize_docs.py`** — sections 27–28 (Acknowledgments, Contributors) added to community MAPPING; fixes bad filenames (`27.--acknowledgments.md`, `28.-contributors.md`) on next MkDocs script run

### Stats
- Tests: >4,500 passing (unchanged)
- 187 features across 15 categories

---

## [1.44.0] - 2026-03-15

### ✨ Added

- **`netstandard2.0` target** — `REslava.Result` now targets `netstandard2.0;net8.0;net9.0;net10.0`; broadens compatibility to .NET Framework 4.6.1+, Unity, Xamarin, and older .NET Core; fixes invisibility in the default NuGet search (framework filter mode = "all")
  - Polyfills added: `IsExternalInit`, `HashCode.Combine` (2–8 args), cross-TFM `Throw.IfNullOrEmpty`/`Throw.IfNull` helper
  - `IErrorFactory<TSelf>`, `Result.Fail<TError>`, `Result<TValue>.Fail<TError>` guarded with `#if NET7_0_OR_GREATER` — `static abstract` interface members require .NET 7+ runtime
  - `CancelAsync()` in timeout extensions guarded with `#if NET8_0_OR_GREATER`

### 🔧 Changed (non-breaking)

- **`REslava.Result` title** updated to `REslava.Result – Result Pattern for .NET | Railway-Oriented Programming` — improves NuGet search ranking (title words weighted heavily)
- **`REslava.Result` tags** extended with `reslava`, `result-pattern`, `railway-oriented-programming`, `functional-programming`

### Stats
- Tests: >4,500 passing
- 187 features across 15 categories

---

## [1.43.0] - 2026-03-15

### ✨ Added

- **`[DomainBoundary]` attribute** — new attribute in `REslava.Result` namespace; marks methods and constructors as domain boundary entry points; accepts optional `layer` string parameter for documentation (`[DomainBoundary("Application")]`)
- **RESL1030 — Domain Boundary Typed Error Crossing** — new Roslyn analyzer in `REslava.Result.Analyzers`; warns (Warning severity) when a `Result<T, TError>` is passed directly as an argument to a `[DomainBoundary]`-decorated method without calling `.MapError()` first; prevents domain-specific error surfaces from leaking across architectural layers
- **SmartEndpoints `ProducesResponseType` for `ErrorsOf<T1..Tn>`** — when a `[AutoGenerateEndpoints]` method returns `Result<T, ErrorsOf<T1..Tn>>`, the SmartEndpoints source generator now emits one `.Produces<Ti>(statusCode)` per union error type in the OpenAPI metadata; previously only the success type was emitted

#### REslava.Result.Flow + REslava.ResultFlow

- **`PipelineNode.SourceFile` / `SourceLine`** — each pipeline node now carries the source file path and 1-indexed line number of its corresponding method call in the user's source, populated from `SyntaxNode.GetLocation().GetLineSpan()`; null for in-memory compilations (empty path guard)
- **Clickable Mermaid nodes (`ResultFlowLinkMode`)** — when set to `vscode`, the renderer emits one `click {nodeId} "vscode://file/{path}:{line}" "Go to {name}"` directive per node with a known source location; Windows backslash paths normalised to forward slashes; opt-in (default `none` — existing output unchanged)
  - `REslava.Result.Flow`: configure via MSBuild `<ResultFlowLinkMode>vscode</ResultFlowLinkMode>` in `.csproj`
  - `REslava.ResultFlow`: configure via `"linkMode": "vscode"` in `resultflow.json`
- **`{MethodName}_Sidecar` constant** — always-generated companion constant alongside every diagram constant; wraps the Mermaid diagram in a `# Pipeline — {name}` heading and fenced code block; write to disk with `File.WriteAllText("{name}.ResultFlow.md", Flows.{name}_Sidecar)`

### Stats
- Tests: 4,510 passing (floor updated: >4,400 → >4,500)
- 187 features across 15 categories

---

## [1.42.0] - 2026-03-15

### ✨ Added

- **`ResultContext` sealed record** — pipeline context carrier embedded in `Result<T>`, `Result<T,TError>`, and non-generic `Result`; carries `Entity`, `EntityId`, `CorrelationId`, `OperationName`, `TenantId` (all nullable strings) through the pipeline
- **Auto-seeding** — `Result<T>.Ok(value)` and `Result<T>.Fail(...)` (both generic forms) set `Context.Entity = typeof(T).Name` automatically; no user code required
- **`WithContext(entityId, correlationId, operationName, tenantId)`** — fluent method on `Result<T>` and `Result<T,TError>` that merges runtime values into the existing `Context`; non-generic `Result.WithContext(...)` also accepts an `entity` parameter
- **Parent-wins context propagation** — all pipeline operators (`Bind`/`BindAsync`, `Ensure`/`EnsureAsync`, `Tap`/`TapAsync`/`TapOnFailure`, `Or`/`OrElse`, `MapError`) copy the incoming result's `Context` to the outgoing result; child result context is ignored
- **`Map`/`MapAsync` entity update** — derives a new `Context` from the parent but updates `Entity = typeof(TOut).Name` on success; entity unchanged on failure
- **Error auto-enrichment** — `ResultContextEnricher` (internal): when a pipeline step produces an error, injects `ResultContext` fields (`Entity`, `EntityId`, `CorrelationId`, `OperationName`, `TenantId`) as tags; non-overwriting — tags already set by the error's factory are preserved
- **`DomainTags.CorrelationId`** — new `TagKey<string>` constant for context → error tag injection
- **`DomainTags.OperationName`** — new `TagKey<string>` constant for context → error tag injection
- **`DomainTags.TenantId`** — new `TagKey<string>` constant for context → error tag injection
- **Typed pipeline propagation** — same parent-wins rules applied to all 7 `Bind` overloads, `Map`, and 7+7 `Ensure`/`EnsureAsync` overloads on `Result<T,TError>`

#### REslava.Result.Flow

- **`WithContext` — Invisible node** — `.WithContext(...)` classified as `NodeKind.Invisible` in the chain extractor; rendered as transparent in the pipeline diagram
- **`TryExtractContextHints`** — new static method on `ResultFlowChainExtractor`; scans method body for `.WithContext(operationName:..., correlationId:...)` literal string arguments
- **Mermaid context footer** — `ResultFlowMermaidRenderer.Render` emits a `%%` footer comment block when context hints are found, documenting `OperationName` and `CorrelationId` values

#### REslava.ResultFlow

- **`WithContext` — Invisible node** — same classification as `REslava.Result.Flow` for consistency

#### REslava.Result.OpenTelemetry (new package)

- **`.WithOpenTelemetry()`** — seeds `ResultContext.CorrelationId` from `Activity.Current.TraceId` and `OperationName` from `Activity.Current.DisplayName`; no-op when no active span; available on `Result<T>`, `Result`, `Result<T,TError>`
- **`.WriteErrorTagsToSpan()`** — on failure, writes all error tags as key-value attributes on `Activity.Current`; passes through unchanged on success or when no active span; available on all three result types

### 🔧 Changed (non-breaking)

- **`FluentValidateExtensionGenerator`** — generated code now emits `ValidationError.Field(fieldName, message)` instead of the deprecated 2-arg constructor
- **`ValidateExtensionGenerator`** — same fix; generated `.Validate()` extension now uses `ValidationError.Field(...)` for field-specific errors

### Stats
- Tests: >4,400 passing (same floor — no new hundred crossed)
- 182 features across 15 categories

---

## [1.41.0] - 2026-03-15

### ✨ Added

- **`TagKey<T>`** — `abstract record TagKey(string Name)` + `sealed record TagKey<T>(string Name) : TagKey(Name)`; typed accessor into `ImmutableDictionary<string, object>` Tags; record equality and value semantics; abstract base enables non-generic storage and enumeration
- **`DomainTags`** — static class with predefined typed domain tag keys: `Entity` (`TagKey<string>`), `EntityId` (`TagKey<object>`), `Field` (`TagKey<string>`), `Value` (`TagKey<object?>`), `Operation` (`TagKey<string>`); used by `NotFoundError`, `ConflictError`, `ValidationError` auto-tags
- **`SystemTags`** — static class with predefined typed integration tag keys: `HttpStatus` (`TagKey<int>`), `ErrorCode` (`TagKey<string>`), `RetryAfter` (`TagKey<int>`), `Service` (`TagKey<string>`); shared contract for Http/AspNetCore packages
- **`WithTag<T>(TagKey<T> key, T value)`** typed overload on `Reason<TReason>` — writes `key.Name` as the string dictionary key; null guard at entry; `Metadata` preserved on CRTP copies
- **`ReasonTagExtensions`** — `TryGet<T>(this IReason, TagKey<T>, out T?)` and `Has<T>(this IReason, TagKey<T>)` typed reads on any `IReason`; safe cast via `is T`; `null` for type mismatch (returns `false`, not exception)
- **`IErrorFactory<TSelf>`** — C# 11 static abstract interface; `static abstract TSelf Create(string message)`; enables type-parameterized error creation without reflection
- **`IErrorFactory<TSelf>` on built-in errors** — `Error`, `NotFoundError`, `ConflictError`, `ValidationError`, `ForbiddenError`, `UnauthorizedError` implement `IErrorFactory<TSelf>`; `ExceptionError` and `ConversionError` excluded (incompatible constructor signatures)
- **`Result.Fail<TError>(string message)`** on both `Result` and `Result<TValue>` — dual constraint `where TError : IError, IErrorFactory<TError>`; delegates to `TError.Create(message)` then to existing `Fail(IError)` overload; `Result<TValue>.Fail<TError>` uses `new` keyword to shadow base
- **`ReasonMetadata.PipelineStep`** (`string?`) — name of the pipeline step that produced the error, for runtime → diagram correlation
- **`ReasonMetadata.NodeId`** (`string?`) — stable node identity matching the diagram node (e.g. `"N0_FindUser"`); emitted by `REslava.Result.Flow` renderer

#### REslava.Result.Flow

- **Gap 1 — lambda body method name** — `TryGetLambdaBodyMethodName()` extracts the inner method name from single-expression lambda arguments: `.Bind(x => SaveUser(x))` now renders step label `"SaveUser"` instead of `"Bind"` in generated Mermaid diagrams
- **Gap 3 — variable initializer resolution** — `ResolveVariableInitializer()` traces a local identifier to its `EqualsValueClauseSyntax`; `var r = FindUser(); return r.Bind(...)` now correctly seeds the chain root from `FindUser`
- **`PipelineNode.NodeId`** (`string?`) — stable node identifier assigned by `ResultFlowMermaidRenderer` before the render loop (`"N{i}_{MethodName}"`); surfaced via `ReasonMetadata.NodeId` for runtime → diagram correlation
- **Mermaid node correlation block** — `%% --- Node correlation (ReasonMetadata.NodeId / PipelineStep) ---` comment block emitted at the end of every generated diagram; lists all visible nodes with their stable `NodeId`

### Stats
- Tests: same floor (>4,300) — no new tests crossed the next hundred
- 169 features across 13 categories

---

## [1.40.0] - 2026-03-14

### ✨ Added

- **`ReasonMetadata` sealed record** — structured system/diagnostic metadata separate from `Tags`; captures `CallerMember`, `CallerFile`, `CallerLine` via compiler-injected `[CallerMemberName/FilePath/LineNumber]` on factory methods; `Empty` singleton for zero-allocation default; `FromCaller()` internal factory
- **`IReasonMetadata` capability interface** — secondary interface (same opt-in pattern as `IAsyncDisposable`); allows reading `Metadata` from an `IReason`-typed reference without breaking existing external implementations; `Reason` base class implements it automatically
- **`ReasonMetadataExtensions`** — `TryGetMetadata(this IReason)` → `ReasonMetadata?` (null-safe, no cast exception for external stubs); `HasCallerInfo(this IReason)` → `bool` using C# 9 property pattern
- **`Reason.Metadata`** property (`internal set`) — all CRTP operations (`WithMessage`, `WithTag`, `WithTags`, `WithTagsFrom`) preserve `Metadata` on copies; `WithMetadata(ReasonMetadata)` fluent override
- **Static error factories with `[CallerMemberName]` capture**:
  - `ValidationError.Field(fieldName, message)` — replaces old `[Obsolete]` `(string fieldName, string message)` constructor
  - `ForbiddenError.For(action, resource)` — replaces old `[Obsolete]` `(string action, string resource)` constructor
  - `ConflictError.Duplicate(entity, field, value)` — replaces old `[Obsolete]` `(string entityName, string conflictField, object conflictValue)` constructor
  - `ConflictError.Duplicate<T>(field, value)` — entity name inferred from `typeof(T).Name`
  - Single-string constructors on all types capture `CallerMember` directly via `[CallerMemberName]` optional parameters
- **JSON serialization** — `ReasonJsonConverter` writes `"metadata"` key when `Metadata != Empty`; reads it back on deserialization; backward-compatible (missing key → `Empty`); `WithMetadata()` call after `new Error(message)` in `ReadError()` correctly overrides the auto-captured `"ReadError"` value
- **`RESL1010` — Unhandled Failure Path** — warns when a `Result<T>` local variable has no failure-aware usage in the enclosing block and is not returned; suppressed by any of `IsSuccess/IsFailure`, `Match`, `Switch`, `TapOnFailure`, `Bind`, `Map`, `Ensure`, `GetValueOr`, `TryGetValue`, `Or`, `OrElse`, `MapError` (or return of the variable)
- **`RESL2002` — Non-Exhaustive `ErrorsOf.Match()`** — warns when `Match()` is called with fewer handler lambdas than the `ErrorsOf<T1..Tn>` union has type arguments; runtime `InvalidOperationException` is prevented at compile time
- **`RESL1021` — Multi-Argument `IError`/`IReason` Constructor** — warns when an implementation has a public constructor with 2+ required non-optional parameters; allowed shapes: `()`, `(string)`, `(string, Exception)`, `[Obsolete]`-marked, non-public; encourages static factory pattern for correct `[CallerMemberName]` capture
- **ResultFlow `PipelineNode.ErrorHint`** — syntactically extracted error type name for body-scan (`Result<T>`) pipelines; set when a step argument is `new SomeError(...)` or `SomeError.Factory(...)` (receiver name ends with `"Error"` or `"Reason"`); used as fallback in `FailLabel()` when `ErrorType` is null (type-read mode still takes precedence)

### 🔧 Changed (non-breaking)

- Old multi-parameter constructors on `ValidationError`, `ForbiddenError`, `ConflictError`, `UnauthorizedError`, `ConversionError` marked `[Obsolete]` — no runtime behavior change, callers see a deprecation warning
- `Reason.cs` — abstract base class now declares `public abstract class Reason : IReason, IReasonMetadata`
- `ResultFlowMermaidRenderer.FailLabel()` — now uses `ErrorHint` as body-scan fallback; `ErrorType` from type-read mode unchanged and still takes precedence

### Stats
- 4,328 tests passing across net8.0, net9.0, net10.0 (1,306×3) + AspNetCore (131) + ResultFlow (58) + analyzer (114) + FluentValidation (26) + Http (20×3)
- 145 features across 13 categories

---

## [1.39.1] - 2026-03-11

Minor update: Fixed and updated NuGet package `REslava.Result` README 

## [1.39.0] - 2026-03-10

### ⚠️ Breaking Changes
- **`OneOf<T1..T8>` — `readonly struct` → `sealed class`** — copy semantics become reference semantics; `default(OneOf<T1,T2>)` returns `null` (was zeroed struct). Nullable reference types (already enabled) flag every unsafe callsite. Extremely rare to depend on copy semantics in practice.

### ✨ Added
- **`OneOf<T1..T7>` and `OneOf<T1..T8>`** — two new arities for full arity symmetry alongside the existing T2–T6 types
- **`OneOfBase<T1..T8>`** — new unconstrained abstract class holding all shared dispatch logic (`IsT1..T8`, `AsT1..T8`, `Match`, `Switch`, `Equals`, `GetHashCode`, `ToString`); `OneOf` and `ErrorsOf` both inherit it — dispatch logic written once
- **`IOneOf<T1..T8>`** — new shared interface implemented by both `OneOf<>` and `ErrorsOf<>`; enables generic programming over any discriminated union
- **`ErrorsOf<T1..T8>`** — new error union type; `where Ti : IError` constraint on all type parameters; implements `IError` itself (delegates `Message`/`Tags` to the active case); implicit conversions from each `Ti`; factory methods `FromT1..FromT8`; inherits `OneOfBase` shared dispatch
- **`Result<TValue, TError> where TError : IError`** — new typed result type; factory `Ok(value)` / `Fail(error)`; `IsSuccess`, `IsFailure`, `Value` (throws on failure), `Error` (throws on success)
- **`Bind` ×7 — typed pipeline** — grows the error union one slot per step: `Result<TIn,T1>.Bind(f) → Result<TOut, ErrorsOf<T1,T2>>` through 7→8 slot; the normalization trick (each step normalizes via implicit conversion) keeps the overload count O(n) not combinatorial
- **`Map` — typed pipeline** — transforms the success value; error type unchanged; single generic overload
- **`Tap` / `TapOnFailure` — typed pipeline** — side effects on success / failure; original result returned unchanged
- **`Ensure` ×7 — typed pipeline** — guard conditions that widen the error union by one slot when the predicate fails; same growth pattern as `Bind`; eagerly evaluates the error argument
- **`EnsureAsync` ×7 — typed pipeline** — async variant of `Ensure`; predicate is `Func<TValue, Task<bool>>`; result itself evaluated synchronously
- **`MapError` — typed pipeline** — translates the error surface via `Func<TErrorIn, TErrorOut>`; use at layer boundaries to collapse unions or adapt to a different error vocabulary; success forwarded unchanged
- **`Result.Flow` — type-read mode** — when a `[ResultFlow]`-annotated method returns `Result<T, TError>`, failure edges in the generated Mermaid diagram now show the exact error type (e.g. `fail: ErrorsOf<ValidationError, InventoryError>`); reads `TypeArguments[1]` from the Roslyn return type symbol — zero body scanning; body-scan mode for `Result<T>` is unchanged
- **Sample 17** — end-to-end typed checkout pipeline with exhaustive `Match` at callsite over `ErrorsOf<ValidationError, InventoryError, PaymentError, DatabaseError>`

### Stats
- 4,198 tests passing across net8.0, net9.0, net10.0 (1,280×3) + generator AspNetCore (131) + Result.Flow (22) + ResultFlow (40) + analyzer (79) + FluentValidation bridge (26) + Http (20×3)
- 153 features across 13 categories

---

## [1.38.1] - 2026-03-09

### 🐛 Fixed
- **`REslava.Result.Flow` chain walker bug** — `IInvocationOperation.Instance` traversal stopped after the first node for two patterns: (1) chains starting with `Result<T>.Ok(...)` (static call — `Instance` is null for static methods), and (2) async chains on `Task<Result<T>>` (`*Async` extension methods). Fixed by replacing `Instance` traversal with a **syntax-walk + per-node `semanticModel.GetOperation()`** approach — reliably captures all steps regardless of calling convention.

### ✨ Added
- **`REslava.ResultFlow` — ` ```mermaid ` fence in code action** — the "Insert diagram as comment" code action (REF002) now wraps the inserted Mermaid diagram in a `` ```mermaid `` / ` ``` ` fence. The diagram renders inline in VS Code, GitHub, Rider, and any other Markdown-aware IDE. The `[ResultFlow] Pipeline: {name}` header line is removed (method name is already visible below the comment).
- **`REslava.Result.Flow` — REF002 analyzer + "Insert diagram as comment" code action** — the native companion package now emits the same REF002 diagnostic on every `[ResultFlow]` method whose chain is detectable. Accepting the code action inserts a full-fidelity Mermaid comment (type travel + typed error edges) with the `` ```mermaid `` fence — matching the source-generated `const string` output exactly.
- **3 regression tests** in `REslava.Result.Flow.Tests` — guard against recurrence of the chain walker bug for Ensure×3 chains, `Result<T>.Ok(...)` roots, and multi-step mixed chains.
- **Pre-inserted Mermaid diagrams** in both sample projects (`samples/resultflow/Program.cs` — 7 methods; `samples/REslava.Result.Flow.Demo/Program.cs` — 5 methods) — each `[ResultFlow]` method ships with its diagram as an IDE-previewable comment.

### Stats
- 3,986 tests passing across net8.0, net9.0, net10.0 (1,216×3) + generator AspNetCore (131) + Result.Flow (22) + ResultFlow (40) + analyzer (79) + FluentValidation bridge (26) + Http (20×3)

---

## [1.38.0] - 2026-03-08

### ✨ Added
- **`RESL1009` analyzer + code fix** — detects `try/catch` blocks that produce a `Result<T>` and offers a one-click migration to `Result<T>.Try(...)` / `Result<T>.TryAsync(...)`. Reduces boilerplate and enforces the railway-oriented pattern.
- **RF-1 — Async step annotation (⚡)** — `REslava.ResultFlow` source generator now appends ⚡ to any pipeline step resolved via `await`. Annotation appears inline in the Mermaid node label (e.g. `"EnsureAsync ⚡<br/>User"`) — zero configuration required.
- **RF-2 — Success type travel** — `REslava.ResultFlow` source generator now infers the success type `T` of each pipeline step using generic Roslyn type extraction (no `IResultBase`, no `IError` — works with any Result library). Type-preserving steps show `"MethodName<br/>T"`; type-changing steps show `"MethodName<br/>T → U"`. Falls back to method-name-only when the return type is non-generic or unresolvable.
- **RF-3 — `REslava.Result.Flow` native companion package** — new `REslava.Result.Flow` package extends pipeline visualisation with REslava.Result-specific semantics: uses `IResultBase` and `IError` as Roslyn anchors to infer both success types and error surfaces, and annotates typed failure edges in the Mermaid diagram.

### Stats
- 3,983 tests passing across net8.0, net9.0, net10.0 (1,216×3) + generator (131) + ResultFlow (39) + analyzer (79) + FluentValidation bridge (26) + Http (20×3)
- 140 features across 13 categories

---

## [1.37.0] - 2026-03-07

### ✨ Added
- **`Switch()` / `SwitchAsync()`** — void side-effect dispatch on `Result` and `Result<T>`; routes success/failure to two actions without returning a value; explicit intent signal for void branching (standard name in LanguageExt/OneOf); `Task<Result>` / `Task<Result<T>>` extensions enable clean end-of-chain dispatch after async pipelines — filling a gap that `void Match` extensions do not cover.
- **`MapError()` / `MapErrorAsync()`** — transforms errors in the failure path; symmetric counterpart to `Map`; success passes through unchanged; result state (IsSuccess/IsFailure) never changes; async overload accepts `Func<ImmutableList<IError>, Task<ImmutableList<IError>>>`;  `Task<Result>` / `Task<Result<T>>` extensions with sync and async mapper overloads.
- **`Or()` / `OrElse()` / `OrElseAsync()`** — fallback result on failure; simpler API than `Recover`; `Or(fallback)` is eager, `OrElse(factory)` is lazy and receives the full error list; fallback can itself be a failure; `Task<Result>` / `Task<Result<T>>` extensions for all three variants.
- **`ResultFlowChainExtractor` updated** — `MapError`/`MapErrorAsync` → `SideEffectFailure`; `Or`/`OrElse`/`OrElseAsync` → `TransformWithRisk`.

### Stats
- 3,960 tests passing across net8.0, net9.0, net10.0 (1,216×3) + generator (131) + ResultFlow (27) + analyzer (68) + FluentValidation bridge (26) + Http (20×3)
- 136 features across 13 categories

---

## [1.36.0] - 2026-03-02

### ✨ Added
- **`REslava.ResultFlow` package** — standalone library-agnostic pipeline visualizer, independent of `REslava.Result`. Contains `[ResultFlow]` source generator, `REF002` analyzer, and "Insert diagram as comment" code action. Works with any fluent Result library. Target: `using REslava.ResultFlow;`.
- **`REF002` analyzer + Code Action** — Info diagnostic on every `[ResultFlow]` method with a detectable chain. Single-click code action inserts the Mermaid pipeline diagram as a `/* ... */` block comment directly above the method, respecting existing indentation. No build required.
- **Convention dictionary expansion** — built-in support for **ErrorOr** (`Then`/`ThenAsync`, `Switch`/`SwitchAsync`) and **LanguageExt** (`Filter`, `Do`/`DoAsync`, `DoLeft`/`DoLeftAsync`) alongside REslava.Result — zero configuration.
- **`resultflow.json` AdditionalFile config** — escape hatch for custom or third-party libraries. Config entries **override** the built-in dictionary. Supported keys: `bind`, `map`, `tap`, `tapOnFailure`, `gatekeeper`, `terminal`. REF003 Warning emitted on malformed JSON (fallback to convention).

### ⚠️ Breaking Changes
- **`[ResultFlow]` attribute namespace changed**: `REslava.Result.SourceGenerators` → `REslava.ResultFlow`. Update `using REslava.Result.SourceGenerators;` to `using REslava.ResultFlow;` and replace the `REslava.Result.SourceGenerators` package reference with `REslava.ResultFlow`.
- **`REslava.Result.SourceGenerators` package renamed** to `REslava.Result.AspNetCore`. No stub package — update your reference directly.

### 📚 Documentation
- NuGet README for `REslava.ResultFlow` expanded with Installation, Supported Libraries table, `resultflow.json` config, Code Action description, Diagnostics
- `mkdocs/resultflow/` new top-level section with `index.md`
- `mkdocs/reference/features/index.md`: ResultFlow section expanded to 10 features (total 133 across 13 categories)
- README: section 16.4 expanded with Standalone Package, Code Action, Supported Libraries, `resultflow.json` subsections
- Acknowledgments: ErrorOr (Amanti Band) and LanguageExt (Paul Louth) added

### Stats
- 3,783 tests passing across net8.0, net9.0, net10.0 (1,157×3) + generator (143) + ResultFlow (27) + analyzer (68) + FluentValidation bridge (26) + Http (20×3)
- 133 features across 13 categories

---

## [1.35.0] - 2026-03-02

### ✨ Added
- **`[ResultFlow]` source generator** — decorate any fluent `Result<T>` pipeline method with `[ResultFlow]`; a Mermaid `flowchart LR` diagram is emitted as a `public const string` in `Generated.ResultFlow.{ClassName}_Flows` at compile time — zero runtime overhead, zero manual maintenance. Supports 9 operation semantics: `Ensure`/`EnsureAsync` (gatekeeper with `|fail|` edge), `Bind`/`BindAsync` (transform with risk), `Map`/`MapAsync` (pure transform), `Tap`/`TapAsync` (side effect — success), `TapOnFailure`/`TapOnFailureAsync` (side effect — failure), `TapBoth` (side effect — both), `Match` (terminal, no outbound edges), `WithSuccess`/`WithError` (invisible — traversed, not rendered). REF001 info diagnostic emitted when method body cannot be parsed as a fluent chain.

### 📚 Documentation
- README: `### 🗺️ Pipeline Visualization — [ResultFlow]` section with Mermaid diagram, color legend, and generated output example
- `mkdocs/index.md`: comparison table row + teaser admonition
- `mkdocs/reference/features/index.md`: section 10 — ResultFlow (5 features, total 128 across 13 categories)

### Stats
- 3,768 tests passing across net8.0, net9.0, net10.0 (1,157×3) + generator (143) + analyzer (68) + FluentValidation bridge (26) + Http (20×3)
- 128 features across 13 categories

---

## [1.34.0] - 2026-03-02

### ⚠️ Breaking Changes
- **`IResultResponse<T>` renamed to `IResultBase<T>`** — `IResultResponse` carried an unintended HTTP connotation; `IResultBase` is the semantically correct name — it is the base contract for all `Result` types, not an HTTP response. If you reference `IResultResponse<T>` directly in your code, update to `IResultBase<T>`.

### 📚 Documentation
- Fill documentation gaps: Http extensions usage, generator setup guide, `ConversionError` documented in error type table

### Stats
- 3,756 tests passing (unchanged from v1.33.0)
- 123 features across 12 categories (unchanged)

---

## [1.33.0] - 2026-03-01

### ✨ Added
- **`REslava.Result.Http`** — new 5th NuGet package; wraps `HttpClient` calls so every HTTP response and network failure becomes a typed `Result<T>`; public API: `GetResult<T>(string/Uri)`, `PostResult<TBody, TResponse>`, `PutResult<TBody, TResponse>`, `DeleteResult`, `DeleteResult<T>`; configurable via `HttpResultOptions` (custom `JsonSerializerOptions`, custom `StatusCodeMapper`); default mapper: `NotFoundError`, `UnauthorizedError`, `ForbiddenError`, `ConflictError`, `ValidationError`, generic `Error` for all other 4xx/5xx; network failures (`HttpRequestException`) and timeouts (`OperationCanceledException`) wrapped in `ExceptionError`; user-initiated cancellation re-thrown; no extra NuGet dependencies on net8+

### 📚 Documentation
- README/TOC full restructure — 26 sections with improved logical hierarchy and sub-section grouping
- MkDocs site restructure — sub-folder grid-card navigation, `architecture/source-generators/` sub-folder, orphan detection script (`scripts/find_orphans.py`), pipeline script fixes

### Stats
- 3,756 tests passing across net8.0, net9.0, net10.0 (1,157×3) + generator (131) + analyzer (68) + FluentValidation bridge (26) + Http (20×3)
- 123 features across 12 categories

---

## [1.32.0] - 2026-02-28

### ✨ Added
- **`Result.Validate(r1, r2, ..., mapper)`** — applicative validation; runs 2/3/4 independent `Result<T>` validations simultaneously; accumulates ALL errors without short-circuiting; maps heterogeneous success values to `Result<TResult>` via typed mapper func; mapper is only invoked when all inputs succeed
- **`Result<T>.Deconstruct()`** — C# 8+ tuple unpacking; `var (value, errors) = result` (2-component) and `var (isSuccess, value, errors) = result` (3-component) for `Result<T>`; `var (isSuccess, errors) = result` for non-generic `Result`; value is `default` when `IsFailure`
- **`Maybe<T>` ↔ `Result<T>` interop** — `maybe.ToResult(Func<IError>)`, `maybe.ToResult(IError)`, `maybe.ToResult(string)` bridge `None` to typed failure; `result.ToMaybe()` converts success to `Some(value)` and failure to `None` (error info intentionally discarded)

### Stats
- 3,696 tests passing across net8.0, net9.0, net10.0 (1,157×3) + generator (131) + analyzer (68) + FluentValidation bridge (26)

---

## [1.31.0] - 2026-02-28

### ✨ Added
- **`Result.WithLogger(ILogger, string)`** / **`LogOnFailure(ILogger, string)`** — Tap-style ILogger integration; logs Debug on success, Warning on domain failure (non-`ExceptionError`), Error on `ExceptionError`; structured log properties: `result.outcome`, `result.error.type`, `result.error.message`; `Task<Result<T>>` and `Task<Result>` extensions with `CancellationToken`
- **`Result.Recover()`** / **`RecoverAsync()`** — railway recovery; transforms any failure into a new `Result<T>` (success or failure) via `Func<ImmutableList<IError>, Result<T>>`; error list passed to recovery func enables context-aware branching (e.g. skip recovery on `ForbiddenError`); non-generic `Result` variant included; `Task<Result<T>>` extensions with `CancellationToken`
- **`Result.Filter()`** / **`FilterAsync()`** — converts success to failure when a predicate fails; `Func<T, IError>` error factory enables value-dependent contextual messages (e.g. `"User 'John' is not active"`); 3 sync overloads: factory / static `IError` / string message; async predicate variant (`Func<T, Task<bool>>`); `Task<Result<T>>` extensions; predicate exceptions wrapped in `ExceptionError`

### Stats
- 3,591 tests passing across net8.0, net9.0, net10.0 (1,122×3) + generator (131) + analyzer (68) + FluentValidation bridge (26)

---

## [1.30.0] - 2026-02-27

### ✨ Added
- **`Result.Catch<TException>()`** / **`CatchAsync<TException>()`** — inline typed exception handler in the railway pipeline; if the result contains an `ExceptionError` wrapping `TException`, replaces it with the error returned by the handler (in-place, preserving position in the reasons list); `Task<Result<T>>` extension also catches `TException` thrown directly from the source task
- **`Result.WithActivity(Activity?)`** — Tap-style extension that enriches an existing OTel `Activity` span with result outcome metadata: `result.outcome` (`"success"` / `"failure"`), `result.error.type`, `result.error.message`, `result.error.count` (when > 1 error); sets `ActivityStatusCode.Ok` / `ActivityStatusCode.Error`; null-safe (no-op when activity is null); no new NuGet dependency — uses BCL `System.Diagnostics.Activity`

### Stats
- 3,432 tests passing across net8.0, net9.0, net10.0 (1,069×3) + generator (131) + analyzer (68) + FluentValidation bridge (26)

---

## [1.29.0] - 2026-02-25

### ⚠️ Breaking Changes
- **`IsFailed` renamed to `IsFailure`** — `IsSuccess` / `IsFailure` is the correct symmetric pair. `IsFailed` was past-tense verb form (semantically incorrect for a boolean property). No alias or `[Obsolete]` shim — update call sites directly.

### ✨ Added
- **Console samples — 3 new examples** covering v1.27.0–v1.28.0 features:
  - `14_ValidationDSL.cs` — all 19 native DSL rules with real-world order validator
  - `15_OneOf5_OneOf6.cs` — 5/6-way unions, chain extensions (up/down conversions), checkout pipeline
  - `16_AsyncPatterns_Advanced.cs` — WhenAll, Retry (backoff), Timeout, TapOnFailure, OkIf/FailIf, Try/TryAsync
- **FastMinimalAPI validation showcase** — side-by-side comparison of all three validation approaches:
  - `/api/smart/validation` — DataAnnotations + `[Validate]` auto-guard vs. native Validation DSL
  - `/api/smart/fluent-validation` — `[FluentValidate]` migration bridge demo (optional)
- **FastMvcAPI validation parity** — explicit `request.Validate()` guard + `CancellationToken` in MVC controllers
- **Feature Reference page** — `mkdocs/reference/features/index.md` — 109 features across 11 categories

### 🔧 Fixed
- **release.yml** — `REslava.Result.FluentValidation` added to Build and Pack steps (was missing; caused v1.28.0 NuGet package to lack its embedded README)

### Stats
- 3,339 tests passing across net8.0, net9.0, net10.0 + generator (131) + analyzer (68) + FluentValidation bridge (26)

---

## [1.28.0] - 2026-02-25

### ✨ Added
- **REslava.Result.FluentValidation** — new 4th NuGet package (generator-only, `DevelopmentDependency=true`); `[FluentValidate]` attribute emits `.Validate(IValidator<T> validator)` + `.ValidateAsync(IValidator<T>, CancellationToken)` extension methods per decorated type; bridge for teams migrating from FluentValidation who want `Result<T>` and SmartEndpoints integration without rewriting existing validators
- **SmartEndpoints FluentValidation injection** — when a POST/PUT body parameter type is decorated with `[FluentValidate]`, the generated lambda automatically adds `IValidator<T>` as a DI-injected parameter and emits the `.Validate(validator)` guard block; adds `using FluentValidation;` and `using Generated.FluentValidationExtensions;` conditionally
- **RESL1006 analyzer** — compile-error diagnostic when both `[Validate]` and `[FluentValidate]` are applied to the same type; prevents conflicting `.Validate()` extension method signatures

### Stats
- 3,339 tests passing across net8.0, net9.0, net10.0 + generator (131) + analyzer (68) + FluentValidation bridge (26)

---

## [1.27.0] - 2026-02-25

### ✨ Added
- **CancellationToken Support in SmartEndpoints** — generated endpoint lambdas detect `CancellationToken cancellationToken = default` in service method signatures and inject it as an endpoint parameter; service methods remain opt-in; fully backward-compatible
- **OneOf5 / OneOf6** — new `OneOf<T1..T5>` and `OneOf<T1..T6>` readonly structs with full `Match`, `Switch`, `MapT*`, `BindT*`, equality, `GetHashCode`, `ToString`, and implicit conversions; matching the same API surface as OneOf2–OneOf4
- **OneOf chain extensions** — complete arity chain in `OneOfExtensions`: `ToFourWay`, `ToFiveWay`, `ToSixWay` up-conversions (anchors the new type via a `defaultValue` parameter) plus nullable, mapper, and `WithFallback` down-conversions across the full 2↔3↔4↔5↔6 chain
- **Native Validation DSL** — 19 fluent extension methods on `ValidatorRuleBuilder<T>` via `ValidatorRuleBuilderExtensions`; `Expression<Func<T, TProperty>>` selectors auto-infer property names for default error messages:
  - **String**: `NotEmpty`, `NotWhiteSpace`, `MinLength`, `MaxLength`, `Length`, `EmailAddress`, `Matches`, `StartsWith`, `EndsWith`, `Contains`
  - **Numeric** (generic `where TNum : struct, IComparable<TNum>`): `GreaterThan`, `LessThan`, `Range`, `Positive`, `NonNegative` — work for `int`, `long`, `double`, `decimal`, etc.
  - **Collection**: `NotEmpty<T,TItem>`, `MinCount`, `MaxCount`
  - **Reference**: `NotNull`
- **DocFX API Reference** — all public types, members, and XML documentation now fully surfaced in the hosted API reference at `/reference/api/`

### 🔧 Fixed
- **OneOf4 bug fixes** — 10+ edge-case fixes across `AsT*` guard behaviour, `MapT*` propagation, `BindT*` null handling
- **OneOf5/6 source generators** — added `OneOf5ToIResultGenerator`, `OneOf6ToIResultGenerator`, `OneOf5ToActionResultGenerator`, `OneOf6ToActionResultGenerator`

### Stats
- 3,313 tests passing across net8.0, net9.0, net10.0 + generator (131) + analyzer (68)

---

## [1.26.0] - 2026-02-24

### ✨ Added
- **RESL1005 analyzer** — Info-level diagnostic that suggests a domain-specific error type (`NotFoundError`, `ConflictError`, `UnauthorizedError`, `ForbiddenError`, `ValidationError`) when `new Error("...")` is used with a message that implies a well-known HTTP error category. Helps developers discover domain errors that carry automatic HTTP status context and integrate with `ToIResult()`.
- **SmartEndpoints: Auto-Validation** — when a method's body parameter type is decorated with `[Validate]`, the generated endpoint lambda automatically calls `.Validate()` and returns early with the validation result before invoking the service. Requires no attribute on the method — decoration on the type is the only signal. Adds `using Generated.ValidationExtensions;` conditionally.

### Stats
- 2,862 tests passing across net8.0, net9.0, net10.0 + generator (106) + analyzer (68) tests

## [1.25.0] - 2026-02-24

### ✨ Added
- **Documentation website** — MkDocs Material site auto-generated from `README.md` on every push; 8 nav sections, dark/light mode, search, social cards, git revision dates
- **DocFX API reference landing page** — Bootstrap card grid with namespace cards, Core Types at a Glance, and quick-links to docs/GitHub/NuGet; deployed at `/reference/api/`
- **CI path filtering** — CI workflow now uses allowlist (`src/**`, `tests/**`) instead of denylist; docs-only commits no longer trigger the test suite

### 🔧 Fixed
- `organize_docs.py`: `reference/api-docs` path typo corrected to `reference/api-doc`
- Docs workflow trigger: added `docfx/**` path and corrected self-reference from `mkdocs.yml` to `mkdocs-docfx.yml`
- MkDocs Reference index: replaced copy-pasted placeholder descriptions with accurate content per card

### Stats
- 2,843 tests passing across net8.0, net9.0, net10.0 + generator + analyzer tests

## [1.24.0] - 2026-02-23

### ✨ Added
- `[Validate]` source generator — decorate any record/class to get a `.Validate()` extension method returning `Result<T>`; delegates to `Validator.TryValidateObject` so all 20+ `DataAnnotations` types work automatically; invalid fields surface as `ValidationError` with `FieldName` populated; composable with `.Bind()` / `.ToIResult()` / `.ToActionResult()`

### Stats
- 2,843 tests passing across net8.0, net9.0, net10.0 + generator + analyzer tests

## [1.23.0] - 2026-02-23

### ✨ Added
- **SmartEndpoints: Endpoint Filters** — new `[SmartFilter(typeof(T))]` attribute (`AllowMultiple = true`) generates `.AddEndpointFilter<T>()` for each filter in declaration order
- **SmartEndpoints: Output Caching** — `CacheSeconds` property on `[AutoGenerateEndpoints]` (class default) and `[AutoMapEndpoint]` (method override); generates `.CacheOutput(x => x.Expire(...))` only for GET endpoints; `-1` = explicit opt-out
- **SmartEndpoints: Rate Limiting** — `RateLimitPolicy` property on both attribute levels; generates `.RequireRateLimiting("policy")`; `"none"` = explicit opt-out; method value overrides class default
- **FastMinimalAPI Demo: SmartCatalogController** — showcases all three features with `LoggingEndpointFilter`, output cache + rate limiter middleware registered in `Program.cs`
- **scripts/validate-release.sh** — pre-release validation checklist (9 checks: version, CHANGELOG, release notes file, README roadmap/history, tests, git state, TODO check, test count accuracy)
- **11 new source generator tests** — `SmartEndpoints_FiltersAndCachingTests.cs`

### Stats
- 2,836 tests passing across net8.0, net9.0, net10.0 + generator + analyzer tests

## [1.22.0] - 2026-02-18

### ✨ Added
- **OneOfToActionResult source generator** — `ToActionResult()` extension methods for `OneOf<T1,...,T4>` in MVC controllers
  - 3 thin generator wrappers (OneOf2, OneOf3, OneOf4) with shared orchestrator
  - IError.Tags-first mapping + type-name heuristic fallback, MVC result types
  - Generated into `namespace Generated.OneOfActionResultExtensions`
  - MVC demo controllers updated — all `OneOf.Match()` replaced with `.ToActionResult()` one-liners
- **12 new source generator tests** for OneOfToActionResult (2/3/4 arity)

### 🔧 Fixed
- **OneOfToIResult: tag-based error mapping** — `MapErrorToHttpResult` now checks `IError.Tags["HttpStatusCode"]` first, falls back to type-name heuristic only for non-IError types. Domain errors with custom `HttpStatusCode` tags now map correctly.
- **ValidationError → 422** — OneOfToIResult heuristic and SmartEndpoints OpenAPI both now map `ValidationError`/`Invalid` to 422 (was 400)

### 📝 Changed
- **SmartEndpoints OpenAPI: accurate error status codes** — `DetermineOpenApiStatusCode` maps `ValidationError` to 422 (was 400). `Result<T>` endpoints now declare `.Produces(400)`, `.Produces(404)`, `.Produces(409)`, `.Produces(422)` (was only 400).

### Stats
- 2,836 tests passing across net8.0, net9.0, net10.0 + generator + analyzer tests

## [1.21.0] - 2026-02-17

### ✨ Added
- **ResultToActionResult source generator** — `ToActionResult()` extension methods for ASP.NET MVC controllers
  - Convention-based: reads `HttpStatusCode` tag from domain errors, auto-maps to `IActionResult` types
  - Explicit overload: `ToActionResult(onSuccess, onFailure)` escape hatch for full control
  - HTTP verb variants: `ToPostActionResult()` (201), `ToPutActionResult()` (200), `ToPatchActionResult()` (200), `ToDeleteActionResult()` (204)
  - Private `MapErrorToActionResult` helper: 401→`UnauthorizedResult`, 403→`ForbidResult`, 404→`NotFoundObjectResult`, 409→`ConflictObjectResult`, default→`ObjectResult`
  - Generated into `namespace Generated.ActionResultExtensions`
  - Zero runtime dependency — MVC types emitted as string literals by the generator
- **FastMvcAPI demo app** (`samples/FastMvcAPI.REslava.Result.Demo`)
  - MVC equivalent of existing Minimal API demo — same domain (Users, Products, Orders)
  - Showcases `ToActionResult()` one-liners and `OneOf.Match()` with MVC result types
  - Runs on port 5001 (side-by-side with Minimal API demo on 5000)
  - Scalar UI at `/scalar/v1`
- **9 new source generator tests** for ResultToActionResult generator

### Stats
- 2,813 tests passing across net8.0, net9.0, net10.0 + generator + analyzer tests

## [1.20.0] - 2026-02-17

### ✨ Added

**Structured Error Hierarchy (Domain Errors)**
- `NotFoundError` — HTTP 404, with `(entityName, id)` constructor and `EntityName`/`EntityId` tags
- `ValidationError` — HTTP 422, with `FieldName` property, `(fieldName, message)` constructor
- `ConflictError` — HTTP 409, with `(entityName, conflictField, conflictValue)` constructor
- `UnauthorizedError` — HTTP 401, with default "Authentication required" message
- `ForbiddenError` — HTTP 403, with `(action, resource)` constructor
- All domain errors use CRTP pattern (`Reason<TSelf>, IError`), carry `HttpStatusCode` and `ErrorType` tags, and support fluent `WithTag`/`WithMessage` chaining
- 27 new domain error tests

**Test Coverage Hardening (123 new tests)**
- `ResultConditionalTests` — 39 tests covering all `OkIf`/`FailIf` overloads (non-generic, generic, lazy, async)
- `ResultTryTests` — 15 tests covering `Try`/`TryAsync` (success, exception, custom handler, null guards, cancellation)
- `ResultCombineTests` — 18 tests covering `Merge`/`Combine`/`CombineParallelAsync`
- `ResultTapExtensionsTests` — 30 tests covering `TapOnFailure`/`TapBoth`/`TapAsync` variants
- `ResultLINQTaskExtensionsTests` — 21 tests covering `Task<Result<S>>` LINQ extensions

### 🔧 Changed

**ResultToIResult Generator — Domain Error-Aware HTTP Mapping**
- `ToIResult`, `ToPostResult`, `ToPutResult`, `ToPatchResult`, `ToDeleteResult` now read the `HttpStatusCode` tag from domain errors instead of always returning 400
- Supports both `HttpStatusCode` (library convention) and `StatusCode` (legacy convention) tags
- Maps: 404→`NotFound`, 401→`Unauthorized`, 403→`Forbid`, 409→`Conflict`, others→`Problem(statusCode)`
- Extracted shared `MapErrorToIResult` helper (eliminated 5x duplicated error blocks)

**ValidationResult.Failure — Uses ValidationError**
- `ValidationResult<T>.Failure(string)` now creates `ValidationError` instead of generic `Error`
- Failures automatically carry `HttpStatusCode=422` and `ErrorType=Validation` tags

**Performance: Cached Computed Properties**
- `Result.Errors` and `Result.Successes` are now lazy-cached on first access
- `Result.IsFailure` uses `Errors.Count > 0` instead of re-enumerating `Reasons.OfType<IError>().Any()`
- Safe because `Result` is immutable (`Reasons` has `private init`)

**SmartEndpoints: Convention-Based Route Prefix**
- Default route prefix derived from class name (e.g., `UserService` → `/api/users`) instead of hard-coded `/api/test`
- Strips common suffixes: Service, Controller, Endpoints, Endpoint

**Result.ToString() Override**
- Base `Result` class now overrides `ToString()`: `Result: IsSuccess='True', Reasons=[...]`

**ExceptionError Namespace Fix**
- `ExceptionError` moved from global namespace to `REslava.Result` namespace (was polluting consumers' global scope)

**Result\<T\> Constructor Encapsulation**
- Two `public` constructors changed to `internal` — prevents construction of invalid states bypassing factory methods
- Added `InternalsVisibleTo("REslava.Result.Tests")`

### 🧹 Removed

**Source Generator Dead Code Cleanup**
- Deleted duplicate `HttpStatusCodeMapper` (2 files — static and instance versions)
- Deleted orphan `SmartEndpointExtensionGenerator.cs` (stale intermediate version)
- Deleted `Test1.cs` (empty placeholder) and `ConsoleTest.cs.disabled` (abandoned)
- Removed duplicate `ExtractStringArrayFromAttributeData` method in `SmartEndpointsOrchestrator`
- Removed marker comments from `Result.Combine.cs`, `Result.Conversions.cs`, `Result.Generic.cs`

**Demo App: Migrated to Library Domain Errors**
- Deleted 3 custom error files (`NotFoundErrors.cs`, `ValidationErrors.cs`, `BusinessErrors.cs`) — 12 custom error classes replaced by 5 library domain errors
- Simplified OneOf signatures (e.g., `OneOf<ValidationError, InvalidPriceError, ProductResponse>` → `OneOf<ValidationError, ProductResponse>`)
- Demo app now references local project instead of NuGet package (for latest domain errors)

### 📊 Stats

- **2,798 tests passing** (896 x 3 TFMs + 56 source generator + 54 analyzer)
- 150 new tests in this release
- 7 files deleted, 5 domain error types added

---

## [1.19.0] - 2026-02-16

### ✨ Added

**RESL1004 — Async Result Not Awaited Analyzer**
- Detects `Task<Result<T>>` assigned to `var` without `await` in async methods
- Code fix: automatically adds `await` keyword
- Skips: explicit `Task<...>` type declarations, non-async methods, returned tasks

**CancellationToken Support Throughout**
- Added `CancellationToken cancellationToken = default` to all async methods
- Instance methods: `TapAsync`, `BindAsync`, `MapAsync`, `MatchAsync`
- Factory methods: `TryAsync` (generic and non-generic)
- Extension methods: `BindAsync`, `MapAsync`, `TapAsync`, `TapOnFailureAsync`, `WithSuccessAsync`, `EnsureAsync`, `EnsureNotNullAsync`, `SelectManyAsync`, `SelectAsync`, `WhereAsync`, `MatchAsync`
- Source-compatible: existing code compiles without changes
- 13 new CancellationToken tests + 8 new analyzer tests

---

## [1.18.0] - 2026-02-16

### ✨ Added

**Task-Based Async Patterns (WhenAll, Retry, Timeout)**
- `Result.WhenAll()` — run 2/3/4 async Result operations concurrently, returning typed tuples with aggregated errors
- `Result.WhenAll(IEnumerable<Task<Result<T>>>)` — collection variant returning `Result<ImmutableList<T>>`
- `Result.Retry()` — retry async operations with configurable delay, exponential backoff, and CancellationToken support
- `.Timeout()` extension on `Task<Result<T>>` — enforce time limits with TimeoutTag metadata on timeout errors
- Exception-safe: faulted/cancelled tasks wrapped in `ExceptionError`, `OperationCanceledException` stops retries
- Non-generic overloads for `Result.Retry()` and `.Timeout()`
- 41 new async pattern tests (per TFM)

---

## [1.17.0] - 2026-02-16

### ✨ Added

**JSON Serialization Support (System.Text.Json)**
- `JsonConverter<Result<T>>` — serializes as `{ "isSuccess": true, "value": ..., "errors": [], "successes": [] }`
- `JsonConverter<OneOf<T1,T2>>`, `OneOf<T1,T2,T3>`, `OneOf<T1,T2,T3,T4>` — serializes as `{ "index": 0, "value": ... }`
- `JsonConverter<Maybe<T>>` — serializes as `{ "hasValue": true, "value": ... }`
- `JsonSerializerOptions.AddREslavaResultConverters()` extension method to register all converters
- Error/Success reasons serialized with type name, message, and tags metadata
- Zero new dependencies — uses built-in `System.Text.Json`
- All converters use hardcoded camelCase property names for predictable output
- 48 new serialization tests (16 per TFM)

---

## [1.16.0] - 2026-02-16

### 🔧 Changed

**NuGet Package READMEs — Discoverability**
- Created tailored NuGet README for `REslava.Result` — focused quick-start with before/after code comparison
- Created tailored NuGet README for `REslava.Result.SourceGenerators` — SmartEndpoints showcase with before/after
- Created tailored NuGet README for `REslava.Result.Analyzers` — diagnostic rules table with code fix examples
- Each package now has its own focused README (~60-75 lines) instead of sharing the full GitHub README (~800+ lines)
- NuGet READMEs stored in `docs/nuget/` directory

---

## [1.15.0] - 2026-02-15

### 🧹 Removed

**Project Cleanup — Node.js Toolchain & Legacy Files**
- Removed Node.js release toolchain (`package.json`, `package-lock.json`, `.versionrc.json`) — superseded by `Directory.Build.props` + GitHub Actions `release.yml`
- Removed Husky git hooks (`.husky/`) and commitlint (`commitlint.config.js`) — commit validation now handled by CI/CD pipeline
- Removed `scripts/` directory (5 files: `clean-before-test.ps1`, `quick-clean.ps1`, `update-github-release.sh`, `update-versions.js`, `CLEAN-BEFORE-TEST.md`) — superseded by CI/CD pipeline
- Removed `templates/` directory — incomplete, unpublished dotnet template
- Removed `samples/NuGetValidationTest/` — stale test project with v1.9.0 package references

### 🔧 Changed

**Documentation Refresh**
- Standardized emoji: replaced 🏗️ with 📐 across all 34 markdown files (fixed anchor link issues with variation selector)
- Updated README.md Roadmap section (v1.15.0 current, refreshed milestone descriptions)
- Removed speculative "Future Versions" section from README.md
- Updated test counts to 2,004+ throughout documentation
- Rewrote `samples/README.md` to reflect actual sample projects

---

## [1.14.2] - 2026-02-15

### ✨ Added

**New Analyzers & Code Fixes (Phase 2 + 3)**
- **RESL1003 — Prefer Match() over if-check**: Info-level suggestion when both `.Value` and `.Errors` are accessed in complementary `if`/`else` branches. Detects all 4 condition variants: `IsSuccess`, `IsFailure`, `!IsSuccess`, `!IsFailure`
- **RESL2001 — Unsafe OneOf.AsT* access**: Warning when `.AsT1`–`.AsT4` is accessed on `OneOf<T1,...>` without checking the corresponding `.IsT*` first. Supports guard detection via if-checks and early returns
- **RESL1001 Code Fix**: Two fix options — wrap in `if (result.IsSuccess) { ... }` guard, or replace with `.Match(v => v, e => default)`
- **RESL2001 Code Fix**: Replaces `.AsT*` with complete `.Match()` call, generating all arity lambdas with `NotImplementedException()` placeholders

**Infrastructure**
- Shared `GuardDetectionHelper` with parameterized `GuardConfig` — reusable guard detection for both Result and OneOf analyzers
- Generic `AnalyzerTestHelper` with `CreateAnalyzerTest<T>()` and `CreateCodeFixTest<T,F>()` methods
- `OneOfStubSource` test stubs for all 3 OneOf arities
- 28 new analyzer tests (46 total), 2,004 total project tests

### 🔧 Changed
- Refactored `UnsafeValueAccessAnalyzer` to use shared `GuardDetectionHelper` (247 → ~80 lines)

---

## [1.14.1] - 2026-02-10

### 🔧 Changed

**Source Generator Consolidation: OneOfToIResult**
- Consolidated `OneOf2ToIResult`, `OneOf3ToIResult`, `OneOf4ToIResult` into a single `OneOfToIResult` directory
- Replaced 15 near-identical files with 7 arity-parameterized shared implementations
- Single `OneOfToIResultOrchestrator` handles all arities (2, 3, 4) via constructor parameter
- 3 thin `[Generator]` wrappers remain (Roslyn requires separate classes per generator)
- Unified test file with 12 tests replacing 3 separate test files
- No API changes — generated output is identical

---

## [1.14.0] - 2026-02-10

### ✨ Added

**NEW: REslava.Result.Analyzers NuGet Package**
- New companion NuGet package providing Roslyn diagnostic analyzers for REslava.Result
- **RESL1001 — Unsafe Result<T>.Value access**: Warns when `.Value` is accessed without checking `IsSuccess` or `IsFailure` first. Detects 5 guard patterns: `if (result.IsSuccess)`, `if (!result.IsFailure)`, else-branch of `IsFailure`, early return, and early throw
- **RESL1002 — Discarded Result<T> return value**: Warns when a method returning `Result<T>` or `Task<Result<T>>` is called and the return value is ignored, silently swallowing errors
- 18 analyzer tests (10 for RESL1001, 8 for RESL1002)
- Zero-dependency analyzer — ships as `analyzers/dotnet/cs` in the NuGet package

**NuGet Package Improvements**
- Added package icon to REslava.Result.SourceGenerators and REslava.Result.Analyzers
- Added package README to REslava.Result.Analyzers
- Release pipeline now builds and publishes all 3 packages

### 🔧 Fixed

**CI/CD Pipeline**
- Release workflow now includes REslava.Result.Analyzers in build, pack, and publish steps

---

## [1.13.0] - 2026-02-10

### ✨ Added

**SmartEndpoints: Authorization & Policy Support**
- Class-level `RequiresAuth = true` on `[AutoGenerateEndpoints]` — all endpoints emit `.RequireAuthorization()`
- Class-level `Policies = new[] { "Admin" }` — emits `.RequireAuthorization("Admin")`
- Class-level `Roles = new[] { "Admin", "Manager" }` — emits `.RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,Manager" })`
- Method-level `[SmartAllowAnonymous]` attribute — overrides class auth, emits `.AllowAnonymous()`
- Method-level `[AutoMapEndpoint(AllowAnonymous = true, Roles = ...)]` — fine-grained control
- Auth inheritance: class-level defaults propagate to all methods unless overridden
- Auto-adds `.Produces(401)` to OpenAPI metadata for auth-protected endpoints
- Conditional `using Microsoft.AspNetCore.Authorization;` only when Roles are used
- 12 new authorization tests (`SmartEndpoints_AuthorizationTests.cs`)

### 📚 Documented

**LINQ Query Syntax for Result<T>** (already implemented, now formally documented)
- `Select`, `SelectMany` (2-param + 3-param for query syntax), `Where` — all with async variants
- Enables: `from user in GetUser(id) from account in GetAccount(user.AccountId) select ...`
- 35 tests passing across net8.0, net9.0, net10.0
- Console sample: `05_LINQSyntax.cs` with 8 progressive examples
- Moved from "Next Up" to "Current" in roadmap

### ✨ Added

**Demo App: JWT Bearer Authentication Showcase**
- JWT Bearer auth configured for SmartEndpoints auth demo
- `/auth/token` endpoint generates test JWTs with optional role parameter
- `SmartOrderController` uses `RequiresAuth = true` with `[SmartAllowAnonymous]` on `GetOrders()`
- Side-by-side comparison: authenticated SmartOrders vs unauthenticated SmartProducts

**SmartEndpoints: OpenAPI Metadata Auto-Generation**
- Endpoints now emit full OpenAPI metadata from return type analysis at compile time
  - `.WithName("ControllerBase_MethodName")` — globally unique endpoint names
  - `.WithSummary("...")` — auto-generated from PascalCase method name or XML doc `<summary>`
  - `.WithTags("...")` — auto-generated from class name (strips Controller/Service suffix, splits PascalCase)
  - `.Produces<T>(200)` — typed success response from `Result<T>` or non-error OneOf type arguments
  - `.Produces(statusCode)` — error status codes inferred from error type names (NotFound→404, Conflict→409, Unauthorized→401, Forbidden→403, Database→500, Validation/default→400)
- Endpoints are grouped per controller using `MapGroup(prefix).WithTags(tag)` instead of flat registration
  - Relative routes within groups (e.g., `/{id}` instead of `/api/products/{id}`)
  - Controller-scoped variable names (e.g., `smartProductGroup`)
- Status code deduplication — two errors mapping to 400 produce a single `.Produces(400)`
- 21 new tests covering all OpenAPI metadata features (`SmartEndpoints_OpenApiMetadataTests.cs`)

---

## [1.12.2] - 2026-02-09

### 🔧 Fixed

**SmartEndpoints Source Generator**
- Fixed SmartEndpointsGenerator to delegate to orchestrator instead of emitting hardcoded stub endpoint
  - Removed inline placeholder code (`/api/simple/test`) that bypassed the real generator pipeline
  - Generator now follows the same SOLID pattern as all other generators (Generator → Orchestrator)
- Added dependency injection support in generated endpoints
  - Services are now injected via ASP.NET Minimal API parameter binding instead of `new ClassName()`
- Added async/await support in generated endpoints
  - Detects `Task<Result<T>>` and `Task<OneOf<...>>` return types
  - Generates proper `async`/`await` lambda syntax
- Added missing `using` directives in generated code
  - `using System.Threading.Tasks;`
  - `using Generated.ResultExtensions;`
  - `using Generated.OneOfExtensions;`

---

## [1.12.1] - 2026-02-08

### ✨ Added

**Sample Applications**
- **FastMinimalAPI.REslava.Result.Demo** - Production-ready ASP.NET Core Minimal API demonstration
  - Comprehensive README with learning path (Level 1, 2, 3 patterns)
  - 15 endpoints showcasing Result&lt;T&gt; and OneOf patterns
  - Real-world business scenarios (Users, Products, Orders)
  - Rich error handling with custom error types
  - In-memory database with seed data for testing
  - Full CRUD operations with validation

- **REslava.Result.Samples.Console** - Educational console application
  - 13 progressive examples from basic to advanced
  - Comprehensive feature coverage (20+ patterns)
  - LINQ syntax, async operations, validation pipelines
  - Maybe&lt;T&gt; and OneOf&lt;T1,T2,T3,T4&gt; functional patterns
  - Result↔OneOf conversions and integrations

### 🔧 Fixed

**Source Generators**
- Removed hardcoded namespace reference in SmartEndpointsGenerator
  - Fixed `using MinimalApi.Net10.REslavaResult.Models;` that caused build errors
  - Generator now works with any project namespace

**FastMinimalAPI Demo**
- Fixed OneOf5 → OneOf4 conversion in CreateOrder endpoint
  - Consolidated UserInactiveError into ValidationError
  - Updated all endpoint handlers to match new signature
  - Corrected parameter ordering in OrderResponse constructors
  - Fixed Product.StockQuantity property references

**Console Sample**
- Fixed XML documentation warnings in Maybe&lt;T&gt; examples
- Corrected project reference paths after directory restructuring

### 📚 Improved

**Documentation**
- Added comprehensive README for Console sample
- Updated FastMinimalAPI README to reflect actual implementation
- Clarified error handling patterns and use cases

### 📊 Stats

- **Sample Apps**: 2 new comprehensive demos
- **Example Files**: 13 progressive console examples
- **API Endpoints**: 15 web API endpoints demonstrating patterns
- **Lines of Code**: ~3,500 lines of sample code
- **Build Status**: ✅ All samples build and run successfully

---

## [1.12.0] - 2026-02-07

### ✨ Added
- **OneOf4ToIResult Generator** - 4-way discriminated unions with intelligent HTTP mapping
- **Enhanced SmartEndpoints** - Better OneOf4 support and automatic endpoint generation
- **Complete Generator Integration** - All generators working together seamlessly
- **Automated Testing Infrastructure** - 1,928 tests passing with bash script validation

### 🚀 Improved  
- **Fast APIs Development** - 10x faster development, 90% less code
- **Self-Explanatory Development** - Zero boilerplate, business logic only
- **Zero Manual Configuration** - Automatic route, error, and status mapping
- **Comprehensive Documentation** - Updated README, release notes, quick-start guides

### 🔧 Fixed
- Project reference paths after directory restructuring
- Package metadata paths for README and icon files
- Test project compilation issues
- Source generator test infrastructure

### 📊 Stats
- 1,928 tests passing (up from 1,902)
- 17 source generator tests passing
- 9 integration tests passing
- 95%+ code coverage maintained

## [1.11.0] - 2025-02-05

### 🎯 Added
- **SmartEndpoints Generator** - Complete Zero Boilerplate API generation
  - Automatic route generation with parameter awareness
  - Intelligent HTTP method detection (GET/POST/PUT/DELETE)
  - Route prefix support via `[AutoGenerateEndpoints(RoutePrefix = "...")]`
  - Full integration with existing OneOf2/OneOf3 extensions
  - Comprehensive error handling with automatic HTTP status mapping

### 🔄 Changed
- **Route Inference** - Enhanced to include `{id}` parameters when needed
- **OneOf Integration** - SmartEndpoints now uses existing OneOf extensions
- **Generated Code** - Cleaned up debug code and production-ready

### 🧪 Fixed
- **SmartEndpoints Warnings** - Resolved null reference warnings
- **Route Generation** - Fixed parameter-aware route inference
- **Test Coverage** - Added comprehensive MSTest suite for SmartEndpoints

### ⚠️ Breaking Changes
- **SmartEndpoints Route Inference** - Generated routes now properly include `{id}` parameters
  - Routes may change from `/api/User` to `/api/User/{id}` for methods with ID parameters
  - This improves route correctness and is a recommended update

### 📚 Documentation
- Updated README with comprehensive SmartEndpoints examples
- Added breaking changes documentation
- Enhanced troubleshooting section

---

## [1.10.3] - 2025-02-05

### 🎯 Added
- **OneOf2ToIResult Generator** - Two-type error handling
- **OneOf3ToIResult Generator** - Three-type error handling
- **Intelligent HTTP Mapping** - Automatic error type detection
- **Comprehensive Error Coverage** - All common error scenarios

### 🔄 Changed
- **Error Detection** - Smart error type identification
- **HTTP Status Mapping** - Automatic response code generation

---

## [1.10.2] - 2025-02-05

### 🎯 Added
- **ResultToIResult Generator** - Basic Result<T> conversion
- **HTTP Status Mapping** - Intelligent error response generation
- **ProblemDetails Support** - Structured error responses

### 🔄 Changed
- **Core Library** - Enhanced error handling capabilities

---

## [1.10.1] - 2025-02-05

### 🎯 Added
- **Initial Release** - Core Result types
- **Error Handling** - Basic error type definitions
- **HTTP Integration** - ASP.NET Core IResult support

### 🔄 Changed
- **Initial Setup** - Project structure and packaging

---

## [1.10.0] - 2025-02-05

### 🎯 Added
- **Framework Foundation** - Railway-oriented programming patterns
- **Result Types** - Success, Error, ValidationError types
- **Basic HTTP Integration** - IResult conversion methods

### 🔄 Changed
- **Initial Setup** - Project structure and packaging
