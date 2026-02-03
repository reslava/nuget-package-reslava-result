# Incremental Development Guide

## 🎯 Purpose
This guide ensures we develop features incrementally, test thoroughly, and avoid assumptions.

## 📋 Development Process

### Phase 1: Planning
- [ ] Define clear requirements
- [ ] Identify all edge cases
- [ ] Plan testing strategy
- [ ] Document success criteria

### Phase 2: Implementation
- [ ] Start with minimal working version
- [ ] Add one feature at a time
- [ ] Test each increment thoroughly
- [ ] No assumptions - verify everything

### Phase 3: Testing
- [ ] Unit tests for each component
- [ ] Integration tests for workflows
- [ ] End-to-end testing
- [ ] Performance testing

### Phase 4: Documentation
- [ ] Update API documentation
- [ ] Create usage examples
- [ ] Update templates
- [ ] Review and validate

## 🚀 Feature Development Checklist

### Before Starting
- [ ] Requirements are clear and documented
- [ ] Testing strategy is defined
- [ ] Success criteria are measurable
- [ ] Dependencies are identified

### During Development
- [ ] Implement one small feature at a time
- [ ] Test each feature immediately
- [ ] Verify no regressions
- [ ] Document decisions and trade-offs

### Before Completion
- [ ] All tests pass
- [ ] Edge cases handled
- [ ] Performance acceptable
- [ ] Documentation complete

## 🧪 Testing Standards

### Unit Testing
```csharp
[Test]
public void GetUser_WhenUserExists_ReturnsSuccess()
{
    // Arrange
    var service = new UserService();
    
    // Act
    var result = service.GetUser(1);
    
    // Assert
    Assert.IsTrue(result.IsT2); // User case
    Assert.AreEqual("Sample User", result.AsT2.Name);
}

[Test]
public void GetUser_WhenUserNotFound_ReturnsError()
{
    // Arrange
    var service = new UserService();
    
    // Act
    var result = service.GetUser(999);
    
    // Assert
    Assert.IsTrue(result.IsT1); // Error case
    Assert.IsInstanceOfType<UserNotFoundError>(result.AsT1);
}
```

### Integration Testing
```csharp
[Test]
public async Task GetUserEndpoint_WhenCalled_ReturnsCorrectResponse()
{
    // Arrange
    var app = CreateTestApp();
    
    // Act
    var response = await app.GetAsync("/api/users/1");
    
    // Assert
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    var user = await response.Content.ReadFromJsonAsync<User>();
    Assert.IsNotNull(user);
}
```

### End-to-End Testing
```powershell
# Test all endpoints
./scripts/Test-AllSamples.ps1 -Verbose

# Test specific sample
./scripts/Test-Sample.ps1 -Sample "OneOfTest.Api"
```

## 📊 Incremental Development Example

### T1,T2,T3 Implementation

#### Step 1: Minimal Test Case
```csharp
// Start with simplest possible T1,T2,T3
OneOf<ValidationError, UserNotFoundError, User> result = 
    new ValidationError("Test", "Test message");

return result.ToIResult(); // Test this first
```

#### Step 2: Verify Compilation
```bash
dotnet build
# Must compile without errors
```

#### Step 3: Test Runtime
```powershell
# Start server and test endpoint
curl -X GET http://localhost:5000/test-t1t2t3
# Must return 422 with validation error
```

#### Step 4: Add Complexity
```csharp
// Add actual business logic
OneOf<ValidationError, UserNotFoundError, User> result = request switch
{
    null => new ValidationError("Request", "Request body required"),
    { Name: null or "" } => new ValidationError("Name", "Name required"),
    var req when !_users.Any(u => u.Id == id) => new UserNotFoundError(id),
    var req => _users.First(u => u.Id == id)
};
```

#### Step 5: Test All Scenarios
```powershell
# Test all three type scenarios
curl -X PUT http://localhost:5000/api/users/1 -d "{}" # ValidationError
curl -X PUT http://localhost:5000/api/users/999 -d "{}" # UserNotFoundError  
curl -X PUT http://localhost:5000/api/users/1 -d '{"name":"Test"}' # Success
```

## 🔍 Quality Gates

### Code Quality
- [ ] Code follows project conventions
- [ ] No TODO comments left in production code
- [ ] Proper error handling
- [ ] Adequate logging

### Testing Quality
- [ ] All tests pass
- [ ] Test coverage > 80%
- [ ] Integration tests cover main scenarios
- [ ] Performance tests meet requirements

### Documentation Quality
- [ ] API documentation updated
- [ ] Code comments are clear
- [ ] Examples are working
- [ ] Templates are updated

## 📝 Lessons Learned

### Common Pitfalls
1. **Assumptions** - Never assume generated code works without testing
2. **Incomplete Testing** - Test all code paths, not just happy paths
3. **Documentation Drift** - Keep docs in sync with code
4. **Performance Regressions** - Test performance impact of changes

### Best Practices
1. **Test-First Development** - Write tests before implementation
2. **Small Increments** - One small feature at a time
3. **Continuous Integration** - Automated testing on every change
4. **Peer Review** - Second pair of eyes on all changes

## 🎯 Success Metrics

### Technical Metrics
- Test coverage percentage
- Number of bugs found in production
- Performance benchmarks
- Code review pass rate

### Process Metrics
- Time from feature start to completion
- Number of regressions per release
- Customer satisfaction scores
- Developer productivity

## 🔄 Continuous Improvement

### Retrospective Questions
1. What went well in this increment?
2. What could be improved?
3. What did we learn?
4. What will we do differently next time?

### Process Updates
- Update development guide based on lessons learned
- Improve templates based on user feedback
- Enhance testing scripts based on gaps found
- Refine quality gates based on issues discovered
