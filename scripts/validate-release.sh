#!/usr/bin/env bash
# validate-release.sh — Pre-release validation checklist for REslava.Result packages.
#
# Usage:
#   ./scripts/validate-release.sh 1.23.0        # validate a specific version
#   ./scripts/validate-release.sh               # auto-read version from Directory.Build.props
#   ./scripts/validate-release.sh --skip-tests  # skip dotnet test (quick structural checks only)
#
# Checks:
#   1. Directory.Build.props <Version> matches argument
#   2. CHANGELOG.md has ## [VERSION] entry
#   3. docs/github/GITHUB_RELEASE_v{VERSION}.md exists
#   4. README.md Roadmap shows this version as (Current) ✅
#   5. README.md Version History has **v{VERSION}** entry
#   6. All tests pass (dotnet test --configuration Release)
#   7. No uncommitted git changes
#   8. No TODO / placeholder text in release notes
#   9. Test counts in docs match actual test output

set -u

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
CACHE_FILE="$REPO_ROOT/scripts/.test-counts-cache"
TMPFILE="$REPO_ROOT/scripts/.test-output-tmp"

SKIP_TESTS=false
VERSION=""
FAIL=0

# ANSI colors (gracefully degraded when not a terminal)
if [[ -t 1 ]]; then
  RED='\033[0;31m'
  GREEN='\033[0;32m'
  YELLOW='\033[1;33m'
  BOLD='\033[1m'
  RESET='\033[0m'
else
  RED='' GREEN='' YELLOW='' BOLD='' RESET=''
fi

# ─── Argument parsing ─────────────────────────────────────────────────────────

for arg in "$@"; do
  case "$arg" in
    --skip-tests) SKIP_TESTS=true ;;
    -h|--help)
      sed -n '2,13p' "$0" | sed 's/^# \?//'
      exit 0
      ;;
    [0-9]*) VERSION="$arg" ;;
  esac
done

# Auto-read version from Directory.Build.props if not provided
if [[ -z "$VERSION" ]]; then
  VERSION=$(sed -n 's/.*<Version>\([^<]*\).*/\1/p' "$REPO_ROOT/Directory.Build.props" | head -1 || true)
  if [[ -n "$VERSION" ]]; then
    echo "No version specified — using version from Directory.Build.props: $VERSION"
  fi
fi

if [[ -z "$VERSION" ]]; then
  echo "ERROR: Could not determine version. Pass it as an argument or set <Version> in Directory.Build.props."
  exit 1
fi

echo ""
echo -e "${BOLD}Validating release v${VERSION}${RESET}"
echo "──────────────────────────────────────────"
echo ""

# ─── Helpers ─────────────────────────────────────────────────────────────────

pass() { echo -e "  ${GREEN}✓${RESET}  $1"; }
fail() { echo -e "  ${RED}✗${RESET}  $1"; FAIL=$(( FAIL + 1 )); }
warn() { echo -e "  ${YELLOW}!${RESET}  $1"; }

fmt() { echo "$1" | sed ':a;s/\B[0-9]\{3\}\>$/,&/;ta'; }

# ─── Check 1: Directory.Build.props version ──────────────────────────────────

echo -e "${BOLD}1. Version in Directory.Build.props${RESET}"

PROPS_VERSION=$(sed -n 's/.*<Version>\([^<]*\).*/\1/p' "$REPO_ROOT/Directory.Build.props" | head -1 || true)
if [[ "$PROPS_VERSION" == "$VERSION" ]]; then
  pass "Directory.Build.props: <Version>${PROPS_VERSION}</Version>"
else
  fail "Directory.Build.props has version '${PROPS_VERSION}', expected '${VERSION}'"
fi
echo ""

# ─── Check 2: CHANGELOG.md entry ─────────────────────────────────────────────

echo -e "${BOLD}2. CHANGELOG.md entry${RESET}"

if grep -q "^## \[${VERSION}\]" "$REPO_ROOT/CHANGELOG.md"; then
  pass "CHANGELOG.md has ## [${VERSION}] entry"
else
  fail "CHANGELOG.md is missing ## [${VERSION}] entry"
fi
echo ""

# ─── Check 3: GitHub release notes file ──────────────────────────────────────

echo -e "${BOLD}3. GitHub release notes file${RESET}"

RELEASE_FILE="$REPO_ROOT/docs/github/GITHUB_RELEASE_v${VERSION}.md"
if [[ -f "$RELEASE_FILE" ]]; then
  pass "docs/github/GITHUB_RELEASE_v${VERSION}.md exists"
else
  fail "docs/github/GITHUB_RELEASE_v${VERSION}.md not found"
fi
echo ""

# ─── Check 4: README.md Roadmap (Current) marker ─────────────────────────────

echo -e "${BOLD}4. README.md Roadmap — (Current) ✅ marker${RESET}"

if grep -q "v${VERSION} (Current) ✅" "$REPO_ROOT/README.md"; then
  pass "README.md Roadmap: v${VERSION} (Current) ✅"
else
  fail "README.md Roadmap does not show 'v${VERSION} (Current) ✅'"
  CURRENT_LINE=$(grep -m1 "(Current) ✅" "$REPO_ROOT/README.md" || true)
  if [[ -n "$CURRENT_LINE" ]]; then
    warn "Currently marked as (Current): $(echo "$CURRENT_LINE" | sed 's/^[[:space:]]*//' | cut -c1-80)"
  fi
fi
echo ""

# ─── Check 5: README.md Version History ──────────────────────────────────────

echo -e "${BOLD}5. README.md Version History entry${RESET}"

if grep -q "\*\*v${VERSION}\*\*" "$REPO_ROOT/README.md"; then
  pass "README.md Version History: **v${VERSION}** entry found"
else
  fail "README.md Version History is missing **v${VERSION}** entry"
fi
echo ""

# ─── Check 6: Test suite ─────────────────────────────────────────────────────

echo -e "${BOLD}6. Test suite${RESET}"

if [[ "$SKIP_TESTS" == true ]]; then
  warn "Skipping tests (--skip-tests). Pass/fail status not verified."
else
  echo "  Running tests (this may take a minute)..."
  dotnet test "$REPO_ROOT" --configuration Release --verbosity minimal 2>&1 \
    | tr -d '\r' > "$TMPFILE" || true

  FAILED_COUNT=$(grep -oE 'Failed:[[:space:]]+[0-9]+' "$TMPFILE" | sed 's/Failed:[[:space:]]*//' | awk '{s+=$1} END{print s+0}')
  PASSED_COUNT=$(grep -oE 'Passed:[[:space:]]+[0-9]+' "$TMPFILE" | sed 's/Passed:[[:space:]]*//' | awk '{s+=$1} END{print s+0}')

  if [[ "$FAILED_COUNT" -gt 0 ]]; then
    fail "$FAILED_COUNT test(s) failed — fix all failures before tagging"
    grep '^Failed' "$TMPFILE" | sed 's/^/      /'
  else
    pass "$PASSED_COUNT tests passed across all TFMs"

    # Cache counts for check 9
    CORE_LINES=$(grep -E '^Passed!' "$TMPFILE" | grep 'REslava\.Result\.Tests\.dll' || true)
    TFM_COUNT=$(echo "$CORE_LINES" | grep -c '.' || echo 0)
    CORE_PER_TFM=$(echo "$CORE_LINES" | head -1 | grep -oE 'Passed:[[:space:]]+[0-9]+' | sed 's/Passed:[[:space:]]*//' || true)
    GENERATOR=$(grep -E '^Passed!' "$TMPFILE" | grep 'AspNetCore\.Tests\.dll' | grep -oE 'Passed:[[:space:]]+[0-9]+' | sed 's/Passed:[[:space:]]*//' || true)
    RESULTFLOW=$(grep -E '^Passed!' "$TMPFILE" | grep 'ResultFlow\.Tests\.dll' | grep -oE 'Passed:[[:space:]]+[0-9]+' | sed 's/Passed:[[:space:]]*//' || true)
    RESULTFLOW=${RESULTFLOW:-0}
    ANALYZER=$(grep -E '^Passed!' "$TMPFILE" | grep 'Analyzers\.Tests\.dll' | grep -oE 'Passed:[[:space:]]+[0-9]+' | sed 's/Passed:[[:space:]]*//' || true)
    FLUENT=$(grep -E '^Passed!' "$TMPFILE" | grep 'FluentValidation\.Tests\.dll' | grep -oE 'Passed:[[:space:]]+[0-9]+' | sed 's/Passed:[[:space:]]*//' || true)
    FLUENT=${FLUENT:-0}

    HTTP_LINES=$(grep -E '^Passed!' "$TMPFILE" | grep 'Http\.Tests\.dll' || true)
    HTTP_TFM_COUNT=$(echo "$HTTP_LINES" | grep -c '.' 2>/dev/null || echo 0)
    HTTP_PER_TFM=$(echo "$HTTP_LINES" | head -1 | grep -oE 'Passed:[[:space:]]+[0-9]+' | sed 's/Passed:[[:space:]]*//' || true)
    HTTP_PER_TFM=${HTTP_PER_TFM:-0}
    HTTP_TOTAL=$(( HTTP_PER_TFM * HTTP_TFM_COUNT ))

    # Save the authoritative PASSED_COUNT as TOTAL — covers all test projects
    # including REslava.Result.Flow.Tests and any future suites without needing
    # per-suite extraction logic.
    cat > "$CACHE_FILE" <<EOF
TOTAL=$PASSED_COUNT
EOF
  fi
  rm -f "$TMPFILE"
fi
echo ""

# ─── Check 7: No uncommitted git changes ─────────────────────────────────────

echo -e "${BOLD}7. Git working tree${RESET}"

GIT_STATUS=$(git -C "$REPO_ROOT" status --porcelain 2>/dev/null || true)
if [[ -z "$GIT_STATUS" ]]; then
  pass "Working tree is clean — no uncommitted changes"
else
  CHANGED=$(echo "$GIT_STATUS" | grep -c '.' || echo 0)
  fail "$CHANGED uncommitted change(s) — commit or stash before tagging"
  echo "$GIT_STATUS" | head -10 | sed 's/^/      /'
  if [[ "$CHANGED" -gt 10 ]]; then
    warn "  ... ($(( CHANGED - 10 )) more files not shown)"
  fi
fi
echo ""

# ─── Check 8: No TODO / placeholder in release notes ─────────────────────────

echo -e "${BOLD}8. Release notes completeness${RESET}"

if [[ -f "$RELEASE_FILE" ]]; then
  TODO_LINES=$(grep -inE '\bTODO\b|placeholder|\bdescribe\b|fill in|\bTBD\b' "$RELEASE_FILE" || true)
  if [[ -z "$TODO_LINES" ]]; then
    pass "No TODO/placeholder text in GITHUB_RELEASE_v${VERSION}.md"
  else
    TODO_COUNT=$(echo "$TODO_LINES" | grep -c '.' || echo 0)
    fail "$TODO_COUNT TODO/placeholder line(s) in GITHUB_RELEASE_v${VERSION}.md"
    echo "$TODO_LINES" | head -5 | sed 's/^/      /'
    if [[ "$TODO_COUNT" -gt 5 ]]; then
      warn "  ... ($(( TODO_COUNT - 5 )) more lines not shown)"
    fi
  fi
else
  warn "Skipping (release file not found — see check 3)"
fi
echo ""

# ─── Check 9: Test count floor ────────────────────────────────────────────────
#
# We use a FLOOR threshold (not an exact count) to avoid updating docs after
# every release. Only update TEST_FLOOR and the README.md badge together when
# the suite crosses the next hundred mark (e.g. 3900 → 4000).
# See CLAUDE.md §6 "Test Count Convention" for the full rule.

echo -e "${BOLD}9. Test count floor${RESET}"

# ── Memorized floor — update when test count crosses the next hundred ──────────
TEST_FLOOR=3900
# ─────────────────────────────────────────────────────────────────────────────

if [[ -f "$CACHE_FILE" ]]; then
  # shellcheck disable=SC1090
  source "$CACHE_FILE"

  # 9a. Actual count must be at or above the floor
  if [[ "$TOTAL" -lt "$TEST_FLOOR" ]]; then
    fail "Test count ${TOTAL} is below the floor ${TEST_FLOOR} — possible regression or floor not yet updated"
  else
    pass "Test count ${TOTAL} ≥ floor ${TEST_FLOOR} ✓"
  fi

  # 9b. README badge must use the ">FLOOR" format (not an exact number)
  README_BADGE_RAW=$(grep -oE 'tests->[0-9]+%20passing' "$REPO_ROOT/README.md" | head -1 || true)
  BADGE_FLOOR=$(echo "$README_BADGE_RAW" | sed 's/tests->//;s/%20passing//' || true)
  if [[ -z "$README_BADGE_RAW" ]]; then
    fail "README.md badge does not use the '>N' format — expected 'tests->${TEST_FLOOR}%20passing'"
    warn "Update badge to: ![Test Suite](https://img.shields.io/badge/tests->%20${TEST_FLOOR}%20passing-brightgreen)"
  elif [[ "$BADGE_FLOOR" -ne "$TEST_FLOOR" ]]; then
    fail "README.md badge floor is ${BADGE_FLOOR}, script floor is ${TEST_FLOOR} — update both together when crossing a hundred"
  else
    pass "README.md badge: >$TEST_FLOOR format matches script floor ✓"
  fi
else
  if [[ "$SKIP_TESTS" == true ]]; then
    warn "No cached test counts (--skip-tests). Run without flag to enable floor check."
  else
    warn "Test counts cache not populated (check 6 may have failed). Skipping floor check."
  fi
fi
echo ""

# ─── Summary ─────────────────────────────────────────────────────────────────

echo "──────────────────────────────────────────"
if [[ "$FAIL" -eq 0 ]]; then
  echo -e "${GREEN}${BOLD}✓ All checks passed — v${VERSION} is ready to tag!${RESET}"
  echo ""
  echo "  Next step:  git tag v${VERSION} && git push origin v${VERSION}"
  exit 0
else
  echo -e "${RED}${BOLD}✗ ${FAIL} check(s) failed — fix the issues above before tagging.${RESET}"
  exit 1
fi
