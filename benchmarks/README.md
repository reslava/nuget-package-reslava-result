# Performance Benchmarks

This benchmark suite compares REslava.Result with traditional exception handling.

## Running the Benchmarks

```bash
cd benchmarks
dotnet run -c Release
```

## Benchmark Results

**Latest Results (.NET 8.0, AMD Ryzen 5 4600G):**

| Method | Mean | Error | StdDev | Ratio | Gen0 | Allocated | Alloc Ratio |
|--------|-----|-------|--------|-------|------|-----------|-------------|
| ExceptionChaining | 34.35 us | 0.673 us | 1.231 us | 1.00 | 30.58 | 62.5 KB | 1.00 |
| **REslavaResultChaining** | **1,140.76 us** | **21.691 us** | **22.275 us** | **33.10** | **1,113.28** | **2,273.44 KB** | **36.38** |

## Analysis

### Performance Characteristics

**Exception Handling (Baseline):**
- ✅ **Faster execution**: 34.35 μs average
- ✅ **Lower memory allocation**: 62.5 KB per operation
- ✅ **Fewer GC collections**: 30.58 Gen0 collections per 1000 operations

**REslava.Result:**
- ⚠️ **Slower execution**: 1,140.76 μs average (33x slower)
- ⚠️ **Higher memory allocation**: 2,273.44 KB per operation (36x more)
- ⚠️ **More GC pressure**: 1,113.28 Gen0 collections per 1000 operations

### Why This Makes Sense

**Exception handling is optimized for the "happy path":**
- No exceptions thrown = minimal overhead
- .NET optimizes for non-exceptional flow
- Direct method calls with minimal allocation

**Result pattern has inherent overhead:**
- Object allocation for each Result instance
- Method call overhead for chaining
- Immutable design creates new objects each step

## The Real Value Proposition

**Performance isn't the primary advantage of Result patterns.** The benefits are:

### 1. **Type Safety & Predictability**
```csharp
// Exceptions: Hidden failure paths
public User GetUser(int id) {
    var user = _db.Find(id);
    if (user == null) throw new NotFoundException(); // Hidden in signature
    return user;
}

// Result: Explicit failure handling
public Result<User> GetUser(int id) {
    var user = _db.Find(id);
    return user == null ? Result<User>.Fail("Not found") : Result<User>.Ok(user);
}
```

### 2. **Composability**
```csharp
// Railway-style chaining with explicit error flow
var result = ValidateUser(user)
    .Bind(u => CheckPermissions(u))
    .Bind(u => SaveToDatabase(u))
    .Map(u => Transform(u));
```

### 3. **Better Testing**
```csharp
// Easy to test both success and failure paths
Assert.IsTrue(result.IsSuccess);
Assert.AreEqual(expectedUser, result.Value);

// vs complex exception testing
Assert.Throws<NotFoundException>(() => GetUser(-1));
```

## When to Use Each Approach

### Use Exceptions When:
- Performance is critical
- Failure is truly exceptional (rare)
- You need stack traces for debugging
- Simple control flow is sufficient

### Use REslava.Result When:
- Failure is expected and part of business logic
- You need explicit error handling
- Composability is important
- Type safety is a priority
- You want to avoid hidden control flow

## Conclusion

The benchmark shows that **Result patterns trade performance for safety and expressiveness**. This is a valid trade-off in many business applications where:

- Code maintainability is more important than micro-optimizations
- Error handling is part of the business logic
- Team collaboration benefits from explicit error flows

**REslava.Result's CRTP approach provides the best type safety among Result patterns**, even if it's slower than exceptions for simple cases.
