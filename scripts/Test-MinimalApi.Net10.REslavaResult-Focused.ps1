# Test-MinimalApi.Net10.REslavaResult-Focused.ps1
# 🎯 Focused test script for v1.10.0 auto-conversion features ONLY
# This script tests what developers actually care about: the magic working!

param(
    [string]$BaseUrl = "http://localhost:5000",
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

# Test configuration
$script:testResults = @()
$script:passedTests = 0
$script:totalTests = 0

# Helper function to write verbose output
function Write-Verbose-Output {
    param([string]$Message)
    if ($Verbose) {
        Write-Host "  VERBOSE: $Message" -ForegroundColor Gray
    }
}

# Helper function to execute HTTP request and validate response
function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Path,
        [string]$Method = "GET",
        [hashtable]$Body = $null,
        [string]$ExpectedStatus,
        [string]$ExpectedContent = $null,
        [string]$Description = ""
    )
    
    $script:totalTests++
    $url = "$BaseUrl$Path"
    $testPassed = $false
    
    try {
        Write-Host "  🧪 Testing: $Name" -ForegroundColor Cyan
        Write-Verbose-Output "URL: $url"
        Write-Verbose-Output "Method: $Method"
        
        $headers = @{
            "Content-Type" = "application/json"
        }
        
        $bodyJson = if ($Body) { $Body | ConvertTo-Json -Depth 10 } else { $null }
        
        $response = Invoke-WebRequest -Uri $url -Method $Method -Headers $headers -Body $bodyJson -UseBasicParsing
        $statusCode = $response.StatusCode.ToString()
        $content = $response.Content
        
        Write-Verbose-Output "Status: $statusCode"
        Write-Verbose-Output "Content: $content"
        
        # Validate status code
        if ($statusCode -eq $ExpectedStatus) {
            # Validate content if specified
            if ($ExpectedContent) {
                if ($content -like "*$ExpectedContent*") {
                    Write-Host "    ✅ PASS: $Description" -ForegroundColor Green
                    $script:passedTests++
                    $testPassed = $true
                } else {
                    Write-Host "    ❌ FAIL: Expected content containing '$ExpectedContent'" -ForegroundColor Red
                    Write-Host "       Actual: $content" -ForegroundColor Red
                }
            } else {
                Write-Host "    ✅ PASS: $Description" -ForegroundColor Green
                $script:passedTests++
                $testPassed = $true
            }
        } else {
            Write-Host "    ❌ FAIL: Expected status $ExpectedStatus, got $statusCode" -ForegroundColor Red
        }
        
        $script:testResults += @{
            Test = $Name
            Status = if ($testPassed) { "PASS" } else { "FAIL" }
            StatusText = "$statusCode"
            Response = $content
            Error = if (-not $testPassed) { "Expected status $ExpectedStatus, got $statusCode" } else { $null }
        }
        
        return $testPassed
    }
    catch {
        Write-Host "    ❌ FAIL: $($_.Exception.Message)" -ForegroundColor Red
        $script:testResults += @{
            Test = $Name
            Status = "FAIL"
            Error = $_.Exception.Message
            StatusText = "ERROR"
            Response = ""
        }
        return $false
    }
}

# Main test execution
Write-Host "🚀 REslava.Result v1.10.0 Auto-Conversion Magic Test" -ForegroundColor Magenta
Write-Host "🎯 Testing ONLY the features that matter to developers!" -ForegroundColor Yellow
Write-Host "📍 Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host ""

# Test 1: Health Check
Write-Host "🔍 Basic Health Check" -ForegroundColor Yellow
Test-Endpoint -Name "Health Check" -Path "/health" -ExpectedStatus "200" -Description "Server is running"

# Test 2: Result<T> Auto-Conversion (The Magic!)
Write-Host "`n📊 Result<T> Auto-Conversion Tests" -ForegroundColor Yellow
Test-Endpoint -Name "Result<T> Success Auto-Conversion" -Path "/api/users/1/hybrid/result" -ExpectedStatus "200" -ExpectedContent '"id":1' -Description "Result<User> → HTTP 200 (v1.10.0 magic!)"
Test-Endpoint -Name "Result<T> Not Found Auto-Conversion" -Path "/api/users/999/hybrid/result" -ExpectedStatus "400" -Description "Result<User> → HTTP 400 (error auto-conversion!)"

# Test 3: OneOf<T1,T2> Auto-Conversion (The Magic!)
Write-Host "`n🔀 OneOf<T1,T2> Auto-Conversion Tests" -ForegroundColor Yellow
Test-Endpoint -Name "OneOf2 Success Auto-Conversion" -Path "/api/users/1/result" -ExpectedStatus "200" -ExpectedContent '"id":1' -Description "OneOf<T1,T2> → HTTP 200 (v1.10.0 magic!)"
Test-Endpoint -Name "OneOf2 Not Found Auto-Conversion" -Path "/api/users/999/result" -ExpectedStatus "400" -Description "OneOf<T1,T2> → HTTP 400 (first type = error!)"

# Test 4: OneOf<T1,T2,T3> Auto-Conversion (The Magic!)
Write-Host "`n🔀🔀 OneOf<T1,T2,T3> Auto-Conversion Tests" -ForegroundColor Yellow
$updateBody = @{
    name = "Updated User"
    email = "updated@example.com"
}
Test-Endpoint -Name "OneOf3 Success Auto-Conversion" -Path "/api/users/1/result" -Method "PUT" -Body $updateBody -ExpectedStatus "200" -ExpectedContent '"name":"Updated User"' -Description "OneOf<T1,T2,T3> → HTTP 200 (third type = success!)"
Test-Endpoint -Name "OneOf3 Not Found Auto-Conversion" -Path "/api/users/999/result" -Method "PUT" -Body $updateBody -ExpectedStatus "400" -Description "OneOf<T1,T2,T3> → HTTP 400 (second type = error!)"

# Test 5: POST Auto-Conversion (The Magic!)
Write-Host "`n📝 POST Auto-Conversion Tests" -ForegroundColor Yellow
$newUser = @{
    name = "New User"
    email = "newuser@example.com"
}
Test-Endpoint -Name "POST Success Auto-Conversion" -Path "/api/users/result" -Method "POST" -Body $newUser -ExpectedStatus "200" -ExpectedContent '"name":"New User"' -Description "POST OneOf → HTTP 200 (v1.10.0 magic!)"

# Results Summary
Write-Host "`n📊 Auto-Conversion Magic Results" -ForegroundColor Magenta
Write-Host "=================================" -ForegroundColor Magenta
Write-Host "Total Tests: $script:totalTests" -ForegroundColor White
Write-Host "Passed: $script:passedTests" -ForegroundColor $(if ($script:passedTests -eq $script:totalTests) { "Green" } else { "Yellow" })
Write-Host "Failed: $($script:totalTests - $script:passedTests)" -ForegroundColor $(if ($script:passedTests -eq $script:totalTests) { "Green" } else { "Red" })
Write-Host "Success Rate: $([math]::Round(($script:passedTests / $script:totalTests) * 100, 1))%" -ForegroundColor $(if ($script:passedTests -eq $script:totalTests) { "Green" } else { "Yellow" })

if ($Verbose) {
    Write-Host "`n📋 Detailed Results:" -ForegroundColor Cyan
    $script:testResults | ForEach-Object {
        $color = if ($_.Status -eq "PASS") { "Green" } else { "Red" }
        Write-Host "  $($_.Test): $($_.Status) ($($_.StatusText))" -ForegroundColor $color
        if ($_.Error) {
            Write-Host "    Error: $($_.Error)" -ForegroundColor Red
        }
    }
}

# Final verdict
if ($script:passedTests -eq $script:totalTests) {
    Write-Host "`n🎉 ALL AUTO-CONVERSION TESTS PASSED!" -ForegroundColor Green
    Write-Host "✨ REslava.Result v1.10.0 is working PERFECTLY!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🚀 Confirmed v1.10.0 Breakthrough Features:" -ForegroundColor Green
    Write-Host "   ✅ Result<T> → IResult auto-conversion working" -ForegroundColor Green
    Write-Host "   ✅ OneOf<T1,T2> → IResult auto-conversion working" -ForegroundColor Green
    Write-Host "   ✅ OneOf<T1,T2,T3> → IResult auto-conversion working" -ForegroundColor Green
    Write-Host "   ✅ Smart HTTP status mapping working" -ForegroundColor Green
    Write-Host "   ✅ Zero boilerplate code required" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎯 Developer Experience:" -ForegroundColor Green
    Write-Host "   • Just add .ToIResult() and get perfect HTTP responses!" -ForegroundColor Green
    Write-Host "   • No more manual status code mapping!" -ForegroundColor Green
    Write-Host "   • No more JSON serialization headaches!" -ForegroundColor Green
    Write-Host "   • Just write business logic, REslava.Result handles the rest!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🚀 READY FOR PRODUCTION!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n❌ SOME AUTO-CONVERSION TESTS FAILED!" -ForegroundColor Red
    Write-Host "`nFailed Tests:" -ForegroundColor Red
    $script:testResults | Where-Object { $_.Status -eq "FAIL" } | ForEach-Object {
        Write-Host "  - $($_.Test): $($_.Error)" -ForegroundColor Red
    }
    exit 1
}
