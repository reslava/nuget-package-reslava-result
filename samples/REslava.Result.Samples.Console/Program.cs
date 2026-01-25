using System.ComponentModel;
using REslava.Result;

using REslava.Result.Samples.Console;

Console.WriteLine("===========================================");
Console.WriteLine("REslava.Result - Sample Examples");
Console.WriteLine("===========================================\n");

// Run examples
await RunExample("01. Basic Usage", BasicUsageSamples.Run);
await RunExample("02. Validation Pipeline", ValidationPipelineSamples.Run);
//await RunExample("03. Error Handling", ErrorHandlingSamples.Run);
//await RunExample("04. Async Operations", AsyncOperationsSamples.Run);
//await RunExample("05. LINQ Syntax", LinqSyntaxSamples.Run);
//await RunExample("06. Custom Errors", CustomErrorsSamples.Run);
//await RunExample("07. Real World Scenarios", RealWorldScenariosSamples.Run);
await RunExample("08. Validation Rules", ValidationRulesSamples.Run);

Console.WriteLine("\\n===========================================");
Console.WriteLine("All examples completed!");
Console.WriteLine("===========================================");

static async Task RunExample(string name, Func<Task> example)
{
    Console.WriteLine($"\n--- {name} ---");
    try
    {
        await example();
        Console.WriteLine($"✓ {name} completed\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ {name} failed: {ex.Message}\n");
    }
}
