# Production Tests

## 📋 Purpose

This directory contains **production tests** that use **package references** to the published source generator package. These tests validate the package works correctly in real-world scenarios.

## 🧪 Test Projects

### **CleanPackageTest**
- **Type**: Production Test
- **Reference**: Package references to published generator
- **Purpose**: Validate package functionality
- **Usage**: Test with NuGet package distribution

## 🚀 Usage

### **Running Tests**
```bash
cd CleanPackageTest/CleanTest.Api
dotnet run --urls http://localhost:6001
```

### **Building Tests**
```bash
cd CleanPackageTest/CleanTest.Api
dotnet build
```

## 🔄 Production Validation Workflow

1. **Install package** from NuGet (or local NuGet)
2. **Test functionality** with package references
3. **Validate generated code** works correctly
4. **Ensure compatibility** across .NET versions

## 📝 Notes

- **Package References**: Uses NuGet package `REslava.Result.SourceGenerators`
- **Real-world simulation** - Mimics actual usage scenarios
- **Package validation** - Tests package distribution
- **CI/CD ready** - Suitable for automated testing

## 🎯 When to Use

- **Package validation** before release
- **Production testing** with real packages
- **CI/CD pipelines** for automated testing
- **Compatibility testing** across environments

## ✅ Validation Checklist

- [ ] Package installs successfully
- [ ] Generated code appears correctly
- [ ] All HTTP method extensions work
- [ ] No duplicate generation errors
- [ ] Performance is acceptable
- [ ] Compatible with target .NET versions

## 🚫 Limitations

- **Requires package** - Cannot test without published package
- **Slower iteration** - Need to create packages for changes
- **No source debugging** - Package references only

---

*For development testing, see the `../Development` directory.*
