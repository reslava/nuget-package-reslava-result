# REslava.Result v1.47.5

## 🔧 NuGet README Images Fixed

NuGet.org blocks external image URLs (including `raw.githubusercontent.com`) in package READMEs. Diagram SVGs were showing as broken image icons with only the alt-text visible.

**Fix:** showcase SVGs are now packed **locally inside the `.nupkg`** and referenced via relative `images/` paths — NuGet.org renders them directly from the package without any external request.

### Packages updated

- **`REslava.Result`** — `Pipelines_AdminCheckout.svg`, `OrderService_PlaceOrderCross_LayerView.svg`
- **`REslava.Result.Flow`** — `OrderService_PlaceOrderCross.svg`, `OrderService_PlaceOrderCross_LayerView.svg`, `OrderService_PlaceOrderCross_ErrorPropagation.svg`, `MatchDemo_ConfirmOrder.svg`, `Legend.svg`, `FulfillmentService_FulfillOrder.svg`

No code changes, no API changes, no test count change (4,676).

---

### NuGet Packages

| Package | Version |
|---------|---------|
| [REslava.Result](https://www.nuget.org/packages/REslava.Result/1.47.5) | 1.47.5 |
| [REslava.Result.Flow](https://www.nuget.org/packages/REslava.Result.Flow/1.47.5) | 1.47.5 |
| [REslava.Result.AspNetCore](https://www.nuget.org/packages/REslava.Result.AspNetCore/1.47.5) | 1.47.5 |
| [REslava.Result.Http](https://www.nuget.org/packages/REslava.Result.Http/1.47.5) | 1.47.5 |
| [REslava.Result.Analyzers](https://www.nuget.org/packages/REslava.Result.Analyzers/1.47.5) | 1.47.5 |
| [REslava.Result.OpenTelemetry](https://www.nuget.org/packages/REslava.Result.OpenTelemetry/1.47.5) | 1.47.5 |
| [REslava.ResultFlow](https://www.nuget.org/packages/REslava.ResultFlow/1.47.5) | 1.47.5 |
| [REslava.Result.FluentValidation](https://www.nuget.org/packages/REslava.Result.FluentValidation/1.47.5) | 1.47.5 |
