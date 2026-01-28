using REslava.Result.SourceGenerators;
using REslava.Result;
using Microsoft.AspNetCore.Http;
using Generated.ResultExtensions;

// Enable source generator for this assembly
[assembly: GenerateResultExtensions]

public class TestProgram
{
    public static void Main()
    {
        Console.WriteLine("🧪 Testing Main Generator...");
        
        // Test success case
        var successResult = Result<string>.Ok("Hello World!");
        
        // Test error case
        var errorResult = Result<string>.Fail("Product not found");
        
        Console.WriteLine("✅ Test setup complete!");
        Console.WriteLine($"Success result: {successResult.Value}");
        Console.WriteLine($"Error result: {errorResult.GetErrorMessage()}");
        
        // Test generated extensions
        try 
        {
            Console.WriteLine("🔍 Testing generated ToIResult() extensions...");
            
            // Test success case
            var successIResult = successResult.ToIResult();
            Console.WriteLine($"✅ Success ToIResult() works: {successIResult.GetType().Name}");
            
            // Test error case
            var errorIResult = errorResult.ToIResult();
            Console.WriteLine($"✅ Error ToIResult() works: {errorIResult.GetType().Name}");
            
            Console.WriteLine("🎉 GENERATED EXTENSIONS WORKING!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Generator test failed: {ex.Message}");
        }
    }
}
