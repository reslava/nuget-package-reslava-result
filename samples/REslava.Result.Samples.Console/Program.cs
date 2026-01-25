using System.ComponentModel;
using REslava.Result;
using REslava.Result.AdvancedPatterns;
using REslava.Result.Samples.Console;
using REslava.Result.Samples.Console.Examples;

Console.WriteLine("===========================================");
Console.WriteLine("REslava.Result - Sample Examples");
Console.WriteLine("===========================================\n");

// Run examples
await RunExample("01. Basic Usage", BasicUsageSamples.Run);
await RunExample("02. Validation Pipeline", ValidationPipelineSamples.Run);
await RunExample("03. Error Handling", ErrorHandlingSamples.Run);
await RunExample("04. Async Operations", AsyncOperationsSamples.Run);
await RunExample("05. LINQ Syntax", LINQSyntaxSamples.Run);
await RunExample("06. Custom Errors", CustomErrorsSamples.Run);
await RunExample("07. Real World Scenarios", RealWorldScenariosSamples.Run);
await RunExample("08. Validation Rules", ValidationRulesSamples.Run);
await RunExample("09. Advanced Patterns - Maybe<T>", AdvancedPatterns_Maybe.Run);
await RunExample("10. Advanced Patterns - OneOf<T1, T2>", AdvancedPatterns_OneOf.Run);
await RunExample("11. Advanced Patterns - OneOf<T1, T2, T3>", AdvancedPatterns_OneOf3.Run);
await RunExample("12. Result ↔ OneOf Conversions", Result_OneOf_Conversions.Run);

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
