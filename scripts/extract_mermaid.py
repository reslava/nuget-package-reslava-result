#!/usr/bin/env python3
"""
Extract the inner content of Mermaid diagrams from a source file (e.g., C#).
Blocks look like:

    /* ... */
    ```mermaid
    ---
    title: DiagramName
    ---
    flowchart ...
    ```*/
    /* ... */

The script saves each diagram's content (including frontmatter) as <cleaned_title>.mmd.
No surrounding fences are added.
"""

import re
import sys
from pathlib import Path

# Unicode ranges for emoji removal
EMOJI_PATTERN = re.compile(
    "["
    "\U0001F600-\U0001F64F"  # emoticons
    "\U0001F300-\U0001F5FF"  # symbols & pictographs
    "\U0001F680-\U0001F6FF"  # transport & map symbols
    "\U0001F1E0-\U0001F1FF"  # flags (iOS)
    "\U00002702-\U000027B0"
    "\U000024C2-\U0001F251"
    "\U0001F900-\U0001F9FF"  # supplemental symbols
    "\U0001FA70-\U0001FAFF"  # symbols extended-A
    "\U00002600-\U000026FF"  # misc symbols
    "\U00002B50-\U00002B55"  # stars
    "]+",
    flags=re.UNICODE,
)

def extract_title(frontmatter: str) -> str | None:
    """Extract the first word of the title from YAML frontmatter."""
    for line in frontmatter.splitlines():
        if line.lower().startswith("title:"):
            title_part = line.split(":", 1)[1].strip()
            first_word = title_part.split()[0] if title_part else ""
            clean = EMOJI_PATTERN.sub("", first_word).strip()
            return clean if clean else None
    return None

def extract_mermaid_blocks(content: str):
    """Generator that yields (title, inner_content) for each mermaid block."""
    lines = content.splitlines()
    i = 0
    while i < len(lines):
        line = lines[i].rstrip()
        # Look for opening line
        if line.strip() == "```mermaid":
            start = i
            i += 1
            block_lines = []
            # Collect until a line that starts with "```" (closing marker)
            while i < len(lines) and not lines[i].lstrip().startswith("```"):
                block_lines.append(lines[i].rstrip())
                i += 1
            # Check if we found a closing marker
            if i < len(lines) and lines[i].lstrip().startswith("```"):
                # We have a complete block
                inner_content = "\n".join(block_lines)
                # Frontmatter is expected between "---" lines
                if block_lines and block_lines[0].startswith("---"):
                    try:
                        end_fm = block_lines.index("---", 1)
                        frontmatter = "\n".join(block_lines[1:end_fm])
                        title = extract_title(frontmatter)
                        if title:
                            yield title, inner_content
                        else:
                            print(f"⚠️  Skipping block at line {start+1}: no valid title found.", file=sys.stderr)
                    except ValueError:
                        print(f"⚠️  Skipping block at line {start+1}: incomplete frontmatter.", file=sys.stderr)
                else:
                    print(f"⚠️  Skipping block at line {start+1}: missing frontmatter.", file=sys.stderr)
            else:
                print(f"⚠️  Skipping block at line {start+1}: no closing '```' found.", file=sys.stderr)
            # Move past the closing line (if we are still inside the file)
            if i < len(lines):
                i += 1
        else:
            i += 1

def main():
    if len(sys.argv) != 2:
        print("Usage: extract_mermaid_inner.py <source_file>")
        sys.exit(1)

    source = Path(sys.argv[1])
    if not source.is_file():
        print(f"Error: file '{source}' not found.")
        sys.exit(1)

    content = source.read_text(encoding="utf-8")
    diagrams = list(extract_mermaid_blocks(content))

    if not diagrams:
        print("No mermaid diagrams found.")
        return

    for title, inner_content in diagrams:
        # Sanitise filename
        safe_title = re.sub(r'[^\w\-]', '_', title)
        out_file = Path(f"{safe_title}.mmd")
        counter = 1
        while out_file.exists():
            out_file = Path(f"{safe_title}_{counter}.mmd")
            counter += 1
        out_file.write_text(inner_content, encoding="utf-8")
        print(f"✅ Saved {out_file}")

if __name__ == "__main__":
    main()