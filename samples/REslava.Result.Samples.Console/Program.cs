using System.ComponentModel;
using REslava.Result;
//using REslava.Result.Samples.Console.Examples;

Console.WriteLine("===========================================");
Console.WriteLine("REslava.Result - Sample Examples");
Console.WriteLine("===========================================\n");

// Run examples
//await RunExample("01. Basic Usage", BasicUsage.Run);
//await RunExample("02. Validation Pipeline", ValidationPipeline.Run);
//await RunExample("03. Error Handling", ErrorHandling.Run);
//await RunExample("04. Async Operations", AsyncOperations.Run);
//await RunExample("05. LINQ Syntax", LinqSyntax.Run);
//await RunExample("06. Custom Errors", CustomErrors.Run);
//await RunExample("07. Real World Scenarios", RealWorldScenarios.Run);

Console.WriteLine("\\n===========================================");
Console.WriteLine("All examples completed!");
Console.WriteLine("===========================================");

static async Task RunExample(string name, Func<Task> example)
{
    Console.WriteLine($"\\n--- {name} ---");
    try
    {
        await example();
        Console.WriteLine($"✓ {name} completed\\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ {name} failed: {ex.Message}\\n");
    }
}
