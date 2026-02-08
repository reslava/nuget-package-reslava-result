# REslava.Result Console Samples

Comprehensive console application demonstrating all features of the REslava.Result library through 13 progressive examples.

## What This Demonstrates

This sample app provides hands-on examples of every REslava.Result feature, from basic patterns to advanced functional programming concepts.

### Learning Path

**Level 1: Fundamentals**
- `01_BasicUsage.cs` - Core Result<T> patterns, success/failure handling
- `02_ValidationPipeline.cs` - Chained validation with Ensure
- `03_ErrorHandling.cs` - Rich error context and metadata

**Level 2: Intermediate**
- `04_AsyncOperations.cs` - Async/await patterns with TryAsync
- `05_LINQSyntax.cs` - Query syntax (from-where-select)
- `06_CustomErrors.cs` - Custom error types and fluent builders
- `07_RealWorldScenarios.cs` - Practical business logic examples
- `08_ValidationRules.cs` - Rule-based validation patterns

**Level 3: Advanced**
- `09_AdvancedPatterns_Maybe.cs` - Maybe&lt;T&gt; functional pattern
- `10_AdvancedPatterns_OneOf.cs` - OneOf&lt;T1, T2&gt; discriminated unions
- `11_AdvancedPatterns_OneOf3.cs` - OneOf&lt;T1, T2, T3&gt; tri-way unions
- `12_Result_OneOf_Conversions.cs` - Converting between Result and OneOf
- `13_OneOf_Result_Integration.cs` - Mixed pipeline scenarios

## Running the Examples

```bash
dotnet run
```

All 13 examples will execute sequentially, demonstrating each pattern with clear output.

## Key Features Demonstrated

- ✅ Result&lt;T&gt; success/failure patterns
- ✅ Implicit conversions (value→Result, Error→Result)
- ✅ Pattern matching with Match()
- ✅ Safe value access (GetValueOr, TryGetValue)
- ✅ Validation pipelines (Ensure, EnsureNotNull)
- ✅ Async operations (TryAsync, BindAsync)
- ✅ LINQ query syntax support
- ✅ Custom error types with metadata
- ✅ Maybe&lt;T&gt; for optional values
- ✅ OneOf&lt;T1,T2,T3,T4&gt; discriminated unions
- ✅ Result↔OneOf conversions

## Complementary Resources

For practical web API usage, see:
- **FastMinimalAPI.REslava.Result.Demo** - Real-world ASP.NET Core integration

## Requirements

- .NET 10.0 or later
- REslava.Result v1.12.0+

## License

MIT License
