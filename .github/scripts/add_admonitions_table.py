#!/usr/bin/env python3
"""
Enhanced admonition inserter for MkDocs markdown files.
Detects patterns and wraps them with Material for MkDocs admonitions.
Now respects fenced code blocks – does not add admonitions inside them.
"""

import re
from pathlib import Path

# ================= CONFIGURATION =================
# Patterns: (regex, admonition_type, [lookahead_lines])
# lookahead_lines: 0 = auto-detect indented block or code fence
PATTERNS = [
    (r'New in v\d+\.\d+\.\d+', 'new', 0),
    (r'v\d+\.\d+\.\d+.*(new|feature|added)', 'new', 0),
    (r'RESL\d{4}', 'warning', 0),
    (r'^\*\*Note:\*\*', 'note', 1),
    (r'^\*\*Tip:\*\*', 'tip', 1),
    (r'^\*\*Warning:\*\*', 'warning', 1),
    (r'^\*\*Caution:\*\*', 'caution', 1),
    (r'^\*\*Important:\*\*', 'important', 1),
    (r'^\*\*Example:\*\*', 'example', 0),
]

def is_table_row(line):
    """Return True if line looks like a markdown table row."""
    stripped = line.strip()
    return stripped.startswith('|') and '|' in stripped[1:]

def mark_code_block_lines(lines):
    """
    Return a list of booleans indicating whether each line is inside a fenced code block
    or an HTML <div> block (e.g. MkDocs grid cards).  Lines in either are protected from
    admonition insertion.
    """
    in_fence = False
    in_div = False
    flags = [False] * len(lines)
    for i, line in enumerate(lines):
        stripped = line.strip()
        if stripped.startswith('```') or stripped.startswith('~~~'):
            in_fence = not in_fence
        if '<div' in stripped:
            in_div = True
        flags[i] = in_fence or in_div
        if '</div>' in stripped:
            in_div = False
    return flags

def wrap_comparison_table(lines, start_idx, in_code_block):
    """
    Detect the feature comparison table and wrap it in an admonition.
    Returns (new_lines, new_index) or (None, start_idx) if not a match.
    """
    if in_code_block[start_idx]:
        return None, start_idx
    line = lines[start_idx].rstrip('\n')
    # Skip if line is indented (already part of an admonition)
    if line.startswith((' ', '\t')):
        return None, start_idx
    # Check if this is the specific comparison table header
    if not ('REslava.Result' in line and 'FluentResults' in line and 'ErrorOr' in line):
        return None, start_idx

    # Collect all consecutive table lines
    table_lines = [line]
    i = start_idx + 1
    while i < len(lines) and is_table_row(lines[i]):
        table_lines.append(lines[i].rstrip('\n'))
        i += 1

    # Build the admonition block
    admon = ['!!! example "Feature Comparison"']
    for tbl_line in table_lines:
        admon.append(f'    {tbl_line}')
    admon.append('')  # blank line after admonition
    return admon, i

def add_admonitions_to_file(filepath, patterns):
    """Process a single file, return True if file was modified."""
    with open(filepath, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    # Precompute which lines are inside fenced code blocks
    in_code_block = mark_code_block_lines(lines)

    new_lines = []
    i = 0
    modified = False

    while i < len(lines):
        line = lines[i].rstrip('\n')

        # Skip empty lines (keep them)
        if line == '':
            new_lines.append(line)
            i += 1
            continue

        # Skip already existing admonitions
        if line.strip().startswith(('!!!', '???')):
            new_lines.append(line)
            i += 1
            continue

        # Skip lines that are indented – they are already part of a previous admonition/code block
        if line.startswith((' ', '\t')):
            new_lines.append(line)
            i += 1
            continue

        # Skip lines that are inside a fenced code block
        if in_code_block[i]:
            new_lines.append(line)
            i += 1
            continue

        # Check for comparison table first
        admon, next_i = wrap_comparison_table(lines, i, in_code_block)
        if admon is not None:
            new_lines.extend(admon)
            i = next_i
            modified = True
            continue

        # Try regular patterns
        matched = False
        for pattern, admon_type, lookahead in patterns:
            if re.search(pattern, line, re.IGNORECASE):
                matched = True
                new_lines.append(f'!!! {admon_type} "{line.strip()}"')
                i += 1
                modified = True

                # Collect content
                content_lines = []
                if lookahead > 0:
                    # Fixed number of following lines
                    for _ in range(lookahead):
                        if i < len(lines):
                            content_lines.append(lines[i].rstrip('\n'))
                            i += 1
                else:
                    # Auto-detect indented block or code fence
                    while i < len(lines):
                        next_line = lines[i].rstrip('\n')
                        # Stop on blank line before a heading
                        if next_line.strip() == '' and (i+1 < len(lines) and lines[i+1].strip().startswith('#')):
                            break
                        if next_line.strip().startswith('```'):
                            # Capture whole code block
                            content_lines.append(next_line)
                            i += 1
                            while i < len(lines) and not lines[i].rstrip('\n').strip().startswith('```'):
                                content_lines.append(lines[i].rstrip('\n'))
                                i += 1
                            if i < len(lines):
                                content_lines.append(lines[i].rstrip('\n'))
                                i += 1
                            continue
                        if next_line.startswith('    ') or next_line.startswith('\t'):
                            content_lines.append(next_line)
                            i += 1
                        else:
                            break
                # Add indented content
                for cl in content_lines:
                    new_lines.append(f'    {cl}')
                new_lines.append('')
                break

        if not matched:
            new_lines.append(line)
            i += 1

    # Write back only if changed
    new_content = '\n'.join(new_lines)
    with open(filepath, 'r', encoding='utf-8') as f:
        original_content = f.read()
    if new_content != original_content:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(new_content)
        return True
    return False

def main():
    docs_dir = Path('mkdocs')
    if not docs_dir.exists():
        print(f"❌ Error: {docs_dir} directory not found.")
        return

    total_files = 0
    modified_files = 0

    for md_file in docs_dir.rglob('*.md'):
        total_files += 1
        try:
            if add_admonitions_to_file(md_file, PATTERNS):
                print(f"✅ Modified: {md_file}")
                modified_files += 1
            else:
                print(f"⏺️  Unchanged: {md_file}")
        except Exception as e:
            print(f"❌ Error processing {md_file}: {e}")

    print("\n" + "="*50)
    print(f"SUMMARY: {modified_files} of {total_files} files modified.")
    if modified_files:
        print("Run `git diff` to see the changes.")
    else:
        print("No files were changed – no matching patterns found or all already processed.")

if __name__ == '__main__':
    main()