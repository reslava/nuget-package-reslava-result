# 🧪 NuGet Validation Test

## 🎯 Purpose

This test application validates that the **published NuGet packages** work correctly in a real-world scenario. It uses only the official NuGet packages, not local references.

## 📦 Packages Tested

- **REslava.Result v1.9.0** - Core library with Result pattern and HTTP extensions
- **REslava.Result.SourceGenerators v1.9.4** - SOLID architecture source generator

## 🚀 Quick Start

### **1. Install Dependencies**
```bash
cd Samples/NuGetValidationTest/NuGetValidationTest.Api
dotnet restore
```

### **2. Run the Application**
```bash
dotnet run --urls http://localhost:7001
```

### **3. Test All Endpoints**
```bash
# Health check
curl http://localhost:7001/api/health

# Test basic REslava.Result functionality
curl http://localhost:7001/api/test/basic

# Test all HTTP method extensions
curl http://localhost:7001/api/test/get/1
curl -X POST http://localhost:7001/api/test/post -H "Content-Type: application/json" -d '{"name":"Test Product","price":99.99,"description":"Test product"}'
curl -X PUT http://localhost:7001/api/test/put/1 -H "Content-Type: application/json" -d '{"name":"Updated Product","price":149.99,"description":"Updated description"}'
curl -X DELETE http://localhost:7001/api/test/delete/1
curl -X PATCH http://localhost:7001/api/test/patch/1 -H "Content-Type: application/json" -d '{"name":"Patched Product","price":79.99}'

# Test error scenarios
curl http://localhost:7001/api/test/error/notfound
curl http://localhost:7001/api/test/error/validation
curl http://localhost:7001/api/test/error/unauthorized
```

## 🧪 Test Scenarios

### **✅ Success Cases**
- **Basic Result pattern** - Core library functionality
- **ToIResult()** - GET requests (200 OK)
- **ToPostResult()** - POST requests (201 Created)
- **ToPutResult()** - PUT requests (200 OK)
- **ToDeleteResult()** - DELETE requests (200 OK)
- **ToPatchResult()** - PATCH requests (200 OK)

### **❌ Error Cases**
- **Not Found** - 404 status code
- **Validation** - 400 status code
- **Unauthorized** - 401 status code
- **Forbidden** - 403 status code
- **Conflict** - 409 status code

## 🔍 Validation Checklist

### **✅ Expected Results**
- [ ] **Build succeeds** with zero errors
- [ ] **Generated code** appears in `obj/generated/`
- [ ] **All HTTP extensions work** correctly
- [ ] **Status codes are correct** for each method
- [ ] **Error handling works** properly
- [ ] **No duplicate generation** errors (CS0101, CS0579)

### **🔍 Generated Code Verification**
```bash
# Check generated files exist
ls obj/Debug/net10.0/generated/REslava.Result.SourceGenerators/

# Verify generated extensions
cat obj/Debug/net10.0/generated/REslava.Result.SourceGenerators/*.g.cs | grep "ToIResult\|ToPostResult\|ToPutResult\|ToDeleteResult\|ToPatchResult"
```

## 📊 Expected HTTP Responses

### **✅ Success Responses**
```json
// GET /api/test/get/1
{
  "id": 1,
  "name": "Test Product 1",
  "price": 99.99,
  "description": "Test product for GET validation",
  "createdAt": "2026-01-31T13:00:00Z"
}
```

### **❌ Error Responses**
```json
// GET /api/test/get/0
{
  "type": "https://httpstatuses.com/400",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid product ID"
}
```

## 🎯 Success Indicators

### **✅ Build Success**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:03.45
```

### **✅ Generated Code Present**
```
obj/Debug/net10.0/generated/REslava.Result.SourceGenerators/
├── ResultToIResultExtensions.g.cs
├── GenerateResultExtensionsAttribute.g.cs
└── MapToProblemDetailsAttribute.g.cs
```

### **✅ All Extensions Working**
- **ToIResult()** → 200 OK on success, 400/404/500 on errors
- **ToPostResult()** → 201 Created on success
- **ToPutResult()** → 200 OK on success
- **ToDeleteResult()** → 200 OK on success
- **ToPatchResult()** → 200 OK on success

## 🚨 Troubleshooting

### **Build Errors**
- **CS1061**: Extension method not found → Check source generator installation
- **CS0101/CS0579**: Duplicate generation → Should not occur with v1.9.4 SOLID architecture

### **Runtime Errors**
- **500 Internal Server Error**: Check generated code compilation
- **404 Not Found**: Verify endpoint registration in Program.cs

### **Package Issues**
- **Package not found**: Verify NuGet package versions (1.9.0 and 1.9.4)
- **Version conflicts**: Clear NuGet cache: `dotnet nuget locals all --clear`

## 🎉 Validation Success

If all tests pass, this confirms:
- ✅ **NuGet packages are correctly published**
- ✅ **SOLID architecture works perfectly**
- ✅ **No duplicate generation issues**
- ✅ **Users can install and use without issues**

---

**This test validates the complete success of the REslava.Result.SourceGenerators v1.9.4 SOLID architecture release!** 🎯
