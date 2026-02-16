# REslava.Result v1.17.0 — JSON Serialization Support

> **Result\<T>, OneOf, and Maybe\<T> now serialize and deserialize with System.Text.Json out of the box.**

---

## What's Changed

### JSON Serialization (System.Text.Json)

REslava.Result types now have full JSON serialization support via custom `JsonConverter` implementations. Zero new dependencies — uses the built-in `System.Text.Json`.

**Registration:**

```csharp
using REslava.Result.Serialization;

var options = new JsonSerializerOptions();
options.AddREslavaResultConverters();
```

**Result\<T> output:**
```json
{ "isSuccess": true, "value": { "name": "Alice" }, "errors": [], "successes": [] }
```

**OneOf\<T1,T2> output:**
```json
{ "index": 0, "value": "hello" }
```

**Maybe\<T> output:**
```json
{ "hasValue": true, "value": 42 }
```

### Converters included:

- `ResultJsonConverterFactory` — handles `Result<T>` for any `T`
- `OneOfJsonConverterFactory` — handles `OneOf<T1,T2>`, `OneOf<T1,T2,T3>`, `OneOf<T1,T2,T3,T4>`
- `MaybeJsonConverterFactory` — handles `Maybe<T>`
- Error/Success reasons serialized with `type`, `message`, and `tags` metadata

### Design notes:

- All error types (`Error`, `ExceptionError`, `ConversionError`, custom errors) serialize with their type name for diagnostics
- On deserialization, all errors reconstruct as `Error` with message + tags preserved
- Tag values deserialize as `JsonElement` for type-safe extraction
- Property names are hardcoded camelCase (`isSuccess`, `value`, `errors`, `hasValue`, `index`) regardless of `JsonSerializerOptions.PropertyNamingPolicy`

---

## Package Updates

| Package | Version | Description |
|---------|---------|-------------|
| `REslava.Result` | v1.17.0 — [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.17.0) | Core library |
| `REslava.Result.SourceGenerators` | v1.17.0 — [View on NuGet](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.17.0) | ASP.NET source generators |
| `REslava.Result.Analyzers` | v1.17.0 — [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.17.0) | Roslyn safety analyzers |

---

## Testing

- **2,148 total tests** across all packages and TFMs (48 new serialization tests)
- All tests green on net8.0, net9.0, and net10.0

---

## Breaking Changes

None. This is a purely additive release.

---

**MIT License** | [Full Changelog](../../CHANGELOG.md)
