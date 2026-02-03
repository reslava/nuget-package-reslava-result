# Clean-Before-Test Script for REslava.Result Source Generators
# This script ensures a clean environment before testing source generators
# Usage: .\clean-before-test.ps1

param(
    [switch]$Verbose,
    [switch]$Confirm
)

Write-Host "🧹 Starting Clean-Before-Test Script..." -ForegroundColor Green

# Function to safely remove directories
function Remove-BuildArtifacts {
    param(
        [string]$Path,
        [string]$ProjectName
    )
    
    $artifacts = @("bin", "obj", "GeneratedFiles")
    
    foreach ($artifact in $artifacts) {
        $fullPath = Join-Path $Path $artifact
        if (Test-Path $fullPath) {
            if ($Verbose) {
                Write-Host "  Removing $artifact from $ProjectName..." -ForegroundColor Yellow
            }
            try {
                Remove-Item -Path $fullPath -Recurse -Force -ErrorAction SilentlyContinue
                if ($Verbose) {
                    Write-Host "    ✅ Removed $fullPath" -ForegroundColor Green
                }
            }
            catch {
                Write-Host "    ❌ Failed to remove $fullPath : $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    }
}

# Function to run dotnet clean
function Invoke-DotNetClean {
    param(
        [string]$Path,
        [string]$ProjectName,
        [string]$ProjectFile = $null
    )
    
    Write-Host "🧹 Cleaning $ProjectName..." -ForegroundColor Cyan
    
    Push-Location $Path
    try {
        if ($ProjectFile) {
            $result = dotnet clean $ProjectFile --verbosity quiet
        } else {
            $result = dotnet clean --verbosity quiet
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ $ProjectName cleaned successfully" -ForegroundColor Green
        } else {
            Write-Host "  ❌ $ProjectName clean failed" -ForegroundColor Red
        }
    }
    finally {
        Pop-Location
    }
}

# Function to verify clean state
function Test-CleanState {
    param(
        [string]$Path,
        [string]$ProjectName
    )
    
    Write-Host "🔍 Verifying clean state for $ProjectName..." -ForegroundColor Cyan
    
    $artifacts = @("bin", "obj", "GeneratedFiles")
    $foundArtifacts = @()
    
    foreach ($artifact in $artifacts) {
        $fullPath = Join-Path $Path $artifact
        if (Test-Path $fullPath) {
            $foundArtifacts += $artifact
        }
    }
    
    if ($foundArtifacts.Count -eq 0) {
        Write-Host "  ✅ $ProjectName is clean" -ForegroundColor Green
        return $true
    } else {
        Write-Host "  ⚠️  $ProjectName still has artifacts: $($foundArtifacts -join ', ')" -ForegroundColor Yellow
        return $false
    }
}

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir

Write-Host "📁 Root directory: $RootDir" -ForegroundColor Blue

# Define project paths
$Projects = @{
    "Core Library" = @{
        Path = Join-Path $RootDir "src"
        ProjectFile = "REslava.Result.csproj"
    }
    "Source Generator" = @{
        Path = Join-Path $RootDir "SourceGenerator"
        ProjectFile = "REslava.Result.SourceGenerators.csproj"
    }
    "Test API" = @{
        Path = Join-Path $RootDir "samples\OneOfTest.Api"
        ProjectFile = "OneOfTest.Api.csproj"
    }
}

# Step 1: Remove build artifacts
Write-Host "`n🗑️  Step 1: Removing build artifacts..." -ForegroundColor Magenta
foreach ($project in $Projects.GetEnumerator()) {
    $projectName = $project.Key
    $projectInfo = $project.Value
    
    Write-Host "  Processing $projectName..." -ForegroundColor Yellow
    Remove-BuildArtifacts -Path $projectInfo.Path -ProjectName $projectName
}

# Step 2: Run dotnet clean
Write-Host "`n🧹 Step 2: Running dotnet clean..." -ForegroundColor Magenta
foreach ($project in $Projects.GetEnumerator()) {
    $projectName = $project.Key
    $projectInfo = $project.Value
    
    Invoke-DotNetClean -Path $projectInfo.Path -ProjectName $projectName -ProjectFile $projectInfo.ProjectFile
}

# Step 3: Final verification
Write-Host "`n🔍 Step 3: Verifying clean state..." -ForegroundColor Magenta
$allClean = $true
foreach ($project in $Projects.GetEnumerator()) {
    $projectName = $project.Key
    $projectInfo = $project.Value
    
    $isClean = Test-CleanState -Path $projectInfo.Path -ProjectName $projectName
    if (-not $isClean) {
        $allClean = $false
    }
}

# Summary
Write-Host "`n📊 Clean-Before-Test Summary:" -ForegroundColor Cyan
if ($allClean) {
    Write-Host "🎉 All projects are clean! Ready for testing." -ForegroundColor Green
    Write-Host "✅ Environment is ready for source generator testing" -ForegroundColor Green
} else {
    Write-Host "⚠️  Some projects may still have artifacts. Manual cleanup may be required." -ForegroundColor Yellow
    Write-Host "❌ Environment may not be fully clean" -ForegroundColor Yellow
}

Write-Host "`n🚀 Clean-Before-Test Script completed!" -ForegroundColor Green
Write-Host "You can now run your tests with confidence!" -ForegroundColor Blue
