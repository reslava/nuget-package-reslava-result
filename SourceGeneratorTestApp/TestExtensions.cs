using REslava.Result;
using REslava.Result.SourceGenerators;

// Enable the source generator
[assembly: GenerateResultExtensions]

// Test that the extensions are generated and working
public static class TestRunner
{
    public static void RunTest()
    {
        Console.WriteLine("Testing generated extensions...");
        
        // Test basic ToIResult
        var successResult = Result<string>.Ok("Hello World!");
        Console.WriteLine($"Success result: {successResult.IsSuccess}");
        
        // Test error result
        var errorResult = Result<string>.Fail("Test error");
        Console.WriteLine($"Error result: {errorResult.IsSuccess}");
        
        Console.WriteLine("Extensions test completed successfully!");
    }
}
