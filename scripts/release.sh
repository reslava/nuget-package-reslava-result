#!/usr/bin/env bash
# release.sh — Full release pipeline orchestrator.
#
# Usage:
#   ./scripts/release.sh 1.24.0              # full release
#   ./scripts/release.sh 1.24.0 --dry-run   # preview phase 1 changes, no edits
#   ./scripts/release.sh --minor             # auto-increment minor version
#   ./scripts/release.sh --patch             # auto-increment patch version
#
# Pipeline:
#   Phase 1 — Scaffold  (automated)
#     1. bump-version.sh          — update Directory.Build.props
#     2. new-changelog-entry.sh   — insert CHANGELOG skeleton
#     3. new-release-notes.sh     — create GitHub release notes skeleton
#     4. sync-test-counts.sh      — run tests and update all count references
#     5. update-readme-version.sh — Roadmap (Current) + Version History
#
#   *** PAUSE: fill in CHANGELOG and release notes ***
#
#   Phase 2 — Ship     (requires user confirmation)
#     6. git add + commit + push
#     7. validate-release.sh      — pre-tag checks on clean tree (aborts on failure)
#     8. tag-release.sh           — annotated tag + push
#
# Note: Phase 2 stages only the 4 standard release files (Directory.Build.props,
# CHANGELOG.md, README.md, release notes). New feature code should be committed
# separately before running this script. Normal workflow: merge feature branches
# first, then run release.sh to handle the version bump artifacts.

set -u

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
S="$REPO_ROOT/scripts"

VERSION=""
DRY_RUN=false
INCREMENT=""

# ─── Argument parsing ─────────────────────────────────────────────────────────

for arg in "$@"; do
  case "$arg" in
    --dry-run)   DRY_RUN=true ;;
    --minor)     INCREMENT=minor ;;
    --patch)     INCREMENT=patch ;;
    --major)     INCREMENT=major ;;
    -h|--help)
      sed -n '2,18p' "$0" | sed 's/^# \?//'
      exit 0
      ;;
    [0-9]*) VERSION="$arg" ;;
  esac
done

# ─── Resolve version ──────────────────────────────────────────────────────────

# If increment flag given, compute new version from current
if [[ -n "$INCREMENT" && -z "$VERSION" ]]; then
  CURRENT=$(grep -oP '<Version>\K[^<]+' "$REPO_ROOT/Directory.Build.props" | head -1)
  MAJOR=$(echo "$CURRENT" | cut -d. -f1)
  MINOR=$(echo "$CURRENT" | cut -d. -f2)
  PATCH=$(echo "$CURRENT" | cut -d. -f3)
  case "$INCREMENT" in
    major) VERSION="$(( MAJOR + 1 )).0.0" ;;
    minor) VERSION="${MAJOR}.$(( MINOR + 1 )).0" ;;
    patch) VERSION="${MAJOR}.${MINOR}.$(( PATCH + 1 ))" ;;
  esac
fi

# Fall back to Directory.Build.props current value if nothing specified
if [[ -z "$VERSION" ]]; then
  VERSION=$(grep -oP '<Version>\K[^<]+' "$REPO_ROOT/Directory.Build.props" | head -1 || true)
  if [[ -n "$VERSION" ]]; then
    echo "No version specified — using current version from Directory.Build.props: $VERSION"
  fi
fi

if [[ -z "$VERSION" ]]; then
  echo "ERROR: Could not determine version. Pass a version or use --minor/--patch/--major."
  exit 1
fi

# ─── ANSI colors ──────────────────────────────────────────────────────────────

if [[ -t 1 ]]; then
  BOLD='\033[1m'; CYAN='\033[0;36m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RESET='\033[0m'
else
  BOLD='' CYAN='' GREEN='' YELLOW='' RESET=''
fi

header() { echo -e "\n${BOLD}${CYAN}══ $1 ══${RESET}"; }
ok()     { echo -e "  ${GREEN}✓${RESET}  $1"; }
note()   { echo -e "  ${YELLOW}→${RESET}  $1"; }

# ─── Dry-run guard ────────────────────────────────────────────────────────────

if [[ "$DRY_RUN" == true ]]; then
  echo -e "${BOLD}Dry run — Phase 1 preview for v${VERSION}${RESET}"
  echo ""
  CURRENT=$(grep -oP '<Version>\K[^<]+' "$REPO_ROOT/Directory.Build.props" | head -1)
  note "bump-version:          ${CURRENT} → ${VERSION}"
  note "new-changelog-entry:   insert ## [${VERSION}] into CHANGELOG.md"
  note "new-release-notes:     create docs/github/GITHUB_RELEASE_v${VERSION}.md"
  note "sync-test-counts:      run tests + update counts everywhere"
  note "update-readme-version: Roadmap (Current) + Version History"
  echo ""
  echo "No changes made."
  exit 0
fi

# ═══════════════════════════════════════════════════════════════════════════════
# PHASE 1 — Scaffold
# ═══════════════════════════════════════════════════════════════════════════════

header "Phase 1 — Scaffold v${VERSION}"

# 1. Bump version
echo ""
note "Step 1: bump-version.sh ${VERSION}"
bash "$S/bump-version.sh" "$VERSION"
ok "Version bumped"

# 2. Scaffold CHANGELOG entry
echo ""
note "Step 2: new-changelog-entry.sh ${VERSION}"
bash "$S/new-changelog-entry.sh" "$VERSION"
ok "CHANGELOG entry created"

# 3. Scaffold GitHub release notes
echo ""
note "Step 3: new-release-notes.sh ${VERSION}"
bash "$S/new-release-notes.sh" "$VERSION"
ok "Release notes file created"

# 4. Run tests + sync counts
echo ""
note "Step 4: sync-test-counts.sh (runs full test suite)"
bash "$S/sync-test-counts.sh"
ok "Tests passed, counts synced"

# 5. Update README
echo ""
note "Step 5: update-readme-version.sh ${VERSION}"
bash "$S/update-readme-version.sh" "$VERSION"
ok "README Roadmap + Version History updated"

# ═══════════════════════════════════════════════════════════════════════════════
# PAUSE
# ═══════════════════════════════════════════════════════════════════════════════

echo ""
echo -e "${BOLD}${YELLOW}══ PAUSE — Fill in release content ══${RESET}"
echo ""
echo "  Edit these files before continuing:"
echo "    CHANGELOG.md                              — fill in Added/Fixed/Changed bullets"
echo "    docs/github/GITHUB_RELEASE_v${VERSION}.md  — fill in feature descriptions"
echo "    README.md Roadmap v${VERSION}              — fill in bullet points"
echo ""
read -r -p "Press Enter when done, or Ctrl+C to abort... "

# ═══════════════════════════════════════════════════════════════════════════════
# PHASE 2 — Ship
# ═══════════════════════════════════════════════════════════════════════════════

header "Phase 2 — Ship v${VERSION}"

# 6. git add + commit + push
echo ""
note "Step 6: git add + commit + push"
git -C "$REPO_ROOT" add \
  Directory.Build.props \
  CHANGELOG.md \
  README.md \
  "docs/github/GITHUB_RELEASE_v${VERSION}.md"

git -C "$REPO_ROOT" commit -m "$(cat <<EOF
chore: release v${VERSION}
EOF
)"
git -C "$REPO_ROOT" push origin
ok "Committed and pushed"

# 7. Validate (tree is clean after commit)
echo ""
note "Step 7: validate-release.sh ${VERSION} --skip-tests"
if ! bash "$S/validate-release.sh" "$VERSION" --skip-tests; then
  echo ""
  echo "ERROR: Validation failed — fix the issues above before tagging."
  exit 1
fi
ok "All checks passed"

# 8. Tag + push
echo ""
note "Step 8: tag-release.sh ${VERSION} --skip-tests"
bash "$S/tag-release.sh" "$VERSION" --skip-tests
ok "Tagged v${VERSION}"

# ─── Done ─────────────────────────────────────────────────────────────────────

echo ""
echo -e "${GREEN}${BOLD}✓ v${VERSION} released!${RESET}"
echo ""
echo "  Tag:    v${VERSION}"
echo "  NuGet:  https://www.nuget.org/packages/REslava.Result/${VERSION}"
