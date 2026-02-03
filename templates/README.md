# REslava.Result Project Templates

## 🎯 Purpose
These templates help developers start new projects with REslava.Result correctly, avoiding common mistakes and ensuring proper setup.

## 📁 Available Templates

### 1. Minimal API with REslava.Result
- **Path**: `templates/minimal-api-re-slava-result/`
- **Description**: Minimal API project with REslava.Result and OneOf support
- **Features**: T1,T2 OneOf, proper error handling, source generators

### 2. Web API with REslava.Result
- **Path**: `templates/webapi-re-slava-result/`
- **Description**: Full Web API project with controllers and comprehensive error handling
- **Features**: T1,T2 OneOf, advanced patterns, validation

### 3. Console App with REslava.Result
- **Path**: `templates/console-re-slava-result/`
- **Description**: Console application demonstrating Result pattern usage
- **Features**: Result pattern, error handling, functional programming

## 🚀 How to Use Templates

### Using .NET CLI
```bash
# Install template
dotnet new install REslava.Result.Templates

# Create new project
dotnet new re-slava-minimal-api -n MyAwesomeApi
dotnet new re-slava-webapi -n MyWebApi
dotnet new re-slava-console -n MyConsoleApp
```

### Using Visual Studio
1. Install template package: `REslava.Result.Templates`
2. Create new project
3. Search for "REslava.Result" templates
4. Choose desired template

## ✅ What Templates Include

### Proper Project Setup
- Correct package references
- Source generator configuration
- Compiler settings for generated files
- Proper namespace organization

### Working Examples
- Pre-configured endpoints
- Error handling patterns
- OneOf usage examples
- Best practices implementation

### Testing Infrastructure
- Unit test projects
- Integration test setup
- Sample test cases
- Testing utilities

## 🎯 Benefits

### ✅ Avoid Common Mistakes
- Correct package versions
- Proper source generator setup
- Right namespace usage
- Correct OneOf patterns

### ✅ Start Fast
- Working code from day one
- Best practices built-in
- Comprehensive examples
- Ready-to-use patterns

### ✅ Learn Properly
- Idiomatic REslava.Result usage
- Functional programming patterns
- Error handling best practices
- Performance considerations

## 📝 Template Features

### Minimal API Template
```csharp
// Proper OneOf usage with source generators
app.MapGet("/api/users/{id}", (int id) => {
    OneOf<UserNotFoundError, User> result = GetUser(id);
    return result.ToIResult(); // Generated extension method
});
```

### Web API Template
```csharp
[ApiController]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public IResult GetUser(int id)
    {
        OneOf<UserNotFoundError, User> result = _userService.GetUser(id);
        return result.ToIResult(); // Generated extension method
    }
}
```

### Console Template
```csharp
// Result pattern usage
Result<User> result = UserValidator.Validate(request);
return result.Match(
    user => ProcessUser(user),
    error => HandleError(error)
);
```

## 🔧 Customization

Templates are designed to be:
- **Easy to modify** - Clear structure and comments
- **Extensible** - Add your own patterns
- **Production-ready** - Includes logging, validation, error handling
- **Well-documented** - Comprehensive comments and examples

## 📚 Additional Resources

- [REslava.Result Documentation](../README.md)
- [OneOf Usage Guide](../docs/OneOf-Usage.md)
- [Error Handling Patterns](../docs/Error-Handling.md)
- [Best Practices](../docs/Best-Practices.md)
