# REslava.Result.Diagnostics

HTTP trace endpoint for [REslava.Result](https://www.nuget.org/packages/REslava.Result/) pipeline runtime observation.

Exposes `RingBufferObserver` data via `GET /reslava/traces` so the **REslava.Result Extensions** VSIX can display the Live panel (`📋 History · 🎯 Single · ⏭ Step · 🔁 Replay` modes).

## Installation

```
dotnet add package REslava.Result.Diagnostics
```

Requires `REslava.Result` (added automatically as a transitive dependency).

## Quick start

### ASP.NET Core

```csharp
using REslava.Result.Observers;
using REslava.Result.Diagnostics;

var buffer = new RingBufferObserver();
PipelineObserver.Register(buffer);

var app = builder.Build();
app.MapResultFlowTraces(buffer);   // GET /reslava/traces
app.Run();
```

### Console / Worker

```csharp
using REslava.Result.Observers;
using REslava.Result.Diagnostics;

var buffer = new RingBufferObserver();
PipelineObserver.Register(buffer);

using var host = PipelineTraceHost.Start(buffer, port: 5297);
// ... run your app ...
```

Then open the **REslava.Result Extensions** VSIX in VS Code and click `▶ Debug` on any `[ResultFlow]` method.

## Links

- [GitHub](https://github.com/reslava/nuget-package-reslava-result)
- [Documentation](https://reslava.github.io/nuget-package-reslava-result)
- [CHANGELOG](https://github.com/reslava/nuget-package-reslava-result/blob/main/CHANGELOG.md)
