# Maybe\<T> API Reference

## Overview

`Maybe<T>` represents an optional value that may or may not be present. It's a type-safe alternative to null references that enables functional programming patterns.

## Constructor Methods

### FromValue(T value)
Creates a `Maybe<T>` containing the specified value.

```csharp
Maybe<User> user = Maybe<User>.FromValue(new User("Alice"));
```

**Parameters:**
- `value` (T): The value to wrap

**Returns:** `Maybe<T>` containing the value

---

### None
Creates an empty `Maybe<T>` instance.

```csharp
Maybe<User> empty = Maybe<User>.None;
```

**Returns:** `Maybe<T>` representing no value

---

## Functional Methods

### Select<TResult>(Func<T, TResult> selector)
Transforms the value if present, similar to LINQ's Select.

```csharp
Maybe<int> numbers = Maybe<int>.FromValue(42);
Maybe<string> text = numbers.Select(x => x.ToString()); // "42"
```

**Parameters:**
- `selector` (Func<T, TResult>): Function to transform the value

**Returns:** `Maybe<TResult>` containing transformed value or None

**Throws:** `ArgumentNullException` if selector is null

---

### Filter(Func<T, bool> predicate)
Filters the value based on a predicate, returning None if predicate fails.

```csharp
Maybe<int> number = Maybe<int>.FromValue(42);
Maybe<int> filtered = number.Filter(x => x > 40); // Contains 42
Maybe<int> failed = number.Filter(x => x > 50);  // None
```

**Parameters:**
- `predicate` (Func<T, bool>): Function to test the value

**Returns:** `Maybe<T>` containing value if predicate passes, None otherwise

**Throws:** `ArgumentNullException` if predicate is null

---

### Bind<TResult>(Func<T, Maybe<TResult>> binder)
Chains Maybe operations, flattening nested Maybe types.

```csharp
Maybe<string> userId = Maybe<string>.FromValue("user123");
Maybe<User> user = userId.Bind(id => GetUserById(id));
```

**Parameters:**
- `binder` (Func<T, Maybe<TResult>>): Function that returns Maybe<TResult>

**Returns:** `Maybe<TResult>` from binder function or None

**Throws:** `ArgumentNullException` if binder is null

---

### Match<TResult>(Func<T, TResult> some, Func<TResult> none)
Pattern matching for Maybe values.

```csharp
string message = user.Match(
    some: u => $"User: {u.Name}",
    none: () => "No user found"
);
```

**Parameters:**
- `some` (Func<T, TResult>): Function to execute when value is present
- `none` (Func<TResult>): Function to execute when value is absent

**Returns:** Result of executing appropriate function

**Throws:** `ArgumentNullException` if any parameter is null

---

### Switch(Action<T> some, Action none)
Executes side effects based on Maybe state.

```csharp
user.Switch(
    some: u => Console.WriteLine($"Processing {u.Name}"),
    none: () => Console.WriteLine("No user to process")
);
```

**Parameters:**
- `some` (Action<T>): Action to execute when value is present
- `none` (Action): Action to execute when value is absent

**Throws:** `ArgumentNullException` if any parameter is null

---

## Utility Methods

### ValueOrDefault()
Gets the value if present, default(T) otherwise.

```csharp
Maybe<string> name = Maybe<string>.FromValue("Alice");
string result = name.ValueOrDefault(); // "Alice"

Maybe<string> empty = Maybe<string>.None;
string result2 = empty.ValueOrDefault(); // null
```

**Returns:** The contained value or default(T)

---

### ValueOrDefault(T defaultValue)
Gets the value if present, specified default otherwise.

```csharp
Maybe<string> name = Maybe<string>.None;
string result = name.ValueOrDefault("Default"); // "Default"
```

**Parameters:**
- `defaultValue` (T): Default value to return if None

**Returns:** The contained value or specified default

---

## Properties

### IsSome
Gets whether the Maybe contains a value.

```csharp
if (user.IsSome)
{
    Console.WriteLine("User is present");
}
```

**Returns:** `true` if value is present, `false` otherwise

---

### IsNone
Gets whether the Maybe is empty.

```csharp
if (user.IsNone)
{
    Console.WriteLine("No user found");
}
```

**Returns:** `true` if empty, `false` otherwise

---

### Value
Gets the contained value. Throws if None.

```csharp
if (user.IsSome)
{
    User u = user.Value; // Safe to access
}
```

**Returns:** The contained value

**Throws:** `InvalidOperationException` if Maybe is None

---

## Conversion Methods

### ToMaybe()
Extension method to convert nullable types to Maybe.

```csharp
string? nullableName = GetName();
Maybe<string> maybeName = nullableName.ToMaybe();
```

**Returns:** `Maybe<T>` containing value if not null, None otherwise

---

### ToNullable()
Converts Maybe to nullable type.

```csharp
Maybe<string> maybeName = Maybe<string>.FromValue("Alice");
string? nullableName = maybeName.ToNullable(); // "Alice"
```

**Returns:** Nullable T containing value if Some, null otherwise

---

## Equality and Comparison

### Equals(object? obj)
Compares Maybe values for equality.

```csharp
Maybe<int> a = Maybe<int>.FromValue(42);
Maybe<int> b = Maybe<int>.FromValue(42);
bool equal = a.Equals(b); // true
```

**Parameters:**
- `obj` (object?): Object to compare with

**Returns:** `true` if equal, `false` otherwise

---

### GetHashCode()
Gets hash code for the Maybe value.

```csharp
int hash = user.GetHashCode();
```

**Returns:** Hash code based on contained value or None state

---

### ToString()
String representation of the Maybe.

```csharp
Maybe<int> some = Maybe<int>.FromValue(42);
Console.WriteLine(some.ToString()); // "Some(42)"

Maybe<int> none = Maybe<int>.None;
Console.WriteLine(none.ToString()); // "None"
```

**Returns:** String representation showing Some(value) or None

---

## LINQ Integration

Maybe<T> supports LINQ query syntax when using the `SelectMany` method:

```csharp
Maybe<int> a = Maybe<int>.FromValue(2);
Maybe<int> b = Maybe<int>.FromValue(3);

Maybe<int> result = from x in a
                   from y in b
                   select x + y; // Some(5)
```

---

## Examples

### Database Query Pattern
```csharp
public Maybe<User> GetUserById(int id)
{
    var user = _database.FindUser(id);
    return user != null ? Maybe<User>.FromValue(user) : Maybe<User>.None;
}

public Maybe<UserDto> GetUserDto(int id)
{
    return GetUserById(id)
        .Select(user => new UserDto(user.Name, user.Email))
        .Filter(dto => !string.IsNullOrEmpty(dto.Email));
}
```

### Configuration Pattern
```csharp
public Maybe<string> GetConfigValue(string key)
{
    return _configuration.TryGetValue(key, out var value)
        ? Maybe<string>.FromValue(value)
        : Maybe<string>.None;
}

public int GetTimeout()
{
    return GetConfigValue("timeout")
        .Select(int.Parse)
        .Filter(timeout => timeout > 0)
        .ValueOrDefault(30); // Default 30 seconds
}
```

### Validation Chain
```csharp
public Maybe<ValidatedUser> ValidateUser(User user)
{
    return Maybe<User>.FromValue(user)
        .Filter(u => !string.IsNullOrEmpty(u.Name))
        .Filter(u => u.Age >= 18)
        .Select(u => new ValidatedUser(u));
}
```

---

## Performance Notes

- **Memory Allocation**: Zero allocation for None instances
- **Boxing**: Value types are boxed when stored in Some
- **Inlining**: Most methods are small and can be inlined by JIT
- **Pattern Matching**: Match is optimized for common cases

---

## See Also

- [Advanced Patterns Guide](Advanced-Patterns.md) - Usage patterns and best practices
- [OneOf API](Oneof.md) - Discriminated union patterns
- [Integration Extensions](Integration-Extensions.md) - Result integration methods
