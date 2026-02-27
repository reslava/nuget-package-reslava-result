#!/usr/bin/env bash
# update-readme-version.sh — Update README.md Roadmap and Version History for a new release.
#
# Usage:
#   ./scripts/update-readme-version.sh 1.24.0 "Short description"
#   ./scripts/update-readme-version.sh 1.24.0   # description defaults to "TODO: describe"
#
# What it changes in README.md:
#   Roadmap:         Strips "(Current)" from previous version, inserts new (Current) block above it
#   Version History: Prepends "- **v{VERSION}** - {description}" to the top of the list

set -u

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
README="$REPO_ROOT/README.md"

VERSION=""
DESCRIPTION=""

# ─── Argument parsing ─────────────────────────────────────────────────────────

for arg in "$@"; do
  case "$arg" in
    -h|--help)
      sed -n '2,11p' "$0" | sed 's/^# \?//'
      exit 0
      ;;
    [0-9]*) VERSION="$arg" ;;
    *)      DESCRIPTION="$arg" ;;
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

if [[ -z "$DESCRIPTION" ]]; then
  DESCRIPTION="TODO: describe"
fi

# ─── Validation ───────────────────────────────────────────────────────────────

if [[ ! -f "$README" ]]; then
  echo "ERROR: README.md not found at $README"
  exit 1
fi

# Check new version isn't already present
if grep -q "### v${VERSION} (Current) ✅" "$README"; then
  echo "ERROR: README.md already has '### v${VERSION} (Current) ✅' — nothing to do."
  exit 1
fi

# Check there's an existing (Current) line to demote
if ! grep -q "(Current) ✅" "$README"; then
  echo "ERROR: No '(Current) ✅' line found in README.md. Cannot determine previous version."
  exit 1
fi

# ─── Step 1: Roadmap — demote old (Current), insert new block above it ────────

# Find old current line number
OLD_LINE=$(grep -n "(Current) ✅" "$README" | head -1 | cut -d: -f1)
OLD_HEADER=$(sed -n "${OLD_LINE}p" "$README")
OLD_VERSION=$(echo "$OLD_HEADER" | grep -oP '(?<=### v)[0-9]+\.[0-9]+\.[0-9]+')

if [[ -z "$OLD_VERSION" ]]; then
  echo "ERROR: Could not extract old version from line: $OLD_HEADER"
  exit 1
fi

echo "Demoting v${OLD_VERSION} (Current) → v${OLD_VERSION} ✅"
echo "Inserting  v${VERSION} (Current) ✅ above it"

# Demote old (Current) marker on that line
sed -i "${OLD_LINE}s/ (Current) ✅/ ✅/" "$README"

# Insert new (Current) block BEFORE the demoted line (now at same line number)
# Use line-number insertion: i\ inserts before line N
# We insert 3 lines: heading, bullet placeholder, blank line
sed -i "${OLD_LINE}i\\### v${VERSION} (Current) ✅\n- TODO: describe features\n" "$README"

# ─── Step 2: Version History — prepend new entry at top of list ───────────────

# Find the first "- **v" line in the Version History section
VH_LINE=$(grep -n "^- \*\*v[0-9]" "$README" | head -1 | cut -d: -f1)

if [[ -z "$VH_LINE" ]]; then
  echo "WARNING: Could not find Version History list in README.md. Skipping that section."
else
  echo "Prepending  v${VERSION} entry to Version History (line ${VH_LINE})"
  sed -i "${VH_LINE}i\\- **v${VERSION}** - ${DESCRIPTION}" "$README"
fi

# ─── Summary ──────────────────────────────────────────────────────────────────

echo ""
echo "Done. README.md updated:"
echo "  Roadmap:         ### v${VERSION} (Current) ✅  (above demoted v${OLD_VERSION})"
echo "  Version History: - **v${VERSION}** - ${DESCRIPTION}"
echo ""
echo "Next steps:"
echo "  1. Fill in the Roadmap bullet points for v${VERSION}"
if [[ "$DESCRIPTION" == "TODO: describe" ]]; then
  echo "  2. Update the Version History entry with a real description"
fi
