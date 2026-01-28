using REslava.Result.SourceGenerators;

// Test assembly attribute
[assembly: GenerateResultExtensions]

public class TestProgram
{
    public static void Main()
    {
        Console.WriteLine("🧪 Testing Source Generator in Isolation...");
        
        // Test if simple test generator works
        try 
        {
            var test = "hello".TestMethod();
            Console.WriteLine($"✅ TestGenerator works: {test}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TestGenerator failed: {ex.Message}");
        }
        
        // Test if ToIResult extension method exists
        try 
        {
            var result = REslava.Result.Result<string>.Ok("test");
            var httpResult = result.ToIResult();
            Console.WriteLine("✅ SUCCESS: ToIResult() extension method found!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FAILURE: {ex.Message}");
        }
    }
}
