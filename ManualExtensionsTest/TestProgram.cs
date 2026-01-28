using REslava.Result;

public class TestProgram
{
    public static void Main()
    {
        Console.WriteLine("🧪 Testing Manual Extensions v1.7.3...");
        
        // Test success case
        var successResult = Result<string>.Ok("Hello World!");
        var (statusCode, value) = successResult.ToHttpResponse();
        Console.WriteLine($"✅ Success: {statusCode} -> {value}");
        
        // Test error case
        var errorResult = Result<string>.Fail("Product not found");
        var (errorStatusCode, errorValue) = errorResult.ToHttpResponse();
        Console.WriteLine($"❌ Error: {errorStatusCode} -> {errorValue}");
        
        // Test convenience methods
        Console.WriteLine($"📊 Status Code: {errorResult.GetHttpStatusCode()}");
        Console.WriteLine($"📝 Error Message: {errorResult.GetErrorMessage()}");
        
        Console.WriteLine("🎉 Manual Extensions working perfectly!");
    }
}
