using System;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Samples.Console.Examples;

public static class AdvancedPatterns_OneOf
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== OneOf<T1, T2> Advanced Patterns ===\n");

        BasicOneOfOperations();
        ErrorHandlingWithTypedErrors();

        System.Console.WriteLine("\n=== OneOf<T1, T2> Patterns Complete ===");
        await Task.CompletedTask;
    }

    private static void BasicOneOfOperations()
    {
        System.Console.WriteLine("1. Basic OneOf Operations:");
        System.Console.WriteLine("---------------------------");

        OneOf<string, int> textOrNumber = "Hello World";
        OneOf<string, int> numberOrText = 42;

        System.Console.WriteLine($"Text or number: {textOrNumber}");
        System.Console.WriteLine($"Number or text: {numberOrText}");

        var result1 = textOrNumber.Match<string>(
            case1: text => "Text: " + text.ToUpper(),
            case2: number => "Number: " + (number * 2)
        );

        System.Console.WriteLine($"Result: {result1}");

        textOrNumber.Switch(
            case1: text => System.Console.WriteLine($"Processing text: {text}"),
            case2: number => System.Console.WriteLine($"Processing number: {number}")
        );

        System.Console.WriteLine();
    }

    private static void ErrorHandlingWithTypedErrors()
    {
        System.Console.WriteLine("2. Error Handling with Typed Errors:");
        System.Console.WriteLine("--------------------------------------");

        var userResult = GetUserById(1);
        var invalidResult = GetUserById(999);

        ProcessUserResult(userResult);
        ProcessUserResult(invalidResult);

        System.Console.WriteLine();
    }

    private static void ProcessUserResult(OneOf<ApiError, User> result)
    {
        result.Match<string>(
            case1: error => { System.Console.WriteLine($"❌ User error: {error.Message} (Code: {error.Code})"); return ""; },
            case2: user => { System.Console.WriteLine($"✅ User found: {user.Name} ({user.Email})"); return ""; }
        );
    }

    private static OneOf<ApiError, User> GetUserById(int id)
    {
        return id switch
        {
            1 => new User("Alice Johnson", "alice@example.com"),
            _ => new ApiError("NotFound", $"User with ID {id} not found", 404)
        };
    }

    public record User(string Name, string Email);
    public record ApiError(string Type, string Message, int Code);
}