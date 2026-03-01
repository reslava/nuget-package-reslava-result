#!/usr/bin/env python3
"""
Detect orphan .md files in mkdocs/ — pages with no inbound links from any other .md file.

Run from the project root after the pipeline:

    python scripts/find_orphans.py

A page is an orphan if no other .md file contains a link whose resolved path points to it.
index.md files are excluded from the orphan list (they are section landing pages).
"""

import re
import sys
from pathlib import Path

DOCS_DIR = Path("mkdocs")

# Same protected dirs as clean_docs.py — skip these (hand-maintained content)
SKIP_DIRS = [
    "reference/api-doc",
    "architecture/solid",
    "code-examples/samples",
]

# Matches [any text](target) — captures the raw link target
LINK_RE = re.compile(r'\[[^\]]*\]\(([^)]+)\)')


def resolve_target(source, raw):
    """
    Resolve a raw link target relative to `source` (a resolved Path) to a .md Path.
    Returns None if the link is external, anchor-only, or non-markdown.
    """
    # Strip anchor and whitespace
    raw = raw.split('#')[0].strip()
    if not raw:
        return None
    if raw.startswith(('http://', 'https://', 'ftp://', 'mailto:')):
        return None

    target = (source.parent / raw).resolve()

    # Directory link (trailing slash or points to an existing dir) → index.md
    if raw.endswith('/') or target.is_dir():
        return target / 'index.md'

    # Explicit .md link
    if raw.endswith('.md'):
        return target

    # Known non-markdown assets — skip
    raw_suffix = Path(raw).suffix.lower()
    if raw_suffix in {'.html', '.htm', '.png', '.jpg', '.jpeg', '.gif', '.svg', '.pdf'}:
        return None

    # Everything else: treat as a markdown link without extension.
    # Use str concatenation (not .with_suffix) so dots inside the stem are preserved,
    # e.g. "v1.22.0" → "v1.22.0.md", "resl1001--unsafe-.value-..." → "...value-....md"
    return Path(str(target) + '.md')


def main():
    if not DOCS_DIR.exists():
        print(f"❌ {DOCS_DIR} not found. Run from the project root after the pipeline.")
        sys.exit(1)

    skip_abs = {(DOCS_DIR / d).resolve() for d in SKIP_DIRS if (DOCS_DIR / d).exists()}

    # Build file index: resolved absolute Path → display relative Path
    all_md = {}
    for f in DOCS_DIR.rglob("*.md"):
        r = f.resolve()
        if not any(r.is_relative_to(s) for s in skip_abs):
            all_md[r] = f.relative_to(DOCS_DIR)

    all_resolved = set(all_md)

    # Count inbound links per file
    inbound = {r: 0 for r in all_resolved}

    for src_resolved in all_md:
        try:
            content = src_resolved.read_text(encoding='utf-8')
        except Exception:
            continue

        for m in LINK_RE.finditer(content):
            target = resolve_target(src_resolved, m.group(1))
            if target in inbound:
                inbound[target] += 1

    # Collect orphans: non-index files with zero inbound links
    orphans = sorted(
        all_md[r]
        for r in all_resolved
        if inbound[r] == 0 and all_md[r].name != "index.md"
    )

    total = len(all_md)
    if not orphans:
        print(f"✅ No orphans — every non-index page has at least one inbound link. ({total} pages checked)")
        return

    print(f"⚠️  {len(orphans)} orphan page(s) out of {total} checked:\n")
    current_folder = None
    for rel in orphans:
        folder = str(rel.parent)
        if folder != current_folder:
            print(f"\n  📁 {folder}/")
            current_folder = folder
        print(f"     {rel.name}")

    print()


if __name__ == "__main__":
    main()
