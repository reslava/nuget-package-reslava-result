#!/usr/bin/env python3
"""
generate_flow_catalog.py
Generates mkdocs/reference/flow-catalog/index.md from the ResultFlow
generator output of a target project.

Usage:
    python scripts/generate_flow_catalog.py
    python scripts/generate_flow_catalog.py --project path/to/MyProject
    python scripts/generate_flow_catalog.py --project path/to/MyProject --output path/to/output.md
    python scripts/generate_flow_catalog.py --export-mmd images/
    python scripts/generate_flow_catalog.py --help

Defaults:
    --project  samples/REslava.Result.Flow.Demo
    --output   mkdocs/reference/flow-catalog/index.md

--export-mmd DIR:
    Instead of writing the MkDocs catalog, export each diagram constant as a
    .mmd file into DIR.  Naming: {ClassName}_{ConstantName}.mmd
    Legend is exported once as Legend.mmd (no class prefix).
    Exported types: Pipeline, _LayerView, _ErrorSurface, _ErrorPropagation.
    Skipped types:  _Stats, _Sidecar.
"""

import argparse
import os
import re
import sys
from pathlib import Path
from collections import defaultdict

# ── Suffix display names ────────────────────────────────────────────────────
# Maps constant suffix → (section title, description)
SUFFIX_META = {
    "":                 ("Pipeline",         "Success path, typed error edges, async steps"),
    "_LayerView":       ("Layer View",       "Architecture layers — Domain / Application / Infrastructure boundaries"),
    "_Stats":           ("Stats",            "Node count, error count, depth, async steps"),
    "_ErrorSurface":    ("Error Surface",    "All possible errors grouped by the step that produces them"),
    "_ErrorPropagation":("Error Propagation","Error types grouped by the architectural layer they originate from"),
}
SKIP_SUFFIXES = {"_Sidecar"}
# Exact constant names that are per-class (not per-method) and should not appear as diagrams
SKIP_NAMES = {"Legend"}
# Suffixes excluded from .mmd export (Stats is a markdown table; Sidecar is wrapped markdown)
_EXPORT_SKIP_SUFFIXES = {"_Stats", "_Sidecar"}
# Exact names exported once with no class prefix
_EXPORT_ONCE_NAMES = {"Legend"}

# Suffix sort order for display
SUFFIX_ORDER = ["", "_LayerView", "_Stats", "_ErrorSurface", "_ErrorPropagation"]


def find_generated_files(project_dir: Path) -> list[Path]:
    """Find all *_Flows.g.cs files in the project's obj/Generated directory."""
    gen_root = project_dir / "obj" / "Generated"
    if not gen_root.exists():
        print(f"ERROR: Generated files not found at {gen_root}", file=sys.stderr)
        print("       Run 'dotnet build' on the project first.", file=sys.stderr)
        sys.exit(1)

    files = list(gen_root.rglob("*_Flows.g.cs"))
    if not files:
        print(f"ERROR: No *_Flows.g.cs files found under {gen_root}", file=sys.stderr)
        sys.exit(1)

    return sorted(files)


def extract_constants(cs_file: Path, skip_names: "set[str] | None" = None) -> dict[str, str]:
    """
    Extract all public const string constants from a C# verbatim string file.
    Returns {constant_name: mermaid_content}.
    Skips constants whose name is in skip_names (default: SKIP_NAMES) or suffix is in SKIP_SUFFIXES.
    Unescapes verbatim string doubled-quote sequences ("" → ").
    Pass skip_names=set() to include all constants (e.g. for --export-mmd mode).
    """
    if skip_names is None:
        skip_names = SKIP_NAMES
    text = cs_file.read_text(encoding="utf-8")
    constants = {}

    # State machine: find `public const string NAME = @"` then collect until closing `";`
    lines = text.splitlines()
    i = 0
    while i < len(lines):
        line = lines[i]
        m = re.match(r'\s*public const string (\w+) = @"(.*)', line)
        if m:
            name = m.group(1)
            first_content = m.group(2)  # content on same line as opening @"

            # Determine suffix
            skip = name in skip_names
            if not skip:
                for s in SKIP_SUFFIXES:
                    if name.endswith(s):
                        skip = True
                        break

            # Collect lines until closing `";`
            content_lines = []
            if first_content:
                content_lines.append(first_content)

            i += 1
            found_close = False
            while i < len(lines):
                l = lines[i]
                # Closing: line is exactly `";` (possibly with whitespace) — old multi-line format
                if re.match(r'^\s*";\s*$', l):
                    found_close = True
                    i += 1
                    break
                # Closing: line ends with `";` — new compact format (@"content...last-line";)
                stripped = l.rstrip()
                if stripped.endswith('";'):
                    content_lines.append(stripped[:-2])
                    found_close = True
                    i += 1
                    break
                content_lines.append(l)
                i += 1

            if found_close and not skip:
                raw = "\n".join(content_lines)
                # Unescape verbatim doubled-quotes
                content = raw.replace('""', '"')
                constants[name] = content.strip()
        else:
            i += 1

    return constants


def class_name_from_file(cs_file: Path) -> str:
    """OrderService_Flows.g.cs → OrderService"""
    stem = cs_file.stem  # e.g. OrderService_Flows.g
    stem = stem.replace(".g", "")  # OrderService_Flows
    return stem.replace("_Flows", "")


def group_by_method(constants: dict[str, str]) -> dict[str, dict[str, str]]:
    """
    Groups {const_name: content} by method name.
    Returns {method_name: {suffix: content}}.
    """
    methods = defaultdict(dict)
    for name, content in constants.items():
        matched_suffix = None
        for suffix in SUFFIX_ORDER:
            if suffix == "" and not any(name.endswith(s) for s in SUFFIX_ORDER if s):
                matched_suffix = ""
                break
            elif suffix and name.endswith(suffix):
                matched_suffix = suffix
                break
        if matched_suffix is not None:
            method_name = name[: len(name) - len(matched_suffix)] if matched_suffix else name
            methods[method_name][matched_suffix] = content
    return dict(methods)


def render_page(catalog: list[tuple[str, dict[str, dict[str, str]]]]) -> str:
    """
    Render the full MkDocs page.
    catalog: list of (class_name, {method_name: {suffix: content}})
    """
    lines = [
        "---",
        "title: Architectural Flow Catalog",
        "description: Live pipeline and architecture diagrams auto-generated from the REslava.Result.Flow.Demo project — see exactly what ResultFlow produces for real application code.",
        "---",
        "",
        "# 🗺️ Architectural Flow Catalog",
        "",
        "> **See ResultFlow in action on real code.** Every diagram on this page is auto-generated "
        "from the [`REslava.Result.Flow.Demo`](https://github.com/reslava/nuget-package-reslava-result/tree/main/samples/REslava.Result.Flow.Demo) "
        "project — the same output you get when you annotate your own methods with `[ResultFlow]`.",
        "",
        "Each method shows its full set of generated views: pipeline flow, architecture layer view, "
        "stats, error surface, and error propagation — all derived automatically from the source code with zero manual work.",
        "",
        "!!! info",
        "    This page is regenerated automatically on every release. Do not edit manually.",
        "",
        "---",
        "",
    ]

    for class_name, methods in catalog:
        lines.append(f"## {class_name}")
        lines.append("")

        for method_name, views in methods.items():
            lines.append(f"### {method_name}")
            lines.append("")

            for suffix in SUFFIX_ORDER:
                if suffix not in views:
                    continue
                content = views[suffix]
                title, description = SUFFIX_META[suffix]

                lines.append(f"#### {title}")
                lines.append("")
                lines.append(f"*{description}*")
                lines.append("")

                # _Stats is a Markdown table — render as plain markdown, not mermaid
                if suffix == "_Stats":
                    lines.append(content)
                # Detect if content already contains a mermaid fence (e.g. _Sidecar style)
                elif "```mermaid" in content:
                    lines.append(content)
                else:
                    lines.append("```mermaid")
                    lines.append(content)
                    lines.append("```")
                lines.append("")

            lines.append("---")
            lines.append("")

    return "\n".join(lines)


def _should_export(const_name: str) -> bool:
    """Returns True if this constant should be written as a .mmd file."""
    # Skip _Stats and _Sidecar
    for s in _EXPORT_SKIP_SUFFIXES:
        if const_name.endswith(s):
            return False
    # Export _LayerView, _ErrorSurface, _ErrorPropagation
    for s in ("_LayerView", "_ErrorSurface", "_ErrorPropagation"):
        if const_name.endswith(s):
            return True
    # Export plain Pipeline (name has no recognised multi-word suffix)
    all_known = {"_LayerView", "_Stats", "_ErrorSurface", "_ErrorPropagation", "_Sidecar"}
    return not any(const_name.endswith(s) for s in all_known)


def export_mmd(cs_files: "list[Path]", export_dir: Path) -> None:
    """
    Export diagram constants as .mmd files into export_dir.

    Naming convention: {ClassName}_{ConstantName}.mmd
    Exception: Legend is exported once as Legend.mmd (no class prefix).
    """
    export_dir.mkdir(parents=True, exist_ok=True)
    legend_written = False
    exported = 0

    for cs_file in cs_files:
        class_name = class_name_from_file(cs_file)
        # Include Legend (pass empty skip_names so nothing is pre-filtered)
        constants = extract_constants(cs_file, skip_names=set())

        for const_name, content in constants.items():
            # Legend: write once, no class prefix
            if const_name in _EXPORT_ONCE_NAMES:
                if not legend_written:
                    out = export_dir / f"{const_name}.mmd"
                    out.write_text(content, encoding="utf-8")
                    print(f"  Exported: {out.name}")
                    exported += 1
                    legend_written = True
                continue

            if not _should_export(const_name):
                continue

            out = export_dir / f"{class_name}_{const_name}.mmd"
            out.write_text(content, encoding="utf-8")
            print(f"  Exported: {out.name}")
            exported += 1

    print(f"Total   : {exported} .mmd file(s) written to {export_dir}")


def main():
    repo_root = Path(__file__).parent.parent

    parser = argparse.ArgumentParser(
        description="Generate MkDocs flow catalog from ResultFlow generator output."
    )
    parser.add_argument(
        "--project",
        default=str(repo_root / "samples" / "REslava.Result.Flow.Demo"),
        help="Path to the target project directory (must have EmitCompilerGeneratedFiles=true)",
    )
    parser.add_argument(
        "--output",
        default=str(repo_root / "mkdocs" / "demo" / "flow-catalog.md"),
        help="Output MkDocs page path",
    )
    parser.add_argument(
        "--export-mmd",
        default=None,
        metavar="DIR",
        help="Export diagram constants as .mmd files to DIR (skips catalog generation)",
    )
    args = parser.parse_args()

    project_dir = Path(args.project)

    print(f"Project : {project_dir}")

    # Find generated files
    cs_files = find_generated_files(project_dir)
    print(f"Found   : {len(cs_files)} *_Flows.g.cs file(s)")

    # ── Export .mmd mode ──────────────────────────────────────────────────────
    if args.export_mmd:
        export_dir = Path(args.export_mmd)
        print(f"Export  : {export_dir}")
        export_mmd(cs_files, export_dir)
        return

    # ── Catalog generation mode ───────────────────────────────────────────────
    output_path = Path(args.output)
    print(f"Output  : {output_path}")

    catalog = []
    for cs_file in cs_files:
        class_name = class_name_from_file(cs_file)
        constants = extract_constants(cs_file)
        if not constants:
            continue
        methods = group_by_method(constants)
        if methods:
            catalog.append((class_name, methods))
            print(f"  {class_name}: {len(methods)} method(s)")

    if not catalog:
        print("WARNING: No diagram constants found — output will be empty.", file=sys.stderr)

    # Render and write
    page = render_page(catalog)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(page, encoding="utf-8")
    print(f"Written : {output_path}")


if __name__ == "__main__":
    main()
