using System;
using System.Linq;
using REslava.Result.AdvancedPatterns;

namespace REslava.Result.Samples.Console.Examples;

/// <summary>
/// Demonstrates Maybe<T> functional programming patterns
/// Alternative to null references with type-safe optional values
/// </summary>
public static class AdvancedPatterns_Maybe
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== Maybe<T> Advanced Patterns ===\n");

        // 1. Basic Maybe Operations
        BasicMaybeOperations();

        // 2. Functional Chaining
        FunctionalChaining();

        // 3. Real-World User Scenario
        RealWorldUserScenario();

        System.Console.WriteLine("\n=== Maybe<T> Patterns Complete ===");
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Demonstrates basic Maybe<T> operations
    /// </summary>
    private static void BasicMaybeOperations()
    {
        System.Console.WriteLine("1. Basic Maybe Operations:");
        System.Console.WriteLine("------------------------");

        // Creating Maybe values
        Maybe<string> someValue = Maybe<string>.Some("Hello World");
        Maybe<string> noneValue = Maybe<string>.None;

        // Implicit conversion
        Maybe<int> number = 42;
        Maybe<string> text = "Functional Programming";

        System.Console.WriteLine($"Some value: {someValue}");
        System.Console.WriteLine($"None value: {noneValue}");
        System.Console.WriteLine($"Number: {number}");
        System.Console.WriteLine($"Text: {text}");

        // Safe value extraction
        System.Console.WriteLine($"Safe extraction: {someValue.ValueOrDefault("Default")}");
        System.Console.WriteLine($"None extraction: {noneValue.ValueOrDefault("Default")}");

        System.Console.WriteLine();
    }

    /// <summary>
    /// Shows functional chaining with Maybe<T>
    /// </summary>
    private static void FunctionalChaining()
    {
        System.Console.WriteLine("2. Functional Chaining:");
        System.Console.WriteLine("----------------------");

        var result = Maybe<string>.Some("  hello world  ")
            .Map(s => s.Trim())
            .Map(s => s.ToUpper())
            .Filter(s => s.Length > 5)
            .Map(s => $"{s}!")
            .ValueOrDefault("Default Result");

        System.Console.WriteLine($"Chained result: {result}");

        var failedResult = Maybe<string>.Some("hi")
            .Map(s => s.Trim())
            .Filter(s => s.Length > 5) // This will fail
            .Map(s => s.ToUpper())
            .ValueOrDefault("Too Short");

        System.Console.WriteLine($"Failed chain: {failedResult}");
        System.Console.WriteLine();
    }

    /// <summary>
    /// Real-world user lookup scenario
    /// </summary>
    private static void RealWorldUserScenario()
    {
        System.Console.WriteLine("3. Real-World User Scenario:");
        System.Console.WriteLine("---------------------------");

        // Simulate user database
        var users = new[]
        {
            new User(1, "Alice", "alice@example.com", 28),
            new User(2, "Bob", "bob@example.com", 35),
            new User(3, "Charlie", "charlie@example.com", 42)
        };

        // Find user and get formatted email
        var userEmail = FindUserById(users, 2)
            .Map(user => user.Email)
            .Map(email => email.ToUpper())
            .ValueOrDefault("User not found");

        System.Console.WriteLine($"User email: {userEmail}");

        // Try to find non-existent user
        var nonExistentUser = FindUserById(users, 99)
            .Map(user => user.Name)
            .ValueOrDefault("User not found");

        System.Console.WriteLine($"Non-existent user: {nonExistentUser}");

        // Get user age category
        var ageCategory = FindUserById(users, 1)
            .Filter(user => user.Age >= 18)
            .Map(user => user.Age >= 65 ? "Senior" : user.Age >= 30 ? "Adult" : "Young Adult")
            .ValueOrDefault("Invalid User");

        System.Console.WriteLine($"Age category: {ageCategory}");
        System.Console.WriteLine();
    }

    // Helper methods for demonstration
    private static Maybe<User> FindUserById(User[] users, int id)
    {
        var user = users.FirstOrDefault(u => u.Id == id);
        return user != null ? Maybe<User>.Some(user) : Maybe<User>.None;
    }

    // Supporting classes
    private record User(int Id, string Name, string Email, int Age);
}
