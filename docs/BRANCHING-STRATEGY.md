# Branching Strategy - REslava.Result

## Overview

This project uses a simplified **Git Flow** branching strategy optimized for solo development.

## Branch Structure
```
main
  └─ Production-ready releases only
  └─ Tags: v1.0.0, v1.1.0, v2.0.0

dev
  └─ Active development branch
  └─ Always stable enough to test
  └─ Merges from feature/* branches

feature/*
  └─ Individual feature development
  └─ Branch from: dev
  └─ Merge to: dev

fix/*
  └─ Bug fixes
  └─ Branch from: dev
  └─ Merge to: dev

hotfix/*
  └─ Critical production fixes
  └─ Branch from: main
  └─ Merge to: main, then merge main to dev
```

## Branch Descriptions

### `main` - Production Branch
- **Purpose**: Production-ready code only
- **Protected**: Yes
- **Direct commits**: Never
- **Receives merges from**: `dev` (when ready to release)
- **Creates**: Release tags (v1.0.0, etc.)

**Rules:**
- Only merge from `dev` when ready to release
- Always create a release tag after merging
- Never commit directly to this branch
- All code must be tested before merging

### `dev` - Development Branch
- **Purpose**: Integration branch for features
- **Protected**: No (for solo dev)
- **Direct commits**: Small changes only
- **Receives merges from**: `feature/*`, `fix/*`
- **Merges to**: `main` (for releases)

**Rules:**
- Should always be stable enough to test
- Run tests before pushing
- Can accept small commits directly
- Merge feature branches here

### `feature/*` - Feature Branches
- **Purpose**: Develop new features
- **Branch from**: `dev`
- **Merge to**: `dev`
- **Naming**: `feature/descriptive-name`

**Examples:**
- `feature/add-map-method`
- `feature/add-async-support`
- `feature/add-validation-extensions`

**Rules:**
- One feature per branch
- Keep branches short-lived (days, not weeks)
- Delete after merging

### `fix/*` - Bug Fix Branches
- **Purpose**: Fix bugs
- **Branch from**: `dev`
- **Merge to**: `dev`
- **Naming**: `fix/descriptive-name`

**Examples:**
- `fix/null-handling-in-bind`
- `fix/memory-leak`
- `fix/incorrect-validation`

**Rules:**
- One bug per branch
- Include regression test
- Delete after merging

### `hotfix/*` - Emergency Fix Branches
- **Purpose**: Critical fixes for production
- **Branch from**: `main`
- **Merge to**: `main`, then merge `main` to `dev`
- **Naming**: `hotfix/descriptive-name`

**Examples:**
- `hotfix/critical-security-issue`
- `hotfix/data-loss-bug`

**Rules:**
- Only for critical production issues
- Minimal changes only
- Immediate release after fix
- Must merge back to both `main` and `dev`

## Workflows

### Daily Development
```bash
# 1. Start on dev
git checkout dev
git pull origin dev

# 2. Create feature branch
git checkout -b feature/add-map-method

# 3. Work and commit
# ... make changes ...
git add .
npm run commit

# 4. When complete, merge to dev
git checkout dev
git merge feature/add-map-method
git push origin dev

# 5. Delete feature branch
git branch -d feature/add-map-method
```

### Release Process
```bash
# 1. Prepare dev branch
git checkout dev
git pull origin dev
dotnet test  # Ensure all tests pass

# 2. Merge to main
git checkout main
git pull origin main
git merge dev

# 3. Create release
npm run release:minor
git push --follow-tags

# 4. Merge back to dev
git checkout dev
git merge main
git push origin dev
```

### Hotfix Process
```bash
# 1. Branch from main
git checkout main
git pull origin main
git checkout -b hotfix/critical-fix

# 2. Fix and test
# ... make fix ...
dotnet test
git add .
npm run commit

# 3. Merge to main
git checkout main
git merge hotfix/critical-fix

# 4. Release immediately
npm run release
git push --follow-tags

# 5. Merge to dev
git checkout dev
git merge main
git push origin dev

# 6. Delete hotfix branch
git branch -d hotfix/critical-fix
```

## Branch Naming Conventions

### Good Names ✅
- `feature/add-map-method`
- `feature/implement-async-support`
- `fix/null-reference-in-bind`
- `fix/incorrect-error-message`
- `hotfix/security-vulnerability`

### Bad Names ❌
- `my-branch`
- `test`
- `temp`
- `wip`
- `feature`
- `updates`

### Naming Format
```
<type>/<short-description-in-kebab-case>

Types: feature, fix, hotfix
```

## Visualization
```
main    ─●─────────────────●─────────●──→
         │                 │         │
         │    Release      │         │
         │    v0.1.0       │         │
         │                 │         │
dev     ─●───●───●───●───●─●───●───●─●──→
         │   │   │   │   │     │   │
         └─feature-1┘ │   │     │   │
             └─feature-2─┘ │     │   │
                 └─fix-1───┘     │   │
                       └─feature-3┘ │
                           └─fix-2──┘
```

## GitHub Settings (Optional)

If you want to add protection rules later:

### main Branch Protection
- ✅ Require pull request reviews
- ✅ Require status checks to pass
- ✅ Include administrators (optional for solo)

### For Team Development (Future)
- Add branch protection for `dev`
- Require PR reviews
- Enable CI checks

## FAQ

**Q: Can I commit directly to dev?**
A: Yes, for small changes (typos, quick fixes). For features, use feature branches.

**Q: When should I create a feature branch?**
A: For any substantial change that takes more than 30 minutes.

**Q: Do I need to delete feature branches?**
A: Yes! Keep your repository clean. Delete after merging.

**Q: What if I forget to branch and commit to dev?**
A: That's okay! For solo dev, committing to dev is acceptable. Just try to branch for bigger features.

**Q: Can I have multiple feature branches at once?**
A: Yes! Work on multiple features in parallel, merge them to dev separately.

---

*Last updated: 2025-01-08*
