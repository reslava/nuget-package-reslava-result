# Comprehensive test script for all REslava.Result generators
# Tests all endpoints and verifies responses

Write-Host "🚀 Starting comprehensive REslava.Result testing..." -ForegroundColor Green
$baseUrl = "http://localhost:5000"
$testResults = @()

# Function to test endpoint
function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Url,
        [string]$ExpectedResponse,
        [int]$ExpectedStatus = 200
    )
    
    try {
        Write-Host "🔍 Testing: $Name" -ForegroundColor Yellow
        Write-Host "   URL: $Url" -ForegroundColor Gray
        
        $response = Invoke-RestMethod -Uri $Url -Method Get -ErrorAction Stop
        $statusCode = 200  # Success for 200 responses
        
        if ($statusCode -eq $ExpectedStatus -and $response -like "*$ExpectedResponse*") {
            Write-Host "   ✅ PASS: Response matches expected" -ForegroundColor Green
            $testResults += [PSCustomObject]@{ Test = $Name; Status = "PASS"; Response = $response }
        } else {
            Write-Host "   ❌ FAIL: Expected '$ExpectedResponse' but got '$response'" -ForegroundColor Red
            $testResults += [PSCustomObject]@{ Test = $Name; Status = "FAIL"; Response = $response }
        }
    }
    catch {
        Write-Host "   ❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += [PSCustomObject]@{ Test = $Name; Status = "ERROR"; Response = "Exception" }
    }
    Write-Host ""
}

# Test all endpoints
Write-Host "📋 Testing Basic Controllers..." -ForegroundColor Cyan
Test-Endpoint -Name "Basic Controller" -Url "$baseUrl/api/test" -ExpectedResponse "Simple test works - controllers are registered!"

Write-Host "📋 Testing SmartEndpoints..." -ForegroundColor Cyan
Test-Endpoint -Name "SmartEndpoints Extension" -Url "$baseUrl/api/simple/test" -ExpectedResponse "Simple test works - SmartEndpoints active!"

Write-Host "📋 Testing Result Generator..." -ForegroundColor Cyan
Test-Endpoint -Name "Result<T> to IResult" -Url "$baseUrl/api/test-all/result" -ExpectedResponse "Result conversion works!"

Write-Host "📋 Testing OneOf Generators..." -ForegroundColor Cyan
Test-Endpoint -Name "OneOf2 to IResult" -Url "$baseUrl/api/test-all/oneof2" -ExpectedResponse "OneOf2 conversion works!"
Test-Endpoint -Name "OneOf3 to IResult" -Url "$baseUrl/api/test-all/oneof3" -ExpectedResponse "OneOf3 conversion works!"
Test-Endpoint -Name "OneOf4 to IResult" -Url "$baseUrl/api/test-all/oneof4" -ExpectedResponse "OneOf4 conversion works!"

Write-Host "📋 Testing Error Scenarios..." -ForegroundColor Cyan
Test-Endpoint -Name "Error Handling" -Url "$baseUrl/api/test-all/error" -ExpectedResponse "Test error scenario"

Write-Host "📋 Testing Health Endpoint..." -ForegroundColor Cyan
Test-Endpoint -Name "Health Check" -Url "$baseUrl/health" -ExpectedResponse "healthy"

# Summary
Write-Host "📊 TEST SUMMARY" -ForegroundColor Magenta
Write-Host "=================" -ForegroundColor Magenta

$passed = ($testResults | Where-Object { $_.Status -eq "PASS" }).Count
$failed = ($testResults | Where-Object { $_.Status -eq "FAIL" }).Count
$errors = ($testResults | Where-Object { $_.Status -eq "ERROR" }).Count
$total = $testResults.Count

Write-Host "Total Tests: $total" -ForegroundColor White
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor Red
Write-Host "Errors: $errors" -ForegroundColor Red

if ($failed -eq 0 -and $errors -eq 0) {
    Write-Host "🎉 ALL TESTS PASSED! REslava.Result is working correctly!" -ForegroundColor Green
} else {
    Write-Host "⚠️  Some tests failed. Check the results above." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📋 Detailed Results:" -ForegroundColor Cyan
$testResults | Format-Table -AutoSize
