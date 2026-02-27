#!/usr/bin/env bash
# bump-version.sh — Update <Version> in Directory.Build.props.
#
# Usage:
#   ./scripts/bump-version.sh 1.24.0       # set explicit version
#   ./scripts/bump-version.sh --minor      # 1.23.0 → 1.24.0
#   ./scripts/bump-version.sh --patch      # 1.23.0 → 1.23.1
#   ./scripts/bump-version.sh --major      # 1.23.0 → 2.0.0
#   ./scripts/bump-version.sh --dry-run --minor  # preview, no change

set -u

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROPS="$REPO_ROOT/Directory.Build.props"

NEW_VERSION=""
INCREMENT=""
DRY_RUN=false

for arg in "$@"; do
  case "$arg" in
    --dry-run)   DRY_RUN=true ;;
    --minor)     INCREMENT=minor ;;
    --patch)     INCREMENT=patch ;;
    --major)     INCREMENT=major ;;
    -h|--help)
      sed -n '2,10p' "$0" | sed 's/^# \?//'
      exit 0
      ;;
    [0-9]*) NEW_VERSION="$arg" ;;
  esac
done

# ─── Read current version ─────────────────────────────────────────────────────

CURRENT=$(grep -oP '<Version>\K[^<]+' "$PROPS" | head -1 || true)

if [[ -z "$CURRENT" ]]; then
  echo "ERROR: Could not read <Version> from Directory.Build.props."
  exit 1
fi

MAJOR=$(echo "$CURRENT" | cut -d. -f1)
MINOR=$(echo "$CURRENT" | cut -d. -f2)
PATCH=$(echo "$CURRENT" | cut -d. -f3)

# ─── Compute new version ──────────────────────────────────────────────────────

if [[ -n "$NEW_VERSION" ]]; then
  : # explicit version — use as-is
elif [[ -n "$INCREMENT" ]]; then
  case "$INCREMENT" in
    major) NEW_VERSION="$(( MAJOR + 1 )).0.0" ;;
    minor) NEW_VERSION="${MAJOR}.$(( MINOR + 1 )).0" ;;
    patch) NEW_VERSION="${MAJOR}.${MINOR}.$(( PATCH + 1 ))" ;;
  esac
else
  echo "ERROR: Provide a version (e.g. 1.24.0) or an increment flag (--minor, --patch, --major)."
  exit 1
fi

# ─── Validate format ──────────────────────────────────────────────────────────

if ! echo "$NEW_VERSION" | grep -qP '^\d+\.\d+\.\d+$'; then
  echo "ERROR: Version '${NEW_VERSION}' is not in N.N.N format."
  exit 1
fi

if [[ "$NEW_VERSION" == "$CURRENT" ]]; then
  echo "Version is already ${CURRENT} — nothing to do."
  exit 0
fi

# ─── Apply ────────────────────────────────────────────────────────────────────

echo "${CURRENT} → ${NEW_VERSION}"

if [[ "$DRY_RUN" == true ]]; then
  echo "(dry run — no changes made)"
  exit 0
fi

sed -i "s|<Version>${CURRENT}</Version>|<Version>${NEW_VERSION}</Version>|" "$PROPS"

# Verify
AFTER=$(grep -oP '<Version>\K[^<]+' "$PROPS" | head -1 || true)
if [[ "$AFTER" == "$NEW_VERSION" ]]; then
  echo "Updated Directory.Build.props: <Version>${NEW_VERSION}</Version>"
else
  echo "ERROR: Update failed — found '${AFTER}' instead of '${NEW_VERSION}'."
  exit 1
fi
