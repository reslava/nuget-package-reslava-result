# Quick Clean Script - Minimal version for fast cleanup
# Usage: .\quick-clean.ps1

Write-Host "🧹 Quick Clean Script..." -ForegroundColor Green

$RootDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Clean all projects in parallel
$jobs = @()

# Core Library
$jobs += Start-Job -ScriptBlock {
    param($Path)
    Set-Location $Path
    Remove-Item -Recurse -Force "bin","obj","GeneratedFiles" -ErrorAction SilentlyContinue
    dotnet clean --verbosity quiet
} -ArgumentList (Join-Path $RootDir "src")

# Source Generator
$jobs += Start-Job -ScriptBlock {
    param($Path)
    Set-Location $Path
    Remove-Item -Recurse -Force "bin","obj","GeneratedFiles" -ErrorAction SilentlyContinue
    dotnet clean REslava.Result.SourceGenerators.csproj --verbosity quiet
} -ArgumentList (Join-Path $RootDir "SourceGenerator")

# Test API
$jobs += Start-Job -ScriptBlock {
    param($Path)
    Set-Location $Path
    Remove-Item -Recurse -Force "bin","obj","GeneratedFiles" -ErrorAction SilentlyContinue
    dotnet clean --verbosity quiet
} -ArgumentList (Join-Path $RootDir "samples\OneOfTest.Api")

# Wait for all jobs
$jobs | Wait-Job | Receive-Job

# Cleanup jobs
$jobs | Remove-Job

Write-Host "✅ Quick clean completed!" -ForegroundColor Green
Write-Host "🚀 Ready for testing!" -ForegroundColor Blue
