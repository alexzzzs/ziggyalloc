
BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.6584)
Unknown processor
.NET SDK 9.0.304
  [Host]   : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2
  .NET 9.0 : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Runtime=.NET 9.0  Server=True  

 Method                | Job       | Toolchain              | Mean            | Error         | StdDev         | Median          | Ratio     | RatioSD  | Gen0     | Gen1     | Gen2     | Allocated | Alloc Ratio |
---------------------- |---------- |----------------------- |----------------:|--------------:|---------------:|----------------:|----------:|---------:|---------:|---------:|---------:|----------:|------------:|
 ManagedArray_Small    | .NET 9.0  | Default                |        79.26 ns |      1.604 ns |       1.576 ns |        79.05 ns |      1.00 |     0.00 |   0.0113 |        - |        - |     424 B |        1.00 |
 UnmanagedArray_Small  | .NET 9.0  | Default                |       138.87 ns |      0.943 ns |       0.836 ns |       139.05 ns |      1.75 |     0.04 |   0.0017 |        - |        - |      64 B |        0.15 |
 ArrayPool_Small       | .NET 9.0  | Default                |        42.22 ns |      0.381 ns |       0.338 ns |        42.15 ns |      0.53 |     0.01 |        - |        - |        - |         - |        0.00 |
 ManagedArray_Medium   | .NET 9.0  | Default                |     6,778.47 ns |    134.753 ns |     209.794 ns |     6,825.33 ns |     84.75 |     2.88 |   1.0529 |        - |        - |   40024 B |       94.40 |
 UnmanagedArray_Medium | .NET 9.0  | Default                |     5,024.58 ns |     47.550 ns |      42.152 ns |     5,020.08 ns |     63.43 |     1.48 |        - |        - |        - |      64 B |        0.15 |
 ArrayPool_Medium      | .NET 9.0  | Default                |     3,610.75 ns |     16.482 ns |      15.418 ns |     3,610.69 ns |     45.64 |     0.91 |        - |        - |        - |         - |        0.00 |
 ManagedArray_Large    | .NET 9.0  | Default                | 1,062,002.53 ns | 13,499.125 ns |  11,966.623 ns | 1,063,744.63 ns | 13,407.11 |   321.78 | 998.0469 | 998.0469 | 998.0469 | 4000360 B |    9,434.81 |
 UnmanagedArray_Large  | .NET 9.0  | Default                | 2,320,186.03 ns | 68,665.464 ns | 202,461.669 ns | 2,249,531.25 ns | 27,055.34 | 2,497.15 |        - |        - |        - |      65 B |        0.15 |
 ArrayPool_Large       | .NET 9.0  | Default                |   535,192.77 ns | 14,988.563 ns |  44,194.115 ns |   552,816.78 ns |  6,197.97 |   415.29 |        - |        - |        - |         - |        0.00 |
                       |           |                        |                 |               |                |                 |           |          |          |          |          |           |             |
 ManagedArray_Small    | InProcess | InProcessEmitToolchain |        79.02 ns |      1.543 ns |       1.895 ns |        78.77 ns |      1.00 |     0.00 |   0.0337 |        - |        - |     424 B |       1.000 |
 UnmanagedArray_Small  | InProcess | InProcessEmitToolchain |       202.66 ns |      1.284 ns |       1.138 ns |       203.09 ns |      2.55 |     0.07 |   0.0050 |        - |        - |      64 B |       0.151 |
 ArrayPool_Small       | InProcess | InProcessEmitToolchain |        67.68 ns |      0.356 ns |       0.333 ns |        67.71 ns |      0.85 |     0.02 |        - |        - |        - |         - |       0.000 |
 ManagedArray_Medium   | InProcess | InProcessEmitToolchain |     6,574.42 ns |    126.764 ns |     130.177 ns |     6,594.24 ns |     82.86 |     2.55 |   3.1738 |        - |        - |   40024 B |      94.396 |
 UnmanagedArray_Medium | InProcess | InProcessEmitToolchain |    12,207.51 ns |     63.199 ns |      49.342 ns |    12,223.52 ns |    153.53 |     4.04 |        - |        - |        - |      64 B |       0.151 |
 ArrayPool_Medium      | InProcess | InProcessEmitToolchain |     6,156.00 ns |     15.888 ns |      14.862 ns |     6,159.11 ns |     77.58 |     1.89 |        - |        - |        - |         - |       0.000 |
 ManagedArray_Large    | InProcess | InProcessEmitToolchain | 1,581,672.64 ns | 10,393.806 ns |   9,722.373 ns | 1,585,827.15 ns | 19,932.34 |   521.46 | 998.0469 | 998.0469 | 998.0469 | 4004613 B |   9,444.842 |
 UnmanagedArray_Large  | InProcess | InProcessEmitToolchain | 1,747,018.08 ns |  6,097.465 ns |   5,405.244 ns | 1,746,262.11 ns | 21,999.97 |   556.74 |        - |        - |        - |      75 B |       0.177 |
 ArrayPool_Large       | InProcess | InProcessEmitToolchain |   399,648.03 ns |  1,278.233 ns |   1,195.660 ns |   399,855.18 ns |  5,036.21 |   121.13 |        - |        - |        - |       3 B |       0.007 |
