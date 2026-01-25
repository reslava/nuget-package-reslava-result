using REslava.Result;
using System;

namespace REslava.Result.Samples.Console;

/// <summary>
/// Demonstrates the Validation Rules framework for declarative, composable validation.
/// </summary>
public static class ValidationRulesSamples
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== Validation Rules Samples ===\n");

        await BasicValidationExample();
    }

    /// <summary>
    /// Basic validation with simple rules
    /// </summary>
    private static async Task BasicValidationExample()
    {
        System.Console.WriteLine("📋 Basic Validation Example");
        System.Console.WriteLine("----------------------------");

        // Create a simple email validator
        var emailValidator = new ValidatorRuleBuilder<string>()
            .Rule(email => email, "Required", "Email is required", email => !string.IsNullOrEmpty(email))
            .Rule(email => email, "Format", "Invalid email format", email => email.Contains("@"))
            .Build();

        // Test valid email
        var validResult = emailValidator.Validate("user@example.com");
        System.Console.WriteLine($"Valid email: {validResult.IsSuccess}");

        // Test invalid email
        var invalidResult = emailValidator.Validate("invalid-email");
        System.Console.WriteLine($"Invalid email: {invalidResult.IsSuccess}");
        if (invalidResult.IsFailed)
        {
            foreach (var error in invalidResult.ValidationErrors)
                System.Console.WriteLine($"  Error: {error.Message}");
        }

        System.Console.WriteLine();
    }
}
