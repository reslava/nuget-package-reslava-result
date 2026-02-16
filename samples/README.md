# 📚 REslava.Result Samples

**Working examples demonstrating REslava.Result, Source Generators, and Analyzers.**

---

## 📂 Sample Projects

| Project | Framework | Description |
|---------|-----------|-------------|
| **[FastMinimalAPI.REslava.Result.Demo](FastMinimalAPI.REslava.Result.Demo/)** | .NET 10 Minimal API | Full-featured demo with SmartEndpoints, OneOf, EF Core In-Memory, Scalar UI |
| **[REslava.Result.Samples.Console](REslava.Result.Samples.Console/)** | .NET 10 Console | Core library examples: Result<T>, Maybe<T>, OneOf, LINQ, Validation |

---

## 🚀 Quick Start

### Web API Demo (FastMinimalAPI)

```bash
cd samples/FastMinimalAPI.REslava.Result.Demo
dotnet run
# Open http://localhost:5000/scalar/v1 for Scalar API docs
```

**Features demonstrated:**
- `[AutoGenerateEndpoints]` SmartEndpoints with auto HTTP method/route inference
- `Result<T>.ToIResult()` and `OneOf<...>.ToIResult()` extensions
- Authorization support (`RequiresAuth`, `Roles`, `AllowAnonymous`)
- OpenAPI metadata auto-generation
- EF Core In-Memory database

### Console Samples

```bash
cd samples/REslava.Result.Samples.Console
dotnet run
```

**Features demonstrated:**
- `Result<T>` creation, chaining, and error handling
- `Maybe<T>` safe null handling
- `OneOf<T1,T2,T3>` discriminated unions
- LINQ query syntax (`from ... in ... select`)
- Functional composition (`Compose`, `Bind`, `Map`)
- Validation framework

---

## 📚 Related Documentation

- **[Main README](../README.md)** — Full project overview and API reference
- **[CHANGELOG](../CHANGELOG.md)** — Version history
- **[Custom Generator Guide](../docs/how-to-create-custom-generator.md)** — Build your own source generators
