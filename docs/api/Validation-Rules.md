# Validation Rules API Reference

**🎯 Complete API documentation for the Validation Rules Framework**

The Validation Rules Engine provides a powerful, fluent way to define and execute validation rules that integrate seamlessly with the Result pattern.

## 📋 Table of Contents

- [Core Classes](#-core-classes)
- [ValidatorRulesBuilder](#validatorrulesbuilder)
- [IValidatorRule Interface](#ivalidatorrule-interface)
- [Built-in Rule Types](#-built-in-rule-types)
- [ValidationResult](#validationresult)
- [Extension Methods](#-extension-methods)
- [Examples](#-examples)

## 📐 Core Classes

### ValidationResult<T>

Represents the result of a validation operation.

```csharp
public class ValidationResult<T> : IResultResponse<T>
{
    public bool IsSuccess { get; }
    public bool IsFailed { get; }
    public bool IsValid { get; }
    public T? Value { get; }
    public IReadOnlyList<IReason> Reasons { get; }
    public IReadOnlyList<IError> ValidationErrors { get; }
    
    // Factory methods
    public static ValidationResult<T> Success(T value);
    public static ValidationResult<T> Failure(params IReason[] errors);
    public static ValidationResult<T> Failure(IEnumerable<IReason> errors);
}
```

**Properties:**
- `IsValid` - `true` if validation passed, `false` if failed
- `ValidationErrors` - Collection of validation errors (convenience property)
- Inherits all standard Result<T> properties and methods

## 🔧 ValidatorRuleBuilder<T>

The main entry point for building validation rules using a fluent API.

```csharp
public class ValidatorRuleBuilder<T>
{
    public ValidatorRuleBuilder<T> Rule<TProperty>(Expression<Func<T, TProperty>> propertySelector, string ruleName, string errorMessage, Func<TProperty, bool> validator);
    public ValidatorRuleBuilder<T> RuleAsync<TProperty>(Expression<Func<T, TProperty>> propertySelector, string ruleName, string errorMessage, Func<TProperty, Task<bool>> validator);
    public ValidatorRuleBuilder<T> AddRule(IValidatorRule<T> rule);
    public ValidatorRuleSet<T> Build();
}
```

### ValidatorRuleSetBuilder<T>

Fluent builder for creating validation rule sets.

```csharp
public class ValidatorRuleSetBuilder<T>
{
    // Basic validation rules
    public ValidatorRuleSetBuilder<T> Required(string? message = null);
    public ValidatorRuleSetBuilder<T> Must<TProperty>(Func<TProperty, bool> predicate, string message);
    public ValidatorRuleSetBuilder<T> Must<TProperty>(Func<TProperty, bool> predicate, Error error);
    
    // Async validation rules
    public ValidatorRuleSetBuilder<T> MustAsync<TProperty>(Func<TProperty, Task<bool>> predicate, string message);
    public ValidatorRuleSetBuilder<T> MustAsync<TProperty>(Func<TProperty, Task<bool>> predicate, Error error);
    
    // Conditional validation
    public ValidatorRuleSetBuilder<T> MustWhen<TProperty>(
        Func<T, bool> condition,
        Func<TProperty, bool> predicate,
        string message);
    
    public ValidatorRuleSetBuilder<T> MustWhenAsync<TProperty>(
        Func<T, bool> condition,
        Func<TProperty, Task<bool>> predicate,
        string message);
    
    // Custom rules
    public ValidatorRuleSetBuilder<T> AddRule<TProperty>(IValidatorRule<TProperty> rule);
    
    // Build validator
    public ValidatorRuleSet<T> Build();
}
```

## 🔌 IValidatorRule Interface

Base interface for all validation rules.

```csharp
public interface IValidatorRule<T>
{
    Task<IResultResponse<T>> ValidateAsync(T value);
}
```

## 🎯 Built-in Rule Types

### PredicateValidatorRule<T>

Rule that validates using a predicate function.

```csharp
public class PredicateValidatorRule<T> : IValidatorRule<T>
{
    // Synchronous predicate
    public static PredicateValidatorRule<T> Create(Func<T, bool> predicate, string message);
    public static PredicateValidatorRule<T> Create(Func<T, bool> predicate, Error error);
    
    // Asynchronous predicate
    public static PredicateValidatorRule<T> CreateAsync(Func<T, Task<bool>> predicate, string message);
    public static PredicateValidatorRule<T> CreateAsync(Func<T, Task<bool>> predicate, Error error);
}
```

### AsyncPredicateValidatorRule<T>

Specialized rule for async validation scenarios.

```csharp
public class AsyncPredicateValidatorRule<T> : IValidatorRule<T>
{
    public AsyncPredicateValidatorRule(Func<T, Task<bool>> validator, Error error);
    public Task<IResultResponse<T>> ValidateAsync(T value);
}
```

### RequiredValidatorRule<T>

Rule that validates that a value is not null or empty.

```csharp
public class RequiredValidatorRule<T> : IValidatorRule<T>
{
    public RequiredValidatorRule(string? message = null);
    public Task<IResultResponse<T>> ValidateAsync(T value);
}
```

## 📦 ValidatorRuleSet<T>

Container for a collection of validation rules.

```csharp
public class ValidatorRuleSet<T> : IValidatorRule<T>
{
    // Properties
    public IReadOnlyList<IValidatorRule<T>> Rules { get; }
    
    // Validation methods
    public Task<IResultResponse<T>> ValidateAsync(T value);
    public ValidationResult<T> Validate(T value);
    
    // Combination
    public ValidatorRuleSet<T> Combine(ValidatorRuleSet<T> other);
}
```

## 🔄 Extension Methods

### Validation Extensions for Result<T>

```csharp
public static class ValidationExtensions
{
    // Validate a value and return Result<T>
    public static Result<T> Validate<T>(this T value, ValidatorRuleSet<T> validator);
    
    // Async validation
    public static Task<Result<T>> ValidateAsync<T>(this T value, ValidatorRuleSet<T> validator);
    
    // Validate Result<T> content
    public static Result<T> Validate<T>(this Result<T> result, ValidatorRuleSet<T> validator);
    public static Task<Result<T>> ValidateAsync<T>(this Result<T> result, ValidatorRuleSet<T> validator);
}
```

### Ensure Extensions

```csharp
public static class ResultValidationExtensions
{
    // Ensure conditions on Result<T>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error error);
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, string errorMessage);
    
    // Async versions
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Error error);
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, string errorMessage);
    
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<bool>> predicate, Error error);
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<bool>> predicate, string errorMessage);
}
```

## 📚 Examples

### Basic Usage

```csharp
using REslava.Result;

// Create a simple validator
var emailValidator = new ValidatorRuleBuilder<string>()
    .Rule(email => email, "Required", "Email is required", email => !string.IsNullOrEmpty(email))
    .Rule(email => email, "Format", "Invalid email format", email => email.Contains("@"))
    .Build();

// Validate
var result = emailValidator.Validate("user@example.com");
if (result.IsSuccess)
{
    Console.WriteLine($"Valid email: {result.Value}");
}
else
{
    foreach (var error in result.ValidationErrors)
    {
        Console.WriteLine($"Error: {error.Message}");
    }
}
```

### Complex Object Validation

```csharp
public class User
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}

// Create comprehensive user validator
var userValidator = ValidatorRulesBuilder<User>
    .For(u => u.FirstName)
        .Required("First name is required")
        .Must(name => name.Length >= 2, "First name must be at least 2 characters")
    .For(u => u.LastName)
        .Required("Last name is required")
        .Must(name => name.Length >= 2, "Last name must be at least 2 characters")
    .For(u => u.Email)
        .Required("Email is required")
        .Must(e => e.Contains("@"), "Invalid email format")
        .MustAsync(async email => !await EmailExistsAsync(email), "Email already exists")
    .For(u => u.Age)
        .Must(age => age >= 18, "Must be 18 or older")
        .Must(age => age <= 120, "Invalid age")
    .Build();

// Validate user
var user = new User { FirstName = "John", LastName = "Doe", Email = "john@example.com", Age = 25 };
var validationResult = await userValidator.ValidateAsync(user);
```

### Custom Rules

```csharp
// Create reusable custom rules
var strongPasswordRule = PredicateValidatorRule<string>.Create(
    password => password.Length >= 8 
        && char.IsUpper(password[0]) 
        && char.IsDigit(password[^1]), 
    "Password must be 8+ chars, start with uppercase, end with digit");

var uniqueUsernameRule = PredicateValidatorRule<string>.CreateAsync(
    async username => !await UsernameExistsAsync(username),
    "Username already taken");

// Use custom rules
var userValidator = ValidatorRulesBuilder<User>
    .For(u => u.Password)
        .AddRule(strongPasswordRule)
    .For(u => u.Username)
        .AddRule(uniqueUsernameRule)
    .Build();
```

### Conditional Validation

```csharp
var orderValidator = ValidatorRulesBuilder<Order>
    .For(o => o.Total)
        .Must(total => total > 0, "Order total must be positive")
    .For(o => o.PaymentMethod)
        .Required("Payment method required")
    .For(o => o.CreditCard)
        .MustWhen(
            order => order.PaymentMethod == PaymentMethod.CreditCard,
            card => card != null && card.IsValid(),
            "Valid credit card required for credit card payments")
    .For(o => o.BillingAddress)
        .MustWhen(
            order => order.PaymentMethod != PaymentMethod.Cash,
            address => address != null,
            "Billing address required for non-cash payments")
    .Build();
```

### Combining Validators

```csharp
// Create specialized validators
var personalInfoValidator = ValidatorRulesBuilder<User>
    .For(u => u.FirstName).Required("First name required")
    .For(u => u.LastName).Required("Last name required")
    .For(u => u.Email).Must(e => e.Contains("@"), "Invalid email")
    .Build();

var securityValidator = ValidatorRulesBuilder<User>
    .For(u => u.Password).Must(p => p.Length >= 8, "Password too short")
    .For(u => u.SecurityQuestion).Required("Security question required")
    .Build();

// Combine validators
var completeValidator = personalInfoValidator.Combine(securityValidator);
var result = await completeValidator.ValidateAsync(user);
```

### Integration with Result Pattern

```csharp
public async Task<Result<User>> RegisterUserAsync(UserRegistrationDto dto)
{
    // Validate input using validation rules
    var validationResult = await userValidator.ValidateAsync(dto);
    if (validationResult.IsFailed)
        return Result<User>.Fail(validationResult.Errors);
    
    // Create user
    var user = new User(dto.Email, dto.Name);
    
    // Save to database
    var saveResult = await userRepository.SaveAsync(user);
    if (saveResult.IsFailed)
        return saveResult;
    
    return Result<User>.Ok(user);
}

// Using Ensure extension methods
public async Task<Result<User>> UpdateUserAsync(User user)
{
    return await Result<User>.Ok(user)
        .EnsureAsync(u => u.Email.Contains("@"), "Invalid email format")
        .EnsureAsync(async u => !await EmailExistsAsync(u.Email), "Email already exists")
        .TapAsync(async u => await userRepository.UpdateAsync(u));
}
```

## 🎯 Best Practices

1. **Use Specific Error Messages** - Provide clear, actionable feedback
2. **Create Reusable Rules** - Build libraries of common validation rules
3. **Use Async Wisely** - Only use async validation when needed (database/API calls)
4. **Test Rules Separately** - Unit test validation rules in isolation
5. **Document Complex Rules** - Add comments for business logic validation
6. **Combine Validators** - Build complex validation from simple, focused validators

## 🔄 Error Handling

Validation rules automatically handle exceptions and convert them to `ExceptionError`:

```csharp
var validator = ValidatorRulesBuilder<string>
    .For(s => s)
        .Must(value => {
            if (value == "throw")
                throw new InvalidOperationException("Test exception");
            return true;
        }, "This should not be reached")
    .Build();

var result = validator.Validate("throw");
// result.IsFailed will be true
// result.Errors[0] will be an ExceptionError
```

## 📝 Thread Safety

All validation rules are thread-safe and can be reused across multiple validation operations. Validators are immutable after creation.
