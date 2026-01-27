using REslava.Result;
using REslava.Result.SourceGenerators;

// Enable the source generator
[assembly: GenerateResultExtensions]

var result = Result<string>.Ok("Hello World!");
Console.WriteLine($"Result created: {result.IsSuccess}");

// Test if the extension method exists
try 
{
    var iresult = result.ToIResult();
    Console.WriteLine("✅ ToIResult() extension method found and working!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
}

Console.WriteLine("Test completed.");
