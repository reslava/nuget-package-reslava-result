#!/bin/bash

# Comprehensive test script for all REslava.Result generators
# Tests all endpoints and verifies responses

echo "🚀 Starting comprehensive REslava.Result testing..."
baseUrl="http://localhost:5000"
passed=0
failed=0
errors=0
total=0

# Function to test endpoint
test_endpoint() {
    local name="$1"
    local url="$2"
    local expected_response="$3"
    local expected_status="${4:-200}"
    
    echo "🔍 Testing: $name"
    echo "   URL: $url"
    
    response=$(curl -s "$url")
    status_code=$(curl -s -o /dev/null -w "%{http_code}" "$url")
    
    if [ "$status_code" -eq "$expected_status" ] && [[ "$response" == *"$expected_response"* ]]; then
        echo "   ✅ PASS: Response matches expected"
        ((passed++))
    else
        echo "   ❌ FAIL: Expected '$expected_response' but got '$response' (Status: $status_code)"
        ((failed++))
    fi
    echo ""
    ((total++))
}

# Test all endpoints
echo "📋 Testing Basic Controllers..."
test_endpoint "Basic Controller" "$baseUrl/api/test" "Simple test works - controllers are registered!"

echo "📋 Testing SmartEndpoints..."
test_endpoint "SmartEndpoints Extension" "$baseUrl/api/simple/test" "Simple test works - SmartEndpoints active!"

echo "📋 Testing Test Controller Simple..."
test_endpoint "Test Controller Simple" "$baseUrl/api/test-all/simple" "Simple test works!"

echo "📋 Testing Result Generator..."
test_endpoint "Result<T> to IResult" "$baseUrl/api/test-all/result" "Result conversion works!"

echo "📋 Testing OneOf Generators..."
test_endpoint "OneOf2 to IResult" "$baseUrl/api/test-all/oneof2" "OneOf2 conversion works!"
test_endpoint "OneOf3 to IResult" "$baseUrl/api/test-all/oneof3" "OneOf3 conversion works!"
test_endpoint "OneOf4 to IResult" "$baseUrl/api/test-all/oneof4" "OneOf4 conversion works!"

echo "📋 Testing Error Scenarios..."
test_endpoint "Error Handling" "$baseUrl/api/test-all/error" "Test error scenario"

echo "📋 Testing Health Endpoint..."
test_endpoint "Health Check" "$baseUrl/health" "healthy"

# Summary
echo "📊 TEST SUMMARY"
echo "================="
echo "Total Tests: $total"
echo "Passed: $passed"
echo "Failed: $failed"
echo "Errors: $errors"

if [ $failed -eq 0 ] && [ $errors -eq 0 ]; then
    echo "🎉 ALL TESTS PASSED! REslava.Result is working correctly!"
else
    echo "⚠️  Some tests failed. Check the results above."
fi

echo ""
echo "📋 Test Results Summary:"
echo "======================"
echo "✅ PASSED: $passed tests"
echo "❌ FAILED: $failed tests"
echo "💥 ERRORS: $errors tests"
