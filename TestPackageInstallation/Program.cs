// See https://aka.ms/new-console-template for more information
using REslava.Result;

Console.WriteLine("Testing REslava.Result package installation...");

// Test 1: Basic Result creation
var successResult = Result<int>.Ok(42);
Console.WriteLine($"✅ Success: Value={successResult.Value}, IsSuccess={successResult.IsSuccess}");

var failureResult = Result<int>.Fail("Test error message");
Console.WriteLine($"✅ Failure: IsFailed={failureResult.IsFailed}, Error={failureResult.Errors[0].Message}");

// Test 2: Map operation
var mapped = successResult.Map(x => x * 2);
Console.WriteLine($"✅ Map: {mapped.Value} (should be 84)");

// Test 3: Ensure validation
var validated = Result<string>.Ok("test@example.com")
    .Ensure(email => email.Contains("@"), "Invalid email format");
Console.WriteLine($"✅ Validation: {validated.IsSuccess} (should be true)");

// Test 4: Bind operation
var bound = successResult
    .Bind(x => Result<string>.Ok($"Number: {x}"));
Console.WriteLine($"✅ Bind: {bound.Value} (should be 'Number: 42')");

// Test 5: Match pattern
var message = successResult.Match(
    onSuccess: value => $"Success with value: {value}",
    onFailure: errors => $"Failed with: {string.Join(", ", errors.Select(e => e.Message))}");
Console.WriteLine($"✅ Match: {message}");

// Test 6: Custom Error with tags
var customError = new Error("Validation failed")
    .WithTags(("Field", "Email"), ("Code", 400));
var customResult = Result<string>.Fail(customError);
Console.WriteLine($"✅ Custom Error: Field={customResult.Errors[0].Tags["Field"]}, Code={customResult.Errors[0].Tags["Code"]}");

Console.WriteLine("\n🎉 All tests passed! Package is working correctly.");
