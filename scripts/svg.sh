#!/bin/bash
# svg.sh
# Full SVG pipeline — run locally before each release.
#
# Pipeline:
#   1. dotnet build Demo          → refreshes obj/Generated/*_Flows.g.cs
#   2. generate_flow_catalog.py   → exports *.mmd files into images/
#   3. mermaid-to-svg.sh          → converts *.mmd → *.svg (width-aware + SVGO)
#
# Requirements (local only — not installed in CI):
#   dotnet, python3, mmdc (@mermaid-js/mermaid-cli), svgo
#
# Usage:
#   bash scripts/svg.sh
#   (run from repo root)

set -e

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DEMO_PROJECT="$REPO_ROOT/samples/REslava.Result.Flow.Demo"
IMAGES_DIR="$REPO_ROOT/images"

echo "══════════════════════════════════════════════════════════"
echo "  svg.sh — ResultFlow SVG pipeline"
echo "══════════════════════════════════════════════════════════"

# ── Step 1: Build Demo to refresh generated files ────────────────────────────
echo ""
echo "Step 1: dotnet build Demo"
echo "──────────────────────────────────────────────────────────"
dotnet build "$DEMO_PROJECT" --no-restore -v q

# ── Step 2: Export .mmd files ─────────────────────────────────────────────────
echo ""
echo "Step 2: export .mmd files → $IMAGES_DIR"
echo "──────────────────────────────────────────────────────────"
python3 "$REPO_ROOT/scripts/generate_flow_catalog.py" \
  --project "$DEMO_PROJECT" \
  --export-mmd "$IMAGES_DIR"

# ── Step 3: Convert .mmd → .svg ──────────────────────────────────────────────
echo ""
echo "Step 3: mermaid-to-svg (width-aware + SVGO)"
echo "──────────────────────────────────────────────────────────"
cd "$IMAGES_DIR"
bash "$REPO_ROOT/scripts/mermaid-to-svg.sh"

echo ""
echo "══════════════════════════════════════════════════════════"
echo "  Done. SVGs + PNGs written to $IMAGES_DIR"
echo "  Commit images/*.svg and images/*.png before tagging the release."
echo "══════════════════════════════════════════════════════════"
