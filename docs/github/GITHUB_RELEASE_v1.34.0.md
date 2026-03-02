# REslava.Result v1.34.0

One breaking rename and a round of documentation improvements.

---

## ⚠️ Breaking Change — `IResultResponse<T>` → `IResultBase<T>`

The `IResultResponse<T>` interface has been renamed to `IResultBase<T>`.

**Why:** `IResultResponse` carried an unintended HTTP connotation — it is the base contract for all `Result` types, not an HTTP response. `IResultBase` is the semantically correct name.

**Migration:** This only affects consumers who reference `IResultResponse<T>` directly in their code (e.g. as a generic constraint or parameter type). A global find-and-replace is sufficient:

```
IResultResponse<T>  →  IResultBase<T>
```

Most users interact with `Result<T>` directly and are **not affected** by this change.

---

## 📚 Documentation Improvements

- **Http extensions** — full usage guide for `GetResult<T>`, `PostResult`, `PutResult`, `DeleteResult`, `HttpResultOptions`
- **Generator setup** — step-by-step guide for adding `REslava.Result.SourceGenerators` to a project
- **`ConversionError`** — documented in the error type reference table

---

## 📦 NuGet

| Package | Link |
|---------|------|
| REslava.Result | [View on NuGet](https://www.nuget.org/packages/REslava.Result/1.34.0) |
| REslava.Result.SourceGenerators | [View on NuGet](https://www.nuget.org/packages/REslava.Result.SourceGenerators/1.34.0) |
| REslava.Result.Analyzers | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Analyzers/1.34.0) |
| REslava.Result.FluentValidation | [View on NuGet](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.34.0) |
| REslava.Result.Http | [View on NuGet](https://www.nuget.org/packages/REslava.Result.Http/1.34.0) |

---

## Stats

- 3,756 tests passing across net8.0, net9.0, net10.0 (1,157×3) + generator (131) + analyzer (68) + FluentValidation bridge (26) + Http (20×3)
- 123 features across 12 categories
