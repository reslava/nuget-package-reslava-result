```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.7623)
AMD Ryzen 5 4600G with Radeon Graphics, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.102
  [Host]     : .NET 8.0.23 (8.0.2325.60607), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.23 (8.0.2325.60607), X64 RyuJIT AVX2


```
| Method                | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0      | Allocated  | Alloc Ratio |
|---------------------- |------------:|----------:|----------:|------:|--------:|----------:|-----------:|------------:|
| ExceptionChaining     |    34.35 μs |  0.673 μs |  1.231 μs |  1.00 |    0.00 |   30.5786 |    62.5 KB |        1.00 |
| REslavaResultChaining | 1,140.76 μs | 21.691 μs | 22.275 μs | 33.10 |    1.42 | 1113.2813 | 2273.44 KB |       36.38 |
