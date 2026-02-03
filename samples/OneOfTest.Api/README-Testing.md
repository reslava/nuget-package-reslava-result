# OneOfTest.Api Testing Guide

## 🧪 Automated Testing

### Quick Test
```powershell
# Run all tests
.\Test-Endpoints.ps1

# Run with verbose output
.\Test-Endpoints.ps1 -Verbose

# Test different server URL
.\Test-Endpoints.ps1 -BaseUrl "http://localhost:8080"
```

## 📋 Manual Testing Commands

### Basic Endpoints
```powershell
# Test success case
curl http://localhost:5007/api/users/1

# Test error case
curl http://localhost:5007/api/users/999

# Test health check
curl http://localhost:5007/health
```

### POST Requests
```powershell
# Create valid user
curl -X POST http://localhost:5007/api/users `
  -H "Content-Type: application/json" `
  -d '{"name":"Test User","email":"test@example.com"}'

# Create invalid user (empty name)
curl -X POST http://localhost:5007/api/users `
  -H "Content-Type: application/json" `
  -d '{"name":"","email":"test@example.com"}'

# Create invalid user (bad email)
curl -X POST http://localhost:5007/api/users `
  -H "Content-Type: application/json" `
  -d '{"name":"Test User","email":"invalid-email"}'
```

## 🔍 OneOf Variants Tested

### ✅ T1,T2 OneOf (Two Types)
- `OneOf<UserNotFoundError, User>`
- **T1**: `UserNotFoundError` → 404 Not Found
- **T2**: `User` → 200 OK

### ✅ Source Generator Features
- **Extension Methods**: `.ToIResult()` automatically generated
- **HTTP Mapping**: Intelligent status code mapping
- **JSON Serialization**: Proper JSON output for all types

### ✅ HTTP Status Codes Tested
- **200 OK**: Success responses
- **404 Not Found**: Error responses
- **400 Bad Request**: Validation errors

## 🎯 Expected Behavior

### Success Response (200 OK)
```json
{
  "id": 1,
  "name": "Test User",
  "email": "test@example.com",
  "createdAt": "2026-02-02T12:06:11.7852121Z",
  "updatedAt": null
}
```

### Error Response (404 Not Found)
```json
"Not Found: User with id '999' not found"
```

## 🚀 Running the Server

```bash
# Start the server
dotnet run --urls="http://localhost:5007"

# Or use a different port
dotnet run --urls="http://localhost:8080"
```

## 📊 Test Coverage

- ✅ **GET /api/users/{id}** - Success and error cases
- ✅ **POST /api/users** - Success and validation errors
- ✅ **GET /health** - Health check endpoint
- ✅ **OneOf to IResult conversion** - All variants
- ✅ **JSON serialization** - Proper object mapping
- ✅ **HTTP status mapping** - Correct status codes

## 🔧 Troubleshooting

### Server Not Running
```bash
# Check if server is running
curl http://localhost:5007/health

# If 404, server is running but endpoint doesn't exist
# If connection refused, server is not running
```

### Build Issues
```bash
# Clean build
dotnet clean
dotnet build

# Check for compilation errors
dotnet build --verbosity normal
```

### Generated Code Issues
```bash
# Check generated files
Get-ChildItem GeneratedFiles -Recurse -Filter "*.g.cs"

# Verify extension methods exist
Get-ChildItem GeneratedFiles -Recurse -Filter "*Extensions*.g.cs"
```

## 📝 Notes

- All tests assume the server is running on `http://localhost:5007`
- Use `-Verbose` flag to see full response data
- Tests automatically clean up after each request
- Script provides clear pass/fail indicators
- Failed tests show detailed error information
