# 📚 REslava.Result.SourceGenerators Samples

**🎯 Complete showcase of v1.10.0 capabilities including the revolutionary OneOf integration!**

## 🚀 Purpose

Clean example applications demonstrating v1.10.0 SOLID architecture with **breakthrough OneOf integration**.

## � Sample Projects

### **🌐 Web API Samples**
| Project | Framework | Features | What You'll Learn |
|---------|------------|----------|-------------------|
| **OneOfTest.Api** | .NET 10 Minimal API | 🆕 OneOf2ToIResult, OneOf3ToIResult, Smart Auto-Detection | **v1.10.0 breakthrough features** |
| **MinimalApi.Net10.Reference** | .NET 10 Minimal API | Pure .NET patterns (baseline) | Traditional approach comparison |
| **MinimalApi.Net10.REslava.Result.v1.7.3** | .NET 10 Minimal API | ResultToIResult generator | v1.7.3 capabilities |

### **📚 Learning Categories**
| Category | Projects | Focus |
|----------|-----------|-------|
| **🚀 Quick Start** | OneOfTest.Api | See v1.10.0 magic immediately |
| **📊 Comparison** | All ASP.NET samples | Before/after comparison |
| **🧪 Testing** | OneOfTest.Api | Comprehensive test coverage |
| **🏗️ Architecture** | All samples | SOLID design patterns |

---

## 🎯 Recommended Learning Path

### **🚀 Path 1: Experience the Magic (30 minutes)**
```bash
# 1. See v1.10.0 breakthrough features
cd samples/OneOfTest.Api
dotnet run

# 2. Test the OneOf integration
curl http://localhost:5007/api/users/oneof/1     # Two-type OneOf
curl -X POST http://localhost:5007/api/users/oneof \
  -H "Content-Type: application/json" \
  -d '{"name":"John","email":"john@example.com"}'  # Three-type OneOf

# 3. Run automated tests
.\Test-Endpoints.ps1
```

### **📚 Path 2: Understand the Architecture (45 minutes)**
```bash
# 1. Compare approaches
cd samples/ASP.NET/MinimalApi.Net10.Reference
# Review traditional approach

cd ../MinimalApi.Net10.REslava.Result.v1.7.3  
# Review v1.7.3 improvements

cd ../OneOfTest.Api
# Review v1.10.0 breakthrough
```

### **🧪 Path 3: Deep Dive (60 minutes)**
1. **Read**: [Main README](../README.md) - Complete overview
2. **Study**: [Advanced Patterns](../README.md#-advanced-patterns) - Maybe, OneOf, Validation
3. **Explore**: [Custom Generator Guide](../docs/how-to-create-custom-generator.md) - Extensibility
4. **Review**: [Testing & Quality](../README.md#-testing--quality-assurance) - 1902+ tests

---

## 🌟 v1.10.0 Breakthrough Features

### **🔥 OneOf Integration Revolution**

#### **Two-Type OneOf Support**
```csharp
using OneOf;

public OneOf<NotFoundError, User> GetUser(int id) { /* logic */ }

// Auto-converts to HTTP response!
app.MapGet("/users/oneof/{id}", async (int id) => 
    await userService.GetUserOneOfAsync(id));
// → 200 OK with User OR 404 Not Found
```

#### **Three-Type OneOf Support**
```csharp
public OneOf<ValidationError, NotFoundError, User> CreateUser(CreateUserRequest request) { /* logic */ }

// Auto-converts to HTTP response!
app.MapPost("/users/oneof", async (CreateUserRequest request) => 
    await userService.CreateUserWithOneOfAsync(request));
// → 400 Bad Request, 404 Not Found, OR 200 OK
```

#### **Smart Auto-Detection**
```csharp
// Your code - no changes needed
public Result<User> GetUser(int id) { /* ... */ }           // → ResultToIResult
public OneOf<Error, User> GetExternalUser(int id) { /* ... */ } // → OneOf2ToIResult  
public OneOf<ValidationError, NotFoundError, User> CreateUser() { /* ... */ } // → OneOf3ToIResult
```

### **� Advanced Patterns Showcase**

#### **Maybe<T> - Safe Null Handling**
```csharp
Maybe<User> user = GetUserFromCache(id);
string email = user
    .Select(u => u.Email)
    .Filter(e => e.Contains("@"))
    .ValueOrDefault("no-reply@example.com");
```

#### **Validation Framework**
```csharp
var validator = Validator.Create<User>()
    .Rule(u => u.Email, email => email.Contains("@"), "Invalid email")
    .Rule(u => u.Name, name => !string.IsNullOrWhiteSpace(name), "Name required");

var result = await validator.ValidateAsync(user);
```

#### **Functional Composition**
```csharp
var createUser = Compose(
    ValidateRequest,
    MapToUser,
    SaveUser,
    SendNotification
);
```

---

## 🧪 Testing & Quality Assurance

### **📊 Test Coverage**
- **1902+ Tests Passing** - Comprehensive coverage
- **95%+ Code Coverage** - Production ready
- **Zero Flaky Tests** - Reliable CI/CD

### **🚀 Automated Testing**
```bash
# Run comprehensive test suite
cd samples/OneOfTest.Api
.\Test-Endpoints.ps1

# Test specific scenarios
.\Test-Endpoints.ps1 -BaseUrl "http://localhost:8080"
.\Test-Endpoints.ps1 -Verbose
```

### **📋 Manual Testing Commands**
```bash
# Result<T> endpoints
curl http://localhost:5007/api/users/1          # Success
curl http://localhost:5007/api/users/999        # Not found

# OneOf<T1,T2> endpoints  
curl http://localhost:5007/api/users/oneof/1     # Success
curl http://localhost:5007/api/users/oneof/999   # Not found

# OneOf<T1,T2,T3> endpoints
curl -X POST http://localhost:5007/api/users/oneof \
  -H "Content-Type: application/json" \
  -d '{"name":"John","email":"john@example.com"}'  # Success

curl -X POST http://localhost:5007/api/users/oneof \
  -H "Content-Type: application/json" \
  -d '{"name":"","email":""}'                      # Validation error
```

---

## 📊 Feature Matrix

| Feature | Pure .NET 10 | REslava.Result v1.7.3 | REslava.Result v1.10.0 🆕 |
|---------|--------------|----------------------|---------------------------|
| **Result<T> Auto-Conversion** | ❌ Manual | ✅ Smart mapping | ✅ Smart mapping |
| **External OneOf Support** | ❌ Manual | ❌ Not supported | ✅ **Auto-conversion** |
| **Three-Type OneOf Support** | ❌ Manual | ❌ Not supported | ✅ **Auto-conversion** |
| **Smart Auto-Detection** | ❌ N/A | ❌ Manual setup | ✅ **Zero configuration** |
| **Maybe<T> Support** | ❌ Manual | ✅ Built-in | ✅ Built-in |
| **Validation Framework** | ✅ Basic | ✅ Advanced | ✅ Advanced |
| **Boilerplate Reduction** | ❌ 0% | ✅ 70-90% | ✅ 70-90% |
| **Setup Complexity** | ❌ High | ✅ Medium | ✅ **Zero effort** |
| **Namespace Conflicts** | ❌ N/A | ✅ Clean | ✅ **Perfect isolation** |

---

## 🎯 Quick Start Commands

### **🚀 Get Running in 30 Seconds**
```bash
# Clone and run the v1.10.0 showcase
git clone https://github.com/reslava/REslava.Result.git
cd REslava.Result/samples/OneOfTest.Api
dotnet run

# Test immediately
curl http://localhost:5007/api/users/oneof/1
```

### **📚 Explore All Samples**
```bash
# Browse all sample projects
cd samples
ls

# Run each sample
cd OneOfTest.Api && dotnet run
cd ../MinimalApi.Net10.REslava.Result.v1.7.3 && dotnet run
cd ../MinimalApi.Net10.Reference && dotnet run
```

### **🧪 Run Full Test Suite**
```bash
# Run all tests across the project
cd ../../tests/REslava.Result.SourceGenerators.Tests
dotnet test --verbosity normal

# Should see: Test summary: total: 1902, failed: 0, succeeded: 1902
```

---

## 📚 Documentation Links

### **🎯 Choose Your Adventure**

| Goal | 📖 Start Here | 🎯 What You'll Learn |
|------|---------------|---------------------|
| **See OneOf Magic** | [OneOfTest.Api](OneOfTest.Api/) | 🆕 v1.10.0 breakthrough features |
| **Compare Approaches** | [ASP.NET Samples](ASP.NET/README.md) | Detailed feature comparison |
| **Learn Patterns** | [Main README](../README.md#-advanced-patterns) | Maybe, OneOf, Validation |
| **Build Generators** | [Custom Generator Guide](../docs/how-to-create-custom-generator.md) | Extensibility platform |
| **Understand Architecture** | [Complete Architecture](../README.md#-complete-architecture) | SOLID design principles |
| **Verify Quality** | [Testing & Quality](../README.md#-testing--quality-assurance) | 1902+ tests, CI/CD |

### **🔗 Essential Resources**
- **[Main Project README](../README.md)** - Complete v1.10.0 overview
- **[Quick Start Guide](../QUICK-START.md)** - 30-second setup
- **[CHANGELOG](../CHANGELOG.md)** - Complete version history
- **[Custom Generator Guide](../docs/how-to-create-custom-generator.md)** - Extending the platform

---

## 🏆 Why These Samples Matter

### **🎯 Educational Value**
- **Before/After Comparison** - See the transformation clearly
- **Real-World Scenarios** - Practical, applicable examples
- **Best Practices** - Industry-standard patterns
- **Performance Focus** - Optimized implementations

### **🚀 Development Acceleration**
- **Copy-Paste Ready** - Working code you can adapt
- **Tested Patterns** - Proven solutions
- **Common Use Cases** - Frequently encountered scenarios
- **Extensible Designs** - Easy to modify and extend

### **📊 Quality Assurance**
- **1902+ Tests** - Comprehensive validation
- **CI/CD Pipeline** - Automated quality checks
- **Performance Benchmarks** - Speed and memory metrics
- **Documentation** - Complete explanations

---

## 🎉 Experience the Future!

**🚀 v1.10.0 represents a quantum leap in .NET development:**

- **Zero boilerplate** for both Result<T> and OneOf<T>
- **Zero configuration** with intelligent auto-detection  
- **Zero conflicts** through perfect coexistence
- **Maximum productivity** with 70-90% code reduction

**Start with `OneOfTest.Api` and experience the magic for yourself!** ✨

*These samples aren't just code - they're a glimpse into the future of .NET development.* 🚀
