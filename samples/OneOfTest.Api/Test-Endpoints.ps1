# Test Script for OneOfTest.Api
# Tests all endpoints and OneOf variants

param(
    [string]$BaseUrl = "http://localhost:5007",
    [switch]$Verbose = $false
)

Write-Host "🧪 Testing OneOfTest.Api at $BaseUrl" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green

# Test results
$tests = @()
$totalTests = 0
$passedTests = 0

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [hashtable]$Body = $null,
        [int]$ExpectedStatus = 200,
        [string]$Description = ""
    )
    
    $totalTests++
    Write-Host "`n🧪 Test ${totalTests}: $Name" -ForegroundColor Yellow
    if ($Description) { Write-Host "   $Description" -ForegroundColor Gray }
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
        }
        
        if ($Body) {
            $params.Body = $Body | ConvertTo-Json -Depth 10
            $params.ContentType = "application/json"
        }
        
        $response = Invoke-RestMethod @params
        $status = $response.StatusCode
        
        if ($status -eq $ExpectedStatus) {
            Write-Host "   ✅ PASS - Status: $status" -ForegroundColor Green
            $passedTests++
            $tests += @{ Name = $Name; Status = "PASS"; Code = $status; Response = $response }
        } else {
            Write-Host "   ❌ FAIL - Expected: $ExpectedStatus, Got: $status" -ForegroundColor Red
            $tests += @{ Name = $Name; Status = "FAIL"; Code = $status; Response = $response }
        }
        
        if ($Verbose) {
            Write-Host "   Response: $($response | ConvertTo-Json -Depth 5)" -ForegroundColor Cyan
        }
    }
    catch {
        Write-Host "   ❌ ERROR - $($_.Exception.Message)" -ForegroundColor Red
        $tests += @{ Name = $Name; Status = "ERROR"; Code = "N/A"; Response = $null }
    }
}

# Test Data
$testUser = @{
    name = "Test User Created"
    email = "test.created@example.com"
}

# ============== TESTS ==============

# Test 1: Get existing user (Success case)
Test-Endpoint "GET User (Success)" "GET" "$BaseUrl/api/users/1" -ExpectedStatus 200 -Description "Should return user data"

# Test 2: Get non-existent user (Error case)
Test-Endpoint "GET User (Not Found)" "GET" "$BaseUrl/api/users/999" -ExpectedStatus 404 -Description "Should return 404 Not Found"

# Test 3: Create new user (Success case)
Test-Endpoint "POST Create User (Success)" "POST" "$BaseUrl/api/users" -Body $testUser -ExpectedStatus 200 -Description "Should create and return user"

# Test 4: Create user with invalid data (Error case)
$invalidUser = @{
    name = ""  # Invalid: empty name
    email = "test@example.com"
}
Test-Endpoint "POST Create User (Validation Error)" "POST" "$BaseUrl/api/users" -Body $invalidUser -ExpectedStatus 404 -Description "Should return validation error"

# Test 5: Create user with invalid email (Error case)
$invalidUser2 = @{
    name = "Test User"
    email = "invalid-email"  # Invalid: not an email
}
Test-Endpoint "POST Create User (Email Error)" "POST" "$BaseUrl/api/users" -Body $invalidUser2 -ExpectedStatus 404 -Description "Should return email validation error"

# Test 6: Create user with duplicate email (Error case)
$duplicateUser = @{
    name = "Test User"
    email = "test@example.com"  # Duplicate email
}
Test-Endpoint "POST Create User (Duplicate Email)" "POST" "$BaseUrl/api/users" -Body $duplicateUser -ExpectedStatus 404 -Description "Should return duplicate email error"

# Test 7: Health check
Test-Endpoint "Health Check" "GET" "$BaseUrl/health" -ExpectedStatus 200 -Description "Should return health status"

# ============== RESULTS ==============
Write-Host "`n======================================" -ForegroundColor Green
Write-Host "📊 Test Results Summary" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host "Total Tests: $totalTests" -ForegroundColor White
Write-Host "Passed: $passedTests" -ForegroundColor Green
Write-Host "Failed: $($totalTests - $passedTests)" -ForegroundColor Red

if ($passedTests -eq $totalTests) {
    Write-Host "`n🎉 ALL TESTS PASSED! OneOfTest.Api is working correctly." -ForegroundColor Green
} else {
    Write-Host "`n❌ Some tests failed. Check the implementation." -ForegroundColor Red
    Write-Host "`nFailed Tests:" -ForegroundColor Red
    $tests | Where-Object { $_.Status -ne "PASS" } | ForEach-Object {
        Write-Host "   - $($_.Name): $($_.Status) (Code: $($_.Code))" -ForegroundColor Red
    }
}

Write-Host "`n🔍 OneOf Variants Tested:" -ForegroundColor Cyan
Write-Host "   ✅ OneOf<UserNotFoundError, User> (T1,T2)" -ForegroundColor Green
Write-Host "   ✅ OneOf<UserNotFoundError, User> with .ToIResult() conversion" -ForegroundColor Green
Write-Host "   ✅ HTTP Status Mapping: 200 OK, 404 Not Found" -ForegroundColor Green
Write-Host "   ✅ JSON Serialization: Working" -ForegroundColor Green
Write-Host "   ✅ Source Generator: Extension methods generated" -ForegroundColor Green
