#!/usr/bin/env bash
# sync-test-counts.sh — Run tests and update all documentation with actual counts.
#
# Usage:
#   ./scripts/sync-test-counts.sh          # run tests, then update docs
#   ./scripts/sync-test-counts.sh --dry-run # show what would change, don't write
#   ./scripts/sync-test-counts.sh --skip-tests # update from last test run (reads cached counts)
#
# What it updates (current/latest references only — never touches historical versions):
#   README.md          — badge, summary, test commands, quality metrics
#   CHANGELOG.md       — latest version's Stats section
#   docs/github/GITHUB_RELEASE_v<latest>.md — test count line

set -u

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
CACHE_FILE="$REPO_ROOT/scripts/.test-counts-cache"
TMPFILE="$REPO_ROOT/scripts/.test-output-tmp"

DRY_RUN=false
SKIP_TESTS=false

for arg in "$@"; do
  case "$arg" in
    --dry-run)    DRY_RUN=true ;;
    --skip-tests) SKIP_TESTS=true ;;
    -h|--help)
      sed -n '2,12p' "$0" | sed 's/^# \?//'
      exit 0
      ;;
  esac
done

# ─── Step 1: Run tests and capture counts ───────────────────────────────────

run_tests() {
  echo "Running tests..."
  dotnet test "$REPO_ROOT" --configuration Release --verbosity minimal 2>&1 \
    | tr -d '\r' > "$TMPFILE" || true

  # Extract only the summary lines (Passed!/Failed! lines)
  local summaries
  summaries=$(grep -E '^(Passed|Failed)!' "$TMPFILE" || true)

  if [[ -z "$summaries" ]]; then
    echo "ERROR: No test summary lines found. Raw output:"
    cat "$TMPFILE"
    rm -f "$TMPFILE"
    exit 1
  fi

  # Check for any failures
  local failed_count
  failed_count=$(echo "$summaries" | grep -oP 'Failed:\s+\K[0-9]+' | awk '{s+=$1} END{print s+0}')

  if [[ "$failed_count" -gt 0 ]]; then
    echo "ERROR: $failed_count test(s) failed. Fix failures before updating docs."
    echo "$summaries" | grep '^Failed'
    rm -f "$TMPFILE"
    exit 1
  fi

  # Core library: lines containing "REslava.Result.Tests.dll"
  local core_lines core_per_tfm tfm_count
  core_lines=$(echo "$summaries" | grep 'REslava\.Result\.Tests\.dll' || true)
  tfm_count=$(echo "$core_lines" | wc -l | tr -d ' ')
  core_per_tfm=$(echo "$core_lines" | head -1 | grep -oP 'Passed:\s+\K[0-9]+')

  # Generator tests
  local generator
  generator=$(echo "$summaries" | grep 'SourceGenerators\.Tests\.dll' | grep -oP 'Passed:\s+\K[0-9]+')

  # Analyzer tests
  local analyzer
  analyzer=$(echo "$summaries" | grep 'Analyzers\.Tests\.dll' | grep -oP 'Passed:\s+\K[0-9]+')

  rm -f "$TMPFILE"

  if [[ -z "$core_per_tfm" || -z "$generator" || -z "$analyzer" ]]; then
    echo "ERROR: Could not parse test counts."
    echo "  core_per_tfm=$core_per_tfm generator=$generator analyzer=$analyzer"
    exit 1
  fi

  local core_total=$(( core_per_tfm * tfm_count ))
  local total=$(( core_total + generator + analyzer ))

  cat > "$CACHE_FILE" <<EOF
CORE_PER_TFM=$core_per_tfm
TFM_COUNT=$tfm_count
CORE_TOTAL=$core_total
GENERATOR=$generator
ANALYZER=$analyzer
TOTAL=$total
EOF

  echo "Tests passed: core=$core_per_tfm x $tfm_count TFMs ($core_total) + generator=$generator + analyzer=$analyzer = $total total"
}

load_cache() {
  if [[ ! -f "$CACHE_FILE" ]]; then
    echo "ERROR: No cached test counts found. Run without --skip-tests first."
    exit 1
  fi
  source "$CACHE_FILE"
  echo "Using cached counts: core=$CORE_PER_TFM x $TFM_COUNT TFMs ($CORE_TOTAL) + generator=$GENERATOR + analyzer=$ANALYZER = $TOTAL total"
}

if [[ "$SKIP_TESTS" == true ]]; then
  load_cache
else
  run_tests
  source "$CACHE_FILE"
fi

# ─── Step 2: Build formatted strings ────────────────────────────────────────

# Format number with commas (e.g., 2825 → 2,825)
fmt() {
  echo "$1" | sed ':a;s/\B[0-9]\{3\}\>$/,&/;ta'
}

TOTAL_FMT=$(fmt "$TOTAL")
CORE_TOTAL_FMT=$(fmt "$CORE_TOTAL")

# ─── Step 3: Update files ───────────────────────────────────────────────────

update_readme() {
  local f="$REPO_ROOT/README.md"
  echo "Updating README.md..."

  sed -i -E "
    # Badge: tests-NNNN%20passing
    s|tests-[0-9]+%20passing|tests-${TOTAL}%20passing|g

    # '**N,NNN Tests Passing**' heading (only the bold heading line)
    s|\*\*[0-9,]+ Tests Passing\*\*|**${TOTAL_FMT} Tests Passing**|

    # 'N,NNN/N,NNN tests passing' quality metric (requires comma = 1000+, avoids 5/5 ratios)
    s|[0-9]+,[0-9]+/[0-9]+,[0-9]+ tests passing|${TOTAL_FMT}/${TOTAL_FMT} tests passing|

    # Core library: 'NNN tests per TFM (...) = N,NNN tests'
    s|[0-9]+ tests per TFM \(net[0-9.]+, net[0-9.]+, net[0-9.]+\) = [0-9,]+ tests|${CORE_PER_TFM} tests per TFM (net8.0, net9.0, net10.0) = ${CORE_TOTAL_FMT} tests|

    # 'Source Generator Tests**: NN tests'
    s|Source Generator Tests\*\*: [0-9]+ tests|Source Generator Tests**: ${GENERATOR} tests|

    # 'Analyzer Tests**: NN tests'
    s|Analyzer Tests\*\*: [0-9]+ tests|Analyzer Tests**: ${ANALYZER} tests|

    # 'Run all tests (N,NNN tests across N TFMs)'
    s|Run all tests \([0-9,]+ tests across [0-9]+ TFMs\)|Run all tests (${TOTAL_FMT} tests across ${TFM_COUNT} TFMs)|

    # 'Source Generator tests (NN tests)'
    s|Source Generator tests \([0-9]+ tests\)|Source Generator tests (${GENERATOR} tests)|

    # 'Analyzer tests (NN tests)'
    s|Analyzer tests \([0-9]+ tests\)|Analyzer tests (${ANALYZER} tests)|

    # 'Total Tests: N,NNN+ passing'
    s|Total Tests: [0-9,]+\+? passing|Total Tests: ${TOTAL_FMT}+ passing|
  " "$f"
}

update_changelog() {
  local f="$REPO_ROOT/CHANGELOG.md"
  echo "Updating CHANGELOG.md (latest version only)..."

  # Find the first "tests passing across net" line and update only that one
  local line_num
  line_num=$(grep -n 'tests passing across net' "$f" | head -1 | cut -d: -f1) || true
  if [[ -n "$line_num" ]]; then
    sed -i -E "${line_num}s|[0-9,]+ tests passing|${TOTAL_FMT} tests passing|" "$f"
  fi
}

update_latest_release() {
  # Find the latest GITHUB_RELEASE_v*.md by version sort
  local latest
  latest=$(ls "$REPO_ROOT"/docs/github/GITHUB_RELEASE_v*.md 2>/dev/null | sort -V | tail -1) || true
  if [[ -z "$latest" ]]; then
    echo "  No release notes found, skipping."
    return
  fi
  echo "Updating $(basename "$latest")..."

  sed -i -E "s|[0-9,]+ tests passing|${TOTAL_FMT} tests passing|g" "$latest"
}

if [[ "$DRY_RUN" == true ]]; then
  echo ""
  echo "Dry run — showing current values in docs vs actual:"
  echo "  Actual total: $TOTAL_FMT  (core=$CORE_PER_TFM x $TFM_COUNT + gen=$GENERATOR + analyzer=$ANALYZER)"
  echo ""
  echo "Matches that would be updated:"
  grep -nP 'tests-[0-9]+%20passing' "$REPO_ROOT/README.md" | head -5 | sed 's/^/  README.md:/'
  grep -nP '[0-9,]+ Tests Passing' "$REPO_ROOT/README.md" | head -5 | sed 's/^/  README.md:/'
  grep -nP 'Source Generator Tests\*\*: [0-9]+ tests' "$REPO_ROOT/README.md" | sed 's/^/  README.md:/'
  grep -nP 'Analyzer Tests\*\*: [0-9]+ tests' "$REPO_ROOT/README.md" | sed 's/^/  README.md:/'
  grep -nP '[0-9,]+ tests per TFM' "$REPO_ROOT/README.md" | sed 's/^/  README.md:/'
  grep -nP 'tests passing across net' "$REPO_ROOT/CHANGELOG.md" | head -1 | sed 's/^/  CHANGELOG.md:/'
  echo ""
  echo "No files were modified."
else
  update_readme
  update_changelog
  update_latest_release

  echo ""
  echo "Done. Updated docs with:"
  echo "  Total:     $TOTAL_FMT"
  echo "  Core:      $CORE_PER_TFM x $TFM_COUNT TFMs = $CORE_TOTAL_FMT"
  echo "  Generator: $GENERATOR"
  echo "  Analyzer:  $ANALYZER"
fi
