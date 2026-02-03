# Clean-Before-Test Scripts Documentation

## Overview

These scripts ensure a clean environment before testing REslava.Result source generators to avoid false conclusions and time waste.

## Scripts

### 1. clean-before-test.ps1 (Full Version)
**Purpose:** Complete environment cleanup with detailed feedback and verification

**Usage:**
```powershell
# Basic usage
.\clean-before-test.ps1

# Verbose output
.\clean-before-test.ps1 -Verbose

# With confirmation prompts
.\clean-before-test.ps1 -Confirm
```

**Features:**
- ✅ Removes all bin/obj/GeneratedFiles folders
- ✅ Runs dotnet clean on all projects
- ✅ Verifies clean state
- ✅ Detailed progress reporting
- ✅ Error handling and reporting
- ✅ Summary of cleanup results

### 2. quick-clean.ps1 (Fast Version)
**Purpose:** Quick cleanup for rapid iteration

**Usage:**
```powershell
# Quick cleanup
.\quick-clean.ps1
```

**Features:**
- ✅ Parallel cleanup for speed
- ✅ Minimal output
- ✅ Fast execution
- ✅ Same cleanup effectiveness

## Projects Cleaned

Both scripts clean these projects:
1. **Core Library** (`src/REslava.Result.csproj`)
2. **Source Generator** (`SourceGenerator/REslava.Result.SourceGenerators.csproj`)
3. **Test API** (`samples/OneOfTest.Api/OneOfTest.Api.csproj`)

## Cleanup Steps

1. **Remove Build Artifacts**
   - Delete `bin/`, `obj/`, `GeneratedFiles/` folders
   - Force removal with error suppression

2. **Run dotnet clean**
   - Core library: `dotnet clean`
   - Source generator: `dotnet clean REslava.Result.SourceGenerators.csproj`
   - Test API: `dotnet clean`

3. **Verify Clean State**
   - Check for remaining artifacts
   - Report clean/dirty status

## When to Use

### Before Testing Source Generators
- Always run before testing generator changes
- Use before debugging generator issues
- Run before verifying generated output

### Before Building from Scratch
- Use when switching between branches
- Run after merging changes
- Use before release builds

### When Issues Occur
- When generators appear to fail unexpectedly
- When cached artifacts might interfere
- When build behavior is inconsistent

## Best Practices

1. **Always clean before testing** source generators
2. **Use full version** for thorough verification
3. **Use quick version** for rapid iteration
4. **Run from root directory** of the project
5. **Check output** for any errors or warnings

## Troubleshooting

### Script Fails to Run
```powershell
# Set execution policy
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Files Still Exist After Cleanup
- Check if files are in use
- Close Visual Studio or other IDEs
- Run script as administrator
- Manually delete stubborn files

### Permission Errors
- Run PowerShell as administrator
- Check file/folder permissions
- Ensure no processes are using the files

## Integration with Development Workflow

### Recommended Workflow:
1. Make changes to source generator
2. Run `.\clean-before-test.ps1`
3. Build and test
4. Verify generated output
5. Repeat as needed

### Git Integration:
Add scripts to `.gitignore` if you don't want to commit them, or commit them for team consistency.

## Notes

- Scripts are idempotent - safe to run multiple times
- All operations use `-ErrorAction SilentlyContinue` to avoid failures
- Parallel execution in quick version for speed
- Full version provides detailed feedback for debugging
