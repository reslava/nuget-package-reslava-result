using System;
using REslava.Result.AdvancedPatterns;
using static REslava.Result.AdvancedPatterns.OneOfExtensions;

namespace REslava.Result.Samples.Console.Examples;

/// <summary>
/// Demonstrates OneOf&lt;T1, T2, T3&gt; three-way discriminated union patterns.
/// Type-safe alternative for handling three distinct states or value types.
/// </summary>
public static class AdvancedPatterns_OneOf3
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== OneOf<T1, T2, T3> Advanced Patterns ===\n");

        // 1. Basic OneOf3 Operations
        BasicOneOf3Operations();

        // 2. API Response with Three States
        ApiResponseWithThreeStates();

        // 3. Configuration Parsing
        ConfigurationParsing();

        // 4. Database Operations
        DatabaseOperations();

        // 5. Conversion Between 2-way and 3-way
        ConversionExamples();

        System.Console.WriteLine("\n=== OneOf<T1, T2, T3> Patterns Complete ===");
        await Task.CompletedTask;
    }

    private static void BasicOneOf3Operations()
    {
        System.Console.WriteLine("1. Basic OneOf3 Operations:");
        System.Console.WriteLine("----------------------------");

        // Creating OneOf3 values
        OneOf<string, int, bool> textOrNumberOrBool = "Hello World";
        OneOf<string, int, bool> numberOrTextOrBool = 42;
        OneOf<string, int, bool> boolOrTextOrNumber = true;

        System.Console.WriteLine($"Text/Number/Bool: {textOrNumberOrBool}");
        System.Console.WriteLine($"Number/Text/Bool: {numberOrTextOrBool}");
        System.Console.WriteLine($"Bool/Text/Number: {boolOrTextOrNumber}");

        // Pattern matching with three cases
        var result1 = textOrNumberOrBool.Match<string>(
            case1: text => $"Text: {text.ToUpper()}",
            case2: number => $"Number: {number * 2}",
            case3: flag => $"Boolean: {flag}"
        );

        var result2 = numberOrTextOrBool.Match<string>(
            case1: text => $"Text: {text.ToUpper()}",
            case2: number => $"Number: {number * 2}",
            case3: flag => $"Boolean: {flag}"
        );

        var result3 = boolOrTextOrNumber.Match<string>(
            case1: text => $"Text: {text.ToUpper()}",
            case2: number => $"Number: {number * 2}",
            case3: flag => $"Boolean: {flag}"
        );

        System.Console.WriteLine($"Result 1: {result1}");
        System.Console.WriteLine($"Result 2: {result2}");
        System.Console.WriteLine($"Result 3: {result3}");

        // Side effects with Switch
        textOrNumberOrBool.Switch(
            case1: text => System.Console.WriteLine($"Processing text: {text}"),
            case2: number => System.Console.WriteLine($"Processing number: {number}"),
            case3: flag => System.Console.WriteLine($"Processing boolean: {flag}")
        );

        System.Console.WriteLine();
    }

    private static void ApiResponseWithThreeStates()
    {
        System.Console.WriteLine("2. API Response with Three States:");
        System.Console.WriteLine("-----------------------------------");

        // Simulate different API responses
        var successResponse = CallApi("/users/1");
        var clientErrorResponse = CallApi("/users/999");
        var serverErrorResponse = CallApi("/internal-error");

        ProcessApiResponse(successResponse);
        ProcessApiResponse(clientErrorResponse);
        ProcessApiResponse(serverErrorResponse);

        System.Console.WriteLine();
    }

    private static void ConfigurationParsing()
    {
        System.Console.WriteLine("3. Configuration Parsing:");
        System.Console.WriteLine("-------------------------");

        // Parse different configuration values
        var timeoutConfig = ParseConfigValue("timeout", "30");
        var enabledConfig = ParseConfigValue("enabled", "true");
        var debugConfig = ParseConfigValue("debug", "false");
        var invalidConfig = ParseConfigValue("invalid", "maybe");

        ProcessConfigValue("timeout", timeoutConfig);
        ProcessConfigValue("enabled", enabledConfig);
        ProcessConfigValue("debug", debugConfig);
        ProcessConfigValue("invalid", invalidConfig);

        System.Console.WriteLine();
    }

    private static void DatabaseOperations()
    {
        System.Console.WriteLine("4. Database Operations:");
        System.Console.WriteLine("----------------------");

        // Database operations with three outcomes
        var createdRecord = SaveRecord(new Record("New Record"));
        var updatedRecord = SaveRecord(new Record("Existing Record", 1));
        var deletedRecord = SaveRecord(new Record("To Be Deleted", 2, true));

        ProcessDatabaseResult(createdRecord);
        ProcessDatabaseResult(updatedRecord);
        ProcessDatabaseResult(deletedRecord);

        System.Console.WriteLine();
    }

    private static void ConversionExamples()
    {
        System.Console.WriteLine("5. Conversion Between 2-way and 3-way:");
        System.Console.WriteLine("-------------------------------------");

        // Convert 2-way to 3-way
        var twoWaySuccess = OneOf<Error, Success>.FromT2(new Success("Data processed"));
        var threeWay = OneOfExtensions.ToThreeWay(twoWaySuccess, new ServerError("No server errors", 200));
        
        System.Console.WriteLine($"2-way to 3-way: {threeWay}");

        // Convert 3-way to 2-way with mapping
        var threeWayResponse = OneOf<Success, ClientError, ServerError>.FromT3(new ServerError("Database timeout", 500));
        var twoWayMapped = threeWayResponse.ToTwoWay(
            t3ToT1: serverError => new Success($"Server error handled: {serverError.Message}")
        );
        
        System.Console.WriteLine($"3-way to 2-way (mapped): {twoWayMapped}");

        // Convert 3-way to 2-way with fallback
        var threeWayWarning = OneOf<Success, ClientError, ServerError>.FromT2(new ClientError("Invalid request", 400));
        var twoWayFallback = threeWayWarning.ToTwoWayWithFallback(
            fallbackT1: new Success("Default success")
        );
        
        System.Console.WriteLine($"3-way to 2-way (fallback): {twoWayFallback}");

        System.Console.WriteLine();
    }

    // Helper methods

    private static void ProcessApiResponse(OneOf<Success, ClientError, ServerError> response)
    {
        response.Match<string>(
            case1: success => { System.Console.WriteLine($"✅ Success: {success.Message}"); return ""; },
            case2: clientError => { System.Console.WriteLine($"⚠️ Client Error: {clientError.Message} (Code: {clientError.Code})"); return ""; },
            case3: serverError => { System.Console.WriteLine($"❌ Server Error: {serverError.Message} (Code: {serverError.Code})"); return ""; }
        );
    }

    private static void ProcessConfigValue(string key, OneOf<ConfigError, int, bool> config)
    {
        config.Match<string>(
            case1: error => { System.Console.WriteLine($"❌ Config '{key}' error: {error.Message}"); return ""; },
            case2: number => { System.Console.WriteLine($"✅ Config '{key}': {number} (integer)"); return ""; },
            case3: flag => { System.Console.WriteLine($"✅ Config '{key}': {flag} (boolean)"); return ""; }
        );
    }

    private static void ProcessDatabaseResult(OneOf<Created, Updated, Deleted> result)
    {
        result.Match<string>(
            case1: created => { System.Console.WriteLine($"📝 Created: {created.Record.Name} (ID: {created.Record.Id})"); return ""; },
            case2: updated => { System.Console.WriteLine($"✏️ Updated: {updated.Record.Name} (ID: {updated.Record.Id})"); return ""; },
            case3: deleted => { System.Console.WriteLine($"🗑️ Deleted: {deleted.Record.Name} (ID: {deleted.Record.Id})"); return ""; }
        );
    }

    // Simulation methods

    private static OneOf<Success, ClientError, ServerError> CallApi(string endpoint)
    {
        return endpoint switch
        {
            "/users/1" => new Success("User retrieved successfully"),
            "/users/999" => new ClientError("User not found", 404),
            "/internal-error" => new ServerError("Internal server error", 500),
            _ => new ServerError("Unknown endpoint", 404)
        };
    }

    private static OneOf<ConfigError, int, bool> ParseConfigValue(string key, string value)
    {
        return key.ToLowerInvariant() switch
        {
            "timeout" when int.TryParse(value, out var timeout) => timeout,
            "enabled" when bool.TryParse(value, out var enabled) => enabled,
            "debug" when bool.TryParse(value, out var debug) => debug,
            "timeout" => new ConfigError("InvalidTimeout", $"Cannot parse '{value}' as integer"),
            "enabled" => new ConfigError("InvalidBoolean", $"Cannot parse '{value}' as boolean"),
            "debug" => new ConfigError("InvalidBoolean", $"Cannot parse '{value}' as boolean"),
            _ => new ConfigError("UnknownKey", $"Unknown configuration key: {key}")
        };
    }

    private static OneOf<Created, Updated, Deleted> SaveRecord(Record record)
    {
        return record.Id switch
        {
            0 => new Created(record with { Id = 42 }),
            1 => new Updated(record),
            2 => new Deleted(record),
            _ => new Created(record with { Id = 999 })
        };
    }

    // Supporting classes

    public record Success(string Message);
    public record ClientError(string Message, int Code);
    public record ServerError(string Message, int Code);
    public record ConfigError(string Type, string Message);
    public record Record(string Name, int Id = 0, bool ShouldDelete = false);
    public record Created(Record Record);
    public record Updated(Record Record);
    public record Deleted(Record Record);
}
