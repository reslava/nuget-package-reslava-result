# Development Tests

## 📋 Purpose

This directory contains **development tests** that use **project references** to the local source generator code. These tests are used for active development and debugging of the source generator.

## 🧪 Test Projects

### **MinimalApi.Net10.REslava.Result.v1.9.0.Test**
- **Type**: Development Test
- **Reference**: Project references to local generator
- **Purpose**: Active development and debugging
- **Usage**: Run with `dotnet run` or `dotnet build`

## 🚀 Usage

### **Running Tests**
```bash
cd MinimalApi.Net10.REslava.Result.v1.9.0.Test
dotnet run --urls http://localhost:5001
```

### **Building Tests**
```bash
cd MinimalApi.Net10.REslava.Result.v1.9.0.Test
dotnet build
```

## 🔄 Development Workflow

1. **Make changes** to source generator code
2. **Run tests** to validate changes
3. **Debug issues** with project references
4. **Iterate quickly** without package dependencies

## 📝 Notes

- **Project References**: Uses local `../../REslava.Result.SourceGenerators.csproj`
- **Fast Iteration**: No need to create packages for testing
- **Debugging**: Full source code available for debugging
- **Hot Reload**: Changes reflect immediately in tests

## 🎯 When to Use

- **Active development** of source generator
- **Debugging** generator issues
- **Testing new features** before packaging
- **Performance profiling** of generator

## 🚫 Limitations

- **Not for production testing** (uses project references)
- **Local dependencies** (requires local source code)
- **Development only** (not suitable for CI/CD)

---

*For production testing, see the `../Production` directory.*
