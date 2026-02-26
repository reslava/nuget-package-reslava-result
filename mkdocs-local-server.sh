#!/bin/bash
set -e  # Exit immediately if any command fails

echo "🧹 Cleaning docs folder (except index.md)..."
python .github/scripts/clean_docs.py --no-dry-run --yes

echo "📄 Splitting README into sections..."
mdsplit ./README.md -l 2 -o mkdocs/ -f -e utf-8

echo "🔤 Removing leading emojis from level 2 headings..."
python .github/scripts/remove_emoji_headings.py --no-dry-run

echo "📝 Adding frontmatter and removing duplicate headings..."
python .github/scripts/add_frontmatter_and_clean_heading.py

echo "🧹 Cleaning up mdsplit output (H1 file, flatten)..."
python .github/scripts/cleanup_mdsplit.py

echo "📂 Organizing files into folders..."
python .github/scripts/organize_docs.py

echo "🔡 Lowercasing all filenames (except index.md)..."
python .github/scripts/lowercase_filenames.py

echo "🗑️ Removing generated Table-of-Contents.md..."
rm -f mkdocs/table-of-contents.md

echo "🔗 Fixing broken links and external references..."
python .github/scripts/fix_broken_links.py --apply

echo "🎨 Adding admonitions to markdown files..."
python .github/scripts/add_admonitions_table.py

echo "🧭 Generating navigation..."
python .github/scripts/generate_nav.py

echo "🏗️ Building MkDocs site..."
mkdocs build

#echo "🏗️ Build DocFX API documentation"
#cd docfx
#docfx metadata docfx.json
#docfx build docfx.json

echo "▶️ Launch web serve"
cd site
python -m http.server

echo "✅ Done!"
