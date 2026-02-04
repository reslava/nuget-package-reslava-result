#!/bin/bash

echo "🚀 Updating GitHub Release v1.10.1 with professional content..."

# Read the release notes from the file
RELEASE_NOTES=$(cat docs/github/GITHUB_RELEASE_v1.10.1.md)

# Create/update the GitHub release
gh release edit v1.10.1 \
  --title "🚀 REslava.Result v1.10.1 - Documentation Synchronization Fix" \
  --notes "$RELEASE_NOTES" \
  --latest

echo "✅ GitHub Release v1.10.1 updated successfully!"
echo "📦 View at: https://github.com/reslava/nuget-package-reslava-result/releases/tag/v1.10.1"
