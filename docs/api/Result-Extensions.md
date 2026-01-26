# Result Extensions API Reference

This document contains all extension methods for Result types.

## Result.Bind.Extensions

Extension methods for binding operations on Task<Result<T>>.

| Method | Signature | Description |
|--------|-----------|-------------|
| **BindAsync** | `Task<Result<U>> BindAsync<T, U>(this Task<Result<T>> resultTask, Func<T, Task<Result<U>>> binder)` | Chains async operations on Task<Result<T>> |

## Result.Map.Extensions

Extension methods for working with Task<Result<T>>.

| Method | Signature | Description |
|--------|-----------|-------------|
| **MapAsync** | `Task<Result<U>> MapAsync<T, U>(this Task<Result<T>> resultTask, Func<T, U> mapper)` | Asynchronously maps value inside a Task<Result<T>> to a new value |
| **MapAsync** | `Task<Result<U>> MapAsync<T, U>(this Task<Result<T>> resultTask, Func<T, Task<U>> mapper)` | Asynchronously maps value inside a Task<Result<T>> using async mapper |
| **WithSuccessAsync** | `Task<Result<T>> WithSuccessAsync<T>(this Task<Result<T>> resultTask, string message)` | Asynchronously adds a success reason to a Task<Result<T>> |
| **WithSuccessAsync** | `Task<Result<T>> WithSuccessAsync<T>(this Task<Result<T>> resultTask, ISuccess success)` | Asynchronously adds a success reason to a Task<Result<T>> |

## Result.Tap.Extensions

Extension methods for tap operations on Result types.

### Non-Generic Result Extensions

| Method | Signature | Description |
|--------|-----------|-------------|
| **TapAsync** | `Task<Result> TapAsync(this Task<Result> resultTask, Action action)` | Awaits result then executes a side effect without modifying it |
| **TapAsync** | `Task TapAsync(this Task<Result> resultTask, Func<Task> action)` | Awaits result then executes an async side effect without modifying it |
| **TapOnFailure** | `Result TapOnFailure(this Result result, Action<IError> action)` | Executes action on first error if result failed |
| **TapOnFailureAsync** | `Task<Result> TapOnFailureAsync(this Result result, Func<IError, Task> action)` | Executes async action on first error if result failed |

### Generic Result<T> Extensions

| Method | Signature | Description |
|--------|-----------|-------------|
| **TapAsync** | `Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Action<T> action)` | Awaits result then executes a side effect with value without modifying it |
| **TapAsync** | `Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Func<T, Task> action)` | Awaits result then executes an async side effect with value without modifying it |
| **TapOnFailure** | `Result<T> TapOnFailure<T>(this Result<T> result, Action<IError> action)` | Executes action on first error if result failed |
| **TapOnFailureAsync** | `Task<Result<T>> TapOnFailureAsync<T>(this Result<T> result, Func<IError, Task> action)` | Executes async action on first error if result failed |
| **TapOnFailureAsync** | `Task<Result<T>> TapOnFailureAsync<T>(this Task<Result<T>> resultTask, Action<IError> action)` | Awaits result then executes action on first error if result failed |
| **TapOnFailure** | `Result<T> TapOnFailure<T>(this Result<T> result, Action<ImmutableList<IError>> action)` | Executes action on all errors if result failed |
| **TapBoth** | `Result<T> TapBoth<T>(this Result<T> result, Action<Result<T>> action)` | Executes action on the entire result regardless of success/failure |

## Result.LINQ.Extensions

LINQ query syntax support for Result types. Enables functional composition using C# query expressions.

### SelectMany - Two Parameter (Bind equivalent)

| Method | Signature | Description |
|--------|-----------|-------------|
| **SelectMany** | `Result<T> SelectMany<S, T>(this Result<S> source, Func<S, Result<T>> selector)` | Projects each element of a Result into a new Result and flattens resulting sequences. Equivalent to Bind |
| **SelectManyAsync** | `Task<Result<T>> SelectManyAsync<S, T>(this Result<S> source, Func<S, Task<Result<T>>> selector)` | Asynchronously projects each element of a Result into a new Result |

### SelectMany - Three Parameter (LINQ query syntax support)

| Method | Signature | Description |
|--------|-----------|-------------|
| **SelectMany** | `Result<T> SelectMany<S, I, T>(this Result<S> source, Func<S, Result<I>> selector, Func<S, I, T> resultSelector)` | Enables LINQ query syntax with multiple from clauses |
| **SelectManyAsync** | `Task<Result<T>> SelectManyAsync<S, I, T>(this Result<S> source, Func<S, Task<Result<I>>> selector, Func<S, I, Task<T>> resultSelector)` | Async version with both selector and resultSelector async |
| **SelectManyAsync** | `Task<Result<T>> SelectManyAsync<S, I, T>(this Result<S> source, Func<S, Result<I>> selector, Func<S, I, Task<T>> resultSelector)` | Mixed async/sync: sync selector, async resultSelector |
| **SelectManyAsync** | `Task<Result<T>> SelectManyAsync<S, I, T>(this Result<S> source, Func<S, Task<Result<I>>> selector, Func<S, I, T> resultSelector)` | Mixed async/sync: async selector, sync resultSelector |

### Select (Map equivalent)

| Method | Signature | Description |
|--------|-----------|-------------|
| **Select** | `Result<T> Select<S, T>(this Result<S> source, Func<S, T> selector)` | Projects each element of a Result into a new form. Equivalent to Map |
| **SelectAsync** | `Task<Result<T>> SelectAsync<S, T>(this Result<S> source, Func<S, Task<T>> selector)` | Asynchronously projects each element of a Result into a new form |

### Where (Filter)

| Method | Signature | Description |
|--------|-----------|-------------|
| **Where** | `Result<S> Where<S>(this Result<S> source, Func<S, bool> predicate)` | Filters a Result based on a predicate. If predicate returns false, converts success to failure |
| **Where** | `Result<S> Where<S>(this Result<S> source, Func<S, bool> predicate, string errorMessage)` | Filters a Result based on a predicate with custom error message |
| **WhereAsync** | `Task<Result<S>> WhereAsync<S>(this Result<S> source, Func<S, Task<bool>> predicate)` | Asynchronously filters a Result based on a predicate |
| **WhereAsync** | `Task<Result<S>> WhereAsync<S>(this Result<S> source, Func<S, Task<bool>> predicate, string errorMessage)` | Asynchronously filters a Result based on a predicate with custom error message |

### Task<Result<T>> Extensions for LINQ

| Method | Signature | Description |
|--------|-----------|-------------|
| **WhereAsync** | `Task<Result<S>> WhereAsync<S>(this Task<Result<S>> resultTask, Func<S, bool> predicate, string errorMessage)` | Awaits result then filters based on a predicate |
| **WhereAsync** | `Task<Result<S>> WhereAsync<S>(this Task<Result<S>> resultTask, Func<S, Task<bool>> predicate, string errorMessage)` | Awaits result then filters based on an async predicate |
| **SelectAsync** | `Task<Result<T>> SelectAsync<S, T>(this Task<Result<S>> resultTask, Func<S, T> selector)` | Awaits result then projects to a new value |
| **SelectAsync** | `Task<Result<T>> SelectAsync<S, T>(this Task<Result<S>> resultTask, Func<S, Task<T>> selector)` | Awaits result then projects to a new value asynchronously |
| **SelectManyAsync** | `Task<Result<T>> SelectManyAsync<S, T>(this Task<Result<S>> resultTask, Func<S, Result<T>> selector)` | Awaits result then projects each element into a new Result |
| **SelectManyAsync** | `Task<Result<T>> SelectManyAsync<S, T>(this Task<Result<S>> resultTask, Func<S, Task<Result<T>>> selector)` | Awaits result then asynchronously projects each element into a new Result |
| **SelectManyAsync** | `Task<Result<T>> SelectManyAsync<S, I, T>(this Task<Result<S>> resultTask, Func<S, Result<I>> selector, Func<S, I, T> resultSelector)` | Awaits result then projects with query syntax support (sync selector, sync resultSelector) |
| **SelectManyAsync** | `Task<Result<T>> SelectManyAsync<S, I, T>(this Task<Result<S>> resultTask, Func<S, Task<Result<I>>> selector, Func<S, I, T> resultSelector)` | Awaits result then projects with query syntax support (async selector, sync resultSelector) |
| **SelectManyAsync** | `Task<Result<T>> SelectManyAsync<S, I, T>(this Task<Result<S>> resultTask, Func<S, Result<I>> selector, Func<S, I, Task<T>> resultSelector)` | Awaits result then projects with query syntax support (sync selector, async resultSelector) |
| **SelectManyAsync** | `Task<Result<T>> SelectManyAsync<S, I, T>(this Task<Result<S>> resultTask, Func<S, Task<Result<I>>> selector, Func<S, I, Task<T>> resultSelector)` | Awaits result then projects with query syntax support (both selector and resultSelector are async) |

## Result.Validation.Extensions

Extension methods for validation operations on Result types.

### Ensure - Sync with Error

| Method | Signature | Description |
|--------|-----------|-------------|
| **Ensure** | `Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error error)` | Ensures that a condition is met, otherwise returns a failed result |

### Ensure - Sync with String

| Method | Signature | Description |
|--------|-----------|-------------|
| **Ensure** | `Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, string errorMessage)` | Ensures that a condition is met, otherwise returns a failed result |

### Ensure - Multiple Validations

| Method | Signature | Description |
|--------|-----------|-------------|
| **Ensure** | `Result<T> Ensure<T>(this Result<T> result, params (Func<T, bool> predicate, Error error)[] validations)` | Ensures that multiple conditions are met, otherwise returns a failed result |

### EnsureAsync - Task<Result<T>> + Sync Predicate + Error

| Method | Signature | Description |
|--------|-----------|-------------|
| **EnsureAsync** | `Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Error error)` | Awaits result then ensures that a condition is met |
| **EnsureAsync** | `Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, string errorMessage)` | Awaits result then ensures that a condition is met |

### EnsureAsync - Result<T> + Async Predicate + Error

| Method | Signature | Description |
|--------|-----------|-------------|
| **EnsureAsync** | `Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<bool>> predicate, Error error)` | Ensures that an async condition is met, otherwise returns a failed result |
| **EnsureAsync** | `Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<bool>> predicate, string errorMessage)` | Ensures that an async condition is met, otherwise returns a failed result |

### EnsureAsync - Task<Result<T>> + Async Predicate + Error

| Method | Signature | Description |
|--------|-----------|-------------|
| **EnsureAsync** | `Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, Error error)` | Awaits result then ensures that an async condition is met |
| **EnsureAsync** | `Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, string errorMessage)` | Awaits result then ensures that an async condition is met |

### Common Validations

| Method | Signature | Description |
|--------|-----------|-------------|
| **EnsureNotNull** | `Result<T> EnsureNotNull<T>(this Result<T> result, string errorMessage) where T : class` | Ensures value is not null |
| **EnsureNotNullAsync** | `Task<Result<T>> EnsureNotNullAsync<T>(this Task<Result<T>> resultTask, string errorMessage) where T : class` | Awaits result then ensures that value is not null |
