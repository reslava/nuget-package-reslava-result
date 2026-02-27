#!/usr/bin/env bash
# new-changelog-entry.sh — Scaffold a new CHANGELOG.md entry for a version.
#
# Usage:
#   ./scripts/new-changelog-entry.sh 1.24.0    # scaffold for specific version
#   ./scripts/new-changelog-entry.sh           # auto-read version from Directory.Build.props
#
# Inserts at the top of CHANGELOG.md (after the header):
#   ## [VERSION] - YYYY-MM-DD
#   ### ✨ Added
#   -
#   ### Stats
#   - N,NNN tests passing across net8.0, net9.0, net10.0 + generator + analyzer tests

set -u

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
CHANGELOG="$REPO_ROOT/CHANGELOG.md"
CACHE_FILE="$REPO_ROOT/scripts/.test-counts-cache"

VERSION=""

for arg in "$@"; do
  case "$arg" in
    -h|--help)
      sed -n '2,13p' "$0" | sed 's/^# \?//'
      exit 0
      ;;
    [0-9]*) VERSION="$arg" ;;
  esac
done

# Auto-read version from Directory.Build.props if not provided
if [[ -z "$VERSION" ]]; then
  VERSION=$(grep -oP '<Version>\K[^<]+' "$REPO_ROOT/Directory.Build.props" | head -1 || true)
  if [[ -n "$VERSION" ]]; then
    echo "No version specified — using version from Directory.Build.props: $VERSION"
  fi
fi

if [[ -z "$VERSION" ]]; then
  echo "ERROR: Could not determine version. Pass it as an argument or set <Version> in Directory.Build.props."
  exit 1
fi

# ─── Validation ───────────────────────────────────────────────────────────────

if [[ ! -f "$CHANGELOG" ]]; then
  echo "ERROR: CHANGELOG.md not found at $CHANGELOG"
  exit 1
fi

if grep -q "^## \[${VERSION}\]" "$CHANGELOG"; then
  echo "ERROR: CHANGELOG.md already has a ## [${VERSION}] entry — nothing to do."
  exit 1
fi

# ─── Resolve test count ───────────────────────────────────────────────────────

TODAY=$(date +%Y-%m-%d)

if [[ -f "$CACHE_FILE" ]]; then
  source "$CACHE_FILE"
  TOTAL_FMT=$(echo "$TOTAL" | sed ':a;s/\B[0-9]\{3\}\>$/,&/;ta')
  echo "Using cached test count: $TOTAL_FMT"
else
  TOTAL_FMT="TODO"
  echo "No cached test counts — run ./scripts/sync-test-counts.sh after tests pass to fill in the count."
fi

# ─── Find insertion point (first ## [N.N.N] line) ────────────────────────────

INSERT_LINE=$(grep -n "^## \[[0-9]" "$CHANGELOG" | head -1 | cut -d: -f1)

if [[ -z "$INSERT_LINE" ]]; then
  echo "ERROR: Could not find an existing version entry in CHANGELOG.md to insert before."
  exit 1
fi

# ─── Build the new entry block ────────────────────────────────────────────────

NEW_ENTRY="## [${VERSION}] - ${TODAY}\n\n### ✨ Added\n-\n\n### Stats\n- ${TOTAL_FMT} tests passing across net8.0, net9.0, net10.0 + generator + analyzer tests\n"

# Insert before INSERT_LINE (blank line separator added after the block)
sed -i "${INSERT_LINE}i\\${NEW_ENTRY}" "$CHANGELOG"

echo ""
echo "Done. Inserted ## [${VERSION}] at line ${INSERT_LINE} of CHANGELOG.md"
echo ""
echo "Next steps:"
echo "  1. Fill in the '### ✨ Added' bullet points"
echo "  2. Add '### 🔧 Fixed' or '### 📝 Changed' sections if needed"
if [[ "$TOTAL_FMT" == "TODO" ]]; then
  echo "  3. Run ./scripts/sync-test-counts.sh to fill in the test count"
fi
