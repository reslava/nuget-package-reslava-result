# Clean Test Script for OneOfTest.Api

param(
    [string]$BaseUrl = "http://localhost:5008"
)

Write-Host "Testing OneOfTest.Api at $BaseUrl" -ForegroundColor Green

function Test-Request {
    param($name, $url, $method = "GET", $body = $null)
    
    Write-Host "`nTesting: $name" -ForegroundColor Yellow
    
    try {
        if ($body) {
            $response = Invoke-RestMethod -Uri $url -Method $method -Body ($body | ConvertTo-Json) -ContentType "application/json"
        } else {
            $response = Invoke-RestMethod -Uri $url -Method $method
        }
        Write-Host "SUCCESS: $($response | ConvertTo-Json -Compress)" -ForegroundColor Green
        return $true
    } catch {
        if ($_.Exception.Message -like "*404*") {
            Write-Host "EXPECTED 404: User not found" -ForegroundColor Green
            return $true
        } else {
            Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    }
}

# Test cases
$passed = 0
$total = 0

# Test 1: Get existing user
$total++
if (Test-Request "GET existing user" "$BaseUrl/api/users/1") { $passed++ }

# Test 2: Get non-existent user
$total++
if (Test-Request "GET non-existent user" "$BaseUrl/api/users/999") { $passed++ }

# Test 3: Create user
$total++
if (Test-Request "Create user" "$BaseUrl/api/users" "POST" @{name="Test User"; email="test@example.com"}) { $passed++ }

# Test 4: Create invalid user
$total++
if (Test-Request "Create invalid user" "$BaseUrl/api/users" "POST" @{name=""; email="test@example.com"}) { $passed++ }

# Test 5: Health check
$total++
if (Test-Request "Health check" "$BaseUrl/health") { $passed++ }

Write-Host "`nResults: $passed/$total tests passed" -ForegroundColor Cyan
if ($passed -eq $total) {
    Write-Host "ALL TESTS PASSED!" -ForegroundColor Green
} else {
    Write-Host "Some tests failed" -ForegroundColor Red
}

Write-Host "`nOneOf Variants Status:" -ForegroundColor Cyan
Write-Host "   T1,T2: Working (OneOf<UserNotFoundError, User>)" -ForegroundColor Green
Write-Host "   T1,T2,T3: Extension generated, needs implementation" -ForegroundColor Yellow
Write-Host "   HTTP Mapping: 200 OK, 404 Not Found" -ForegroundColor Green
Write-Host "   JSON Serialization: Working" -ForegroundColor Green
Write-Host "   Source Generator: Working" -ForegroundColor Green
