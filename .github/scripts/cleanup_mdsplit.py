#!/usr/bin/env python3
"""
Clean up after mdsplit -l 3.

With -l 3, mdsplit creates directly inside mkdocs/:
  - README.md                          (H1 wrapper — delete it)
  - 1.--Table-of-Contents.md           (TOC — delete it)
  - 7.--REslava.Result-Core-Library.md (H2 wrapper — delete if section has sub-pages)
  - 7.--REslava.Result-Core-Library/   (H2 folders containing H3 files — flatten then delete)
    └── 7.1.-Operations.md
    └── 7.2.-Async-Patterns.md
    ...
  - 3.--Quick-Start.md                 (H2 wrapper with no folder = standalone content — KEEP)

This script:
  1. Deletes mkdocs/README.md (mdsplit H1 wrapper).
  2. Deletes the Table-of-Contents file (section 1.*).
  3. For each numbered H2 folder (e.g. 7.--..., 9.--...):
     - Moves all .md files (rglob) to mkdocs/ root (keeping original names).
     - Removes the folder.
     - Records the section number as "has sub-pages".
  4. Deletes H2 wrapper files for sections that had sub-pages.
     These are near-empty (0-1 lines of content) and redundant — the
     subfolder index.md serves as the section landing page instead.
     H2 wrappers for sections WITHOUT sub-pages are kept (they ARE the content).

Prefix stripping (H2 "7.--" and H3 "7.1.-") is handled by organize_docs.py after routing.
"""

import re
import shutil
from pathlib import Path

DOCS_DIR = Path("mkdocs")

# Matches any folder/file whose name starts with one or more digits followed by a dot.
_NUMBERED_RE = re.compile(r'^(\d+)\.')


def main():
    if not DOCS_DIR.exists():
        print("❌ docs/ directory not found.")
        return

    # 1. Delete the mdsplit H1 wrapper (README.md at mkdocs root)
    readme = DOCS_DIR / "README.md"
    if readme.exists():
        print(f"🗑️  Deleting H1 wrapper: {readme.name}")
        readme.unlink()

    # 2. Delete Table-of-Contents file (section 1)
    for toc_file in DOCS_DIR.glob("1.--*.md"):
        print(f"🗑️  Deleting TOC file: {toc_file.name}")
        toc_file.unlink()

    # 3. Find all numbered H2 folders at mkdocs root
    folders = sorted(
        p for p in DOCS_DIR.iterdir()
        if p.is_dir() and _NUMBERED_RE.match(p.name)
    )

    if not folders:
        print("✅ No numbered section folders found.")
        return

    # Track which section numbers had sub-pages (H2 folders)
    sections_with_subpages: set[str] = set()

    for folder in folders:
        m = _NUMBERED_RE.match(folder.name)
        if m:
            sections_with_subpages.add(m.group(1))

        print(f"📂 Flattening: {folder.name}/")
        moved = 0
        for md_file in folder.rglob("*.md"):
            dest = DOCS_DIR / md_file.name
            if dest.exists():
                print(f"  ⚠️ Skipping {md_file.name}: already exists at destination.")
                continue
            print(f"  Moving {md_file.name}")
            shutil.move(str(md_file), str(dest))
            moved += 1
        print(f"  Moved {moved} file(s).")
        shutil.rmtree(str(folder))
        print(f"  Removed: {folder.name}/")

    # 4. Delete H2 wrapper files for sections that had sub-pages.
    # H2 wrapper identification: file at root whose name starts with "{n}.-"
    # where the char after the dot is a dash (not a digit like H3 files "7.1.-...").
    print("\n🗑️  Removing H2 wrapper stubs for sections that have sub-pages...")
    deleted = 0
    for md_file in sorted(DOCS_DIR.glob("*.md")):
        if md_file.name == "index.md":
            continue
        m = _NUMBERED_RE.match(md_file.name)
        if not m:
            continue
        section_num = m.group(1)
        if section_num not in sections_with_subpages:
            continue
        # Distinguish H2 wrapper ("7.--Title") from H3 file ("7.1.-Title"):
        # After "{n}." the H2 wrapper has a dash; H3 files have a digit.
        rest = md_file.name[len(section_num) + 1:]  # everything after "7."
        if rest.startswith('-'):
            print(f"  Deleting H2 wrapper: {md_file.name}")
            md_file.unlink()
            deleted += 1

    if deleted:
        print(f"  Deleted {deleted} H2 wrapper stub(s).")
    else:
        print("  No H2 wrappers to delete.")


if __name__ == "__main__":
    main()
