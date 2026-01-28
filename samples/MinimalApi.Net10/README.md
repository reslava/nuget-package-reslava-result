# MinimalAPI .NET 10 Samples

This directory contains sample projects demonstrating different approaches to building Minimal APIs with .NET 10.

## 📁 Project Structure

```
samples/MinimalApi.Net10/
├── MinimalApi.Net10.Reference/           # Pure .NET 10 implementation (baseline)
├── MinimalApi.Net10.REslava.Result.v1.7.0/  # Using REslava.Result v1.7.0 with source generators
└── MinimalApi.Net10.REslava.Result.v1.8.0/  # Using REslava.Result v1.8.0 with new features
```

## 🎯 Purpose

These samples serve as **educational references** to compare different development approaches:

### **Pure .NET 10 (Reference)**
- ✅ Built-in validation features
- ✅ Manual error handling
- ✅ Standard Minimal API patterns
- ❌ More boilerplate code
- ❌ Repetitive validation logic

### **REslava.Result v1.7.0**
- ✅ Source generator magic
- ✅ Automatic Result<T> conversion
- ✅ Zero boilerplate error handling
- ✅ RFC 7807 compliance
- ✅ 70-90% code reduction

### **REslava.Result v1.8.0** (Coming Soon)
- ✅ All v1.7.0 benefits
- ✅ New advanced features
- ✅ Enhanced developer experience
- ✅ Latest best practices

## 🚀 Quick Comparison

| Feature | Pure .NET 10 | REslava.Result v1.7.0 |
|---------|---------------|----------------------|
| **Lines per endpoint** | ~30 | ~5 |
| **Validation** | Manual ModelState checks | Automatic |
| **Error handling** | Manual Results.* | Built-in HTTP mapping |
| **Type safety** | Runtime | Compile-time |
| **Performance** | Runtime overhead | Zero runtime overhead |

## 📚 How to Use

### **Run Individual Samples**
```bash
cd samples/MinimalApi.Net10/MinimalApi.Net10.Reference/MinimalApi.Net10.Reference
dotnet run

# Access Swagger at: https://localhost:xxxx/swagger
```

### **Compare Side-by-Side**
1. **Open both projects** in separate IDE windows
2. **Compare endpoint implementations** (Endpoints/ folders)
3. **Run both applications** to test behavior
4. **Review code reduction** in REslava.Result version

## 🎓 Learning Path

1. **Start with Reference** - Understand pure .NET 10 approach
2. **Try v1.7.0** - See source generator benefits
3. **Explore v1.8.0** - Learn latest features and improvements

## 📖 Featured Scenarios

All samples include the same business scenarios for direct comparison:

- **Product Management** - Full CRUD operations
- **Order Processing** - Complex business logic
- **Validation** - Simple and advanced scenarios
- **Error Handling** - Consistent HTTP responses

---

**These samples demonstrate the power of source generators and the REslava.Result library for modern .NET development.**
