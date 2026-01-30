# Quick Start Guide - REslava.Result v1.9.0

Welcome! This guide gets you up and running with REslava.Result v1.9.0 and its revolutionary Core Library architecture.

## 🚀 Quick Start (30 seconds)

### 📦 Installation

```bash
dotnet add package REslava.Result
dotnet add package REslava.Result.SourceGenerators.Core
dotnet add package REslava.Result.SourceGenerators.Generators.ResultToIResult
```

### 🔧 Project Setup

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core library infrastructure -->
    <ProjectReference Include="../SourceGenerator/Core/REslava.Result.SourceGenerators.Core.csproj" 
                     ReferenceOutputAssembly="false" 
                     OutputItemType="Analyzer" />
    
    <!-- Refactored generator as analyzer -->
    <ProjectReference Include="../SourceGenerator/Generators/ResultToIResult/ResultToIResultGenerator.csproj" 
                     ReferenceOutputAssembly="false" 
                     OutputItemType="Analyzer" />
    
    <!-- Refactored generator for attribute access -->
    <ProjectReference Include="../SourceGenerator/Generators/ResultToIResult/ResultToIResultGenerator.csproj" 
                     ReferenceOutputAssembly="true" />
    
    <!-- Core Result package -->
    <ProjectReference Include="../src/REslava.Result.csproj" />
  </ItemGroup>
</Project>
```

### 🎯 Enable Auto-Conversion

```csharp
using REslava.Result.SourceGenerators;

[assembly: GenerateResultExtensions(
    Namespace = "Generated.ResultExtensions",
    IncludeErrorTags = true,
    GenerateHttpMethodExtensions = true,
    DefaultErrorStatusCode = 400,
    IncludeDetailedErrors = true,
    GenerateAsyncMethods = true
)]

var builder = WebApplication.CreateBuilder(args);
// ... rest of your setup
```

### 🌐 Use in Your API

```csharp
using Generated.ResultExtensions;

app.MapGet("/users/{id}", (int id) =>
{
    if (id <= 0)
        return Result<User>.Fail("Invalid ID: must be positive");
    
    var user = GetUserById(id);
    return user.IsSuccess 
        ? Result<User>.Ok(user)
        : Result<User>.Fail($"User {id} not found");
});

app.MapPost("/users", (User user) =>
{
    var validation = ValidateUser(user);
    if (!validation.IsSuccess)
        return validation;
    
    var created = CreateUser(user);
    return created;
});
```

---

## 🏗️ What's New in v1.9.0

### **🔧 Core Library Architecture**
- **Modular Infrastructure** - Reusable components for generator development
- **Configuration-Driven** - Flexible, type-safe configuration management
- **100% Test Coverage** - 32 tests with comprehensive scenarios
- **Better Error Handling** - Graceful handling of edge cases and null inputs

### **📦 Enhanced Components**
- **CodeBuilder** - Fluent code generation with proper indentation
- **HttpStatusCodeMapper** - Smart HTTP status mapping with conventions
- **AttributeParser** - Robust attribute configuration parsing
- **IncrementalGeneratorBase<TConfig>** - Base class for rapid generator development

---

## 📋 Table of Contents

- [Quick Start](#-quick-start-30-seconds)
- [What's New in v1.9.0](#-whats-new-in-v190)
- [Core Library Components](#-core-library-components)
- [Configuration Options](#-configuration-options)
- [Migration from v1.7.3](#-migration-from-v173)
- [Development Workflow](#-development-workflow)
- [Branch Strategy](#-branch-strategy)
- [Making Commits](#-making-commits)
- [Common Scenarios](#-common-scenarios)
- [Testing](#-testing)
- [Troubleshooting](#-troubleshooting)

---

## 🏗️ Core Library Components

### **📝 CodeBuilder**
Fluent API for generating well-formatted C# code:

```csharp
var builder = new CodeBuilder();
var code = builder
    .AppendLine("namespace Generated.Extensions")
    .Indent()
    .AppendClassDeclaration("ResultExtensions", "public", "static")
    .Indent()
    .AppendMethodDeclaration("ToIResult", "IResult", "this Result<T> result", "T", "public", "static")
    .AppendLine("if (result.IsSuccess) return Results.Ok(result.Value);")
    .AppendLine("return Results.Problem(CreateProblemDetails(result.Errors));")
    .CloseBrace()
    .CloseBrace()
    .ToString();
```

### **🌐 HttpStatusCodeMapper**
Smart HTTP status code mapping:

```csharp
var mapper = new HttpStatusCodeMapper();

// Convention-based mapping
int statusCode = mapper.DetermineStatusCode("UserNotFoundError"); // 404
int validationCode = mapper.DetermineStatusCode("ValidationError"); // 422

// Custom mapping
mapper.AddMapping("BusinessError", 422);
int businessCode = mapper.DetermineStatusCode("BusinessError"); // 422
```

### **⚙️ Configuration System**
Type-safe configuration management:

```csharp
public class MyGeneratorConfig : GeneratorConfigurationBase<MyGeneratorConfig>
{
    public string Namespace { get; set; } = "Generated";
    public bool IncludeErrorTags { get; set; } = true;
    public int DefaultErrorStatusCode { get; set; } = 400;
    
    public override bool Validate() => !string.IsNullOrEmpty(Namespace);
    public override object Clone() => new MyGeneratorConfig { Namespace = Namespace, IncludeErrorTags = IncludeErrorTags };
}
```

---

## ⚙️ Configuration Options

### **GenerateResultExtensionsAttribute**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Namespace` | string | "Generated" | Namespace for generated code |
| `IncludeErrorTags` | bool | true | Include error tags in responses |
| `GenerateHttpMethodExtensions` | bool | true | Generate HTTP method-specific extensions |
| `DefaultErrorStatusCode` | int | 400 | Default HTTP status code for errors |
| `IncludeDetailedErrors` | bool | false | Include detailed error information |
| `GenerateAsyncMethods` | bool | true | Generate async extension methods |
| `CustomErrorMappings` | string[] | null | Custom error type to status code mappings |

### **Example Configurations**

#### **Basic Setup**
```csharp
[assembly: GenerateResultExtensions]
```

#### **Full Configuration**
```csharp
[assembly: GenerateResultExtensions(
    Namespace = "MyApp.Generated",
    IncludeErrorTags = true,
    GenerateHttpMethodExtensions = true,
    DefaultErrorStatusCode = 422,
    IncludeDetailedErrors = true,
    GenerateAsyncMethods = true,
    CustomErrorMappings = new[] { 
        "InventoryOutOfStock:422",
        "PaymentDeclined:402",
        "AccountSuspended:403"
    }
)]
```

---

## 🔄 Migration from v1.7.3

### **Package References**

#### **Before (v1.7.3):**
```xml
<ProjectReference Include="../SourceGenerator/REslava.Result.SourceGenerators.csproj" 
                 ReferenceOutputAssembly="false" 
                 OutputItemType="Analyzer" />
```

#### **After (v1.9.0):**
```xml
<ProjectReference Include="../SourceGenerator/Core/REslava.Result.SourceGenerators.Core.csproj" 
                 ReferenceOutputAssembly="false" 
                 OutputItemType="Analyzer" />
<ProjectReference Include="../SourceGenerator/Generators/ResultToIResult/ResultToIResultGenerator.csproj" 
                 ReferenceOutputAssembly="false" 
                 OutputItemType="Analyzer" />
<ProjectReference Include="../SourceGenerator/Generators/ResultToIResult/ResultToIResultGenerator.csproj" 
                 ReferenceOutputAssembly="true" />
```

### **Generated Extensions**

#### **Before (v1.7.3):**
```csharp
using Generated; // Default namespace
```

#### **After (v1.9.0):**
```csharp
using Generated.ResultExtensions; // Your custom namespace
```

---

## 🌿 Branch Strategy

We use a simplified Git Flow for solo development:

### Branch Structure
```
main        → Production releases only (v1.0.0, v1.1.0, etc.)
dev         → Active development (merge feature branches here)
feature/*   → Individual features (branch from dev)
fix/*       → Bug fixes (branch from dev)
test/*      → Test additions or improvements (branch from dev)
hotfix/*    → Critical production fixes (branch from main)
```

### Branch Rules

- **main**: Only merge from dev when ready to release
- **dev**: Default working branch, always stable enough to test
- **feature/**: Create for each new feature
- **fix/**: Create for each bug fix
- **test/**: Create for test-related work
- **hotfix/**: Create for critical production fixes (from main)

---

## 🔄 Development Workflow

### Simple Workflow (Quick Changes: 5-30 minutes)

For small, straightforward changes where you'll merge immediately:

```bash
# 1. Start on dev branch
git checkout dev
git pull origin dev

# 2. Create feature branch
git checkout -b test/map

# 3. Make your changes
# ... code, code, code ...

# 4. Stage and commit
git add .
npm run commit
# Type: test
# Scope: Map
# Description: add Map method tests

# 5. Merge to dev immediately
git checkout dev
git merge test/map

# 6. Delete local branch
git branch -d test/map

# 7. Push dev
git push origin dev
```

**Note**: No need to push the feature branch since you're merging right away.

---

### Safe Workflow (Complex Changes: Hours/Days)

For larger features where you want backup and work over multiple sessions:

```bash
# 1. Start on dev branch
git checkout dev
git pull origin dev

# 2. Create feature branch
git checkout -b feature/add-validation

# 3. Make your changes
# ... code, code, code ...

# 4. Stage and commit
git add .
npm run commit
# Type: feat
# Scope: Validation
# Description: add Ensure and EnsureNotNull methods

# 5. Push for backup (IMPORTANT for longer work!)
git push -u origin feature/add-validation

# 6. Continue working over multiple days
# ... more changes ...
git add .
npm run commit
git push  # Keep remote updated

# 7. When feature is complete, merge to dev
git checkout dev
git merge feature/add-validation

# 8. Delete BOTH local and remote branches
git branch -d feature/add-validation
git push origin --delete feature/add-validation

# 9. Push dev
git push origin dev
```

---

### When to Push Feature Branches?

| Scenario | Push to Origin? | Why? |
|----------|----------------|------|
| Quick fix (< 30 min) | **Optional** | Merging immediately, no backup needed |
| Work in progress | **Yes** | Backup in case of machine failure |
| Multi-day feature | **Yes** | Safety and continuity |
| Experimentation | **Yes** | May want to reference later |
| Working from multiple machines | **Yes** | Sync across devices |
| Want to create PR for self-review | **Yes** | GitHub PR requires remote branch |

**Rule of thumb**: If you'll merge within the hour, skip pushing the feature branch. Otherwise, push it for safety.

---

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

? Select the type of change: test
? What is the scope of this change: Map
? Write a SHORT, IMPERATIVE tense description: add Map method tests
? Provide a LONGER description: (press enter to skip)
? Are there any breaking changes? No
? Does this change affect any open issues? (press enter to skip)

[dev abc123d] test(Map): add Map method tests
 1 file changed, 25 insertions(+)
```

---

## 📝 Commit Types Reference

### Primary Types

| Type | When to Use | Branch | Example |
|------|-------------|--------|---------|
| `feat` | New features | feature/* | `feat(Result): add Map method` |
| `fix` | Bug fixes | fix/* | `fix(Result): handle null in Bind` |
| `docs` | Documentation | any | `docs(README): add usage examples` |
| `test` | Tests | test/* | `test(Map): add Map method tests` |
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

### Scenario 1: Adding a New Feature (Multi-day work)

```bash
# 1. Start from dev
git checkout dev
git pull origin dev

# 2. Create feature branch
git checkout -b feature/add-validation

# 3. Implement feature
# ... write code in src/Extensions/ValidationExtensions.cs ...

# 4. Commit and push for backup
git add src/Extensions/ValidationExtensions.cs
npm run commit
# Type: feat
# Scope: Validation
# Description: add Ensure and EnsureNotNull methods
git push -u origin feature/add-validation

# 5. Next day: write tests
# ... write tests in tests/ValidationExtensions_Tests.cs ...

# 6. Commit tests and push
git add tests/ValidationExtensions_Tests.cs
npm run commit
# Type: test
# Scope: Validation
# Description: add validation extension tests
git push

# 7. Update documentation
git add README.md
npm run commit
# Type: docs
# Scope: README
# Description: add validation examples
git push

# 8. Merge to dev
git checkout dev
git merge feature/add-validation

# 9. Delete both local and remote branches
git branch -d feature/add-validation
git push origin --delete feature/add-validation

# 10. Push dev
git push origin dev
```

---

### Scenario 2: Quick Bug Fix (< 30 minutes)

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

# 5. Merge to dev immediately (no need to push feature branch)
git checkout dev
git merge fix/bind-null-handling

# 6. Delete local branch and push dev
git branch -d fix/bind-null-handling
git push origin dev
```

---

### Scenario 3: Adding Tests (Quick work)

```bash
# 1. Create test branch from dev
git checkout dev
git pull origin dev
git checkout -b test/map

# 2. Write tests
# ... add tests for Map method ...

# 3. Commit
git add tests/Result_Map.cs
npm run commit
# Type: test
# Scope: Map
# Description: add Map method tests

# 4. Merge immediately (no push needed)
git checkout dev
git merge test/map

# 5. Cleanup and push
git branch -d test/map
git push origin dev
```

---

### Scenario 4: Hotfix for Production

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

# 7. Merge back to dev (IMPORTANT!)
git checkout dev
git merge main
git push origin dev

# 8. Cleanup (delete both local and remote)
git branch -d hotfix/critical-memory-leak
git push origin --delete hotfix/critical-memory-leak
```

---

### Scenario 5: Preparing a Release

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

### Problem: Pushed feature branch but want to delete it
```bash
# Delete both local and remote
git branch -d feature/old-feature
git push origin --delete feature/old-feature
```

### Problem: Forgot to delete remote branch after merging
```bash
# List all remote branches
git branch -r

# Delete specific remote branch
git push origin --delete feature/old-feature
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
   ✅ test/map-method
   ❌ my-branch
   ❌ temp
   ```

4. **Push feature branches for longer work**
   ```bash
   # If working > 1 hour, push for backup
   git push -u origin feature/my-feature
   ```

5. **Delete both local AND remote branches**
   ```bash
   git branch -d feature/done
   git push origin --delete feature/done
   ```

6. **Preview releases before creating**
   ```bash
   npm run release:dry  # Always do this first!
   ```

7. **Test before releasing**
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

6. **Don't leave remote branches after merging**
   ```bash
   # Always cleanup both
   git branch -d feature/done
   git push origin --delete feature/done
   ```

---

## 📚 Quick Reference

### Essential Commands - Simple Workflow
```bash
# Quick changes (merge immediately)
git checkout dev                    # Start on dev
git pull origin dev                 # Get latest
git checkout -b test/my-test        # Create branch
npm run commit                      # Make commit
git checkout dev                    # Back to dev
git merge test/my-test              # Merge
git branch -d test/my-test          # Delete local
git push origin dev                 # Push dev
```

### Essential Commands - Safe Workflow
```bash
# Complex changes (multi-day work)
git checkout dev                    # Start on dev
git pull origin dev                 # Get latest
git checkout -b feature/my-feature  # Create branch
npm run commit                      # Make commit
git push -u origin feature/my-feature  # Push for backup

# ... continue work over days ...
npm run commit
git push

# When done
git checkout dev                    # Back to dev
git merge feature/my-feature        # Merge feature
git branch -d feature/my-feature    # Delete local
git push origin --delete feature/my-feature  # Delete remote
git push origin dev                 # Push dev
```

### Releases (from main branch)
```bash
git checkout main                   # Switch to main
git merge dev                       # Bring dev changes
npm run release:dry                 # Preview release
npm run release:minor               # Create release
git push --follow-tags              # Push with tags
git checkout dev                    # Back to dev
git merge main                      # Sync dev with main
git push origin dev                 # Push dev
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

### Cleanup Commands
```bash
# Delete merged local branches
git branch --merged dev | grep -v "^\*\|main\|dev" | xargs git branch -d

# View all remote branches
git branch -r

# Prune deleted remote branches from local
git remote prune origin
```

---

## 🆘 Need Help?

### Quick Links

- **Repository**: https://github.com/reslava/nuget-package-reslava-result
- **Issues**: https://github.com/reslava/nuget-package-reslava-result/issues
- **Releases**: https://github.com/reslava/nuget-package-reslava-result/releases

### Common Questions

**Q: Should I push feature branches to origin?**
A: 
- **Quick work (< 1 hour)**: Optional, you're merging soon anyway
- **Longer work (> 1 hour)**: Yes, always push for backup
- **Multi-day features**: Yes, absolutely push regularly

**Q: Should I delete feature branches after merging?**
A: Yes! Delete **both local and remote** branches to keep workspace clean:
```bash
git branch -d feature/done
git push origin --delete feature/done
```

**Q: What if I forgot to delete a remote branch?**
A: No problem, delete it anytime:
```bash
git push origin --delete feature/old-branch
```

**Q: How often should I release?**
A: 
- Beta: Weekly or as needed for testing
- Patch: As needed for bugs (can be daily)
- Minor: When features are complete (every 1-2 weeks)
- Major: Rarely (months or years)

**Q: Can I work directly on dev for small changes?**
A: Yes! For tiny changes (typos, small fixes), committing directly to dev is fine.

**Q: How do I see which remote branches exist?**
A: Use `git branch -r` to list all remote branches.

---

## 🎓 Learning Path

### Week 1: Basics
- [ ] Use `npm run commit` for all commits
- [ ] Learn commit types (feat, fix, docs, test)
- [ ] Practice writing good commit messages
- [ ] Work on feature branches
- [ ] Understand when to push vs. not push

### Week 2: Branching
- [ ] Create feature branches regularly
- [ ] Merge features to dev
- [ ] Keep dev stable
- [ ] Delete both local and remote branches
- [ ] Practice the simple vs. safe workflow

### Week 3: Releases
- [ ] Practice `npm run release:dry`
- [ ] Create a beta release
- [ ] Review generated CHANGELOG
- [ ] Understand semantic versioning
- [ ] Clean up after releases

### Week 4: Advanced
- [ ] Handle merge conflicts
- [ ] Create hotfixes from main
- [ ] Manage multiple feature branches
- [ ] Perfect your release process
- [ ] Master branch cleanup

---

## 🎉 You're Ready!

You now know:
- ✅ Branch strategy (main, dev, feature/*, test/*, fix/*)
- ✅ Making proper commits with `npm run commit`
- ✅ When to push feature branches (and when not to)
- ✅ Deleting both local and remote branches
- ✅ Creating releases with `commit-and-tag-version`
- ✅ Semantic versioning
- ✅ Complete workflow from feature to release

**Remember**: 
- Work on feature branches
- Push long-running branches for backup
- Merge to dev frequently
- Delete both local and remote branches after merging
- Release from main
- Always use `npm run commit`!

Happy coding! 🚀

---

*Last updated: 2026-01-10*
*Using: commit-and-tag-version for releases*
*Branch strategy: Git Flow (main + dev)*
*Branch cleanup: Local AND remote deletion*
