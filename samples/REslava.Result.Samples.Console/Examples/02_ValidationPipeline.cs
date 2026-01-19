using REslava.Result;

namespace REslava.Result.Samples.Console;

/// <summary>
/// Demonstrates validation pipelines using the Result pattern.
/// </summary>
public static class ValidationPipelineSamples
{
    public static async Task Run()
    {
        System.Console.WriteLine("=== Validation Pipeline Samples ===\n");

        SimpleValidation();
        ChainedValidation();
        MultipleValidations();
        CustomErrorMessages();
        NullValidation();
        ComplexValidationPipeline();
        ConditionalValidation();
        ValidationWithTransformation();

        System.Console.WriteLine("\n=== Validation Pipeline Complete ===\n");
    }

    #region Simple Validation

    private static void SimpleValidation()
    {
        System.Console.WriteLine("--- Simple Validation ---");

        // Valid case
        var result1 = Result<int>.Ok(25)
            .Ensure(age => age >= 18, "Must be 18 or older");

        System.Console.WriteLine($"Age 25 validation: {result1.IsSuccess}");

        // Invalid case
        var result2 = Result<int>.Ok(15)
            .Ensure(age => age >= 18, "Must be 18 or older");

        System.Console.WriteLine($"Age 15 validation: {result2.IsSuccess}");
        if (result2.IsFailed)
        {
            System.Console.WriteLine($"  Error: {result2.Errors[0].Message}");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Chained Validation

    private static void ChainedValidation()
    {
        System.Console.WriteLine("--- Chained Validation ---");

        var result = Result<string>.Ok("test@example.com")
            .EnsureNotNull("Email cannot be null")
            .Ensure(e => e.Contains("@"), "Email must contain @")
            .Ensure(e => e.Length >= 5, "Email too short")
            .Ensure(e => !e.StartsWith("@"), "Email cannot start with @");

        System.Console.WriteLine($"Email validation: {result.IsSuccess}");
        System.Console.WriteLine($"Email: {result.Value}");

        // Failed validation
        var invalid = Result<string>.Ok("bad")
            .EnsureNotNull("Email cannot be null")
            .Ensure(e => e.Contains("@"), "Email must contain @")
            .Ensure(e => e.Length >= 5, "Email too short");

        System.Console.WriteLine($"\nInvalid email: {invalid.IsSuccess}");
        if (invalid.IsFailed)
        {
            System.Console.WriteLine($"  First error: {invalid.Errors[0].Message}");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Multiple Validations

    private static void MultipleValidations()
    {
        System.Console.WriteLine("--- Multiple Validations ---");

        // All validations in one call - collects ALL failures
        var password = "weak";
        var result = Result<string>.Ok(password)
            .Ensure(
                (p => p.Length >= 8, new Error("Min 8 characters")),
                (p => p.Any(char.IsDigit), new Error("Requires digit")),
                (p => p.Any(char.IsUpper), new Error("Requires uppercase")),
                (p => p.Any(c => "!@#$%^&*".Contains(c)), new Error("Requires special character"))
            );

        System.Console.WriteLine($"Password validation: {result.IsSuccess}");
        if (result.IsFailed)
        {
            System.Console.WriteLine($"Errors found: {result.Errors.Count}");
            foreach (var error in result.Errors)
            {
                System.Console.WriteLine($"  ✗ {error.Message}");
            }
        }

        // Valid password
        var validPassword = "StrongP@ss123";
        var validResult = Result<string>.Ok(validPassword)
            .Ensure(
                (p => p.Length >= 8, new Error("Min 8 characters")),
                (p => p.Any(char.IsDigit), new Error("Requires digit")),
                (p => p.Any(char.IsUpper), new Error("Requires uppercase")),
                (p => p.Any(c => "!@#$%^&*".Contains(c)), new Error("Requires special character"))
            );

        System.Console.WriteLine($"\nStrong password validation: {validResult.IsSuccess}");

        System.Console.WriteLine();
    }

    #endregion

    #region Custom Error Messages

    private static void CustomErrorMessages()
    {
        System.Console.WriteLine("--- Custom Error Messages ---");

        var age = 15;
        var error = new Error($"Age {age} is below minimum requirement")
            .WithTag("Field", "Age")
            .WithTag("MinimumAge", 18)
            .WithTag("ProvidedAge", age)
            .WithTag("ErrorCode", "AGE_TOO_LOW");

        var result = Result<int>.Ok(age)
            .Ensure(a => a >= 18, error);

        if (result.IsFailed)
        {
            System.Console.WriteLine($"Error: {result.Errors[0].Message}");
            System.Console.WriteLine($"  Field: {result.Errors[0].Tags["Field"]}");
            System.Console.WriteLine($"  Code: {result.Errors[0].Tags["ErrorCode"]}");
            System.Console.WriteLine($"  Minimum: {result.Errors[0].Tags["MinimumAge"]}");
            System.Console.WriteLine($"  Provided: {result.Errors[0].Tags["ProvidedAge"]}");
        }

        System.Console.WriteLine();
    }

    #endregion

    #region Null Validation

    private static void NullValidation()
    {
        System.Console.WriteLine("--- Null Validation ---");

        // Valid case
        var result1 = Result<string>.Ok("Hello")
            .EnsureNotNull("Value cannot be null");

        System.Console.WriteLine($"Non-null validation: {result1.IsSuccess}");

        // Null case
        var result2 = Result<string>.Ok(null!)
            .EnsureNotNull("Value cannot be null");

        System.Console.WriteLine($"Null validation: {result2.IsSuccess}");
        if (result2.IsFailed)
        {
            System.Console.WriteLine($"  Error: {result2.Errors[0].Message}");
        }

        // Multiple validations with null check first
        var email = "test@example.com";
        var result3 = Result<string>.Ok(email)
            .EnsureNotNull("Email is required")
            .Ensure(e => e.Contains("@"), "Invalid email format")
            .Ensure(e => e.Length <= 100, "Email too long");

        System.Console.WriteLine($"\nEmail with null check: {result3.IsSuccess}");

        System.Console.WriteLine();
    }

    #endregion

    #region Complex Validation Pipeline

    private static void ComplexValidationPipeline()
    {
        System.Console.WriteLine("--- Complex Validation Pipeline ---");

        var user = new User
        {
            Name = "Alice",
            Email = "alice@example.com",
            Age = 25,
            Username = "alice123"
        };

        var result = ValidateUser(user);

        System.Console.WriteLine($"User validation: {result.IsSuccess}");
        if (result.IsSuccess)
        {
            System.Console.WriteLine($"  User: {result.Value.Name} ({result.Value.Email})");
        }

        // Invalid user
        var invalidUser = new User
        {
            Name = "B",
            Email = "invalid",
            Age = 15,
            Username = "ab"
        };

        var invalidResult = ValidateUser(invalidUser);
        System.Console.WriteLine($"\nInvalid user validation: {invalidResult.IsSuccess}");
        if (invalidResult.IsFailed)
        {
            System.Console.WriteLine($"Errors: {invalidResult.Errors.Count}");
            foreach (var error in invalidResult.Errors)
            {
                System.Console.WriteLine($"  ✗ {error.Message}");
            }
        }

        System.Console.WriteLine();
    }

    private static Result<User> ValidateUser(User user)
    {
        return Result<User>.Ok(user)
            .EnsureNotNull("User cannot be null")
            .Ensure(
                (u => !string.IsNullOrWhiteSpace(u.Name), new Error("Name is required")),
                (u => u.Name.Length >= 2, new Error("Name too short")),
                (u => u.Name.Length <= 50, new Error("Name too long")),
                (u => !string.IsNullOrWhiteSpace(u.Email), new Error("Email is required")),
                (u => u.Email.Contains("@"), new Error("Invalid email format")),
                (u => u.Age >= 18, new Error("Must be 18 or older")),
                (u => u.Age <= 120, new Error("Invalid age")),
                (u => !string.IsNullOrWhiteSpace(u.Username), new Error("Username is required")),
                (u => u.Username.Length >= 3, new Error("Username too short")),
                (u => u.Username.Length <= 20, new Error("Username too long"))
            );
    }

    #endregion

    #region Conditional Validation

    private static void ConditionalValidation()
    {
        System.Console.WriteLine("--- Conditional Validation ---");

        // Using OkIf
        var age1 = 25;
        var result1 = Result<int>.OkIf(
            age1 >= 18,
            age1,
            "Must be 18 or older"
        );

        System.Console.WriteLine($"OkIf (age 25): {result1.IsSuccess}");

        var age2 = 15;
        var result2 = Result<int>.OkIf(
            age2 >= 18,
            age2,
            "Must be 18 or older"
        );

        System.Console.WriteLine($"OkIf (age 15): {result2.IsSuccess}");
        if (result2.IsFailed)
        {
            System.Console.WriteLine($"  Error: {result2.Errors[0].Message}");
        }

        // Using FailIf
        var temperature = 45;
        var result3 = Result<int>.FailIf(
            temperature > 40,
            "Temperature too high",
            temperature
        );

        System.Console.WriteLine($"\nFailIf (temp 45): {result3.IsSuccess}");
        if (result3.IsFailed)
        {
            System.Console.WriteLine($"  Error: {result3.Errors[0].Message}");
        }

        var result4 = Result<int>.FailIf(
            temperature < 0,
            "Temperature below freezing",
            temperature
        );

        System.Console.WriteLine($"FailIf (temp 45, check freezing): {result4.IsSuccess}");

        System.Console.WriteLine();
    }

    #endregion

    #region Validation with Transformation

    private static void ValidationWithTransformation()
    {
        System.Console.WriteLine("--- Validation with Transformation ---");

        var email = "  TEST@EXAMPLE.COM  ";

        var result = Result<string>.Ok(email)
            .EnsureNotNull("Email is required")
            .Map(e => e.Trim())
            .Map(e => e.ToLower())
            .Ensure(e => e.Contains("@"), "Invalid email format")
            .Ensure(e => e.Length >= 5, "Email too short")
            .Ensure(e => !e.StartsWith("@"), "Invalid email start");

        System.Console.WriteLine($"Email transformation and validation: {result.IsSuccess}");
        if (result.IsSuccess)
        {
            System.Console.WriteLine($"  Original: '{email}'");
            System.Console.WriteLine($"  Transformed: '{result.Value}'");
        }

        // Validation with Bind
        var userId = 123;
        var userResult = Result<int>.Ok(userId)
            .Ensure(id => id > 0, "Invalid user ID")
            .Bind(id => GetUser(id))
            .Ensure(u => u.IsActive, "User is not active")
            .Ensure(u => !u.IsLocked, "User account is locked");

        System.Console.WriteLine($"\nUser validation with Bind: {userResult.IsSuccess}");
        if (userResult.IsSuccess)
        {
            System.Console.WriteLine($"  User: {userResult.Value.Name}");
        }

        System.Console.WriteLine();
    }

    private static Result<UserAccount> GetUser(int id)
    {
        // Simulate user lookup
        return Result<UserAccount>.Ok(new UserAccount
        {
            Id = id,
            Name = "Alice",
            IsActive = true,
            IsLocked = false
        });
    }

    #endregion

    #region Helper Classes

    private class User
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public int Age { get; set; }
        public string Username { get; set; } = "";
    }

    private class UserAccount
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
    }

    #endregion
}
