# Result Factories API Reference

This document contains all factory methods for creating Result instances.

## Result.Generic

Factory methods for generic Result<TValue> class.

### Success Creation

| Method | Signature | Description |
|--------|-----------|-------------|
| **Ok** | `Result<TValue> Ok(TValue value)` | Creates a successful result with a value |
| **Ok** | `Result<TValue> Ok(TValue value, string message)` | Creates a successful result with value and success message |
| **Ok** | `Result<TValue> Ok(TValue value, ISuccess success)` | Creates a successful result with value and success reason |
| **Ok** | `Result<TValue> Ok(TValue value, IEnumerable<string> messages)` | Creates a successful result with value and multiple success messages |
| **Ok** | `Result<TValue> Ok(TValue value, ImmutableList<ISuccess> successes)` | Creates a successful result with value and multiple success reasons |

### Failure Creation

| Method | Signature | Description |
|--------|-----------|-------------|
| **Fail** | `Result<TValue> Fail(string message)` | Creates a failed result with an error message |
| **Fail** | `Result<TValue> Fail(IError error)` | Creates a failed result with an error reason |
| **Fail** | `Result<TValue> Fail(IEnumerable<string> messages)` | Creates a failed result with multiple error messages |
| **Fail** | `Result<TValue> Fail(IEnumerable<IError> errors)` | Creates a failed result with multiple error reasons |

### Conversion

| Method | Signature | Description |
|--------|-----------|-------------|
| **FromResult** | `Result<TValue> FromResult(Result result)` | Converts a non-generic failed Result to generic Result<TValue>. Cannot be used with successful results |

## Result.NonGeneric

Factory methods for non-generic Result class.

### Success Creation

| Method | Signature | Description |
|--------|-----------|-------------|
| **Ok** | `Result Ok()` | Creates a successful result with no value |
| **Ok** | `Result Ok(string message)` | Creates a successful result with a success message |
| **Ok** | `Result Ok(ISuccess success)` | Creates a successful result with a success reason |
| **Ok** | `Result Ok(IEnumerable<string> messages)` | Creates a successful result with multiple success messages |
| **Ok** | `Result Ok(ImmutableList<ISuccess> successes)` | Creates a successful result with multiple success reasons |

### Failure Creation

| Method | Signature | Description |
|--------|-----------|-------------|
| **Fail** | `Result Fail(string message)` | Creates a failed result with an error message |
| **Fail** | `Result Fail(IError error)` | Creates a failed result with an error reason |
| **Fail** | `Result Fail(IEnumerable<string> messages)` | Creates a failed result with multiple error messages |
| **Fail** | `Result Fail(IEnumerable<IError> errors)` | Creates a failed result with multiple error reasons |

### Fluent Methods

| Method | Signature | Description |
|--------|-----------|-------------|
| **WithSuccess** | `Result WithSuccess(string message)` | Adds a success reason with message |
| **WithSuccess** | `Result WithSuccess(ISuccess success)` | Adds a success reason |
| **WithSuccesses** | `Result WithSuccesses(IEnumerable<ISuccess> successes)` | Adds multiple success reasons |
| **WithReason** | `Result WithReason(IReason reason)` | Adds a reason |
| **WithReasons** | `Result WithReasons(ImmutableList<IReason> reasons)` | Adds multiple reasons |
| **WithError** | `Result WithError(string message)` | Adds an error reason with message |
| **WithError** | `Result WithError(IError error)` | Adds an error reason |
| **WithErrors** | `Result WithErrors(IEnumerable<IError> errors)` | Adds multiple error reasons |

## Result.Combine

Methods for combining multiple Results.

### Non-Generic Result

| Method | Signature | Description |
|--------|-----------|-------------|
| **Merge** | `Result Merge(IEnumerable<Result> results)` | Merges multiple results, preserving all reasons. Returns failed if ANY result is failed |
| **Merge** | `Result Merge(params Result[] results)` | Merges multiple results with params syntax |
| **Combine** | `Result Combine(IEnumerable<Result> results)` | Combines results - ALL must succeed for combined result to succeed |
| **Combine** | `Result Combine(params Result[] results)` | Combines results with params syntax |
| **CombineParallelAsync** | `Task<Result> CombineParallelAsync(IEnumerable<Task<Result>> resultTasks)` | Combines results from parallel async operations |

### Generic Result<TValue>

| Method | Signature | Description |
|--------|-----------|-------------|
| **Combine** | `Result<IEnumerable<TValue>> Combine(IEnumerable<Result<TValue>> results)` | Combines multiple Result<T> - ALL must succeed. Returns Result<IEnumerable<T>> with all values |
| **Combine** | `Result<IEnumerable<TValue>> Combine(params Result<TValue>[] results)` | Combines results with params syntax |
| **CombineParallelAsync** | `Task<Result<IEnumerable<TValue>>> CombineParallelAsync(IEnumerable<Task<Result<TValue>>> resultTasks)` | Combines results from parallel async operations |

## Result.Conditional

Conditional factory methods based on predicates.

### Non-Generic Result

#### OkIf Methods

| Method | Signature | Description |
|--------|-----------|-------------|
| **OkIf** | `Result OkIf(bool condition, string errorMessage)` | Returns Ok if condition is true, otherwise Fail with message |
| **OkIf** | `Result OkIf(bool condition, IError error)` | Returns Ok if condition is true, otherwise Fail with error |
| **OkIf** | `Result OkIf(bool condition, string errorMessage, string successMessage)` | Returns Ok with success message if condition is true, otherwise Fail |
| **OkIf** | `Result OkIf(Func<bool> predicate, string errorMessage)` | Evaluates condition lazily - useful for expensive checks |
| **OkIfAsync** | `Task<Result> OkIfAsync(Func<Task<bool>> predicate, string errorMessage)` | Async version - evaluates condition asynchronously |

#### FailIf Methods

| Method | Signature | Description |
|--------|-----------|-------------|
| **FailIf** | `Result FailIf(bool condition, string errorMessage)` | Returns Fail if condition is true, otherwise Ok |
| **FailIf** | `Result FailIf(bool condition, IError error)` | Returns Fail if condition is true, otherwise Ok |
| **FailIf** | `Result FailIf(bool condition, string errorMessage, string successMessage)` | Returns Fail if condition is true, otherwise Ok with success message |
| **FailIf** | `Result FailIf(Func<bool> predicate, string errorMessage)` | Evaluates condition lazily |
| **FailIfAsync** | `Task<Result> FailIfAsync(Func<Task<bool>> predicate, string errorMessage)` | Async version |

### Generic Result<TValue>

#### OkIf Methods

| Method | Signature | Description |
|--------|-----------|-------------|
| **OkIf** | `Result<TValue> OkIf(bool condition, TValue value, string errorMessage)` | Returns Ok with value if condition is true, otherwise Fail |
| **OkIf** | `Result<TValue> OkIf(bool condition, TValue value, IError error)` | Returns Ok with value if condition is true, otherwise Fail with error |
| **OkIf** | `Result<TValue> OkIf(bool condition, Func<TValue> valueFactory, string errorMessage)` | Evaluates value lazily - only creates value if condition is true |
| **OkIfAsync** | `Task<Result<TValue>> OkIfAsync(bool condition, Func<Task<TValue>> valueFactory, string errorMessage)` | Async version - creates value asynchronously if condition is true |
| **OkIf** | `Result<TValue> OkIf(Func<bool> predicate, Func<TValue> valueFactory, string errorMessage)` | Evaluates both condition and value lazily |
| **OkIfAsync** | `Task<Result<TValue>> OkIfAsync(Func<Task<bool>> predicate, Func<Task<TValue>> valueFactory, string errorMessage)` | Async version - evaluates both condition and value asynchronously |

#### FailIf Methods

| Method | Signature | Description |
|--------|-----------|-------------|
| **FailIf** | `Result<TValue> FailIf(bool condition, string errorMessage, TValue value)` | Returns Fail if condition is true, otherwise Ok with value |
| **FailIf** | `Result<TValue> FailIf(bool condition, IError error, TValue value)` | Returns Fail if condition is true, otherwise Ok with error |
| **FailIf** | `Result<TValue> FailIf(bool condition, string errorMessage, Func<TValue> valueFactory)` | Returns Fail if condition is true, otherwise Ok with lazy value |
| **FailIf** | `Result<TValue> FailIf(Func<bool> predicate, string errorMessage, Func<TValue> valueFactory)` | Evaluates both condition and value lazily |
| **FailIfAsync** | `Task<Result<TValue>> FailIfAsync(Func<Task<bool>> predicate, string errorMessage, Func<Task<TValue>> valueFactory)` | Async version - evaluates both condition and value asynchronously |

## Result.Try

Exception handling factory methods.

### Non-Generic Result

| Method | Signature | Description |
|--------|-----------|-------------|
| **Try** | `Result Try(Action operation, Func<Exception, IError>? errorHandler = null)` | Executes an operation and wraps result in a Result |
| **TryAsync** | `Task<Result> TryAsync(Func<Task> operation, Func<Exception, IError>? errorHandler = null)` | Asynchronously executes an operation and wraps result in a Result |

### Generic Result<TValue>

| Method | Signature | Description |
|--------|-----------|-------------|
| **Try** | `Result<TValue> Try(Func<TValue> operation, Func<Exception, IError>? errorHandler = null)` | Executes an operation and wraps result in a Result with return value |
| **TryAsync** | `Task<Result<TValue>> TryAsync(Func<Task<TValue>> operation, Func<Exception, IError>? errorHandler = null)` | Asynchronously executes an operation and wraps result in a Result with return value |
