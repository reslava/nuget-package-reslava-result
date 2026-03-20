#!/usr/bin/env python3
"""
generate_flow_catalog.py
Generates mkdocs/reference/flow-catalog/index.md from the ResultFlow
generator output of a target project.

Usage:
    python scripts/generate_flow_catalog.py
    python scripts/generate_flow_catalog.py --project path/to/MyProject
    python scripts/generate_flow_catalog.py --project path/to/MyProject --output path/to/output.md
    python scripts/generate_flow_catalog.py --help

Defaults:
    --project  samples/REslava.Result.Flow.Demo
    --output   mkdocs/reference/flow-catalog/index.md
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


def extract_constants(cs_file: Path) -> dict[str, str]:
    """
    Extract all public const string constants from a C# verbatim string file.
    Returns {constant_name: mermaid_content}.
    Skips constants in SKIP_SUFFIXES.
    Unescapes verbatim string doubled-quote sequences ("" → ").
    """
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
            skip = name in SKIP_NAMES
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
    args = parser.parse_args()

    project_dir = Path(args.project)
    output_path = Path(args.output)

    print(f"Project : {project_dir}")
    print(f"Output  : {output_path}")

    # Find generated files
    cs_files = find_generated_files(project_dir)
    print(f"Found   : {len(cs_files)} *_Flows.g.cs file(s)")

    # Parse each file
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
