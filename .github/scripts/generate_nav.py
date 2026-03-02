#!/usr/bin/env python3
"""
Generate hierarchical MkDocs navigation from the docs folder.
- Folder names become section titles.
- index.md becomes a page inside its section (labeled "Overview" by default, or from frontmatter).
- Other .md files become pages under their parent section.
- Files are sorted naturally.
- Updates only the 'nav' section in mkdocs.yml.
- Strips leading emojis from all titles.
"""

import os
import re
import yaml
from pathlib import Path

SECTION_ORDER = [
    "getting-started",
    "core-concepts",
    "aspnet",
    "resultflow",
    "safety-analyzers",
    "architecture",
    "code-examples",
    "testing",
    "reference",
    "community"
]

# Unicode ranges for emojis and common symbols (inclusive)
EMOJI_RANGES = [
    (0x1F000, 0x1F9FF),  # Miscellaneous Symbols and Pictographs, Emoticons, etc.
    (0x2600, 0x26FF),    # Miscellaneous Symbols
    (0x2700, 0x27BF),    # Dingbats
]

def is_emoji(char):
    """Return True if the character's code point falls in any emoji range."""
    code = ord(char)
    return any(start <= code <= end for start, end in EMOJI_RANGES)

def strip_leading_emojis(text):
    """Remove leading emoji characters from text."""
    if not text:
        return text
    # Find first non-emoji character
    for i, ch in enumerate(text):
        if not is_emoji(ch):
            break
    else:
        # All characters are emojis
        return ''
    # Return the rest of the string after the emoji run
    return text[i:]

# Optional: read frontmatter for better titles
try:
    import frontmatter
    HAS_FRONTMATTER = True
except ImportError:
    HAS_FRONTMATTER = False
    print("Note: python-frontmatter not installed. Using filename-based titles.")

# Import modules that provide custom YAML constructors
try:
    import material.extensions.emoji
    import pymdownx.superfences
except ImportError as e:
    print(f"Warning: {e}. YAML loading may fail if custom tags are used.")

MKDOCS_YML = Path("mkdocs.yml")
DOCS_DIR = Path("mkdocs")

def natural_sort_key(path):
    """Sort strings with numbers naturally (e.g., file2 before file10)."""
    return [int(part) if part.isdigit() else part.lower()
            for part in re.split(r'(\d+)', str(path))]

def get_title_from_file(filepath, default_as_filename=True):
    """
    Extract title from frontmatter if available.
    If not, return filename converted to Title Case (unless default_as_filename=False).
    Strips leading emojis from the title.
    """
    title = None
    if HAS_FRONTMATTER:
        try:
            with open(filepath, 'r', encoding='utf-8') as f:
                post = frontmatter.load(f)
                if post.get('title'):
                    title = post['title']
                    print(f"DEBUG: Found title '{title}' in {filepath}")
        except Exception as e:
            print(f"DEBUG: Error reading frontmatter in {filepath}: {e}")
    
    if title is None and default_as_filename:
        title = filepath.stem.replace('-', ' ').title()
    
    # Strip leading emojis if we have a title
    if title:
        title = strip_leading_emojis(title).strip()
        if not title:
            title = "Overview"  # fallback if stripped to nothing
    
    return title

def generate_nav(current_path):
    """
    Recursively build navigation structure.
    Returns a list of dicts suitable for mkdocs.yml 'nav'.
    """
    entries = []
    items = sorted(current_path.iterdir(), key=natural_sort_key)

    # Directories first (they become sections)
    dirs = [p for p in items if p.is_dir() and not p.name.startswith('.')]
    # Sort dirs according to SECTION_ORDER (folders not in the list go at the end)
    dirs.sort(key=lambda d: SECTION_ORDER.index(d.name) if d.name in SECTION_ORDER else len(SECTION_ORDER))
    for dir_path in dirs:
        dir_name = dir_path.name
        index_file = dir_path / "index.md"
        sub_nav = generate_nav(dir_path)

        # Section title = folder name (Title Case), then strip emojis
        section_title = dir_name.replace('-', ' ').title()
        section_title = strip_leading_emojis(section_title).strip()
        # Special case for "aspnet" -> "ASP.NET"
        if dir_name.lower() == "aspnet":
            section_title = "ASP.NET"
        # Build the list of entries for this section
        section_entries = []

        # If index.md exists, add it as a page with a friendly label
        if index_file.exists():
            # Try to get a nice title from frontmatter; fallback to "Overview"
            index_title = get_title_from_file(index_file, default_as_filename=False)
            if index_title is None:
                index_title = "Overview"  # default label for the landing page
            section_entries.append({index_title: str(index_file.relative_to(DOCS_DIR)).replace('\\', '/')})

        # Add all other pages from this directory (already in sub_nav)
        section_entries.extend(sub_nav)

        if section_entries:
            entries.append({section_title: section_entries})

    # Process markdown files (excluding index.md) – these are pages without subfolders
    files = [p for p in items if p.is_file() and p.suffix == '.md'
             and p.name != 'index.md' and not p.name.startswith('.')]
    for file_path in files:
        # These files are at the current level (no subfolder). They become top-level pages.
        rel_path = file_path.relative_to(DOCS_DIR)
        title = get_title_from_file(file_path)
        # Ensure title is not empty
        if not title:
            title = "Untitled"
        entries.append({title: str(rel_path).replace('\\', '/')})

    return entries

def main():
    if not MKDOCS_YML.exists():
        print(f"❌ {MKDOCS_YML} not found.")
        return
    if not DOCS_DIR.exists():
        print(f"❌ {DOCS_DIR} not found.")
        return

    print("🔍 Scanning docs/ and generating hierarchical navigation...")
    nav_structure = generate_nav(DOCS_DIR)
    # Prepend Home if it exists (index.md is always there)
    if DOCS_DIR.joinpath("index.md").exists():
        nav_structure.insert(0, {"Home": "index.md"})

    # Load existing mkdocs.yml with FullLoader (handles custom tags)
    with open(MKDOCS_YML, 'r', encoding='utf-8') as f:
        config = yaml.load(f, Loader=yaml.FullLoader) or {}

    # Update only the nav section
    config['nav'] = nav_structure

    # Write back with proper formatting
    with open(MKDOCS_YML, 'w', encoding='utf-8') as f:
        yaml.dump(config, f, indent=2, allow_unicode=True, sort_keys=False)

    print(f"✅ Hierarchical navigation updated in {MKDOCS_YML}")

if __name__ == "__main__":
    main()