# Result Methods API Reference

This document contains all public methods available in Result classes.

## Result.Generic

Core methods for generic Result<TValue> class.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| **Value** | `TValue` | Gets the value if the result is successful. Throws if failed |
| **IsSuccess** | `bool` | Gets whether the result is successful |
| **IsFailed** | `bool` | Gets whether the result is failed |
| **Reasons** | `ImmutableList<IReason>` | Gets all reasons (successes and errors) |
| **Errors** | `ImmutableList<IError>` | Gets only error reasons |
| **Successes** | `ImmutableList<ISuccess>` | Gets only success reasons |

### Constructors

| Constructor | Signature | Description |
|-------------|-----------|-------------|
| **Result** | `Result(TValue? value, ImmutableList<IReason> reasons)` | Initializes with value and multiple reasons |
| **Result** | `Result(TValue? value, IReason reason)` | Initializes with value and single reason |

### Safe Value Access

| Method | Signature | Description |
|--------|-----------|-------------|
| **GetValueOr** | `TValue GetValueOr(TValue defaultValue)` | Gets value if successful, otherwise returns specified default |
| **GetValueOr** | `TValue GetValueOr(Func<TValue> defaultValueFactory)` | Gets value if successful, otherwise computes default lazily |
| **GetValueOr** | `TValue GetValueOr(Func<ImmutableList<IError>, TValue> errorHandler)` | Gets value if successful, otherwise computes default from errors |
| **TryGetValue** | `bool TryGetValue(out TValue value)` | Tries to get the value using .NET try pattern |

### Fluent Methods

| Method | Signature | Description |
|--------|-----------|-------------|
| **WithReason** | `Result<TValue> WithReason(IReason reason)` | Adds a reason while preserving the value |
| **WithReasons** | `Result<TValue> WithReasons(ImmutableList<IReason> reasons)` | Adds multiple reasons while preserving the value |
| **WithSuccess** | `Result<TValue> WithSuccess(string message)` | Adds a success reason with message |
| **WithSuccess** | `Result<TValue> WithSuccess(ISuccess success)` | Adds a success reason |
| **WithSuccesses** | `Result<TValue> WithSuccesses(IEnumerable<ISuccess> successes)` | Adds multiple success reasons |
| **WithError** | `Result<TValue> WithError(string message)` | Adds an error reason with message |
| **WithError** | `Result<TValue> WithError(IError error)` | Adds an error reason |
| **WithErrors** | `Result<TValue> WithErrors(IEnumerable<IError> errors)` | Adds multiple error reasons |

### ToString

| Method | Signature | Description |
|--------|-----------|-------------|
| **ToString** | `string ToString()` | Returns string representation including the value |

## Result.Map

Transformation methods for Result<TValue>.

| Method | Signature | Description |
|--------|-----------|-------------|
| **Map** | `Result<TOut> Map<TOut>(Func<TValue, TOut> mapper)` | Transforms the value of a successful result using mapper function |
| **MapAsync** | `Task<Result<TOut>> MapAsync<TOut>(Func<TValue, Task<TOut>> mapper)` | Asynchronously transforms the value using async mapper function |

## Result.Tap

Side-effect methods that don't modify the result.

### Non-Generic Result

| Method | Signature | Description |
|--------|-----------|-------------|
| **Tap** | `Result Tap(Action action)` | Executes a side effect without modifying the result |
| **TapAsync** | `Task<Result> TapAsync(Func<Task> action)` | Executes an async side effect without modifying the result |

### Generic Result<TValue>

| Method | Signature | Description |
|--------|-----------|-------------|
| **Tap** | `Result<TValue> Tap(Action<TValue> action)` | Executes a side effect with the value without modifying the result |
| **TapAsync** | `Task<Result<TValue>> TapAsync(Func<TValue, Task> action)` | Executes an async side effect with the value without modifying the result |

## Result.Match

Pattern matching methods for Result types.

### Non-Generic Result

| Method | Signature | Description |
|--------|-----------|-------------|
| **Match** | `TOut Match<TOut>(Func<TOut> onSuccess, Func<ImmutableList<IError>, TOut> onFailure)` | Matches result to one of two functions based on success or failure |
| **Match** | `void Match(Action onSuccess, Action<ImmutableList<IError>> onFailure)` | Executes one of two actions based on success or failure |
| **MatchAsync** | `Task<TOut> MatchAsync<TOut>(Func<Task<TOut>> onSuccess, Func<ImmutableList<IError>, Task<TOut>> onFailure)` | Asynchronously matches result to one of two async functions |
| **MatchAsync** | `Task MatchAsync(Func<Task> onSuccess, Func<ImmutableList<IError>, Task> onFailure)` | Asynchronously executes one of two async actions |

### Generic Result<TValue>

| Method | Signature | Description |
|--------|-----------|-------------|
| **Match** | `TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<ImmutableList<IError>, TOut> onFailure)` | Matches result with value to one of two functions |
| **Match** | `void Match(Action<TValue> onSuccess, Action<ImmutableList<IError>> onFailure)` | Executes one of two actions with value based on success or failure |
| **MatchAsync** | `Task<TOut> MatchAsync<TOut>(Func<TValue, Task<TOut>> onSuccess, Func<ImmutableList<IError>, Task<TOut>> onFailure)` | Asynchronously matches result with value to one of two async functions |
| **MatchAsync** | `Task MatchAsync(Func<TValue, Task> onSuccess, Func<ImmutableList<IError>, Task> onFailure)` | Asynchronously executes one of two async actions with value |

## Result.Bind

Chaining methods for sequential operations on Result<TValue>.

| Method | Signature | Description |
|--------|-----------|-------------|
| **Bind** | `Result<TOut> Bind<TOut>(Func<TValue, Result<TOut>> binder)` | Chains another operation that returns a Result (FlatMap/SelectMany) |
| **BindAsync** | `Task<Result<TOut>> BindAsync<TOut>(Func<TValue, Task<Result<TOut>>> binder)` | Asynchronously chains another operation that returns a Result |

## Result.Conversions

Type conversion methods between Result variants.

### Non-Generic Result

| Method | Signature | Description |
|--------|-----------|-------------|
| **ToResult** | `Result<TValue> ToResult<TValue>(TValue value)` | Converts to Result<TValue> with provided value |
| **ToResult** | `Result<TValue> ToResult<TValue>(Func<TValue> valueFactory)` | Converts to Result<TValue> using value factory |

### Generic Result<TValue>

| Method | Signature | Description |
|--------|-----------|-------------|
| **ToResult** | `Result ToResult()` | Converts to non-generic Result, discarding the value |

#### Implicit Conversions

| From | To | Description |
|------|----|-------------|
| `TValue` | `Result<TValue>` | Implicitly converts a value to a successful Result |
| `Error` | `Result<TValue>` | Implicitly converts an Error to a failed Result |
| `ExceptionError` | `Result<TValue>` | Implicitly converts an ExceptionError to a failed Result |
| `Error[]` | `Result<TValue>` | Implicitly converts an array of Errors to a failed Result |
| `List<Error>` | `Result<TValue>` | Implicitly converts a List of Errors to a failed Result |
| `ExceptionError[]` | `Result<TValue>` | Implicitly converts an array of ExceptionErrors to a failed Result |
| `List<ExceptionError>` | `Result<TValue>` | Implicitly converts a List of ExceptionErrors to a failed Result |
