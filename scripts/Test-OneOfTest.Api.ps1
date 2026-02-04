# Comprehensive Testing Script for All REslava.Result Samples

param(
    [string]$BasePort = "5000",
    [switch]$Verbose = $false,
    [switch]$StopOnFailure = $false
)

Write-Host "🧪 REslava.Result Comprehensive Testing" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

$testResults = @()
$totalTests = 0
$passedTests = 0

function Test-Sample {
    param(
        [string]$Name,
        [string]$Path,
        [string]$Url,
        [hashtable]$Endpoints
    )
    
    Write-Host "`n🔍 Testing Sample: $Name" -ForegroundColor Yellow
    Write-Host "Path: $Path" -ForegroundColor Gray
    Write-Host "URL: $Url" -ForegroundColor Gray
    
    $sampleResults = @()
    $samplePassed = 0
    $sampleTotal = 0
    
    foreach ($endpoint in $Endpoints.GetEnumerator()) {
        $sampleTotal++
        $totalTests++
        
        $testName = "$($endpoint.Key) - $($endpoint.Value.Method)"
        Write-Host "  Testing: $testName" -ForegroundColor Cyan
        
        try {
            $params = @{
                Uri = "$Url$($endpoint.Value.Path)"
                Method = $endpoint.Value.Method
            }
            
            if ($endpoint.Value.Body) {
                $params.Body = $endpoint.Value.Body | ConvertTo-Json -Depth 10
                $params.ContentType = "application/json"
            }
            
            $response = Invoke-RestMethod @params
            Write-Host "    ✅ PASS - Status: $($response.StatusCode ?? '200')" -ForegroundColor Green
            $samplePassed++
            $passedTests++
            
            if ($Verbose) {
                Write-Host "    Response: $($response | ConvertTo-Json -Compress)" -ForegroundColor Gray
            }
            
            $sampleResults += @{
                Test = $testName
                Status = "PASS"
                Response = $response
            }
        }
        catch {
            $expectedStatus = $endpoint.Value.ExpectedStatus
            if ($expectedStatus -and $_.Exception.Message -like "*$expectedStatus*") {
                Write-Host "    ✅ PASS - Expected $expectedStatus" -ForegroundColor Green
                $samplePassed++
                $passedTests++
                $sampleResults += @{
                    Test = $testName
                    Status = "PASS"
                    Response = "Expected $expectedStatus"
                }
            } else {
                Write-Host "    ❌ FAIL - $($_.Exception.Message)" -ForegroundColor Red
                $sampleResults += @{
                    Test = $testName
                    Status = "FAIL"
                    Error = $_.Exception.Message
                }
                
                if ($StopOnFailure) {
                    Write-Host "🛑 Stopping on failure as requested" -ForegroundColor Red
                    return $false
                }
            }
        }
    }
    
    $testResults += @{
        Sample = $Name
        Path = $Path
        Url = $Url
        Passed = $samplePassed
        Total = $sampleTotal
        Results = $sampleResults
    }
    
    Write-Host "  Sample Results: $samplePassed/$sampleTotal tests passed" -ForegroundColor $(if ($samplePassed -eq $sampleTotal) { "Green" } else { "Yellow" })
    return $samplePassed -eq $sampleTotal
}

# Sample Definitions
$samples = @(
    @{
        Name = "OneOfTest.Api (T1,T2)"
        Path = "samples\OneOfTest.Api"
        Url = "http://localhost:5000"
        Endpoints = @{
            "GET User Success" = @{
                Path = "/api/users/1"
                Method = "GET"
                ExpectedStatus = "200"
            }
            "GET User Not Found" = @{
                Path = "/api/users/999"
                Method = "GET"
                ExpectedStatus = "404"
            }
            "POST Create User" = @{
                Path = "/api/users"
                Method = "POST"
                Body = @{ name = "Test User"; email = "test@example.com" }
                ExpectedStatus = "404"
            }
            "Health Check" = @{
                Path = "/health"
                Method = "GET"
                ExpectedStatus = "200"
            }
        }
    }
    # Add more samples as they become available
)

# Run Tests
$allPassed = $true
foreach ($sample in $samples) {
    $result = Test-Sample @sample
    if (-not $result) {
        $allPassed = $false
    }
}

# Final Results
Write-Host "`n=====================================" -ForegroundColor Green
Write-Host "📊 FINAL RESULTS" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Total Tests: $totalTests" -ForegroundColor White
Write-Host "Passed: $passedTests" -ForegroundColor Green
Write-Host "Failed: $($totalTests - $passedTests)" -ForegroundColor $(if ($totalTests - $passedTests -gt 0) { "Red" } else { "Green" })

if ($allPassed -and $passedTests -eq $totalTests) {
    Write-Host "`n🎉 ALL TESTS PASSED! All samples are working correctly." -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n❌ SOME TESTS FAILED!" -ForegroundColor Red
    Write-Host "`nFailed Samples:" -ForegroundColor Red
    $testResults | Where-Object { $_.Passed -lt $_.Total } | ForEach-Object {
        Write-Host "  - $($_.Sample): $($_.Passed)/$($_.Total) tests passed" -ForegroundColor Red
        $_.Results | Where-Object { $_.Status -eq "FAIL" } | ForEach-Object {
            Write-Host "    - $($_.Test): $($_.Error)" -ForegroundColor Red
        }
    }
    exit 1
}
