#!/usr/bin/env python3
"""
Add frontmatter title from the first heading and remove that heading.
Prevents duplicate page titles when Material theme displays the frontmatter title.
"""

import re
from pathlib import Path
import frontmatter

DOCS_DIR = Path("mkdocs")

# These folders contain hand-crafted static content — never touch them.
SKIP_DIRS = {
    DOCS_DIR / "architecture" / "solid",
    DOCS_DIR / "code-examples" / "samples",
    DOCS_DIR / "reference" / "api-doc",
}

def should_skip(filepath):
    if filepath.name == "index.md":
        return True
    for skip_dir in SKIP_DIRS:
        try:
            filepath.relative_to(skip_dir)
            return True
        except ValueError:
            pass
    return False

def process_file(filepath):
    """Extract first heading, set as frontmatter title, and remove heading line."""
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Load existing frontmatter if any
    try:
        post = frontmatter.loads(content)
        body = post.content
        metadata = post.metadata
    except Exception:
        # No valid frontmatter – treat whole file as body
        body = content
        metadata = {}

    lines = body.splitlines()
    heading_line_idx = None
    heading_text = None

    in_code_block = False
    for i, line in enumerate(lines):
        stripped = line.strip()
        if stripped.startswith('```') or stripped.startswith('~~~'):
            in_code_block = not in_code_block
            continue
        if in_code_block:
            continue
        if stripped.startswith('# '):
            heading_text = stripped[2:].strip()
            heading_line_idx = i
            break
        elif stripped.startswith('## '):
            heading_text = stripped[3:].strip()
            heading_line_idx = i
            break
        elif stripped.startswith('### '):
            heading_text = stripped[4:].strip()
            heading_line_idx = i
            break

    if heading_text and not metadata.get('title'):
        # Strip leading section-number prefix, e.g. "7.1. ", "21.3. ", "16.4.1. "
        metadata['title'] = re.sub(r'^[\d]+(?:\.[\d]+)*\.\s*', '', heading_text)

    if heading_line_idx is not None:
        # Remove the heading line
        del lines[heading_line_idx]
        # Also remove any blank lines immediately after? (optional)
        # while heading_line_idx < len(lines) and lines[heading_line_idx].strip() == '':
        #     del lines[heading_line_idx]

    new_body = '\n'.join(lines).strip()
    new_post = frontmatter.Post(new_body, **metadata)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(frontmatter.dumps(new_post))
    print(f"✅ Processed: {filepath}")

def main():
    if not DOCS_DIR.exists():
        print(f"❌ {DOCS_DIR} not found.")
        return

    for md_file in DOCS_DIR.rglob("*.md"):
        if should_skip(md_file):
            continue
        process_file(md_file)

if __name__ == "__main__":
    main()