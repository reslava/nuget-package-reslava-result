using System;
using System.Linq;
using System.Reflection;

namespace GeneratorTestConsole;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Testing Generator Assembly ===");
        
        try
        {
            // Load the generator assembly
            var assemblyPath = @"j:\src\nuget-package-reslava-result\SourceGenerator\bin\Debug\netstandard2.0\REslava.Result.SourceGenerators.dll";
            var assembly = Assembly.LoadFrom(assemblyPath);
            
            Console.WriteLine($"Loaded assembly: {assembly.FullName}");
            Console.WriteLine($"Location: {assembly.Location}");
            
            // Find all types with [Generator] attribute
            var generatorTypes = assembly.GetTypes()
                .Where(t => t.GetCustomAttributes().Any(a => a.GetType().Name.Contains("Generator")))
                .ToList();
            
            Console.WriteLine($"\nFound {generatorTypes.Count} generator types:");
            foreach (var type in generatorTypes)
            {
                Console.WriteLine($"  - {type.FullName}");
                
                // Check if it implements IIncrementalGenerator
                var interfaces = type.GetInterfaces();
                var hasIncrementalInterface = interfaces.Any(i => i.Name.Contains("IIncrementalGenerator"));
                Console.WriteLine($"    Implements IIncrementalGenerator: {hasIncrementalInterface}");
            }
            
            // Also check for TestGenerator specifically
            var testGeneratorType = assembly.GetType("REslava.Result.SourceGenerators.TestGenerator");
            if (testGeneratorType != null)
            {
                Console.WriteLine($"\n✅ TestGenerator found: {testGeneratorType.FullName}");
            }
            else
            {
                Console.WriteLine("\n❌ TestGenerator NOT found!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
