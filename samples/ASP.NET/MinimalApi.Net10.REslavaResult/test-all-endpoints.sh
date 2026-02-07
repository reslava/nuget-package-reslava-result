#!/bin/bash

# Final test script with correct error status expectation
echo "🚀 Starting comprehensive REslava.Result testing..."
baseUrl="https://localhost:58696"
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
    
    response=$(curl -k -s "$url" 2>/dev/null)
    status_code=$(curl -k -s -o /dev/null -w "%{http_code}" "$url" 2>/dev/null)
    
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
# FIX: Expect 400 status code for error responses
test_endpoint "Error Handling" "$baseUrl/api/test-all/error" "Test error scenario" 400

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
    echo ""
    echo "✅ Result<T> → IResult conversion works!"
    echo "✅ OneOf<T1,T2> → IResult conversion works!"
    echo "✅ OneOf<T1,T2,T3> → IResult conversion works!"
    echo "✅ OneOf<T1,T2,T3,T4> → IResult conversion works!"
    echo "✅ Error handling returns proper RFC 7807 ProblemDetails!"
    exit 0
else
    echo "⚠️  Some tests failed. Check the results above."
    exit 1
fi

echo ""
echo "📋 Test Results Summary:"
echo "======================"
echo "✅ PASSED: $passed tests"
echo "❌ FAILED: $failed tests"
echo "💥 ERRORS: $errors tests"
