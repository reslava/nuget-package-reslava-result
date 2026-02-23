#!/usr/bin/env python3
"""
Delete all .md files under mkdocs/ except index.md and those inside excluded directories.
Supports --dry-run (default) and --no-dry-run with optional --yes to skip confirmation.
"""

import argparse
from pathlib import Path

# Directories to exclude from deletion (relative to mkdocs/).
# Files under these folders (and their subfolders) will NOT be deleted.
EXCLUDE_DIRS = [
    "reference/api-doc",   # example – add yours here
    "architecture/solid",
    "code-examples/samples"
    # "other/folder",
]

def main():
    parser = argparse.ArgumentParser(description="Clean .md files from mkdocs/ (except index.md and excluded dirs).")
    parser.add_argument(
        "--dry-run",
        action="store_true",
        default=True,
        help="Preview files that would be deleted (default: true)"
    )
    parser.add_argument(
        "--no-dry-run",
        action="store_false",
        dest="dry_run",
        help="Actually delete files (use with caution!)"
    )
    parser.add_argument(
        "--yes",
        action="store_true",
        help="Skip confirmation prompt (use with --no-dry-run)"
    )
    args = parser.parse_args()

    docs_dir = Path("mkdocs")
    if not docs_dir.exists():
        print(f"❌ Directory {docs_dir} not found.")
        return

    # Build a set of absolute paths for excluded directories (for fast lookup)
    excluded_paths = [docs_dir / d for d in EXCLUDE_DIRS if (docs_dir / d).exists()]

    # Recursively find all .md files, excluding index.md and files inside excluded dirs
    to_delete = []
    for md_file in docs_dir.rglob("*.md"):
        if md_file.name.lower() == "index.md":
            continue
        # Check if this file is inside any excluded directory
        if any(excluded in md_file.parents for excluded in excluded_paths):
            continue
        to_delete.append(md_file)

    if not to_delete:
        print("✅ No .md files to delete (excluding index.md and protected dirs).")
        return

    print(f"Found {len(to_delete)} files to delete:")
    for f in to_delete:
        print(f"  - {f}")

    if args.dry_run:
        print("\n🔍 Dry‑run mode – no files deleted.")
        print("To actually delete, run with --no-dry-run flag.")
        return

    if not args.yes:
        confirm = input(f"\n⚠️  Delete {len(to_delete)} files? (yes/no): ")
        if confirm.lower() != "yes":
            print("❌ Cancelled.")
            return

    for f in to_delete:
        f.unlink()
        print(f"Deleted: {f}")
    print("✅ Done.")

if __name__ == "__main__":
    main()