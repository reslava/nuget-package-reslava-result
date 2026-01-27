using REslava.Result;
using REslava.Result.SourceGenerators;

// Enable the source generator
[assembly: GenerateResultExtensions]

// Test if the attribute is available
var attr = new GenerateResultExtensionsAttribute();
Console.WriteLine("Attribute created successfully!");

Console.WriteLine("Testing source generator...");

// Test basic functionality
var result = Result<string>.Ok("Hello, World!");
Console.WriteLine($"Result created: {result.IsSuccess}");

// This should use the generated extension method
try 
{
    var iresult = result.ToIResult();
    Console.WriteLine("ToIResult() extension method found and executed!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine("Source generator may not be working properly.");
}

Console.WriteLine("Done.");
