# Quick Start Guide - REslava.Result

Welcome! This guide explains commit automation and the development workflow.

## 📋 Table of Contents

- [Development Workflow](#-development-workflow)
- [Branch Strategy](#-branch-strategy)
- [Making Commits](#-making-commits)
- [Commit Types Reference](#-commit-types-reference)
- [Creating Releases](#-creating-releases)
- [Common Scenarios](#-common-scenarios)
- [Troubleshooting](#-troubleshooting)
- [Tips & Best Practices](#-tips--best-practices)

---

## 🌿 Branch Strategy

We use a simplified Git Flow for solo development:

### Branch Structure
```
main        → Production releases only (v1.0.0, v1.1.0, etc.)
dev         → Active development (merge feature branches here)
feature/*   → Individual features (branch from dev)
fix/*       → Bug fixes (branch from dev)
hotfix/*    → Critical production fixes (branch from main)
```

### Branch Rules

- **main**: Only merge from dev when ready to release
- **dev**: Default working branch, always stable enough to test
- **feature/**: Create for each new feature
- **fix/**: Create for each bug fix

---

## 🔄 Development Workflow

### Daily Work Cycle
```bash
# 1. Start on dev branch
git checkout dev
git pull origin dev

# 2. Create feature branch
git checkout -b feature/add-map-method

# 3. Make your changes
# ... code, code, code ...

# 4. Stage and commit
git add .
npm run commit
# Type: feat
# Scope: Result
# Description: add Map method for value transformation

# 5. Push feature branch
git push -u origin feature/add-map-method

# 6. When feature is complete, merge to dev
git checkout dev
git merge feature/add-map-method

# 7. Delete feature branch
git branch -d feature/add-map-method

# 8. Push dev
git push origin dev
```

### When Ready to Release
```bash
# 1. Ensure dev is ready
git checkout dev
git pull origin dev
dotnet build
dotnet test

# 2. Merge dev to main
git checkout main
git pull origin main
git merge dev

# 3. Create release (see "Creating Releases" section)
npm run release:beta  # or release, release:minor, etc.

# 4. Push everything
git push origin main --follow-tags

# 5. Go back to dev for next work
git checkout dev
```

---

## 💬 Making Commits

### ✅ Always Use This Command
```bash
npm run commit
```

**Never use `git commit -m "message"` directly!**

### Interactive Commit Process

When you run `npm run commit`, you'll see:
```
? Select the type of change that you're committing: (Use arrow keys)
❯ feat:     A new feature
  fix:      A bug fix
  docs:     Documentation only changes
  style:    Changes that do not affect the meaning of the code
  refactor: A code change that neither fixes a bug nor adds a feature
  perf:     A code change that improves performance
  test:     Adding missing tests or correcting existing tests
```

**Follow these steps:**

1. **Select Type**: Use arrow keys, press Enter
2. **Scope** (optional): Type scope (e.g., "Result", "Factory") or press Enter
3. **Short Description**: Brief description (e.g., "add Map method")
4. **Long Description** (optional): Press Enter to skip
5. **Breaking Changes**: Type "N" (or "Y" if backward incompatible)
6. **Issues**: Press Enter (or reference issue like "#123")

### Example Session
```bash
$ npm run commit

? Select the type of change: feat
? What is the scope of this change: Result
? Write a SHORT, IMPERATIVE tense description: add Map method
? Provide a LONGER description: (press enter to skip)
? Are there any breaking changes? No
? Does this change affect any open issues? (press enter to skip)

[dev abc123d] feat(Result): add Map method
 2 files changed, 45 insertions(+)
```

---

## 📝 Commit Types Reference

### Primary Types

| Type | When to Use | Branch | Example |
|------|-------------|--------|---------|
| `feat` | New features | feature/* | `feat(Result): add Map method` |
| `fix` | Bug fixes | fix/* | `fix(Result): handle null in Bind` |
| `docs` | Documentation | any | `docs(README): add usage examples` |
| `test` | Tests | feature/*, fix/* | `test(Map): add Map method tests` |
| `refactor` | Code improvements | feature/* | `refactor(Result): simplify error handling` |

### Secondary Types

| Type | When to Use | Example |
|------|-------------|---------|
| `perf` | Performance | `perf(Result): optimize Bind` |
| `style` | Formatting | `style: fix indentation` |
| `build` | Build system | `build: update .NET SDK` |
| `ci` | CI/CD | `ci: add coverage workflow` |
| `chore` | Maintenance | `chore: update gitignore` |

---

## 🚀 Creating Releases

### Release Workflow

Releases happen from the **main** branch only.
```bash
# 1. Ensure you're on main with latest code
git checkout main
git pull origin main

# 2. Preview the release (ALWAYS DO THIS FIRST!)
npm run release:dry

# 3. Review what would change:
#    - Version bump (0.1.0 → 0.2.0)
#    - CHANGELOG.md entries
#    - Git tag (v0.2.0)

# 4. If preview looks good, create the release
npm run release:minor  # or :beta, or :major

# 5. Push to GitHub
git push --follow-tags

# 6. Create GitHub Release (manual)
#    Go to: https://github.com/reslava/nuget-package-reslava-result/releases
#    Click "Draft a new release"
#    Select the new tag
#    Copy changelog content
#    Publish

# 7. Return to dev branch
git checkout dev
git merge main  # Bring release commits back to dev
git push origin dev
```

### Release Commands
```bash
# Preview what would happen (safe - doesn't change anything)
npm run release:dry

# Create beta release (0.1.0-beta.0, 0.1.0-beta.1, etc.)
npm run release:beta

# Create patch release (0.1.0 → 0.1.1)
# Use for bug fixes only
npm run release

# Create minor release (0.1.0 → 0.2.0)
# Use for new features (backward compatible)
npm run release:minor

# Create major release (0.1.0 → 1.0.0)
# Use for breaking changes
npm run release:major
```

### Version Numbers Explained

Following [Semantic Versioning](https://semver.org/):
```
Version: MAJOR.MINOR.PATCH-PRERELEASE

1.2.3-beta.0
│ │ │  └─ Prerelease (beta, alpha, rc)
│ │ └──── PATCH: Bug fixes
│ └────── MINOR: New features (backward compatible)
└──────── MAJOR: Breaking changes
```

**Examples:**
- `0.1.0` → `0.1.1`: Fixed bugs (PATCH)
- `0.1.0` → `0.2.0`: Added features (MINOR)
- `0.9.0` → `1.0.0`: First stable release (MAJOR)
- `1.0.0` → `2.0.0`: Breaking changes (MAJOR)

### When to Release

**Beta releases (0.x.x-beta.x):**
- Testing new features
- Getting feedback
- Pre-release testing

**Patch releases (x.x.PATCH):**
- Bug fixes only
- No new features
- Release often (weekly if needed)

**Minor releases (x.MINOR.x):**
- New features
- Backward compatible
- Release when feature set is complete

**Major releases (MAJOR.x.x):**
- Breaking changes
- API changes
- Release rarely (months/years)

---

## 🎯 Common Scenarios

### Scenario 1: Adding a New Feature
```bash
# 1. Start from dev
git checkout dev
git pull origin dev

# 2. Create feature branch
git checkout -b feature/add-validation

# 3. Implement feature
# ... write code in src/Extensions/ValidationExtensions.cs ...

# 4. Write tests
# ... write tests in tests/ValidationExtensions_Tests.cs ...

# 5. Commit feature code
git add src/Extensions/ValidationExtensions.cs
npm run commit
# Type: feat
# Scope: Validation
# Description: add Ensure and EnsureNotNull methods

# 6. Commit tests
git add tests/ValidationExtensions_Tests.cs
npm run commit
# Type: test
# Scope: Validation
# Description: add validation extension tests

# 7. Update documentation
git add README.md
npm run commit
# Type: docs
# Scope: README
# Description: add validation examples

# 8. Merge to dev
git checkout dev
git merge feature/add-validation

# 9. Push dev
git push origin dev

# 10. Delete feature branch
git branch -d feature/add-validation
```

### Scenario 2: Fixing a Bug
```bash
# 1. Create fix branch from dev
git checkout dev
git pull origin dev
git checkout -b fix/bind-null-handling

# 2. Fix the bug
# ... edit src/Results/Result.Bind.cs ...

# 3. Add regression test
# ... add test to tests/Result_Bind.cs ...

# 4. Commit fix
git add src/Results/Result.Bind.cs tests/Result_Bind.cs
npm run commit
# Type: fix
# Scope: Result
# Description: handle null values in Bind method

# 5. Merge to dev
git checkout dev
git merge fix/bind-null-handling

# 6. Push and cleanup
git push origin dev
git branch -d fix/bind-null-handling
```

### Scenario 3: Hotfix for Production
```bash
# 1. Create hotfix from main (not dev!)
git checkout main
git pull origin main
git checkout -b hotfix/critical-memory-leak

# 2. Fix the critical bug
# ... make minimal changes ...

# 3. Test thoroughly
dotnet test

# 4. Commit
git add .
npm run commit
# Type: fix
# Scope: Result
# Description: resolve memory leak in long operations

# 5. Merge to main
git checkout main
git merge hotfix/critical-memory-leak

# 6. Create patch release immediately
npm run release
git push --follow-tags

# 7. Merge back to dev (important!)
git checkout dev
git merge main
git push origin dev

# 8. Cleanup
git branch -d hotfix/critical-memory-leak
```

### Scenario 4: Preparing a Release
```bash
# 1. Ensure dev is ready
git checkout dev
dotnet build
dotnet test
git log --oneline -10  # Review recent commits

# 2. Merge to main
git checkout main
git pull origin main
git merge dev

# 3. Preview release
npm run release:dry

# Expected output:
# ✔ bumping version in package.json from 0.1.0 to 0.2.0
# ✔ outputting changes to CHANGELOG.md
# ✔ committing package.json and CHANGELOG.md
# ✔ tagging release v0.2.0

# 4. Review what would change
git diff HEAD package.json  # Check version
# Review CHANGELOG preview in output

# 5. If good, create release
npm run release:minor

# 6. Review what was created
git log --oneline -3
git show HEAD:CHANGELOG.md

# 7. Push to GitHub
git push --follow-tags

# 8. Create GitHub Release
# - Go to Releases page
# - Click "Draft a new release"
# - Select tag v0.2.0
# - Copy changelog
# - Publish

# 9. Merge back to dev
git checkout dev
git merge main
git push origin dev
```

---

## 🔧 Troubleshooting

### Problem: On wrong branch
```bash
# Check current branch
git branch --show-current

# Switch to correct branch
git checkout dev  # or main
```

### Problem: Forgot to branch from dev
```bash
# You made commits directly on dev - that's okay!
# Just continue working on dev

# Or if you want to move commits to a feature branch:
git checkout -b feature/my-feature  # Creates branch with your commits
git checkout dev
git reset --hard origin/dev  # Resets dev to remote
git checkout feature/my-feature  # Back to your work
```

### Problem: Need to undo last commit
```bash
# Undo commit but keep changes
git reset --soft HEAD~1

# Redo the commit
npm run commit
```

### Problem: Merge conflict
```bash
# When merging feature to dev causes conflict
git checkout dev
git merge feature/my-feature

# CONFLICT in src/Results/Result.cs

# 1. Open file and resolve conflicts
# 2. Remove conflict markers (<<<<, ====, >>>>)
# 3. Stage resolved files
git add src/Results/Result.cs

# 4. Complete merge
git commit
# (Git will create merge commit automatically)

# 5. Push
git push origin dev
```

---

## 💡 Tips & Best Practices

### ✅ DO

1. **Always work on feature branches**
```bash
   git checkout -b feature/my-feature
```

2. **Keep dev stable**
   - Test before merging to dev
   - Dev should always build and pass tests

3. **Use descriptive branch names**
```
   ✅ feature/add-async-support
   ✅ fix/null-reference-in-bind
   ❌ my-branch
   ❌ temp
```

4. **Commit often, push regularly**
```bash
   # Small, focused commits
   git add src/Results/Result.Map.cs
   npm run commit
```

5. **Preview releases before creating**
```bash
   npm run release:dry  # Always do this first!
```

6. **Test before releasing**
```bash
   dotnet build
   dotnet test
```

### ❌ DON'T

1. **Don't commit directly to main**
   - Always merge from dev
   - main = production only

2. **Don't use `git commit -m` directly**
   - Breaks changelog generation
   - Use `npm run commit` instead

3. **Don't mix multiple features in one branch**
```
   ❌ feature/add-map-bind-match  # Too many things
   ✅ feature/add-map            # One feature
   ✅ feature/add-bind           # One feature
   ✅ feature/add-match          # One feature
```

4. **Don't forget to pull before creating branches**
```bash
   git checkout dev
   git pull origin dev  # ← Don't forget this!
   git checkout -b feature/new-feature
```

5. **Don't release without testing**
   - Always build and test first
   - Check `npm run release:dry` output

---

## 📚 Quick Reference

### Essential Commands
```bash
# Daily workflow
git checkout dev                    # Start on dev
git pull origin dev                 # Get latest
git checkout -b feature/my-feature  # Create branch
npm run commit                      # Make commit
git push origin feature/my-feature  # Push branch
git checkout dev                    # Back to dev
git merge feature/my-feature        # Merge feature
git push origin dev                 # Push dev

# Releases (from main branch)
git checkout main                   # Switch to main
git merge dev                       # Bring dev changes
npm run release:dry                 # Preview release
npm run release:minor               # Create release
git push --follow-tags              # Push with tags
git checkout dev                    # Back to dev
git merge main                      # Sync dev with main

# Viewing
git log --oneline --graph           # View history
git status                          # Check status
git branch                          # List branches
```

### Release Commands
```bash
npm run release:dry        # Preview (safe, no changes)
npm run release:beta       # Beta: 0.1.0-beta.0
npm run release            # Patch: 0.1.0 → 0.1.1
npm run release:minor      # Minor: 0.1.0 → 0.2.0
npm run release:major      # Major: 0.1.0 → 1.0.0
```

### Branch Commands
```bash
git branch                          # List local branches
git branch -r                       # List remote branches
git branch -d feature/old           # Delete local branch
git push origin --delete feature/old # Delete remote branch
git checkout -b feature/new         # Create and switch
git branch -m old-name new-name     # Rename branch
```

---

## 🆘 Need Help?

### Quick Links

- **Repository**: https://github.com/reslava/nuget-package-reslava-result
- **Issues**: https://github.com/reslava/nuget-package-reslava-result/issues
- **Releases**: https://github.com/reslava/nuget-package-reslava-result/releases

### Common Questions

**Q: What if I committed to the wrong branch?**
A: See "Troubleshooting" section above - you can move commits to a new branch.

**Q: Should I delete feature branches after merging?**
A: Yes! Keep your workspace clean. Delete both local and remote branches.

**Q: How often should I release?**
A: 
- Beta: Weekly or as needed for testing
- Patch: As needed for bugs (can be daily)
- Minor: When features are complete (every 1-2 weeks)
- Major: Rarely (months or years)

**Q: Can I work directly on dev for small changes?**
A: Yes! For tiny changes (typos, small fixes), committing directly to dev is fine.

---

## 🎓 Learning Path

### Week 1: Basics
- [ ] Use `npm run commit` for all commits
- [ ] Learn commit types (feat, fix, docs, test)
- [ ] Practice writing good commit messages
- [ ] Work on feature branches

### Week 2: Branching
- [ ] Create feature branches regularly
- [ ] Merge features to dev
- [ ] Keep dev stable
- [ ] Delete old branches

### Week 3: Releases
- [ ] Practice `npm run release:dry`
- [ ] Create a beta release
- [ ] Review generated CHANGELOG
- [ ] Understand semantic versioning

### Week 4: Advanced
- [ ] Handle merge conflicts
- [ ] Create hotfixes from main
- [ ] Manage multiple feature branches
- [ ] Perfect your release process

---

## 🎉 You're Ready!

You now know:
- ✅ Branch strategy (main, dev, feature/*)
- ✅ Making proper commits with `npm run commit`
- ✅ Creating releases with `commit-and-tag-version`
- ✅ Semantic versioning
- ✅ Complete workflow from feature to release

**Remember**: 
- Work on feature branches
- Merge to dev frequently
- Release from main
- Always use `npm run commit`!

Happy coding! 🚀

---

*Last updated: 2026-01-08*
*Using: commit-and-tag-version for releases*
*Branch strategy: Git Flow (main + dev)*
