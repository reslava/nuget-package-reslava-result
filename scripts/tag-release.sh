#!/usr/bin/env bash
# tag-release.sh — Validate, tag, and push a release.
#
# Usage:
#   ./scripts/tag-release.sh 1.24.0        # validate + tag + push
#   ./scripts/tag-release.sh               # auto-read version from Directory.Build.props
#   ./scripts/tag-release.sh --skip-tests  # pass --skip-tests to validate-release.sh
#
# Steps:
#   1. Run validate-release.sh (abort if any check fails)
#   2. Push main branch to origin (ensures CI fires and branch is up to date)
#   3. Create annotated git tag v{VERSION}
#   4. Push tag to origin

set -u

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"

VERSION=""
SKIP_TESTS_FLAG=""

for arg in "$@"; do
  case "$arg" in
    --skip-tests) SKIP_TESTS_FLAG="--skip-tests" ;;
    -h|--help)
      sed -n '2,12p' "$0" | sed 's/^# \?//'
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
  echo "ERROR: Could not determine version."
  exit 1
fi

TAG="v${VERSION}"

# ─── Guard: tag must not already exist ───────────────────────────────────────

if git -C "$REPO_ROOT" tag --list | grep -q "^${TAG}$"; then
  echo "ERROR: Tag ${TAG} already exists. Nothing to do."
  exit 1
fi

# ─── Step 1: Run validate-release.sh ─────────────────────────────────────────

echo "Running validate-release.sh ${VERSION} ${SKIP_TESTS_FLAG}..."
echo ""

if ! bash "$REPO_ROOT/scripts/validate-release.sh" "$VERSION" $SKIP_TESTS_FLAG; then
  echo ""
  echo "ERROR: Validation failed — fix the issues above before tagging."
  exit 1
fi

# ─── Step 2: Push main branch ────────────────────────────────────────────────

echo ""
echo "Pushing main to origin..."
git -C "$REPO_ROOT" push origin main

# ─── Step 3: Create annotated tag ────────────────────────────────────────────

echo ""
echo "Creating tag ${TAG}..."
git -C "$REPO_ROOT" tag -a "$TAG" -m "Release ${TAG}"

# ─── Step 4: Push tag ────────────────────────────────────────────────────────

echo "Pushing ${TAG} to origin..."
git -C "$REPO_ROOT" push origin "$TAG"

echo ""
TAG_SHA=$(git -C "$REPO_ROOT" rev-list -n 1 "$TAG")
echo "Tagged ${TAG} @ ${TAG_SHA}"
