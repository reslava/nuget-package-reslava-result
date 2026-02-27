#!/usr/bin/env bash
# new-release-notes.sh — Scaffold a GitHub release notes file for a new version.
#
# Usage:
#   ./scripts/new-release-notes.sh 1.24.0          # create for specific version
#   ./scripts/new-release-notes.sh                 # auto-read version from Directory.Build.props
#
# Creates:
#   docs/github/GITHUB_RELEASE_v{VERSION}.md
#
# Pre-fills:
#   - Version, date, NuGet package links
#   - Test count from .test-counts-cache (if available) or placeholder
#   - Feature section with TODO placeholder

set -u

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
CACHE_FILE="$REPO_ROOT/scripts/.test-counts-cache"

VERSION=""

for arg in "$@"; do
  case "$arg" in
    -h|--help)
      sed -n '2,12p' "$0" | sed 's/^# \?//'
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

OUTPUT="$REPO_ROOT/docs/github/GITHUB_RELEASE_v${VERSION}.md"

if [[ -f "$OUTPUT" ]]; then
  echo "ERROR: $OUTPUT already exists. Delete it first if you want to regenerate."
  exit 1
fi

# ─── Resolve test count ───────────────────────────────────────────────────────

TODAY=$(date +%Y-%m-%d)

if [[ -f "$CACHE_FILE" ]]; then
  # shellcheck disable=SC1090
  source "$CACHE_FILE"
  # Format with commas
  TOTAL_FMT=$(echo "$TOTAL" | sed ':a;s/\B[0-9]\{3\}\>$/,&/;ta')
  echo "Using cached test count: $TOTAL_FMT"
else
  TOTAL_FMT="TODO"
  echo "No cached test counts — run ./scripts/sync-test-counts.sh after tests pass to fill in the count."
fi

# ─── Write release notes file ─────────────────────────────────────────────────

cat > "$OUTPUT" <<EOF
# v${VERSION} — TODO: Release Title

## TODO: Feature 1

TODO: describe

## Test Suite

- ${TOTAL_FMT} tests passing across net8.0, net9.0, net10.0

## NuGet Packages

- [REslava.Result ${VERSION}](https://www.nuget.org/packages/REslava.Result/${VERSION})
- [REslava.Result.SourceGenerators ${VERSION}](https://www.nuget.org/packages/REslava.Result.SourceGenerators/${VERSION})
- [REslava.Result.Analyzers ${VERSION}](https://www.nuget.org/packages/REslava.Result.Analyzers/${VERSION})
EOF

echo ""
echo "Created: docs/github/GITHUB_RELEASE_v${VERSION}.md"
echo ""
echo "Next steps:"
echo "  1. Replace 'TODO: Release Title' with a short release title"
echo "  2. Replace 'TODO: Feature 1' sections with actual feature descriptions"
if [[ "$TOTAL_FMT" == "TODO" ]]; then
  echo "  3. Run ./scripts/sync-test-counts.sh to fill in the test count"
fi
