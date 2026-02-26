#!/bin/bash
set -e  # Exit immediately if any command fails

echo "🧹 Cleaning docs folder (except index.md)..."
python .github/scripts/clean_docs.py --no-dry-run --yes

echo "🏗️ Building MkDocs site..."
mkdocs build