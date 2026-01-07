#!/bin/bash

# setup-dev-branch.sh
# Creates and pushes dev branch for development workflow

set -e

GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${GREEN}"
cat << "EOF"
╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║           Setup Dev Branch                                ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝
EOF
echo -e "${NC}"

# Check if dev branch exists
if git show-ref --verify --quiet refs/heads/dev; then
    echo "✅ Dev branch already exists locally"
else
    echo "Creating dev branch from main..."
    git checkout main
    git checkout -b dev
    echo "✅ Dev branch created"
fi

# Check if dev exists on remote
if git ls-remote --heads origin dev | grep -q dev; then
    echo "✅ Dev branch already exists on GitHub"
else
    echo "Pushing dev branch to GitHub..."
    git push -u origin dev
    echo "✅ Dev branch pushed to GitHub"
fi

# Switch to dev
git checkout dev

echo ""
echo -e "${GREEN}✅ Dev branch is ready!${NC}"
echo ""
echo "You're now on the 'dev' branch."
echo "Start creating feature branches:"
echo "  $ git checkout -b feature/my-feature"
echo ""
