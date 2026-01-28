using Generated.Simple;

public class TestProgram
{
    public static void Main()
    {
        Console.WriteLine("🧪 Testing Source Generator Loading...");
        
        // Test if ANY generator is working
        try 
        {
            var test = "hello".TestMethod();
            Console.WriteLine($"✅ SUCCESS: Generator loaded! {test}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FAILURE: Generator not loaded - {ex.Message}");
        }
    }
}
